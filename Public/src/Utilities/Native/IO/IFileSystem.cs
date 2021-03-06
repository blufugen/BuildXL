// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using BuildXL.Native.IO.Windows;
using BuildXL.Utilities;
using Microsoft.Win32.SafeHandles;

namespace BuildXL.Native.IO
{
    /// <summary>
    /// Interface for file system related functions and calls
    /// </summary>
    public interface IFileSystem
    {
        #region Directory related functions

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <remarks>
        /// Supports paths beyond MAX_PATH.
        /// The provided path is expected canonicalized even without a long-path prefix.
        /// </remarks>
        void CreateDirectory(string directoryPath);

        /// <summary>
        /// Tries to open a directory handle.
        /// </summary>
        /// <remarks>
        /// The returned handle is suitable for operations such as <see cref="Windows.FileSystemWin.GetVolumeInformationByHandleW"/> but not wrapping in a <see cref="FileStream"/>.
        /// <see cref="FileDesiredAccess.Synchronize"/> is added implciitly.
        /// This function does not throw for any failure of CreateFileW.
        /// </remarks>
        OpenFileResult TryOpenDirectory(
            string directoryPath,
            FileDesiredAccess desiredAccess,
            FileShare shareMode,
            FileFlagsAndAttributes flagsAndAttributes,
            out SafeFileHandle handle);

        /// <summary>
        /// Tries to open a directory handle.
        /// </summary>
        /// <remarks>
        /// The returned handle is suitable for operations such as <see cref="Windows.FileSystemWin.GetVolumeInformationByHandleW"/> but not wrapping in a <see cref="FileStream"/>.
        /// <see cref="FileDesiredAccess.Synchronize"/> is added implciitly.
        /// This function does not throw for any failure of CreateFileW.
        /// </remarks>
        OpenFileResult TryOpenDirectory(string directoryPath, FileShare shareMode, out SafeFileHandle handle);

        /// <summary>
        /// Enumerates all the names and attributes of entries in the given directory. See <see cref="EnumerateDirectoryEntries(string,bool,System.Action{string,string,System.IO.FileAttributes})"/>
        /// </summary>
        EnumerateDirectoryResult EnumerateDirectoryEntries(
            string directoryPath,
            bool recursive,
            Action<string, string, FileAttributes> handleEntry);

        /// <summary>
        /// Enumerates the names and attributes of entries in the given directory using a search pattern.
        /// </summary>
        /// <remarks>
        /// Supports paths beyond MAX_PATH.
        /// The provided path is expected canonicalized even without a long-path prefix.
        /// </remarks>
        EnumerateDirectoryResult EnumerateDirectoryEntries(
            string directoryPath,
            bool recursive,
            string pattern,
            Action<string, string, FileAttributes> handleEntry);

        /// <summary>
        /// Enumerates the names and attributes of entries in the given directory.
        /// </summary>
        /// <remarks>
        /// Supports paths beyond MAX_PATH.
        /// The provided path is expected canonicalized even without a long-path prefix.
        /// </remarks>
        EnumerateDirectoryResult EnumerateDirectoryEntries(string directoryPath, Action<string, FileAttributes> handleEntry);

        /// <summary>
        /// Enumerates the names and attributes of entries in the given directory using a search pattern and accumulates the
        /// results in a See <see cref="IDirectoryEntriesAccumulator" />
        /// </summary>
        EnumerateDirectoryResult EnumerateDirectoryEntries(
            string directoryPath,
            bool enumerateDirectory,
            string pattern,
            uint directoriesToSkipRecursively,
            bool recursive,
            IDirectoryEntriesAccumulator accumulators);

        /// <summary>
        /// Removes a directory
        /// </summary>
        /// <remarks>
        /// This calls the native RemoveDirectory function which only marks the directory for deletion on close, so it
        /// may not be deleted if there are other open handles
        /// Supports paths beyond MAX_PATH if prefix is added
        /// </remarks>
        void RemoveDirectory(string path);

        /// <summary>
        /// Removes a directory
        /// </summary>
        /// <remarks>
        /// This calls the native RemoveDirectory function which only marks the directory for deletion on close, so it
        /// may not be deleted if there are other open handles
        /// Supports paths beyond MAX_PATH if prefix is added
        /// </remarks>
        bool TryRemoveDirectory(string path, out int hr);

        /// <summary>
        /// Attempts to set 'delete' disposition on the given handle, such that its directory entry is unlinked when all remaining handles are closed.
        /// </summary>
        bool TrySetDeletionDisposition(SafeFileHandle handle);

        #endregion

        #region File related functions

        /// <summary>
        /// Tries to open a file handle (for a new or existing file).
        /// </summary>
        /// <remarks>
        /// This is a thin wrapper for <c>CreateFileW</c>.
        /// Note that unlike other managed wrappers, this does not throw exceptions.
        /// Supports paths greater than MAX_PATH if the appropriate prefix is used
        /// </remarks>
        OpenFileResult TryCreateOrOpenFile(
            string path,
            FileDesiredAccess desiredAccess,
            FileShare shareMode,
            FileMode creationDisposition,
            FileFlagsAndAttributes flagsAndAttributes,
            out SafeFileHandle handle);

        /// <summary>
        /// Tries to open a file via its <see cref="FileId"/>. The <paramref name="existingHandleOnVolume"/> can be any handle on the same volume
        /// (file IDs are unique only per volume).
        /// </summary>
        /// <remarks>
        /// This function does not throw for any failure of <c>OpenFileById</c>.
        /// </remarks>
        OpenFileResult TryOpenFileById(
            SafeFileHandle existingHandleOnVolume,
            FileId fileId,
            FileDesiredAccess desiredAccess,
            FileShare shareMode,
            FileFlagsAndAttributes flagsAndAttributes,
            out SafeFileHandle handle);

        /// <summary>
        /// Tries to open a new file handle (with new access, share mode, and flags) via an existing handle to that file.
        /// </summary>
        /// <remarks>
        /// Wrapper for <c>ReOpenFile</c>.
        /// </remarks>
        ReOpenFileStatus TryReOpenFile(
            SafeFileHandle existing,
            FileDesiredAccess desiredAccess,
            FileShare shareMode,
            FileFlagsAndAttributes flagsAndAttributes,
            out SafeFileHandle reopenedHandle);

        /// <summary>
        /// Creates file stream.
        /// </summary>
        FileStream CreateFileStream(
            string path,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            FileOptions options,
            bool force);

        /// <summary>
        /// Attempts to delete file using posix semantics. Note: this function requires Win10 RS2 to run successfully.
        /// Otherwise the API call will just fail. Returns true if the deletion was successful.
        /// </summary>
        bool TryPosixDelete(string pathToDelete, out OpenFileResult openFileResult);

        /// <summary>
        /// Attempts to rename a file (via a handle) to its destination. The handle must have been opened with DELETE access.
        /// </summary>
        bool TryRename(SafeFileHandle handle, string destination, bool replaceExisting);

        /// <summary>
        /// Gets the <see cref="FileFlagsAndAttributes"/> for opening a directory.
        /// </summary>
        FileFlagsAndAttributes GetFileFlagsAndAttributesForPossibleReparsePoint(string expandedPath);

        /// <summary>
        /// Thin wrapper for native GetFileAttributesW that throws an exception on failure
        /// </summary>
        /// <remarks>
        /// Supports paths greater than MAX_PATH.
        /// </remarks>
        FileAttributes GetFileAttributes(string path);

        /// <summary>
        /// Calls GetFileInformationByHandleEx on the given file handle to retrieve its attributes. This requires 'READ ATTRIBUTES' access on the handle.
        /// </summary>
        FileAttributes GetFileAttributesByHandle(SafeFileHandle fileHandle);

        /// <summary>
        /// Thin wrapper for native SetFileAttributesW that throws an exception on failure
        /// </summary>
        /// <remarks>
        /// Supports paths greater than MAX_PATH.
        /// </remarks>
        void SetFileAttributes(string path, FileAttributes attributes);

        #endregion

        #region Soft- (Junction) and Hardlink functions

        /// <summary>
        /// Creates junction.
        /// A junction is essentially a softlink (a.k.a. symlink) between directories
        /// So, we would expect deleting that directory would make the junction point to missing data
        /// </summary>
        /// <param name="junctionPoint">Junction name.</param>
        /// <param name="targetDir">Target directory.</param>
        void CreateJunction(string junctionPoint, string targetDir);

        /// <summary>
        /// Tries to create a symbolic link.
        /// </summary>
        bool TryCreateSymbolicLink(string symLinkFileName, string targetFileName, bool isTargetFile);

        /// <summary>
        /// Tries to create a hardlink to the given file. The destination must not exist.
        /// </summary>
        CreateHardLinkStatus TryCreateHardLink(string link, string linkTarget);

        /// <summary>
        /// Tries to create a hardlink to the given file via the SetInformationFile API. This is slightly
        /// different than calling CreateHardLinkW since it allows a link to be created on a linkTarget that
        /// is not writeable
        /// </summary>
        CreateHardLinkStatus TryCreateHardLinkViaSetInformationFile(string link, string linkTarget, bool replaceExisting = true);

        /// <summary>
        /// Gets hard link count by handle.
        /// </summary>
        uint GetHardLinkCountByHandle(SafeFileHandle handle);

        /// <summary>
        /// Returns whether the reparse point type is actionable, i.e., a mount point or a symlink.
        /// </summary>
        /// <param name="reparsePointType">The type of the reparse point.</param>
        /// <returns>true if this is an actionable reparse point, otherwise false.</returns>
        bool IsReparsePointActionable(ReparsePointType reparsePointType);

        /// <summary>
        /// Returns <see cref="ReparsePointType"/> of a path.
        /// </summary>
        /// <param name="path">Path to check for reparse point.</param>
        /// <returns>The type of the reparse point.</returns>
        Possible<ReparsePointType> TryGetReparsePointType(string path);


        /// <summary>
        /// Whether <paramref name="path"/> is a WCI reparse point
        /// </summary>
        /// <remarks>
        /// The case of WCI reparse points is handled by this specific function (as opposed to
        /// including this functionality in <see cref="TryGetReparsePointType(string)"/>) since
        /// there is a slightly increased perf cost compared to the regular file attribute check
        /// </remarks>
        bool IsWciReparsePoint(string path);

        /// <summary>
        /// Gets chain of reparse points.
        /// </summary>
        /// <param name="handle">File handle.</param>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="chainOfReparsePoints">List representing chain of reparse points.</param>
        /// <remarks>
        /// The list <paramref name="chainOfReparsePoints"/> includes the source path.
        /// </remarks>
        void GetChainOfReparsePoints(SafeFileHandle handle, string sourcePath, IList<string> chainOfReparsePoints);

        /// <summary>
        /// Tries to get reparse point target.
        /// </summary>
        /// <param name="handle">File handle.</param>
        /// <param name="sourcePath">Source path.</param>
        /// <returns>Reparse point target if successful.</returns>
        Possible<string> TryGetReparsePointTarget(SafeFileHandle handle, string sourcePath);

        #endregion

        #region Journaling functions

        /// <summary>
        /// Calls FSCTL_READ_FILE_USN_DATA on the given file handle. This returns a scrubbed USN record that contains all fields other than
        /// TimeStamp, Reason, and SourceInfo. If the volume's journal is disabled or the file has not been touched since journal creation,
        /// the USN field of the record will be 0.
        /// </summary>
        /// <remarks>
        /// <paramref name="forceJournalVersion2"/> results in requesting a USN_RECORD_V2 result (even on 8.1+, which supports USN_RECORD_V3).
        /// This allows testing V2 marshaling when not running a downlevel OS.
        /// </remarks>
        MiniUsnRecord? ReadFileUsnByHandle(SafeFileHandle fileHandle, bool forceJournalVersion2 = false);

        /// <summary>
        /// Calls FSCTL_READ_USN_JOURNAL on the given volume handle.
        /// </summary>
        /// <remarks>
        /// <paramref name="forceJournalVersion2"/> results in requesting a USN_RECORD_V2 result (even on 8.1+, which supports USN_RECORD_V3).
        /// This allows testing V2 marshaling when not running a downlevel OS.
        /// <paramref name="buffer"/> is a caller-provided buffer (which does not need to be pinned). The contents of the buffer are undefined (the
        /// purpose of the buffer parameter is to allow pooling / re-using buffers across repeated journal reads).
        /// </remarks>
        ReadUsnJournalResult TryReadUsnJournal(
            SafeFileHandle volumeHandle,
            byte[] buffer,
            ulong journalId,
            Usn startUsn = default(Usn),
            bool forceJournalVersion2 = false,
            bool isJournalUnprivileged = false);

        /// <summary>
        /// Calls FSCTL_QUERY_USN_JOURNAL on the given volume handle.
        /// </summary>
        QueryUsnJournalResult TryQueryUsnJournal(SafeFileHandle volumeHandle);

        /// <summary>
        /// Calls FSCTL_WRITE_USN_CLOSE_RECORD on the given file handle, and returns the new USN (not a full record). The new USN corresponds to
        /// a newly-written 'close' record, meaning that any not-yet-checkpointed (deferred) change reasons have been flushed.
        /// If writing the close record fails due to the volume's journal being disabled, null is returned.
        /// </summary>
        Usn? TryWriteUsnCloseRecordByHandle(SafeFileHandle fileHandle);

        #endregion

        #region Volume handling functions

        /// <summary>
        /// Enumerates volumes on the system and, for those accessible, returns a pair of (volume guid path, serial)
        /// </summary>
        /// <remarks>
        /// The volume guid path ends in a trailing slash, so it is openable as a directory (the volume root).
        /// The serial is the same as defined by <see cref="GetVolumeSerialNumberByHandle"/> for any file on the volume;
        /// note that the top 32 bits may be insignificant if long serials cannot be retireved on this platform.
        /// </remarks>
        List<Tuple<VolumeGuidPath, ulong>> ListVolumeGuidPathsAndSerials();

        /// <summary>
        /// Returns a 64-bit volume serial number for the volume containing the given file when possible.
        /// If retrieving a 64-bit serial is not supported on this platform, this returns a synthesized one via
        /// sign-extending the 32-bit short serial (<see cref="GetShortVolumeSerialNumberByHandle"/>).
        /// </summary>
        FileSystemType GetVolumeFileSystemByHandle(SafeFileHandle fileHandle);

        /// <summary>
        /// Returns a 32-bit volume serial number for the volume containing the given file.
        /// </summary>
        /// <remarks>
        /// This is the short serial number as seen in 'dir', whereas <see cref="TryGetFileIdAndVolumeIdByHandle" /> returns a
        /// longer (64-bit) serial (this short serial should be in its low bits).
        /// </remarks>
        uint GetShortVolumeSerialNumberByHandle(SafeFileHandle fileHandle);

        /// <summary>
        /// Returns a 64-bit volume serial number for the volume containing the given file when possible.
        /// If retrieving a 64-bit serial is not supported on this platform, this returns a synthesized one via
        /// sign-extending the 32-bit short serial (<see cref="GetShortVolumeSerialNumberByHandle"/>).
        /// </summary>
        /// <remarks>
        /// This picks between <see cref="TryGetFileIdAndVolumeIdByHandle" /> (if available) and <see cref="GetShortVolumeSerialNumberByHandle"/>.
        /// </remarks>
        ulong GetVolumeSerialNumberByHandle(SafeFileHandle fileHandle);

        /// <summary>
        /// Calls GetFileInformationByHandleEx on the given file handle to retrieve its file ID and volume ID. Those two IDs together uniquely identify a file.
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/hh802691(v=vs.85).aspx
        /// </summary>
        /// <remarks>
        /// The needed API was added in Windows 8.1 / Server 2012R2. If this function returns null, we will call <see cref="GetVolumeSerialNumberByHandle" />
        /// Note that even if the API is supported, the underlying volume for the given handle may not; only in that case, this function returns <c>null</c>.
        /// </remarks>
        FileIdAndVolumeId? TryGetFileIdAndVolumeIdByHandle(SafeFileHandle fileHandle);

        /// <summary>
        /// Whether <paramref name="volume"/> is a virtual drive (mapped to an actual drive)
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        bool IsVolumeMapped(string volume);

        #endregion

        #region Generic file system helpers
        /// <summary>
        /// Returns maximum directory path length.
        /// </summary>
        int MaxDirectoryPathLength();

        /// <summary>
        /// Tries to read the seek penalty property from a drive handle
        /// </summary>
        /// <param name="driveHandle">Handle to the drive. May either be a physical drive (ex: \\.\PhysicalDrive0)
        /// or a logical drive (ex: \\.c\:)</param>
        /// <param name="hasSeekPenalty">Set to the appropriate value if the check is successful</param>
        /// <param name="error">Error code returned by the native api</param>
        /// <returns>True if the property was able to be read</returns>
        bool TryReadSeekPenaltyProperty(SafeFileHandle driveHandle, out bool hasSeekPenalty, out int error);

        /// <summary>
        /// Indicates path existence (as a file, as a directory, or not at all) via probing with <c>GetFileAttributesW</c>
        /// </summary>
        /// <remarks>
        /// Supports paths greater than MAX_PATH.
        /// </remarks>
        Possible<PathExistence, NativeFailure> TryProbePathExistence(string path, bool followSymlink);

        /// <summary>
        /// Whether the provided path matches the pattern (e.g. "*.cs")
        /// </summary>
        bool PathMatchPattern(string path, string pattern);

        /// <summary>
        /// Calls GetFileInformationByHandleEx on the given file handle to determine whether it is pending deletion. The pending deletion state
        /// occurs when some handle A is opened to the file with FileShare.Delete mode specified, and a delete operation is completed through some other handle B.
        /// Windows will not actually delete the file until both handles A and B (and any other handles) are closed. Until that point in time, the file
        /// is placed on the Windows delete queue and marked as pending deletion.
        /// Any attempt to access the file after it is pending deletion will cause either
        /// [Windows NT Status Code STATUS_DELETE_PENDING] or [WIN32 Error code ERROR_ACCESS_DENIED] depending on the API
        /// </summary>
        /// <returns>TRUE if the file is in the delete queue; otherwise, false.</returns>
        bool IsPendingDelete(SafeFileHandle fileHandle);

        /// <summary>
        /// Returns a fully-normalized path corresponding to the given file handle. If a <paramref name="volumeGuidPath"/> is requested,
        /// the returned path will start with an NT-style path with a volume guid such as <c>\\?\Volume{2ce38532-4595-11e3-93ec-806e6f6e6963}\</c>.
        /// Otherwise, a DOS-style path starting with a drive-letter will be returned if possible (if the file's volume is not mounted to a drive letter,
        /// then this function falls back to act as if <paramref name="volumeGuidPath"/> was true).
        /// </summary>
        string GetFinalPathNameByHandle(SafeFileHandle handle, bool volumeGuidPath = false);

        /// <summary>
        /// Flushes cached pages for a file back to the filesystem. Unlike <c>FlushFileBuffers</c>, this does NOT
        /// issue a *disk-wide* cache flush, and so does NOT guarantee that written data is durable on disk (but it does
        /// force pages dirtied by e.g. a writable memory-mapping to be visible to the filesystem).
        /// The given handle must be opened with write access.
        /// </summary>
        /// <remarks>
        /// This wraps <c>NtFlushBuffersFileEx</c> and returns <c>NtStatus</c> that indicates whether the flush was a success.
        /// </remarks>
        NtStatus FlushPageCacheToFilesystem(SafeFileHandle handle);

        #endregion
    }
}
