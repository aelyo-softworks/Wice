﻿namespace Wice.Samples.Gallery.Pages;

public partial class SampleVisual : Dock
{
    public SampleVisual(Sample sample)
    {
        ExceptionExtensions.ThrowIfNull(sample, nameof(sample));

        var desc = sample.Description.Nullify();
        if (desc != null)
        {
            var sampleTb = new TextBox
            {
                FontSize = 20,
                Margin = D2D_RECT_F.Thickness(0, 0, 0, 10),
                Text = desc
            };
            SetDockType(sampleTb, DockType.Top);
            Children.Add(sampleTb);
        }

        var sampleBorder = new Border
        {
            BorderThickness = 1,
            Padding = 10,
            BorderBrush = new SolidColorBrush(D3DCOLORVALUE.LightGray)
        };
        SetDockType(sampleBorder, DockType.Top);
        Children.Add(sampleBorder);

        var dock = new Dock();
        sampleBorder.Children.Add(dock);

        DoWhenAttachedToComposition(() =>
        {
            sample.Compositor = Compositor;
            sample.Window = Window;
            sample.Layout(dock);

            var text = sample.GetSampleText();
            if (text != null)
            {
                var _codeBox = new CodeBox
                {
                    Margin = D2D_RECT_F.Thickness(0, 10, 0, 0),
                    RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor())
                };
                _codeBox.Options |= TextHostOptions.WordWrap;
                _codeBox.Padding = 5;
                _codeBox.CodeLanguage = WiceLanguage.Default.Id; // init & put in repo
                SetDockType(_codeBox, DockType.Top);
                _codeBox.CodeText = text;
                dock.Children.Add(_codeBox);
            }
        });
    }
}
