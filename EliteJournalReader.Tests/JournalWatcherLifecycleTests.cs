#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    [TestClass]
    public class JournalWatcherLifecycleTests
    {
        private string _tempDirectory = null!;
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        [TestInitialize]
        public void Initialize()
        {
            _tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "EliteJournalReader.Lifecycle",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                // A failed assertion should not be hidden by temporary-file cleanup.
            }
        }

        [TestMethod]
        public async Task FileGrowth_DoesNotDisableFutureDirectoryDiscovery()
        {
            // **Validates: Requirements 2.9, 2.10, 3.4, 3.5, 3.6**
            string currentPath = Path.Combine(
                _tempDirectory, "Journal.20260407140000.01.log");
            WriteJournal(currentPath, Record("2026-04-07T14:00:00Z", "Sol"));

            var receivedSystems = new ConcurrentQueue<string>();
            var watcher = new JournalWatcher(_tempDirectory);
            watcher.MessageReceived += (_, args) =>
            {
                string? system = args.EventArgs.OriginalEvent?["StarSystem"]?.Value<string>();
                if (system != null)
                    receivedSystems.Enqueue(system);
            };

            try
            {
                await watcher.StartWatching();
                Assert.IsTrue(await WaitUntilAsync(
                    () => receivedSystems.Contains("Sol"), 3000));

                File.AppendAllText(
                    currentPath,
                    Record("2026-04-07T14:00:01Z", "Barnard's Star"),
                    Utf8NoBom);
                Assert.IsTrue(await WaitUntilAsync(
                    () => receivedSystems.Contains("Barnard's Star"), 3000),
                    "The selected-file reader must retain its approximately 500 ms append cadence.");
                Assert.IsTrue(watcher.IsDirectoryDiscoveryActive,
                    "Selected-file growth must not stop watcher-lifetime directory discovery.");

                // Suppress the notification path after growth. The persistent fallback scan
                // must still discover and select this chronologically newer session.
                watcher.EnableRaisingEvents = false;
                string newerPath = Path.Combine(
                    _tempDirectory, "Journal.20260407150000.01.log");
                WriteJournal(newerPath, Record("2026-04-07T15:00:00Z", "Achenar"));

                int discoveryTimeout =
                    JournalWatcher.DIRECTORY_DISCOVERY_INTERVAL_MILLISECONDS + 3000;
                Assert.IsTrue(await WaitUntilAsync(
                    () => receivedSystems.Contains("Achenar"), discoveryTimeout),
                    "A future fallback scan must remain active after selected-file growth.");
                Assert.AreEqual(Path.GetFileName(newerPath), watcher.LatestJournalFile);
            }
            finally
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await watcher.StopWatchingAsync(timeout.Token);
                watcher.Dispose();
            }
        }

        [TestMethod]
        public async Task StopWatchingAsync_StopsReaderDiscoveryAndSignalLoops()
        {
            // **Validates: Requirements 2.10, 3.6**
            string currentPath = Path.Combine(
                _tempDirectory, "Journal.20260407160000.01.log");
            WriteJournal(currentPath, Record("2026-04-07T16:00:00Z", "Sol"));

            var receivedSystems = new ConcurrentQueue<string>();
            var watcher = new JournalWatcher(_tempDirectory);
            watcher.MessageReceived += (_, args) =>
            {
                string? system = args.EventArgs.OriginalEvent?["StarSystem"]?.Value<string>();
                if (system != null)
                    receivedSystems.Enqueue(system);
            };

            await watcher.StartWatching();
            Assert.IsTrue(await WaitUntilAsync(
                () => watcher.IsSelectedFilePollingActive &&
                      watcher.IsDirectoryDiscoveryActive &&
                      watcher.IsSignalProcessorActive,
                3000), "All watcher loops must be owned while watching.");

            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                await watcher.StopWatchingAsync(timeout.Token);

            Assert.IsFalse(watcher.IsSelectedFilePollingActive);
            Assert.IsFalse(watcher.IsDirectoryDiscoveryActive);
            Assert.IsFalse(watcher.IsSignalProcessorActive);
            Assert.IsFalse(watcher.IsLive);

            int countAfterStop = receivedSystems.Count;
            File.AppendAllText(
                currentPath,
                Record("2026-04-07T16:00:01Z", "Barnard's Star"),
                Utf8NoBom);
            WriteJournal(
                Path.Combine(_tempDirectory, "Journal.20260407170000.01.log"),
                Record("2026-04-07T17:00:00Z", "Achenar"));
            await Task.Delay(JournalWatcher.UPDATE_INTERVAL_MILLISECONDS + 250);

            Assert.HasCount(countAfterStop, receivedSystems,
                "No selected-file or discovery work may continue after shutdown returns.");
            watcher.Dispose();
        }

        private static void WriteJournal(string path, string record)
        {
            File.WriteAllText(path, record, Utf8NoBom);
        }

        private static string Record(string timestamp, string starSystem)
        {
            return $"{{\"timestamp\":\"{timestamp}\",\"event\":\"FSDJump\"," +
                   $"\"StarSystem\":\"{starSystem}\",\"SystemAddress\":1," +
                   "\"StarPos\":[0,0,0]}\n";
        }

        private static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeoutMilliseconds)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                    return true;

                await Task.Delay(25);
            }

            return condition();
        }
    }
}
