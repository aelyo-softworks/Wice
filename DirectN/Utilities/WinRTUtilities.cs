using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Windows.Foundation.Metadata;
#if NET
using WinRT;
#endif

namespace DirectN
{
    public static class WinRTUtilities
    {
        private static readonly ConcurrentDictionary<ushort, bool> _apiContractAvailable = new ConcurrentDictionary<ushort, bool>();

        [DllImport("combase")]
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
#if NET
        private static extern HRESULT RoGetActivationFactory([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] string activatableClassId, [MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object factory);
#else
        private static extern HRESULT RoGetActivationFactory([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] string activatableClassId, [MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object factory);
#endif
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments

        public static T GetActivationFactory<T>(string activatableClassId, bool throwOnError = true) => (T)GetActivationFactory(activatableClassId, typeof(T).GUID, throwOnError);
        public static object GetActivationFactory(string activatableClassId, Guid iid, bool throwOnError = true)
        {
            if (activatableClassId == null)
                throw new ArgumentNullException(nameof(activatableClassId));

            RoGetActivationFactory(activatableClassId, iid, out var comp).ThrowOnError(throwOnError);
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

        public static T WinRTCast<T>(this object obj)
        {
            if (obj == null)
                return default;
#if NET

            var unk = Marshal.GetIUnknownForObject(obj);
            return MarshalInterface<T>.FromAbi(unk);
#else
            return (T)obj;
#endif
        }

        public static T ComCast<T>(this object obj)
        {
            if (obj == null)
                return default;
#if NET
            return obj.As<T>();
#else
            return (T)obj;
#endif
        }
    }
}
