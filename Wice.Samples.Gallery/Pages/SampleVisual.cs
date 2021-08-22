using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleVisual : Border
    {
        public SampleVisual(Sample sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            var sampleTb = new TextBox();
            sampleTb.FontSize = 20;
            sampleTb.Text = sample.Description;
            //SetDockType(sampleTb, DockType.Top);
            Children.Add(sampleTb);
            sample.Layout(this);
        }
    }
}
