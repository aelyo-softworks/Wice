using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("Windows Interface Composition Engine")]
[assembly: AssemblyCompany("Aelyo Softworks")]
[assembly: AssemblyProduct("Wice")]
[assembly: AssemblyCopyright("Copyright (©) Aelyo Softworks 2020-2023. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("ef67ab82-0abc-4054-aada-293600d276d6")]

#if NET
[assembly: System.Runtime.Versioning.SupportedOSPlatform("Windows10.0.17763.0")]
#endif
