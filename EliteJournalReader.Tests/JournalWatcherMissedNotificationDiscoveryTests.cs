#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    [TestClass]
    public sealed class JournalWatcherMissedNotificationDiscoveryTests
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly TimeSpan DiscoveryTimeout = TimeSpan.FromMilliseconds(
            JournalWatcher.DIRECTORY_DISCOVERY_INTERVAL_MILLISECONDS + 3000);

        [TestMethod]
        public async Task MissedNotification_DiscoversNewerSession_RejectsOlderAndSamePart()
        {
            // **Validates: Requirements 2.9, 2.10, 3.4, 3.5, 3.6**
            await VerifyMissedNotificationDiscoveryAsync(new DiscoveryScenario(
                "Journal.20260407140000.02.log",
                "Journal.2026-04-07T130000.99.log",
                "Journal.2026-04-07T140000.02.log",
                "Journal.20260407150000.01.log",
                "newer-session"));
        }

        [TestMethod]
        public async Task MissedNotification_DiscoversHigherEquivalentSessionPart_RejectsOlderAndSamePart()
        {
            // **Validates: Requirements 2.9, 2.10, 3.4, 3.5, 3.6**
            await VerifyMissedNotificationDiscoveryAsync(new DiscoveryScenario(
                "Journal.20260407140000.02.log",
                "Journal.2026-04-07T130000.99.log",
                "Journal.2026-04-07T140000.02.log",
                "Journal.2026-04-07T140000.03.log",
                "higher-current-session-part"));
        }

        private static async Task VerifyMissedNotificationDiscoveryAsync(DiscoveryScenario scenario)
        {
            string directory = Path.Combine(
                Path.GetTempPath(), "EliteJournalReader.MissedNotification", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            var received = new ConcurrentQueue<string>();
            var selections = new ConcurrentQueue<string>();
            var errors = new ConcurrentQueue<Exception>();
            var watcher = new JournalWatcher(directory);
            NewJournalFileEvent selectionEvent = watcher.GetEvent<NewJournalFileEvent>();
            EventHandler<NewJournalFileEvent.NewJournalFileEventArgs> selectionHandler = (sender, args) =>
            {
                if (ReferenceEquals(sender, watcher))
                    selections.Enqueue(args.Filename);
            };

            watcher.MessageReceived += (_, args) =>
            {
                string? identity = args.EventArgs.OriginalEvent?["DiscoveryTestId"]?.Value<string>();
                if (identity != null)
                    received.Enqueue(identity);
            };
            watcher.Error += (_, args) => errors.Enqueue(args.GetException());
            selectionEvent.AddHandler(selectionHandler);

            try
            {
                WriteCompleteRecord(
                    Path.Combine(directory, scenario.CurrentFile),
                    Record("current", "2026-04-07T14:00:00Z"));

                await watcher.StartWatching();
                Assert.IsTrue(await WaitUntilAsync(() => received.Count(id => id == "current") == 1,
                    TimeSpan.FromSeconds(3)), "Startup framing did not dispatch the current complete record.");
                Assert.AreEqual(scenario.CurrentFile, watcher.LatestJournalFile,
                    "The scenario must start on the intended current journal.");
                Assert.IsTrue(watcher.IsDirectoryDiscoveryActive,
                    "Periodic directory discovery must remain active before notifications are suppressed.");

                // Smallest seam: disable only FileSystemWatcher callbacks. The selected-file reader,
                // signal processor, and watcher-lifetime periodic discovery remain production code.
                watcher.EnableRaisingEvents = false;

                using FileStream olderWriter = CreateOpenWriter(
                    Path.Combine(directory, scenario.OlderFile),
                    Record("older", "2026-04-07T13:00:00Z"));
                using FileStream samePartWriter = CreateOpenWriter(
                    Path.Combine(directory, scenario.SamePartFile),
                    Record("same-part", "2026-04-07T14:00:00Z"));
                using FileStream eligibleWriter = CreateOpenWriter(
                    Path.Combine(directory, scenario.EligibleFile),
                    Record(scenario.EligibleIdentity, "2026-04-07T15:00:00Z"));

                AssertCompleteRecordVisibleWhileWriterOpen(eligibleWriter, scenario);

                bool selected = await WaitUntilAsync(
                    () => string.Equals(watcher.LatestJournalFile, scenario.EligibleFile,
                        StringComparison.OrdinalIgnoreCase),
                    DiscoveryTimeout);
                if (!selected)
                {
                    string selectedFile = watcher.LatestJournalFile ?? "<null>";
                    bool distractorWon = string.Equals(selectedFile, scenario.OlderFile,
                            StringComparison.OrdinalIgnoreCase)
                        || string.Equals(selectedFile, scenario.SamePartFile,
                            StringComparison.OrdinalIgnoreCase);
                    Assert.Fail(distractorWon
                        ? $"Selection failure: ineligible file '{selectedFile}' displaced current '{scenario.CurrentFile}'."
                        : $"Discovery failure: periodic scan did not select eligible '{scenario.EligibleFile}' " +
                          $"within {DiscoveryTimeout}. Current='{selectedFile}', errors={DescribeErrors(errors)}.");
                }

                Assert.IsTrue(await WaitUntilAsync(
                    () => received.Count(id => id == scenario.EligibleIdentity) == 1,
                    TimeSpan.FromSeconds(2)),
                    $"Framing/dispatch failure after selecting '{scenario.EligibleFile}'. " +
                    $"Received=[{string.Join(",", received)}], errors={DescribeErrors(errors)}.");

                await Task.Delay(JournalWatcher.UPDATE_INTERVAL_MILLISECONDS + 250);
                Assert.AreEqual(1, received.Count(id => id == scenario.EligibleIdentity),
                    "The complete eligible record must dispatch exactly once.");
                Assert.AreEqual(0, received.Count(id => id == "older"),
                    "A newly created older session must never dispatch.");
                Assert.AreEqual(0, received.Count(id => id == "same-part"),
                    "An equivalent same-part file must never dispatch.");
                CollectionAssert.AreEqual(new[] { scenario.EligibleFile }, selections.ToArray(),
                    "Only the eligible newer session or higher current-session part may be selected.");
                Assert.IsEmpty(errors, $"Watcher errors: {DescribeErrors(errors)}");
            }
            finally
            {
                selectionEvent.RemoveHandler(selectionHandler);
                using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await watcher.StopWatchingAsync(stopTimeout.Token);
                watcher.Dispose();
                Directory.Delete(directory, recursive: true);
            }
        }

        private static FileStream CreateOpenWriter(string path, string record)
        {
            var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            byte[] bytes = Utf8NoBom.GetBytes(record);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
            return stream;
        }

        private static void WriteCompleteRecord(string path, string record)
        {
            File.WriteAllText(path, record, Utf8NoBom);
        }

        private static void AssertCompleteRecordVisibleWhileWriterOpen(
            FileStream writer, DiscoveryScenario scenario)
        {
            using var reader = new FileStream(
                writer.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var buffer = new MemoryStream();
            reader.CopyTo(buffer);
            byte[] visibleBytes = buffer.ToArray();

            Assert.AreEqual(1, visibleBytes.Count(value => value == (byte)'\n'),
                $"Flush/framing input failure for '{scenario.EligibleFile}'.");
            Assert.AreEqual(writer.Length, visibleBytes.LongLength,
                $"Compatible opening did not expose all flushed bytes for '{scenario.EligibleFile}'.");
        }

        private static string Record(string identity, string timestamp)
        {
            return new JObject
            {
                ["timestamp"] = timestamp,
                ["event"] = "Music",
                ["MusicTrack"] = identity,
                ["DiscoveryTestId"] = identity
            }.ToString(Formatting.None) + "\n";
        }

        private static async Task<bool> WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (predicate())
                    return true;

                await Task.Delay(25);
            }

            return predicate();
        }

        private static string DescribeErrors(ConcurrentQueue<Exception> errors)
        {
            return errors.IsEmpty ? "none" : string.Join(" | ", errors.Select(error => error.Message));
        }

        private sealed record DiscoveryScenario(
            string CurrentFile,
            string OlderFile,
            string SamePartFile,
            string EligibleFile,
            string EligibleIdentity);
    }
}
