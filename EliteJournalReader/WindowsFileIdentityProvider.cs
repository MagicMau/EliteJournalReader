using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace EliteJournalReader
{
    /// <summary>
    /// Windows implementation of <see cref="IFileIdentityProvider"/>.
    /// Uses GetFileInformationByHandle to obtain VolumeSerialNumber and FileIndex,
    /// providing a stable file identity that survives renames but changes when a file
    /// is deleted and recreated at the same path.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class WindowsFileIdentityProvider : IFileIdentityProvider
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        public FileIdentity? GetIdentity(FileStream stream)
        {
            if (stream == null || stream.SafeFileHandle == null || stream.SafeFileHandle.IsInvalid)
                return null;

            try
            {
                if (GetFileInformationByHandle(stream.SafeFileHandle, out var info))
                {
                    long volumeId = info.VolumeSerialNumber;
                    long fileId = ((long)info.FileIndexHigh << 32) | info.FileIndexLow;
                    return new FileIdentity(volumeId, fileId);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Failed to get file identity from handle: {ex.Message}");
                Trace.TraceInformation(ex.StackTrace);
            }

            return null;
        }

        public FileIdentity? GetIdentity(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return GetIdentity(stream);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Failed to get file identity for path '{filePath}': {ex.Message}");
                Trace.TraceInformation(ex.StackTrace);
                return null;
            }
        }
    }
}
