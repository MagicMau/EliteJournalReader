﻿using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Token that triggers the end of the current journal watcher
        /// </summary>
        private CancellationTokenSource journalCancellationTokenSource;

        /// <summary>
        /// Because the journal is kept open, we might not get notified through the FileWatcher
        /// So, in cases where we expect a new file might come, poll the directory to see if it does.
        /// </summary>
        private bool isPollingForNewFile = false;

        /// <summary>
        /// Keep a map of event names to event objects
        /// </summary>
        private static readonly Dictionary<string, JournalEvent> journalEventsByName = new Dictionary<string, JournalEvent>();

        /// <summary>
        /// Also map the event objects by their type
        /// </summary>
        private static readonly Dictionary<Type, JournalEvent> journalEvents = new Dictionary<Type, JournalEvent>();

        /// <summary>
        /// Fire one single event
        /// </summary>
        private readonly bool fireSingleEvent = false;

        public event EventHandler<bool> LiveStatusChange;

        private bool isLive;
        public bool IsLive
        {
            get
            {
                if (ReadingHistory)
                {
                    return false;
                }
                return isLive;
            }
            protected set
            {
                isLive = value;
                LiveStatusChange?.Invoke(this, isLive);
            }
        }

        /// <summary>
        /// Use reflection to generate a list of event handlers. This allows for a dynamic list of handler classes, one for each type
        /// of event.
        /// </summary>
        static JournalWatcher()
        {
            try
            {

                var allHandlerTypes = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => typeof(JournalEvent).IsAssignableFrom(type));

                var handlers = from type in allHandlerTypes
                               where !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface)
                               select (JournalEvent)Activator.CreateInstance(type);

                foreach (var handler in handlers)
                {
                    try
                    {
                        journalEvents[handler.GetType()] = handler;
                        foreach (string eventName in handler.EventNames)
                        {
                            journalEventsByName[eventName] = handler;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("Error initializing JournalWatcher: " + handler.GetType().FullName);
                        var exception = e;
                        while (exception != null)
                        {
                            System.Diagnostics.Trace.TraceError(exception.ToString());
                            System.Diagnostics.Trace.TraceError(exception.StackTrace);
                            exception = exception.InnerException;
                        }

                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.ToString());
                    if (exSub is FileNotFoundException exFileNotFound)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }

                string errorMessage = sb.ToString();
                System.Diagnostics.Trace.TraceError("Error initializing JournalWatcher, loading " + ex.Message + " - " + ex.Source);
                System.Diagnostics.Trace.TraceError(ex.ToString());
                System.Diagnostics.Trace.TraceError(errorMessage);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Error initializing JournalWatcher");
                var exception = e;
                while (exception != null)
                {
                    System.Diagnostics.Trace.TraceError(exception.ToString());
                    System.Diagnostics.Trace.TraceError(exception.StackTrace);
                    exception = exception.InnerException;
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public JournalWatcher(string path, bool fireSingleEvent = false)
        {
            this.fireSingleEvent = fireSingleEvent;
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

        private readonly Regex journalFileRegex = new Regex(@"^(?<path>.*)\\Journal(Beta)?\.(?<timestamp>[0-9T-]+)\.(?<part>\d+)\.log$", RegexOptions.Compiled);

        /// <summary>
        /// This will look into the journal folder and check the latest journal.
        /// It will then fire events from all previous events in the current play session to facilitate
        /// rebuilding a status object before going "live".
        /// </summary>
        /// <returns></returns>
        private long ProcessPreviousJournals()
        {
            long offset = -1;
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f));
                if (!journals.Any())
                {
                    return 0; // there's nothing
                }

                // return the list until we find one with a part number 01.
                int partNr = 1;
                var match = journalFileRegex.Match(journals.First());
                if (match.Success && match.Groups["Commander"] != null)
                {
                    int.TryParse(match.Groups["part"].Value, out partNr);
                }

                var previousFiles = journals.Take(partNr).Reverse();

                // now process each journal
                foreach (string filename in previousFiles)
                {
                    string journalFile = System.IO.Path.Combine(Path, filename);
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        LatestJournalFile = filename;
                        //Trace.TraceInformation($"Journal: now reading previous entries from {LatestJournalFile}.");
                        offset = ParseData(reader, 0, filename);
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return -1;
            }

            return offset;
        }

        public bool ReadingHistory { get; private set; }
        public void ParseHistory(IProgress<string> progress)
        {
            if (progress is null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            ReadingHistory = true;
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderBy(x => GetFileCreationDate(x));
                if (!journals.Any())
                {
                    return; // there's nothing
                }

                // now process each journal
                foreach (string filename in journals)
                {
                    string journalFile = System.IO.Path.Combine(Path, filename);
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string[] fName = filename.Split('\\');
                        //Trace.TraceInformation($"Journal: now reading previous entries from {filename}.");
#pragma warning disable IDE0056 // Use index operator
                        progress.Report($"{fName[fName.Length - 1]}");
#pragma warning restore IDE0056 // Use index operator
                        _ = ParseData(reader, 0, filename);
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return;
            }

            ReadingHistory = false;
        }

        public void ParseHistory()
        {
            ReadingHistory = true;
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderBy(x => GetFileCreationDate(x));
                if (!journals.Any())
                {
                    return; // there's nothing
                }

                // now process each journal
                foreach (string filename in journals)
                {
                    //Trace.TraceInformation($"Journal: now reading previous entries from {filename}.");
                    string journalFile = System.IO.Path.Combine(Path, filename);
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string[] fName = filename.Split('\\');
                        //Trace.TraceInformation($"Journal: now reading previous entries from {filename}.");
                        _ = ParseData(reader, 0, filename);
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return;
            }

            ReadingHistory = false;
        }

        private DateTime GetFileCreationDate(string path)
        {
            try
            {
                var creationTime = File.GetCreationTimeUtc(path);
                var lastWriteTime = File.GetLastWriteTimeUtc(path);
                return creationTime < lastWriteTime ? creationTime : lastWriteTime;
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
                //Trace.TraceError($"Cannot watch non-existing folder {Path}.");
                return;
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel(false); // should not happen, but let's be safe, okay?
            }

            cancellationTokenSource = new CancellationTokenSource();

            long offset = 0;

            // before we start watching, rerun all events up until now (including any previous parts of this game session)

            await Task.Run(() => {
                offset = ProcessPreviousJournals();

                // because we might just have read an old log file, make sure we don't miss the new one when it arrives
                StartPollingForNewJournal();
                Created += async (sender, args) => await UpdateLatestJournalFile();
                Changed += JournalWatcher_Changed;

                if (offset >= 0)
                {
                    // finally send an event that we've gone live
                    IsLive = true;
                    FireEvent("MagicMau.IsLiveEvent", new JObject(new JProperty("timestamp", DateTime.UtcNow)));

                    if (!string.IsNullOrEmpty(LatestJournalFile))
                    {
                        CheckForJournalUpdateAsync(LatestJournalFile, offset);
                    }
                }

                EnableRaisingEvents = true;
            });
        }

        public virtual async Task StartWatchingFromFileOffset(string filename, long offset)
        {
            if (EnableRaisingEvents)
            {
                // Already watching
                return;
            }

            if (!Directory.Exists(Path))
            {
                //Trace.TraceError($"Cannot watch non-existing folder {Path}.");
                return;
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel(false); // should not happen, but let's be safe, okay?
            }

            cancellationTokenSource = new CancellationTokenSource();

            // before we start watching, rerun all events up until now (including any previous parts of this game session)
            await Task.Run(() => {
                offset = ProcessPreviousJournals(filename, offset);

                // because we might just have read an old log file, make sure we don't miss the new one when it arrives
                StartPollingForNewJournal();
                Created += async (sender, args) => await UpdateLatestJournalFile();
                Changed += JournalWatcher_Changed;

                if (offset >= 0)
                {
                    // finally send an event that we've gone live
                    IsLive = true;
                    //FireEvent("MagicMau.IsLiveEvent", new JObject(new JProperty("timestamp", DateTime.UtcNow)));

                    if (!string.IsNullOrEmpty(LatestJournalFile))
                    {
                        CheckForJournalUpdateAsync(LatestJournalFile, offset);
                    }
                }

                EnableRaisingEvents = true;
            });
        }

        private long ProcessPreviousJournals(string lastFilename, long readoffset)
        {
            long offset = -1;
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderBy(f => GetFileCreationDate(f)).ToList();
                if (!journals.Any())
                {
                    return 0; // there's nothing
                }

                if (string.IsNullOrEmpty(lastFilename) == false)
                {
                    string position = journals.FirstOrDefault(x => x.Contains(lastFilename));

                    int index = journals.IndexOf(position);

                    if (index > 0)
                    {
                        journals.RemoveRange(0, index);
                    }
                }

                // now process each journal
                for (int i = 0; i < journals.Count; i++)
                {
                    string filename = journals[i];
                    if (i > 0)
                    {
                        readoffset = 0;
                    }
                    string journalFile = System.IO.Path.Combine(Path, filename);

                    using var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    LatestJournalFile = filename;
                    //Trace.TraceInformation($"Journal: now reading previous entries from {LatestJournalFile}.");
                    offset = ParseData(reader, readoffset, filename);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return -1;
            }

            return offset;
        }

        public CargoEvent.CargoEventArgs ReadCargoJson()
        {
            try
            {
                string cargoPath = System.IO.Path.Combine(Path, "Cargo.json");

                if (!File.Exists(cargoPath))
                {
                    return null;
                }

                string json = File.ReadAllText(cargoPath, Encoding.UTF8);

                CargoEvent.CargoEventArgs cargo = JsonConvert.DeserializeObject<CargoEvent.CargoEventArgs>(json);

                return cargo;
            }
            catch (Exception e)
            {
                Trace.TraceWarning($"Error reading cargo.json journal file: {e.Message}");
                Trace.TraceInformation(e.ToString());
            }

            return null;
        }

        private async void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // if we're not watching anything, let's see if there is a log available
            if (LatestJournalFile == null || e.Name != LatestJournalFile)
            {
                await UpdateLatestJournalFile();
            }
        }

        internal void StartPollingForNewJournal()
        {
            if (isPollingForNewFile || cancellationTokenSource.IsCancellationRequested)
            {
                return; // we're already polling or no longer needed
            }

            isPollingForNewFile = true;
            Task.Run(async () => {
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
                IsLive = false;

                cancellationTokenSource?.Cancel();

                journalThread?.Join();
            }
            catch (System.OperationCanceledException e)
            {
                //Trace.TraceError($"Error while stopping Journal watcher: {e.Message}");
                Trace.TraceInformation(e.StackTrace);
            }
            catch { }
        }

        public long CurrentOffset { get; private set; }
        private void CheckForJournalUpdateAsync(string filename, long startOffset)
        {
            journalThreadId++;

            if (journalCancellationTokenSource != null)
            {
                journalCancellationTokenSource.Cancel();
            }

            if (journalThread != null && journalThread.IsAlive)
            {
                try
                {
                    if (!journalThread.Join(30000))
                    {
                        Trace.TraceError($"Something went wrong shutting down the previous journal reader thread");
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

            journalCancellationTokenSource = new CancellationTokenSource();
            journalThread = new Thread(state => {
                // keep a current ID for this thread. If the ID changes, we are watching a different file, and this thread can exit.
                (int id, long offset, var journalFileName, var cancellationToken) = (ValueTuple<int, long, string, CancellationToken>)state;
                string journalFile = System.IO.Path.Combine(Path, journalFileName);

#if DEBUG
                Trace.TraceInformation($"Journal: now starting journal thread {id} for {journalFile} from offset {offset}.");
#endif

                try
                {

                    using var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    while (id == journalThreadId && !cancellationToken.IsCancellationRequested)
                    {
                        // check for updates every 0.5 seconds
                        // if we are no longer watching (this thread), stop.
                        if (!Pause() || id != journalThreadId)
                        {
                            return;
                        }

                        // if the file size has not changed, idle
                        if (reader.BaseStream.Length <= offset)
                        {
                            continue;
                        }

                        // we found new data, so this is definitely not a stale file
                        isPollingForNewFile = false;

                        // parse the data we just read
                        offset = ParseData(reader, offset, journalFile);

                        Trace.TraceInformation($"Journal: now reading from offset {offset}.");
                    }

                }
                catch (Exception e)
                {
                    Trace.TraceError($"Something went wrong in the journal reader thread {id}: {e.Message}");
                    Trace.TraceInformation(e.StackTrace);
                    // Something went wrong, let's check log files again
                    LatestJournalFile = null;
                }
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (id == journalThreadId)
                    {
                        // We're here, so something must've gone wrong
                        // Let's try again in a few seconds
                        Pause();
                        UpdateLatestJournalFile().Wait(cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Trace.TraceInformation("Journal: Watcher Stopped");
#endif
                }

#if DEBUG
                Trace.TraceInformation($"Journal: end of journal thread for {journalFile}.");
#endif

            })
            {
                Name = "Journal Watcher",
                IsBackground = true
            };
            journalThread.Start((journalThreadId, startOffset, filename, journalCancellationTokenSource.Token));

        }

        private long ParseData(StreamReader reader, long offset, string filename)
        {
            try
            {
                // seek to the last max offset
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                // read new data
                string newData = reader.ReadToEnd();

                ParseText(newData, filename, offset);
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
                    offset = reader.BaseStream.Position;
                    CurrentOffset = offset;
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Exception while updating position in journal file: {e.Message}");
                    // might be something wrong with the file - let's start polling for a new one
                    StartPollingForNewJournal();
                }
            }
            return offset;
        }

        // Parses multiple lines of journal data
        public void ParseText(string text, string filename, long offset)
        {
            // split the new data into lines
            string[] lines = text.Split('\r', '\n');
            long lineDiffere = offset;
            // parse each line
            foreach (string line in lines)
            {
                Parse(line, filename, lineDiffere);
                lineDiffere += line.Length + 1;
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
            string[] journals = Directory.GetFiles(Path, DefaultFilter);

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
            string latestJournal = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f)).FirstOrDefault();

            bool isChanged = latestJournal != null && LatestJournalFile != latestJournal;
            if (isChanged)
            {
                LatestJournalFile = latestJournal;
                isPollingForNewFile = false;
                Trace.TraceInformation($"Journal: now reading from {LatestJournalFile}.");
                CheckForJournalUpdateAsync(latestJournal, 0);
            }


            return latestJournal;
        }

        /// <summary>
        /// Parses a line of JSON from the journal and fire a .NET event handler.
        /// </summary>
        /// <param name="line"></param>
        protected void Parse(string line, string filename, long offset)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            try
            {
                var evt = JObject.Parse(line);
                string eventType = evt.Value<string>("event");
                if (string.IsNullOrEmpty(eventType))
                {
                    return; // no event, nothing to do
                }

#if DEBUG
                if (IsLive)
                {
                    Trace.TraceInformation($"Journal - firing event {eventType} @ {evt["timestamp"]?.Value<string>()}\r\n\t{line}");
                }
#endif          
                var journalEventArgs = FireEvent(eventType, evt, fireSingleEvent);

                if (fireSingleEvent)
                {
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(journalEventArgs, eventType));
                    return;
                }
               
                if (journalEventArgs != null)
                {
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(journalEventArgs, eventType));
                }
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
        private JournalEventArgs FireEvent(string eventType, JObject evt, bool singleEvent = false)
        {
            if (journalEventsByName.TryGetValue(eventType, out var handler))
            {
                return handler.FireEvent(this, evt);
            }
#if DEBUG
            else
            {
                Trace.TraceWarning("No event handler registered for journal event of type: " + eventType);
                Console.WriteLine("No event handler registered for journal event of type: " + eventType);
            }
#endif
            return null;
        }

        public static JournalEventArgs GetEventData(string eventdata)
        {
            var evnt = JObject.Parse(eventdata);

            string eventType = evnt["event"].ToString();

            if(string.IsNullOrEmpty(eventType))
            {
                return null;
            }
            if (journalEventsByName.TryGetValue(eventType, out var handler))
            {
                // return handler.JsonToEvent(null, evnt);
            }

            return null;
        }

        public TJournalEvent GetEvent<TJournalEvent>() where TJournalEvent : JournalEvent
        {
            var type = typeof(TJournalEvent);
            return journalEvents.ContainsKey(type) ? journalEvents[type] as TJournalEvent : null;
        }
    }
}
