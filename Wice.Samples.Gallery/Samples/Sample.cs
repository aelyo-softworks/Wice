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

        public override string ToString() => Description;
    }
}
