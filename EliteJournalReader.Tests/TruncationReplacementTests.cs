#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteJournalReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    /// <summary>
    /// Tests for file truncation and in-place replacement detection in the JournalWatcher reader loop.
    /// Validates that:
    /// - Truncation resets all framing state and restarts at byte zero
    /// - Identity change resets state and restarts at byte zero
    /// - Old partial bytes never combine with replacement content
    /// - Ordinary append-only files retain committed offsets and exactly-once dispatch
    ///
    /// **Validates: Requirements 2.13, 2.17, 3.7**
    /// </summary>
    [TestClass]
    [TestCategory("Preservation")]
    public class TruncationReplacementTests
    {
        private string _tempDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),
                "EliteJournalReader.TruncReplace",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }

        #region Truncation Detection

        /// <summary>
        /// When a watched file is truncated (length shrinks below the read offset),
        /// the reader resets all framing state and restarts consumption from byte zero.
        /// The replacement records dispatch exactly once.
        /// </summary>
        [TestMethod]
        public async Task Truncation_ResetsFramerAndRestartsAtByteZero()
        {
            // **Validates: Requirements 2.13, 2.17**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T160000.01.log");

            // Write initial content (two complete records)
            var record1 = MakeRecord("2026-04-07T16:00:00Z", "FSDJump", ("StarSystem", "Sol"));
            var record2 = MakeRecord("2026-04-07T16:00:01Z", "FSDJump", ("StarSystem", "Alpha Centauri"));
            File.WriteAllText(journalPath, record1 + "\n" + record2 + "\n");

            var receivedEvents = new List<string>();
            var watcher = new JournalWatcher(_tempDir);
            watcher.MessageReceived += (_, e) =>
            {
                var obj = e.EventArgs.OriginalEvent as JObject;
                receivedEvents.Add(obj?["StarSystem"]?.Value<string>() ?? "");
            };

            await watcher.StartWatching();

            // Wait for initial records to be processed
            await WaitForCondition(() => receivedEvents.Count >= 2, timeout: 3000);
            Assert.AreEqual(2, receivedEvents.Count, "Should process both initial records");
            Assert.AreEqual("Sol", receivedEvents[0]);
            Assert.AreEqual("Alpha Centauri", receivedEvents[1]);

            // Truncate and write new content (replacement)
            var replacement = MakeRecord("2026-04-07T16:01:00Z", "FSDJump", ("StarSystem", "Betelgeuse"));
            File.WriteAllText(journalPath, replacement + "\n");

            // Wait for the replacement record to be processed
            await WaitForCondition(() => receivedEvents.Count >= 3, timeout: 5000);

            // The replacement record should be dispatched exactly once
            Assert.AreEqual("Betelgeuse", receivedEvents[2],
                "Replacement record must dispatch from byte zero with correct content");

            await watcher.StopWatchingAsync();
        }

        /// <summary>
        /// When a file is truncated to zero bytes and then new content is written,
        /// the reader detects the shrink and starts from byte zero when new data arrives.
        /// </summary>
        [TestMethod]
        public async Task Truncation_ToZeroBytes_RestartsOnNewContent()
        {
            // **Validates: Requirements 2.13**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T170000.01.log");

            // Write initial content
            var record1 = MakeRecord("2026-04-07T17:00:00Z", "Docked", ("StationName", "Jameson Memorial"));
            File.WriteAllText(journalPath, record1 + "\n");

            var receivedEvents = new List<string>();
            var watcher = new JournalWatcher(_tempDir);
            watcher.MessageReceived += (_, e) => receivedEvents.Add(e.EventType);

            await watcher.StartWatching();
            await WaitForCondition(() => receivedEvents.Count >= 1, timeout: 3000);

            // Truncate to zero
            using (var fs = new FileStream(journalPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite))
            {
                // File is now empty
            }

            // Write new content after truncation
            await Task.Delay(100);
            var record2 = MakeRecord("2026-04-07T17:01:00Z", "Undocked", ("StationName", "Jameson Memorial"));
            File.AppendAllText(journalPath, record2 + "\n");

            // Wait for the new record
            await WaitForCondition(() => receivedEvents.Count >= 2, timeout: 5000);
            Assert.AreEqual("Undocked", receivedEvents[1],
                "After truncation to zero, new record must be processed from byte zero");

            await watcher.StopWatchingAsync();
        }

        #endregion

        #region Partial Bytes Never Combine with Replacement

        /// <summary>
        /// When a file has partial (unterminated) bytes pending and is then truncated/replaced,
        /// the old partial bytes must never combine with the replacement content.
        /// </summary>
        [TestMethod]
        public async Task Truncation_PartialBytesNeverCombineWithReplacement()
        {
            // **Validates: Requirements 2.13, 2.17**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T180000.01.log");

            // Write a complete record followed by partial bytes (no newline)
            var record1 = MakeRecord("2026-04-07T18:00:00Z", "FSDJump", ("StarSystem", "Sol"));
            string partialData = "{\"timestamp\":\"2026-04-07T18:00:01Z\",\"event\":\"FSDJump\",\"StarSystem\":\"PARTIAL_NEVER_COMBINE";
            File.WriteAllText(journalPath, record1 + "\n" + partialData);

            var receivedSystems = new List<string>();
            var watcher = new JournalWatcher(_tempDir);
            watcher.MessageReceived += (_, e) =>
            {
                var obj = e.EventArgs.OriginalEvent as JObject;
                var sys = obj?["StarSystem"]?.Value<string>();
                if (sys != null) receivedSystems.Add(sys);
            };

            await watcher.StartWatching();
            await WaitForCondition(() => receivedSystems.Count >= 1, timeout: 3000);
            Assert.AreEqual("Sol", receivedSystems[0]);

            // Give time for the partial bytes to be seen by the framer
            await Task.Delay(600);

            // Now truncate and write completely different content
            var replacement = MakeRecord("2026-04-07T18:02:00Z", "FSDJump", ("StarSystem", "NewContent"));
            File.WriteAllText(journalPath, replacement + "\n");

            await WaitForCondition(() => receivedSystems.Count >= 2, timeout: 5000);

            // The received events must NOT contain any combined garbage
            // "PARTIAL_NEVER_COMBINE" should never appear in dispatched events
            Assert.IsFalse(receivedSystems.Any(s => s.Contains("PARTIAL_NEVER_COMBINE")),
                "Old partial bytes must never combine with replacement content");
            Assert.AreEqual("NewContent", receivedSystems[1],
                "Replacement record must dispatch correctly from byte zero");

            await watcher.StopWatchingAsync();
        }

        #endregion

        #region Preservation: Ordinary Append-Only Files

        /// <summary>
        /// Ordinary append-only files retain committed offsets and dispatch each record exactly once.
        /// This is the preservation property — normal operation must not be affected by the
        /// truncation detection logic.
        /// </summary>
        [TestMethod]
        public async Task Preservation_AppendOnly_RetainsOffsetsAndExactlyOnceDispatch()
        {
            // **Validates: Requirements 3.7**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T190000.01.log");

            // Start with one record
            var record1 = MakeRecord("2026-04-07T19:00:00Z", "FSDJump", ("StarSystem", "Sol"));
            File.WriteAllText(journalPath, record1 + "\n");

            var receivedSystems = new List<string>();
            var watcher = new JournalWatcher(_tempDir);
            watcher.MessageReceived += (_, e) =>
            {
                var obj = e.EventArgs.OriginalEvent as JObject;
                var sys = obj?["StarSystem"]?.Value<string>();
                if (sys != null) receivedSystems.Add(sys);
            };

            await watcher.StartWatching();
            await WaitForCondition(() => receivedSystems.Count >= 1, timeout: 3000);

            // Append additional records — file only grows, never shrinks
            var record2 = MakeRecord("2026-04-07T19:00:01Z", "FSDJump", ("StarSystem", "AlphaCentauri"));
            File.AppendAllText(journalPath, record2 + "\n");
            await WaitForCondition(() => receivedSystems.Count >= 2, timeout: 3000);

            var record3 = MakeRecord("2026-04-07T19:00:02Z", "FSDJump", ("StarSystem", "Barnard"));
            File.AppendAllText(journalPath, record3 + "\n");
            await WaitForCondition(() => receivedSystems.Count >= 3, timeout: 3000);

            // Preservation: each record dispatched exactly once, in order
            Assert.AreEqual(3, receivedSystems.Count, "Each appended record dispatches exactly once");
            Assert.AreEqual("Sol", receivedSystems[0]);
            Assert.AreEqual("AlphaCentauri", receivedSystems[1]);
            Assert.AreEqual("Barnard", receivedSystems[2]);

            // No duplicates
            Assert.AreEqual(receivedSystems.Count, receivedSystems.Distinct().Count(),
                "No duplicate records should be dispatched for append-only files");

            await watcher.StopWatchingAsync();
        }

        /// <summary>
        /// Multiple appends to an append-only file never trigger a reset —
        /// the committed offset only advances forward.
        /// </summary>
        [TestMethod]
        public async Task Preservation_AppendOnly_NeverResetsOnGrowth()
        {
            // **Validates: Requirements 3.7**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T200000.01.log");

            // Write initial records
            var sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.AppendLine(MakeRecord($"2026-04-07T20:00:{i:D2}Z", "FSDJump", ("StarSystem", $"System{i}")));
            }
            File.WriteAllText(journalPath, sb.ToString());

            var receivedSystems = new List<string>();
            var watcher = new JournalWatcher(_tempDir);
            watcher.MessageReceived += (_, e) =>
            {
                var obj = e.EventArgs.OriginalEvent as JObject;
                var sys = obj?["StarSystem"]?.Value<string>();
                if (sys != null) receivedSystems.Add(sys);
            };

            await watcher.StartWatching();
            await WaitForCondition(() => receivedSystems.Count >= 5, timeout: 3000);

            // Append more
            for (int i = 5; i < 8; i++)
            {
                File.AppendAllText(journalPath,
                    MakeRecord($"2026-04-07T20:00:{i:D2}Z", "FSDJump", ("StarSystem", $"System{i}")) + "\n");
                await Task.Delay(100);
            }

            await WaitForCondition(() => receivedSystems.Count >= 8, timeout: 5000);

            // All 8 systems received in order, no duplicates from reset
            Assert.AreEqual(8, receivedSystems.Count, "All records dispatched exactly once without reset");
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual($"System{i}", receivedSystems[i], $"Record {i} in correct order");
            }

            await watcher.StopWatchingAsync();
        }

        #endregion

        #region Identity Provider Injection

        /// <summary>
        /// Verifies that the file identity provider is injectable for testing.
        /// A custom provider can simulate identity changes without actual file replacement.
        /// </summary>
        [TestMethod]
        public async Task Injectable_IdentityProvider_DetectsSimulatedReplacement()
        {
            // **Validates: Requirements 2.13, 2.17**
            var journalPath = Path.Combine(_tempDir, "Journal.2026-04-07T210000.01.log");

            // Write initial content
            var record1 = MakeRecord("2026-04-07T21:00:00Z", "FSDJump", ("StarSystem", "Original"));
            File.WriteAllText(journalPath, record1 + "\n");

            var fakeProvider = new FakeFileIdentityProvider();
            fakeProvider.SetIdentity(new FileIdentity(1, 100));

            var receivedSystems = new List<string>();
            var watcher = new JournalWatcher(_tempDir, fakeProvider);
            watcher.MessageReceived += (_, e) =>
            {
                var obj = e.EventArgs.OriginalEvent as JObject;
                var sys = obj?["StarSystem"]?.Value<string>();
                if (sys != null) receivedSystems.Add(sys);
            };

            await watcher.StartWatching();
            await WaitForCondition(() => receivedSystems.Count >= 1, timeout: 3000);
            Assert.AreEqual("Original", receivedSystems[0]);

            // Wait for at least one reader loop iteration to capture the current identity
            await Task.Delay(700);

            // Simulate identity change (as if the file was replaced at the OS level)
            // and rewrite file content. The identity change triggers a full reset.
            fakeProvider.SetIdentity(new FileIdentity(1, 200));

            // Rewrite the file with replacement content
            var record2 = MakeRecord("2026-04-07T21:01:00Z", "FSDJump", ("StarSystem", "Replacement"));
            File.WriteAllText(journalPath, record2 + "\n");

            // Wait for identity-change-triggered reset and re-read
            await WaitForCondition(() => receivedSystems.Contains("Replacement"), timeout: 5000);

            // The replacement content should be dispatched
            Assert.IsTrue(receivedSystems.Contains("Replacement"),
                "Identity change must trigger reset and dispatch replacement content");

            await watcher.StopWatchingAsync();
        }

        #endregion

        #region Helper Methods

        private static string MakeRecord(string timestamp, string eventType, params (string Key, string Value)[] props)
        {
            var jo = new JObject
            {
                ["timestamp"] = timestamp,
                ["event"] = eventType
            };
            foreach (var (key, value) in props)
            {
                jo[key] = value;
            }
            // Add required properties for known events
            switch (eventType)
            {
                case "FSDJump":
                    if (!jo.ContainsKey("SystemAddress")) jo["SystemAddress"] = 10477373803L;
                    if (!jo.ContainsKey("StarPos")) jo["StarPos"] = new JArray(0, 0, 0);
                    break;
                case "Docked":
                    if (!jo.ContainsKey("StationType")) jo["StationType"] = "Orbis";
                    if (!jo.ContainsKey("StarSystem")) jo["StarSystem"] = "Sol";
                    break;
            }
            return jo.ToString(Formatting.None);
        }

        private static async Task WaitForCondition(Func<bool> condition, int timeout)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeout);
            while (!condition() && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }
        }

        #endregion
    }

    /// <summary>
    /// Fake file identity provider for testing truncation/replacement detection.
    /// Allows tests to simulate identity changes without actual file system operations.
    /// </summary>
    internal sealed class FakeFileIdentityProvider : IFileIdentityProvider
    {
        private FileIdentity? _identity;

        public void SetIdentity(FileIdentity? identity) => _identity = identity;

        public FileIdentity? GetIdentity(FileStream stream) => _identity;
        public FileIdentity? GetIdentity(string filePath) => _identity;
    }
}
