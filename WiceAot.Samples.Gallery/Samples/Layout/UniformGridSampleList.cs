﻿using Wice.Samples.Gallery.Samples.Layout.UniformGrid;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class UniformGridSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.GridView;
        public override string SubTitle => "The UniformGrid visual is used to position content in rows and columns where all the cells in the grid have the same size.";

        protected override IEnumerable<Sample> Types
        {
            get
            {
                yield return new ShapesUniformGridSample();
                yield return new SimpleUniformGridSample();
            }
        }
    }
}
