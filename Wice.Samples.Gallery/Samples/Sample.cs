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

        public virtual string TypeName
        {
            get
            {
                const string postfix = nameof(Sample);
                var typeName = GetType().Name;
                if (typeName.Length > postfix.Length && typeName.EndsWith(postfix))
                    return typeName.Substring(0, typeName.Length - postfix.Length);

                return typeName;
            }
        }

        public virtual string Title => TypeName;
        public virtual string Description => "The " + Title + " sample";

        public abstract string IconText { get; }
    }
}
