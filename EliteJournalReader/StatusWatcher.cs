using EliteJournalReader.Events;
using Newtonsoft.Json;
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
    /// File watcher and parser for the status.json to be introduced in Elite:Dangerous 3.0.
    /// This file is regenerated approximately every second.
    /// The file contains a single json statement that is fired off as a .NET event
    /// </summary>
    public class StatusWatcher : FileSystemWatcher
    {
        public event EventHandler<StatusFileEvent> StatusUpdated;

        /// <summary>
        ///     The default filter
        /// </summary>
        private const string DefaultFilter = @"Status.json";

        /// <summary>
        /// Token to signal that we are no longer watching
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public StatusWatcher(string path)
        {
            Initialize(path);
        }

        protected void Initialize(string path)
        {
            Filter = DefaultFilter;
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;

            try
            {
                Path = System.IO.Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in setting path: " + ex.Message);
            }
        }

        /// <summary>
        /// For unit tests only
        /// </summary>
        protected StatusWatcher()
        {
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
        public virtual void StartWatching()
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
                cancellationTokenSource.Cancel(); // should not happen, but let's be safe, okay?

            cancellationTokenSource = new CancellationTokenSource();

            Created += UpdateStatus;
            Changed += UpdateStatus;
            Renamed += UpdateStatus;

            EnableRaisingEvents = true;
        }

        public virtual void StopWatching()
        {
            if (EnableRaisingEvents)
            {
                Created -= UpdateStatus;

                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();

                EnableRaisingEvents = false;
            }
        }

        protected void UpdateStatus(object sender, FileSystemEventArgs e)
        {
            UpdateStatus(e.FullPath, 0);
        }

        private DateTime lastTimestamp = DateTime.MinValue;
        private void UpdateStatus(string fullPath, int attempt)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    var evt = JToken.ReadFrom(jsonTextReader).ToObject<StatusFileEvent>();
                    if (evt == null)
                        throw new ArgumentNullException($"Unexpected empty status.json file");

                    // only fire the event if it's new data
                    if (evt.Timestamp > lastTimestamp)
                    {
                        lastTimestamp = evt.Timestamp;
                        FireStatusUpdatedEvent(evt);
                    }
                }
            }
            catch (IOException ioe)
            {
                // it could be that we are trying to read at the exact same time the 
                // game is writing a new status.json. To handle this case, we simply give it another shot.
                if (attempt < 5)
                    UpdateStatus(fullPath, attempt++);
                else
                    Trace.TraceError($"IO Error while reading from status.json: {ioe.Message}\n{ioe.StackTrace}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error while reading from status.json: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected void FireStatusUpdatedEvent(StatusFileEvent evt) => StatusUpdated?.Invoke(this, evt);
    }

}
