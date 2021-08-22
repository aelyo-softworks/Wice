using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wice.Samples.Gallery.Samples
{
    public abstract class Sample
    {
        protected Sample()
        {
        }

        public abstract int SortOrder { get; }
        public abstract string Description { get; }
        public abstract void Layout(Visual parent);
    }
}
