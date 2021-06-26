using DirectN;

namespace Wice
{
    public interface IScrollView : IPropertyOwner
    {
        D2D_SIZE_F ExtentSize { get; }
        D2D_SIZE_F ViewSize { get; }
        D2D_RECT_F ChildRenderRect { get; }

        float VerticalLineSize { get; set; }
        float HorizontalLineSize { get; set; }
        float VerticalOffset { get; set; }
        float HorizontalOffset { get; set; }
    }
}
