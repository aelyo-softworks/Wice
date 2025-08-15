namespace Wice;

/// <summary>
/// Defines options that control how a drawing surface is created and rendered.
/// </summary>
/// <remarks>
/// Intended to be passed when creating rendering surfaces within Wice components.
/// </remarks>
public class SurfaceCreationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether drawing should be aligned to device pixel boundaries.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to snap rendered content to whole pixels to reduce blurriness;
    /// otherwise, <see langword="false"/> to allow sub-pixel rendering. Default is <see langword="true"/>.
    /// </value>
    public virtual bool SnapToPixels { get; set; } = true;
}
