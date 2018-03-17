using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EliteJournalReader
{

    /// <summary>
    /// File watcher and parser for the new journal feed to be introduced in Elite:Dangerous 2.2.
    /// It reads the file as it comes in and parses it on a line by line basis.
    /// All events are fired as .NET events to be consumed by other classes.
    /// </summary>
    public class JournalWatcher : FileSystemWatcher
    {
        public const int UPDATE_INTERVAL_MILLISECONDS = 500;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        /// <summary>
        ///     The default filter
        /// </summary>
        private const string DefaultFilter = @"Journal*.*.log";
        
        /// <summary>
        ///     The last offset used when reading the netLog file.
        /// </summary>
        private long lastOffset;

        /// <summary>
        ///     The latest log file
        /// </summary>
        public string LatestJournalFile { get; private set; }

        /// <summary>
        /// Monitor the journal in a separate thread
        /// </summary>
        private Thread journalThread = null;
        private volatile int journalThreadId = 0;

        /// <summary>
        /// Token to signal that we are no longer watching
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Because the journal is kept open, we might not get notified through the FileWatcher
        /// So, in cases where we expect a new file might come, poll the directory to see if it does.
        /// </summary>
        private bool isPollingForNewFile = false;

        /// <summary>
        /// Keep a map of event names to event objects
        /// </summary>
        private static Dictionary<string, JournalEvent> journalEventsByName = new Dictionary<string, JournalEvent>();

        /// <summary>
        /// Also map the event objects by their type
        /// </summary>
        private static Dictionary<Type, JournalEvent> journalEvents = new Dictionary<Type, JournalEvent>();

        public bool IsLive { get; protected set; }

        /// <summary>
        /// Use reflection to generate a list of event handlers. This allows for a dynamic list of handler classes, one for each type
        /// of event.
        /// </summary>
        static JournalWatcher()
        {
            var allHandlerTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(JournalEvent).IsAssignableFrom(type));
            
            var handlers = from type in allHandlerTypes
                           where !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface)
                           select (JournalEvent)Activator.CreateInstance(type);

            foreach(var handler in handlers)
            {
                journalEvents[handler.GetType()] = handler;
                foreach (var eventName in handler.EventNames)
                    journalEventsByName[eventName] = handler;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public JournalWatcher(string path)
        {
            Filter = DefaultFilter;
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            try
            {
                Path = System.IO.Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in setting path: " + ex.Message);
            }
        }

        protected JournalWatcher()
        {
            // to be used for unit tests when we're not actually checking file systems
        }

        private readonly Regex journalFileRegex = new Regex(@"^(?<path>.*)\\Journal(Beta)?\.(?<timestamp>\d+)\.(?<part>\d+)\.log$", RegexOptions.Compiled);

        /// <summary>
        /// This will look into the journal folder and check the latest journal.
        /// It will then fire events from all previous events in the current play session to facilitate
        /// rebuilding a status object before going "live".
        /// </summary>
        /// <returns></returns>
        private bool ProcessPreviousJournals()
        {
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f));
                if (!journals.Any())
                    return true; // there's nothing

                // return the list until we find one with a part number 01.
                int partNr = 1;
                var match = journalFileRegex.Match(journals.First());
                if (match.Success)
                    int.TryParse(match.Groups["part"].Value, out partNr);

                var previousFiles = journals.Take(partNr).Reverse();

                // now process each journal
                foreach (var filename in previousFiles)
                {
                    string journalFile = System.IO.Path.Combine(Path, filename);
                    using (StreamReader reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        lastOffset = 0;
                        LatestJournalFile = filename;
                        Trace.TraceInformation($"Journal: now reading previous entries from {LatestJournalFile}.");
                        ParseData(reader);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return false;
            }

            return true;
        }

        private DateTime GetFileCreationDate(string path)
        {
            try
            {
                return File.GetCreationTime(path);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///     Starts the watching.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Throws an exception if the <see cref="Path" /> does not contain netLogs
        ///     files.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The directory specified in <see cref="P:System.IO.FileSystemWatcher.Path" />
        ///     could not be found.
        /// </exception>
        public virtual async Task StartWatching()
        {
            if (EnableRaisingEvents)
            {
                // Already watching
                return;
            }

            if (!Directory.Exists(Path))
            {
                Trace.TraceError($"Cannot watch non-existing folder {Path}.");
                return;
            }

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel(false); // should not happen, but let's be safe, okay?

            cancellationTokenSource = new CancellationTokenSource();

            // before we start watching, rerun all events up until now (including any previous parts of this game session)
            await Task.Run(() =>
            {
                if (ProcessPreviousJournals())
                {
                    // finally send an event that we've gone live
                    IsLive = true;
                    FireEvent("MagicMau.IsLiveEvent", new JObject(new JProperty("timestamp", DateTime.Now)));
                }
            });

            // because we might just have read an old log file, make sure we don't miss the new one when it arrives
            StartPollingForNewJournal();

            Created += async (sender, args) => await UpdateLatestJournalFile();
            Changed += JournalWatcher_Changed;

            EnableRaisingEvents = true;
            if (!string.IsNullOrEmpty(LatestJournalFile))
                CheckForJournalUpdateAsync(LatestJournalFile);
        }

        private async void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // if we're not watching anything, let's see if there is a log available
            if (LatestJournalFile == null || e.Name != LatestJournalFile)
                await UpdateLatestJournalFile();
        }

        internal void StartPollingForNewJournal()
        {
            if (isPollingForNewFile || cancellationTokenSource.IsCancellationRequested)
                return; // we're already polling or no longer needed

            isPollingForNewFile = true;
            Task.Run(async () =>
            {
                while (isPollingForNewFile)
                {
                    try
                    {
                        await Task.Delay(5000, cancellationTokenSource.Token); // check every five seconds
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            isPollingForNewFile = false;
                            return;
                        }

                        await UpdateLatestJournalFile();
                    }
                    catch (TaskCanceledException)
                    {
                        isPollingForNewFile = false;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Error while polling for new journal: {e.Message}.");
                    }
                }
            });
        }

        public virtual void StopWatching()
        {
            try
            {
                EnableRaisingEvents = false;

                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();

                if (journalThread != null)
                    journalThread.Join();
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while stopping Journal watcher: {e.Message}");
                Trace.TraceInformation(e.StackTrace);
            }
        }


        private void CheckForJournalUpdateAsync(string filename)
        {
            journalThreadId++;
            if (journalThread != null && journalThread.IsAlive)
            {
                try
                {
                    if (!journalThread.Join(30000))
                    {
                        journalThread.Abort();
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Something went wrong shutting down the previous journal reader thread: {e.Message}");
                }
                finally
                {
                    journalThread = null;
                }
            }

            journalThread = new Thread(ido =>
            {
                // keep a current ID for this thread. If the ID changes, we are watching a different file, and this thread can exit.
                int id = (int)ido;
                var journalFile = System.IO.Path.Combine(Path, filename);
                try
                {
                    using (StreamReader reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (id == journalThreadId)
                        {
                            // check for updates every 0.5 seconds
                            // if we are no longer watching (this thread), stop.
                            if (!Pause() || id != journalThreadId)
                                return;

                            // if the file size has not changed, idle
                            if (reader.BaseStream.Length == lastOffset)
                                continue;

                            // we found new data, so this is definitely not a stale file
                            isPollingForNewFile = false;

                            // parse the data we just read
                            ParseData(reader);
                        }
                    }
                }
                catch
                {
                    // Something went wrong, let's check log files again
                    LatestJournalFile = null;
                }

                if (id == journalThreadId)
                {
                    // We're here, so something must've gone wrong
                    // Let's try again in a few seconds
                    Pause();
                    UpdateLatestJournalFile().Wait(cancellationTokenSource.Token);
                }
            })
            {
                Name = "Journal Watcher",
                IsBackground = true
            };
            journalThread.Start(journalThreadId);

        }

        private void ParseData(StreamReader reader)
        {
            try
            {
                // seek to the last max offset
                reader.BaseStream.Seek(lastOffset, SeekOrigin.Begin);

                // read new data
                var newData = reader.ReadToEnd();

                // split the new data into lines
                var lines = newData.Split('\r', '\n');

                // parse each line
                foreach (var line in lines)
                    Parse(line);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception while parsing journal data: {e.Message}");
            }
            finally
            {
                try
                {
                    // update the last max offset
                    lastOffset = reader.BaseStream.Position;
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Exception while updating position in journal file: {e.Message}");
                    // might be something wrong with the file - let's start polling for a new one
                    StartPollingForNewJournal();
                }
            }
        }

        private bool Pause()
        {
            try
            {
                Task.Delay(UPDATE_INTERVAL_MILLISECONDS, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Updates the <see cref="LatestJournalFile" /> property.
        /// </summary>
        private async Task<string> UpdateLatestJournalFile()
        {
            // filenames have format: Journal.160922194205.01.log
            var journals = Directory.GetFiles(Path, DefaultFilter);

            // keep waiting until there is a journal, or we're being cancelled.
            while (journals.Length == 0)
            {
                try
                {
                    await Task.Delay(UPDATE_INTERVAL_MILLISECONDS, cancellationTokenSource.Token);
                    journals = Directory.GetFiles(Path, DefaultFilter);
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
            }

            // because the timestamp is in the filename, we can just sort by filename descending.
            var latestJournal = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f)).FirstOrDefault();

            bool isChanged = latestJournal != null && LatestJournalFile != latestJournal;
            if (isChanged)
            {
                lastOffset = 0;
                LatestJournalFile = latestJournal;
                isPollingForNewFile = false;
                Trace.TraceInformation($"Journal: now reading from {LatestJournalFile}.");

                CheckForJournalUpdateAsync(latestJournal);
            }


            return latestJournal;
        }

        /// <summary>
        /// Parses a line of JSON from the journal and fire a .NET event handler.
        /// </summary>
        /// <param name="line"></param>
        protected void Parse(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            try
            {
                var evt = JObject.Parse(line);
                var eventType = evt.Value<string>("event");
#if DEBUG
                if (IsLive)
                    Trace.TraceInformation($"Journal - firing event {eventType} @ {evt["timestamp"]?.Value<string>()}\r\n\t{line}");
#endif
                var journalEventArgs = FireEvent(eventType, evt);
                if (journalEventArgs != null)
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(journalEventArgs, eventType));
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception handling journal event:\r\n\t{line}\r\n\t{e.GetType().FullName}: {e.Message}");
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Find the event handler for the given type. If found, invoke it.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="evt"></param>
        private JournalEventArgs FireEvent(string eventType, JObject evt)
        {
            if (journalEventsByName.TryGetValue(eventType, out JournalEvent handler))
                return handler.FireEvent(this, evt);
            else
                Trace.TraceWarning("No event handler registered for journal event of type: " + eventType);

            return null;
        }

        public TJournalEvent GetEvent<TJournalEvent>() where TJournalEvent : JournalEvent
        {
            var type = typeof(TJournalEvent);
            if (journalEvents.ContainsKey(type))
                return journalEvents[type] as TJournalEvent;
            return null;
        }
    }
    
}
