# What is it?

![Wice](Assets/wice_color.svg)

Windows Interface Composition Engine ("Wice") is a .NET UI engine for creating Windows application.

Here are the key points for Wice:

* It's available for the .NET Framework 4.7.2 and higher (use the "Wice" project).
* It's available for .NET 5 and higher (use the "WiceCore" project).
* It requires Windows 10 version 1809 ("Redstone 5").
* It's not cross-platform and was never intended to be.
* It's not dependent on WPF nor Winforms, nor WinUI 2 nor 3, nor Windows XAML, nor UWP, it's **another UI Framework**. The way it works is somewhat inspired from WPF, but there is no technical dependency over it.
* It's based on Windows UI Composition (aka Direct Composition) DirectX 11, Direct 2D and WIC, so it uses composition and doesn't redraw the screen all the time, only when needed.
* It has no editor, no descriptive markup langage, it's a code-only UI Framework.

# Why does it exist?
But... why Wice?

* It's 100% open source C# code, with something like 50000 lines of code (which is a relatively small code base).
* It's more modern than Winforms (GDI/GDI+) and WPF (still based on DirectX 9).
* It has no sandbox like UWP, and doesn't require the Win2D crap.
* It compiles much faster than UWP or WinUI projects because the tooling is the standard .NET tooling.
* It has no external dependencies, so it's free from deployment pain (read: UPW and current WinUI 3 packaging that takes hours to compile and "deploy" <rant> why should I need to "deploy" my apps? </rant>). Using .NET 5, you can even publish your app as a single zero-dependency .exe.
* It has the real Windows Acrylic (no hack!) brush w/o the need for UWP.
* It ships with an integrated in-process (Snoop-like for people familiar with WPF) visual Spy utility (just press F9 in debug mode).

# Status
Wice is still a work in progress. The base system is working quite well but it's not 100% finished.

Post an issue if you have a problem using the test and sample code.
