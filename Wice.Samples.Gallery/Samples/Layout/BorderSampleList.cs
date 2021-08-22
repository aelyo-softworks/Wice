using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class BorderSampleList : SampleList
    {
        public BorderSampleList()
        {
        }

        public override string IconText => MDL2GlyphResource.PageMarginPortraitNormal;
        public override string Header => "Use a Border control to draw a boundary line, background, or both, around another object. A Border can contain only one child object.";
        public override string Description => "A container control that draws a boundary line, background or both around another object.";
    }
}
