namespace Wice.Utilities;

public static partial class IOUtilities
{
    public const int DefaultWrapSharingViolationsRetryCount = 10;
    public const int DefaultWrapSharingViolationsWaitTime = 100;
    public static readonly DateTime MinFileTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal const string _applicationOctetStream = "application/octet-stream";

    private static readonly ConcurrentDictionary<string, string?> _extensionsByContentType = new(StringComparer.OrdinalIgnoreCase);

    [LibraryImport("urlmon", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int FindMimeFromData(nint pBC,
        [MarshalAs(UnmanagedType.LPWStr)] string? pwzUrl,
        nint pBuffer,
        int cbSize,
        string? pwzMimeProposed,
        uint dwMimeFlags,
        out nint ppwzMimeOut,
        int dwReserverd
        );

    public static unsafe string? FindContentType(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        const int FMFD_ENABLEMIMESNIFFING = 0x2;
        const int FMFD_RETURNUPDATEDIMGMIMES = 0x20;
        fixed (byte* p = bytes)
        {
            FindMimeFromData(0, null, (nint)p, bytes.Length, null, FMFD_RETURNUPDATEDIMGMIMES | FMFD_ENABLEMIMESNIFFING, out var ptr, 0);
            if (ptr == 0)
                return null;

            var ct = Marshal.PtrToStringUni(ptr);
            Marshal.FreeCoTaskMem(ptr);
            return ct != _applicationOctetStream ? ct : null;
        }
    }

    public static string? FindContentType(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            return null;

        var bytes = new byte[256];
        using var file = File.OpenRead(filePath);
        _ = file.Read(bytes, 0, bytes.Length);
        return FindContentType(bytes);
    }

    public static string? GetFileExtensionFromContentType(string contentType)
    {
        if (contentType == null)
            return null;

        if (!_extensionsByContentType.TryGetValue(contentType, out var ext))
        {
            var key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + contentType, false);
            if (key != null)
            {
                ext = string.Format("{0}", key.GetValue("extension")).Nullify();
            }
            _extensionsByContentType.AddOrUpdate(contentType, ext, (k, o) => ext);
        }
        return ext;
    }

    public static string ExtractAssemblyResource(string directoryPath, string name) => ExtractAssemblyResource(Assembly.GetCallingAssembly(), directoryPath, name);
    public static string ExtractAssemblyResource(Assembly assembly, string directoryPath, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(directoryPath);
        assembly ??= Assembly.GetCallingAssembly();
        var guid = (assembly.FullName + "\0" + name).ComputeGuidHash();
        var location = System.IO.Path.Combine(directoryPath, guid.ToString("N") + System.IO.Path.GetExtension(name));
        if (!FileExists(location))
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new WiceException("0001: Cannot find stream '" + name + "' in assembly '" + assembly.FullName + "'.");

            WrapSharingViolations(() =>
            {
                FileEnsureDirectory(location);
                using var file = File.Create(location);
                stream.CopyTo(file);
            });
        }
        return location;
    }

    public static bool EnsureDirectory(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        if (!System.IO.Path.IsPathRooted(directoryPath))
        {
            directoryPath = System.IO.Path.GetFullPath(directoryPath);
        }

        if (DirectoryExists(directoryPath))
            return false;

        Directory.CreateDirectory(directoryPath);
        return true;
    }

    public static bool FileEnsureDirectory(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!System.IO.Path.IsPathRooted(filePath))
        {
            filePath = System.IO.Path.GetFullPath(filePath);
        }

        var dir = System.IO.Path.GetDirectoryName(filePath);
        if (dir == null || Directory.Exists(dir))
            return false;

        Directory.CreateDirectory(dir);
        return true;
    }

    public static bool FileMove(string source, string destination, bool unprotect = true, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        FileDelete(destination, unprotect, throwOnError);
        FileEnsureDirectory(destination);

        if (throwOnError)
        {
            File.Move(source, destination);
        }
        else
        {
            try
            {
                File.Move(source, destination);
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    public static bool FileDelete(string filePath, bool unprotect = true, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!FileExists(filePath))
            return false;

        if (throwOnError)
        {
            Delete();
        }
        else
        {
            try
            {
                Delete();
            }
            catch
            {
                return false;
            }
        }

        void Delete()
        {
            var attributes = File.GetAttributes(filePath);
            if (attributes.HasFlag(FileAttributes.ReadOnly) && unprotect)
            {
                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
            }

            File.Delete(filePath);
        }
        return true;
    }

    public static bool FileOverwrite(string source, string destination, bool unprotect = true, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source);
        if (PathIsEqual(source, destination))
            return false;

        FileDelete(destination, unprotect, throwOnError);
        FileEnsureDirectory(destination);

        if (throwOnError)
        {
            File.Copy(source, destination, true);
        }
        else
        {
            try
            {
                File.Copy(source, destination, true);
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    public static bool FileExists(string path)
    {
        if (path == null)
            return false;

        try
        {
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public static bool DirectoryExists(string path)
    {
        if (path == null)
            return false;

        try
        {
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public static bool PathIsEqual(string path1, string path2, bool normalize = true)
    {
        ArgumentNullException.ThrowIfNull(path1);
        ArgumentNullException.ThrowIfNull(path2);
        if (normalize)
        {
            path1 = System.IO.Path.GetFullPath(path1);
            path2 = System.IO.Path.GetFullPath(path2);
        }

        return path1.EqualsIgnoreCase(path2);
    }

    public static bool PathIsChildOrEqual(string path, string child, bool normalize = true) => PathIsChild(path, child, normalize) || PathIsEqual(path, child, normalize);
    public static bool PathIsChild(string path, string child, bool normalize = true) => PathIsChild(path, child, normalize, out var _);
    public static bool PathIsChild(string path, string child, bool normalize, out string? subPath)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(child);
        subPath = null;
        if (normalize)
        {
            path = System.IO.Path.GetFullPath(path);
            child = System.IO.Path.GetFullPath(child);
        }

        path = StripTerminatingPathSeparators(path)!;
        if (path == null)
            return false;

        if (child.Length < (path.Length + 1))
            return false;

        var newChild = System.IO.Path.Combine(path, child[(path.Length + 1)..]);
        var b = newChild.EqualsIgnoreCase(child);
        if (b)
        {
            subPath = child[path.Length..];
            while (subPath.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                subPath = subPath[1..];
            }
        }
        return b;
    }

    public static string? StripTerminatingPathSeparators(string? path)
    {
        if (path == null)
            return null;

        while (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
        {
            path = path[..^1];
        }
        return path;
    }

    public delegate bool WrapSharingViolationsExceptionsCallback(IOException exception, int retryCount, int maxRetryCount, int waitTime);
    public static void WrapSharingViolations(Action action, WrapSharingViolationsExceptionsCallback? exceptionsCallback = null, int maxRetryCount = DefaultWrapSharingViolationsRetryCount, int waitTime = DefaultWrapSharingViolationsWaitTime)
    {
        ArgumentNullException.ThrowIfNull(action);
        for (var i = 0; i < maxRetryCount; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException ioe)
            {
                if (IsSharingViolation(ioe) && i < (maxRetryCount - 1))
                {
                    var wait = true;
                    if (exceptionsCallback != null)
                    {
                        wait = exceptionsCallback(ioe, i, maxRetryCount, waitTime);
                    }

                    if (wait)
                    {
                        Thread.Sleep(waitTime);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public static T? WrapSharingViolations<T>(Func<T> func, WrapSharingViolationsExceptionsCallback? exceptionsCallback = null, int maxRetryCount = DefaultWrapSharingViolationsRetryCount, int waitTime = DefaultWrapSharingViolationsWaitTime)
    {
        ArgumentNullException.ThrowIfNull(func);
        for (var i = 0; i < maxRetryCount; i++)
        {
            try
            {
                return func();
            }
            catch (IOException ioe)
            {
                if (IsSharingViolation(ioe) && i < (maxRetryCount - 1))
                {
                    var wait = true;
                    if (exceptionsCallback != null)
                    {
                        wait = exceptionsCallback(ioe, i, maxRetryCount, waitTime);
                    }

                    if (wait)
                    {
                        Thread.Sleep(waitTime);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        return default;
    }

    public static bool IsSharingViolation(IOException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        return exception.HResult == ERROR_SHARING_VIOLATION;
    }

    public static string? UrlCombine(params string[] urls)
    {
        if (urls == null)
            return null;

        var sb = new StringBuilder();
        foreach (var url in urls)
        {
            if (string.IsNullOrEmpty(url))
                continue;

            if (sb.Length > 0)
            {
                if (sb[^1] != '/' && url[0] != '/')
                {
                    sb.Append('/');
                }
            }
            sb.Append(url);
        }
        return sb.ToString();
    }

    private static readonly string[] _reservedFileNames =
    [
        "con", "prn", "aux", "nul",
        "com0", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
        "lpt0", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
    ];

    private static bool IsAllDots(string fileName)
    {
        foreach (char c in fileName)
        {
            if (c != '.')
                return false;
        }
        return true;
    }

    private static int GetDriveNameEnd(string path)
    {
        var pos = path.IndexOf(':');
        if (pos < 0)
            return -1;

        var pos2 = path.IndexOfAny([System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar]);
        if (pos2 < pos)
            return -1;

        return pos;
    }

    private static int GetServerNameEnd(string path, out bool onlyServer)
    {
        onlyServer = false;
        if (!path.StartsWith(@"\\"))
            return -1;

        var pos = path.IndexOf(System.IO.Path.DirectorySeparatorChar, 3);
        if (pos < 3)
            return -1;

        var pos2 = path.IndexOf(System.IO.Path.DirectorySeparatorChar, pos + 1);
        if (pos2 < pos)
        {
            onlyServer = true;
            return -1;
        }
        return pos2;
    }

    public static string PathToValidFilePath(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        var sb = new StringBuilder(filePath.Length);
        var fn = new StringBuilder();
        var serverNameEnd = GetServerNameEnd(filePath, out bool onlyServer);
        if (onlyServer)
            return filePath;

        var start = 0;
        if (serverNameEnd >= 0)
        {
            // path includes? server name? just skip it, don't validate it
            start = serverNameEnd + 1;
        }
        else
        {
            var driveNameEnd = GetDriveNameEnd(filePath);
            if (driveNameEnd >= 0)
            {
                start = driveNameEnd + 1;
            }
        }

        for (var i = start; i < filePath.Length; i++)
        {
            var c = filePath[i];
            if (c == System.IO.Path.DirectorySeparatorChar || c == System.IO.Path.AltDirectorySeparatorChar)
            {
                if (fn.Length > 0)
                {
                    sb.Append(PathToValidFileName(fn.ToString()));
                    fn.Length = 0;
                }
                sb.Append(c);
                continue;
            }

            fn.Append(c);
        }

        if (fn.Length > 0)
        {
            sb.Append(PathToValidFileName(fn.ToString()));
        }

        var s = start == 0 ? sb.ToString() : filePath[..start] + sb.ToString();
        if (s.EqualsIgnoreCase(filePath))
            return filePath;

        return s;
    }

    public static string PathToValidFileName(string fileName, string? reservedNameFormat = null, string? reservedCharFormat = null)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        reservedNameFormat = reservedNameFormat.Nullify() ?? "_{0}_";
        reservedCharFormat = reservedCharFormat.Nullify() ?? "_x{0}_";
        if (Array.IndexOf(_reservedFileNames, fileName.ToLowerInvariant()) >= 0 || IsAllDots(fileName))
            return string.Format(reservedNameFormat, fileName);

        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(fileName.Length);
        foreach (char c in fileName)
        {
            if (Array.IndexOf(invalid, c) >= 0)
            {
                sb.AppendFormat(reservedCharFormat, (short)c);
            }
            else
            {
                sb.Append(c);
            }
        }

        var s = sb.ToString();
        if (s.Length >= 255) // a segment is always 255 max even with long file names
        {
            s = s[..254];
        }

        if (s.EqualsIgnoreCase(fileName))
            return fileName;

        return s;
    }

    public static bool PathHasInvalidChars(string? path)
    {
        if (path == null)
            return true;

        for (var i = 0; i < path.Length; i++)
        {
            var c = path[i];
            if (c == 0x22 ||
                c == 0x3C ||
                c == 0x3E ||
                c == 0x7C ||
                c < 0x20)
                return true;
        }
        return false;
    }

    public static bool PathIsValidFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            return false;

        if (Array.IndexOf(_reservedFileNames, fileName.ToLowerInvariant()) >= 0)
            return false;

        return !IsAllDots(fileName);
    }

    public static bool IsPathRooted(string path)
    {
        if (path == null)
            return false;

        var length = path.Length;
        if (length < 1 || (path[0] != System.IO.Path.DirectorySeparatorChar && path[0] != System.IO.Path.AltDirectorySeparatorChar))
            return (length >= 2 && path[1] == System.IO.Path.VolumeSeparatorChar);

        return true;
    }

    public static string? PathCombineNoCheck(params string[] paths) => PathCombineNoCheck(System.IO.Path.DirectorySeparatorChar, paths);
    public static string? PathCombineNoCheck(char separator, params string[] paths)
    {
        if (paths == null)
            return null;

        if (paths.Length == 0)
            return null;

        if (paths.Length == 1)
            return paths[0];

        var sb = new StringBuilder();
        for (var i = 0; i < paths.Length; i++)
        {
            if (string.IsNullOrEmpty(paths[i]))
                continue;

            if (IsPathRooted(paths[i]))
            {
                sb = new StringBuilder(paths[i]);
                continue;
            }

            if (sb.Length > 0)
            {
                if (sb[^1] == separator)
                {
                    if (paths[i][0] == separator)
                    {
                        sb.Append(paths[i].AsSpan(1));
                    }
                    else
                    {
                        sb.Append(paths[i]);
                    }
                }
                else
                {
                    if (paths[i][0] == separator)
                    {
                        sb.Append(paths[i]);
                    }
                    else
                    {
                        sb.Append(separator);
                        sb.Append(paths[i]);
                    }
                }
            }
            else
            {
                sb.Append(paths[i]);
            }
        }
        return sb.ToString();
    }
}
