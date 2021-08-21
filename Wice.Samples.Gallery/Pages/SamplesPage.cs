using System;
using System.Collections.Generic;
using System.Linq;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    public abstract class SamplesPage : Page
    {
        protected SamplesPage()
        {
            // load all samples in this assembly, using reflection
            Samples = GetType().Assembly.GetTypes()
                .Where(t => typeof(Sample).IsAssignableFrom(t) && t.Namespace == typeof(Program).Namespace + ".Samples." + TypeName)
                .Select(t => (Sample)Activator.CreateInstance(t))
                .OrderBy(t => t.TypeName)
                .ToList()
                .AsReadOnly();

            // add a wrap that holds all samples
            var wrap = new Wrap();
            wrap.Orientation = Orientation.Horizontal;
            foreach (var sample in Samples)
            {
                // use a custom button for a sample
                var btn = new SampleButton(sample);
                btn.Click += (s, e) =>
                {
                    MessageBox.Show(Window, sample.Title);
                };
                wrap.Children.Add(btn);
            }

            Children.Add(wrap);
        }

        public IReadOnlyList<Sample> Samples { get; }
    }
}
