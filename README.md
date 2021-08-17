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
* It has no external dependencies, so it's free from deployment pain (read: UPW and current WinUI 3 packaging that takes hours to compile and "deploy" `<rant>` why should I need to "deploy" my apps at all?`</rant>`). Using .NET 5, you can even publish your app as a single zero-dependency .exe.
* It has the real Windows Acrylic (no hack!) brush w/o the need for UWP.
* It ships with an integrated in-process (Snoop-like for people familiar with WPF) visual Spy utility (just press F9 in debug mode).

# Status
Wice is still a work in progress. The base system is working quite well but it's not 100% finished.

Post an issue if you have a problem or a question, using sample reproducible code.

# List of projects
.NET Standard & Framework projects:
* **DirectN**: a .NET Standard 2 project that contains .NET interop and utility code for DXGI, WIC, DirectX 11, Direct2D, Direct Write, etc. It's a substract (with some additions and modifications) of this Open Source project: https://github.com/smourier/DirectN
* **Wice**: the Wice engine.
* **Wice.Samples.Gallery**: a sample demo / gallery project. **===>** This is what you should try if you're new to Wice.
* Wice.Tests: a test bench project. You shouldn't really use it
* Wice.DevTools: an internal tool that helps synchronize .NET 5 projects from .NET Framework projects.

.NET 5 projects:
* **DirectNCore**: this is the same as DirectN but compiled for .NET 5. All sources are linked to DirectN.
* **WiceCore**: this is the same as Wice but compiled for .NET 5. All sources are linked to Wice.
* **WiceCore.Samples.Gallery**: this is the same as Wice.Samples.Gallery but compiled for .NET 5. All sources are linked to Wice.Samples.Gallery.
* WiceCore.Tests: this is the same as Wice.Tests but compiled for .NET 5. All sources are linked to Wice.Tests. 