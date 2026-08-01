#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    /// Observation-first baseline for ordinary reader selection and concurrent consumption.
    /// Mixed compact/ISO identities, startup partial records, and missed notifications are
    /// intentionally excluded because tasks 2, 3, and 5 characterize those bug inputs.
    /// </summary>
    [TestClass]
    [TestCategory("Preservation")]
    public class OrdinaryReaderPreservationTests
    {
        private const ulong SelectionSeed = 0x08A11CE5UL;
        private const ulong FramingSeed = 0x08F24A6EUL;
        private static readonly DateTime BaseUtc =
            new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc);

        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void Property_OrdinarySameFormSelection_PreservesStartupAndLiveDecisions()
        {
            // **Validates: Requirements 3.4, 3.5**
            Property property = FsCheck.FSharp.Prop.ForAll(
                OrdinarySelectionScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<OrdinarySelectionScenario, bool>(ObserveSelection));

            Check.One(PreservationConfig(SelectionSeed, 100), property);
            TestContext.WriteLine(
                $"selection-seed=0x{SelectionSeed:X}; forms=compact-only|ISO-only; " +
                "startup=greatest-session/numeric-parts; live=older:false,higher-part:true; " +
                "partitions=normal|beta; legacy=isolated");
        }
        [TestMethod]
        public void LegacyFallback_SelectsOneLatestFileWithoutCanonicalMixing()
        {
            // **Validates: Requirements 3.4**
            string older = @"C:\Journals\Journal.log";
            string newer = @"C:\Journals\Journal-legacy.log";
            var writes = new Dictionary<string, DateTime>
            {
                [older] = BaseUtc,
                [newer] = BaseUtc.AddMinutes(1),
            };

            var selector = new JournalSessionSelector();
            IReadOnlyList<string> selected = selector.SelectSessionFiles(
                new[] { older, newer }, path => writes[path]);

            CollectionAssert.AreEqual(new[] { newer }, selected.ToArray());
            TestContext.WriteLine(
                "legacy-baseline=latest-write-only; selected=Journal-legacy.log; count=1");
        }

        [TestMethod]
        public void Property_Utf8AppendGrouping_PreservesSharingFramingAndCommittedOffsets()
        {
            // **Validates: Requirements 3.6**
            Property property = FsCheck.FSharp.Prop.ForAll(
                Utf8AppendScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<Utf8AppendScenario, bool>(ObserveFraming));

            Check.One(PreservationConfig(FramingSeed, 60), property);
            TestContext.WriteLine(
                $"framing-seed=0x{FramingSeed:X}; writer-share=Read; reader-share=ReadWrite; " +
                "pre-newline-dispatch=0; post-newline-dispatch=1; " +
                "committed-offset=byte-after-LF; repeated-read-dispatch=0");
        }

        [TestMethod]
        public void FramerReset_DiscardsPendingBytesAndCommitsReplacementFromZero()
        {
            // **Validates: Requirements 3.6**
            string directory = CreateTemporaryDirectory("Reset");
            string path = Path.Combine(directory, "Journal.20260407160000.01.log");
            var framer = new JournalRecordFramer(0);
            string partial = MakeFsdJumpRecord("discarded-🚀");
            string replacement = MakeFsdJumpRecord("replacement-核心星");

            try
            {
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(partial));
                using (var reader = OpenReader(path))
                {
                    Assert.IsEmpty(framer.ReadCompleteRecords(reader));
                }
                Assert.AreEqual(0L, framer.CommittedOffset);
                Assert.IsTrue(framer.HasPendingBytes);

                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(replacement + "\n"));
                framer.Reset();
                string[] observed;
                using (var reader = OpenReader(path))
                {
                    observed = framer.ReadCompleteRecords(reader);
                }

                CollectionAssert.AreEqual(new[] { replacement }, observed);
                Assert.AreEqual(Encoding.UTF8.GetByteCount(replacement + "\n"), framer.CommittedOffset);
                Assert.IsFalse(framer.HasPendingBytes);
                TestContext.WriteLine(
                    $"replacement-baseline=reset-to-zero; old-committed=0; " +
                    $"new-committed={framer.CommittedOffset}; delivered=replacement-only");
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }
        [TestMethod]
        public async Task LiveCurrentFileAppend_PreservesSharingNewlineGateAndTypedExactlyOnceDelivery()
        {
            // **Validates: Requirements 3.6**
            string directory = CreateTemporaryDirectory("Typed");
            string path = Path.Combine(directory, "Journal.20260407170000.01.log");
            File.WriteAllBytes(path, Array.Empty<byte>());

            var typedSystems = new ConcurrentQueue<string>();
            var messageSystems = new ConcurrentQueue<string>();
            var delivered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var watcher = new JournalWatcher(directory);
            watcher.GetEvent<FSDJumpEvent>().Fired += (_, args) =>
            {
                typedSystems.Enqueue(args.StarSystem);
                delivered.TrySetResult(true);
            };
            watcher.MessageReceived += (_, args) =>
            {
                if (!string.Equals(args.EventType, "FSDJump", StringComparison.Ordinal))
                    return;

                string? system = (args.EventArgs.OriginalEvent as JObject)?["StarSystem"]?.Value<string>();
                if (system != null)
                    messageSystems.Enqueue(system);
            };

            try
            {
                await watcher.StartWatching();
                string record = MakeFsdJumpRecord("typed-🚀-核心星");
                byte[] bytes = Encoding.UTF8.GetBytes(record);
                int split = FindInteriorUtf8Split(bytes);

                using (var writer = new FileStream(
                    path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    await writer.WriteAsync(bytes.AsMemory(0, split));
                    await writer.FlushAsync();
                    await Task.Delay(TimeSpan.FromMilliseconds(650));
                    Assert.IsEmpty(typedSystems, "Unterminated bytes must not dispatch typed events.");
                    Assert.IsEmpty(messageSystems, "Unterminated bytes must not dispatch messages.");

                    await writer.WriteAsync(bytes.AsMemory(split));
                    await writer.WriteAsync(new byte[] { (byte)'\n' });
                    await writer.FlushAsync();
                    writer.Flush(flushToDisk: true);
                }

                Task completed = await Task.WhenAny(delivered.Task, Task.Delay(TimeSpan.FromSeconds(4)));
                Assert.AreSame(delivered.Task, completed, "Complete flushed record was not delivered within the bound.");
                await Task.Delay(TimeSpan.FromMilliseconds(650));

                CollectionAssert.AreEqual(new[] { "typed-🚀-核心星" }, typedSystems.ToArray());
                CollectionAssert.AreEqual(new[] { "typed-🚀-核心星" }, messageSystems.ToArray());
                TestContext.WriteLine(
                    "typed-baseline=current-file append; writer-share=Read; pre-LF=0; " +
                    "typed=1; message=1; value=typed-🚀-核心星");
            }
            finally
            {
                using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await watcher.StopWatchingAsync(stopTimeout.Token);
                watcher.Dispose();
                DeleteTemporaryDirectory(directory);
            }
        }

        private static bool ObserveSelection(OrdinarySelectionScenario scenario)
        {
            var selector = new JournalSessionSelector();
            IReadOnlyList<string> selected = selector.SelectSessionFiles(
                scenario.InputFiles,
                path => path == scenario.OlderFile ? DateTime.MaxValue : DateTime.MinValue);

            bool startupMatches = selected.SequenceEqual(
                scenario.ExpectedStartupFiles,
                StringComparer.OrdinalIgnoreCase);
            bool olderNewlyCreatedRejected = !selector.ShouldSwitchToFile(
                scenario.CurrentSessionKey,
                scenario.CurrentPart,
                scenario.OlderFile);
            bool samePartRejected = !selector.ShouldSwitchToFile(
                scenario.CurrentSessionKey,
                scenario.CurrentPart,
                scenario.SamePartFile);
            bool higherCurrentPartAccepted = selector.ShouldSwitchToFile(
                scenario.CurrentSessionKey,
                scenario.CurrentPart,
                scenario.HigherPartFile);
            bool newerSessionAccepted = selector.ShouldSwitchToFile(
                scenario.CurrentSessionKey,
                scenario.CurrentPart,
                scenario.NewerFile);

            return startupMatches &&
                olderNewlyCreatedRejected &&
                samePartRejected &&
                higherCurrentPartAccepted &&
                newerSessionAccepted;
        }

        private static bool ObserveFraming(Utf8AppendScenario scenario)
        {
            string directory = CreateTemporaryDirectory("Framing");
            string path = Path.Combine(directory, "Journal.20260407150000.01.log");
            var framer = new JournalRecordFramer(0);
            byte[] recordBytes = scenario.RecordBytes;
            int written = 0;

            try
            {
                File.WriteAllBytes(path, Array.Empty<byte>());
                using (var writer = new FileStream(
                    path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    foreach (int chunkLength in scenario.ChunkLengths)
                    {
                        writer.Write(recordBytes, written, chunkLength);
                        writer.Flush();
                        written += chunkLength;

                        using var reader = OpenReader(path);
                        if (framer.ReadCompleteRecords(reader).Length != 0)
                            return false;
                        if (framer.CommittedOffset != 0)
                            return false;
                    }

                    if (written != recordBytes.Length)
                        return false;

                    writer.WriteByte((byte)'\n');
                    writer.Flush(flushToDisk: true);

                    using var completedReader = OpenReader(path);
                    string[] completed = framer.ReadCompleteRecords(completedReader);
                    if (!completed.SequenceEqual(new[] { scenario.Record }, StringComparer.Ordinal))
                        return false;
                }

                long expectedOffset = recordBytes.LongLength + 1;
                if (framer.CommittedOffset != expectedOffset || framer.HasPendingBytes)
                    return false;

                using var repeatedReader = OpenReader(path);
                return framer.ReadCompleteRecords(repeatedReader).Length == 0 &&
                    framer.CommittedOffset == expectedOffset;
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static Config PreservationConfig(ulong seed, int maxTests)
        {
            var replay = new Replay(new Rnd(seed), null);
            return Config.QuickThrowOnFailure
                .WithMaxTest(maxTests)
                .WithReplay(FSharpOption<Replay>.Some(replay));
        }

        private static Arbitrary<OrdinarySelectionScenario> OrdinarySelectionScenarioArbitrary()
        {
            Gen<OrdinarySelectionScenario> generator = Gen.Elements(false, true)
                .SelectMany(useIso => Gen.Elements(false, true)
                    .SelectMany(isBeta => Gen.Choose(1, 24)
                        .SelectMany(sessionGapHours => Gen.Choose(1, 4)
                            .Select(partCount => new OrdinarySelectionScenario(
                                useIso, isBeta, sessionGapHours, partCount)))));

            return Arb.From(generator, ShrinkOrdinarySelectionScenario);
        }

        private static IEnumerable<OrdinarySelectionScenario> ShrinkOrdinarySelectionScenario(
            OrdinarySelectionScenario scenario)
        {
            if (scenario.PartCount != 1)
                yield return new OrdinarySelectionScenario(
                    scenario.UseIso, scenario.IsBeta, scenario.SessionGapHours, 1);
            if (scenario.SessionGapHours != 1)
                yield return new OrdinarySelectionScenario(
                    scenario.UseIso, scenario.IsBeta, 1, scenario.PartCount);
        }

        private static Arbitrary<Utf8AppendScenario> Utf8AppendScenarioArbitrary()
        {
            string[] values = { "Sol", "München", "核心星", "🚀-α", "𐍈-é" };
            Gen<Utf8AppendScenario> generator = Gen.Elements(values)
                .SelectMany(value => Gen.Choose(0, 12).SelectMany(padding =>
                {
                    var probe = new Utf8AppendScenario(value, padding, 1, 1);
                    return Gen.Choose(1, probe.RecordBytes.Length - 1)
                        .SelectMany(splitPoint => Gen.Choose(1, 24)
                            .Select(groupSize => new Utf8AppendScenario(
                                value, padding, splitPoint, groupSize)));
                }));

            return Arb.From(generator, ShrinkUtf8AppendScenario);
        }

        private static IEnumerable<Utf8AppendScenario> ShrinkUtf8AppendScenario(
            Utf8AppendScenario scenario)
        {
            if (scenario.SplitPoint != 1 || scenario.GroupSize != 1)
                yield return new Utf8AppendScenario(scenario.Utf8Value, scenario.Padding, 1, 1);
            if (scenario.Padding != 0)
                yield return new Utf8AppendScenario(scenario.Utf8Value, 0, 1, scenario.GroupSize);
            if (!string.Equals(scenario.Utf8Value, "Sol", StringComparison.Ordinal))
                yield return new Utf8AppendScenario("Sol", scenario.Padding, 1, scenario.GroupSize);
        }

        private static FileStream OpenReader(string path) =>
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        private static string CreateTemporaryDirectory(string suffix)
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "EliteJournalReader.OrdinaryPreservation",
                suffix,
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void DeleteTemporaryDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (IOException)
            {
                // Delayed filesystem callbacks can briefly retain directory handles.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup remains best effort when Windows still owns a handle.
            }
        }

        private static string MakeFsdJumpRecord(string starSystem) =>
            new JObject
            {
                ["timestamp"] = "2026-04-07T17:00:00Z",
                ["event"] = "FSDJump",
                ["StarSystem"] = starSystem,
                ["SystemAddress"] = 10477373803L,
                ["StarPos"] = new JArray(0, 0, 0)
            }.ToString(Formatting.None);

        private static int FindInteriorUtf8Split(byte[] bytes)
        {
            for (int index = 1; index < bytes.Length; index++)
            {
                if ((bytes[index] & 0xC0) == 0x80)
                    return index;
            }

            return bytes.Length / 2;
        }

        private sealed class OrdinarySelectionScenario
        {
            private static readonly int[] CandidateParts = { 1, 2, 10, 21 };

            public OrdinarySelectionScenario(
                bool useIso,
                bool isBeta,
                int sessionGapHours,
                int partCount)
            {
                UseIso = useIso;
                IsBeta = isBeta;
                SessionGapHours = sessionGapHours;
                PartCount = partCount;

                DateTime olderUtc = BaseUtc;
                DateTime currentUtc = BaseUtc.AddHours(sessionGapHours);
                DateTime newerUtc = currentUtc.AddHours(sessionGapHours);
                string olderIdentity = FormatSession(olderUtc, useIso);
                string currentIdentity = FormatSession(currentUtc, useIso);
                string newerIdentity = FormatSession(newerUtc, useIso);
                string journalPrefix = isBeta ? "JournalBeta" : "Journal";
                string root = @"C:\Journals";

                OlderFile = Path.Combine(root, $"{journalPrefix}.{olderIdentity}.99.log");
                int[] parts = CandidateParts.Take(partCount).ToArray();
                ExpectedStartupFiles = parts
                    .Select(part => Path.Combine(
                        root, $"{journalPrefix}.{currentIdentity}.{part:D2}.log"))
                    .ToArray();

                InputFiles = ExpectedStartupFiles
                    .Reverse()
                    .Concat(new[] { OlderFile, Path.Combine(root, "Journal-legacy.log") })
                    .ToArray();
                CurrentSessionKey = (isBeta ? "Beta:" : "") + currentIdentity;
                CurrentPart = parts.Max();
                SamePartFile = Path.Combine(
                    root, $"{journalPrefix}.{currentIdentity}.{CurrentPart:D2}.log");
                HigherPartFile = Path.Combine(
                    root, $"{journalPrefix}.{currentIdentity}.{CurrentPart + 7:D2}.log");
                NewerFile = Path.Combine(
                    root, $"{journalPrefix}.{newerIdentity}.01.log");
            }

            public bool UseIso { get; }
            public bool IsBeta { get; }
            public int SessionGapHours { get; }
            public int PartCount { get; }
            public string[] InputFiles { get; }
            public string[] ExpectedStartupFiles { get; }
            public string OlderFile { get; }
            public string CurrentSessionKey { get; }
            public int CurrentPart { get; }
            public string SamePartFile { get; }
            public string HigherPartFile { get; }
            public string NewerFile { get; }

            private static string FormatSession(DateTime value, bool useIso) =>
                value.ToString(
                    useIso ? "yyyy-MM-dd'T'HHmmss" : "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture);

            public override string ToString() =>
                $"Seed=0x{SelectionSeed:X8}; Form={(UseIso ? "ISO" : "compact")}; " +
                $"Partition={(IsBeta ? "beta" : "normal")}; GapHours={SessionGapHours}; " +
                $"Parts=[{string.Join(",", ExpectedStartupFiles.Select(Path.GetFileName))}]";
        }

        private sealed class Utf8AppendScenario
        {
            public Utf8AppendScenario(
                string utf8Value,
                int padding,
                int splitPoint,
                int groupSize)
            {
                Utf8Value = utf8Value;
                Padding = padding;
                SplitPoint = splitPoint;
                GroupSize = groupSize;
            }

            public string Utf8Value { get; }
            public int Padding { get; }
            public int SplitPoint { get; }
            public int GroupSize { get; }
            public string Record => MakeFsdJumpRecord(
                $"prefix-{Utf8Value}-{new string('x', Padding)}-suffix");
            public byte[] RecordBytes => Encoding.UTF8.GetBytes(Record);

            public int[] ChunkLengths
            {
                get
                {
                    int total = RecordBytes.Length;
                    int first = Math.Clamp(SplitPoint, 1, total - 1);
                    var chunks = new List<int> { first };
                    int remaining = total - first;
                    while (remaining > 0)
                    {
                        int next = Math.Min(GroupSize, remaining);
                        chunks.Add(next);
                        remaining -= next;
                    }
                    return chunks.ToArray();
                }
            }

            public override string ToString() =>
                $"Seed=0x{FramingSeed:X8}; Utf8={Utf8Value}; Padding={Padding}; " +
                $"Split={SplitPoint}; GroupSize={GroupSize}; " +
                $"Bytes={RecordBytes.Length}; Chunks=[{string.Join(",", ChunkLengths)}]";
        }
    }
}
