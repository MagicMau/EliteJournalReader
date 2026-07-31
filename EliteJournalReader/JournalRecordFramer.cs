using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EliteJournalReader
{
    /// <summary>
    /// Byte-level newline framing cursor for journal files.
    /// Tracks committed offset (last confirmed newline boundary), read offset (current stream position),
    /// and pending UTF-8 bytes that have not yet been terminated by a newline.
    /// Only dispatches complete newline-terminated records and never commits partial data.
    /// </summary>
    internal sealed class JournalRecordFramer
    {
        private const int ReadBufferSize = 4096;
        private const byte LF = (byte)'\n';
        private const byte CR = (byte)'\r';

        // UTF-8 BOM bytes: EF BB BF
        private static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };

        private readonly byte[] _readBuffer = new byte[ReadBufferSize];

        /// <summary>
        /// Bytes that have been read but not yet terminated by a newline.
        /// These remain uncommitted between reads.
        /// </summary>
        private byte[] _pendingBytes = Array.Empty<byte>();

        /// <summary>
        /// Byte position immediately after the last terminating newline whose record has been dispatched.
        /// Recovery restarts from this position.
        /// </summary>
        public long CommittedOffset { get; private set; }

        /// <summary>
        /// Transient byte position through bytes already loaded, including incomplete suffix.
        /// </summary>
        public long ReadOffset { get; private set; }

        /// <summary>
        /// Whether BOM detection has already been performed (only relevant at byte zero).
        /// </summary>
        private bool _bomChecked;

        /// <summary>
        /// Initializes the framer at the given starting offset.
        /// </summary>
        public JournalRecordFramer(long startOffset)
        {
            CommittedOffset = startOffset;
            ReadOffset = startOffset;
            _bomChecked = startOffset > 0;
        }

        /// <summary>
        /// Reads available bytes from the stream starting at ReadOffset, extracts all complete
        /// newline-terminated records, and returns them as decoded strings.
        /// The stream must be seekable and opened with at least FileAccess.Read and FileShare.ReadWrite.
        /// Committed offset advances only through the last consumed newline.
        /// Incomplete trailing bytes are retained internally for the next call.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <returns>Array of complete decoded lines (without line terminators).</returns>
        public string[] ReadCompleteRecords(FileStream stream)
        {
            if (stream.Length <= ReadOffset && _pendingBytes.Length == 0)
                return Array.Empty<string>();

            // Seek to current read position
            stream.Seek(ReadOffset, SeekOrigin.Begin);

            // Read all available bytes from the current position
            var allNewBytes = ReadAllAvailableBytes(stream);
            if (allNewBytes.Length == 0 && _pendingBytes.Length == 0)
                return Array.Empty<string>();

            // Combine pending bytes with newly read bytes
            byte[] workingBytes;
            if (_pendingBytes.Length > 0)
            {
                workingBytes = new byte[_pendingBytes.Length + allNewBytes.Length];
                Buffer.BlockCopy(_pendingBytes, 0, workingBytes, 0, _pendingBytes.Length);
                Buffer.BlockCopy(allNewBytes, 0, workingBytes, _pendingBytes.Length, allNewBytes.Length);
            }
            else
            {
                workingBytes = allNewBytes;
            }

            // Handle BOM only at byte zero
            int dataStart = 0;
            if (!_bomChecked)
            {
                _bomChecked = true;
                if (workingBytes.Length >= 3 &&
                    workingBytes[0] == Utf8Bom[0] &&
                    workingBytes[1] == Utf8Bom[1] &&
                    workingBytes[2] == Utf8Bom[2])
                {
                    dataStart = 3;
                }
            }

            // Scan for newline-terminated records
            var completedLines = new System.Collections.Generic.List<string>();
            int lastNewlineEnd = dataStart; // index in workingBytes after last consumed newline

            for (int i = dataStart; i < workingBytes.Length; i++)
            {
                if (workingBytes[i] == LF)
                {
                    // Found a newline - extract the record
                    int lineEnd = i;

                    // Strip optional preceding CR
                    if (lineEnd > lastNewlineEnd && workingBytes[lineEnd - 1] == CR)
                        lineEnd--;

                    int lineLength = lineEnd - lastNewlineEnd;
                    if (lineLength > 0)
                    {
                        string line = Encoding.UTF8.GetString(workingBytes, lastNewlineEnd, lineLength);
                        completedLines.Add(line);
                    }

                    // Move past the LF
                    lastNewlineEnd = i + 1;
                }
            }

            // Calculate how many new bytes (beyond pending) contributed to completed records
            // The committed offset advances only through the last consumed newline
            if (lastNewlineEnd > dataStart && completedLines.Count > 0)
            {
                // Calculate how far into the new bytes the last newline was
                // pendingBytes contributed [0.._pendingBytes.Length) of workingBytes
                // newBytes contributed [_pendingBytes.Length..workingBytes.Length) of workingBytes
                long bytesConsumedFromTotal = lastNewlineEnd;
                long pendingBytesUsed = _pendingBytes.Length;

                // The committed offset should advance to account for consumed bytes
                // from the original ReadOffset minus the pending bytes that were already tracked
                long newCommittedOffset = ReadOffset - pendingBytesUsed + bytesConsumedFromTotal;

                // Account for BOM if it was at the start and part of the first read
                if (CommittedOffset == 0 && dataStart > 0)
                {
                    // BOM was skipped, but it's still in the file bytes
                    // newCommittedOffset already accounts for dataStart through lastNewlineEnd
                }

                CommittedOffset = newCommittedOffset;
            }

            // Retain incomplete suffix as pending bytes
            int remainingLength = workingBytes.Length - lastNewlineEnd;
            if (remainingLength > 0)
            {
                _pendingBytes = new byte[remainingLength];
                Buffer.BlockCopy(workingBytes, lastNewlineEnd, _pendingBytes, 0, remainingLength);
            }
            else
            {
                _pendingBytes = Array.Empty<byte>();
            }

            // Update ReadOffset to current stream position
            ReadOffset = stream.Position;

            return completedLines.ToArray();
        }

        /// <summary>
        /// Reads all currently available bytes from the stream into a contiguous array.
        /// </summary>
        private byte[] ReadAllAvailableBytes(FileStream stream)
        {
            using var ms = new MemoryStream();
            int bytesRead;
            while ((bytesRead = stream.Read(_readBuffer, 0, _readBuffer.Length)) > 0)
            {
                ms.Write(_readBuffer, 0, bytesRead);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Resets all framing state. Used when file identity changes or truncation is detected.
        /// </summary>
        public void Reset()
        {
            CommittedOffset = 0;
            ReadOffset = 0;
            _pendingBytes = Array.Empty<byte>();
            _bomChecked = false;
        }

        /// <summary>
        /// Returns true if there are pending bytes that have not been committed
        /// (i.e., bytes after the last newline that haven't been terminated yet).
        /// </summary>
        public bool HasPendingBytes => _pendingBytes.Length > 0;
    }
}
