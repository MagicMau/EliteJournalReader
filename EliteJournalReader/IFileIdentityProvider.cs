using System;
using System.IO;

namespace EliteJournalReader
{
    /// <summary>
    /// Represents a stable identity for a file that survives renames but changes on replacement.
    /// Used to detect truncation or in-place replacement of watched journal files.
    /// </summary>
    public readonly struct FileIdentity : IEquatable<FileIdentity>
    {
        /// <summary>
        /// Volume serial number (Windows) or device ID (metadata fallback).
        /// </summary>
        public long VolumeId { get; }

        /// <summary>
        /// File index on the volume (Windows) or a composite of creation time + path hash (metadata fallback).
        /// </summary>
        public long FileId { get; }

        public FileIdentity(long volumeId, long fileId)
        {
            VolumeId = volumeId;
            FileId = fileId;
        }

        public bool Equals(FileIdentity other) => VolumeId == other.VolumeId && FileId == other.FileId;
        public override bool Equals(object obj) => obj is FileIdentity other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(VolumeId, FileId);
        public static bool operator ==(FileIdentity left, FileIdentity right) => left.Equals(right);
        public static bool operator !=(FileIdentity left, FileIdentity right) => !left.Equals(right);

        public override string ToString() => $"FileIdentity(Vol={VolumeId}, Id={FileId})";
    }

    /// <summary>
    /// Injectable provider for stable file identity. Enables testing and platform-specific implementations.
    /// 
    /// On Windows, use GetFileInformationByHandle to obtain VolumeSerialNumber + FileIndex for a stable
    /// identity that survives renames but changes when a file is deleted and recreated at the same path.
    /// 
    /// On unsupported platforms (or as a documented fallback), use file metadata:
    /// creation time combined with the file path hash provides a reasonable approximation.
    /// The metadata fallback may produce false positives on systems that don't preserve creation time
    /// accurately, but for the journal watcher use case (detecting truncation/replacement) it is sufficient
    /// because a replaced file will almost always have a different creation time.
    /// </summary>
    public interface IFileIdentityProvider
    {
        /// <summary>
        /// Gets the identity of a file from an open file stream handle.
        /// Returns null if identity cannot be determined.
        /// </summary>
        FileIdentity? GetIdentity(FileStream stream);

        /// <summary>
        /// Gets the identity of a file by path (opens the file briefly if needed).
        /// Returns null if identity cannot be determined.
        /// </summary>
        FileIdentity? GetIdentity(string filePath);
    }
}
