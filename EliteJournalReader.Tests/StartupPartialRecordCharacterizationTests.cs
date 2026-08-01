#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteJournalReader;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    /// <summary>
    /// Unfixed-code characterization of the startup-to-live newline framing boundary.
    /// Production watcher and framer behavior is intentionally left unchanged.
    /// </summary>
    [TestClass]
    [TestCategory("BugConditionExploration")]
    public class StartupPartialRecordCharacterizationTests
    {
        private const ulong StartupPartialSeed = 0x51A27EEDUL;
        private static readonly TimeSpan DispatchTimeout = TimeSpan.FromMilliseconds(1800);
        private static readonly TimeSpan DuplicateObservationWindow = TimeSpan.FromMilliseconds(650);

        [TestMethod]
        public void Property_StartupPartialRecord_CommitsAndDispatchesOnlyAfterNewline()
        {
            // **Validates: Requirements 1.10**
            Property property = FsCheck.FSharp.Prop.ForAll(
                StartupPartialScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<StartupPartialScenario, bool>(scenario =>
                    ObserveBoundaryAsync(scenario).GetAwaiter().GetResult()));

            var replay = new Replay(new Rnd(StartupPartialSeed), null);
            Config config = Config.QuickThrowOnFailure
                .WithMaxTest(12)
                .WithReplay(FSharpOption<Replay>.Some(replay));

            Check.One(config, property);
        }

        private static async Task<bool> ObserveBoundaryAsync(StartupPartialScenario scenario)
        {
            string tempDir = Path.Combine(
                Path.GetTempPath(),
                "EliteJournalReader.StartupPartial",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string journalPath = Path.Combine(tempDir, "Journal.20260407170000.01.log");
            byte[] recordBytes = scenario.RecordBytes;
            byte[] prefix = recordBytes[..scenario.SplitPoint];
            byte[] suffix = recordBytes[scenario.SplitPoint..];
            File.WriteAllBytes(journalPath, prefix);

            scenario.InitialFileOffset = prefix.LongLength;
            scenario.ExpectedStartupCommittedOffset = 0;
            scenario.ExpectedCompletedCommittedOffset = recordBytes.LongLength + 1;

            var records = new ConcurrentQueue<string>();
            int dispatchCount = 0;
            var dispatched = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var watcher = new StartupOffsetProbeJournalWatcher(tempDir);
            watcher.MessageReceived += (_, args) =>
            {
                if (!string.Equals(args.EventType, "FSDJump", StringComparison.Ordinal))
                    return;

                records.Enqueue(args.Json);
                Interlocked.Increment(ref dispatchCount);
                dispatched.TrySetResult(true);
            };

            try
            {
                scenario.ActualStartupCommittedOffset = await watcher.StartWithObservedStartupAsync();
                scenario.PrematureDispatchCount = Volatile.Read(ref dispatchCount);

                using (var stream = new FileStream(
                    journalPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    await stream.WriteAsync(suffix);
                    await stream.WriteAsync(new byte[] { (byte)'\n' });
                    await stream.FlushAsync();
                    stream.Flush(flushToDisk: true);
                }

                scenario.CompletedFileOffset = new FileInfo(journalPath).Length;
                Task firstDispatch = await Task.WhenAny(
                    dispatched.Task,
                    Task.Delay(DispatchTimeout));
                scenario.DispatchObservedWithinBound = firstDispatch == dispatched.Task;

                if (scenario.DispatchObservedWithinBound)
                    await Task.Delay(DuplicateObservationWindow);

                scenario.DispatchCount = Volatile.Read(ref dispatchCount);
                scenario.ReceivedRecords = records.ToArray();
                scenario.BoundaryAlreadyWorking =
                    scenario.ActualStartupCommittedOffset == scenario.ExpectedStartupCommittedOffset &&
                    scenario.PrematureDispatchCount == 0 &&
                    scenario.CompletedFileOffset == scenario.ExpectedCompletedCommittedOffset &&
                    scenario.DispatchObservedWithinBound &&
                    scenario.DispatchCount == 1 &&
                    scenario.ReceivedRecords.SequenceEqual(new[] { scenario.Record }, StringComparer.Ordinal);

                return scenario.BoundaryAlreadyWorking;
            }
            finally
            {
                using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await watcher.StopWatchingAsync(stopTimeout.Token);
                watcher.Dispose();
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch (IOException)
                {
                    // A delayed filesystem callback can briefly retain a directory handle.
                }
                catch (UnauthorizedAccessException)
                {
                    // Cleanup is best-effort when the operating system still owns the handle.
                }
            }
        }

        private static Arbitrary<StartupPartialScenario> StartupPartialScenarioArbitrary()
        {
            string[] utf8Values = { "Sol", "München", "核心星", "🚀-α" };
            Gen<StartupPartialScenario> generator = Gen.Elements(utf8Values)
                .SelectMany(value => Gen.Choose(0, 8).SelectMany(padding =>
                {
                    var initial = new StartupPartialScenario(value, padding, 1);
                    return Gen.Choose(1, initial.RecordBytes.Length)
                        .Select(split => new StartupPartialScenario(value, padding, split));
                }));

            return Arb.From(generator, ShrinkStartupPartialScenario);
        }

        private static IEnumerable<StartupPartialScenario> ShrinkStartupPartialScenario(
            StartupPartialScenario scenario)
        {
            if (scenario.SplitPoint != 1)
                yield return new StartupPartialScenario(scenario.Utf8Value, scenario.Padding, 1);

            if (!string.Equals(scenario.Utf8Value, "Sol", StringComparison.Ordinal))
            {
                var simpler = new StartupPartialScenario("Sol", scenario.Padding, 1);
                yield return simpler;
            }

            if (scenario.Padding != 0)
                yield return new StartupPartialScenario(scenario.Utf8Value, 0, 1);
        }

        private sealed class StartupOffsetProbeJournalWatcher : JournalWatcher
        {
            public StartupOffsetProbeJournalWatcher(string path) : base(path)
            {
            }

            public async Task<long> StartWithObservedStartupAsync()
            {
                long startupCommittedOffset = ProcessPreviousJournals();
                IsLive = true;
                await StartWatching();
                return startupCommittedOffset;
            }
        }

        private sealed class StartupPartialScenario
        {
            public StartupPartialScenario(string utf8Value, int padding, int splitPoint)
            {
                Utf8Value = utf8Value;
                Padding = padding;
                SplitPoint = splitPoint;
            }

            public string Utf8Value { get; }
            public int Padding { get; }
            public int SplitPoint { get; }
            public string Marker => $"prefix-{Utf8Value}-{new string('x', Padding)}-suffix";
            public string Record => new JObject
            {
                ["timestamp"] = "2026-04-07T17:00:00Z",
                ["event"] = "FSDJump",
                ["StarSystem"] = Marker,
                ["SystemAddress"] = 10477373803L,
                ["StarPos"] = new JArray(0, 0, 0)
            }.ToString(Formatting.None);
            public byte[] RecordBytes => Encoding.UTF8.GetBytes(Record);

            public long InitialFileOffset { get; set; }
            public long ExpectedStartupCommittedOffset { get; set; }
            public long ActualStartupCommittedOffset { get; set; } = -1;
            public long ExpectedCompletedCommittedOffset { get; set; }
            public long CompletedFileOffset { get; set; }
            public int PrematureDispatchCount { get; set; }
            public int DispatchCount { get; set; }
            public bool DispatchObservedWithinBound { get; set; }
            public string[] ReceivedRecords { get; set; } = Array.Empty<string>();
            public bool BoundaryAlreadyWorking { get; set; }

            public override string ToString() =>
                $"Seed=0x{StartupPartialSeed:X8}; ShrunkSplit={SplitPoint}; Utf8={Utf8Value}; " +
                $"Offsets(initial={InitialFileOffset}, startupExpected={ExpectedStartupCommittedOffset}, " +
                $"startupActual={ActualStartupCommittedOffset}, completedExpected={ExpectedCompletedCommittedOffset}, " +
                $"completedActual={CompletedFileOffset}); PrematureDispatch={PrematureDispatchCount}; " +
                $"DispatchCount={DispatchCount}; DispatchWithinBound={DispatchObservedWithinBound}; " +
                $"BoundaryAlreadyWorking={BoundaryAlreadyWorking}";
        }
    }
}
