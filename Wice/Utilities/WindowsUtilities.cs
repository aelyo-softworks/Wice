using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wice.Utilities
{
    public static class WindowsUtilities
    {
        private static readonly ConcurrentDictionary<string, string> _loadedStrings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool AllocConsole();

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern bool FreeConsole();
#pragma warning restore CA1401 // P/Invokes should not be visible

        public static string LoadString(string libPath, int id, int lcid = -1)
        {
            if (libPath == null)
                throw new ArgumentNullException(nameof(libPath));

            if (lcid == -1) // default => current UI culture
            {
                lcid = Thread.CurrentThread.CurrentUICulture.LCID;
            }

            var key = lcid + "!" + id + "!" + libPath;
            if (_loadedStrings.TryGetValue(key, out var str))
                return str;

            const int LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20;
            var h = LoadLibraryEx(libPath, IntPtr.Zero, LOAD_LIBRARY_AS_IMAGE_RESOURCE);
            if (h == IntPtr.Zero)
                return key;

            var oldLcid = GetThreadUILanguage();
            SetThreadUILanguage((ushort)lcid);

            var ret = LoadString(h, id, out var ptr, 0);
            
            SetThreadUILanguage(oldLcid);

            if (ret == 0)
                return key;

            // each string starts with its size (a bit like a bstr)
            var len = Marshal.ReadInt16(ptr, -2);
            str = len > 0 ? Marshal.PtrToStringUni(ptr, len) : key;
            _loadedStrings[key] = str;
            FreeLibrary(h);
            return str;
        }

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int LoadString(IntPtr hInstance, int id, out IntPtr lpBuffer, int cchBufferMax);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, int dwFlags);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr FreeLibrary(IntPtr hLibModule);

        [DllImport("kernel32")]
        private static extern IntPtr SetThreadUILanguage(ushort LangId);

        [DllImport("kernel32")]
        private static extern ushort GetThreadUILanguage();
    }
}