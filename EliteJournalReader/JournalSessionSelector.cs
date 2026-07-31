using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EliteJournalReader
{
    /// <summary>
    /// Selects journal files for session reconstruction using deterministic parsed-name logic.
    /// Replaces the old metadata-first selection that could mix sessions or order parts lexically.
    ///
    /// Rules:
    /// 1. Parse canonical Journal[Beta].&lt;session&gt;.&lt;part&gt;.log filenames.
    /// 2. Group by (beta marker + session identity).
    /// 3. Select the greatest parsed session.
    /// 4. Order that session's parts by numeric part number, then normalized filename as tie-breaker.
    /// 5. When no canonical files exist, fall back to exactly one legacy file by last-write UTC,
    ///    then normalized filename ordinal. Never mix legacy and canonical files.
    /// </summary>
    internal sealed class JournalSessionSelector
    {
        /// <summary>
        /// Matches canonical journal filenames:
        ///   Journal.20260407173045.01.log      (compact timestamp)
        ///   Journal.2026-04-07T170000.01.log   (ISO-ish timestamp)
        ///   JournalBeta.20260407173045.01.log  (beta variant)
        ///
        /// Capture groups:
        ///   beta      - "Beta" or empty
        ///   session   - the session timestamp string (various formats)
        ///   part      - integer part number
        /// </summary>
        private static readonly Regex CanonicalPattern = new Regex(
            @"^Journal(?<beta>Beta)?\.(?<session>[0-9T\-]+)\.(?<part>\d+)\.log$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Represents a parsed canonical journal filename.
        /// </summary>
        internal readonly struct ParsedJournalFile : IComparable<ParsedJournalFile>
        {
            public string FullPath { get; }
            public string Filename { get; }
            public bool IsBeta { get; }
            public string SessionIdentity { get; }
            public int PartNumber { get; }

            /// <summary>
            /// A normalized session key combining beta marker and session identity.
            /// Used for grouping files that belong to the same session.
            /// </summary>
            public string SessionKey => (IsBeta ? "Beta:" : "") + SessionIdentity;

            public ParsedJournalFile(string fullPath, string filename, bool isBeta, string sessionIdentity, int partNumber)
            {
                FullPath = fullPath;
                Filename = filename;
                IsBeta = isBeta;
                SessionIdentity = sessionIdentity;
                PartNumber = partNumber;
            }

            /// <summary>
            /// Compares by session identity descending, then part number ascending.
            /// Session identity is compared ordinally (which works for both timestamp formats).
            /// </summary>
            public int CompareTo(ParsedJournalFile other)
            {
                int sessionCmp = StringComparer.OrdinalIgnoreCase.Compare(SessionIdentity, other.SessionIdentity);
                if (sessionCmp != 0) return sessionCmp;
                int partCmp = PartNumber.CompareTo(other.PartNumber);
                if (partCmp != 0) return partCmp;
                return StringComparer.OrdinalIgnoreCase.Compare(Filename, other.Filename);
            }
        }

        /// <summary>
        /// Attempts to parse a filename into a canonical journal file descriptor.
        /// Returns null if the filename does not match the canonical pattern.
        /// </summary>
        internal static ParsedJournalFile? TryParse(string fullPath)
        {
            string filename = Path.GetFileName(fullPath);
            var match = CanonicalPattern.Match(filename);
            if (!match.Success)
                return null;

            bool isBeta = !string.IsNullOrEmpty(match.Groups["beta"].Value);
            string session = match.Groups["session"].Value;

            if (!int.TryParse(match.Groups["part"].Value, out int partNumber))
                return null;

            return new ParsedJournalFile(fullPath, filename, isBeta, session, partNumber);
        }

        /// <summary>
        /// Selects journal files for previous-session reconstruction.
        /// Returns the files in the order they should be processed (earliest part first).
        /// </summary>
        /// <param name="filePaths">All journal file paths in the directory.</param>
        /// <param name="getLastWriteUtc">Function to get a file's last-write UTC time.</param>
        /// <returns>Ordered list of file paths belonging to the selected session.</returns>
        public IReadOnlyList<string> SelectSessionFiles(IEnumerable<string> filePaths, Func<string, DateTime> getLastWriteUtc)
        {
            var allFiles = filePaths.ToList();
            if (allFiles.Count == 0)
                return Array.Empty<string>();

            // Attempt to parse all files as canonical
            var parsed = new List<ParsedJournalFile>();
            var legacyFiles = new List<string>();

            foreach (var path in allFiles)
            {
                var result = TryParse(path);
                if (result.HasValue)
                    parsed.Add(result.Value);
                else
                    legacyFiles.Add(path);
            }

            // If we have canonical files, use them exclusively (never mix with legacy)
            if (parsed.Count > 0)
                return SelectCanonicalSession(parsed);

            // Legacy fallback: select exactly one file
            return SelectLegacyFile(legacyFiles, getLastWriteUtc);
        }

        /// <summary>
        /// For live mode: determines whether a new file should cause a session switch.
        /// Returns true only if the new file belongs to a newer session or is a higher
        /// part of the currently selected session.
        /// </summary>
        /// <param name="currentSessionKey">The session key of the currently watched session.</param>
        /// <param name="currentMaxPart">The highest part number currently known in the session.</param>
        /// <param name="newFilePath">The path of the newly detected file.</param>
        /// <returns>True if the watcher should switch to the new file.</returns>
        public bool ShouldSwitchToFile(string currentSessionKey, int currentMaxPart, string newFilePath)
        {
            var parsed = TryParse(newFilePath);
            if (!parsed.HasValue)
                return false; // Cannot parse — ignore in live mode

            var newFile = parsed.Value;

            // Switch to a newer session
            int sessionCmp = StringComparer.OrdinalIgnoreCase.Compare(newFile.SessionKey, currentSessionKey);
            if (sessionCmp > 0)
                return true;

            // Same session: switch only if higher part number
            if (sessionCmp == 0 && newFile.PartNumber > currentMaxPart)
                return true;

            return false;
        }

        /// <summary>
        /// Selects the greatest canonical session and returns its parts ordered numerically.
        /// </summary>
        private static IReadOnlyList<string> SelectCanonicalSession(List<ParsedJournalFile> parsed)
        {
            // Group by session key (beta + session identity)
            var groups = parsed.GroupBy(f => f.SessionKey, StringComparer.OrdinalIgnoreCase);

            // Find the greatest session: compare session identity ordinally (timestamp strings sort correctly)
            string greatestSessionKey = null;
            string greatestSessionIdentity = null;
            foreach (var group in groups)
            {
                var representative = group.First();
                if (greatestSessionIdentity == null ||
                    StringComparer.OrdinalIgnoreCase.Compare(representative.SessionIdentity, greatestSessionIdentity) > 0)
                {
                    greatestSessionKey = group.Key;
                    greatestSessionIdentity = representative.SessionIdentity;
                }
            }

#if DEBUG
            Trace.TraceInformation($"JournalSessionSelector: selected canonical session '{greatestSessionKey}' from {parsed.Count} file(s).");
#endif

            // Select only files from the greatest session, ordered by numeric part then filename
            var sessionFiles = parsed
                .Where(f => StringComparer.OrdinalIgnoreCase.Equals(f.SessionKey, greatestSessionKey))
                .OrderBy(f => f.PartNumber)
                .ThenBy(f => f.Filename, StringComparer.OrdinalIgnoreCase)
                .Select(f => f.FullPath)
                .ToList();

#if DEBUG
            Trace.TraceInformation($"JournalSessionSelector: session has {sessionFiles.Count} part(s): {string.Join(", ", sessionFiles.Select(Path.GetFileName))}");
#endif

            return sessionFiles;
        }

        /// <summary>
        /// Legacy fallback: select exactly one file by last-write UTC descending,
        /// then normalized filename as ordinal tie-breaker.
        /// Never returns more than one file; never mixes with canonical files.
        /// </summary>
        private static IReadOnlyList<string> SelectLegacyFile(List<string> legacyFiles, Func<string, DateTime> getLastWriteUtc)
        {
            if (legacyFiles.Count == 0)
                return Array.Empty<string>();

            // Order by last-write UTC descending, then by filename ordinal ascending as tie-breaker
            var selected = legacyFiles
                .OrderByDescending(f => getLastWriteUtc(f))
                .ThenBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .First();

#if DEBUG
            Trace.TraceInformation($"JournalSessionSelector: legacy fallback selected '{Path.GetFileName(selected)}'.");
#endif

            return new[] { selected };
        }
    }
}
