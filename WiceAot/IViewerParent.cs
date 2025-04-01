namespace Wice;

public interface IViewerParent
{
    Viewer Viewer { get; }
    bool IsVerticalScrollBarVisible { get; }
    bool IsHorizontalScrollBarVisible { get; }

    float HorizontalOffset { get; set; }
    float VerticalOffset { get; set; }
}
