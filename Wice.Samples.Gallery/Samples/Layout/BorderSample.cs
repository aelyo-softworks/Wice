using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class BorderSample : Sample
    {
        public BorderSample()
        {
        }

        public override string IconText => MDL2GlyphResource.PageMarginPortraitNormal;
        public override string Description => "Use a Border control to draw a boundary line, background, or both, around another object. A Border can contain only one child object.";
    }
}
