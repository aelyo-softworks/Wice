using System;
using System.Linq;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    public abstract class SampleListPage : Page
    {
        protected SampleListPage()
        {
            // load all sample lists in this assembly and folder, using reflection
            var sampleLists = GetType().Assembly.GetTypes()
                .Where(t => typeof(SampleList).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace == typeof(Program).Namespace + ".Samples." + TypeName)
                .Select(t => (SampleList)Activator.CreateInstance(t))
                .OrderBy(t => t.TypeName);

            // add a wrap that holds all sample lists
            var wrap = new Wrap();
            wrap.Orientation = Orientation.Horizontal;
            foreach (var list in sampleLists)
            {
                // use a custom button for a sample
                var btn = new SampleButton(list);
                btn.Click += (s, e) =>
                {
                    var page = new SampleListVisual(list);
                    ((GalleryWindow)Window).ShowPage(page);
                };
                wrap.Children.Add(btn);
            }

            Children.Add(wrap);
        }
    }
}
