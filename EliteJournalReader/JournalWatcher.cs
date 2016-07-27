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
        public const int UPDATE_INTERVAL_MILLISECONDS = 5000;
        

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
        private string latestJournalFile;

        /// <summary>
        /// Monitor the journal in a separate thread
        /// </summary>
        private Thread journalThread = null;
        private volatile int journalThreadId = 0;

        /// <summary>
        /// Token to signal that we are no longer watching
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        private static Dictionary<string, JournalEvent> journalEventsByName = new Dictionary<string, JournalEvent>();
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
        ///     Gets or sets the latest log file.
        /// </summary>
        public string LatestJournalFile
        {
            get
            {
                return latestJournalFile;
            }
            set
            {
                latestJournalFile = value;
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

        /// <summary>
        /// Forces a refresh of the <see cref="LatestJournalFile"/> and calls <see cref="CheckForSystemChange"/>
        /// </summary>
        public async void Refresh()
        {
            await UpdateLatestJournalFile();
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

            var _ = UpdateLatestJournalFile(); // let's not care that it's awaitable
            Created += async (sender, args) => await UpdateLatestJournalFile();
            Changed += JournalWatcher_Changed;

            EnableRaisingEvents = true;
        }

        private async void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // if we're not watching anything, let's see if there is a log available
            if (latestJournalFile == null || e.Name != latestJournalFile)
                await UpdateLatestJournalFile();
        }

        public void StopWatching()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            if (journalThread != null)
                journalThread.Join();
        }


        private void CheckForSystemChangeAsync()
        {
            journalThread = new Thread(ido =>
            {
                // keep a current ID for this thread. If the ID changes, we are watching a different file, and this thread can exit.
                int id = (int)ido;
                var logFile = System.IO.Path.Combine(Path, latestJournalFile);
                try
                {
                    using (StreamReader reader = new StreamReader(new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (id == journalThreadId)
                        {
                            // check for updates every 5 seconds
                            // if we are no longer watching (this thread), stop.
                            if (!Pause() || id != journalThreadId)
                                return;

                            // if the file size has not changed, idle
                            if (reader.BaseStream.Length == lastOffset)
                                continue;

                            // seek to the last max offset
                            reader.BaseStream.Seek(lastOffset, SeekOrigin.Begin);

                            // read new data
                            var newData = reader.ReadToEnd();

                            // update the last max offset
                            lastOffset = reader.BaseStream.Position;

                            // split the new data into lines
                            var lines = newData.Split('\r', '\n');

                            // parse each line
                            foreach (var line in lines)
                                Parse(line);
                        }
                    }
                }
                catch
                {
                    // Something went wrong, let's check log files again
                    latestJournalFile = null;
                }

                if (id == journalThreadId)
                {
                    // We're here, so something must've gone wrong
                    // Let's try again in a few seconds
                    Pause();
                    UpdateLatestJournalFile().Wait();
                }
            });
            journalThread.IsBackground = true;
            journalThread.Start(++journalThreadId);

        }

        private bool Pause()
        {
            try
            {
                Task.Delay(UPDATE_INTERVAL_MILLISECONDS, cancellationTokenSource.Token).Wait();
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
            var journals = Directory.GetFiles(Path, "Journal.*");

            while (journals.Length == 0)
            {
                try
                {
                    await Task.Delay(UPDATE_INTERVAL_MILLISECONDS, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
            }

            var latestJournal = journals.OrderByDescending(f => f).First();

            if (latestJournalFile != latestJournal)
            {
                lastOffset = 0;
                LatestJournalFile = latestJournal;
                CheckForSystemChangeAsync();
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
                FireEvent(eventType, evt);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
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
