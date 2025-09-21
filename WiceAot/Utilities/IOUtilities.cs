using System.Security.Cryptography;

namespace Wice.Utilities;

/// <summary>
/// Utility helpers for common I/O scenarios:
/// - Safe file and directory operations that handle sharing violations and read-only attributes.
/// - Extraction of embedded assembly resources to disk with deterministic naming.
/// - Retry wrappers for <see cref="System.IO.IOException"/> sharing violations.
/// - Native DLL loading support (for non-.NET Framework targets).
/// </summary>
public static partial class IOUtilities
{
    /// <summary>
    /// The default number of retries performed by <see cref="WrapSharingViolations(System.Action, WrapSharingViolationsExceptionsCallback?, int, int)"/>
    /// and <see cref="WrapSharingViolations{T}(System.Func{T}, WrapSharingViolationsExceptionsCallback?, int, int)"/> when a sharing violation occurs.
    /// </summary>
    public const int DefaultWrapSharingViolationsRetryCount = 10;

    /// <summary>
    /// The default delay, in milliseconds, to wait between retries when handling sharing violations.
    /// </summary>
    public const int DefaultWrapSharingViolationsWaitTime = 100;

    /// <summary>
    /// Extracts an embedded resource from the calling assembly to the specified directory and returns the extracted file path.
    /// </summary>
    /// <param name="directoryPath">Destination directory where the resource should be extracted.</param>
    /// <param name="name">The full manifest resource name as present in the assembly.</param>
    /// <returns>The full path of the extracted file on disk.</returns>
    public static string ExtractAssemblyResource(string directoryPath, string name) => ExtractAssemblyResource(Assembly.GetCallingAssembly(), directoryPath, name);

    /// <summary>
    /// Extracts an embedded resource from the provided <paramref name="assembly"/> to the specified directory and returns the extracted file path.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource. If null, the calling assembly is used.</param>
    /// <param name="directoryPath">Destination directory where the resource should be extracted.</param>
    /// <param name="name">The full manifest resource name as present in the assembly.</param>
    /// <returns>The full path of the extracted file on disk.</returns>
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

    /// <summary>
    /// Ensures that the specified directory exists, creating it if necessary.
    /// </summary>
    /// <param name="directoryPath">The target directory path. Relative paths are resolved to full paths.</param>
    /// <returns>
    /// True if the directory was created; false if it already existed.
    /// </returns>
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

    /// <summary>
    /// Ensures that the directory containing the specified file path exists, creating it if necessary.
    /// </summary>
    /// <param name="filePath">The full path of a file whose parent directory must exist.</param>
    /// <returns>
    /// True if the directory was created; false if it already existed or no directory part could be determined.
    /// </returns>
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

    /// <summary>
    /// Moves a file to a new location, optionally removing read-only protection from the destination if it exists,
    /// and optionally swallowing errors.
    /// </summary>
    /// <param name="source">The source file to move.</param>
    /// <param name="destination">The destination file path.</param>
    /// <param name="unprotect">If true, removes <see cref="System.IO.FileAttributes.ReadOnly"/> from an existing destination file before deletion.</param>
    /// <param name="throwOnError">If true, exceptions are propagated; otherwise, returns false on failure.</param>
    /// <returns>True if the move succeeded; otherwise false when <paramref name="throwOnError"/> is false.</returns>
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

    /// <summary>
    /// Deletes a file if it exists, optionally removing read-only protection and optionally swallowing errors.
    /// </summary>
    /// <param name="filePath">The file path to delete.</param>
    /// <param name="unprotect">If true, removes <see cref="System.IO.FileAttributes.ReadOnly"/> before deletion.</param>
    /// <param name="throwOnError">If true, exceptions are propagated; otherwise, returns false on failure.</param>
    /// <returns>True if the file existed and was deleted; false if it did not exist or deletion failed with <paramref name="throwOnError"/> set to false.</returns>
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

    /// <summary>
    /// Safely determines whether a file exists at the specified path, returning false if any exception occurs.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
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

    /// <summary>
    /// Safely determines whether a directory exists at the specified path, returning false if any exception occurs.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
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

    /// <summary>
    /// Callback invoked when a sharing violation occurs during <see cref="WrapSharingViolations(System.Action, WrapSharingViolationsExceptionsCallback?, int, int)"/>
    /// or <see cref="WrapSharingViolations{T}(System.Func{T}, WrapSharingViolationsExceptionsCallback?, int, int)"/>.
    /// </summary>
    /// <param name="exception">The encountered <see cref="System.IO.IOException"/>.</param>
    /// <param name="retryCount">The current retry index (0-based).</param>
    /// <param name="maxRetryCount">The total number of allowed retries.</param>
    /// <param name="waitTime">The delay in milliseconds before the next retry if waiting.</param>
    /// <returns>
    /// True to wait <paramref name="waitTime"/> and retry; false to retry immediately without waiting.
    /// </returns>
    public delegate bool WrapSharingViolationsExceptionsCallback(IOException exception, int retryCount, int maxRetryCount, int waitTime);

    /// <summary>
    /// Executes an action and retries on <see cref="System.IO.IOException"/> sharing violations using a fixed delay.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="exceptionsCallback">Optional callback invoked on each sharing violation; can influence waiting strategy.</param>
    /// <param name="maxRetryCount">Maximum number of attempts before the exception is rethrown.</param>
    /// <param name="waitTime">Delay in milliseconds between retries when waiting.</param>
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

    /// <summary>
    /// Executes a function and retries on <see cref="System.IO.IOException"/> sharing violations using a fixed delay.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="exceptionsCallback">Optional callback invoked on each sharing violation; can influence waiting strategy.</param>
    /// <param name="maxRetryCount">Maximum number of attempts before the exception is rethrown.</param>
    /// <param name="waitTime">Delay in milliseconds between retries when waiting.</param>
    /// <returns>The value returned by <paramref name="func"/> when successful.</returns>
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

    /// <summary>
    /// Determines whether the specified <see cref="System.IO.IOException"/> represents a Windows sharing violation.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns>True if the exception is a sharing violation; otherwise, false.</returns>
    public static bool IsSharingViolation(IOException exception)
    {
        ExceptionExtensions.ThrowIfNull(exception, nameof(exception));
        const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        return exception.HResult == ERROR_SHARING_VIOLATION;
    }

#if !NETFRAMEWORK
    private const uint ERROR_MOD_NOT_FOUND = 0x8007007E;

    private static readonly ConcurrentDictionary<string, HRESULT> _initialized = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Ensures that a native DLL is loaded into the current process, optionally extracting it from an assembly's embedded resources.
    /// </summary>
    /// <param name="nativeDllName">The base name of the native DLL without extension (e.g., "foo" for "foo.dll").</param>
    /// <param name="assembly">
    /// Optional assembly to probe for embedded native DLL resources when the DLL is not present on disk.
    /// </param>
    /// <param name="throwOnError">
    /// If true, throws when loading fails or when the module was not found; otherwise, returns the failing <see cref="HRESULT"/>.
    /// </param>
    /// <returns>An <see cref="HRESULT"/> indicating success (<c>S_OK</c>) or failure.</returns>
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
