namespace Wice;

/// <summary>
/// Displays a single <see cref="CompositionSpriteShape"/> built from one <see cref="CompositionGeometry"/>.
/// </summary>
public abstract class SingleShape : Shape, IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleShape"/> class.
    /// </summary>
    protected SingleShape()
    {
    }

    /// <summary>
    /// Gets the single composition geometry used by this visual once attached to composition.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionGeometry? Geometry { get; private set; }

    /// <summary>
    /// Gets the composition sprite shape created from <see cref="Geometry"/> once attached to composition.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionSpriteShape? Shape { get; private set; }

    /// <summary>
    /// Creates the <see cref="CompositionGeometry"/> used by this visual.
    /// </summary>
    /// <returns>A non-null geometry created with the current window compositor.</returns>
    protected abstract CompositionGeometry? CreateGeometry();

    /// <summary>
    /// Creates the <see cref="CompositionSpriteShape"/> to be added to the underlying <see cref="ShapeVisual"/>.
    /// </summary>
    /// <returns>
    /// The created sprite shape, or null to indicate failure (which will cause an exception during attach).
    /// </returns>
    protected virtual CompositionSpriteShape? CreateShape() => Window?.Compositor?.CreateSpriteShape(Geometry);

    /// <inheritdoc/>
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

    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only
    /// unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Shape?.Dispose();
                Geometry?.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
