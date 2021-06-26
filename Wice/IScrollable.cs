namespace Wice
{
    public interface IScrollable
    {
        bool SupportsVerticalScrolling { get; }
        bool SupportsHorizontalScrolling { get; }
    }
}
