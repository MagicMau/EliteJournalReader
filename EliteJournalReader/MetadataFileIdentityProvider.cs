using System;
using System.Diagnostics;
using System.IO;

namespace EliteJournalReader
{
    /// <summary>
    /// Documented metadata fallback implementation of <see cref="IFileIdentityProvider"/>.
    /// Used on platforms where volume/file identity from the open handle is unavailable.
    /// 
    /// This fallback uses the file's creation time (as ticks) combined with a hash of the
    /// full file path to approximate stable identity. It detects replacement because:
    /// - A deleted-and-recreated file at the same path will have a new creation time.
    /// - A truncated file keeps the same creation time (detected separately via length check).
    /// 
    /// Limitations:
    /// - Systems with low-resolution file timestamps may not detect rapid replace cycles.
    /// - If creation time is not preserved by the filesystem, this may produce false positives.
    /// - Unlike the Windows provider, this cannot distinguish hardlinks or detect renames.
    /// For the journal watcher use case these limitations are acceptable because the primary
    /// detection is length-based (truncation) and replacement almost always changes creation time.
    /// </summary>
    internal sealed class MetadataFileIdentityProvider : IFileIdentityProvider
    {
        public FileIdentity? GetIdentity(FileStream stream)
        {
            if (stream == null || string.IsNullOrEmpty(stream.Name))
                return null;

            return GetIdentity(stream.Name);
        }

        public FileIdentity? GetIdentity(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            try
            {
                if (!File.Exists(filePath))
                    return null;

                var info = new FileInfo(filePath);
                // Use creation time ticks as the "volume" component (approximation)
                long creationTicks = info.CreationTimeUtc.Ticks;
                // Use a stable hash of the full path as the "file index" component
                long pathHash = GetStablePathHash(info.FullName);

                return new FileIdentity(creationTicks, pathHash);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Failed to get file metadata identity for '{filePath}': {ex.Message}");
                Trace.TraceInformation(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Produces a stable hash for the given path using case-insensitive comparison
        /// to match typical Windows filesystem behavior.
        /// </summary>
        private static long GetStablePathHash(string fullPath)
        {
            // Use ordinal ignore-case hash for Windows-style paths
            return (long)fullPath.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
