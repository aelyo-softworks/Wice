namespace Wice
{
    public interface IViewerParent
    {
        Viewer Viewer { get; }
        float HorizontalOffset { get; set; }
        float VerticalOffset { get; set; }
    }
}
