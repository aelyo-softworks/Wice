using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("Wice - Samples - Gallery")]
[assembly: AssemblyCompany("Aelyo Softworks")]
[assembly: AssemblyProduct("Wice")]
[assembly: AssemblyCopyright("Copyright (©) Aelyo Softworks 2020-2024. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("40474e94-07d8-42c1-8bad-2366a0b635d1")]

#if NET
[assembly: System.Runtime.Versioning.SupportedOSPlatform("Windows10.0.17763.0")]
#endif
