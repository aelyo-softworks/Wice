using System;
using System.Collections.Generic;
using System.Linq;

namespace Wice.Samples.Gallery.Samples
{
    public abstract class SampleList
    {
        protected SampleList()
        {
            // load all samples in this assembly and folder, using reflection
            Samples = GetType().Assembly.GetTypes()
                .Where(t => typeof(Sample).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace == GetType().Namespace + "." + TypeName)
                .Select(t => (Sample)Activator.CreateInstance(t))
                .OrderBy(t => t.SortOrder)
                .ToList()
                .AsReadOnly();
        }

        public abstract string IconText { get; }

        public virtual string Title => TypeName;
        public virtual string Header => "The " + Title + " sample list";
        public virtual string Description => "The " + Title + " visual.";
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

        public IReadOnlyList<Sample> Samples { get; }
    }
}
