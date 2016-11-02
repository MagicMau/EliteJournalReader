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
        
        /// <summary>
        ///     The default filter
        /// </summary>
        private const string DefaultFilter = @"Journal.*.log";
        
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
                Trace.WriteLine("Exception in setting path: " + ex.Message);
            }
        }

        /// <summary>
        ///     Determines whether the Path contains netLog files.
        /// </summary>
        /// <returns><c>true</c> if the Path contains netLog files; otherwise, <c>false</c>.</returns>
        public bool IsValidPath()
        {
            return IsValidPath(Path);
        }

        /// <summary>
        ///     Determines whether the specified path contains netLog files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified path contains netLog files; otherwise, <c>false</c>.</returns>
        public bool IsValidPath(string path)
        {
            var filesFound = false;
            try
            {
                filesFound = Directory.GetFiles(path, Filter).Any();
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            return filesFound;
        }

        private readonly Regex journalFileRegex = new Regex(@"^(?<path>.*)\\Journal\.(?<timestamp>\d+)\.(?<part>\d+)\.log$", RegexOptions.Compiled);

        /// <summary>
        /// This will look into the journal folder and check the latest journal.
        /// It will then fire events from all previous events in the current play session to facilitate
        /// rebuilding a status object before going "live".
        /// </summary>
        /// <returns></returns>
        private void ProcessPreviousJournals()
        {
            try
            {
                var journals = Directory.GetFiles(Path, "Journal.*").OrderByDescending(f => f);
                if (!journals.Any())
                    return; // there's nothing

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
                        Trace.WriteLine($"Journal: now reading previous entries from {LatestJournalFile}.");
                        ParseData(reader);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
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
        public void StartWatching()
        {
            if (EnableRaisingEvents)
            {
                // Already watching
                return;
            }

            if (!IsValidPath())
            {
                //throw new InvalidOperationException(
                //    string.Format("Directory {0} does not contain journal files?!", this.Path));
                return; // fail silently
            }

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel(); // should not happen, but let's be safe, okay?

            cancellationTokenSource = new CancellationTokenSource();

            // before we start watching, rerun all events up until now (including any previous parts of this game session)
            ProcessPreviousJournals();
            // because we might just have read an old log file, make sure we don't miss the new one when it arrives
            StartPollingForNewJournal();

            Created += async (sender, args) => await UpdateLatestJournalFile();
            Changed += JournalWatcher_Changed;

            EnableRaisingEvents = true;
            CheckForJournalUpdateAsync();
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
                        Trace.WriteLine($"Error while polling for new journal: {e.Message}.");
                    }
                }
            });
        }

        public void StopWatching()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            if (journalThread != null)
                journalThread.Join();
        }


        private void CheckForJournalUpdateAsync()
        {
            journalThread = new Thread(ido =>
            {
                // keep a current ID for this thread. If the ID changes, we are watching a different file, and this thread can exit.
                int id = (int)ido;
                var journalFile = System.IO.Path.Combine(Path, LatestJournalFile);
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
            });
            journalThread.Name = "Journal Watcher";
            journalThread.IsBackground = true;
            journalThread.Start(++journalThreadId);

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
                Trace.WriteLine($"Exception while parsing journal data: {e.Message}");
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
                    Trace.WriteLine($"Exception while updating position in journal file: {e.Message}");
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
            var journals = Directory.GetFiles(Path, "Journal.*");

            // keep waiting until there is a journal, or we're being cancelled.
            while (journals.Length == 0)
            {
                try
                {
                    await Task.Delay(UPDATE_INTERVAL_MILLISECONDS, cancellationTokenSource.Token);
                    journals = Directory.GetFiles(Path, "Journal.*");
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
            }

            // because the timestamp is in the filename, we can just sort by filename descending.
            var latestJournal = journals.OrderByDescending(f => f).First();

            bool isChanged = LatestJournalFile != latestJournal;
            if (isChanged)
            {
                lastOffset = 0;
                LatestJournalFile = latestJournal;
                isPollingForNewFile = false;
                Trace.WriteLine($"Journal: now reading from {LatestJournalFile}.");

                CheckForJournalUpdateAsync();
            }


            return latestJournal;
        }

        /// <summary>
        /// Parses a line of JSON from the journal and fire a .NET event handler.
        /// </summary>
        /// <param name="line"></param>
        private void Parse(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            try
            {
                var evt = JObject.Parse(line);
                var eventType = evt.Value<string>("event");
#if DEBUG
                Trace.WriteLine($"Journal - firing event {eventType} @ {evt["timestamp"]?.Value<string>()}\r\n\t{line}");
#endif
                FireEvent(eventType, evt);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Exception handling journal event:\r\n\t{line}\r\n\tException: {e.Message}");
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Find the event handler for the given type. If found, invoke it.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="evt"></param>
        private void FireEvent(string eventType, JObject evt)
        {
            JournalEvent handler;
            if (journalEventsByName.TryGetValue(eventType, out handler))
                handler.FireEvent(this, evt);
            else
                Trace.WriteLine("No event handler registered for journal event of type: " + eventType);
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
