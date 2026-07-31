using EliteJournalReader.Events;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        /// The actual unwrapped reader-loop task. This is the real async body,
        /// not an outer Task wrapping another Task.
        /// </summary>
        private Task _readerTask;

        /// <summary>
        /// Asynchronous lifecycle gate — serializes start, stop, restart, cancellation,
        /// and file switching so at most one reader is active at any time.
        /// </summary>
        private readonly SemaphoreSlim _lifecycleGate = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Signal channel: filesystem callbacks enqueue signals here instead of
        /// performing lifecycle work directly. The reader loop or a dedicated
        /// consumer drains these signals under the lifecycle gate.
        /// </summary>
        private readonly System.Collections.Concurrent.ConcurrentQueue<LifecycleSignal> _signalQueue
            = new System.Collections.Concurrent.ConcurrentQueue<LifecycleSignal>();

        /// <summary>
        /// Wakes the signal processor when a new signal is enqueued.
        /// </summary>
        private readonly SemaphoreSlim _signalReady = new SemaphoreSlim(0);

        /// <summary>
        /// Token to signal that we are no longer watching
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Token that triggers the end of the current journal reader
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

        /// <summary>
        /// Byte-level newline framing cursor for the current journal file.
        /// Commits offset only through the last terminating newline.
        /// </summary>
        private JournalRecordFramer journalFramer;

        /// <summary>
        /// Injectable file-identity provider for detecting truncation or in-place replacement.
        /// On Windows uses GetFileInformationByHandle for volume/file identity.
        /// Falls back to metadata-based identity on unsupported platforms.
        /// </summary>
        private IFileIdentityProvider _fileIdentityProvider;

        // Track if handlers are registered
        private bool handlersRegistered = false;

        /// <summary>
        /// Captures a reader-task fault so it can be surfaced to callers of StopWatchingAsync.
        /// </summary>
        private Exception _readerFault;

        /// <summary>
        /// Signals that filesystem callbacks enqueue for the lifecycle processor.
        /// </summary>
        private enum LifecycleSignal
        {
            FileCreated,
            FileChanged
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
        public JournalWatcher(string path) : this(path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance with an explicit file-identity provider.
        /// Pass null to use the platform default (Windows volume/file identity or metadata fallback).
        /// </summary>
        public JournalWatcher(string path, IFileIdentityProvider fileIdentityProvider)
        {
            Filter = DefaultFilter;
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            _fileIdentityProvider = fileIdentityProvider ?? CreateDefaultFileIdentityProvider();
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
            _fileIdentityProvider = CreateDefaultFileIdentityProvider();
        }

        /// <summary>
        /// Creates the platform-appropriate default file identity provider.
        /// On Windows, uses GetFileInformationByHandle for stable volume/file identity.
        /// On other platforms, uses the documented metadata fallback (creation time + path hash).
        /// </summary>
        private static IFileIdentityProvider CreateDefaultFileIdentityProvider()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsFileIdentityProvider();
            return new MetadataFileIdentityProvider();
        }

        /// <summary>
        /// Sets a custom file-identity provider. Primarily intended for testing.
        /// </summary>
        internal void SetFileIdentityProvider(IFileIdentityProvider provider)
        {
            _fileIdentityProvider = provider ?? CreateDefaultFileIdentityProvider();
        }

        // Updated regex: use Path.DirectorySeparatorChar for cross-platform compatibility
        private readonly Regex journalFileRegex = new Regex(
            $@"Journal(Beta)?\.(?<timestamp>[0-9T-]+)\.(?<part>\d+)\.log$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// This will look into the journal folder and check the latest journal.
        /// It will then fire events from all previous events in the current play session to facilitate
        /// rebuilding a status object before going "live".
        ///
        /// Uses JournalSessionSelector to parse canonical filenames, group by session identity,
        /// select the greatest session, and order by numeric part number. Falls back to a single
        /// legacy file when no canonical files exist. Never mixes sessions or orders lexically.
        /// </summary>
        /// <returns></returns>
        protected long ProcessPreviousJournals()
        {
            long offset = -1;
            try
            {
                var allFiles = Directory.GetFiles(Path, DefaultFilter);
                if (allFiles.Length == 0)
                    return 0; // there's nothing

                var selector = new JournalSessionSelector();
                var sessionFiles = selector.SelectSessionFiles(allFiles, GetLastWriteUtc);

                if (sessionFiles.Count == 0)
                    return 0;

                // Process each journal file in the selected session (ordered by part number)
                foreach (string journalFile in sessionFiles)
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

        /// <summary>
        /// Gets the last write time in UTC for a file path. Used by JournalSessionSelector
        /// for legacy fallback ordering.
        /// </summary>
        private DateTime GetLastWriteUtc(string path)
        {
            try
            {
                return File.GetLastWriteTimeUtc(path);
            }
            catch
            {
                return DateTime.MinValue;
            }
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
            await _lifecycleGate.WaitAsync().ConfigureAwait(false);
            try
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
                _readerFault = null;

                // before we start watching, rerun all events up until now (including any previous parts of this game session)
                await Task.Run(() => {
                    if (!IsLive)
                        lastJournalFileOffset = ProcessPreviousJournals();

                    // because we might just have read an old log file, make sure we don't miss the new one when it arrives
                    StartPollingForNewJournal();
                }).ConfigureAwait(false);

                // Unregister previous handlers to prevent leaks/duplicates
                if (handlersRegistered)
                {
                    Created -= JournalWatcher_Created;
                    Changed -= JournalWatcher_Changed;
                    handlersRegistered = false;
                }

                // Register handlers
                Created += JournalWatcher_Created;
                Changed += JournalWatcher_Changed;
                handlersRegistered = true;

                if (lastJournalFileOffset >= 0)
                {
                    // finally send an event that we've gone live
                    IsLive = true;
                    FireEvent("MagicMau.IsLiveEvent", new JObject(new JProperty("timestamp", DateTime.UtcNow)));

                    if (!string.IsNullOrEmpty(LatestJournalFile))
                        StartReaderTask(LatestJournalFile, lastJournalFileOffset);
                }

                EnableRaisingEvents = true;

                // Start the signal processor that drains filesystem callback signals
                StartSignalProcessor();
            }
            finally
            {
                _lifecycleGate.Release();
            }
        }

        // Filesystem callbacks synchronously enqueue a signal instead of performing lifecycle work.
        // The signal processor drains these under the lifecycle gate.
        private void JournalWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _signalQueue.Enqueue(LifecycleSignal.FileCreated);
            _signalReady.Release();
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

        private void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Enqueue a change signal — lifecycle work happens on the signal processor
            _signalQueue.Enqueue(LifecycleSignal.FileChanged);
            _signalReady.Release();
        }

        internal void StartPollingForNewJournal()
        {
            if (isPollingForNewFile || cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                return; // we're already polling, not started, or no longer needed

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

                        // Acquire the lifecycle gate before performing file checks
                        await _lifecycleGate.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                        try
                        {
                            await UpdateLatestJournalFile();
                        }
                        finally
                        {
                            _lifecycleGate.Release();
                        }
                    }
                    catch (OperationCanceledException)
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

        /// <summary>
        /// Stops the watcher asynchronously. This is the primary stop method.
        /// Awaits the actual reader task before clearing state, exposing reader faults to callers.
        /// </summary>
        public virtual async Task StopWatchingAsync(CancellationToken cancellationToken = default)
        {
            await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await StopWatchingCoreAsync().ConfigureAwait(false);
            }
            finally
            {
                _lifecycleGate.Release();
            }
        }

        /// <summary>
        /// Core stop logic — must be called under the lifecycle gate.
        /// </summary>
        private async Task StopWatchingCoreAsync()
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

            // Await the actual reader task — this is the unwrapped task, not an outer wrapper
            if (_readerTask != null)
            {
                try
                {
                    await _readerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is normal control flow
                }
                catch (Exception ex)
                {
                    // Propagate reader faults to the error surface but don't throw here
                    _readerFault = ex;
                    Trace.TraceError($"Reader task faulted during stop: {ex.Message}");
                    Trace.TraceInformation(ex.StackTrace);
                    OnError(new ErrorEventArgs(ex));
                }
                finally
                {
                    _readerTask = null;
                }
            }

            // Release the signal processor if it's waiting
            _signalReady.Release();
        }

        /// <summary>
        /// Synchronous compatibility wrapper. Observes the same reader task as StopWatchingAsync.
        /// Retained for callers that cannot yet be made async.
        /// </summary>
        public virtual void StopWatching()
        {
            // Acquire the lifecycle gate synchronously
            _lifecycleGate.Wait();
            try
            {
                StopWatchingCoreAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while stopping Journal watcher: {e.Message}");
                Trace.TraceInformation(e.StackTrace);
            }
            finally
            {
                _lifecycleGate.Release();
            }
        }


        /// <summary>
        /// Starts the signal processor that drains filesystem callback signals
        /// and performs lifecycle transitions under the lifecycle gate.
        /// </summary>
        private void StartSignalProcessor()
        {
            var token = cancellationTokenSource.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await _signalReady.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    // Drain all queued signals
                    bool needsFileCheck = false;
                    while (_signalQueue.TryDequeue(out _))
                    {
                        needsFileCheck = true;
                    }

                    if (!needsFileCheck || token.IsCancellationRequested)
                        continue;

                    // Perform lifecycle work under the gate
                    await _lifecycleGate.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (token.IsCancellationRequested)
                            return;

                        await UpdateLatestJournalFile().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error processing filesystem signal: {ex.Message}");
                        Trace.TraceInformation(ex.StackTrace);
                        OnError(new ErrorEventArgs(ex));
                    }
                    finally
                    {
                        _lifecycleGate.Release();
                    }
                }
            }, token);
        }

        /// <summary>
        /// Starts the reader loop for the given journal file.
        /// Must be called under the lifecycle gate.
        /// Uses Task.Run to get a properly unwrapped task.
        /// </summary>
        private void StartReaderTask(string filename, long startOffset)
        {
            if (journalCancellationTokenSource != null)
                journalCancellationTokenSource.Cancel();

            // Await the previous reader before starting a new one
            if (_readerTask != null && !_readerTask.IsCompleted)
            {
                try
                {
                    _readerTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling the previous reader
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Previous reader task faulted: {ex.Message}");
                    Trace.TraceInformation(ex.StackTrace);
                    OnError(new ErrorEventArgs(ex));
                }
                finally
                {
                    _readerTask = null;
                }
            }

            journalCancellationTokenSource = new CancellationTokenSource();
            string journalFile = System.IO.Path.Combine(Path, filename);
            long offset = startOffset;
            var cancellationToken = journalCancellationTokenSource.Token;
            _readerFault = null;

            // Task.Run returns the unwrapped task — the actual async reader body.
            // This is the fix for the nested-task semantics of Task.Factory.StartNew(async () => ...).
            _readerTask = Task.Run(() => ReaderLoopAsync(journalFile, offset, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// The actual reader loop. Runs as the unwrapped task stored in _readerTask.
        /// Exceptions propagate to awaiting callers through the task.
        /// Before each read, compares file identity and length with current state.
        /// On truncation (shrink) or identity change, resets all framing state and restarts from byte zero.
        /// </summary>
        private async Task ReaderLoopAsync(string journalFile, long offset, CancellationToken cancellationToken)
        {
#if DEBUG
            Trace.TraceInformation($"Journal: now starting reader for {journalFile} from offset {offset}.");
#endif
            // Initialize the byte-level framer for this journal file
            var framer = new JournalRecordFramer(offset);
            journalFramer = framer;

            // Capture initial file identity from the open handle
            FileIdentity? currentIdentity = null;
            var stream = new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try
            {
                currentIdentity = _fileIdentityProvider.GetIdentity(stream);
#if DEBUG
                Trace.TraceInformation($"Journal: initial file identity for {journalFile}: {currentIdentity}");
#endif

                while (!cancellationToken.IsCancellationRequested)
                {
                    // check for updates every 0.5 seconds
                    if (!await PauseAsync(cancellationToken) || cancellationToken.IsCancellationRequested)
                        return;

                    // Before reading, check for truncation or replacement
                    bool needsReset = false;
                    long currentLength;

                    try
                    {
                        currentLength = stream.Length;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream was closed during reset — exit normally
                        return;
                    }

                    // Check 1: file length shrunk below read offset (truncation)
                    if (currentLength < framer.ReadOffset)
                    {
                        Trace.TraceWarning($"Journal: file truncated — length {currentLength} < read offset {framer.ReadOffset}. Resetting.");
                        needsReset = true;
                    }

                    // Check 2: file identity changed (in-place replacement)
                    if (!needsReset)
                    {
                        var newIdentity = _fileIdentityProvider.GetIdentity(stream);
                        if (currentIdentity.HasValue && newIdentity.HasValue && currentIdentity.Value != newIdentity.Value)
                        {
                            Trace.TraceWarning($"Journal: file identity changed from {currentIdentity} to {newIdentity}. Resetting.");
                            needsReset = true;
                        }
                        // Also check by path in case the handle-based identity is stale on some filesystems
                        if (!needsReset)
                        {
                            var pathIdentity = _fileIdentityProvider.GetIdentity(journalFile);
                            if (currentIdentity.HasValue && pathIdentity.HasValue && currentIdentity.Value != pathIdentity.Value)
                            {
                                Trace.TraceWarning($"Journal: file identity at path changed from {currentIdentity} to {pathIdentity}. Resetting.");
                                needsReset = true;
                            }
                        }
                    }

                    if (needsReset)
                    {
                        // Close the old handle — discard committed/read offsets and pending bytes
                        stream.Dispose();

                        // Reset framer state completely — old partial bytes can never combine with replacement content
                        framer.Reset();
                        lastJournalFileOffset = 0;

                        // Reopen and consume from byte zero
                        stream = new FileStream(journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        currentIdentity = _fileIdentityProvider.GetIdentity(stream);
#if DEBUG
                        Trace.TraceInformation($"Journal: reopened {journalFile} after reset. New identity: {currentIdentity}");
#endif
                        // Continue the loop — will parse from offset 0 on next iteration's read
                        continue;
                    }

                    // if the file size has not changed beyond committed data, idle
                    if (currentLength <= framer.ReadOffset && !framer.HasPendingBytes)
                        continue;

                    // we found new data, so this is definitely not a stale file
                    isPollingForNewFile = false;

                    // parse the data using byte-level newline framing
                    offset = ParseData(stream, framer);
                }
            }
            finally
            {
                stream.Dispose();
            }
#if DEBUG
            Trace.TraceInformation($"Journal: end of reader for {journalFile}.");
#endif
        }

        private long ParseData(FileStream stream, JournalRecordFramer framer)
        {
            try
            {
                // Read complete newline-terminated records using byte-level framing
                string[] completedLines = framer.ReadCompleteRecords(stream);

                foreach (string line in completedLines)
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
                // Update the last committed offset (only through last consumed newline)
                lastJournalFileOffset = framer.CommittedOffset;
            }
            return framer.CommittedOffset;
        }

        /// <summary>
        /// Legacy ParseData overload retained for backward compatibility with any subclass usage.
        /// Delegates to the byte-level framer approach.
        /// </summary>
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
        ///     Must be called under the lifecycle gate.
        ///     In live mode, switches only to a newer session or a higher part of the selected session.
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

            // Use the session selector for deterministic live-mode file selection.
            // Switch only to a newer session or a higher part of the current session.
            string latestJournalFileName = DetermineLatestJournalFile(journals);

            bool isChanged = latestJournalFileName != null && LatestJournalFile != latestJournalFileName;
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
                StartReaderTask(LatestJournalFile, 0);
            }
            else if (latestJournalFileName != null && LatestJournalFile == latestJournalFileName)
            {
                // Only start a new task if the file has grown and no reader is active
                if (_readerTask == null || _readerTask.IsCompleted)
                {
                    var fileLength = new FileInfo(System.IO.Path.Combine(Path, LatestJournalFile)).Length;
                    if (fileLength > lastJournalFileOffset)
                    {
                        StartReaderTask(LatestJournalFile, lastJournalFileOffset);
                    }
                }
            }

            return latestJournalFileName;
        }

        /// <summary>
        /// Determines which journal file should be considered "latest" for live-mode switching.
        /// Uses parsed canonical session identity: switches only to a newer session or a higher
        /// part of the currently selected session. Falls back to metadata for legacy files.
        /// </summary>
        private string DetermineLatestJournalFile(string[] journalPaths)
        {
            if (journalPaths.Length == 0)
                return null;

            // If we already have a selected file, use the session selector to check for upgrades
            if (!string.IsNullOrEmpty(LatestJournalFile))
            {
                var currentParsed = JournalSessionSelector.TryParse(
                    System.IO.Path.Combine(Path, LatestJournalFile));

                if (currentParsed.HasValue)
                {
                    // We have a canonical current file — only accept newer sessions or higher parts
                    var selector = new JournalSessionSelector();
                    string bestCandidate = null;
                    string bestSessionKey = currentParsed.Value.SessionKey;
                    int bestPart = currentParsed.Value.PartNumber;

                    foreach (var filePath in journalPaths)
                    {
                        if (selector.ShouldSwitchToFile(bestSessionKey, bestPart, filePath))
                        {
                            var parsed = JournalSessionSelector.TryParse(filePath);
                            if (parsed.HasValue)
                            {
                                bestSessionKey = parsed.Value.SessionKey;
                                bestPart = parsed.Value.PartNumber;
                                bestCandidate = filePath;
                            }
                        }
                    }

                    if (bestCandidate != null)
                        return System.IO.Path.GetFileName(bestCandidate);

                    // No upgrade found — stay on current file
                    return LatestJournalFile;
                }
            }

            // No current file, or current file is legacy — use session selector for initial selection
            var sessionSelector = new JournalSessionSelector();
            var sessionFiles = sessionSelector.SelectSessionFiles(journalPaths, GetLastWriteUtc);
            if (sessionFiles.Count > 0)
            {
                // Return the last file in the session (highest part number)
                return System.IO.Path.GetFileName(sessionFiles[sessionFiles.Count - 1]);
            }

            return null;
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
