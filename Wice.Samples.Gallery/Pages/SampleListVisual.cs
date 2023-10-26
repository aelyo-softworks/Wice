using System;
using DirectN;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleListVisual : Titled
    {
        public SampleListVisual(SampleList sampleList)
        {
            if (sampleList == null)
                throw new ArgumentNullException(nameof(sampleList));

            Title.Text = sampleList.Title;

            // description
            var tb = new TextBox();
            tb.Margin = D2D_RECT_F.Thickness(0, 0, 10, 10);
            tb.FontSize = 18;
            tb.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            tb.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_LIGHT;
            tb.Text = sampleList.Description;
            SetDockType(tb, DockType.Top);
            Children.Add(tb);

            var sv = new ScrollViewer();
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Children.Add(sv);

            // a dock for all sample visuals
            var dock = new Dock();
            dock.Margin = D2D_RECT_F.Thickness(10, 0, 0, 0);
            dock.HorizontalAlignment = Alignment.Near;
            dock.VerticalAlignment = Alignment.Near;
            sv.Child = dock;

            foreach (var sample in sampleList.Samples)
            {
                var visual = new SampleVisual(sample);
                visual.Margin = D2D_RECT_F.Thickness(0, 0, 0, 10);
                SetDockType(visual, DockType.Top);
                dock.Children.Add(visual);
            }
        }
    }
}
