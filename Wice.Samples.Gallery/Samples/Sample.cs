using Windows.UI.Composition;

namespace Wice.Samples.Gallery.Samples
{
    public abstract class Sample
    {
        protected Sample()
        {
        }

        protected internal Compositor Compositor { get; internal set; }
        public abstract int SortOrder { get; }
        public abstract string Description { get; }
        public abstract void Layout(Visual parent);

        public override string ToString() => Description;
    }
}
