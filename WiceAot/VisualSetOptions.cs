namespace Wice;

/// <summary>
/// Provides visual-specific set options that control invalidation behavior after a property is set.
/// Extends <see cref="BaseObjectSetOptions"/> with flags that request measure/arrange/render invalidations
/// on the visual and/or its parent.
/// </summary>
/// <remarks>
/// Typical consumers are property setters on visual elements that decide which parts of the layout/render
/// pipeline to invalidate after a successful value change. The exact precedence with the base options
/// depends on the consuming setter logic.
/// </remarks>
public class VisualSetOptions : BaseObjectSetOptions
{
    internal static VisualSetOptions _noInvalidate = new()
    {
        InvalidateModes = VisualPropertyInvalidateModes.None
    };

    /// <summary>
    /// Gets or sets the visual invalidation modes to request after the property is set.
    /// </summary>
    /// <value>
    /// A bitwise combination of <see cref="VisualPropertyInvalidateModes"/> flags indicating which phases
    /// to invalidate (e.g., <see cref="VisualPropertyInvalidateModes.Measure"/>,
    /// <see cref="VisualPropertyInvalidateModes.Arrange"/>, <see cref="VisualPropertyInvalidateModes.Render"/>),
    /// optionally including parent-propagating variants. If <see langword="null"/>, the consumer's default
    /// invalidation policy applies. Use <see cref="VisualPropertyInvalidateModes.None"/> to explicitly
    /// suppress invalidation.
    /// </value>
    public virtual VisualPropertyInvalidateModes? InvalidateModes { get; set; }
}
