using System;
using DirectN;
using Wice.Samples.Gallery.Samples;
using Wice.Samples.Gallery.Utilities;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleVisual : Dock
    {
        private readonly Sample _sample;
        private CodeBox _codeBox;

        public SampleVisual(Sample sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            _sample = sample;
            var desc = sample.Description.Nullify();
            if (desc != null)
            {
                var sampleTb = new TextBox();
                sampleTb.FontSize = 20;
                sampleTb.Margin = D2D_RECT_F.Thickness(0, 0, 0, 10);
                sampleTb.Text = sample.Description;
                SetDockType(sampleTb, DockType.Top);
                Children.Add(sampleTb);
            }

            var sampleBorder = new Border();
            sampleBorder.BorderThickness = 1;
            sampleBorder.Padding = 10;
            sampleBorder.BorderBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGray);
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
                    _codeBox = new CodeBox();
                    _codeBox.Margin = D2D_RECT_F.Thickness(0, 10, 0, 0);
                    _codeBox.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());
                    _codeBox.Options |= TextHostOptions.WordWrap;
                    _codeBox.Padding = 5;
                    _codeBox.CodeLanguage = WiceLanguage.Default.Id; // init & put in repo
                    SetDockType(_codeBox, DockType.Top);
                    _codeBox.CodeText = text;
                    dock.Children.Add(_codeBox);
                }
            });
        }

        protected override void OnDetachedFromComposition(object sender, EventArgs e)
        {
            base.OnDetachedFromComposition(sender, e);
            _codeBox?.Dispose();
            if (_sample is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
