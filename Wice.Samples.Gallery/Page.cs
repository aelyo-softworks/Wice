namespace Wice.Samples.Gallery
{
    public abstract class Page : Titled
    {
        public Page()
        {
        }

        public abstract string HeaderText { get; }
        public abstract string IconText { get; }
        public abstract string ToolTipText { get; }
    }
}
