using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleVisual : Dock
    {
        public SampleVisual(Sample sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            var sampleTb = new TextBox();
            sampleTb.FontSize = 20;
            sampleTb.Text = sample.Description;
            SetDockType(sampleTb, DockType.Top);
            Children.Add(sampleTb);

            var sampleBorder = new Border();
            sampleBorder.BorderThickness = 1;
            sampleBorder.Padding = 10;
            sampleBorder.BorderBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGray);
            SetDockType(sampleBorder, DockType.Top);
            Children.Add(sampleBorder);

            DoWhenAttachedToComposition(() =>
            {
                sample.Compositor = Compositor;
                sample.Layout(sampleBorder);
            });
        }
    }
}
