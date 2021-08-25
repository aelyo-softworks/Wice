using System;
using System.Collections.Generic;
using System.Linq;

namespace Wice.Samples.Gallery.Samples
{
    public abstract class SampleList
    {
        protected SampleList()
        {
        }

        public abstract string IconText { get; }

        public virtual string Title => TypeName;
        public virtual string SubTitle => "The " + Title + " visual.";
        public virtual string Description => "The " + Title + " sample list";
        public virtual string TypeName
        {
            get
            {
                const string postfix = nameof(SampleList);
                var typeName = GetType().Name;
                if (typeName.Length > postfix.Length && typeName.EndsWith(postfix))
                    return typeName.Substring(0, typeName.Length - postfix.Length);

                return typeName;
            }
        }

        // load all samples in this assembly and folder, using reflection
        public IEnumerable<Sample> Samples => GetType().Assembly.GetTypes()
                .Where(t => typeof(Sample).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace == GetType().Namespace + "." + TypeName)
                .Select(t => (Sample)Activator.CreateInstance(t))
                .OrderBy(t => t.SortOrder);

        public override string ToString() => Title;
    }
}
