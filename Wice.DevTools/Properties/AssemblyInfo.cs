using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("Windows Interface Composition Engine - DevTools")]
[assembly: AssemblyCompany("Aelyo Softworks")]
[assembly: AssemblyProduct("Wice")]
[assembly: AssemblyCopyright("Copyright (©) Aelyo Softworks 2020-2023. All rights reserved.")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("e109055f-baef-4e26-9d3f-be7cdf7e04e6")]
