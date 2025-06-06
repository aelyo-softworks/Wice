using WebView2;
using Wice.Interop;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WiceAot.Tests;

internal partial class TestWindow : Window
{
    public TestWindow()
    {
        //WindowsFrameMode = WindowsFrameMode.Merged;
        Style |= WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_MINIMIZEBOX;
        //SizeToContent = DimensionOptions.WidthAndHeight;
        //Native.EnableBlurBehind();
        //RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        RenderBrush = AcrylicBrush.CreateAcrylicBrush(
            CompositionDevice,
            D3DCOLORVALUE.White,
            0.2f,
            useWindowsAcrylic: false
            );

        //RichTextBoxNoLangOptions();
        //RichTextBoxNoLangOptions();
        //AddRepeatableMouseDown();
        //AddEditableTexts();
        //AddUniformGridShapes(20);
        //AddUniformColorGrid(20);
        //AddUniformGridImmersiveColors();
        //AddUniformGridSysColors();

        //ShowTabs();
        ShowBrowser();
        //LoadSvg();
        //Show64bppImageStream();
        //ShowWebView();
        //ShowPdfView();
        //ZoomableImageWithSV();
        //Pager();
        //ShowProgressBar();
        //LongRunWithCursor();
        //LargeRichTextBox();
        //RichTextBoxFont();
        //LargeText();
        //LargeTextSv();
        //BigText();
        //BigTextSv();

        //DisplayTime();
    }

#pragma warning disable IDE0052 // Remove unread private members
    private Timer? _timer;
#pragma warning restore IDE0052 // Remove unread private members
    private void DisplayTime()
    {
        var label = new TextBox();

        SetRight(label, 15);
        SetBottom(label, 15);
        Children.Add(label);
        _timer = new Timer(state =>
        {
            RunTaskOnMainThread(() =>
            {
                label.Text = DateTime.Now.ToString();
            });
        }, null, 0, 1000);
    }

    public void LoadSvg()
    {
        var stack = new Stack();
        Children.Add(stack);
        var btn = new Button();
        btn.Text.Text = "Load SVG File...";
        stack.Children.Add(btn);
        var svg = new SvgImage
        {
        };
        stack.Children.Add(svg);

        btn.Click += async (s, e) =>
        {

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".svg");

            InitializeWithWindow.Initialize(picker, Window!.Handle);
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await Window!.RunTaskOnMainThread(() => svg.Document = new FileStreamer(file.Path));
            }
        };
    }

    private void Btn_Click(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    public void Show64bppImageStream()
    {
        var img = new Image();
        var stream = File.OpenRead(@"Resources\hdr-image.jxr");
        img.Source = WicUtilities.LoadBitmapSource(stream);
        Children.Add(img);
    }

    public void AddRepeatableMouseDown()
    {
        var border = new Border
        {
            Height = 300,
            Width = 300,
            BorderThickness = 1,
            BackgroundColor = D3DCOLORVALUE.Red,
        };

        border.MouseButtonUp += (s, e) =>
        {
            Application.Trace("MouseUp: " + e.Point.ToString());
        };

        border.MouseButtonDown += (s, e) =>
        {
            Application.Trace("MouseDown: " + e.Point.ToString());
            unsafe
            {
                uint delay = 1;
                WiceCommons.SystemParametersInfoW(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETKEYBOARDDELAY, 0, (nint)(&delay), 0);
                e.RepeatDelay = 250 * (1 + delay);
                e.RepeatInterval = 50;
            }
        };
        Children.Add(border);
    }

    public void RichTextBoxVariousCharsets()
    {
        var wrap = new Wrap { Orientation = Orientation.Vertical };
        var BtnOpen = new Button { Margin = D2D_RECT_F.Thickness(0, 10), VerticalAlignment = Alignment.Near, HorizontalAlignment = Alignment.Near };
        BtnOpen.Text.Text = " Load TXT ";
        Dock.SetDockType(BtnOpen, DockType.Top);
        var sv = new ScrollViewer { Margin = D2D_RECT_F.Thickness(10, 10, 10, 10) };
        Dock.SetDockType(sv, DockType.Top);
        wrap.Children.Add(BtnOpen);
        wrap.Children.Add(sv);

        var text = File.ReadAllText(@"Resources\font.txt");
        var tb = new RichTextBox
        {
            DisposeOnDetachFromComposition = false,
            VerticalAlignment = Alignment.Near,
            Text = text,
            IsFocusable = true,
            FontSize = 12
        };
        tb.Options |= DirectN.Extensions.Utilities.TextHostOptions.WordWrap;
        sv.Viewer.Child = tb;

        BtnOpen.Click += (s, e) =>
        {
            var text = File.ReadAllText(@"Resources\font.txt");
            tb.Text = text;
            tb.Host.ResetCharFormat();
        };

        Children.Add(wrap);
    }

    public void RichTextBoxNoLangOptions()
    {
        var wrap = new Wrap { Orientation = Orientation.Vertical };
        var BtnOpen = new Button { Margin = D2D_RECT_F.Thickness(0, 10), VerticalAlignment = Alignment.Near, HorizontalAlignment = Alignment.Near };
        BtnOpen.Text.Text = " Load TXT ";
        Dock.SetDockType(BtnOpen, DockType.Top);
        var sv = new ScrollViewer { Margin = D2D_RECT_F.Thickness(10, 10, 10, 10) };
        Dock.SetDockType(sv, DockType.Top);
        wrap.Children.Add(BtnOpen);
        wrap.Children.Add(sv);

        var text = File.ReadAllText(@"Resources\Chinese-Traditional.txt");
        var tb = new RichTextBox
        {
            FontName = "Microsoft YaHei",
            DisposeOnDetachFromComposition = false,
            IsFocusable = true,
            FontSize = 12,
        };

        tb.SendMessage(DirectN.Extensions.Utilities.MessageDecoder.EM_SETLANGOPTIONS, new LPARAM { Value = 0 });
        tb.Text = text;

        tb.Options |= DirectN.Extensions.Utilities.TextHostOptions.WordWrap;
        sv.Viewer.Child = tb;

        BtnOpen.Click += (s, e) =>
        {
            tb.Text = text;
        };

        Children.Add(wrap);
    }

    public void ShowBrowser()
    {
        var pages = new string[]
            {
            "https://www.bing.com",
            "https://www.google.com",
            "https://www.microsoft.com",
            "https://www.github.com",
            "https://www.reddit.com",
            "https://www.stackoverflow.com",
        };

        var pagesIndex = 0;

        var tabs = new Tabs();
        tabs.PagesHeader.Spacing = new D2D_SIZE_F(5, 5);
        Children.Add(tabs);

        TabPage? plusPage = null;
        _ = addPage(null);

        plusPage = new TabPage { IsSelectable = false };
        tabs.Pages.Add(plusPage);
        plusPage.Header.AutoSelect = false;
        plusPage.Header.Icon.Text = DirectN.Extensions.Utilities.MDL2GlyphResource.Add;
        plusPage.Header.Text.Text = string.Empty;
        plusPage.Header.HorizontalAlignment = Alignment.Stretch;
        plusPage.Header.HoverRenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
        plusPage.Header.SelectedButtonClick += (s, e) => _ = addPage(null);

        async ValueTask<TabPage> addPage(ICoreWebView2NewWindowRequestedEventArgs? e)
        {
            var url = pages[pagesIndex++];
            if (pagesIndex >= pages.Length)
            {
                pagesIndex = 0;
            }

            var page = new TabPage();

            int index;
            if (plusPage != null)
            {
                index = plusPage.Index;
                tabs.Pages.Insert(index, page);
                page.Header.IsSelected = true;
            }
            else
            {
                index = tabs.Pages.Count;
                tabs.Pages.Add(page);
            }

            if (e != null)
            {
                var uri = PWSTR.Null;
                e.get_Uri(ref uri).ThrowOnError();
                page.Header.Text.Text = uri.ToString() ?? string.Empty;
            }
            else
            {
                page.Header.Text.Text = url;
            }

            page.Header.SelectedBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
            page.Header.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.DarkGray.ToColor());
            page.Header.HoverRenderBrush = Compositor.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
            page.Header.CloseButton!.IsVisible = true;
            page.Header.CloseButtonClick += (s, e) =>
            {
                tabs.Pages.Remove(page);
            };

            var wv = new WebView();
            wv.DocumentTitleChanged += (s, e) => page.Header.Text.Text = e.Value ?? url;
            wv.NewWindowRequested += (s, e) =>
            {
                _ = addPage(e.Value);
            };
            page.Content = wv;
            if (e != null)
            {
                e.GetDeferral(out var deferral).ThrowOnError();
                e.put_Handled(true).ThrowOnError();
                var wv2 = await wv.EnsureWebView2Loaded();
                e.put_NewWindow(wv2!.Object).ThrowOnError();
                deferral.Complete().ThrowOnError();
            }
            else
            {
                wv.SourceUri = url;
            }

            return page;
        }
    }

    public void ShowTabs()
    {
        var tabs = new Tabs();
        tabs.PagesHeader.Spacing = new D2D_SIZE_F(5, 5);
        //tabs.PagesHeader.LastChildFill = true;
        tabs.VerticalAlignment = Alignment.Near;
        Children.Add(tabs);

        TabPage? plusPage = null;
        addPage();
        addPage();
        addPage();

        plusPage = new TabPage();
        tabs.Pages.Add(plusPage);
        plusPage.Header.AutoSelect = false;
        plusPage.Header.Icon.Text = DirectN.Extensions.Utilities.MDL2GlyphResource.Add;
        plusPage.Header.Text.Text = string.Empty;
        plusPage.Header.HorizontalAlignment = Alignment.Stretch;
        plusPage.Header.HoverRenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
        plusPage.Header.SelectedButtonClick += (s, e) => addPage();

        TabPage addPage()
        {
            var page = new TabPage();

            int index;
            if (plusPage != null)
            {
                index = plusPage.Index;
                tabs.Pages.Insert(index, page);
                page.Header.IsSelected = true;
            }
            else
            {
                index = tabs.Pages.Count;
                tabs.Pages.Add(page);
            }

            page.Header.Name = "tp" + index;
            page.Header.Text.Text = "Page " + index;
            page.Header.SelectedBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
            page.Header.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.DarkGray.ToColor());
            page.Header.HoverRenderBrush = Compositor.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
            page.Header.CloseButton!.IsVisible = true;
            page.Header.CloseButtonClick += (s, e) =>
            {
                tabs.Pages.Remove(page);
            };
            return page;
        }
    }

    public void ShowPdfView()
    {
        var pdfView = new PdfView
        {
            SourceFilePath = @"resources\sample.pdf",
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10)
        };
        Children.Add(pdfView);
    }

    public void ShowWebView()
    {
        var webView = new WebView
        {
            SourceUri = "https://www.bing.com",
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10)
        };
        Children.Add(webView);
    }

    public void ZoomableImageWithSV()
    {
        var sv = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        sv.Viewer.IsWidthUnconstrained = false;
        sv.Margin = D2D_RECT_F.Thickness(10);
        Dock.SetDockType(sv, DockType.Top);

        var img = new Image { VerticalAlignment = Alignment.Near, HorizontalAlignment = Alignment.Center };
        sv.Viewer.Children.Add(img);
        Children.Add(sv);

        img.MouseWheel += (s, e) =>
        {
            if (!NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL))
                return;

            const float zoomStepPercent = 0.05f;
            img.Zoom += e.Delta * zoomStepPercent;
        };

        img.Source = Application.CurrentResourceManager.GetWicBitmapSource(@"resources\rainier.jpg");
    }

    public void Pager()
    {
        var lines = File.ReadAllLines(@"Resources\MobyDickNumbered.txt");

        var pagesCount = lines.Length / 1000;
        if (lines.Length % 1000 != 0)
        {
            pagesCount++;
        }

        var pages = new string[pagesCount];
        for (var i = 0; i < pagesCount; i++)
        {
            pages[i] = string.Join(Environment.NewLine, lines.Skip(i * lines.Length / pagesCount).Take(lines.Length / pagesCount));
        }

        var dock = new Dock { LastChildFill = true, HorizontalAlignment = Alignment.Stretch };
        Children.Add(dock);

        var buttons = new Stack { Orientation = Orientation.Horizontal, HorizontalAlignment = Alignment.Stretch };
        Dock.SetDockType(buttons, DockType.Top);
        dock.Children.Add(buttons);

        var sv = new ScrollViewer();
        sv.Viewer.IsWidthUnconstrained = false;
        Dock.SetDockType(sv, DockType.Bottom);
        dock.Children.Add(sv);

        var rtb = new RichTextBox();

        rtb.Document!.Object.GetDocumentFont(out var obj).ThrowOnError();
        using var font = new ComObject<ITextFont2>(obj);
        font.Object.SetSize(12).ThrowOnError();
        using var bstr = new DirectN.Extensions.Utilities.Bstr("Consolas");
        font.Object.SetName(bstr).ThrowOnError();
        rtb.Document.Object.SetDocumentFont(font.Object).ThrowOnError();

        rtb.VerticalAlignment = Alignment.Near;
        rtb.Text = pages[0];
        sv.Viewer.Child = rtb;

        rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        //rtb.FontName = "Consolas";
        //rtb.FontSize = 18;

        foreach (var i in Enumerable.Range(0, pagesCount))
        {
            var btn = new Button { Margin = 8 };
            btn.Text.Text = (i + 1).ToString();
            btn.Click += (s, e) =>
            {
                rtb.Text = pages[i];

                // optionally scroll to left/top
                //sv.HorizontalOffset = 0;
                //sv.VerticalOffset = 0;
            };
            buttons.Children.Add(btn);
        }
    }

    public void LargeRichTextBox()
    {
        var sv = new ScrollViewer
        {
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10)
        };

        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        //var text = File.ReadAllText(@"Resources\ShortText.txt");
        //var text = File.ReadAllText(@"Resources\MobyDickNumbered.txt");
        var tb = new RichTextBox
        {
            VerticalAlignment = Alignment.Near,
            Text = text,
            IsFocusable = true,
        };

        sv.Viewer.Child = tb;

        Children.Add(sv);

        ITextRange? range = null;
        tb.MouseButtonDown += (s, e) =>
        {
            var pt = tb.Window!.ClientToScreen(e.Point);
            tb.Document!.Object.RangeFromPoint(pt.x, pt.y, out range).ThrowOnError();
        };

        tb.MouseMove += (s, e) =>
        {
            if (range != null)
            {
                var pt = tb.Window!.ClientToScreen(e.Point);
                tb.Document!.Object.RangeFromPoint(pt.x, pt.y, out var range2).ThrowOnError();
                range2.GetStart(out var end).ThrowOnError();
                range2.FinalRelease();
                range.SetEnd(end).ThrowOnError();
                range.Select();
            }
        };

        string? selection = null;
        tb.MouseButtonUp += (s, e) =>
        {
            selection = null;
            if (range != null)
            {
                range.GetText(out BSTR pbstr);
                if (pbstr.Value != 0)
                {
                    selection = pbstr.ToString();
                    BSTR.Dispose(ref pbstr);
                }

                range.FinalRelease();
                range = null;
            }

            ReleaseMouseCapture();
        };

        tb.KeyDown += (s, e) =>
        {
            if (e.WithControl && e.Key == VIRTUAL_KEY.VK_C && !string.IsNullOrEmpty(selection))
            {
                DirectN.Extensions.Utilities.Clipboard.SetText(selection);
            }
        };
    }

    public void RichTextBoxFont()
    {
        var sv = new ScrollViewer
        {
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10)
        };

        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        //var text = File.ReadAllText(@"Resources\ShortText.txt");
        //var text = File.ReadAllText(@"Resources\MobyDickNumbered.txt");
        var tb = new RichTextBox
        {
            VerticalAlignment = Alignment.Near,
            Text = text,
            IsFocusable = true,
        };

        sv.Viewer.Child = tb;

        Children.Add(sv);

        var button = new Button();
        button.Text.Text = "click to change font size & name";
        button.Click += (s, e) =>
        {
            tb.Document!.Object.GetDocumentFont(out var font).ThrowOnError();
            using var f = new ComObject<ITextFont2>(font);
            font.SetSize(26).ThrowOnError();

            using var bstr = new DirectN.Extensions.Utilities.Bstr("Consolas");
            font.SetName(bstr).ThrowOnError();

            tb.Document!.Object.SetDocumentFont(font).ThrowOnError();
            button.Remove();
        };
        Children.Add(button);
    }

    public void LargeText()
    {
        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        var tb = new FastTextBox
        {
            VerticalAlignment = Alignment.Near,
            Text = text,
            IsFocusable = true,
        };
        Children.Add(tb);
    }

    public void LargeTextSv()
    {
        var btn = new Button
        {
            VerticalAlignment = Alignment.Near,
            HorizontalAlignment = Alignment.Near,
        };

        btn.Text.Text = "click me";

        var sv = new ScrollViewer();
        sv.Viewer.IsWidthUnconstrained = false;

        var text = File.ReadAllText(@"Resources\MobyDickNumbered.txt");
        var tb = new FastTextBox
        {
            VerticalAlignment = Alignment.Near,
            HorizontalAlignment = Alignment.Near,
            Text = text,
        };

        sv.Viewer.Child = tb;
        sv.Margin = D2D_RECT_F.Thickness(10, 10, 10, 10);

        Children.Add(sv);

        btn.Click += (s, e) =>
        {
            if (tb.Text.Contains("Moby"))
            {
                tb.Text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
            }
            else
            {
                tb.Text = File.ReadAllText(@"Resources\MobyDickNumbered.txt");
            }
            sv.VerticalOffset = 0;
        };

        Children.Add(btn);
    }

    public void BigText()
    {
        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        var tb = new TextBox
        {
            FontFamilyName = "Consolas",
            IsEditable = true,
        };

        tb.PropertyChanged += (s, e) =>
        {
            if (TextBox.TextProperty.Name == e.PropertyName)
            {
            }
        };

        tb.Text = text;

        Children.Add(tb);
    }

    public void BigTextSv()
    {
        var sv = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        //sv.Viewer.IsWidthUnconstrained = false;
        //sv.Viewer.IsHeightUnconstrained = false;

        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        text = File.ReadAllText(@"C:\Users\simon\Downloads\contents\contents.txt");
        //text = "These are colored emoji: 😝👸🎅👨‍👩‍👧‍👦";

        var txt = new TextBox
        {
            FontSize = 16,
            Padding = D2D_RECT_F.Thickness(10, 10, 10, 10),
            VerticalAlignment = Alignment.Near,
            HorizontalAlignment = Alignment.Near,
            //WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER,

            FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED,

            IsEditable = true,
            AcceptsReturn = true,
            AcceptsTab = true,
            Text = text,
            //LineSpacing = new DWRITE_LINE_SPACING { baseline = 16, height = 16, method = DWRITE_LINE_SPACING_METHOD.DWRITE_LINE_SPACING_METHOD_UNIFORM },
            Name = "txt",
        };

        sv.Viewer.Child = txt;
        sv.Margin = D2D_RECT_F.Thickness(10, 10, 10, 10);

        Children.Add(sv);
        //Children.Add(txt);

        var btn = new Button
        {
            VerticalAlignment = Alignment.Near,
            Name = "btn"
        };
        btn.Text.Text = "click";
        //Children.Add(btn);
    }

    public void ShowProgressBar()
    {
        // do something in background
        var clock = new TextBox
        {
            HorizontalAlignment = Alignment.Center,
            FontSize = 40
        };
        Children.Add(clock);

        var timer = new Timer((state) => RunTaskOnMainThread(() =>
        {
            clock.Text = DateTime.Now.ToString();
        }), null, 0, 1000);

        var cancelled = false;

        // show progress bar like (modal)
        var dlg = new DialogBox();
        dlg.TitleBar.IsVisible = false;
        var cancel = dlg.AddCancelButton();
        cancel.Click += (s, e) => cancelled = true;

        // add some visual in the dialog box
        var bar = new Border
        {
            BorderThickness = 1,
            BackgroundColor = D3DCOLORVALUE.Green,
            Width = 200,
            Height = 20,
            Padding = 5
        };
        dlg.DialogContent.Children.Add(bar);

        // make sure user can't close while we're processing
        Closing += (s, e) => e.Cancel = !cancelled;

        // do our processing in background
        Task.Run(async () =>
        {
            do
            {
                // some delay
                await Task.Delay(1000);
                _ = RunTaskOnMainThread(() =>
                {
                    // rotate color
                    var hsv = Hsv.From(bar.BackgroundColor.Value);
                    hsv.Hue += 10;
                    bar.BackgroundColor = hsv.ToD3DCOLORVALUE();
                });
            }
            while (!cancelled);
            timer.Dispose();
            _ = RunTaskOnMainThread(() => clock.Text = "Cancelled");
        });

        Children.Add(dlg);
    }

    public void LongRunWithCursor()
    {
        var label = new TextBox();

        SetRight(label, 15);
        SetBottom(label, 15);
        Children.Add(label);

        Cursor = DirectN.Extensions.Utilities.Cursor.Wait;
        var seconds = 5;
        _timer = new Timer(state =>
        {
            RunTaskOnMainThread(() =>
            {
                if (seconds == 0)
                {
                    label.Text = string.Empty;
                    _timer?.Dispose();
                    Cursor = null;
                }
                else
                {
                    label.Text = seconds.ToString();
                    seconds--;
                }
            });
        }, null, 0, 1000);
    }

    public void AddUniformGridSysColors()
    {
        var grid = new UniformGrid
        {
            BackgroundColor = D3DCOLORVALUE.Transparent,
            Name = "ugrid"
        };
        Children.Add(grid);

        var colors = DirectN.Extensions.Utilities.ColorUtilities.SysColors.ToArray();
        grid.Rows = colors.Length;
        grid.Columns = 3;

        foreach (var color in colors)
        {
            var text = new TextBox();
            grid.Children.Add(text);
            //var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            //text.BackgroundColor = D3DCOLORVALUE.Transparent;
            //text.ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.LightGoldenrodYellow);
            text.Text = color.Item1.ToString();

            var html = new TextBox();
            grid.Children.Add(html);
            //var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            html.BackgroundColor = D3DCOLORVALUE.Transparent;
            html.ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.Gold);
            html.Text = color.Item2.ToString();

            var border = new Border();
            grid.Children.Add(border);
            border.RenderBrush = Compositor!.CreateColorBrush(color.Item2.ToColor());
        }
    }


    public void AddUniformGridImmersiveColors()
    {
        var grid = new UniformGrid
        {
            BackgroundColor = D3DCOLORVALUE.Transparent,
            Name = "ugrid"
        };
        Children.Add(grid);

        var max = 50;
        var colors = DirectN.Extensions.Utilities.ColorUtilities.ImmersiveColors;
        grid.Rows = max;
        grid.Columns = 3;

        int i = 0;
        foreach (var color in colors)
        {
            var text = new TextBox();
            grid.Children.Add(text);
            //var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            //text.BackgroundColor = D3DCOLORVALUE.Transparent;
            //text.ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.LightGoldenrodYellow);
            text.FontSize = 10;
            text.Text = color.Value.Name;

            var html = new TextBox();
            grid.Children.Add(html);
            //var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            html.FontSize = text.FontSize;
            html.Text = color.Value.Color.HtmlString;

            var border = new Border();
            grid.Children.Add(border);
            border.RenderBrush = Compositor!.CreateColorBrush(color.Value.Color.ToColor());
            i++;
            if (i == max)
                break;
        }
    }

    public void AddUniformColorGrid(int size)
    {
        var visual = new UniformGrid
        {
            Name = "ugrid",
            Rows = size
        };
        visual.Columns = visual.Rows;
        Children.Add(visual);

        for (var i = 0; i < visual.Rows; i++)
        {
            for (var j = 0; j < visual.Columns; j++)
            {
                addCell(i, j);
            }
        }

        KeyDown += (s, e) =>
        {
            if (e.Key == VIRTUAL_KEY.VK_R)
            {
                visual.Rows++;
            }
            else if (e.Key == VIRTUAL_KEY.VK_C)
            {
                visual.Columns++;
            }
            else if (e.Key == VIRTUAL_KEY.VK_A)
            {
                var i = visual.Children.Count / visual.Rows;
                var j = visual.Children.Count % visual.Columns;
                addCell(i, j);
            }
        };

        void addCell(int i, int j)
        {
            var cell = new Border
            {
                Name = "cell " + i + "x" + j
            };
            visual.Children.Add(cell);
            var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            cell.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
        }
    }

    public void AddUniformGridShapes(int size)
    {
        var visual = new UniformGrid
        {
            BackgroundColor = D3DCOLORVALUE.Transparent,
            Name = "ugrid",
            Rows = size
        };
        visual.Columns = visual.Rows;
        Children.Add(visual);

        for (var i = 0; i < visual.Rows; i++)
        {
            for (var j = 0; j < visual.Columns; j++)
            {
                addLine(i, j);
                //addEllipse(i, j);
            }
        }

        KeyDown += (s, e) =>
        {
            var i = visual.Children.Count / visual.Rows;
            var j = visual.Children.Count % visual.Columns;
            if (e.Key == VIRTUAL_KEY.VK_R)
            {
                visual.Rows++;
            }
            else if (e.Key == VIRTUAL_KEY.VK_C)
            {
                visual.Columns++;
            }
            else if (e.Key == VIRTUAL_KEY.VK_T)
            {
                addRoundedRectangle(i, j);
            }
            else if (e.Key == VIRTUAL_KEY.VK_L)
            {
                addLine(i, j);
            }
            else
            {
                addEllipse(i, j);
            }
        };

        void addEllipse(int i, int j)
        {
            var shape = new Ellipse();
            visual.Children.Add(shape);
            var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            shape.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
            shape.Shape!.StrokeBrush = Compositor.CreateColorBrush(color.ToColor());
            shape.Shape.StrokeThickness = 0.5f;
        }

        void addLine(int i, int j)
        {
            var shape = new Line();
            visual.Children.Add(shape);
            var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            shape.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
            shape.Shape!.StrokeBrush = Compositor.CreateColorBrush(color.ToColor());
            shape.Shape.StrokeThickness = 0.5f;
            shape.Arranged += (s, e) =>
            {
                shape.Geometry!.End = shape.ArrangedRect.Size.ToVector2();
            };
        }

        void addRoundedRectangle(int i, int j)
        {
            var shape = new RoundedRectangle();
            visual.Children.Add(shape);
            var color = new D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            shape.Geometry!.CornerRadius = new Vector2(10);
            shape.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
            shape.Shape!.StrokeBrush = Compositor.CreateColorBrush(color.ToColor());
            shape.Shape.StrokeThickness = 0.5f;
        }
    }


    public void AddEditableTexts()
    {
        var stack = new Dock
        {
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LemonChiffon.ToColor())
        };
        Children.Add(stack);

        var triggers = Enum.GetValues<EventTrigger>();
        for (var i = 0; i < triggers.Length; i++)
        {
            var text = new TextBox();
            Dock.SetDockType(text, DockType.Top);
            text.IsEditable = true;

            text.TextChangedTrigger = triggers[(i + 2) % triggers.Length];
            text.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
            text.Name = "text#" + i;
            text.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Red);
            stack.Children.Add(text);
            text.Padding = D2D_RECT_F.Thickness(10);
            text.Margin = D2D_RECT_F.Thickness(10);
            text.Text = "Change this text #" + i + " trigger:" + text.TextChangedTrigger;
            var j = i;
            text.TextChanged += (s, e) =>
            {
                Application.Trace("Text #" + j + " changed: " + text.Text);
            };
        }
    }

    public void AddWrap()
    {
        var wrap = new Wrap
        {
            Orientation = Orientation.Horizontal
        };
        Children.Add(wrap);

        var r = new Random(Environment.TickCount);
        var max = 1000;
        for (var i = 0; i < max; i++)
        {
            var b = new Border
            {
                Name = "border" + i
            };
            wrap.Children.Add(b);
            b.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Green.ToColor());
            var color = new D3DCOLORVALUE(0, 1 - (i / (float)max), 1 - (i / (float)max));
            b.RenderBrush = Compositor.CreateColorBrush(color.ToColor());
            b.Width = r.Next(10, 60);
            b.Height = r.Next(10, 60);
        }

        KeyDown += (s, e) =>
        {
            if (e.Key == VIRTUAL_KEY.VK_S)
            {
                if (wrap.Orientation == Orientation.Horizontal)
                {
                    wrap.Orientation = Orientation.Vertical;
                }
                else
                {
                    wrap.Orientation = Orientation.Horizontal;
                }
            }
        };
    }
}
