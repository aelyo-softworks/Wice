namespace Wice;

public enum PlacementMode
{
    Relative, // = InnerTopLeft
    Center,
    Mouse,
    Custom,

    // Outers are in general (unless offsets are set) useless for non Window visuals
    // since they can't draw outside the window
    OuterLeftTopCorner,
    OuterRightTopCorner,
    OuterLeftBottomCorner,
    OuterRightBottomCorner,

    OuterTopLeft,
    OuterTopCenter,
    OuterTopRight,

    OuterRightTop,
    OuterRightCenter,
    OuterRigthBottom,

    OuterBottomLeft,
    OuterBottomCenter,
    OuterBottomRight,

    OuterLeftTop,
    OuterLeftCenter,
    OuterLeftBottom,

    InnerTopCenter,
    InnerTopRight,

    InnerLeftCenter,
    InnerRightCenter,

    InnerBottomLeft,
    InnerBottomCenter,
    InnerBottomRight,
}
