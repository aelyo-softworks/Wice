namespace WiceAot.Tests;

internal class TestWindow : Window
{
    public TestWindow()
    {
        WindowsFrameMode = WindowsFrameMode.Merged;
        Style |= WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_MINIMIZEBOX;
        //SizeToContent = DimensionOptions.WidthAndHeight;
        //Native.EnableBlurBehind();
        RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        //RenderBrush = AcrylicBrush.CreateAcrylicBrush(
        //    CompositionDevice,
        //    D3DCOLORVALUE.White,
        //    0.2f,
        //    useWindowsAcrylic: false
        //    );

        AddEditableTexts();
        DisplayTime();
    }

    private Timer _timer;
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

    public void AddEditableTexts()
    {
        var stack = new Dock();
        //stack.Orientation = Orientation.Vertical;
        //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
        //stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
        stack.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LemonChiffon.ToColor());
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
            text.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
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
