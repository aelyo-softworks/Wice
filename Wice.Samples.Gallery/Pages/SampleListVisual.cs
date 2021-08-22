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

            SampleList = sampleList;
            Title.Text = sampleList.Title;

            var tb = new TextBox();
            tb.FontSize = 18;
            tb.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_LIGHT;
            tb.Text = sampleList.Description;
            Children.Add(tb);

            foreach (var sample in SampleList.Samples)
            {
                var sampleTb = new TextBox();
                sampleTb.Text = sample.Description;
                Children.Add(sampleTb);
            }
        }

        public SampleList SampleList { get; }
    }
}
