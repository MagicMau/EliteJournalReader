using EliteJournalReader.Events;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
        private Task journalTask = null;
        private volatile int journalTaskId = 0;

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

        public bool IsLive { get; protected set; } = false;

        // Track the last processed offset for the current journal file
        private long lastJournalFileOffset = 0;

        // Track if handlers are registered
        private bool handlersRegistered = false;

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
                            journalEventsByName[eventName] = handler;
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

        // Updated regex: use Path.DirectorySeparatorChar for cross-platform compatibility
        private readonly Regex journalFileRegex = new Regex(
            $@"Journal(Beta)?\.(?<timestamp>[0-9T-]+)\.(?<part>\d+)\.log$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// This will look into the journal folder and check the latest journal.
        /// It will then fire events from all previous events in the current play session to facilitate
        /// rebuilding a status object before going "live".
        /// </summary>
        /// <returns></returns>
        protected long ProcessPreviousJournals()
        {
            long offset = -1;
            try
            {
                var journals = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f));
                if (!journals.Any())
                    return 0; // there's nothing

                // return the list until we find one with a part number 01.
                int partNr = 1;
                var match = journalFileRegex.Match(System.IO.Path.GetFileName(journals.First()));
                if (match.Success)
                    int.TryParse(match.Groups["part"].Value, out partNr);

                var previousFiles = journals.Take(partNr).Reverse();

                // now process each journal
                foreach (string journalFile in previousFiles)
                {
                    // Store only the filename
                    LatestJournalFile = System.IO.Path.GetFileName(journalFile);
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        Trace.TraceInformation($"Journal: now reading previous entries from {LatestJournalFile}.");
                        offset = ParseData(reader, 0);
                    }
                }
                // Store the offset for the latest journal file
                lastJournalFileOffset = offset;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while parsing previous data from {LatestJournalFile}: " + e.Message);
                return -1;
            }

            return offset;
        }

        protected DateTime GetFileCreationDate(string path)
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
                Trace.TraceError($"Cannot watch non-existing folder {Path}.");
                return;
            }

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel(false); // should not happen, but let's be safe, okay?

            cancellationTokenSource = new CancellationTokenSource();

            // before we start watching, rerun all events up until now (including any previous parts of this game session)
            await Task.Run(() => {
                if (!IsLive)
                    lastJournalFileOffset = ProcessPreviousJournals();

                // because we might just have read an old log file, make sure we don't miss the new one when it arrives
                StartPollingForNewJournal();

                // Unregister previous handlers to prevent leaks/duplicates
                if (handlersRegistered)
                {
                    Created -= JournalWatcher_Created;
                    Changed -= JournalWatcher_Changed;
                    handlersRegistered = false;
                }

                // Register handlers if not already registered
                if (!handlersRegistered)
                {
                    Created += JournalWatcher_Created;
                    Changed += JournalWatcher_Changed;
                    handlersRegistered = true;
                }

                if (lastJournalFileOffset >= 0)
                {
                    // finally send an event that we've gone live
                    IsLive = true;
                    FireEvent("MagicMau.IsLiveEvent", new JObject(new JProperty("timestamp", DateTime.UtcNow)));

                    if (!string.IsNullOrEmpty(LatestJournalFile))
                        CheckForJournalUpdateAsync(LatestJournalFile, lastJournalFileOffset); // Use last offset
                }

                EnableRaisingEvents = true;
            });
        }

        // Add: Created event handler method
        private async void JournalWatcher_Created(object sender, FileSystemEventArgs e)
        {
            await UpdateLatestJournalFile();
        }

        public MarketEvent.MarketEventArgs ReadMarketJson()
        {
            try
            {
                string marketPath = System.IO.Path.Combine(Path, "Market.json");
                if (!File.Exists(marketPath))
                    return null;

                using var reader = new StreamReader(
                            new FileStream(marketPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var result = JToken.ReadFrom(new JsonTextReader(reader))
                    .ToObject<MarketEvent.MarketEventArgs>();
                return result;
            }
            catch (Exception e)
            {
                Trace.TraceWarning($"Error reading Market.json journal file: {e.Message}");
                Trace.TraceInformation(e.ToString());
            }

            return null;
        }

        public CargoEvent.CargoEventArgs ReadCargoJson()
        {
            try
            {
                string cargoPath = System.IO.Path.Combine(Path, "Cargo.json");
                if (!File.Exists(cargoPath))
                    return null;

                using var reader = new StreamReader(
                            new FileStream(cargoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var result = JToken.ReadFrom(new JsonTextReader(reader))
                    .ToObject<CargoEvent.CargoEventArgs>();
                return result;

            }
            catch (Exception e)
            {
                Trace.TraceWarning($"Error reading cargo.json journal file: {e.Message}");
                Trace.TraceInformation(e.ToString());
            }

            return null;
        }

        public NavRouteEvent.NavRouteEventArgs ReadNavRouteJson()
        {
            // The actual route is written to NavRoute.json, so let's try to read it
            try
            {
                string path = System.IO.Path.Combine(Path, "NavRoute.json");
                if (File.Exists(path))
                {
                    using var reader = new StreamReader(
                        new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    var navRoute = JToken.ReadFrom(
                        new JsonTextReader(reader)).ToObject<NavRouteEvent.NavRouteEventArgs>();
                    return navRoute;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Error reading NavRoute.json: " + e.Message);
                Trace.TraceInformation(e.ToString());
            }

            return null;
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

                // Unregister event handlers to prevent memory leaks
                if (handlersRegistered)
                {
                    Created -= JournalWatcher_Created;
                    Changed -= JournalWatcher_Changed;
                    handlersRegistered = false;
                }

                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();

                if (journalCancellationTokenSource != null)
                    journalCancellationTokenSource.Cancel();

                if (journalTask != null)
                {
                    try
                    {
                        journalTask.Wait(30000); // Wait up to 30 seconds
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var e in ex.InnerExceptions)
                        {
                            Trace.TraceError($"Error while stopping Journal watcher task: {e.Message}");
                            Trace.TraceInformation(e.StackTrace);
                        }
                    }
                    finally
                    {
                        journalTask = null;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while stopping Journal watcher: {e.Message}");
                Trace.TraceInformation(e.StackTrace);
            }
        }


        private void CheckForJournalUpdateAsync(string filename, long startOffset)
        {
            journalTaskId++;

            if (journalCancellationTokenSource != null)
                journalCancellationTokenSource.Cancel();

            if (journalTask != null && !journalTask.IsCompleted)
            {
                try
                {
                    if (!journalTask.Wait(30000))
                    {
                        Trace.TraceError($"Something went wrong shutting down the previous journal reader task");
                    }
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Trace.TraceError($"Something went wrong shutting down the previous journal reader task: {e.Message}");
                    }
                }
                finally
                {
                    journalTask = null;
                }
            }

            journalCancellationTokenSource = new CancellationTokenSource();
            int currentTaskId = journalTaskId;
            string journalFile = System.IO.Path.Combine(Path, filename); // always use full path for file operations
            long offset = startOffset;
            var cancellationToken = journalCancellationTokenSource.Token;

            journalTask = Task.Factory.StartNew(async () => {
#if DEBUG
                Trace.TraceInformation($"Journal: now starting journal task {currentTaskId} for {journalFile} from offset {offset}.");
#endif
                try
                {
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (currentTaskId == journalTaskId && !cancellationToken.IsCancellationRequested)
                        {
                            // check for updates every 0.5 seconds
                            if (!await PauseAsync(cancellationToken) || currentTaskId != journalTaskId)
                                return;

                            // if the file size has not changed, idle
                            if (reader.BaseStream.Length <= offset)
                                continue;

                            // we found new data, so this is definitely not a stale file
                            isPollingForNewFile = false;

                            // parse the data we just read
                            offset = ParseData(reader, offset);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Something went wrong in the journal reader task {currentTaskId}: {e.Message}");
                    Trace.TraceInformation(e.StackTrace);
                    // Something went wrong, let's check log files again
                    LatestJournalFile = null;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (currentTaskId == journalTaskId)
                {
                    // We're here, so something must've gone wrong
                    // Let's try again in a few seconds
                    await PauseAsync(cancellationToken);
                    await UpdateLatestJournalFile();
                }
#if DEBUG
                Trace.TraceInformation($"Journal: end of journal task for {journalFile}.");
#endif
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private long ParseData(StreamReader reader, long offset)
        {
            try
            {
                // seek to the last max offset
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.DiscardBufferedData(); // Ensure StreamReader buffer is in sync with new position

                // Efficiently read new lines from the current offset
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ParseAndProcess(line);
                }
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
                    // Update the lastJournalFileOffset if this is the current file
                    lastJournalFileOffset = offset;
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
        public void ParseText(string text)
        {
            // This method is still used for historical reads, but not for live journal reading.
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
                ParseAndProcess(line);
        }

        private static async Task<bool> PauseAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(UPDATE_INTERVAL_MILLISECONDS, token);
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

            string latestJournal = Directory.GetFiles(Path, DefaultFilter).OrderByDescending(f => GetFileCreationDate(f)).FirstOrDefault();
            string latestJournalFileName = System.IO.Path.GetFileName(latestJournal);

            bool isChanged = latestJournal != null && LatestJournalFile != latestJournalFileName;
            if (isChanged)
            {
                LatestJournalFile = latestJournalFileName;
                isPollingForNewFile = false;
                FireEvent("MagicMau.NewJournalFileEvent", new JObject(
                    new JProperty("timestamp", DateTime.UtcNow),
                    new JProperty("Filename", LatestJournalFile)));
                Trace.TraceInformation($"Journal: now reading from {LatestJournalFile}.");

                // New file, start from offset 0
                lastJournalFileOffset = 0;
                CheckForJournalUpdateAsync(LatestJournalFile, 0);
            }
            else if (latestJournal != null && LatestJournalFile == latestJournalFileName)
            {
                // Only start a new task if the file has grown
                var fileLength = new FileInfo(System.IO.Path.Combine(Path, LatestJournalFile)).Length;
                if (fileLength > lastJournalFileOffset)
                {
                    CheckForJournalUpdateAsync(LatestJournalFile, lastJournalFileOffset);
                }
                // else: do nothing, no new data
            }

            return latestJournalFileName;
        }

        /// <summary>
        /// Parses a line of JSON from the journal and fire a .NET event handler.
        /// </summary>
        /// <param name="line"></param>
        protected void ParseAndProcess(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            try
            {
                var evt = JObject.Parse(line);
                Process(evt, line);
            }
            catch (JsonReaderException jre)
            {
                Trace.TraceError($"Error parsing journal line: {jre.Message}\r\n\t{line}");
                OnError(new ErrorEventArgs(jre));
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception handling journal line:\r\n\t{line}\r\n\t{e.GetType().FullName}: {e.Message}");
                OnError(new ErrorEventArgs(e));
            }
        }

        protected void Process(JObject evt, string json)
        {
            try
            {
                string eventType = evt?.Value<string>("event") ?? throw new ArgumentNullException(nameof(evt));
                if (string.IsNullOrEmpty(eventType))
                    return; // no event, nothing to do

#if DEBUG
                if (IsLive)
                    Trace.TraceInformation($"Journal - firing event {eventType} @ {evt["timestamp"]?.Value<string>()}\r\n\t{evt.ToString(Formatting.None)}");
#endif

                var journalEventArgs = FireEvent(eventType, evt);
                if (journalEventArgs != null)
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(journalEventArgs, eventType, json));
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception handling journal event:\r\n\t{evt.ToString(Formatting.None)}\r\n\t{e.GetType().FullName}: {e.Message}");
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Find the event handler for the given type. If found, invoke it.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="evt"></param>
        protected virtual JournalEventArgs FireEvent(string eventType, JObject evt)
        {
            if (journalEventsByName.TryGetValue(eventType, out var handler))
                return handler.FireEvent(this, evt);
            else
                Trace.TraceWarning("No event handler registered for journal event of type: " + eventType);

            return null;
        }

        public TJournalEvent GetEvent<TJournalEvent>() where TJournalEvent : JournalEvent
        {
            var type = typeof(TJournalEvent);
            return journalEvents.ContainsKey(type) ? journalEvents[type] as TJournalEvent : null;
        }

        public IEnumerable<JournalEventArgs> RetrieveHistoricalEvents(Action<double> progressUpdater, params string[] eventNames)
        {
            var events = new List<JournalEventArgs>();
            var journals = Directory.GetFiles(Path, DefaultFilter).OrderBy(f => GetFileCreationDate(f)).ToArray();
            if (journals.Length == 0)
                return events; // there's nothing

            // now process each journal
            for (int i = 0; i < journals.Length; i++)
            {
                string filename = journals[i];
                progressUpdater?.Invoke(i / (double)journals.Length);
                try
                {
                    string journalFile = System.IO.Path.Combine(Path, filename);
                    using (var reader = new StreamReader(new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            var evt = JObject.Parse(line);
                            string eventType = evt.Value<string>("event");
                            if (string.IsNullOrEmpty(eventType))
                                continue; // no event, nothing to do

                            if (eventNames.Contains(eventType))
                            {
                                var type = journalEventsByName[eventType];
                                events.Add(type.ParseEventArgs(evt));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error while parsing previous data from {filename}: " + e.Message);
                }
            }

            return events;
        }
    }
}
