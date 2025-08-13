using System.Security.Cryptography;

namespace Wice.Utilities;

public static partial class IOUtilities
{
    public const int DefaultWrapSharingViolationsRetryCount = 10;
    public const int DefaultWrapSharingViolationsWaitTime = 100;

    public static string ExtractAssemblyResource(string directoryPath, string name) => ExtractAssemblyResource(Assembly.GetCallingAssembly(), directoryPath, name);
    public static string ExtractAssemblyResource(Assembly assembly, string directoryPath, string name)
    {
        ExceptionExtensions.ThrowIfNull(name, nameof(name));
        ExceptionExtensions.ThrowIfNull(directoryPath, nameof(directoryPath));
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
        ExceptionExtensions.ThrowIfNull(directoryPath, nameof(directoryPath));
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
        ExceptionExtensions.ThrowIfNull(filePath, nameof(filePath));
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
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        ExceptionExtensions.ThrowIfNull(destination, nameof(destination));
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
        ExceptionExtensions.ThrowIfNull(filePath, nameof(filePath));
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

    public delegate bool WrapSharingViolationsExceptionsCallback(IOException exception, int retryCount, int maxRetryCount, int waitTime);
    public static void WrapSharingViolations(Action action, WrapSharingViolationsExceptionsCallback? exceptionsCallback = null, int maxRetryCount = DefaultWrapSharingViolationsRetryCount, int waitTime = DefaultWrapSharingViolationsWaitTime)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
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
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
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
        ExceptionExtensions.ThrowIfNull(exception, nameof(exception));
        const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        return exception.HResult == ERROR_SHARING_VIOLATION;
    }

#if !NETFRAMEWORK
    private const uint ERROR_MOD_NOT_FOUND = 0x8007007E;
    private static readonly ConcurrentDictionary<string, HRESULT> _initialized = new(StringComparer.OrdinalIgnoreCase);

    // assembly can be present in files or in assemblies embedded resources
    public static HRESULT EnsureNativeDllLoaded(string nativeDllName, Assembly? assembly = null, bool throwOnError = true)
    {
        if (!_initialized.TryGetValue(nativeDllName, out var hr))
        {
            hr = EnsureNativeDllLoaded(nativeDllName, assembly);
            if (hr == ERROR_MOD_NOT_FOUND && throwOnError)
                throw new Exception($"Cannot load {nativeDllName}.dll. Make sure it's present in the current's process path.");

            hr.ThrowOnError(throwOnError);
            _initialized[nativeDllName] = hr;
        }
        return hr;
    }

    private static HRESULT EnsureNativeDllLoaded(string nativeDllName, Assembly? assembly)
    {
        HMODULE h;
        if (assembly != null)
        {
            var asmPath = GetDllPathFromAssemblyResources(assembly, nativeDllName);
            if (asmPath != null)
            {
                h = Functions.LoadLibraryW(PWSTR.From(asmPath));
                if (h.Value != 0)
                    return Constants.S_OK;

                return Marshal.GetHRForLastWin32Error();
            }
        }

        string? firstPath = null;
        foreach (var path in GetPossiblePaths(nativeDllName))
        {
            firstPath ??= path;
            h = Functions.LoadLibraryW(PWSTR.From(path));
            if (h.Value != 0)
                return Constants.S_OK;
        }

        if (firstPath == null)
            return ERROR_MOD_NOT_FOUND;

        h = Functions.LoadLibraryW(PWSTR.From(firstPath));
        if (h.Value != 0)
            return Constants.S_OK;

        return Marshal.GetHRForLastWin32Error();
    }

    private static IEnumerable<string> GetPossiblePaths(string nativeDllName)
    {
        if (Environment.ProcessPath == null)
            yield break;

        var processDir = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
        if (processDir == null)
            yield break;

        var name = nativeDllName + ".dll";
        var path = System.IO.Path.Combine(processDir, name);
        if (File.Exists(path))
            yield return path;

        var arch = RuntimeInformation.ProcessArchitecture;
        switch (arch)
        {
            case Architecture.Arm64:
            case Architecture.X64:
            case Architecture.X86:
                path = System.IO.Path.Combine(processDir, "runtimes", "win-" + arch.ToString().ToLowerInvariant(), "native", name);
                if (File.Exists(path))
                    yield return path;

                break;
        }
    }

    private static string? GetDllPathFromAssemblyResources(Assembly assembly, string nativeDllName)
    {
        var arch = RuntimeInformation.ProcessArchitecture.ToString();
        var names = assembly.GetManifestResourceNames();

        var dllName = nativeDllName + ".dll";
        // first check with arch
        var name = names.FirstOrDefault(n => n.Contains(arch, StringComparison.OrdinalIgnoreCase) && n.EndsWith(dllName, StringComparison.OrdinalIgnoreCase));
        if (name == null)
        {
            // fallback to any that's not arch specific
            var allArchs = new[] { "x86", "x64", "arm64" };
            name = names.FirstOrDefault(n => allArchs.All(a => !n.Contains(a)) && n.EndsWith(dllName, StringComparison.OrdinalIgnoreCase));
        }
        if (name == null)
            return null;

        // come up with some local unique temp dir
        var key = $"{assembly.FullName}, {name}";
        var id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(key)));
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), id.ToString(), dllName);
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null || stream.Length < 512) // bug of some sort
            return null;

        var fi = new FileInfo(tempPath);
        if (!fi.Exists || fi.Length != stream.Length)
        {
            var dir = System.IO.Path.GetDirectoryName(tempPath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.CopyTo(file);
        }
        return tempPath;
    }
#endif
}
