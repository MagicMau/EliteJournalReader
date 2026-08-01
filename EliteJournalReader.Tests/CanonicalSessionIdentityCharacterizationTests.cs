#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EliteJournalReader;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    /// <summary>
    /// Unfixed-code characterization for canonical compact/ISO session identity.
    /// These assertions intentionally describe the required normalized behavior and are
    /// expected to fail until JournalSessionSelector compares parsed UTC identities.
    /// </summary>
    [TestClass]
    [TestCategory("BugConditionExploration")]
    public class CanonicalSessionIdentityCharacterizationTests
    {
        private static readonly DateTime BaseUtc =
            new DateTime(2026, 4, 7, 17, 0, 0, DateTimeKind.Utc);

        // Fixed FsCheck seeds make each unfixed counterexample directly reproducible.
        private const ulong EquivalentIdentitySeed = 0x1D3E71A5UL;
        private const ulong StartupSelectionSeed = 0x57A27E11UL;
        private const ulong LiveSwitchSeed = 0x11CE5EEDUL;

        [TestMethod]
        public void Deterministic_EquivalentCompactAndIsoNames_HaveEqualCanonicalIdentity()
        {
            // **Validates: Requirements 1.9**
            string compact = Name(BaseUtc, iso: false, isBeta: false, part: 1);
            string iso = Name(BaseUtc, iso: true, isBeta: false, part: 1);

            var compactParsed = JournalSessionSelector.TryParse(compact);
            var isoParsed = JournalSessionSelector.TryParse(iso);

            Assert.IsNotNull(compactParsed);
            Assert.IsNotNull(isoParsed);
            Assert.AreEqual(compactParsed.Value.SessionKey, isoParsed.Value.SessionKey,
                $"Equivalent UTC identities differ. Files=[{compact}, {iso}]");
        }
        [TestMethod]
        public void Deterministic_MixedFormsStartup_SelectsChronologicallyNewerIsoSession()
        {
            // **Validates: Requirements 1.9**
            string olderCompact = Name(BaseUtc, iso: false, isBeta: false, part: 1);
            string newerIsoPart1 = Name(BaseUtc.AddHours(1), iso: true, isBeta: false, part: 1);
            string newerIsoPart10 = Name(BaseUtc.AddHours(1), iso: true, isBeta: false, part: 10);
            string[] files = { olderCompact, newerIsoPart10, newerIsoPart1 };

            var result = new JournalSessionSelector().SelectSessionFiles(files, _ => DateTime.MinValue);

            CollectionAssert.AreEqual(new[] { newerIsoPart1, newerIsoPart10 }, result.ToArray(),
                $"Startup did not select the greatest UTC session. Files=[{string.Join(", ", files)}]");
        }

        [TestMethod]
        public void Deterministic_EquivalentMixedFormSamePart_DoesNotSwitch()
        {
            // **Validates: Requirements 1.9**
            string currentIso = Name(BaseUtc, iso: true, isBeta: false, part: 2);
            string newlyCreatedCompact = Name(BaseUtc, iso: false, isBeta: false, part: 2);
            string currentKey = Parse(currentIso).SessionKey;

            bool switched = new JournalSessionSelector().ShouldSwitchToFile(
                currentKey, 2, newlyCreatedCompact);

            Assert.IsFalse(switched,
                $"Equivalent same-part file displaced current file. Files=[{currentIso}, {newlyCreatedCompact}]");
        }

        [TestMethod]
        public void Deterministic_EquivalentMixedFormHigherPart_DoesSwitch()
        {
            // **Validates: Requirements 1.9**
            string currentCompact = Name(BaseUtc, iso: false, isBeta: false, part: 2);
            string newlyCreatedIso = Name(BaseUtc, iso: true, isBeta: false, part: 3);
            string currentKey = Parse(currentCompact).SessionKey;

            bool switched = new JournalSessionSelector().ShouldSwitchToFile(
                currentKey, 2, newlyCreatedIso);

            Assert.IsTrue(switched,
                $"Equivalent higher part was not eligible. Files=[{currentCompact}, {newlyCreatedIso}]");
        }

        [TestMethod]
        public void Deterministic_OlderNewlyCreatedCompactSession_DoesNotDisplaceCurrentIsoSession()
        {
            // **Validates: Requirements 1.9**
            string currentIso = Name(BaseUtc.AddHours(1), iso: true, isBeta: false, part: 1);
            string newlyCreatedOlderCompact = Name(BaseUtc, iso: false, isBeta: false, part: 9);
            string currentKey = Parse(currentIso).SessionKey;

            bool switched = new JournalSessionSelector().ShouldSwitchToFile(
                currentKey, 1, newlyCreatedOlderCompact);

            Assert.IsFalse(switched,
                $"Older newly-created session displaced current session. Files=[{currentIso}, {newlyCreatedOlderCompact}]");
        }

        [TestMethod]
        public void Property_EquivalentForms_NormalizeToSameIdentity()
        {
            // **Validates: Requirements 1.9**
            Property property = FsCheck.FSharp.Prop.ForAll(
                EquivalentIdentityArbitrary(),
                FuncConvert.ToFSharpFunc<EquivalentIdentityScenario, bool>(input =>
                {
                    var compact = JournalSessionSelector.TryParse(input.CompactFile);
                    var iso = JournalSessionSelector.TryParse(input.IsoFile);
                    bool isBugConditionInput = compact.HasValue && iso.HasValue;
                    if (!isBugConditionInput)
                        return true;

                    return string.Equals(
                        compact!.Value.SessionKey,
                        iso!.Value.SessionKey,
                        StringComparison.OrdinalIgnoreCase);
                }));

            Check.One(ExplorationConfig(EquivalentIdentitySeed), property);
        }
        [TestMethod]
        public void Property_MixedFormsStartup_SelectsGreatestUtcSessionAndNumericParts()
        {
            // **Validates: Requirements 1.9**
            Property property = FsCheck.FSharp.Prop.ForAll(
                StartupSelectionArbitrary(),
                FuncConvert.ToFSharpFunc<StartupSelectionScenario, bool>(input =>
                {
                    var selector = new JournalSessionSelector();
                    var selected = selector.SelectSessionFiles(input.Files, _ => DateTime.MinValue);
                    bool isBugConditionInput = input.NewerUtc > input.OlderUtc;
                    if (!isBugConditionInput)
                        return true;

                    return selected.SequenceEqual(input.ExpectedFiles, StringComparer.OrdinalIgnoreCase);
                }));

            Check.One(ExplorationConfig(StartupSelectionSeed), property);
        }

        [TestMethod]
        public void Property_MixedFormsLiveSwitch_UsesInstantAndPartMonotonically()
        {
            // **Validates: Requirements 1.9**
            Property property = FsCheck.FSharp.Prop.ForAll(
                LiveSwitchArbitrary(),
                FuncConvert.ToFSharpFunc<LiveSwitchScenario, bool>(input =>
                {
                    string currentKey = Parse(input.CurrentFile).SessionKey;
                    bool actual = new JournalSessionSelector().ShouldSwitchToFile(
                        currentKey, input.CurrentPart, input.NewFile);
                    bool isBugConditionInput = input.UsesMixedForms;
                    return !isBugConditionInput || actual == input.ExpectedSwitch;
                }));

            Check.One(ExplorationConfig(LiveSwitchSeed), property);
        }

        private static Config ExplorationConfig(ulong seed)
        {
            var replay = new Replay(new Rnd(seed), null);
            return Config.QuickThrowOnFailure
                .WithMaxTest(100)
                .WithReplay(FSharpOption<Replay>.Some(replay));
        }

        private static JournalSessionSelector.ParsedJournalFile Parse(string path)
        {
            var parsed = JournalSessionSelector.TryParse(path);
            Assert.IsNotNull(parsed, $"Expected canonical filename: {path}");
            return parsed.Value;
        }

        private static string Name(DateTime utc, bool iso, bool isBeta, int part)
        {
            string prefix = isBeta ? "JournalBeta" : "Journal";
            string session = utc.ToString(iso ? "yyyy-MM-dd'T'HHmmss" : "yyyyMMddHHmmss");
            return $@"C:\Journals\{prefix}.{session}.{part:D2}.log";
        }

        private static Arbitrary<EquivalentIdentityScenario> EquivalentIdentityArbitrary()
        {
            var gen = Gen.Choose(0, 180).SelectMany(offsetMinutes =>
                Gen.Elements(false, true).SelectMany(isBeta =>
                    Gen.Choose(1, 12).Select(part =>
                        new EquivalentIdentityScenario(offsetMinutes, isBeta, part))));

            return Arb.From(gen, ShrinkEquivalentIdentity);
        }

        private static IEnumerable<EquivalentIdentityScenario> ShrinkEquivalentIdentity(
            EquivalentIdentityScenario value)
        {
            if (value.OffsetMinutes != 0)
                yield return new EquivalentIdentityScenario(0, value.IsBeta, value.Part);
            if (value.IsBeta)
                yield return new EquivalentIdentityScenario(value.OffsetMinutes, false, value.Part);
            if (value.Part != 1)
                yield return new EquivalentIdentityScenario(value.OffsetMinutes, value.IsBeta, 1);
        }

        private static Arbitrary<StartupSelectionScenario> StartupSelectionArbitrary()
        {
            var gen = Gen.Choose(1, 180).SelectMany(offsetMinutes =>
                Gen.Elements(false, true).SelectMany(isBeta =>
                    Gen.Choose(1, 4).SelectMany(firstPart =>
                        Gen.Choose(5, 12).Select(secondPart =>
                            new StartupSelectionScenario(
                                offsetMinutes,
                                isBeta,
                                new[] { firstPart, secondPart })))));

            return Arb.From(gen, ShrinkStartupSelection);
        }
        private static IEnumerable<StartupSelectionScenario> ShrinkStartupSelection(
            StartupSelectionScenario value)
        {
            if (value.OffsetMinutes != 1)
                yield return new StartupSelectionScenario(1, value.IsBeta, value.NewerParts);
            if (value.IsBeta)
                yield return new StartupSelectionScenario(value.OffsetMinutes, false, value.NewerParts);
            if (value.NewerParts.Length != 1 || value.NewerParts[0] != 1)
                yield return new StartupSelectionScenario(value.OffsetMinutes, value.IsBeta, new[] { 1 });
        }

        private static Arbitrary<LiveSwitchScenario> LiveSwitchArbitrary()
        {
            var gen = Gen.Choose(0, 2).SelectMany(kind =>
                Gen.Elements(false, true).SelectMany(isBeta =>
                    Gen.Choose(1, 10).Select(part =>
                        new LiveSwitchScenario((LiveSwitchKind)kind, isBeta, part))));

            return Arb.From(gen, ShrinkLiveSwitch);
        }

        private static IEnumerable<LiveSwitchScenario> ShrinkLiveSwitch(LiveSwitchScenario value)
        {
            if (value.Kind != LiveSwitchKind.EquivalentSamePart)
                yield return new LiveSwitchScenario(LiveSwitchKind.EquivalentSamePart, value.IsBeta, value.Part);
            if (value.IsBeta)
                yield return new LiveSwitchScenario(value.Kind, false, value.Part);
            if (value.Part != 1)
                yield return new LiveSwitchScenario(value.Kind, value.IsBeta, 1);
        }

        private sealed class EquivalentIdentityScenario
        {
            public int OffsetMinutes { get; }
            public bool IsBeta { get; }
            public int Part { get; }
            public string CompactFile => Name(BaseUtc.AddMinutes(OffsetMinutes), false, IsBeta, Part);
            public string IsoFile => Name(BaseUtc.AddMinutes(OffsetMinutes), true, IsBeta, Part);

            public EquivalentIdentityScenario(int offsetMinutes, bool isBeta, int part)
            {
                OffsetMinutes = offsetMinutes;
                IsBeta = isBeta;
                Part = part;
            }

            public override string ToString() =>
                $"Files=[{CompactFile}, {IsoFile}]";
        }

        private sealed class StartupSelectionScenario
        {
            public int OffsetMinutes { get; }
            public bool IsBeta { get; }
            public int[] NewerParts { get; }
            public DateTime OlderUtc => BaseUtc;
            public DateTime NewerUtc => BaseUtc.AddMinutes(OffsetMinutes);
            public string OlderFile => Name(OlderUtc, false, IsBeta, 1);
            public string[] ExpectedFiles => NewerParts
                .Distinct()
                .OrderBy(part => part)
                .Select(part => Name(NewerUtc, true, IsBeta, part))
                .ToArray();
            public string[] Files => new[] { OlderFile }
                .Concat(ExpectedFiles.Reverse())
                .ToArray();

            public StartupSelectionScenario(int offsetMinutes, bool isBeta, int[] newerParts)
            {
                OffsetMinutes = offsetMinutes;
                IsBeta = isBeta;
                NewerParts = newerParts;
            }

            public override string ToString() =>
                $"Files=[{string.Join(", ", Files)}]; Expected=[{string.Join(", ", ExpectedFiles)}]";
        }
        private enum LiveSwitchKind
        {
            EquivalentSamePart,
            EquivalentHigherPart,
            OlderNewlyCreatedSession
        }

        private sealed class LiveSwitchScenario
        {
            public LiveSwitchKind Kind { get; }
            public bool IsBeta { get; }
            public int Part { get; }
            public int CurrentPart => Part;
            public bool UsesMixedForms => true;

            public string CurrentFile => Kind switch
            {
                LiveSwitchKind.EquivalentSamePart => Name(BaseUtc, true, IsBeta, Part),
                LiveSwitchKind.EquivalentHigherPart => Name(BaseUtc, false, IsBeta, Part),
                _ => Name(BaseUtc.AddMinutes(1), true, IsBeta, Part)
            };

            public string NewFile => Kind switch
            {
                LiveSwitchKind.EquivalentSamePart => Name(BaseUtc, false, IsBeta, Part),
                LiveSwitchKind.EquivalentHigherPart => Name(BaseUtc, true, IsBeta, Part + 1),
                _ => Name(BaseUtc, false, IsBeta, Part + 1)
            };

            public bool ExpectedSwitch => Kind == LiveSwitchKind.EquivalentHigherPart;

            public LiveSwitchScenario(LiveSwitchKind kind, bool isBeta, int part)
            {
                Kind = kind;
                IsBeta = isBeta;
                Part = part;
            }

            public override string ToString() =>
                $"Kind={Kind}; Files=[{CurrentFile}, {NewFile}]; ExpectedSwitch={ExpectedSwitch}";
        }
    }
}
