using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    [TestClass]
    public class JournalWatcherStartupFramingTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(
                Path.GetTempPath(),
                "EliteJournalReader.StartupFraming",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        [TestMethod]
        public void CanonicalStartup_DispatchesCompleteRecordsAndReturnsLastNewlineOffset()
        {
            // **Validates: Requirements 2.10, 3.6**
            string journalPath = Path.Combine(_tempDir, "Journal.20260407170000.01.log");
            byte[] first = Encoding.UTF8.GetBytes(MakeRecord("München"));
            byte[] second = Encoding.UTF8.GetBytes(MakeRecord("核心星"));
            byte[] incomplete = Encoding.UTF8.GetBytes(MakeRecord("unterminated"));
            byte[] contents = Combine(first, new byte[] { (byte)'\r', (byte)'\n' }, second,
                new byte[] { (byte)'\n' }, incomplete);
            File.WriteAllBytes(journalPath, contents);

            var watcher = new StartupProbeJournalWatcher(_tempDir);
            var typedSystems = new List<string>();
            var messageSystems = new List<string>();
            EventHandler<FSDJumpEvent.FSDJumpEventArgs> typedHandler = (sender, args) =>
            {
                if (ReferenceEquals(sender, watcher))
                    typedSystems.Add(args.StarSystem);
            };
            watcher.GetEvent<FSDJumpEvent>().AddHandler(typedHandler);
            watcher.MessageReceived += (_, args) =>
                messageSystems.Add(args.EventArgs.OriginalEvent.Value<string>("StarSystem"));

            try
            {
                using var concurrentWriter = new FileStream(
                    journalPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                concurrentWriter.Seek(0, SeekOrigin.End);

                long committedOffset = watcher.ProcessStartup();
                long expectedOffset = first.LongLength + 2 + second.LongLength + 1;

                Assert.AreEqual(expectedOffset, committedOffset,
                    "Startup must resume immediately after the last terminating newline.");
                CollectionAssert.AreEqual(
                    new[] { "München", "核心星" },
                    typedSystems,
                    "Typed dispatch must include complete records only.");
                CollectionAssert.AreEqual(
                    new[] { "München", "核心星" },
                    messageSystems,
                    "Message dispatch must include complete records only.");
            }
            finally
            {
                watcher.GetEvent<FSDJumpEvent>().RemoveHandler(typedHandler);
                watcher.Dispose();
            }
        }

        [TestMethod]
        public void CanonicalStartup_UnterminatedRecordDoesNotDispatchOrCommit()
        {
            // **Validates: Requirements 2.10, 3.6**
            string journalPath = Path.Combine(_tempDir, "Journal.20260407170000.01.log");
            File.WriteAllText(
                journalPath,
                MakeRecord("not-yet-complete"),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var watcher = new StartupProbeJournalWatcher(_tempDir);
            int dispatchCount = 0;
            EventHandler<FSDJumpEvent.FSDJumpEventArgs> typedHandler = (sender, _) =>
            {
                if (ReferenceEquals(sender, watcher))
                    dispatchCount++;
            };
            watcher.GetEvent<FSDJumpEvent>().AddHandler(typedHandler);

            try
            {
                long committedOffset = watcher.ProcessStartup();

                Assert.AreEqual(0L, committedOffset,
                    "An unterminated startup suffix must remain available for live resume.");
                Assert.AreEqual(0, dispatchCount,
                    "An unterminated JSON object must not be dispatched during startup.");
            }
            finally
            {
                watcher.GetEvent<FSDJumpEvent>().RemoveHandler(typedHandler);
                watcher.Dispose();
            }
        }

        private static string MakeRecord(string starSystem) => new JObject
        {
            ["timestamp"] = "2026-04-07T17:00:00Z",
            ["event"] = "FSDJump",
            ["StarSystem"] = starSystem,
            ["SystemAddress"] = 10477373803L,
            ["StarPos"] = new JArray(0, 0, 0)
        }.ToString(Formatting.None);

        private static byte[] Combine(params byte[][] parts)
        {
            using var stream = new MemoryStream();
            foreach (byte[] part in parts)
                stream.Write(part, 0, part.Length);
            return stream.ToArray();
        }

        private sealed class StartupProbeJournalWatcher : JournalWatcher
        {
            public StartupProbeJournalWatcher(string path) : base(path)
            {
            }

            public long ProcessStartup() => ProcessPreviousJournals();
        }
    }
}
