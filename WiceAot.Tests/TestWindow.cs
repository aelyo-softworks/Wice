using System.Numerics;

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

        //AddEditableTexts();
        //AddUniformGridShapes(20);
        //AddUniformColorGrid(20);
        //AddUniformGridImmersiveColors();
        //AddUniformGridSysColors();

        LongRunWithCursor();
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

    public void BigText()
    {
        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        var tb = new TextBox
        {
            FontFamilyName = "Consolas",
            Text = text,
            IsEditable = true,
        };
        Children.Add(tb);
    }

    public void BigTextSv()
    {
        var sv = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        //sv.Viewer.IsWidthUnconstrained = false;
        //sv.Viewer.IsHeightUnconstrained = false;

        var text = File.ReadAllText(@"Resources\AliceInWonderlandNumbered.txt");
        //text = File.ReadAllText(@"Resources\MobyDickNumbered.txt");
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
            Name = "txt",
        };

        sv.Viewer.Child = txt;
        sv.Margin = D2D_RECT_F.Thickness(10, 10, 10, 10);

        Children.Add(sv);
        //Children.Add(txt);

        var btn = new Button();
        btn.VerticalAlignment = Alignment.Near;
        btn.Name = "btn";
        btn.Text.Text = "click";
        //Children.Add(btn);
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
            //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
            //text.BackgroundColor = _D3DCOLORVALUE.Transparent;
            //text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGoldenrodYellow);
            text.Text = color.Item1.ToString();

            var html = new TextBox();
            grid.Children.Add(html);
            //var color = new _D3DCOLORVALUE(0, i / (float)visual.Rows, j / (float)visual.Columns);
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
            //stack.Orientation = Orientation.Vertical;
            //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LemonChiffon.ToColor())
        };
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
            text.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
            text.Name = "text#" + i;
            text.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Red);
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
}
