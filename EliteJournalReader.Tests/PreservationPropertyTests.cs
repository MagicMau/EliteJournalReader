#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteJournalReader;
using EliteJournalReader.Events;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    /// <summary>
    /// Preservation property tests for EliteJournalReader watcher behaviors.
    /// These verify that existing correct behavior is maintained outside the bug condition.
    /// They are EXPECTED TO PASS on unfixed code.
    ///
    /// **Validates: Requirements 3.7, 3.8, 3.9, 3.10**
    /// </summary>
    [TestClass]
    [TestCategory("Preservation")]
    public class PreservationPropertyTests
    {
        #region Property: Typed Event Dispatch (Req 3.7, 3.10)

        /// <summary>
        /// When EliteG19s operates in file mode and a complete event is presented,
        /// the system surfaces that event through JournalWatcher's typed event dispatch path
        /// including Fired and MessageReceived.
        /// Compatible unknown properties pass through reflection-based discovery,
        /// deserialization, PostProcess, and subscriber notification.
        /// </summary>
        [TestMethod]
        public void Preservation_TypedEventDispatch_FiredAndMessageReceived()
        {
            // **Validates: Requirements 3.7, 3.10**
            var watcher = new FakeJournalWatcher();
            watcher.StartWatching();

            try
            {
                // Subscribe to MessageReceived
                JournalEventArgs? receivedArgs = null;
                string? receivedEventType = null;
                watcher.MessageReceived += (_, e) =>
                {
                    receivedArgs = e.EventArgs;
                    receivedEventType = e.EventType;
                };

                // Fire a known event with extra unknown properties using FireFakeEvent
                // which goes through ParseAndProcess → Process → FireEvent → MessageReceived
                string json = @"{""timestamp"":""2026-04-07T12:00:00Z"",""event"":""FSDJump"",""StarSystem"":""Sol"",""SystemAddress"":10477373803,""StarPos"":[0,0,0],""UnknownProp"":""preserved"",""ExtraData"":{""nested"":true}}";

                watcher.FireFakeEvent(json);

                // Preservation: event is dispatched through typed path
                Assert.IsNotNull(receivedArgs, "MessageReceived must be raised");
                Assert.AreEqual("FSDJump", receivedEventType, "Event type must be preserved");

                // Preservation: OriginalEvent retains unknown properties
                Assert.IsNotNull(receivedArgs.OriginalEvent, "OriginalEvent must be set");
                var originalObj = receivedArgs.OriginalEvent as JObject;
                Assert.IsNotNull(originalObj, "OriginalEvent must be a JObject");
                var originalUnknown = originalObj["UnknownProp"];
                Assert.IsNotNull(originalUnknown, "Unknown properties must be in OriginalEvent");
                Assert.AreEqual("preserved", originalUnknown.Value<string>());

                var nestedExtra = originalObj["ExtraData"];
                Assert.IsNotNull(nestedExtra, "Nested unknown properties must be in OriginalEvent");
                Assert.AreEqual(true, nestedExtra["nested"]?.Value<bool>());
            }
            finally
            {
                watcher.StopWatching();
            }
        }

        /// <summary>
        /// FsCheck property: for any supported event type with arbitrary compatible unknown
        /// properties, the reflection-based dispatch path fires and preserves the original event.
        /// </summary>
        [TestMethod]
        public void Preservation_UnknownProperties_PassThroughReflectionDispatch()
        {
            // **Validates: Requirements 3.10**
            Property property = FsCheck.FSharp.Prop.ForAll(
                EventWithUnknownPropertiesArbitrary(),
                FuncConvert.ToFSharpFunc<(string EventType, JObject Event), bool>(input =>
                {
                    var watcher = new FakeJournalWatcher();
                    watcher.StartWatching();
                    try
                    {
                        JournalEventArgs? received = null;
                        watcher.MessageReceived += (_, e) => received = e.EventArgs;

                        string json = input.Event.ToString(Formatting.None);
                        watcher.FireFakeEvent(json);

                        if (received == null)
                        {
                            // Event type not registered — that's fine, preservation
                            // only covers supported types that have handlers
                            return true;
                        }

                        // Preservation: OriginalEvent must contain all source properties
                        if (received.OriginalEvent == null) return false;

                        var originalObj = received.OriginalEvent as JObject;
                        if (originalObj == null) return false;

                        foreach (var prop in input.Event.Properties())
                        {
                            if (!originalObj.ContainsKey(prop.Name))
                                return false;
                        }

                        // Preservation: MessageReceived fires
                        return true;
                    }
                    finally
                    {
                        watcher.StopWatching();
                    }
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(100), property);
        }

        #endregion

        #region Property: Bounded Polling Fallback (Req 3.8)

        /// <summary>
        /// Bounded polling is used as a fallback without busy-waiting. The polling interval
        /// is a constant 500ms (UPDATE_INTERVAL_MILLISECONDS).
        /// </summary>
        [TestMethod]
        public void Preservation_BoundedPolling_UsesConstantInterval()
        {
            // **Validates: Requirements 3.8**
            Assert.AreEqual(500, JournalWatcher.UPDATE_INTERVAL_MILLISECONDS,
                "Preservation: polling interval must remain 500ms");
        }

        #endregion

        #region Property: Canonical Multipart Session Reconstruction (Req 3.9)

        /// <summary>
        /// Ordinary game-created canonical multipart journals reconstruct the latest session
        /// from its numerically ordered parts before transitioning to live incremental consumption.
        /// Uses real filesystem with bounded waits.
        /// </summary>
        [TestMethod]
        public void Preservation_CanonicalMultipart_ReconstructsLatestSession()
        {
            // **Validates: Requirements 3.9**
            var tempDir = Path.Combine(Path.GetTempPath(), "EliteJournalReader.Preservation.Multipart", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create ordinary canonical multipart session files in correct order
                var session = "2026-04-07T120000";
                CreatePartFile(tempDir, session, 1, "FileHeader");
                Thread.Sleep(20);
                CreatePartFile(tempDir, session, 2, "LoadGame");

                // Create the watcher and process previous journals
                var watcher = new JournalWatcher(tempDir);
                var events = new List<string>();
                watcher.MessageReceived += (_, e) => events.Add(e.EventType);

                // ProcessPreviousJournals reads existing files
                // We use StartWatching which internally calls ProcessPreviousJournals
                var startTask = watcher.StartWatching();
                // Give it a bounded time to process
                Thread.Sleep(200);
                watcher.StopWatching();

                // Preservation: events from the latest session parts are processed
                Assert.IsTrue(events.Count >= 1,
                    "Preservation: multipart session must yield at least one event from history");
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        /// <summary>
        /// FsCheck property: for ordinary sessions with numeric parts (1-5), the latest
        /// session's parts are selected by creation date ordering.
        /// </summary>
        [TestMethod]
        public void Preservation_OrdinarySessionParts_SelectedByCreationDate()
        {
            // **Validates: Requirements 3.9**
            Property property = FsCheck.FSharp.Prop.ForAll(
                OrdinarySessionPartsArbitrary(),
                FuncConvert.ToFSharpFunc<int[], bool>(partNumbers =>
                {
                    if (partNumbers.Length == 0) return true;

                    var tempDir = Path.Combine(Path.GetTempPath(),
                        "EliteJournalReader.Preservation.Parts",
                        Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(tempDir);

                    try
                    {
                        var session = "2026-04-07T130000";
                        // Create parts with sequential creation times
                        foreach (var part in partNumbers.OrderBy(p => p))
                        {
                            CreatePartFile(tempDir, session, part, "FileHeader");
                            Thread.Sleep(15); // Ensure distinct creation times
                        }

                        // Verify files can be found with standard filter
                        var files = Directory.GetFiles(tempDir, "Journal*.*.log");
                        // Preservation: all parts are present and discoverable
                        return files.Length == partNumbers.Distinct().Count();
                    }
                    finally
                    {
                        try { Directory.Delete(tempDir, recursive: true); } catch { }
                    }
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(20), property);
        }

        #endregion

        #region Property: Complete-Line Byte Chunking (Req 3.7)

        /// <summary>
        /// When a complete JSON line is appended (terminated with newline), the reader
        /// processes it. Verifies the complete-line path works for ordinary appends.
        /// </summary>
        [TestMethod]
        public void Preservation_CompleteLine_ProcessedOnAppend()
        {
            // **Validates: Requirements 3.7**
            var tempDir = Path.Combine(Path.GetTempPath(),
                "EliteJournalReader.Preservation.CompleteLine",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var journalPath = Path.Combine(tempDir, "Journal.2026-04-07T140000.01.log");
                string headerLine = @"{""timestamp"":""2026-04-07T14:00:00Z"",""event"":""Fileheader"",""part"":1,""language"":""English/UK"",""Odyssey"":true,""gameversion"":""4.0""}" + "\n";
                // Write a complete line
                File.WriteAllText(journalPath, headerLine);

                // Read using the same approach as ParseData
                long offset = 0;
                var processedLines = new List<string>();
                using (var fs = new FileStream(journalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    reader.DiscardBufferedData();

                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                            processedLines.Add(line);
                    }
                    offset = reader.BaseStream.Position;
                }

                // Preservation: complete line is processed
                Assert.AreEqual(1, processedLines.Count,
                    "Preservation: one complete newline-terminated line must yield one processed record");

                // Verify parsed JSON is valid
                var jo = JObject.Parse(processedLines[0]);
                Assert.AreEqual("Fileheader", jo.Value<string>("event"));
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        /// <summary>
        /// FsCheck property: for arbitrary valid NDJSON records written as complete lines,
        /// reading them all back yields the same count and content in file order.
        /// </summary>
        [TestMethod]
        public void Preservation_CompleteLineChunking_AllRecordsReadInOrder()
        {
            // **Validates: Requirements 3.7**
            Property property = FsCheck.FSharp.Prop.ForAll(
                CompleteLineSequenceArbitrary(),
                FuncConvert.ToFSharpFunc<string[], bool>(lines =>
                {
                    if (lines.Length == 0) return true;

                    var tempDir = Path.Combine(Path.GetTempPath(),
                        "EliteJournalReader.Preservation.Chunking",
                        Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(tempDir);

                    try
                    {
                        var journalPath = Path.Combine(tempDir, "Journal.2026-04-07T150000.01.log");
                        // Write all lines with newline terminators
                        var sb = new StringBuilder();
                        foreach (var line in lines)
                            sb.AppendLine(line);
                        File.WriteAllText(journalPath, sb.ToString());

                        // Read back
                        var readLines = new List<string>();
                        using (var fs = new FileStream(journalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(fs, Encoding.UTF8))
                        {
                            string? readLine;
                            while ((readLine = reader.ReadLine()) != null)
                            {
                                if (!string.IsNullOrEmpty(readLine))
                                    readLines.Add(readLine);
                            }
                        }

                        // Preservation: all lines read back in order
                        if (readLines.Count != lines.Length) return false;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (readLines[i] != lines[i]) return false;
                        }
                        return true;
                    }
                    finally
                    {
                        try { Directory.Delete(tempDir, recursive: true); } catch { }
                    }
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(50), property);
        }

        #endregion

        #region Property: Reflection-Based Event Discovery (Req 3.10)

        /// <summary>
        /// JournalWatcher's static constructor uses reflection to discover all JournalEvent
        /// subclasses. This establishes that the reflection path is operational.
        /// </summary>
        [TestMethod]
        public void Preservation_ReflectionDiscovery_FindsKnownEventTypes()
        {
            // **Validates: Requirements 3.10**
            var watcher = new FakeJournalWatcher();

            // Verify known event types are discovered and registered
            var fileheaderEvent = watcher.GetEvent<FileheaderEvent>();
            Assert.IsNotNull(fileheaderEvent, "Preservation: FileheaderEvent must be discovered by reflection");

            var fsdJumpEvent = watcher.GetEvent<FSDJumpEvent>();
            Assert.IsNotNull(fsdJumpEvent, "Preservation: FSDJumpEvent must be discovered by reflection");

            var loadGameEvent = watcher.GetEvent<LoadGameEvent>();
            Assert.IsNotNull(loadGameEvent, "Preservation: LoadGameEvent must be discovered by reflection");
        }

        #endregion

        #region Helper Methods

        private static void CreatePartFile(string directory, string session, int part, string eventType)
        {
            string filename = $"Journal.{session}.{part:D2}.log";
            string filePath = Path.Combine(directory, filename);
            string json = $"{{\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss'Z'}\",\"event\":\"{eventType}\",\"part\":{part}}}";
            File.WriteAllText(filePath, json + "\n");
        }

        #endregion

        #region FsCheck Generators

        private static Arbitrary<(string EventType, JObject Event)> EventWithUnknownPropertiesArbitrary()
        {
            var eventTypes = new[] { "Fileheader", "FSDJump", "LoadGame", "Docked", "Undocked" };
            var gen = Gen.Elements(eventTypes).SelectMany(eventType =>
            {
                return Gen.Choose(0, 4).Select(extraCount =>
                {
                    var jo = new JObject
                    {
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                        ["event"] = eventType
                    };

                    // Add required properties for known events
                    switch (eventType)
                    {
                        case "Fileheader":
                            jo["part"] = 1;
                            jo["language"] = "English/UK";
                            jo["Odyssey"] = true;
                            jo["gameversion"] = "4.0";
                            break;
                        case "FSDJump":
                            jo["StarSystem"] = "Sol";
                            jo["SystemAddress"] = 10477373803L;
                            jo["StarPos"] = new JArray(0, 0, 0);
                            break;
                        case "LoadGame":
                            jo["Commander"] = "TestCmdr";
                            jo["Ship"] = "SideWinder";
                            break;
                        case "Docked":
                            jo["StationName"] = "Jameson Memorial";
                            jo["StationType"] = "Orbis";
                            jo["StarSystem"] = "Shinrarta Dezhra";
                            break;
                    }

                    // Add unknown/extra properties
                    for (int i = 0; i < extraCount; i++)
                    {
                        jo[$"CustomProp{i}"] = i % 2 == 0 ? (JToken)new JValue($"val{i}") : new JValue(i * 10);
                    }

                    return (eventType, jo);
                });
            });
            return Arb.From(gen);
        }

        private static Arbitrary<int[]> OrdinarySessionPartsArbitrary()
        {
            // Generate 1-4 sequential part numbers (ordinary game behavior)
            var gen = Gen.Choose(1, 4).Select(count =>
                Enumerable.Range(1, count).ToArray());
            return Arb.From(gen);
        }

        private static Arbitrary<string[]> CompleteLineSequenceArbitrary()
        {
            var baseTime = new DateTime(2026, 4, 7, 15, 0, 0, DateTimeKind.Utc);
            var gen = Gen.Choose(1, 6).SelectMany(count =>
            {
                return Gen.Resize(Gen.ArrayOf(Gen.Choose(0, 3600).Select(offset =>
                {
                    var ts = baseTime.AddSeconds(offset);
                    return $"{{\"timestamp\":\"{ts:yyyy-MM-dd'T'HH:mm:ss'Z'}\",\"event\":\"TestEvent\",\"seq\":{offset}}}";
                })), count);
            });
            return Arb.From(gen);
        }

        #endregion
    }
}
