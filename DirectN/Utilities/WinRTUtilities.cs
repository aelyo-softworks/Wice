﻿using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Windows.Foundation.Metadata;

namespace DirectN
{
    public static class WinRTUtilities
    {
        private static readonly ConcurrentDictionary<ushort, bool> _apiContractAvailable = new ConcurrentDictionary<ushort, bool>();

        [DllImport("combase")]
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        private static extern HRESULT RoGetActivationFactory([MarshalAs(UnmanagedType.HString)] string activatableClassId, [MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IInspectable)] out object factory);
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments

        public static T GetActivationFactory<T>(string activatableClassId, bool throwOnError = true) => (T)GetActivationFactory(activatableClassId, typeof(T).GUID, throwOnError);
        public static object GetActivationFactory(string activatableClassId, Guid iid, bool throwOnError = true)
        {
            if (activatableClassId == null)
                throw new ArgumentNullException(nameof(activatableClassId));

            var hr = RoGetActivationFactory(activatableClassId, iid, out var comp);
            hr.ThrowOnError(throwOnError);
            return comp;
        }

        public static bool Is19H1OrHigher => IsApiContractAvailable(8);

        public static bool IsApiContractAvailable(ushort version)
        {
            if (_apiContractAvailable.TryGetValue(version, out var available))
                return available;

            available = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", version);
            _apiContractAvailable[version] = available;
            return available;
        }
    }
}
