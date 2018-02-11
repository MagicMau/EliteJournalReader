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
        private const string DefaultFilter = @"status.json";

        /// <summary>
        /// Token to signal that we are no longer watching
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public StatusWatcher(string path)
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

            Created += UpdateStatus;

            EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            Created -= UpdateStatus;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            EnableRaisingEvents = false;
        }

        private void UpdateStatus(object sender, FileSystemEventArgs e)
        {
            UpdateStatus(e.FullPath, 0);
        }

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

                    StatusUpdated?.Invoke(this, evt);
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
    }
    
}
