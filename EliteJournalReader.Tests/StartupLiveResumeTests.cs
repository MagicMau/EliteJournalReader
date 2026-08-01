#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Tests
{
    [TestClass]
    public class StartupLiveResumeTests
    {
        private const ulong SplitSeed = 0x10_15_51A2UL;
        private static readonly TimeSpan DispatchTimeout = TimeSpan.FromSeconds(4);

        [TestMethod]
        public async Task StartupPartialUtf8Suffix_CompletesExactlyOnceAfterAppend()
        {
            // **Validates: Requirements 2.10, 3.6**
            var probe = new ResumeScenario("🚀-核心星", 0, 1);
            byte[] recordBytes = probe.RecordBytes;
            int split = Array.FindIndex(recordBytes, value => (value & 0xC0) == 0x80);
            Assert.IsTrue(split > 0, "The deterministic case must split inside a UTF-8 sequence.");

            var scenario = new ResumeScenario(probe.Utf8Value, probe.Padding, split);
            Assert.IsTrue(await ObserveResumeAsync(scenario), scenario.ToString());
        }

        [TestMethod]
        public void Property_StartupCommittedOffset_ResumesGeneratedUtf8SuffixExactlyOnce()
        {
            // **Validates: Requirements 2.10, 3.6**
            Property property = FsCheck.FSharp.Prop.ForAll(
                ResumeScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<ResumeScenario, bool>(scenario =>
                    ObserveResumeAsync(scenario).GetAwaiter().GetResult()));

            var replay = new Replay(new Rnd(SplitSeed), null);
            Config config = Config.QuickThrowOnFailure
                .WithMaxTest(12)
                .WithReplay(FSharpOption<Replay>.Some(replay));
            Check.One(config, property);
        }
        private static async Task<bool> ObserveResumeAsync(ResumeScenario scenario)
        {
            string directory = Path.Combine(
                Path.GetTempPath(), "EliteJournalReader.StartupResume", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "Journal.20260407170000.01.log");

            const string startupSystem = "startup-complete";
            byte[] startupBytes = Encoding.UTF8.GetBytes(MakeRecord(startupSystem) + "\n");
            byte[] recordBytes = scenario.RecordBytes;
            byte[] initialBytes = new byte[startupBytes.Length + scenario.SplitPoint];
            Buffer.BlockCopy(startupBytes, 0, initialBytes, 0, startupBytes.Length);
            Buffer.BlockCopy(recordBytes, 0, initialBytes, startupBytes.Length, scenario.SplitPoint);
            File.WriteAllBytes(path, initialBytes);

            var received = new ConcurrentQueue<string>();
            var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var watcher = new JournalWatcher(directory);
            watcher.MessageReceived += (_, args) =>
            {
                if (!string.Equals(args.EventType, "FSDJump", StringComparison.Ordinal))
                    return;

                string? system = (args.EventArgs.OriginalEvent as JObject)?["StarSystem"]?.Value<string>();
                if (system == null)
                    return;

                received.Enqueue(system);
                if (string.Equals(system, scenario.Marker, StringComparison.Ordinal))
                    completed.TrySetResult(true);
            };

            try
            {
                await watcher.StartWatching();
                if (!received.ToArray().SequenceEqual(new[] { startupSystem }, StringComparer.Ordinal))
                    return false;

                // Allow the live framer to load and retain the uncommitted startup suffix.
                await Task.Delay(JournalWatcher.UPDATE_INTERVAL_MILLISECONDS + 150);
                if (!received.ToArray().SequenceEqual(new[] { startupSystem }, StringComparer.Ordinal))
                    return false;

                using (var writer = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    await writer.WriteAsync(recordBytes.AsMemory(scenario.SplitPoint));
                    await writer.WriteAsync(new byte[] { (byte)'\n' });
                    await writer.FlushAsync();
                    writer.Flush(flushToDisk: true);
                }

                if (await Task.WhenAny(completed.Task, Task.Delay(DispatchTimeout)) != completed.Task)
                    return false;

                await Task.Delay(JournalWatcher.UPDATE_INTERVAL_MILLISECONDS + 150);
                string[] actual = received.ToArray();
                return actual.SequenceEqual(new[] { startupSystem, scenario.Marker }, StringComparer.Ordinal) &&
                    actual.Count(value => value == startupSystem) == 1 &&
                    actual.Count(value => value == scenario.Marker) == 1;
            }
            finally
            {
                using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await watcher.StopWatchingAsync(stopTimeout.Token);
                watcher.Dispose();
                DeleteDirectory(directory);
            }
        }
        private static Arbitrary<ResumeScenario> ResumeScenarioArbitrary()
        {
            string[] values = { "Sol", "München", "核心星", "🚀-α", "𐍈-é" };
            Gen<ResumeScenario> generator = Gen.Elements(values)
                .SelectMany(value => Gen.Choose(0, 8).SelectMany(padding =>
                {
                    var probe = new ResumeScenario(value, padding, 1);
                    return Gen.Choose(1, probe.RecordBytes.Length - 1)
                        .Select(split => new ResumeScenario(value, padding, split));
                }));
            return Arb.From(generator, ShrinkScenario);
        }

        private static IEnumerable<ResumeScenario> ShrinkScenario(ResumeScenario scenario)
        {
            if (scenario.SplitPoint != 1)
                yield return new ResumeScenario(scenario.Utf8Value, scenario.Padding, 1);
            if (scenario.Padding != 0)
                yield return new ResumeScenario(scenario.Utf8Value, 0, 1);
            if (!string.Equals(scenario.Utf8Value, "Sol", StringComparison.Ordinal))
                yield return new ResumeScenario("Sol", scenario.Padding, 1);
        }

        private static string MakeRecord(string starSystem) => new JObject
        {
            ["timestamp"] = "2026-04-07T17:00:00Z",
            ["event"] = "FSDJump",
            ["StarSystem"] = starSystem,
            ["SystemAddress"] = 10477373803L,
            ["StarPos"] = new JArray(0, 0, 0)
        }.ToString(Formatting.None);

        private static void DeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (IOException)
            {
                // Delayed filesystem callbacks may briefly retain a directory handle.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup is best effort while the operating system owns the handle.
            }
        }

        private sealed class ResumeScenario
        {
            public ResumeScenario(string utf8Value, int padding, int splitPoint)
            {
                Utf8Value = utf8Value;
                Padding = padding;
                SplitPoint = splitPoint;
            }

            public string Utf8Value { get; }
            public int Padding { get; }
            public int SplitPoint { get; }
            public string Marker => $"prefix-{Utf8Value}-{new string('x', Padding)}-suffix";
            public string Record => MakeRecord(Marker);
            public byte[] RecordBytes => Encoding.UTF8.GetBytes(Record);

            public override string ToString() =>
                $"Seed=0x{SplitSeed:X}; Utf8={Utf8Value}; Padding={Padding}; " +
                $"Split={SplitPoint}; Bytes={RecordBytes.Length}";
        }
    }
}
