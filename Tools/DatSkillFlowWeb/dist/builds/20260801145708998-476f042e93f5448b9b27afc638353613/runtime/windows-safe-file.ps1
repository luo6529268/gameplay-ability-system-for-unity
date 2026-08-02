[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [System.IO.Stream]$ProtocolStream
)

$ErrorActionPreference = 'Stop'

$nativeSource = @'
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Win32.SafeHandles;

public static class DatSkillFlowSafeFileTransaction
{
    private const uint GENERIC_READ = 0x80000000;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint FILE_READ_ATTRIBUTES = 0x00000080;
    private const uint SYNCHRONIZE = 0x00100000;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint CREATE_NEW = 1;
    private const uint OPEN_EXISTING = 3;
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
    private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    private const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
    private const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    private const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
    private const int FILE_ATTRIBUTE_TAG_INFO_CLASS = 9;
    private const int ERROR_FILE_NOT_FOUND = 2;
    private const int ERROR_PATH_NOT_FOUND = 3;
    private const int ERROR_FILE_EXISTS = 80;
    private const int ERROR_ALREADY_EXISTS = 183;
    private const int MAX_HEADER_BYTES = 256 * 1024;
    private const long UNIX_EPOCH_FILETIME = 116444736000000000L;

    [StructLayout(LayoutKind.Sequential)]
    private struct FILE_ATTRIBUTE_TAG_INFO
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

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

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleEx(
        SafeFileHandle file,
        int fileInformationClass,
        out FILE_ATTRIBUTE_TAG_INFO fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle file,
        out BY_HANDLE_FILE_INFORMATION fileInformation);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleW(
        SafeFileHandle file,
        StringBuilder filePath,
        uint filePathLength,
        uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadFile(
        SafeFileHandle file,
        byte[] buffer,
        uint bytesToRead,
        out uint bytesRead,
        IntPtr overlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WriteFile(
        SafeFileHandle file,
        IntPtr buffer,
        uint bytesToWrite,
        out uint bytesWritten,
        IntPtr overlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushFileBuffers(SafeFileHandle file);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFilePointerEx(
        SafeFileHandle file,
        long distanceToMove,
        out long newFilePointer,
        uint moveMethod);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReplaceFileW(
        string replacedFileName,
        string replacementFileName,
        string backupFileName,
        uint replaceFlags,
        IntPtr exclude,
        IntPtr reserved);

    private sealed class TransactionFailure : Exception
    {
        public readonly string Code;
        public readonly int? Win32Code;
        public Dictionary<string, object> Recovery;

        public TransactionFailure(string code, string message, int? win32Code)
            : base(message)
        {
            Code = code;
            Win32Code = win32Code;
        }
    }

    private sealed class Traversal : IDisposable
    {
        public readonly List<SafeFileHandle> Directories = new List<SafeFileHandle>();
        public readonly List<string> DirectoryPaths = new List<string>();
        public string RootPath;
        public string TargetPath;

        public void Dispose()
        {
            for (int index = Directories.Count - 1; index >= 0; index--)
            {
                Directories[index].Dispose();
            }
            Directories.Clear();
        }
    }

    private sealed class Snapshot
    {
        public string Path;
        public byte[] Bytes;
        public Dictionary<string, object> Fingerprint;
        public BY_HANDLE_FILE_INFORMATION Information;
    }

    private static JavaScriptSerializer Serializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = 32 * 1024 * 1024;
        serializer.RecursionLimit = 32;
        return serializer;
    }

    private static void ReadExact(Stream stream, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int count = stream.Read(buffer, offset, buffer.Length - offset);
            if (count <= 0)
            {
                throw new TransactionFailure("protocol-error", "Unexpected EOF in native transaction protocol.", null);
            }
            offset += count;
        }
    }

    private static int RequiredInt(Dictionary<string, object> request, string name, int minimum, int maximum)
    {
        object raw;
        if (!request.TryGetValue(name, out raw))
        {
            throw new TransactionFailure("protocol-error", "Missing protocol integer: " + name, null);
        }
        long value;
        try
        {
            value = Convert.ToInt64(raw);
        }
        catch (Exception)
        {
            throw new TransactionFailure("protocol-error", "Invalid protocol integer: " + name, null);
        }
        if (value < minimum || value > maximum)
        {
            throw new TransactionFailure("protocol-error", "Out-of-range protocol integer: " + name, null);
        }
        return (int)value;
    }

    private static string RequiredString(Dictionary<string, object> request, string name)
    {
        object raw;
        string value;
        if (!request.TryGetValue(name, out raw) || (value = raw as string) == null || value.Length == 0 || value.IndexOf('\0') >= 0)
        {
            throw new TransactionFailure("protocol-error", "Missing or invalid protocol string: " + name, null);
        }
        return value;
    }

    private static Dictionary<string, object> RequiredRecord(Dictionary<string, object> request, string name)
    {
        object raw;
        Dictionary<string, object> value;
        if (!request.TryGetValue(name, out raw) || (value = raw as Dictionary<string, object>) == null)
        {
            throw new TransactionFailure("protocol-error", "Missing or invalid protocol object: " + name, null);
        }
        return value;
    }

    private static bool HasBarrier(Dictionary<string, object> request, string name)
    {
        object raw;
        object[] values;
        if (!request.TryGetValue("barriers", out raw) || (values = raw as object[]) == null)
        {
            return false;
        }
        foreach (object value in values)
        {
            if (String.Equals(value as string, name, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static void Emit(Dictionary<string, object> value)
    {
        Console.Out.WriteLine(Serializer().Serialize(value));
        Console.Out.Flush();
    }

    private static void Barrier(
        Stream protocol,
        Dictionary<string, object> request,
        string name,
        string targetPath,
        string replacementPath,
        string backupPath)
    {
        if (!HasBarrier(request, name))
        {
            return;
        }
        Emit(new Dictionary<string, object>
        {
            { "type", "barrier" },
            { "name", name },
            { "targetPath", targetPath },
            { "replacementPath", replacementPath },
            { "backupPath", backupPath }
        });
        int acknowledgement = protocol.ReadByte();
        if (acknowledgement != 1)
        {
            throw new TransactionFailure("protocol-error", "Native transaction barrier was not acknowledged.", null);
        }
    }

    private static SafeFileHandle OpenRaw(
        string path,
        uint access,
        uint share,
        uint disposition,
        uint flags,
        string failureCode)
    {
        SafeFileHandle handle = CreateFileW(path, access, share, IntPtr.Zero, disposition, flags, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            int code = Marshal.GetLastWin32Error();
            handle.Dispose();
            throw new TransactionFailure(failureCode, "CreateFileW failed for a transaction path.", code);
        }
        return handle;
    }

    private static FILE_ATTRIBUTE_TAG_INFO AttributeInfo(SafeFileHandle handle)
    {
        FILE_ATTRIBUTE_TAG_INFO info;
        if (!GetFileInformationByHandleEx(handle, FILE_ATTRIBUTE_TAG_INFO_CLASS, out info, (uint)Marshal.SizeOf(typeof(FILE_ATTRIBUTE_TAG_INFO))))
        {
            throw new TransactionFailure("inspection-failed", "GetFileInformationByHandleEx failed.", Marshal.GetLastWin32Error());
        }
        return info;
    }

    private static BY_HANDLE_FILE_INFORMATION HandleInfo(SafeFileHandle handle)
    {
        BY_HANDLE_FILE_INFORMATION info;
        if (!GetFileInformationByHandle(handle, out info))
        {
            throw new TransactionFailure("inspection-failed", "GetFileInformationByHandle failed.", Marshal.GetLastWin32Error());
        }
        return info;
    }

    private static void RequireNoReparse(SafeFileHandle handle, bool directory)
    {
        FILE_ATTRIBUTE_TAG_INFO info = AttributeInfo(handle);
        if ((info.FileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != 0)
        {
            throw new TransactionFailure("reparse-point", "A transaction path is a reparse point.", null);
        }
        bool isDirectory = (info.FileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0;
        if (directory != isDirectory)
        {
            throw new TransactionFailure(directory ? "not-a-directory" : "not-a-file", "A transaction path has the wrong file type.", null);
        }
    }

    private static string FinalPath(SafeFileHandle handle)
    {
        int capacity = 512;
        while (capacity <= 32768)
        {
            StringBuilder builder = new StringBuilder(capacity);
            uint length = GetFinalPathNameByHandleW(handle, builder, (uint)builder.Capacity, 0);
            if (length == 0)
            {
                throw new TransactionFailure("inspection-failed", "GetFinalPathNameByHandleW failed.", Marshal.GetLastWin32Error());
            }
            if (length < builder.Capacity)
            {
                string value = builder.ToString();
                if (value.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
                {
                    value = "\\\\" + value.Substring(8);
                }
                else if (value.StartsWith("\\\\?\\", StringComparison.OrdinalIgnoreCase))
                {
                    value = value.Substring(4);
                }
                return NormalizePath(value);
            }
            capacity = checked((int)length + 1);
        }
        throw new TransactionFailure("inspection-failed", "The final path exceeds the transaction limit.", null);
    }

    private static string NormalizePath(string path)
    {
        string full = Path.GetFullPath(path);
        string root = Path.GetPathRoot(full);
        if (!String.Equals(full, root, StringComparison.OrdinalIgnoreCase))
        {
            full = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        return full;
    }

    private static bool SamePath(string left, string right)
    {
        return String.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contained(string root, string candidate)
    {
        string normalizedRoot = NormalizePath(root);
        string normalizedCandidate = NormalizePath(candidate);
        return normalizedCandidate.Length > normalizedRoot.Length
            && normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            && (normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                || normalizedCandidate[normalizedRoot.Length] == Path.DirectorySeparatorChar);
    }

    private static ulong FileId(BY_HANDLE_FILE_INFORMATION info)
    {
        return ((ulong)info.FileIndexHigh << 32) | info.FileIndexLow;
    }

    private static ulong Size(BY_HANDLE_FILE_INFORMATION info)
    {
        return ((ulong)info.FileSizeHigh << 32) | info.FileSizeLow;
    }

    private static long FileTimeTicks(System.Runtime.InteropServices.ComTypes.FILETIME value)
    {
        return ((long)(uint)value.dwHighDateTime << 32) | (uint)value.dwLowDateTime;
    }

    private static string UnixNanoseconds(System.Runtime.InteropServices.ComTypes.FILETIME value)
    {
        long fileTime = FileTimeTicks(value);
        return checked((fileTime - UNIX_EPOCH_FILETIME) * 100L).ToString();
    }

    private static Dictionary<string, object> Fingerprint(byte[] bytes, BY_HANDLE_FILE_INFORMATION info)
    {
        string digest;
        using (SHA256 algorithm = SHA256.Create())
        {
            digest = Hex(algorithm.ComputeHash(bytes));
        }
        return new Dictionary<string, object>
        {
            { "sha256", digest },
            { "size", bytes.Length },
            { "modifiedNanoseconds", UnixNanoseconds(info.LastWriteTime) },
            { "changedNanoseconds", UnixNanoseconds(info.CreationTime) },
            { "device", info.VolumeSerialNumber.ToString() },
            { "inode", FileId(info).ToString() }
        };
    }

    private static string Hex(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder(bytes.Length * 2);
        foreach (byte value in bytes)
        {
            builder.Append(value.ToString("x2"));
        }
        return builder.ToString();
    }

    private static bool SameStableInfo(BY_HANDLE_FILE_INFORMATION left, BY_HANDLE_FILE_INFORMATION right)
    {
        return left.VolumeSerialNumber == right.VolumeSerialNumber
            && FileId(left) == FileId(right)
            && Size(left) == Size(right)
            && FileTimeTicks(left.LastWriteTime) == FileTimeTicks(right.LastWriteTime)
            && FileTimeTicks(left.CreationTime) == FileTimeTicks(right.CreationTime);
    }

    private static Snapshot ReadSnapshot(SafeFileHandle handle, string path, int maximumBytes)
    {
        RequireNoReparse(handle, false);
        BY_HANDLE_FILE_INFORMATION before = HandleInfo(handle);
        ulong size = Size(before);
        if (size > (ulong)maximumBytes || size > Int32.MaxValue)
        {
            throw new TransactionFailure("read-too-large", "The selected file exceeds the transaction read limit.", null);
        }
        long pointer;
        if (!SetFilePointerEx(handle, 0, out pointer, 0))
        {
            throw new TransactionFailure("read-failed", "Unable to seek the selected file.", Marshal.GetLastWin32Error());
        }
        byte[] bytes = new byte[(int)size];
        int offset = 0;
        byte[] chunk = new byte[Math.Min(64 * 1024, Math.Max(1, bytes.Length))];
        while (offset < bytes.Length)
        {
            uint count;
            uint wanted = (uint)Math.Min(chunk.Length, bytes.Length - offset);
            if (!ReadFile(handle, chunk, wanted, out count, IntPtr.Zero))
            {
                throw new TransactionFailure("read-failed", "ReadFile failed.", Marshal.GetLastWin32Error());
            }
            if (count == 0)
            {
                throw new TransactionFailure("file-changed-during-read", "The selected file became shorter while it was read.", null);
            }
            Buffer.BlockCopy(chunk, 0, bytes, offset, (int)count);
            offset += (int)count;
        }
        uint trailing;
        if (!ReadFile(handle, chunk, 1, out trailing, IntPtr.Zero))
        {
            throw new TransactionFailure("read-failed", "ReadFile failed after the bounded read.", Marshal.GetLastWin32Error());
        }
        BY_HANDLE_FILE_INFORMATION after = HandleInfo(handle);
        if (trailing != 0 || !SameStableInfo(before, after))
        {
            throw new TransactionFailure("file-changed-during-read", "The selected file changed while it was read.", null);
        }
        Snapshot snapshot = new Snapshot();
        snapshot.Path = path;
        snapshot.Bytes = bytes;
        snapshot.Information = after;
        snapshot.Fingerprint = Fingerprint(bytes, after);
        return snapshot;
    }

    private static void WriteAll(SafeFileHandle handle, byte[] bytes)
    {
        if (bytes.Length > 0)
        {
            GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                int offset = 0;
                while (offset < bytes.Length)
                {
                    uint written;
                    uint wanted = (uint)(bytes.Length - offset);
                    IntPtr pointer = new IntPtr(pinned.AddrOfPinnedObject().ToInt64() + offset);
                    if (!WriteFile(handle, pointer, wanted, out written, IntPtr.Zero))
                    {
                        throw new TransactionFailure("write-failed", "WriteFile failed.", Marshal.GetLastWin32Error());
                    }
                    if (written == 0)
                    {
                        throw new TransactionFailure("write-failed", "WriteFile completed without progress.", null);
                    }
                    offset += (int)written;
                }
            }
            finally
            {
                pinned.Free();
            }
        }
        if (!FlushFileBuffers(handle))
        {
            throw new TransactionFailure("flush-failed", "FlushFileBuffers failed.", Marshal.GetLastWin32Error());
        }
    }

    private static SafeFileHandle OpenDirectory(string path)
    {
        SafeFileHandle handle = OpenRaw(
            path,
            FILE_READ_ATTRIBUTES | SYNCHRONIZE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            OPEN_EXISTING,
            FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT,
            "directory-open-failed");
        try
        {
            RequireNoReparse(handle, true);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenExistingFile(string path, string root)
    {
        SafeFileHandle handle = OpenRaw(
            path,
            GENERIC_READ | FILE_READ_ATTRIBUTES | SYNCHRONIZE,
            FILE_SHARE_READ,
            OPEN_EXISTING,
            FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_SEQUENTIAL_SCAN,
            "not-a-file");
        try
        {
            RequireNoReparse(handle, false);
            string final = FinalPath(handle);
            if (!SamePath(final, path) || !Contained(root, final))
            {
                throw new TransactionFailure("root-escape", "The opened file does not match the validated workspace target.", null);
            }
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle CreateNewFile(string path, string root)
    {
        SafeFileHandle handle;
        try
        {
            handle = OpenRaw(
                path,
                GENERIC_READ | GENERIC_WRITE | FILE_READ_ATTRIBUTES | SYNCHRONIZE,
                0,
                CREATE_NEW,
                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_WRITE_THROUGH | FILE_FLAG_OPEN_REPARSE_POINT,
                "create-failed");
        }
        catch (TransactionFailure failure)
        {
            if (failure.Win32Code == ERROR_FILE_EXISTS || failure.Win32Code == ERROR_ALREADY_EXISTS)
            {
                throw new TransactionFailure("already-exists", "The destination already exists.", failure.Win32Code);
            }
            throw;
        }
        try
        {
            RequireNoReparse(handle, false);
            string final = FinalPath(handle);
            if (!SamePath(final, path) || !Contained(root, final))
            {
                throw new TransactionFailure("root-escape", "The created file does not match the validated workspace target.", null);
            }
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static void ValidateLeaf(string value, string label)
    {
        if (String.IsNullOrEmpty(value)
            || value == "."
            || value == ".."
            || value.IndexOf('\0') >= 0
            || value.IndexOf(':') >= 0
            || value.IndexOf('/') >= 0
            || value.IndexOf('\\') >= 0
            || value.EndsWith(".", StringComparison.Ordinal)
            || value.EndsWith(" ", StringComparison.Ordinal))
        {
            throw new TransactionFailure("invalid-logical-path", "Unsafe " + label + ".", null);
        }
        int extension = value.IndexOf('.');
        string baseName = (extension < 0 ? value : value.Substring(0, extension)).ToUpperInvariant();
        bool reserved = baseName == "CON"
            || baseName == "PRN"
            || baseName == "AUX"
            || baseName == "NUL"
            || (baseName.Length == 4
                && (baseName.StartsWith("COM", StringComparison.Ordinal)
                    || baseName.StartsWith("LPT", StringComparison.Ordinal))
                && baseName[3] >= '1'
                && baseName[3] <= '9');
        if (reserved)
        {
            throw new TransactionFailure("invalid-logical-path", "Unsafe " + label + ".", null);
        }
    }

    private static Traversal Traverse(Dictionary<string, object> request)
    {
        Dictionary<string, object> root = RequiredRecord(request, "root");
        string expectedRootPath = NormalizePath(RequiredString(root, "canonicalPath"));
        string expectedVolume = RequiredString(root, "volumeSerial");
        string expectedFileId = RequiredString(root, "fileId");
        string logicalPath = RequiredString(request, "logicalPath");
        if (logicalPath.IndexOf('\\') >= 0 || logicalPath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new TransactionFailure("invalid-logical-path", "The logical path is not a portable relative path.", null);
        }
        string[] segments = logicalPath.Split('/');
        if (segments.Length == 0)
        {
            throw new TransactionFailure("invalid-logical-path", "The logical path is empty.", null);
        }
        foreach (string segment in segments)
        {
            ValidateLeaf(segment, "logical path segment");
        }

        Traversal traversal = new Traversal();
        try
        {
            SafeFileHandle rootHandle = OpenDirectory(expectedRootPath);
            traversal.Directories.Add(rootHandle);
            string rootFinal = FinalPath(rootHandle);
            traversal.DirectoryPaths.Add(rootFinal);
            BY_HANDLE_FILE_INFORMATION rootInfo = HandleInfo(rootHandle);
            if (!SamePath(rootFinal, expectedRootPath)
                || rootInfo.VolumeSerialNumber.ToString() != expectedVolume
                || FileId(rootInfo).ToString() != expectedFileId)
            {
                throw new TransactionFailure("root-changed", "The granted workspace root no longer identifies the same directory.", null);
            }
            traversal.RootPath = rootFinal;
            string current = rootFinal;
            for (int index = 0; index < segments.Length - 1; index++)
            {
                string requestedChild = NormalizePath(Path.Combine(current, segments[index]));
                if (!Contained(rootFinal, requestedChild))
                {
                    throw new TransactionFailure("root-escape", "A requested parent escapes the workspace root.", null);
                }
                SafeFileHandle child = OpenDirectory(requestedChild);
                traversal.Directories.Add(child);
                string childFinal = FinalPath(child);
                if (!SamePath(childFinal, requestedChild) || !Contained(rootFinal, childFinal))
                {
                    throw new TransactionFailure("root-escape", "A requested parent resolves outside the workspace root.", null);
                }
                traversal.DirectoryPaths.Add(childFinal);
                current = childFinal;
            }
            traversal.TargetPath = NormalizePath(Path.Combine(current, segments[segments.Length - 1]));
            if (!Contained(rootFinal, traversal.TargetPath))
            {
                throw new TransactionFailure("root-escape", "The requested target escapes the workspace root.", null);
            }
            return traversal;
        }
        catch
        {
            traversal.Dispose();
            throw;
        }
    }

    private static void RevalidateTraversal(Traversal traversal)
    {
        if (traversal.Directories.Count != traversal.DirectoryPaths.Count)
        {
            throw new TransactionFailure("root-changed", "The validated directory handle set is inconsistent.", null);
        }
        for (int index = 0; index < traversal.Directories.Count; index++)
        {
            SafeFileHandle directory = traversal.Directories[index];
            RequireNoReparse(directory, true);
            string current = FinalPath(directory);
            if (!SamePath(current, traversal.DirectoryPaths[index])
                || (index > 0 && !Contained(traversal.RootPath, current)))
            {
                throw new TransactionFailure("root-changed", "A validated parent directory changed its namespace path.", null);
            }
        }
    }

    private static Dictionary<string, object> Observation(string path, string root, int maximumBytes)
    {
        SafeFileHandle handle = null;
        try
        {
            handle = CreateFileW(
                path,
                GENERIC_READ | FILE_READ_ATTRIBUTES | SYNCHRONIZE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_SEQUENTIAL_SCAN,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                int code = Marshal.GetLastWin32Error();
                handle.Dispose();
                handle = null;
                if (code == ERROR_FILE_NOT_FOUND || code == ERROR_PATH_NOT_FOUND)
                {
                    return new Dictionary<string, object> { { "path", path }, { "exists", false } };
                }
                return new Dictionary<string, object>
                {
                    { "path", path },
                    { "exists", false },
                    { "inspectionError", "win32-" + code }
                };
            }
            FILE_ATTRIBUTE_TAG_INFO attributes = AttributeInfo(handle);
            if ((attributes.FileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != 0
                || (attributes.FileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
            {
                return new Dictionary<string, object>
                {
                    { "path", path },
                    { "exists", true },
                    { "inspectionError", "not-a-regular-file" }
                };
            }
            string final = FinalPath(handle);
            if (!SamePath(final, path) || !Contained(root, final))
            {
                return new Dictionary<string, object>
                {
                    { "path", path },
                    { "exists", true },
                    { "inspectionError", "root-escape" }
                };
            }
            Snapshot snapshot = ReadSnapshot(handle, path, maximumBytes);
            return new Dictionary<string, object>
            {
                { "path", path },
                { "exists", true },
                { "size", snapshot.Bytes.Length },
                { "sha256", snapshot.Fingerprint["sha256"] }
            };
        }
        catch (TransactionFailure failure)
        {
            return new Dictionary<string, object>
            {
                { "path", path },
                { "exists", true },
                { "inspectionError", failure.Code }
            };
        }
        finally
        {
            if (handle != null)
            {
                handle.Dispose();
            }
        }
    }

    private static Dictionary<string, object> Recovery(
        string root,
        string target,
        string replacement,
        string backup,
        int maximumBytes)
    {
        Dictionary<string, object> recovery = new Dictionary<string, object>();
        recovery["target"] = Observation(target, root, maximumBytes);
        if (!String.IsNullOrEmpty(replacement))
        {
            recovery["replacement"] = Observation(replacement, root, maximumBytes);
        }
        if (!String.IsNullOrEmpty(backup))
        {
            recovery["backup"] = Observation(backup, root, maximumBytes);
        }
        return recovery;
    }

    private static bool FingerprintMatches(Dictionary<string, object> actual, Dictionary<string, object> expected)
    {
        string[] stringFields = new string[] { "sha256", "modifiedNanoseconds", "changedNanoseconds", "device", "inode" };
        foreach (string field in stringFields)
        {
            object actualValue;
            object expectedValue;
            if (!actual.TryGetValue(field, out actualValue)
                || !expected.TryGetValue(field, out expectedValue)
                || !String.Equals(Convert.ToString(actualValue), Convert.ToString(expectedValue), StringComparison.Ordinal))
            {
                return false;
            }
        }
        object actualSize;
        object expectedSize;
        return actual.TryGetValue("size", out actualSize)
            && expected.TryGetValue("size", out expectedSize)
            && Convert.ToInt64(actualSize) == Convert.ToInt64(expectedSize);
    }

    private static Dictionary<string, object> SuccessSnapshot(Snapshot snapshot, Dictionary<string, object> recovery)
    {
        return new Dictionary<string, object>
        {
            { "type", "result" },
            { "ok", true },
            { "canonicalPath", snapshot.Path },
            { "fingerprint", snapshot.Fingerprint },
            { "recovery", recovery }
        };
    }

    private static Dictionary<string, object> InspectRoot(Dictionary<string, object> request)
    {
        string requested = NormalizePath(RequiredString(request, "absoluteRoot"));
        SafeFileHandle handle = OpenDirectory(requested);
        try
        {
            string final = FinalPath(handle);
            BY_HANDLE_FILE_INFORMATION info = HandleInfo(handle);
            return new Dictionary<string, object>
            {
                { "type", "result" },
                { "ok", true },
                { "root", new Dictionary<string, object>
                    {
                        { "canonicalPath", final },
                        { "volumeSerial", info.VolumeSerialNumber.ToString() },
                        { "fileId", FileId(info).ToString() }
                    }
                }
            };
        }
        finally
        {
            handle.Dispose();
        }
    }

    private static Dictionary<string, object> ReadOperation(
        Stream protocol,
        Dictionary<string, object> request,
        int maximumBytes)
    {
        using (Traversal traversal = Traverse(request))
        {
            Barrier(protocol, request, "after-directory-handles", traversal.TargetPath, traversal.TargetPath, traversal.TargetPath);
            RevalidateTraversal(traversal);
            using (SafeFileHandle file = OpenExistingFile(traversal.TargetPath, traversal.RootPath))
            {
                Snapshot snapshot = ReadSnapshot(file, FinalPath(file), maximumBytes);
                return new Dictionary<string, object>
                {
                    { "type", "result" },
                    { "ok", true },
                    { "canonicalPath", snapshot.Path },
                    { "bytesBase64", Convert.ToBase64String(snapshot.Bytes) },
                    { "fingerprint", snapshot.Fingerprint }
                };
            }
        }
    }

    private static Dictionary<string, object> SaveAsOperation(
        Stream protocol,
        Dictionary<string, object> request,
        byte[] content,
        int maximumBytes)
    {
        using (Traversal traversal = Traverse(request))
        {
            Barrier(protocol, request, "after-directory-handles", traversal.TargetPath, traversal.TargetPath, traversal.TargetPath);
            RevalidateTraversal(traversal);
            try
            {
                using (SafeFileHandle file = CreateNewFile(traversal.TargetPath, traversal.RootPath))
                {
                    WriteAll(file, content);
                    Snapshot snapshot = ReadSnapshot(file, FinalPath(file), maximumBytes);
                    string expected = Hex(SHA256.Create().ComputeHash(content));
                    if (!String.Equals(Convert.ToString(snapshot.Fingerprint["sha256"]), expected, StringComparison.Ordinal))
                    {
                        throw new TransactionFailure("postcondition-failed", "The created file hash does not match the requested content.", null);
                    }
                    Dictionary<string, object> recovery = new Dictionary<string, object>
                    {
                        { "target", new Dictionary<string, object>
                            {
                                { "path", snapshot.Path },
                                { "exists", true },
                                { "size", snapshot.Bytes.Length },
                                { "sha256", snapshot.Fingerprint["sha256"] }
                            }
                        }
                    };
                    return SuccessSnapshot(snapshot, recovery);
                }
            }
            catch (TransactionFailure failure)
            {
                failure.Recovery = Recovery(traversal.RootPath, traversal.TargetPath, null, null, maximumBytes);
                throw;
            }
        }
    }

    private static Dictionary<string, object> OverwriteOperation(
        Stream protocol,
        Dictionary<string, object> request,
        byte[] content,
        int maximumBytes)
    {
        using (Traversal traversal = Traverse(request))
        {
            string replacementName = RequiredString(request, "replacementName");
            string backupName = RequiredString(request, "backupName");
            ValidateLeaf(replacementName, "replacement name");
            ValidateLeaf(backupName, "backup name");
            string parent = Path.GetDirectoryName(traversal.TargetPath);
            string replacementPath = NormalizePath(Path.Combine(parent, replacementName));
            string backupPath = NormalizePath(Path.Combine(parent, backupName));
            if (!Contained(traversal.RootPath, replacementPath)
                || !Contained(traversal.RootPath, backupPath)
                || SamePath(traversal.TargetPath, replacementPath)
                || SamePath(traversal.TargetPath, backupPath)
                || SamePath(replacementPath, backupPath))
            {
                throw new TransactionFailure("invalid-logical-path", "Unsafe recovery path names.", null);
            }
            Barrier(protocol, request, "after-directory-handles", traversal.TargetPath, replacementPath, backupPath);
            RevalidateTraversal(traversal);
            Dictionary<string, object> expected = RequiredRecord(request, "expectedFingerprint");
            Snapshot oldSnapshot = null;
            Snapshot newSnapshot = null;
            SafeFileHandle target = null;
            SafeFileHandle replacement = null;
            try
            {
                target = OpenExistingFile(traversal.TargetPath, traversal.RootPath);
                oldSnapshot = ReadSnapshot(target, FinalPath(target), maximumBytes);
                if (!FingerprintMatches(oldSnapshot.Fingerprint, expected))
                {
                    throw new TransactionFailure("external-change", "The overwrite target changed after confirmation.", null);
                }
                Dictionary<string, object> backupBefore = Observation(backupPath, traversal.RootPath, maximumBytes);
                if (Convert.ToBoolean(backupBefore["exists"]))
                {
                    throw new TransactionFailure("backup-exists", "The selected backup path already exists.", null);
                }
                replacement = CreateNewFile(replacementPath, traversal.RootPath);
                WriteAll(replacement, content);
                newSnapshot = ReadSnapshot(replacement, FinalPath(replacement), maximumBytes);
                string requestedDigest = Hex(SHA256.Create().ComputeHash(content));
                if (!String.Equals(Convert.ToString(newSnapshot.Fingerprint["sha256"]), requestedDigest, StringComparison.Ordinal))
                {
                    throw new TransactionFailure("postcondition-failed", "The replacement hash does not match the requested content.", null);
                }
                Barrier(protocol, request, "before-publish", traversal.TargetPath, replacementPath, backupPath);
                RevalidateTraversal(traversal);
                Snapshot oldRevalidated = ReadSnapshot(target, FinalPath(target), maximumBytes);
                Snapshot newRevalidated = ReadSnapshot(replacement, FinalPath(replacement), maximumBytes);
                if (!SamePath(oldRevalidated.Path, traversal.TargetPath)
                    || !SamePath(newRevalidated.Path, replacementPath)
                    || !FingerprintMatches(oldRevalidated.Fingerprint, oldSnapshot.Fingerprint)
                    || !FingerprintMatches(newRevalidated.Fingerprint, newSnapshot.Fingerprint))
                {
                    throw new TransactionFailure(
                        "external-change",
                        "A validated overwrite handle changed before publication.",
                        null);
                }
                Dictionary<string, object> backupImmediatelyBefore = Observation(backupPath, traversal.RootPath, maximumBytes);
                if (Convert.ToBoolean(backupImmediatelyBefore["exists"]))
                {
                    throw new TransactionFailure("backup-exists", "The backup path was claimed before publication.", null);
                }
                target.Dispose();
                target = null;
                replacement.Dispose();
                replacement = null;
                bool replaced = ReplaceFileW(
                    traversal.TargetPath,
                    replacementPath,
                    backupPath,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero);
                int replaceError = replaced ? 0 : Marshal.GetLastWin32Error();
                Dictionary<string, object> recovery = Recovery(
                    traversal.RootPath,
                    traversal.TargetPath,
                    replacementPath,
                    backupPath,
                    maximumBytes);
                if (!replaced)
                {
                    TransactionFailure failure = new TransactionFailure(
                        "replace-failed",
                        "ReplaceFileW failed; all recovery paths were preserved.",
                        replaceError);
                    failure.Recovery = recovery;
                    throw failure;
                }
                Dictionary<string, object> targetObservation = (Dictionary<string, object>)recovery["target"];
                Dictionary<string, object> replacementObservation = (Dictionary<string, object>)recovery["replacement"];
                Dictionary<string, object> backupObservation = (Dictionary<string, object>)recovery["backup"];
                string oldDigest = Convert.ToString(oldSnapshot.Fingerprint["sha256"]);
                string newDigest = Convert.ToString(newSnapshot.Fingerprint["sha256"]);
                bool postconditions = Convert.ToBoolean(targetObservation["exists"])
                    && !Convert.ToBoolean(replacementObservation["exists"])
                    && Convert.ToBoolean(backupObservation["exists"])
                    && String.Equals(Convert.ToString(targetObservation.ContainsKey("sha256") ? targetObservation["sha256"] : null), newDigest, StringComparison.Ordinal)
                    && String.Equals(Convert.ToString(backupObservation.ContainsKey("sha256") ? backupObservation["sha256"] : null), oldDigest, StringComparison.Ordinal);
                if (!postconditions)
                {
                    TransactionFailure failure = new TransactionFailure(
                        "postcondition-failed",
                        "ReplaceFileW completed but target/backup hashes failed closed verification.",
                        0);
                    failure.Recovery = recovery;
                    throw failure;
                }
                using (SafeFileHandle published = OpenExistingFile(traversal.TargetPath, traversal.RootPath))
                {
                    Snapshot publishedSnapshot = ReadSnapshot(published, FinalPath(published), maximumBytes);
                    if (!String.Equals(Convert.ToString(publishedSnapshot.Fingerprint["sha256"]), newDigest, StringComparison.Ordinal))
                    {
                        TransactionFailure failure = new TransactionFailure(
                            "postcondition-failed",
                            "The reopened target hash differs from the verified replacement hash.",
                            0);
                        failure.Recovery = recovery;
                        throw failure;
                    }
                    return SuccessSnapshot(publishedSnapshot, recovery);
                }
            }
            catch (TransactionFailure failure)
            {
                if (failure.Recovery == null)
                {
                    if (target != null)
                    {
                        target.Dispose();
                        target = null;
                    }
                    if (replacement != null)
                    {
                        replacement.Dispose();
                        replacement = null;
                    }
                    failure.Recovery = Recovery(
                        traversal.RootPath,
                        traversal.TargetPath,
                        replacementPath,
                        backupPath,
                        maximumBytes);
                }
                throw;
            }
            finally
            {
                if (target != null)
                {
                    target.Dispose();
                }
                if (replacement != null)
                {
                    replacement.Dispose();
                }
            }
        }
    }

    public static void Run(Stream protocol)
    {
        try
        {
            byte[] headerLengthBytes = new byte[4];
            ReadExact(protocol, headerLengthBytes);
            int headerLength = BitConverter.ToInt32(headerLengthBytes, 0);
            if (headerLength < 2 || headerLength > MAX_HEADER_BYTES)
            {
                throw new TransactionFailure("protocol-error", "Invalid native transaction header length.", null);
            }
            byte[] headerBytes = new byte[headerLength];
            ReadExact(protocol, headerBytes);
            Dictionary<string, object> request = Serializer().DeserializeObject(Encoding.UTF8.GetString(headerBytes)) as Dictionary<string, object>;
            if (request == null)
            {
                throw new TransactionFailure("protocol-error", "Invalid native transaction request.", null);
            }
            int contentLength = RequiredInt(request, "contentLength", 0, 16 * 1024 * 1024);
            byte[] content = new byte[contentLength];
            ReadExact(protocol, content);
            string operation = RequiredString(request, "operation");
            Dictionary<string, object> result;
            if (operation == "inspectRoot")
            {
                if (contentLength != 0)
                {
                    throw new TransactionFailure("protocol-error", "inspectRoot does not accept content.", null);
                }
                result = InspectRoot(request);
            }
            else
            {
                int maximumBytes = RequiredInt(request, "maximumBytes", 1, 16 * 1024 * 1024);
                if (contentLength > maximumBytes)
                {
                    throw new TransactionFailure("content-too-large", "Content exceeds the transaction limit.", null);
                }
                if (operation == "read")
                {
                    if (contentLength != 0)
                    {
                        throw new TransactionFailure("protocol-error", "read does not accept content.", null);
                    }
                    result = ReadOperation(protocol, request, maximumBytes);
                }
                else if (operation == "saveAs")
                {
                    result = SaveAsOperation(protocol, request, content, maximumBytes);
                }
                else if (operation == "overwrite")
                {
                    result = OverwriteOperation(protocol, request, content, maximumBytes);
                }
                else
                {
                    throw new TransactionFailure("protocol-error", "Unknown native transaction operation.", null);
                }
            }
            Emit(result);
        }
        catch (TransactionFailure failure)
        {
            Dictionary<string, object> result = new Dictionary<string, object>
            {
                { "type", "result" },
                { "ok", false },
                { "code", failure.Code },
                { "message", failure.Message }
            };
            if (failure.Win32Code.HasValue)
            {
                result["win32Code"] = failure.Win32Code.Value;
            }
            if (failure.Recovery != null)
            {
                result["recovery"] = failure.Recovery;
            }
            Emit(result);
        }
        catch (Exception error)
        {
            Emit(new Dictionary<string, object>
            {
                { "type", "result" },
                { "ok", false },
                { "code", "native-unexpected" },
                { "message", error.Message }
            });
        }
    }
}
'@

try {
    Add-Type -AssemblyName System.Web.Extensions
    Add-Type -TypeDefinition $nativeSource -Language CSharp -ReferencedAssemblies @('System.Web.Extensions')
    [DatSkillFlowSafeFileTransaction]::Run($ProtocolStream)
}
catch {
    [Console]::Out.WriteLine((@{
        type = 'result'
        ok = $false
        code = 'helper-load-failed'
        message = $_.Exception.Message
    } | ConvertTo-Json -Compress))
}
