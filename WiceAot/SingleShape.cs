namespace Wice;

/// <summary>
/// Displays a single <see cref="CompositionSpriteShape"/> built from one <see cref="CompositionGeometry"/>.
/// </summary>
/// <remarks>
/// Lifecycle:
/// - When attached to composition, a geometry is created via <see cref="CreateGeometry"/> (must not be null).
/// - A sprite shape is then created via <see cref="CreateShape"/> and added to the owning <see cref="Shape.CompositionVisual"/>.
/// - In DEBUG builds, composition comments are set to <see cref="Visual.Name"/> for diagnostics.
/// </remarks>
public abstract class SingleShape : Shape
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleShape"/> class.
    /// </summary>
    protected SingleShape()
    {
    }

    /// <summary>
    /// Gets the single composition geometry used by this visual once attached to composition.
    /// </summary>
    /// <remarks>
    /// The value is set during <see cref="OnAttachedToComposition(object?, EventArgs)"/> and remains unchanged
    /// for the lifetime of the attachment. It is null before attachment and after detachment.
    /// </remarks>
    [Category(CategoryRender)]
    public CompositionGeometry? Geometry { get; private set; }

    /// <summary>
    /// Gets the composition sprite shape created from <see cref="Geometry"/> once attached to composition.
    /// </summary>
    /// <remarks>
    /// The value is set during <see cref="OnAttachedToComposition(object?, EventArgs)"/> and remains unchanged
    /// for the lifetime of the attachment. It is null before attachment and after detachment.
    /// </remarks>
    [Category(CategoryRender)]
    public CompositionSpriteShape? Shape { get; private set; }

    /// <summary>
    /// Creates the <see cref="CompositionGeometry"/> used by this visual.
    /// </summary>
    /// <returns>A non-null geometry created with the current window compositor.</returns>
    /// <remarks>
    /// Implementers must return a geometry associated with the owning window's compositor.
    /// Returning null will cause <see cref="OnAttachedToComposition(object?, EventArgs)"/> to throw.
    /// </remarks>
    protected abstract CompositionGeometry? CreateGeometry();

    /// <summary>
    /// Creates the <see cref="CompositionSpriteShape"/> to be added to the underlying <see cref="ShapeVisual"/>.
    /// </summary>
    /// <returns>
    /// The created sprite shape, or null to indicate failure (which will cause an exception during attach).
    /// </returns>
    /// <remarks>
    /// The default implementation calls <c>Window?.Compositor?.CreateSpriteShape(Geometry)</c>.
    /// Override to customize shape parameters before returning (e.g., fill/stroke).
    /// </remarks>
    protected virtual CompositionSpriteShape? CreateShape() => Window?.Compositor?.CreateSpriteShape(Geometry);

    /// <summary>
    /// Called when this visual is attached to the composition tree; builds geometry and sprite shape and
    /// adds it to the <see cref="Shape.CompositionVisual"/>.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event args.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when geometry or shape creation fails, or when the composition visual is not available.
    /// </exception>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        Geometry = CreateGeometry();
        if (Geometry == null)
            throw new InvalidOperationException();

#if DEBUG
        Geometry.Comment = Name;
#endif

        Shape = CreateShape();
        if (Shape == null)
            throw new InvalidOperationException();

#if DEBUG
        Shape.Comment = Name;
#endif
        if (CompositionVisual == null)
            throw new InvalidOperationException();

        CompositionVisual.Shapes.Add(Shape);
        base.OnAttachedToComposition(sender, e);
    }
}
