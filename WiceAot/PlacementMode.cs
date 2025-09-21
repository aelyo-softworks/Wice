namespace Wice;

/// <summary>
/// Describes how a visual (e.g., popup, tooltip, adornment) should be positioned
/// relative to a target visual's bounding rectangle or the mouse cursor.
/// </summary>
public enum PlacementMode
{
    /// <summary>
    /// Places the element's top-left corner at the target's top-left corner (equivalent to an inner top-left placement).
    /// </summary>
    Relative, // = InnerTopLeft

    /// <summary>
    /// Centers the element within the target.
    /// </summary>
    Center,

    /// <summary>
    /// Positions the element relative to the current mouse cursor location.
    /// </summary>
    Mouse,

    /// <summary>
    /// Placement is fully determined by consumer-provided logic (custom calculations/offsets).
    /// </summary>
    Custom,

    // Outers are in general (unless offsets are set) useless for non Window visuals
    // since they can't draw outside the window

    /// <summary>
    /// Outside the target at its top-left corner point (left of and above the target).
    /// </summary>
    OuterLeftTopCorner,

    /// <summary>
    /// Outside the target at its top-right corner point (right of and above the target).
    /// </summary>
    OuterRightTopCorner,

    /// <summary>
    /// Outside the target at its bottom-left corner point (left of and below the target).
    /// </summary>
    OuterLeftBottomCorner,

    /// <summary>
    /// Outside the target at its bottom-right corner point (right of and below the target).
    /// </summary>
    OuterRightBottomCorner,

    /// <summary>
    /// Outside along the top edge, left-aligned with the target.
    /// </summary>
    OuterTopLeft,

    /// <summary>
    /// Outside along the top edge, horizontally centered to the target.
    /// </summary>
    OuterTopCenter,

    /// <summary>
    /// Outside along the top edge, right-aligned with the target.
    /// </summary>
    OuterTopRight,

    /// <summary>
    /// Outside along the right edge, top-aligned with the target.
    /// </summary>
    OuterRightTop,

    /// <summary>
    /// Outside along the right edge, vertically centered to the target.
    /// </summary>
    OuterRightCenter,

    /// <summary>
    /// Outside along the right edge, bottom-aligned with the target.
    /// </summary>
    OuterRigthBottom,

    /// <summary>
    /// Outside along the bottom edge, left-aligned with the target.
    /// </summary>
    OuterBottomLeft,

    /// <summary>
    /// Outside along the bottom edge, horizontally centered to the target.
    /// </summary>
    OuterBottomCenter,

    /// <summary>
    /// Outside along the bottom edge, right-aligned with the target.
    /// </summary>
    OuterBottomRight,

    /// <summary>
    /// Outside along the left edge, top-aligned with the target.
    /// </summary>
    OuterLeftTop,

    /// <summary>
    /// Outside along the left edge, vertically centered to the target.
    /// </summary>
    OuterLeftCenter,

    /// <summary>
    /// Outside along the left edge, bottom-aligned with the target.
    /// </summary>
    OuterLeftBottom,

    /// <summary>
    /// Inside along the top edge, horizontally centered to the target.
    /// </summary>
    InnerTopCenter,

    /// <summary>
    /// Inside along the top edge, right-aligned with the target.
    /// </summary>
    InnerTopRight,

    /// <summary>
    /// Inside along the left edge, vertically centered to the target.
    /// </summary>
    InnerLeftCenter,

    /// <summary>
    /// Inside along the right edge, vertically centered to the target.
    /// </summary>
    InnerRightCenter,

    /// <summary>
    /// Inside along the bottom edge, left-aligned with the target.
    /// </summary>
    InnerBottomLeft,

    /// <summary>
    /// Inside along the bottom edge, horizontally centered to the target.
    /// </summary>
    InnerBottomCenter,

    /// <summary>
    /// Inside along the bottom edge, right-aligned with the target.
    /// </summary>
    InnerBottomRight,
}
