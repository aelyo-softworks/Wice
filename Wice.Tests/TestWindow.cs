using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DirectN;
using Wice.Effects;
using Wice.PropertyGrid;
using Wice.Utilities;
using Windows.UI.Composition;
using Windows.UI.Composition.Diagnostics;

namespace Wice.Tests
{
    public class TestWindow : Window
    {
        public TestWindow()
        {
            //WindowsFrameMode = WindowsFrameMode.Merged;
            Style |= WS.WS_THICKFRAME | WS.WS_CAPTION | WS.WS_SYSMENU | WS.WS_MAXIMIZEBOX | WS.WS_MINIMIZEBOX;
            //SizeToContent = DimensionOptions.WidthAndHeight;
            Native.EnableBlurBehind();
            RenderBrush = AcrylicBrush.CreateAcrylicBrush(
                CompositionDevice,
                _D3DCOLORVALUE.White,
                0.2f,
                useWindowsAcrylic: true
                );

            //SizeToContent = DimensionOptions.WidthAndHeight;
            //AddCounter(1);
            //AddDrawTextCounter(10);

            //AddScrollableReadOnlyText();
            //AddDrawText(this);
            //AddReadOnlyText();
            //AddReadOnlyTexts();
            //AddEditableTexts();
            //AddTextWithSpaces(this);

            //AddSimpleGrid();
            //AddSimpleGrid2();
            AddSimplePropertyGrid();
            //AddSimplePropertyGrid2();
            //AddSimplePropertyGrid3();

            //AddUniformGridShapes(10);
            //AddUniformColorGrid(10);
            //AddUniformGridImmersiveColors();
            //AddUniformGridSysColors();

            //AddScrollViewImage();
            //AddScrollViewSmall();
            //AddFillImage();

            //AddDock();
            //AddOneDockChild();
            //AddDockNumbers();
            //AddDockAbout(); 
            //AddHorizontalButtonsDock();
            //AddVerticalButtonsDock();

            //AddStacks();
            //AddFixedStack();
            //AddStackImageText();
            //AddSizeToContentStack();

            //AddStateBoxes();
            //AddNullableCheckBox();
            //AddCheckBox();
            //AddRadioButtons();
            //AddToggleSwitch();

            //AddButtonInCanvas();
            //AddButtonInDock();
            //AddButton();

            //AddComboBox();
            //AddListBox();
            //AddResizableListBox();
            //AddScollableListBox();
            //AddCheckBoxList();
            //AddFlagsEnumListBox();
            //AddEnumListBox();

            //AddTitleBar();
            //AddControls();
            //AddVisibleSwitch();
            //AddToolTip();
            //AddPopup();
            //AddShadow();
            //AddDialog();

            //KeyDown += (s, e) =>
            //{
            //    //if (e.Key == VirtualKeys.P)
            //    //{
            //    //    AddPopup();
            //    //}

            //    if (e.Key == VirtualKeys.S)
            //    {
            //        RunTaskOnMainThread(() =>
            //        {
            //            //Resize(500, 500);
            //            Height = 500;

            //            //Center();
            //        }, true);
            //    }
            //};
        }

        public void AddDialog()
        {
            Dialog dlg = null;
            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.Escape)
                {
                    if (dlg != null)
                    {
                        dlg.Close();
                        dlg = null;
                    }
                    return;
                }

                if (e.Key != VirtualKeys.D)
                    return;

                if (dlg == null)
                {
                    dlg = new Dialog();
                    dlg.Name = "dlg";
                    resize();
                    Resized += (s2, e2) => resize();

                    dlg.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightPink);
                    Children.Add(dlg);
                }

                void resize()
                {
                    var cr = ClientRect;
                    dlg.Width = cr.Width / 2;
                    dlg.Height = cr.Height / 2;
                }
            };
        }

        public void AddShadow()
        {
            var b = new Canvas();
            b.Margin = 20;
            //b.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.YellowGreen);
            Children.Add(b);

            var shadow = Compositor.CreateDropShadow();
            shadow.BlurRadius = 20;
            b.RenderShadow = shadow;

            var rr = new RoundedRectangle();
            //rr.Margin = 30;
            rr.CornerRadius = new Vector2(10, 10);
            rr.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightPink);
            b.Children.Add(rr);

            rr.ToolTipContentCreator = (tt) => CreateDefaultToolTipContent(tt, "hello world!");
        }

        public void AddControls()
        {
            MinWidth = 400;
            MinHeight = 400;

            var tb = new TitleBar();
            tb.Title.IsVisible = false;
            Dock.SetDockType(tb, DockType.Top);
            tb.IsMain = true;
            Children.Add(tb);

            var menuBack = new Border();
            menuBack.Name = nameof(menuBack);
            menuBack.Width = 300;
            menuBack.RenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0xFFE6E6E6));
            menuBack.Opacity = 0.5f;
            SetLeft(menuBack, 0);
            Children.Add(menuBack);

            var gridPadding = 20;
            var grid = new Grid();
            grid.Name = nameof(grid);
            grid.Columns[0].Size = menuBack.Width - gridPadding * 2;
            grid.Rows[0].Size = tb.Height;
            //grid.Padding = gridPadding;
            grid.Columns.Add(new GridColumn());
            grid.Rows.Add(new GridRow());
            Children.Add(grid);

            var document = new Border();
            document.Name = nameof(document);
            Grid.SetColumn(document, 1);
            Grid.SetRow(document, 1);
            grid.Children.Add(document);

            var stack = new Stack();
            Grid.SetRow(stack, 1);
            grid.Children.Add(stack);

            var header = CreateHeader("Compos", MDL2GlyphResource.PrintCustomRange);
            stack.Children.Add(header);
            //header.Child = CreateMeasureUniformGrid(10);
            header.Child = new Border { Height = 100, RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue) };

            header = CreateHeader("Inputs", MDL2GlyphResource.VideoCapture);
            stack.Children.Add(header);
            header.Child = new Border { Height = 10, RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red) };

            KeyDown += (s, e) =>
            {
                if (e.Key != VirtualKeys.P)
                    return;

                header.Child.Height = 100;
                header.Child.DoWhenRendered(() => header.Viewer.Height = header.Child.RelativeRenderRect.Height, VisualDoOptions.DeferredOnly);
            };

            //var sr = Serialize();

            //var tw = new Window();
            //tw.DeserializeXml(sr);
        }

        public SymbolHeaderedContent CreateHeader(string text, string iconText)
        {
            var header = new SymbolHeaderedContent();
            header.Name = "shc" + text;
            header.Margin = D2D_RECT_F.Thickness(10, 0);
            header.Header.Height = 40;
            header.Header.Icon.Text = iconText;
            header.Header.Text.Margin = D2D_RECT_F.Thickness(10, 0, 10, 0);
            header.Header.Text.Text = text;
            header.Header.Text.FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
            header.Header.Text.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;
            header.ToolTipContentCreator = (tt) => CreateDefaultToolTipContent(tt, "hello " + header.Name);

            var typo = Typography.WithLigatures;
            header.Header.Text.SetTypography(typo.DWriteTypography.Object);

            header.Header.HoverRenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0x80C0C0C0));
            header.HorizontalAlignment = Alignment.Center;
            header.VerticalAlignment = Alignment.Center;
            header.Viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            return header;
        }

        public void AddPopup()
        {
            var popup = new PopupWindow();
            popup.ParentHandle = Native.Handle;
            popup.HorizontalOffset = 300;
            popup.VerticalOffset = 300;
            popup.IsFocusable = false;
            popup.DoWhenAttachedToComposition(() =>
            {
                popup.ResizeClient(100, 100);
            });
            popup.Show();

            var border = new Border();
            border.Height = 100;
            border.Width = 200;
            border.RenderBrush = popup.Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            popup.Children.Add(border);
        }

        public void AddToolTip()
        {
            var border = new Border();
            Children.Add(border);
            border.Height = 100;
            border.Width = 100;
            border.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            border.ToolTipContentCreator = (tt) => CreateDefaultToolTipContent(tt, "hello red world");

            var border2 = new Border();
            border.VerticalAlignment = Alignment.Near;
            Children.Add(border2);
            border2.Height = 100;
            border2.Width = 100;
            border2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            border2.ToolTipContentCreator = (tt) => CreateDefaultToolTipContent(tt, "hello blue world");

            PopupWindow popup = null;
            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.M)
                {
                    MessageBox.Show(this, "hello word");
                    return;
                }

                if (e.Key == VirtualKeys.Escape)
                {
                    if (popup != null)
                    {
                        popup.Destroy();
                        popup = null;
                    }
                    return;
                }

                if (e.Key == VirtualKeys.P)
                {
                    if (popup == null)
                    {
                        popup = new PopupWindow();
                        popup.Width = 100;
                        popup.Height = 100;
                        popup.Name = nameof(popup);
                        popup.PlacementTarget = this;
                        popup.FollowPlacementTarget = true;
                        //popup.ParentHandle = Native.Handle;
                        popup.HorizontalOffset = 300;
                        popup.VerticalOffset = 300;
                        popup.IsFocusable = false;
                        popup.Show();

                        var pborder = new Border();
                        pborder.Height = 100;
                        pborder.Width = 50;
                        pborder.DoWhenAttachedToComposition(() =>
                        {
                            pborder.RenderBrush = popup.Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
                        });
                        popup.Children.Add(pborder);
                    }
                    return;
                }
            };
        }

        public void AddTitleBar()
        {
            var tb = new TitleBar();
            tb.IsMain = true;
            Children.Add(tb);

            KeyDown += (s, e) =>
            {
                Window.Title = "Windows Interface Composition Engine";
            };
        }

        public void AddStateBoxes()
        {
            var cb = new StateButton();
            cb.AddState(new StateButtonState("toto", (b, e, s) => new TextBox { Text = s.ToString() }));
            cb.AddState(new StateButtonState("titi", (b, e, s) => new TextBox { Text = s.ToString() }));
            cb.AddState(new StateButtonState("tutu", (b, e, s) => new TextBox { Text = s.ToString() }));
            cb.Value = "tutu";
            cb.Width = 100;
            cb.Height = 100;
            cb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Gray);
            cb.Click += (s, e) =>
            {
                //MessageBox.Show("click!");
            };
            Children.Add(cb);
        }

        public void AddNullableCheckBox()
        {
            var cb = new NullableCheckBox();
            //cb.HoverRenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            //cb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Gray);
            cb.Click += (s, e) =>
            {
                //MessageBox.Show("click!");
            };
            Children.Add(cb);
        }

        public void AddCheckBox()
        {
            var cb = new CheckBox();
            cb.Click += (s, e) =>
            {
                //MessageBox.Show("click!");
            };
            Children.Add(cb);
        }

        public void AddRadioButtons()
        {
            var cb = new RadioButton();
            //cb.Width = 100;
            //cb.Height = 100;
            cb.Click += (s, e) =>
            {
                //MessageBox.Show("click!");
            };
            Children.Add(cb);
        }

        public void AddToggleSwitch()
        {
            var ts = new ToggleSwitch();
            //ts.Width = 200;
            //ts.Height = 100;
            Children.Add(ts);
        }

        public void AddButtonInCanvas()
        {
            var canvas = new Canvas();
            canvas.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            var btn = new Button();
            //btn.Width = 100;
            //btn.Height = 30;
            btn.Text.Text = "Hey! Click me!";
            btn.Icon.Text = MDL2GlyphResource.Clock;
            btn.Click += (s, e) =>
            {
                //btn.Text.Text = "he!";
                if (string.IsNullOrEmpty(btn.Text.Text))
                {
                    btn.Text.Text = "Cool" + Environment.TickCount;
                    btn.Text.FontSize = null;
                }
                else if (btn.Text.FontSize == 20)
                {
                    btn.Text.Text = "";
                }
                else
                {
                    btn.Text.FontSize = 20;
                }
            };

            Children.Add(canvas);
            canvas.Children.Add(btn);
        }

        public void AddButtonInDock()
        {
            var dock = new Dock();
            dock.LastChildFill = false;
            dock.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            var btn = new Button();
            //btn.Width = 100;
            //btn.Height = 30;
            btn.Text.Text = "Hey! Click me!";
            btn.Icon.Text = MDL2GlyphResource.Clock;
            btn.Click += (s, e) =>
            {
                //btn.Text.Text = "he!";
                if (string.IsNullOrEmpty(btn.Text.Text))
                {
                    btn.Text.Text = "Cool" + Environment.TickCount;
                    btn.Text.FontSize = null;
                }
                else if (btn.Text.FontSize == 20)
                {
                    btn.Text.Text = "";
                }
                else
                {
                    btn.Text.FontSize = 20;
                }
            };

            Children.Add(dock);
            dock.Children.Add(btn);
        }

        public void AddButton()
        {
            var btn = new Button();
            //btn.Width = 100;
            //btn.Height = 30;
            btn.Text.Text = "Hey! Click me please!";
            //btn.Icon.HoverRenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            btn.Icon.Text = MDL2GlyphResource.Clock;
            btn.RenderRotationAngle = 10;

            btn.Click += (s, e) =>
            {
                //btn.Text.Text = "he!";
                if (string.IsNullOrEmpty(btn.Text.Text))
                {
                    btn.Text.Text = "Cool" + Environment.TickCount;
                    btn.Text.FontSize = null;
                }
                else if (btn.Text.FontSize == 20)
                {
                    btn.Text.Text = "";
                }
                else
                {
                    btn.Text.FontSize = 20;
                }
            };
            Children.Add(btn);
        }

        public void AddComboBox()
        {
            var cb = new ComboBox();
            //cb.IntegralHeight = true;
            cb.MaxDropDownHeight = 125;

            //cb.Text.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            ((TextBox)cb.Text).Text = "hello";

            Children.Add(cb);
            //lb.DataSource = new List<string> { "hello", "world" };
            var i = 0;
            var words = File.ReadAllText("lorem.txt").
                ToLowerInvariant().
                Replace(Environment.NewLine, string.Empty).
                Replace(".", string.Empty).
                Replace(",", string.Empty)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s).ToHashSet().Take(24).Select(s => i++ + " : " + s);
            cb.DataSource = words;
        }

        public void AddScollableListBox()
        {
            var sv = new ScrollViewer();
            sv.HorizontalAlignment = Alignment.Center;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            Children.Add(sv);

            var lb = new ListBox();
            sv.Child = lb;
            lb.SelectionMode = SelectionMode.Multiple;
            //lb.Margin = D2D_RECT_F.Thickness(10);
            //lb.VerticalAlignment = Alignment.Stretch;
            //lb.HorizontalAlignment = Alignment.Stretch;

            //lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            var i = 0;
            var words = File.ReadAllText("lorem.txt").
                ToLowerInvariant().
                Replace(Environment.NewLine, string.Empty).
                Replace(".", string.Empty).
                Replace(",", string.Empty)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s).ToHashSet().Take(40).Select(s => i++ + " : " + s);
            lb.DataSource = words;

            lb.ScrollIntoView("27 : dolor");
            lb.Children[27].Focus();
            ((ItemVisual)lb.Children[27]).IsSelected = true;
            ((ItemVisual)lb.Children[15]).IsSelected = true;
        }

        public enum MyEnum
        {
            [Description("Def Value")]
            Value0 = 0,
            Value1 = 1,
            Value1_ = 1,

            [Browsable(false)]
            InvisibleValue,

            Value2
        }

        public void AddEnumListBox()
        {
            var lb = new EnumListBox();
            lb.Value = UriHostNameType.Basic;
            //var lb = new ListBox();
            //lb.DataSource = new object[] { "titi", 123, DateTime.Now, "hello world" };
            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            Children.Add(lb);

            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.L)
                {
                    foreach (var item in lb.Items)
                    {
                        Application.Trace(item + " selected " + item.IsSelected);
                    }
                }
            };
        }

        [Flags]
        public enum MyFlagsEnum
        {
            Value0 = 0x0,
            Value1 = 0x1,
            Value2 = 0x2,
            [Description("My Cool Value")]
            Value4_x = 0x4,
            Value8 = 0x8
        }

        public void AddFlagsEnumListBox()
        {
            var lb = new FlagsEnumListBox();
            //lb.Value = MyFlagsEnum.Value0 | MyFlagsEnum.Value2 | MyFlagsEnum.Value4_x;
            //lb.Value = MyFlagsEnum.Value0;
            lb.Value = SampleDaysOfWeek.WeekDays;
            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            Children.Add(lb);

            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.L)
                {
                    foreach (var item in lb.Items)
                    {
                        Application.Trace(item + " selected " + item.IsSelected);
                    }
                }
            };
        }

        public void AddCheckBoxList()
        {
            var lb = new CheckBoxList();
            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            Children.Add(lb);
            //lb.DataSource = new List<string> { "hello", "world" };
            var i = 0;
            var words = File.ReadAllText("lorem.txt").
                ToLowerInvariant().
                Replace(Environment.NewLine, string.Empty).
                Replace(".", string.Empty).
                Replace(",", string.Empty)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s).ToHashSet().Take(4).Select(s => i++ + " : " + s);
            lb.DataSource = words;
        }

        public void AddResizableListBox()
        {
            var b = new Border();
            b.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            b.HorizontalAlignment = Alignment.Center;
            b.VerticalAlignment = Alignment.Center;
            //b.Height = 200;
            //b.Width = 100;
            Children.Add(b);

            var i = 0;
            var words = File.ReadAllText("lorem.txt").
                ToLowerInvariant().
                Replace(Environment.NewLine, string.Empty).
                Replace(".", string.Empty).
                Replace(",", string.Empty)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s).ToHashSet().Take(10).Select(s => i++ + " : " + s).ToList();

            var lb = new ListBox();
            lb.DataBinder = new DataBinder
            {
                DataItemVisualCreator = (ctx) =>
                {
                    var tb = new TextBox();
                    tb.Height = 20;
                    tb.TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER;
                    tb.Margin = D2D_RECT_F.Thickness(10, 5, 10, 5);
                    tb.IsFocusable = true;
                    ctx.DataVisual = tb;
                },
                DataItemVisualBinder = (ctx) =>
                {
                    ((TextBox)ctx.DataVisual).Text = (string)ctx.Data;
                }
            };

            //lb.VerticalAlignment = Alignment.Center;
            lb.MouseButtonDown += (s, e) =>
            {
                var item = lb.GetVisualForMouseEvent(e);
                if (item != null)
                {
                    MessageBox.Show(this, item.ToString());
                }
            };

            KeyDown += (s2, e2) =>
            {
                if (words.Count > 0)
                {
                    words.RemoveAt(0);
                    ((IDataSourceVisual)lb).BindDataSource();
                }
            };

            b.Children.Add(lb);
            lb.DataSource = words;
        }

        public void AddListBox()
        {
            var lb = new ListBox();
            //lb.IntegralHeight = true;
            //lb.MaxHeight = 200;
            lb.MouseButtonDown += (s, e) =>
            {
                var item = lb.GetVisualForMouseEvent(e);
                if (item != null)
                {
                    //MessageBox.Show(item.ToString());
                }
            };

            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            Children.Add(lb);
            //lb.DataSource = new List<string> { "hello", "world" };
            var i = 0;
            var words = File.ReadAllText("lorem.txt").
                ToLowerInvariant().
                Replace(Environment.NewLine, string.Empty).
                Replace(".", string.Empty).
                Replace(",", string.Empty)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s).ToHashSet().Take(4).Select(s => i++ + " : " + s);
            lb.DataSource = words;
        }

        public void AddSimplePropertyGrid()
        {
            var sv = new ScrollViewer();
            Children.Add(sv);
            var pg = new PropertyGrid.PropertyGrid();
            pg.CellMargin = 5;
            pg.LiveSync = true;
            TextBox.FontSizeProperty.SetValue(pg, 12f);
            //pg.Padding = D2D_RECT_F.Thickness(5);
            //TextBox.AntiAliasingModeProperty.SetValue(pg, D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_ALIASED);
            //TextBox.FontFamilyNameProperty.SetValue(pg, "ProggyCleanTTF");
            //TextBox.ForegroundBrushProperty.SetValue(pg, new SolidColorBrush(_D3DCOLORVALUE.White));
            //pg.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.AliceBlue);
            sv.Viewer.Child = pg;

            //var cus = new { x = 123, y = "héllo" };
            var cus = new SampleCustomer();
            pg.SelectedObject = cus;

            //KeyDown += (s, e) =>
            //    {
            //        if (e.Key == VirtualKeys.End)
            //        {
            //            MessageBox.Show(this, "p:" + cus.Password);
            //        }
            //    };

            //var timer = new Timer((state) =>
            //{
            //    RunTaskOnMainThread(() =>
            //    {
            //        cus.DateOfBirth = cus.DateOfBirth.AddSeconds(1);
            //    });
            //});
            //timer.Change(0, 1000);
        }

        public void AddSimplePropertyGrid2()
        {
            var pg = new PropertyGrid.PropertyGrid();
            pg.VerticalAlignment = Alignment.Center;
            TextBox.FontSizeProperty.SetValue(pg, 12f);
            TextBox.ForegroundBrushProperty.SetValue(pg, new SolidColorBrush(_D3DCOLORVALUE.Black));
            Children.Add(pg);

            var cus = new SampleCustomer();
            pg.SelectedObject = cus;

        }

        public void AddSimplePropertyGrid3()
        {
            //var grid = new Grid();
            //Children.Add(grid);

            var b = new Border();
            //b.VerticalAlignment = Alignment.Near;
            b.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightSalmon);
            b.ToolTipContentCreator = (tt) => CreateDefaultToolTipContent(tt, "hello world!");
            Children.Add(b);

            var dock = new Dock();
            //dock.VerticalAlignment = Alignment.Near;
            //dock.LastChildFill = false;
            b.Children.Add(dock);

            var border = new Border();
            Dock.SetDockType(border, DockType.Top);
            //border.VerticalAlignment = Alignment.Near;
            border.Height = 50;
            border.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            dock.Children.Add(border);

            var pg = new PropertyGrid.PropertyGrid();
            Dock.SetDockType(pg, DockType.Top);
            pg.VerticalAlignment = Alignment.Near;
            pg.HorizontalAlignment = Alignment.Near;
            TextBox.FontSizeProperty.SetValue(pg, 12f);
            //TextBox.ForegroundBrushProperty.SetValue(pg, new SolidColorBrush(_D3DCOLORVALUE.Black));
            dock.Children.Add(pg);

            var cus = new SampleCustomer();
            pg.SelectedObject = cus;

            RunTaskOnMainThread(() =>
            {
                Height = 1000;
                Center();
            }, true);
        }

        public void AddCounter(int tickDivider)
        {
            var canvas = new Canvas();
            canvas.Name = nameof(canvas);
            canvas.Width = 300;
            canvas.Height = 300;
            canvas.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            Children.Add(canvas);

            var tb = new TextBox();
            tb.Name = nameof(tb);
            tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            tb.HorizontalAlignment = Alignment.Center;
            tb.VerticalAlignment = Alignment.Center;
            tb.Width = 200;
            tb.Height = 20;
            tb.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
            canvas.Children.Add(tb);

            var rnd = new Random(Environment.TickCount);
            tb.Text = rnd.Next(int.MinValue, int.MaxValue).ToString();

            //long l = 0;
            var vbt = new VerticalBlankTicker();
            vbt.TickDivider = tickDivider;
            vbt.Tick += (se, ev) =>
            {
                RunTaskOnMainThread(() =>
                {
                    tb.Text = rnd.Next(int.MinValue, int.MaxValue).ToString();
                    //text.Text = (l += rnd.Next(1, 19)).ToString();
                    //Application.Trace("text:" + text.Text);
                });
                ev.Cancel = true;
            };

            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.Space)
                {
                    if (vbt.IsRunning)
                    {
                        vbt.Stop();
                    }
                    else
                    {
                        vbt.Start();
                    }
                }
            };
        }

        public void AddVisibleSwitch()
        {
            var stack = new Stack();
            Children.Add(stack);
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            stack.Width = 300;
            stack.Height = 300;

            var b0 = new Border();
            stack.Children.Add(b0);
            b0.Name = nameof(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b0.Width = 100;
            b0.Height = 100;

            var b1 = new Border();
            stack.Children.Add(b1);
            b1.Name = nameof(b1);
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            b1.Width = 100;
            b1.Height = 100;

            KeyDown += (s, e) =>
            {
                b0.IsVisible = !b0.IsVisible;
            };
        }

        public void AddDrawTextCounter(int tickDivider)
        {
            var text = new TextBox();
            //text.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            text.RenderMode = TextBoxRenderMode.DrawText;
            Children.Add(text);

            text.Text = Environment.TickCount.ToString();
            KeyDown += (s, e) =>
            {
                var vbt = new VerticalBlankTicker();
                vbt.TickDivider = tickDivider;
                vbt.Tick += (se, ev) =>
                {
                    RunTaskOnMainThread(() =>
                    {
                        text.Text = Environment.TickCount.ToString();
                    });
                };
                vbt.Start();
            };
        }

        public void AddEditableTexts()
        {
            var stack = new Dock();
            //stack.Orientation = Orientation.Vertical;
            //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LemonChiffon);
            //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);

            //stack.LastChildFill = false;
            //stack.HorizontalAlignment = Alignment.Center;
            //stack.VerticalAlignment = Alignment.Center;
            Children.Add(stack);

            for (var i = 0; i < 3; i++)
            {
                var text = new TextBox();
                Dock.SetDockType(text, DockType.Top);
                text.IsEditable = true;

                //text.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
                text.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.GreenYellow);
                text.Name = "text#" + i;
                stack.Children.Add(text);
                text.Padding = D2D_RECT_F.Thickness(10);
                text.Margin = D2D_RECT_F.Thickness(10);
                text.Text = "Change this text #" + i;
                //text.HorizontalAlignment = Alignment.Stretch;
                //text.VerticalAlignment = Alignment.Stretch;
                //text.Width = 100;
                //text.Height = 50;
                //text.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
                //text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED;
                //text.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            }
        }

        public void AddReadOnlyText() => AddReadOnlyText(this);
        public static void AddReadOnlyText(Visual parent)
        {
            var text = new TextBox();
            //text.Margin = 30;
            text.Name = "text";
            //text.BackgroundColor = _D3DCOLORVALUE.DarkBlue;
            //text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGoldenrodYellow);
            parent.Children.Add(text);
            //text.Text = "hello world";
            text.Text = File.ReadAllText("lorem.txt");
            text.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED;
            text.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;

            parent.KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.F)
                {
                    var fs = Environment.TickCount % 40;
                    if (fs > 7)
                    {
                        Application.Trace("fs:" + fs);
                        text.FontSize = fs;
                        e.Handled = true;
                    }
                }
            };
        }

        public void AddReadOnlyTexts()
        {
            var dock = new Stack();
            dock.LastChildFill = false;
            Children.Add(dock);

            var text1 = new TextBox();
            text1.Name = nameof(text1);
            dock.Children.Add(text1);
            text1.BackgroundColor = _D3DCOLORVALUE.LightCyan;
            text1.Text = File.ReadAllText("long1.txt");
            //text1.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            //text1.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED;
            text1.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            text1.VerticalAlignment = Alignment.Center;

            var text2 = new TextBox();
            text2.Name = nameof(text2);
            dock.Children.Add(text2);
            text2.BackgroundColor = _D3DCOLORVALUE.LightCoral;
            text2.Text = File.ReadAllText("long2.txt");
            //text2.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            //text2.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED;
            text2.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            //text2.VerticalAlignment = Alignment.Center;
        }

        public static void AddDrawText(Visual parent)
        {
            var text = new TextBox();
            text.RenderMode = TextBoxRenderMode.DrawText;
            text.Margin = 10;
            text.Name = "text";
            text.BackgroundColor = _D3DCOLORVALUE.Blue;
            text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGoldenrodYellow);
            parent.Children.Add(text);
            text.Text = "hello world";
            text.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED;
            text.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
        }

        public static void AddTextWithSpaces(Visual parent)
        {
            var dock = new Dock();
            parent.Children.Add(dock);
            for (var i = 0; i < 10; i++)
            {
                var text = new TextBox();
                text.Name = "text" + i;
                dock.Children.Add(text);

                if (i != 5)
                {
                    text.Text = i.ToString();
                }
                else
                {
                    // 5 space/room must be visible
                    text.Text = " ";
                }
            }
        }

        public void AddScrollableReadOnlyText()
        {
            var sv = new ScrollViewer();
            Children.Add(sv);
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddReadOnlyText(sv.Viewer);

            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.D)
                {
                    if (sv.ScrollMode == ScrollViewerMode.Dock)
                    {
                        sv.ScrollMode = ScrollViewerMode.Overlay;
                    }
                    else
                    {
                        sv.ScrollMode = ScrollViewerMode.Dock;
                    }
                }
            };
        }

        public void AddUniformGridSysColors()
        {
            var grid = new UniformGrid();
            grid.BackgroundColor = _D3DCOLORVALUE.Transparent;
            grid.Name = "ugrid";
            Children.Add(grid);

            var colors = ColorUtilities.SysColors.ToArray();
            grid.Rows = colors.Length;
            grid.Columns = 3;

            foreach (var color in colors)
            {
                var text = new TextBox();
                grid.Children.Add(text);
                //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                //text.BackgroundColor = _D3DCOLORVALUE.Transparent;
                //text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGoldenrodYellow);
                text.Text = color.Item1.ToString();

                var html = new TextBox();
                grid.Children.Add(html);
                //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                html.BackgroundColor = _D3DCOLORVALUE.Transparent;
                html.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.Gold);
                html.Text = color.Item2.ToString();

                var border = new Border();
                grid.Children.Add(border);
                border.RenderBrush = Compositor.CreateColorBrush(color.Item2);
            }
        }

        public void AddUniformGridImmersiveColors()
        {
            var grid = new UniformGrid();
            grid.BackgroundColor = _D3DCOLORVALUE.Transparent;
            grid.Name = "ugrid";
            Children.Add(grid);

            var max = 50;
            var colors = ColorUtilities.ImmersiveColors;
            grid.Rows = max;
            grid.Columns = 3;

            int i = 0;
            foreach (var color in colors)
            {
                var text = new TextBox();
                grid.Children.Add(text);
                //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                //text.BackgroundColor = _D3DCOLORVALUE.Transparent;
                //text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGoldenrodYellow);
                text.FontSize = 10;
                text.Text = color.Value.Name;

                var html = new TextBox();
                grid.Children.Add(html);
                //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                html.FontSize = text.FontSize;
                html.Text = color.Value.Color.HtmlString;

                var border = new Border();
                grid.Children.Add(border);
                border.RenderBrush = Compositor.CreateColorBrush(color.Value.Color);
                i++;
                if (i == max)
                    break;
            }
        }

        public void AddScrollViewSmall()
        {
            var scrollView = new ScrollViewer();
            scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollView.Height = 200;
            scrollView.Width = 200;
            //scrollView.Mode = ScrollViewerMode.Overlay;
            //scrollView.Viewer.ClipChildren = false;
            Children.Add(scrollView);

            //var border = new Border2();
            //border.Name = nameof(border);
            //border.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            //border.Height = 300;
            //border.Width = 300;
            //scrollView.Viewer.Child = border;

            var grid = CreateMeasureUniformGrid(10, 300, 300);
            scrollView.Viewer.Child = grid;
        }

        public static UniformGrid CreateMeasureUniformGrid(int size, int width, int height)
        {
            var grid = new UniformGrid();
            grid.Width = width;
            grid.Height = height;
            grid.Columns = size;
            grid.Rows = size;
            grid.LineBrush = new SolidColorBrush(_D3DCOLORVALUE.Red);
            grid.LineStrokeWidth = 1;
            //parent.Children.Add(grid);
            for (var i = 0; i < grid.Rows; i++)
            {
                for (var j = 0; j < grid.Columns; j++)
                {
                    var cell = new TextBox();
                    cell.IsFocusable = true;
                    cell.Text = i + "x" + j;
                    cell.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
                    cell.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
                    var color = new _D3DCOLORVALUE(0, i / (float)grid.Rows, j / (float)grid.Columns);
                    cell.BackgroundColor = color;
                    cell.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
                    grid.Children.Add(cell);
                }
            }
            return grid;
        }

        public void AddStackImageText()
        {
            var dlgPanel = new Stack();
            dlgPanel.Name = "panel";
            dlgPanel.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            dlgPanel.LastChildFill = false;
            dlgPanel.Orientation = Orientation.Horizontal;
            Children.Add(dlgPanel);

            var image = new Image();
            image.Name = "img";
            image.HorizontalAlignment = Alignment.Center;
            image.VerticalAlignment = Alignment.Center;
            image.Stretch = Stretch.None;
            image.Source = Application.Current.ResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), GetType().Namespace + ".Resources.aelyo_flat.png");
            dlgPanel.Children.Add(image);

            var tb = new TextBox();
            tb.Name = "tb";
            tb.Text = "hello world!";
            tb.FontSize = 20;
            tb.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.Black);
            tb.BackgroundColor = _D3DCOLORVALUE.Yellow;
            //tb.FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
            //tb.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;
            tb.SetTypography(Typography.WithLigatures.DWriteTypography.Object);
            dlgPanel.Children.Add(tb);
        }

        public void AddScrollViewImage()
        {
            var scrollView = new ScrollViewer();
            Children.Add(scrollView);
            scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            var image = new Image();
            image.Name = "image";
            image.Source = Application.Current.ResourceManager.GetWicBitmapSource(@"Resources\rainier.jpg");
            scrollView.Viewer.Child = image;

            KeyDown += (s, e) =>
            {
                //scrollView.VerticalScrollBar.CompositionBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Orange);
                //scrollView.HorizontalScrollBar.IsVisible = !scroller.HorizontalScrollBar.IsVisible;
                if (e.Key == VirtualKeys.Add)
                {
                    scrollView.HorizontalOffset++;
                }
                else if (e.Key == VirtualKeys.Subtract)
                {
                    scrollView.HorizontalOffset--;
                }
                else if (e.Key == VirtualKeys.M)
                {
                    scrollView.ScrollMode = scrollView.ScrollMode == ScrollViewerMode.Dock ? ScrollViewerMode.Overlay : ScrollViewerMode.Dock;
                }
            };
        }

        public void AddUniformGridShapes(int size)
        {
            var visual = new UniformGrid();
            visual.BackgroundColor = _D3DCOLORVALUE.Transparent;
            visual.Name = "ugrid";
            visual.Rows = size;
            visual.Columns = visual.Rows;
            Children.Add(visual);

            for (var i = 0; i < visual.Rows; i++)
            {
                for (var j = 0; j < visual.Columns; j++)
                {
                    addEllipse(i, j);
                }
            }

            KeyDown += (s, e) =>
            {
                var i = visual.Children.Count / visual.Rows;
                var j = visual.Children.Count % visual.Columns;
                if (e.Key == VirtualKeys.R)
                {
                    visual.Rows++;
                }
                else if (e.Key == VirtualKeys.C)
                {
                    visual.Columns++;
                }
                else if (e.Key == VirtualKeys.T)
                {
                    addRoundedRectangle(i, j);
                }
                else if (e.Key == VirtualKeys.L)
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
                var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                shape.RenderBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeThickness = 0.5f;
            }

            void addLine(int i, int j)
            {
                var shape = new Line();
                visual.Children.Add(shape);
                var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                shape.RenderBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeThickness = 0.5f;
                shape.Arranged += (s, e) =>
                {
                    shape.Geometry.End = shape.ArrangedRect.Size.ToVector2();
                };
            }

            void addRoundedRectangle(int i, int j)
            {
                var shape = new RoundedRectangle();
                visual.Children.Add(shape);
                var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                shape.Geometry.CornerRadius = new Vector2(10);
                shape.RenderBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeBrush = Compositor.CreateColorBrush(color);
                shape.Shape.StrokeThickness = 0.5f;
            }
        }

        public void AddSimpleGrid()
        {
            var grid = new Grid();
            Children.Add(grid);
            grid.Name = nameof(grid);
            grid.Margin = 10;

            grid.Columns.Add(new GridColumn());
            grid.Columns.Add(new GridColumn());
            grid.Rows.Add(new GridRow());
            grid.Rows.Add(new GridRow());

            var cell0 = new Border();
            cell0.Name = nameof(cell0);
            grid.Children.Add(cell0);
            cell0.Margin = 10;
            cell0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Purple);

            var cell1 = new Border();
            cell0.Name = nameof(cell1);
            grid.Children.Add(cell1);
            cell1.Margin = 10;
            Grid.SetColumn(cell1, 2);
            Grid.SetRow(cell1, 2);
            cell1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);

            var vsplitter = new GridSplitter();
            vsplitter.Name = nameof(vsplitter);
            vsplitter.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            grid.Children.Add(vsplitter);
            Grid.SetColumn(vsplitter, 1);

            var hsplitter = new GridSplitter();
            hsplitter.Name = nameof(hsplitter);
            hsplitter.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            grid.Children.Add(hsplitter);
            Grid.SetRow(hsplitter, 1);
        }

        public void AddSimpleGrid2()
        {
            var grid = new Grid();
            grid.VerticalAlignment = Alignment.Near;
            Children.Add(grid);
            grid.Name = nameof(grid);
            grid.Margin = 10;

            grid.Columns.Add(new GridColumn());
            grid.Columns.Add(new GridColumn());
            grid.Rows.Add(new GridRow());
            grid.Rows.Add(new GridRow());

            var cell0 = new Border();
            cell0.Name = nameof(cell0);
            cell0.Height = 20;
            grid.Children.Add(cell0);
            cell0.Margin = 10;
            cell0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Purple);

            var cell1 = new Border();
            cell1.Name = nameof(cell1);
            cell1.Height = 20;
            grid.Children.Add(cell1);
            cell1.Margin = 10;
            Grid.SetColumn(cell1, 2);
            Grid.SetRow(cell1, 2);
            cell1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);

            var vsplitter = new GridSplitter();
            vsplitter.Name = nameof(vsplitter);
            vsplitter.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            grid.Children.Add(vsplitter);
            Grid.SetColumn(vsplitter, 1);

            var hsplitter = new GridSplitter();
            hsplitter.Name = nameof(hsplitter);
            hsplitter.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            grid.Children.Add(hsplitter);
            Grid.SetRow(hsplitter, 1);
        }

        public void AddFillImage()
        {
            var border = new Border();
            Children.Add(border);
            //border.CompositionBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Orange);
            //border.BackgroundColor = _D3DCOLORVALUE.Orange;

            var img = new Image();
            border.Children.Add(img);
            //img.BackgroundColor = _D3DCOLORVALUE.Transparent;
            //img.Opacity = 0.4f;
            //img.HoverBackgroundColor = _D3DCOLORVALUE.Wheat;
            //img.Margin = 10;
            //img.InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC;
            img.Source = Application.Current.ResourceManager.GetWicBitmapSource(@"Resources\rainier.jpg");
        }

        public void AddUniformColorGrid(int size)
        {
            var visual = new UniformGrid();
            visual.Name = "ugrid";
            visual.Rows = size;
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
                if (e.Key == VirtualKeys.R)
                {
                    visual.Rows++;
                }
                else if (e.Key == VirtualKeys.C)
                {
                    visual.Columns++;
                }
                else
                {
                    var i = visual.Children.Count / visual.Rows;
                    var j = visual.Children.Count % visual.Columns;
                    addCell(i, j);
                }
            };

            void addCell(int i, int j)
            {
                var cell = new Border();
                cell.Name = "cell " + i + "x" + j;
                visual.Children.Add(cell);
                var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
                cell.RenderBrush = Compositor.CreateColorBrush(color);
            }
        }

        public void AddHorizontalButtonsDock()
        {
            var doc = new Dock();
            doc.Height = 50;
            doc.Name = nameof(doc);
            doc.LastChildFill = false;
            doc.RenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0xFFF0F0F0));
            Children.Add(doc);

            var okButton = new Button();
            okButton.Margin = 10;
            okButton.MinWidth = 70;
            okButton.Click += (s, e) => Destroy();
            okButton.VerticalAlignment = Alignment.Center;
            okButton.Name = nameof(okButton);
            okButton.Text.Text = "OK";
            Dock.SetDockType(okButton, DockType.Right);
            doc.Children.Add(okButton);

            var cancelButton = new Button();
            cancelButton.Margin = 3;
            cancelButton.MinWidth = 70;
            cancelButton.Click += (s, e) => Destroy();
            cancelButton.Name = nameof(cancelButton);
            cancelButton.Text.Text = "Cancel";
            cancelButton.Icon.Text = MDL2GlyphResource.Cancel;
            Dock.SetDockType(cancelButton, DockType.Right);
            doc.Children.Add(cancelButton);
        }

        public void AddVerticalButtonsDock()
        {
            var doc = new Dock();
            doc.Width = 150;
            doc.Name = nameof(doc);
            doc.LastChildFill = false;
            doc.RenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0xFFF0F0F0));
            Children.Add(doc);

            var okButton = new Button();
            okButton.Click += (s, e) => Destroy();
            okButton.Name = nameof(okButton);
            okButton.Text.Text = "OK";
            okButton.Icon.Text = MDL2GlyphResource.Save;
            Dock.SetDockType(okButton, DockType.Bottom);
            doc.Children.Add(okButton);

            var cancelButton = new Button();
            cancelButton.HorizontalAlignment = Alignment.Center;
            cancelButton.Click += (s, e) => Destroy();
            cancelButton.Name = nameof(cancelButton);
            cancelButton.Text.Text = "Cancel";
            Dock.SetDockType(cancelButton, DockType.Bottom);
            doc.Children.Add(cancelButton);
        }

        public void AddStacks()
        {
            var stack = new Stack();
            Children.Add(stack);
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            //stack.Width = 200;

            var b0 = new Border();
            stack.Children.Add(b0);
            b0.Name = nameof(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            //border.HorizontalAlignment = DuiAlignment.Far;
            b0.Width = 100;
            b0.Height = 100;
            //b0.Height = float.MaxValue;

            var b1 = new Stack();
            stack.Children.Add(b1);
            b1.Name = "b1";
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            //border.HorizontalAlignment = DuiAlignment.Far;
            b1.Width = 200;
            //b1.Height = 200;

            var b2 = new Border();
            b2.Name = "b2";
            b2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            b2.Width = 50;
            b2.Height = 60;
            b1.Children.Add(b2);

            var b3 = new Border();
            b3.Name = "b3";
            b3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            b3.Width = 70;
            //b3.Height = 20;
            b1.Children.Add(b3);

            KeyDown += (s, e) =>
            {
                if (e.Key == VirtualKeys.S)
                {
                    if (stack.Orientation == Orientation.Horizontal)
                    {
                        stack.Orientation = Orientation.Vertical;
                    }
                    else
                    {
                        stack.Orientation = Orientation.Horizontal;
                    }
                }
                else if (e.Key == VirtualKeys.V)
                {
                    b0.IsVisible = !b0.IsVisible;
                    b1.HorizontalAlignment = Alignment.Stretch;
                    //b1.Opacity *= 0.9f;
                }
            };
        }

        public void AddSizeToContentStack()
        {
            var stack = new Stack();
            Children.Add(stack);
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            stack.HorizontalAlignment = Alignment.Center;
            stack.VerticalAlignment = Alignment.Center;

            var b0 = new Border();
            stack.Children.Add(b0);
            b0.Name = nameof(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b0.Width = 100;
            b0.Height = 100;

            var b1 = new Border();
            stack.Children.Add(b1);
            b1.Name = nameof(b1);
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            b1.Width = 100;
            b1.Height = 100;
        }

        public void AddFixedStack()
        {
            var stack = new Stack();
            Children.Add(stack);
            stack.Name = nameof(stack);
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            stack.Width = 300;
            stack.Height = 300;

            var b0 = new Border();
            stack.Children.Add(b0);
            b0.Name = nameof(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b0.Width = 100;
            b0.Height = 100;

            var b1 = new Border();
            stack.Children.Add(b1);
            b1.Name = nameof(b1);
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            //b1.Width = 100;
            //b1.Height = 100;

            KeyDown += (s, e) =>
            {
                if (stack.Orientation == Orientation.Horizontal)
                {
                    stack.Orientation = Orientation.Vertical;
                }
                else
                {
                    stack.Orientation = Orientation.Horizontal;
                }
            };
        }

        public void AddOneDockChild()
        {
            var doc = new Dock();
            doc.LastChildFill = false;
            doc.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Orange);
            Children.Add(doc);

            var border = new Border();
            border.Width = 100;
            border.Height = 100;
            doc.Children.Add(border);
            Dock.SetDockType(border, DockType.Right);
            border.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
        }

        public void AddDockAbout()
        {
            var doc = new Dock();
            //doc.AllowOverlap = true;
            doc.LastChildFill = false;
            Children.Add(doc);

            for (var i = 0; i < 10; i++)
            {
                var border = new Border();
                Dock.SetDockType(border, DockType.Top);
                doc.Children.Add(border);

                var tb = new TextBox();
                tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
                tb.FontSize = 30;
                tb.Text = i.ToString();
                tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Black.ChangeAlpha(i / 10f));
                tb.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White.ChangeAlpha((10 - i) / 10f));
                border.Children.Add(tb);
            }

            var borderAbout = new Border();
            borderAbout.Name = nameof(borderAbout);
            Dock.SetDockType(borderAbout, DockType.Bottom);
            doc.Children.Add(borderAbout);

            var tbAbout = new TextBox();
            tbAbout.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
            tbAbout.FontSize = 30;
            tbAbout.Text = "About";
            borderAbout.Children.Add(tbAbout);
        }

        public void AddDockNumbers()
        {
            var doc = new Dock();
            doc.LastChildFill = false;
            doc.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Orange);
            Children.Add(doc);

            for (var i = 0; i < 10; i++)
            {
                var border = new Border();
                //border.Width = 10;
                //border.Height = 50;
                Dock.SetDockType(border, DockType.Right);
                doc.Children.Add(border);

                var tb = new TextBox();
                tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
                tb.FontSize = 30;
                tb.Text = i.ToString();
                tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Black.ChangeAlpha(i / 10f));
                tb.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White.ChangeAlpha((10 - i) / 10f));
                border.Children.Add(tb);
            }
        }

        public void AddDock()
        {
            var dock = new Dock();
            Children.Add(dock);
            dock.Name = "D#" + nameof(dock);
            dock.Margin = 50;
            dock.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Black);

            var b1 = new Border();
            dock.Children.Add(b1);
            Dock.SetDockType(b1, DockType.Top);
            b1.Name = "D#top";
            b1.MinHeight = 20;
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);

            var splitter1 = new DockSplitter();
            splitter1.Name = nameof(splitter1);
            dock.Children.Add(splitter1);
            splitter1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);

            var b2 = new Border();
            dock.Children.Add(b2);
            b2.Name = "D#bottom";
            b2.MinHeight = 20;
            Dock.SetDockType(b2, DockType.Bottom);
            b2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);

            var splitter2 = new DockSplitter();
            splitter2.Name = nameof(splitter2);
            dock.Children.Add(splitter2);
            splitter2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);

            var b3 = new Border();
            dock.Children.Add(b3);
            b3.Name = "D#left";
            b3.MinWidth = 20;
            Dock.SetDockType(b3, DockType.Left);
            b3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);

            var splitter3 = new DockSplitter();
            splitter3.Name = nameof(splitter3);
            dock.Children.Add(splitter3);
            splitter3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);

            var b4 = new Border();
            dock.Children.Add(b4);
            b4.Name = "D#right";
            b4.MinWidth = 20;
            Dock.SetDockType(b4, DockType.Right);
            b4.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Orange);

            var splitter4 = new DockSplitter();
            splitter4.Name = nameof(splitter4);
            dock.Children.Add(splitter4);
            splitter4.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);

            var b5 = new Border();
            dock.Children.Add(b5);
            b5.Name = "D#center";
            Dock.SetDockType(b5, DockType.Left);
            b5.MinWidth = 10;
            b5.MinHeight = 10;
            b5.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Violet);
        }
    }
}
