using WebView2;
using Wice.Interop;
using Wice.Samples.Gallery.Samples.Collections.PropertyGrid;
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

        //AddDocks();
        AddSlider();
        //AddTicks();
        //AddPropertyGrid();
        //AddLogVisual();
        //AddFastLogVisual();
        //ShowTabs();
        //AddScrollableRtbRtfFile();
        //ChoosePdfView();
        //ShowBrowser();
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

    private sealed class MyDock : Dock
    {
        private readonly TextBox _tbLeft;
        private readonly TextBox _tbRight;
        private readonly Border _th;
        private readonly Border _ticks;

        public MyDock()
        {
            //ClipChildren = false;
            //dock.ClipFromParent = false;

            Height = 100;
            DoWhenAttachedToComposition(() => RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor()));

            _tbLeft = new TextBox
            {
                Height = 50,
                Text = "Left",
                VerticalAlignment = Alignment.Near,
                HorizontalAlignment = Alignment.Near,
                Padding = 10,
                ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
            };
            SetDockType(_tbLeft, DockType.Left);
            _tbLeft.DoWhenAttachedToComposition(() => _tbLeft.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor()));
            Children.Add(_tbLeft);

            var b1 = new Border
            {
                Width = 50,
                Height = 50,
                VerticalAlignment = Alignment.Near,
                HorizontalAlignment = Alignment.Near
            };
            SetDockType(b1, DockType.Left);
            b1.DoWhenAttachedToComposition(() => b1.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Violet.ToColor()));
            Children.Add(b1);

            _th = new Border
            {
                Width = 50,
                Height = 50,
                VerticalAlignment = Alignment.Near,
                HorizontalAlignment = Alignment.Near
            };
            SetDockType(_th, DockType.Left);
            _th.DoWhenAttachedToComposition(() => _th.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Green.ToColor()));
            Children.Add(_th);

            _tbRight = new TextBox
            {
                Height = 50,
                Text = "Right",
                VerticalAlignment = Alignment.Near,
                HorizontalAlignment = Alignment.Far,
                Padding = 10,
                ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
            };
            SetDockType(_tbRight, DockType.Right);
            _tbRight.DoWhenAttachedToComposition(() => _tbRight.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor()));
            Children.Add(_tbRight);

            var b2 = new Border
            {
                Width = 50,
                Height = 50,
                VerticalAlignment = Alignment.Near,
                HorizontalAlignment = Alignment.Far
            };
            SetDockType(b2, DockType.Right);
            b2.DoWhenAttachedToComposition(() => b2.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()));
            Children.Add(b2);

            _ticks = new Border
            {
                Height = 50,
                Width = 2000,
                VerticalAlignment = Alignment.Far,
                HorizontalAlignment = Alignment.Near
            };
            SetDockType(_ticks, DockType.Bottom);
            _ticks.Rendered += (s, e) =>
            {
                //_ticks.CompositionVisual.Offset = new Vector3(tbLeft.CompositionVisual.Size.X, 50, 0);
                //_ticks.CompositionVisual.Size = new Vector2(
                //    CompositionVisual.Size.X - tbLeft.CompositionVisual.Size.X - tbRight.CompositionVisual.Size.X, _ticks.CompositionVisual.Size.Y);
            };
            _ticks.DoWhenAttachedToComposition(() => _ticks.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Pink.ToColor()));
            Children.Add(_ticks);
        }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var size = base.MeasureCore(constraint);
            //_th.Width = 100;
            return size;
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            base.ArrangeCore(finalRect);
            var ar = _ticks.ArrangedRect;
            _ticks.Width = Math.Max(0, _tbRight.ArrangedRect.left - _tbLeft.ArrangedRect.right);
            ar.left = _tbLeft.ArrangedRect.right;
            ar.right = _tbRight.ArrangedRect.left;
            _ticks.Arrange(ar);
        }
    }

    public void AddDocks()
    {
        var dock = new MyDock();
        Children.Add(dock);
    }

    public void AddTicks()
    {
        var cv = new Canvas();
        Children.Add(cv);
        cv.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        for (var i = 0; i < 10; i++)
        {
            var tick = new Border
            {
                Width = 2,
                Height = 50,
                RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor())
            };
            cv.Children.Add(tick);

            SetLeft(tick, i * 20);
            //SetRight(tick, 2 + i * 20);
        }
    }

    public void AddSlider()
    {
        //var s = new Stack { Orientation = Orientation.Vertical };
        //var b0 = new Border { Width = 50, Height = 50 };
        //b0.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());
        //s.Children.Add(b0);

        //var b1 = new Border { Width = 50, Height = 50 };
        //b1.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());
        //s.Children.Add(b1);
        //Children.Add(s);
        //return;
        //EnableMouseEventTraces = true;
        //var sl1 = new HorizontalSlider<float> { Margin = 10, Value = .33f };
        var sl1 = new Slider<int> { Margin = 10, Value = 0, MinValue = 0, MaxValue = 100 };
        //var sl1 = new Slider<float> { Margin = 10 };
        //sl1.TicksStep = 10;
        //sl1.TicksOptions |= SliderTicksOptions.ShowTickValues;
        sl1.SnapToTicks = true;
        //sl1.Orientation = Orientation.Vertical;
        //sl1.TextOrientation = Orientation.Vertical;

        //var sl1 = new VerticalSlider<int> { Margin = 10, Value = 10 };
        //var sl1 = new EllipseSlider { Margin = 10, Value = 1 };

        //sl1.MinValueVisual.IsVisible = false;
        //sl1.MaxValueVisual.IsVisible = false;

        //sl1.AutoSize = false;
        //sl1.Height = 40;
        //sl1.MinValueVisual.Margin = D2D_RECT_F.Thickness(10, 0);
        //sl1.MaxValueVisual.Margin = D2D_RECT_F.Thickness(10, 0);
        //sl1.Thumb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGreen.ToColor());
        //sl1.MaxValueVisual.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightBlue.ToColor());
        //sl1.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());
        //sl1.MinValueVisual.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Orange.ToColor());
        //sl1.TicksVisual?.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        Children.Add(sl1);
        //sl1.VerticalAlignment = Alignment.Stretch;
    }

    private sealed class EllipseSlider : Slider<int>
    {
        protected override Visual CreateThumb()
        {
            return new EllipseThumb();
        }
    }

    public void AddPropertyGrid()
    {
        var pg = new PropertyGrid<Model> { Margin = D2D_RECT_F.Thickness(10, 10, 10, 10), LiveSync = true };
        Children.Add(pg);
        pg.SelectedObject = new Model();
    }

    public void AddLogVisual()
    {
        var sv = new ScrollViewer { Margin = D2D_RECT_F.Thickness(10, 10, 10, 10) };
        var log = new LogVisual
        {
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10),
            VerticalAlignment = Alignment.Near,
            BackgroundColor = D3DCOLORVALUE.Transparent,
            ForegroundColor = D3DCOLORVALUE.Black
        };
        sv.Viewer.Child = log;
        Children.Add(sv);

        _ = RunTaskOnMainThread(() => Title = "Press PAUSE to pause/resume logging", true);
        var stoppedEvent = new AutoResetEvent(false);
        KeyDown += (s, e) =>
        {
            if (e.Key == VIRTUAL_KEY.VK_PAUSE)
            {
                // toggle pause/resume
                stoppedEvent.Set();
            }
        };

        Task.Run(() =>
        {
            var lines = File.ReadAllLines(@"Resources\assommoir.txt");
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < lines.Length; i++)
            {
                var line = $"{sw.Elapsed.TotalSeconds,12} - #{i,10} {lines[i]}";
                log.Append(line);

                // not necessarily needed in general, but here we want to see the last "log" line,
                // so we ask ScrollViewer to scroll to the end, no need to wait
                _ = RunTaskOnMainThread(() => sv.VerticalOffset = sv.VerticalMaxOffset);

                // simulate some fast work on this thread
                // note 1 ms won't probably be honored as it depends
                // on Windows system timer resolution.
                // usually, it's more around 16ms
                Thread.Sleep(1);
                // or for example Thread.Sleep(rnd.Next(20, 200));

                // quick check for stop request
                if (stoppedEvent.WaitOne(0))
                {
                    // wait for resume request
                    stoppedEvent.WaitOne();
                }
            }
            _ = RunTaskOnMainThread(() => sv.VerticalOffset = sv.VerticalMaxOffset);
            stoppedEvent.Dispose();
        });
    }

    public void AddFastLogVisual()
    {
        var sv = new ScrollViewer { Margin = D2D_RECT_F.Thickness(10, 10, 10, 10) };
        var log = new FastLogVisual
        {
            Margin = D2D_RECT_F.Thickness(10, 10, 10, 10),
            VerticalAlignment = Alignment.Near,
            BackgroundColor = D3DCOLORVALUE.Transparent,
            ForegroundColor = D3DCOLORVALUE.Black
        };
        sv.Viewer.Child = log;
        Children.Add(sv);

        _ = RunTaskOnMainThread(() => Title = "Press PAUSE to pause/resume logging", true);
        var stoppedEvent = new AutoResetEvent(false);
        KeyDown += (s, e) =>
        {
            if (e.Key == VIRTUAL_KEY.VK_PAUSE)
            {
                // toggle pause/resume
                stoppedEvent.Set();
            }
            else if (e.Key == VIRTUAL_KEY.VK_F11)
            {
                GC.Collect();
                Application.Trace("GC Collected");
            }
        };

        Task.Run(() =>
        {
            // requesting higher timer resolution for this thread
            // note going too low will impact Desktop Window Manager too much (GPU usage, etc.) depending on harware
            _ = DirectN.Functions.timeBeginPeriod(1);
            try
            {
                var lines = File.ReadAllLines(@"Resources\assommoir.txt");
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = $"{sw.Elapsed.TotalSeconds,12} - #{i,10} {lines[i]}";
                    log.Append(line);

                    _ = RunTaskOnMainThread(() => sv.VerticalOffset = sv.VerticalMaxOffset);

                    Thread.Sleep(5); // will give around 5ms depending on timeBeginPeriod call

                    // quick check for stop request
                    if (stoppedEvent.WaitOne(0))
                    {
                        // wait for resume request
                        stoppedEvent.WaitOne();
                    }
                }
                _ = RunTaskOnMainThread(() => sv.VerticalOffset = sv.VerticalMaxOffset);
            }
            finally
            {
                stoppedEvent.Dispose();
                _ = DirectN.Functions.timeEndPeriod(1);
            }
        });
    }

    //protected override void OnPositionChanging(object? sender, DirectN.Extensions.Utilities.ValueEventArgs<WINDOWPOS> e)
    //{
    //    base.OnPositionChanging(sender, e);
    //    var flags = e.Value.flags & ~(SET_WINDOW_POS_FLAGS)0xFFFF8000; // undoc'ed
    //    if (flags.HasFlag(SET_WINDOW_POS_FLAGS.SWP_NOSIZE) && flags.HasFlag(SET_WINDOW_POS_FLAGS.SWP_NOMOVE))
    //        return;

    //    var pos = e.Value;

    //    // uncomment to set position to 0,0 always
    //    //e.Cancel = true;
    //    //pos.x = 0;
    //    e.Value = pos;

    //    Application.Trace("Wice AOT Position: " + pos.x + ", " + pos.y + " - Size: " + pos.cx + ", " + pos.cy + " flags:" + flags);

    //    var title = "Wice AOT";
    //    if (!flags.HasFlag(SET_WINDOW_POS_FLAGS.SWP_NOMOVE))
    //    {
    //        title += " - Position: " + pos.x + ", " + pos.y;
    //    }

    //    if (pos.cx >= 0 || pos.cy >= 0)
    //    {
    //        title += " - Size: " + pos.cx + ", " + pos.cy;
    //    }
    //    Title = title;
    //}

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

        async Task<TabPage> addPage(ICoreWebView2NewWindowRequestedEventArgs? e)
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
                e.get_Uri(out var uri).ThrowOnError();
                page.Header.Text.Text = uri.ToString() ?? string.Empty;
                Marshal.FreeCoTaskMem(uri.Value);
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

    public void ChoosePdfView()
    {
        var stack = new Stack();
        Children.Add(stack);
        var btn = new Button();
        btn.Text.Text = "Load PDF File...";
        stack.Children.Add(btn);
        var pdf = new PdfView
        {
        };
        stack.Children.Add(pdf);

        btn.Click += async (s, e) =>
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");

            InitializeWithWindow.Initialize(picker, Window!.Handle);
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                pdf.SourceStream = File.OpenRead(file.Path);
            }
        };
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
        dlg.TitleBar!.IsVisible = false;
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

    public void AddScrollableRtbRtfFile()
    {
        var sv = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Children.Add(sv);

        var rtf = new RichTextBox
        {
            VerticalAlignment = Alignment.Near,
            HorizontalAlignment = Alignment.Near,
            //Width = 500
        };
        rtf.Options |= DirectN.Extensions.Utilities.TextHostOptions.WordWrap;
        rtf.RtfText = File.ReadAllText(@"resources\wice.rtf");
        sv.Viewer.Child = rtf;
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

public class Model
{
    public string Name { get; set; } = Environment.UserName;
    public int Age { get; set; } = 42;
    public DateTime BirthDate { get; set; } = new DateTime(1980, 1, 1);
    public bool IsMale { get; set; } = true;
    public ConsoleColor FavoriteColor { get; set; } = ConsoleColor.Blue;
    public float Height { get; set; } = 1.75f;
    public double Weight { get; set; } = 70.5;
    public SampleDaysOfWeek WorkDays { get; set; }
}