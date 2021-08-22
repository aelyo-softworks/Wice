using System;
using System.Collections.Generic;
using System.Linq;

namespace Wice.Samples.Gallery.Pages
{
    public abstract class Page : Titled
    {
        protected Page()
        {
            Title.Text = HeaderText;
        }

        // determine a default name using the class name
        public virtual string TypeName
        {
            get
            {
                const string postfix = nameof(Page);
                var typeName = GetType().Name;
                if (typeName.Length > postfix.Length && typeName.EndsWith(postfix))
                    return typeName.Substring(0, typeName.Length - postfix.Length);

                return typeName;
            }
        }

        public virtual string HeaderText => TypeName;
        public virtual string ToolTipText => "The " + HeaderText + " page";
        public virtual DockType DockType => DockType.Top;

        public abstract int SortOrder { get; }
        public abstract string IconText { get; }

        // load all pages in this assembly, using reflection
        public static IEnumerable<Page> GetPages() =>
            typeof(Page).Assembly.GetTypes()
                .Where(t => typeof(Page).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (Page)Activator.CreateInstance(t))
                .OrderBy(t => t.SortOrder);
    }
}
