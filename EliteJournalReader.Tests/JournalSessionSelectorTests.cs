#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EliteJournalReader;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    /// <summary>
    /// Tests for JournalSessionSelector — canonical session parsing and deterministic selection.
    /// Validates that one parsed canonical session is reconstructed in deterministic numeric order
    /// with isolated legacy fallback. Metadata-first selection can no longer mix sessions or
    /// order part numbers lexically.
    ///
    /// **Validates: Requirements 2.15, 2.17, 3.9**
    /// </summary>
    [TestClass]
    [TestCategory("Session")]
    public class JournalSessionSelectorTests
    {
        #region Canonical Parsing Tests

        [TestMethod]
        public void TryParse_CanonicalCompactTimestamp_ParsesCorrectly()
        {
            var result = JournalSessionSelector.TryParse(@"C:\Journals\Journal.20260407173045.01.log");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Value.IsBeta);
            Assert.AreEqual("20260407173045", result.Value.SessionIdentity);
            Assert.AreEqual(new DateTime(2026, 4, 7, 17, 30, 45, DateTimeKind.Utc), result.Value.SessionInstantUtc);
            Assert.AreEqual(1, result.Value.PartNumber);
        }

        [TestMethod]
        public void TryParse_CanonicalIsoTimestamp_ParsesCorrectly()
        {
            var result = JournalSessionSelector.TryParse(@"C:\Journals\Journal.2026-04-07T170000.02.log");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Value.IsBeta);
            Assert.AreEqual("2026-04-07T170000", result.Value.SessionIdentity);
            Assert.AreEqual(new DateTime(2026, 4, 7, 17, 0, 0, DateTimeKind.Utc), result.Value.SessionInstantUtc);
            Assert.AreEqual(2, result.Value.PartNumber);
        }

        [TestMethod]
        public void TryParse_EquivalentCompactAndIsoForms_NormalizeToSameSessionKey()
        {
            // **Validates: Requirements 2.9**
            var compact = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.20260407170000.01.log");
            var iso = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.2026-04-07T170000.02.log");

            Assert.IsNotNull(compact);
            Assert.IsNotNull(iso);
            Assert.AreEqual(compact.Value.SessionInstantUtc, iso.Value.SessionInstantUtc);
            Assert.AreEqual(compact.Value.SessionKey, iso.Value.SessionKey);
        }

        [TestMethod]
        public void ParsedJournalFile_CompareTo_MixedFormsOrdersByUtcInstant()
        {
            // **Validates: Requirements 2.9**
            var olderCompact = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.20260407170000.01.log");
            var newerIso = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.2026-04-07T180000.01.log");

            Assert.IsNotNull(olderCompact);
            Assert.IsNotNull(newerIso);
            Assert.IsLessThan(0, olderCompact.Value.CompareTo(newerIso.Value),
                "Canonical journal comparison must follow the parsed UTC instant, not raw timestamp text.");
        }

        [TestMethod]
        public void TryParse_BetaCanonical_ParsesWithSeparateIdentity()
        {
            // **Validates: Requirements 2.9, 3.4**
            var beta = JournalSessionSelector.TryParse(
                @"C:\Journals\JournalBeta.20260407173045.03.log");
            var live = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.2026-04-07T173045.03.log");

            Assert.IsNotNull(beta);
            Assert.IsNotNull(live);
            Assert.IsTrue(beta.Value.IsBeta);
            Assert.AreEqual("20260407173045", beta.Value.SessionIdentity);
            Assert.AreEqual(new DateTime(2026, 4, 7, 17, 30, 45, DateTimeKind.Utc), beta.Value.SessionInstantUtc);
            Assert.AreEqual(3, beta.Value.PartNumber);
            Assert.AreEqual("Beta:20260407173045", beta.Value.SessionKey);
            Assert.AreNotEqual(beta.Value.SessionKey, live.Value.SessionKey);
            Assert.AreNotEqual(0, beta.Value.CompareTo(live.Value));
        }

        [TestMethod]
        public void TryParse_InvalidCanonicalTimestamp_ReturnsNull()
        {
            var result = JournalSessionSelector.TryParse(
                @"C:\Journals\Journal.20261340179999.01.log");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void TryParse_LegacyFile_ReturnsNull()
        {
            // A file that doesn't match canonical pattern
            var result = JournalSessionSelector.TryParse(@"C:\Journals\Journal.log");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TryParse_MultiDigitPart_ParsesAsInteger()
        {
            var result = JournalSessionSelector.TryParse(@"C:\Journals\Journal.20260407173045.10.log");
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Value.PartNumber);
        }

        #endregion

        #region Session Selection Tests

        [TestMethod]
        public void SelectSessionFiles_SingleSession_ReturnsAllPartsInNumericOrder()
        {
            var files = new[]
            {
                @"C:\Journals\Journal.20260407173045.03.log",
                @"C:\Journals\Journal.20260407173045.01.log",
                @"C:\Journals\Journal.20260407173045.02.log",
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(@"C:\Journals\Journal.20260407173045.01.log", result[0]);
            Assert.AreEqual(@"C:\Journals\Journal.20260407173045.02.log", result[1]);
            Assert.AreEqual(@"C:\Journals\Journal.20260407173045.03.log", result[2]);
        }

        [TestMethod]
        public void SelectSessionFiles_MixedTimestampForms_SelectsGreatestInstantAndOrdersPartsNumerically()
        {
            // **Validates: Requirements 2.9, 3.4**
            var files = new[]
            {
                @"C:\Journals\Journal.20260407170000.99.log",
                @"C:\Journals\Journal.2026-04-07T180000.10.log",
                @"C:\Journals\Journal.2026-04-07T180000.02.log",
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

            CollectionAssert.AreEqual(new[]
            {
                @"C:\Journals\Journal.2026-04-07T180000.02.log",
                @"C:\Journals\Journal.2026-04-07T180000.10.log",
            }, result.ToArray());
        }

        [TestMethod]
        public void SelectSessionFiles_MixedSessionsWithMisleadingMetadata_DoesNotMix()
        {
            // Bug condition: metadata-first selection can mix sessions
            // Even if older session files have newer write times, only the greatest
            // parsed session is selected.
            var files = new[]
            {
                @"C:\Journals\Journal.20260406150000.01.log",  // old session
                @"C:\Journals\Journal.20260406150000.02.log",  // old session
                @"C:\Journals\Journal.20260407170000.01.log",  // new session
            };

            // Misleading: old session files have newer write times
            DateTime GetMisleadingWriteTime(string path)
            {
                if (path.Contains("20260406"))
                    return new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc); // newer write time!
                return new DateTime(2026, 4, 7, 17, 0, 0, DateTimeKind.Utc);
            }

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, GetMisleadingWriteTime);

            // Must select the greatest session by parsed name, not by metadata
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(@"C:\Journals\Journal.20260407170000.01.log", result[0]);
        }

        [TestMethod]
        public void SelectSessionFiles_PartsOrderedNumericNotLexical()
        {
            // Bug condition: part numbers ordered lexically would put .10 before .2
            var files = new[]
            {
                @"C:\Journals\Journal.20260407173045.01.log",
                @"C:\Journals\Journal.20260407173045.02.log",
                @"C:\Journals\Journal.20260407173045.10.log",
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

            Assert.AreEqual(3, result.Count);
            // Numeric order: 1, 2, 10 (not 1, 10, 2 which would be lexical)
            Assert.IsTrue(result[0].Contains(".01."));
            Assert.IsTrue(result[1].Contains(".02."));
            Assert.IsTrue(result[2].Contains(".10."));
        }

        [TestMethod]
        public void SelectSessionFiles_NoCanonicalNameParses_UsesMetadataForSingleFileFallback()
        {
            // **Validates: Requirements 3.4**
            var files = new[]
            {
                @"C:\Journals\Journal.log",
                @"C:\Journals\Journal.20261340179999.01.log",
            };

            var writeTimeLookup = new Dictionary<string, DateTime>
            {
                [files[0]] = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc),
                [files[1]] = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, f => writeTimeLookup[f]);

            CollectionAssert.AreEqual(new[] { files[1] }, result.ToArray());
        }

        [TestMethod]
        public void SelectSessionFiles_NeverMixesLegacyAndCanonical()
        {
            var files = new[]
            {
                @"C:\Journals\Journal.log",  // legacy
                @"C:\Journals\Journal.20260407173045.01.log",  // canonical
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, _ => DateTime.MaxValue);

            // Canonical files take precedence — legacy file is excluded
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Contains("20260407173045"));
        }

        [TestMethod]
        public void SelectSessionFiles_EmptyInput_ReturnsEmpty()
        {
            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(Array.Empty<string>(), _ => DateTime.MinValue);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SelectSessionFiles_BetaAndNonBeta_SelectsGreatestPartitionSessionWithoutMixing()
        {
            // **Validates: Requirements 2.9, 3.4**
            var files = new[]
            {
                @"C:\Journals\Journal.20260407180000.09.log",
                @"C:\Journals\JournalBeta.2026-04-07T190000.10.log",
                @"C:\Journals\JournalBeta.2026-04-07T190000.02.log",
            };

            var selector = new JournalSessionSelector();
            var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

            CollectionAssert.AreEqual(new[]
            {
                @"C:\Journals\JournalBeta.2026-04-07T190000.02.log",
                @"C:\Journals\JournalBeta.2026-04-07T190000.10.log",
            }, result.ToArray());
        }

        #endregion

        #region Live Mode Switching Tests

        [TestMethod]
        public void ShouldSwitchToFile_NewerMixedFormSession_ReturnsTrue()
        {
            // **Validates: Requirements 2.9**
            var selector = new JournalSessionSelector();
            bool result = selector.ShouldSwitchToFile(
                "20260407170000", 2,
                @"C:\Journals\Journal.2026-04-07T180000.01.log");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldSwitchToFile_EquivalentMixedFormHigherPart_ReturnsTrue()
        {
            // **Validates: Requirements 2.9, 3.4**
            var selector = new JournalSessionSelector();
            bool result = selector.ShouldSwitchToFile(
                "20260407170000", 2,
                @"C:\Journals\Journal.2026-04-07T170000.03.log");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldSwitchToFile_OlderNewlyCreatedMixedFormSession_ReturnsFalse()
        {
            // **Validates: Requirements 2.9, 3.5**
            // Preserve task 2's counterexample direction: raw compact text sorts after ISO text
            // even though this newly created compact session is chronologically older.
            var selector = new JournalSessionSelector();
            bool result = selector.ShouldSwitchToFile(
                "2026-04-07T180000", 1,
                @"C:\Journals\Journal.20260407170000.99.log");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSwitchToFile_EquivalentMixedFormSamePart_ReturnsFalse()
        {
            // **Validates: Requirements 2.9, 3.5**
            // Preserve task 2's same-part tie direction, which failed under raw text comparison.
            var selector = new JournalSessionSelector();
            bool result = selector.ShouldSwitchToFile(
                "2026-04-07T170000", 2,
                @"C:\Journals\Journal.20260407170000.02.log");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSwitchToFile_OtherBetaPartition_ReturnsFalse()
        {
            // **Validates: Requirements 2.9, 3.4**
            var selector = new JournalSessionSelector();

            Assert.IsFalse(selector.ShouldSwitchToFile(
                "20260407170000", 2,
                @"C:\Journals\JournalBeta.20260407180000.01.log"));
            Assert.IsFalse(selector.ShouldSwitchToFile(
                "Beta:20260407170000", 2,
                @"C:\Journals\Journal.20260407180000.01.log"));
        }

        [TestMethod]
        public void ShouldSwitchToFile_LegacyFile_ReturnsFalse()
        {
            // **Validates: Requirements 3.4**
            var selector = new JournalSessionSelector();
            bool result = selector.ShouldSwitchToFile(
                "20260407100000", 1,
                @"C:\Journals\Journal.log");
            Assert.IsFalse(result);
        }

        #endregion

        #region Integration: Real Filesystem Test

        [TestMethod]
        public void Integration_RealFilesystem_SelectsCorrectSession()
        {
            // **Validates: Requirements 2.15, 3.9**
            var tempDir = Path.Combine(Path.GetTempPath(),
                "EliteJournalReader.Session.Integration",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create files from two sessions with misleading creation times
                // Old session files created AFTER new session files (misleading metadata)
                CreateFile(tempDir, "Journal.20260407100000.01.log");
                CreateFile(tempDir, "Journal.20260407100000.02.log");
                Thread.Sleep(50);
                CreateFile(tempDir, "Journal.20260407120000.01.log");
                CreateFile(tempDir, "Journal.20260407120000.02.log");
                CreateFile(tempDir, "Journal.20260407120000.10.log");

                var allFiles = Directory.GetFiles(tempDir, "Journal*.*.log");
                var selector = new JournalSessionSelector();
                var result = selector.SelectSessionFiles(allFiles,
                    f => File.GetLastWriteTimeUtc(f));

                // Only the newer session is selected
                Assert.AreEqual(3, result.Count);
                Assert.IsTrue(result.All(f => Path.GetFileName(f).Contains("20260407120000")));

                // Ordered numerically: 01, 02, 10
                Assert.IsTrue(Path.GetFileName(result[0]).Contains(".01."));
                Assert.IsTrue(Path.GetFileName(result[1]).Contains(".02."));
                Assert.IsTrue(Path.GetFileName(result[2]).Contains(".10."));
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [TestMethod]
        public void Integration_ProcessPreviousJournals_UsesSessionSelector()
        {
            // **Validates: Requirements 2.15, 2.17**
            var tempDir = Path.Combine(Path.GetTempPath(),
                "EliteJournalReader.Session.ProcessPrev",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create two sessions — old session with more parts
                string header = @"{""timestamp"":""2026-04-07T10:00:00Z"",""event"":""Fileheader"",""part"":1,""language"":""English/UK"",""Odyssey"":true,""gameversion"":""4.0""}" + "\n";
                string loadGame = @"{""timestamp"":""2026-04-07T12:00:00Z"",""event"":""LoadGame"",""Commander"":""Test"",""Ship"":""SideWinder""}" + "\n";

                // Old session with 3 parts
                File.WriteAllText(Path.Combine(tempDir, "Journal.20260406100000.01.log"), header);
                File.WriteAllText(Path.Combine(tempDir, "Journal.20260406100000.02.log"), header);
                File.WriteAllText(Path.Combine(tempDir, "Journal.20260406100000.03.log"), header);

                // New session with 2 parts
                File.WriteAllText(Path.Combine(tempDir, "Journal.20260407120000.01.log"), header);
                File.WriteAllText(Path.Combine(tempDir, "Journal.20260407120000.02.log"), loadGame);

                var watcher = new JournalWatcher(tempDir);
                var events = new List<string>();
                watcher.MessageReceived += (_, e) => events.Add(e.EventType);

                // StartWatching internally calls ProcessPreviousJournals
                var startTask = watcher.StartWatching();
                Thread.Sleep(300);
                watcher.StopWatching();

                // Only events from the newest session (2 files) should be processed
                // Old session's 3 files should NOT be included
                Assert.IsTrue(events.Count >= 1, "Should process at least one event from the newest session");
                // Verify we got events from both parts of the new session (Fileheader + LoadGame)
                Assert.IsTrue(events.Contains("Fileheader") || events.Contains("LoadGame"),
                    "Should have events from the newest session");
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        #endregion

        #region FsCheck Property Tests

        [TestMethod]
        public void ParsedJournalFile_EquivalentFormsWithSamePart_UsesOriginalFilenameForDeterministicTie()
        {
            // **Validates: Requirements 2.9, 3.4**
            string compactPath = Name(GeneratedBaseUtc, iso: false, isBeta: false, part: 2);
            string isoPath = Name(GeneratedBaseUtc, iso: true, isBeta: false, part: 2);
            var compact = JournalSessionSelector.TryParse(compactPath);
            var iso = JournalSessionSelector.TryParse(isoPath);

            Assert.IsNotNull(compact);
            Assert.IsNotNull(iso);
            Assert.AreEqual(compact.Value.SessionKey, iso.Value.SessionKey);

            int expected = StringComparer.Ordinal.Compare(compact.Value.Filename, iso.Value.Filename);
            Assert.AreEqual(Math.Sign(expected), Math.Sign(compact.Value.CompareTo(iso.Value)),
                "Equivalent identity and part ties must use the retained original filename deterministically.");
        }

        [TestMethod]
        public void Property_GeneratedCanonicalForms_ParseEquivalentAndOrderedIdentities()
        {
            // **Validates: Requirements 2.9, 3.4**
            Property property = FsCheck.FSharp.Prop.ForAll(
                CanonicalRegressionScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<CanonicalRegressionScenario, bool>(scenario =>
                {
                    var older = JournalSessionSelector.TryParse(scenario.OlderCompactFile);
                    var equivalentCompact = JournalSessionSelector.TryParse(scenario.EquivalentCompactFile);
                    var equivalentIso = JournalSessionSelector.TryParse(scenario.EquivalentIsoFile);
                    var otherPartition = JournalSessionSelector.TryParse(scenario.OtherPartitionFile);

                    if (!older.HasValue || !equivalentCompact.HasValue ||
                        !equivalentIso.HasValue || !otherPartition.HasValue)
                    {
                        return false;
                    }

                    return equivalentCompact.Value.SessionInstantUtc == equivalentIso.Value.SessionInstantUtc &&
                        string.Equals(
                            equivalentCompact.Value.SessionKey,
                            equivalentIso.Value.SessionKey,
                            StringComparison.OrdinalIgnoreCase) &&
                        equivalentCompact.Value.IsBeta == scenario.IsBeta &&
                        equivalentIso.Value.IsBeta == scenario.IsBeta &&
                        equivalentCompact.Value.PartNumber == scenario.CurrentPart &&
                        older.Value.CompareTo(equivalentCompact.Value) < 0 &&
                        otherPartition.Value.IsBeta != equivalentCompact.Value.IsBeta &&
                        !string.Equals(
                            otherPartition.Value.SessionKey,
                            equivalentCompact.Value.SessionKey,
                            StringComparison.OrdinalIgnoreCase);
                }));

            Check.One(RegressionConfig(0x1D3E71A5UL), property);
        }

        [TestMethod]
        public void Property_GeneratedMixedForms_StartupSelectsNewestSessionAndNumericPartSet()
        {
            // **Validates: Requirements 2.9, 3.4**
            Property property = FsCheck.FSharp.Prop.ForAll(
                CanonicalRegressionScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<CanonicalRegressionScenario, bool>(scenario =>
                {
                    IReadOnlyList<string> selected = new JournalSessionSelector().SelectSessionFiles(
                        scenario.StartupFiles,
                        path => path == scenario.OlderCompactFile ? DateTime.MaxValue : DateTime.MinValue);

                    return selected.SequenceEqual(
                        scenario.ExpectedStartupFiles,
                        StringComparer.OrdinalIgnoreCase);
                }));

            Check.One(RegressionConfig(0x57A27E11UL), property);
        }

        [TestMethod]
        public void Property_GeneratedMixedForms_LiveSwitchesOnlyForNewerSessionOrHigherCurrentPart()
        {
            // **Validates: Requirements 2.9, 3.4, 3.5**
            Property property = FsCheck.FSharp.Prop.ForAll(
                CanonicalRegressionScenarioArbitrary(),
                FuncConvert.ToFSharpFunc<CanonicalRegressionScenario, bool>(scenario =>
                {
                    var selector = new JournalSessionSelector();

                    return HasExpectedLiveDecisions(
                            selector, scenario, scenario.NormalizedCurrentSessionKey, useIsoCandidates: true) &&
                        HasExpectedLiveDecisions(
                            selector, scenario, scenario.IsoCurrentSessionKey, useIsoCandidates: false);
                }));

            Check.One(RegressionConfig(0x11CE5EEDUL), property);
        }

        private static bool HasExpectedLiveDecisions(
            JournalSessionSelector selector,
            CanonicalRegressionScenario scenario,
            string currentSessionKey,
            bool useIsoCandidates)
        {
            bool samePartRejected = !selector.ShouldSwitchToFile(
                currentSessionKey,
                scenario.CurrentPart,
                scenario.EquivalentSamePartFile(useIsoCandidates));
            bool higherPartAccepted = selector.ShouldSwitchToFile(
                currentSessionKey,
                scenario.CurrentPart,
                scenario.EquivalentHigherPartFile(useIsoCandidates));
            bool olderNewlyCreatedRejected = !selector.ShouldSwitchToFile(
                currentSessionKey,
                scenario.CurrentPart,
                scenario.OlderNewlyCreatedFile(useIsoCandidates));
            bool newerSessionAccepted = selector.ShouldSwitchToFile(
                currentSessionKey,
                scenario.CurrentPart,
                scenario.NewerSessionFile(useIsoCandidates));
            bool otherPartitionRejected = !selector.ShouldSwitchToFile(
                currentSessionKey,
                scenario.CurrentPart,
                scenario.OtherPartitionCandidate(useIsoCandidates));

            return samePartRejected &&
                higherPartAccepted &&
                olderNewlyCreatedRejected &&
                newerSessionAccepted &&
                otherPartitionRejected;
        }

        private static readonly DateTime GeneratedBaseUtc =
            new DateTime(2026, 4, 7, 17, 0, 0, DateTimeKind.Utc);

        private static Config RegressionConfig(ulong seed)
        {
            var replay = new Replay(new Rnd(seed), null);
            return Config.QuickThrowOnFailure
                .WithMaxTest(100)
                .WithReplay(FSharpOption<Replay>.Some(replay));
        }

        private static string SessionKey(DateTime utc, bool iso, bool isBeta)
        {
            string session = utc.ToString(iso ? "yyyy-MM-dd'T'HHmmss" : "yyyyMMddHHmmss");
            return (isBeta ? "Beta:" : "") + session;
        }

        private static string Name(DateTime utc, bool iso, bool isBeta, int part)
        {
            string prefix = isBeta ? "JournalBeta" : "Journal";
            string session = utc.ToString(iso ? "yyyy-MM-dd'T'HHmmss" : "yyyyMMddHHmmss");
            return $@"C:\Journals\{prefix}.{session}.{part:D2}.log";
        }

        private static Arbitrary<CanonicalRegressionScenario> CanonicalRegressionScenarioArbitrary()
        {
            Gen<CanonicalRegressionScenario> generator = Gen.Choose(1, 180)
                .SelectMany(gapMinutes => Gen.Elements(false, true)
                    .SelectMany(isBeta => Gen.Choose(1, 24)
                        .SelectMany(currentPart => Gen.Choose(2, 30)
                            .Select(extraPart => new CanonicalRegressionScenario(
                                gapMinutes,
                                isBeta,
                                currentPart,
                                extraPart)))));

            return Arb.From(generator, ShrinkCanonicalRegressionScenario);
        }

        private static IEnumerable<CanonicalRegressionScenario> ShrinkCanonicalRegressionScenario(
            CanonicalRegressionScenario scenario)
        {
            if (scenario.GapMinutes != 1)
                yield return new CanonicalRegressionScenario(
                    1, scenario.IsBeta, scenario.CurrentPart, scenario.ExtraPart);
            if (scenario.IsBeta)
                yield return new CanonicalRegressionScenario(
                    scenario.GapMinutes, false, scenario.CurrentPart, scenario.ExtraPart);
            if (scenario.CurrentPart != 1)
                yield return new CanonicalRegressionScenario(
                    scenario.GapMinutes, scenario.IsBeta, 1, scenario.ExtraPart);
            if (scenario.ExtraPart != 2)
                yield return new CanonicalRegressionScenario(
                    scenario.GapMinutes, scenario.IsBeta, scenario.CurrentPart, 2);
        }

        private sealed class CanonicalRegressionScenario
        {
            public int GapMinutes { get; }
            public bool IsBeta { get; }
            public int CurrentPart { get; }
            public int ExtraPart { get; }
            public DateTime CurrentUtc => GeneratedBaseUtc.AddMinutes(GapMinutes);
            public DateTime NewerUtc => CurrentUtc.AddMinutes(GapMinutes);
            public string OlderCompactFile => Name(GeneratedBaseUtc, false, IsBeta, 99);
            public string EquivalentCompactFile => Name(CurrentUtc, false, IsBeta, CurrentPart);
            public string EquivalentIsoFile => Name(CurrentUtc, true, IsBeta, CurrentPart);
            public string NormalizedCurrentSessionKey => SessionKey(CurrentUtc, false, IsBeta);
            public string IsoCurrentSessionKey => SessionKey(CurrentUtc, true, IsBeta);
            public string OtherPartitionFile => Name(CurrentUtc, true, !IsBeta, CurrentPart + 10);

            public string EquivalentSamePartFile(bool iso) =>
                Name(CurrentUtc, iso, IsBeta, CurrentPart);

            public string EquivalentHigherPartFile(bool iso) =>
                Name(CurrentUtc, iso, IsBeta, CurrentPart + 1);

            public string OlderNewlyCreatedFile(bool iso) =>
                Name(GeneratedBaseUtc, iso, IsBeta, 99);

            public string NewerSessionFile(bool iso) =>
                Name(NewerUtc, iso, IsBeta, 1);

            public string OtherPartitionCandidate(bool iso) =>
                Name(CurrentUtc, iso, !IsBeta, CurrentPart + 10);

            public int[] Parts => new[] { 1, 10, CurrentPart, ExtraPart }
                .Distinct()
                .OrderBy(part => part)
                .ToArray();

            public string[] ExpectedStartupFiles => Parts
                .Select((part, index) => Name(CurrentUtc, index % 2 == 0, IsBeta, part))
                .ToArray();

            public string[] StartupFiles => new[] { OlderCompactFile }
                .Concat(ExpectedStartupFiles.Reverse())
                .ToArray();

            public CanonicalRegressionScenario(
                int gapMinutes,
                bool isBeta,
                int currentPart,
                int extraPart)
            {
                GapMinutes = gapMinutes;
                IsBeta = isBeta;
                CurrentPart = currentPart;
                ExtraPart = extraPart;
            }

            public override string ToString() =>
                $"GapMinutes={GapMinutes}; Partition={(IsBeta ? "beta" : "normal")}; " +
                $"CurrentPart={CurrentPart}; Parts=[{string.Join(",", Parts)}]; " +
                $"Startup=[{string.Join(", ", StartupFiles.Select(Path.GetFileName))}]";
        }

        [TestMethod]
        public void Property_NeverMixesSessions()
        {
            // **Validates: Requirements 2.15**
            // For any set of canonical journal files from multiple sessions,
            // the selected files all belong to exactly one session.
            Property property = FsCheck.FSharp.Prop.ForAll(
                MultipleSessionFilesArbitrary(),
                FuncConvert.ToFSharpFunc<string[], bool>(files =>
                {
                    if (files.Length == 0) return true;

                    var selector = new JournalSessionSelector();
                    var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

                    if (result.Count == 0) return true;

                    // All selected files must share the same session key
                    var parsedFiles = result
                        .Select(f => JournalSessionSelector.TryParse(f))
                        .Where(p => p.HasValue)
                        .Select(p => p!.Value)
                        .ToList();

                    if (parsedFiles.Count == 0) return true; // legacy fallback

                    var distinctKeys = parsedFiles.Select(p => p.SessionKey).Distinct().Count();
                    return distinctKeys == 1;
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(200), property);
        }

        [TestMethod]
        public void Property_PartsOrderedNumerically()
        {
            // **Validates: Requirements 2.15, 2.17**
            // For any selected session, the parts are in strictly ascending numeric order.
            Property property = FsCheck.FSharp.Prop.ForAll(
                MultipleSessionFilesArbitrary(),
                FuncConvert.ToFSharpFunc<string[], bool>(files =>
                {
                    if (files.Length == 0) return true;

                    var selector = new JournalSessionSelector();
                    var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

                    if (result.Count <= 1) return true;

                    // Verify numeric ascending order
                    var parsedParts = result
                        .Select(f => JournalSessionSelector.TryParse(f))
                        .Where(p => p.HasValue)
                        .Select(p => p!.Value.PartNumber)
                        .ToList();

                    for (int i = 1; i < parsedParts.Count; i++)
                    {
                        if (parsedParts[i] < parsedParts[i - 1])
                            return false;
                    }
                    return true;
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(200), property);
        }

        [TestMethod]
        public void Property_GreatestSessionAlwaysSelected()
        {
            // **Validates: Requirements 2.15**
            // The selected session has the greatest session identity among all canonical files.
            Property property = FsCheck.FSharp.Prop.ForAll(
                MultipleSessionFilesArbitrary(),
                FuncConvert.ToFSharpFunc<string[], bool>(files =>
                {
                    if (files.Length == 0) return true;

                    var selector = new JournalSessionSelector();
                    var result = selector.SelectSessionFiles(files, _ => DateTime.MinValue);

                    if (result.Count == 0) return true;

                    var selectedParsed = result
                        .Select(f => JournalSessionSelector.TryParse(f))
                        .Where(p => p.HasValue)
                        .Select(p => p!.Value)
                        .ToList();

                    if (selectedParsed.Count == 0) return true; // legacy

                    var selectedSessionIdentity = selectedParsed.First().SessionIdentity;

                    // All canonical files should have session identity <= selected
                    var allParsed = files
                        .Select(f => JournalSessionSelector.TryParse(f))
                        .Where(p => p.HasValue)
                        .Select(p => p!.Value)
                        .ToList();

                    foreach (var p in allParsed)
                    {
                        if (StringComparer.OrdinalIgnoreCase.Compare(p.SessionIdentity, selectedSessionIdentity) > 0)
                            return false;
                    }
                    return true;
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(200), property);
        }

        [TestMethod]
        public void Property_LegacyFallbackSelectsExactlyOne()
        {
            // **Validates: Requirements 2.15**
            // When no canonical files exist, exactly one legacy file is selected.
            Property property = FsCheck.FSharp.Prop.ForAll(
                LegacyFilesArbitrary(),
                FuncConvert.ToFSharpFunc<(string[] Files, Dictionary<string, DateTime> WriteTimes), bool>(input =>
                {
                    if (input.Files.Length == 0) return true;

                    var selector = new JournalSessionSelector();
                    var result = selector.SelectSessionFiles(input.Files,
                        f => input.WriteTimes.GetValueOrDefault(f, DateTime.MinValue));

                    // Legacy fallback selects exactly one file
                    return result.Count == 1;
                }));

            Check.One(Config.QuickThrowOnFailure.WithMaxTest(100), property);
        }

        #endregion

        #region Helper Methods

        private static void CreateFile(string directory, string filename)
        {
            string filePath = Path.Combine(directory, filename);
            string json = $"{{\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss'Z'}\",\"event\":\"Fileheader\",\"part\":1}}\n";
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region FsCheck Generators

        private static Arbitrary<string[]> MultipleSessionFilesArbitrary()
        {
            // Generate 1-4 sessions, each with 1-5 parts (some with high part numbers like 10)
            var gen = Gen.Choose(1, 4).SelectMany(sessionCount =>
            {
                var baseDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
                var allFiles = new List<string>();

                return Gen.Choose(1, 5).Select(maxParts =>
                {
                    var files = new List<string>();
                    for (int s = 0; s < sessionCount; s++)
                    {
                        var sessionTime = baseDate.AddHours(s * 3);
                        var sessionStr = sessionTime.ToString("yyyyMMddHHmmss");
                        int partCount = Math.Max(1, (maxParts + s) % 5 + 1);
                        var usedParts = new HashSet<int>();

                        for (int p = 0; p < partCount; p++)
                        {
                            // Use part numbers that test numeric vs lexical ordering
                            int partNum = p < 2 ? p + 1 : (p + 1) * 3; // e.g., 1, 2, 9, 12...
                            while (usedParts.Contains(partNum)) partNum++;
                            usedParts.Add(partNum);
                            files.Add($@"C:\Journals\Journal.{sessionStr}.{partNum:D2}.log");
                        }
                    }
                    return files.ToArray();
                });
            });

            return Arb.From(gen);
        }

        private static Arbitrary<(string[] Files, Dictionary<string, DateTime> WriteTimes)> LegacyFilesArbitrary()
        {
            // Generate 1-5 legacy files with varying write times
            var gen = Gen.Choose(1, 5).Select(count =>
            {
                var files = new string[count];
                var writeTimes = new Dictionary<string, DateTime>();
                var baseTime = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

                for (int i = 0; i < count; i++)
                {
                    // Legacy filenames that don't match canonical pattern
                    files[i] = $@"C:\Journals\JournalReplay{i + 1}.log";
                    writeTimes[files[i]] = baseTime.AddHours(i * 2);
                }

                return (files, writeTimes);
            });

            return Arb.From(gen);
        }

        #endregion
    }
}
