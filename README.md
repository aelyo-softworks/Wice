# Wice

![Wice](Assets/wice_color.svg)

Windows Interface Composition Engine ("Wice") is a .NET UI engine for creating Windows application.

Here are the key points for Wice:

* It's available for the .NET Framework 4.7.2 and higher (use the "Wice" project).
* It's available for .NET 5 and higher (use the "WiceCore" project).
* It requires Windows 10 version 1809 ("Redstone 5").
* It's not cross-platform and was never intended to be.
* It's not dependent on WPF nor Winforms, nor WinUI 2 nor 3, nor Windows XAML, nor UWP, it's **another UI Framework**. The way it works is somewhat inspired from WPF, but there is no technical dependency over it.
* It's based on Windows UI Composition (aka Direct Composition) DirectX 11, Direct 2D and WIC, so it uses composition and doesn't redraw the screen all the time, only when needed.

But... why Wice?

* It's 100% open source C# code, with something like 50000 lines of code (which *is* small).
* It's much smaller than all other Microsoft-provider UI frameworks.
* It's more modern than Winforms (GDI/GDI+) and WPF (still based on DirectX 9).
* It has no sandbox like UWP, so it doesn't require Win2D.
* It compiles much faster than UWP or WinUI because the tooling is the standard .NET tooling.
* It's free from deployment pain (read: UPW and WinUI packaging that takes hours to compile and deploy). Using .NET 5, you can publish as a single zero-dependency .exe.
* It has the real Acrylic brush w/o the need for UWP.
* etc.

