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
}
