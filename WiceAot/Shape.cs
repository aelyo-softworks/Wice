namespace Wice;

/// <summary>
/// Base class for vector shapes rendered via Windows Composition.
/// </summary>
/// <remarks>
/// A <see cref="Shape"/> has no intrinsic size (it does not override Measure); its size is defined by layout
/// or by the geometry of the underlying composition shapes. This type proxies common stroke and fill settings
/// to all <see cref="CompositionSpriteShape"/> instances contained in its <see cref="ShapeVisual"/>.
/// </remarks>
public partial class Shape : Visual
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeBrush"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(Shape), nameof(StrokeBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeThickness"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates measure because stroke contributes to arranged bounds.
    /// </remarks>
    public static VisualProperty StrokeThicknessProperty { get; } = VisualProperty.Add<float>(typeof(Shape), nameof(StrokeThickness), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeDashArray"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeDashArrayProperty { get; } = VisualProperty.Add<float[]>(typeof(Shape), nameof(StrokeDashArray), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeStartCap"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeStartCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeStartCap), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeMiterLimit"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering. Default is <c>1.0</c>.
    /// </remarks>
    public static VisualProperty StrokeMiterLimitProperty { get; } = VisualProperty.Add(typeof(Shape), nameof(StrokeMiterLimit), VisualPropertyInvalidateModes.Render, 1f);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeLineJoin"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeLineJoinProperty { get; } = VisualProperty.Add<CompositionStrokeLineJoin>(typeof(Shape), nameof(StrokeLineJoin), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeEndCap"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeEndCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeEndCap), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeDashOffset"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeDashOffsetProperty { get; } = VisualProperty.Add<float>(typeof(Shape), nameof(StrokeDashOffset), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="StrokeDashCap"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty StrokeDashCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeDashCap), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="IsStrokeNonScaling"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering.
    /// </remarks>
    public static VisualProperty IsStrokeNonScalingProperty { get; } = VisualProperty.Add<bool>(typeof(Shape), nameof(IsStrokeNonScaling), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Creates the logical children collection for this shape.
    /// </summary>
    /// <returns>An empty children collection. Shapes do not host child visuals.</returns>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(0);

    /// <summary>
    /// Gets the underlying <see cref="ShapeVisual"/> used to render this shape, or null if not attached.
    /// </summary>
    public new ShapeVisual? CompositionVisual => (ShapeVisual?)base.CompositionVisual;

    /// <summary>
    /// Creates the underlying composition visual for this shape.
    /// </summary>
    /// <returns>
    /// A <see cref="ShapeVisual"/> from the window compositor if available; otherwise, null.
    /// </returns>
    protected override ContainerVisual? CreateCompositionVisual() => Window?.Compositor?.CreateShapeVisual();

    /// <summary>
    /// Gets or sets the fill brush applied to all sprite shapes in this <see cref="Shape"/>.
    /// </summary>
    /// <remarks>
    /// This maps to <c>CompositionSpriteShape.FillBrush</c>. It is an alias of <see cref="Visual.RenderBrush"/>
    /// for consistency with the composition API.
    /// </remarks>
    [Category(CategoryRender)]
    public CompositionBrush? FillBrush { get => RenderBrush; set => RenderBrush = value; }

    /// <summary>
    /// Gets or sets the stroke brush applied to all sprite shapes in this <see cref="Shape"/>.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush? StrokeBrush { get => (CompositionBrush?)GetPropertyValue(StrokeBrushProperty); set => SetPropertyValue(StrokeBrushProperty, value); }

    /// <summary>
    /// Gets or sets the stroke thickness applied to all sprite shapes in this <see cref="Shape"/>.
    /// </summary>
    [Category(CategoryRender)]
    public float StrokeThickness { get => (float)GetPropertyValue(StrokeThicknessProperty)!; set => SetPropertyValue(StrokeThicknessProperty, value); }

    /// <summary>
    /// Gets or sets the dash pattern for the stroke applied to all sprite shapes in this <see cref="Shape"/>.
    /// </summary>
    [Category(CategoryRender)]
    public float[] StrokeDashArray { get => (float[])GetPropertyValue(StrokeDashArrayProperty)!; set => SetPropertyValue(StrokeDashArrayProperty, value); }

    /// <summary>
    /// Gets or sets the starting cap style of the stroke for all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeStartCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeStartCapProperty)!; set => SetPropertyValue(StrokeStartCapProperty, value); }

    /// <summary>
    /// Gets or sets the miter limit for mitered joins on all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public float StrokeMiterLimit { get => (float)GetPropertyValue(StrokeMiterLimitProperty)!; set => SetPropertyValue(StrokeMiterLimitProperty, value); }

    /// <summary>
    /// Gets or sets the line join style for the stroke on all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionStrokeLineJoin StrokeLineJoin { get => (CompositionStrokeLineJoin)GetPropertyValue(StrokeLineJoinProperty)!; set => SetPropertyValue(StrokeLineJoinProperty, value); }

    /// <summary>
    /// Gets or sets the ending cap style of the stroke for all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeEndCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeEndCapProperty)!; set => SetPropertyValue(StrokeEndCapProperty, value); }

    /// <summary>
    /// Gets or sets the dash offset for the stroke on all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public float StrokeDashOffset { get => (float)GetPropertyValue(StrokeDashOffsetProperty)!; set => SetPropertyValue(StrokeDashOffsetProperty, value); }

    /// <summary>
    /// Gets or sets the cap applied to dashes in dashed strokes for all sprite shapes.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeDashCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeDashCapProperty)!; set => SetPropertyValue(StrokeDashCapProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the stroke width should not scale with transforms.
    /// </summary>
    [Category(CategoryRender)]
    public bool IsStrokeNonScaling { get => (bool)GetPropertyValue(IsStrokeNonScalingProperty)!; set => SetPropertyValue(IsStrokeNonScalingProperty, value); }

    /// <summary>
    /// Overrides property setting to mirror persisted values to the composition layer.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns><see langword="true"/> if the stored value changed; otherwise <see langword="false"/>.</returns>
    /// <exception cref="WiceException">Thrown when a UI-thread call is required and not on the UI thread.</exception>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == StrokeBrushProperty)
        {
            SetStrokeBrush((CompositionBrush)value!);
        }
        else if (property == RenderBrushProperty)
        {
            SetFillBrush((CompositionBrush)value!);
        }
        else if (property == StrokeThicknessProperty)
        {
            SetStrokeThicknessProperty((float)value!);
        }
        else if (property == StrokeDashArrayProperty)
        {
            SetStrokeDashArray((float[])value!);
        }
        else if (property == StrokeStartCapProperty)
        {
            SetStrokeStartCap((CompositionStrokeCap)value!);
        }
        else if (property == StrokeMiterLimitProperty)
        {
            SetStrokeMiterLimit((float)value!);
        }
        else if (property == StrokeLineJoinProperty)
        {
            SetStrokeLineJoin((CompositionStrokeLineJoin)value!);
        }
        else if (property == StrokeEndCapProperty)
        {
            SetStrokeEndCap((CompositionStrokeCap)value!);
        }
        else if (property == StrokeDashOffsetProperty)
        {
            SetStrokeDashOffset((float)value!);
        }
        else if (property == StrokeDashCapProperty)
        {
            SetStrokeDashCap((CompositionStrokeCap)value!);
        }
        else if (property == IsStrokeNonScalingProperty)
        {
            SetIsStrokeNonScaling((bool)value!);
        }
        return true;
    }

    /// <summary>
    /// Called when the visual is attached to the composition tree; pushes managed state to composition objects.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        SetStrokeBrush(StrokeBrush);
        SetFillBrush(FillBrush);
        SetStrokeThicknessProperty(StrokeThickness);
        SetStrokeDashArray(StrokeDashArray);
        SetStrokeStartCap(StrokeStartCap);
        SetStrokeMiterLimit(StrokeMiterLimit);
        SetStrokeLineJoin(StrokeLineJoin);
        SetStrokeEndCap(StrokeEndCap);
        SetStrokeDashOffset(StrokeDashOffset);
        SetStrokeDashCap(StrokeDashCap);
        SetIsStrokeNonScaling(IsStrokeNonScaling);
    }

    /// <summary>
    /// Applies the stroke start cap to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The start cap.</param>
    protected virtual void SetStrokeStartCap(CompositionStrokeCap value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeStartCap = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke miter limit to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The miter limit.</param>
    protected virtual void SetStrokeMiterLimit(float value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeMiterLimit = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke line join to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The line join.</param>
    protected virtual void SetStrokeLineJoin(CompositionStrokeLineJoin value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeLineJoin = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke end cap to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The end cap.</param>
    protected virtual void SetStrokeEndCap(CompositionStrokeCap value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeEndCap = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke dash offset to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The dash offset.</param>
    protected virtual void SetStrokeDashOffset(float value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeDashOffset = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke dash cap to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The dash cap.</param>
    protected virtual void SetStrokeDashCap(CompositionStrokeCap value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeDashCap = value;
            }
        }
    }

    /// <summary>
    /// Applies the non-scaling stroke mode to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">True to make stroke thickness independent of transforms; otherwise false.</param>
    protected virtual void SetIsStrokeNonScaling(bool value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.IsStrokeNonScaling = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke dash array to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The dash pattern.</param>
    protected virtual void SetStrokeDashArray(float[] value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeDashArray.Clear();
                shape.StrokeDashArray.AddRange(value);
            }
        }
    }

    /// <summary>
    /// Applies the stroke thickness to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="value">The stroke thickness.</param>
    protected virtual void SetStrokeThicknessProperty(float value)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeThickness = value;
            }
        }
    }

    /// <summary>
    /// Applies the stroke brush to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="brush">The brush to apply.</param>
    protected virtual void SetStrokeBrush(CompositionBrush? brush)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.StrokeBrush = brush;
            }
        }
    }

    /// <summary>
    /// Applies the fill brush to all <see cref="CompositionSpriteShape"/> children of the <see cref="CompositionVisual"/>.
    /// </summary>
    /// <param name="brush">The brush to apply.</param>
    /// <remarks>
    /// If you get "The parameter is incorrect. Invalid argument to parameter value.Unsupported source brush type."
    /// ensure you are using a brush created by the compositor associated with the owning <see cref="Window"/>.
    /// </remarks>
    protected virtual void SetFillBrush(CompositionBrush? brush)
    {
        var shapeVisual = CompositionVisual;
        if (shapeVisual != null)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                // if you get "The parameter is incorrect. Invalid argument to parameter value.Unsupported source brush type.'"
                // then you're not using the Compositor corresponding to the Window
                shape.FillBrush = brush;
            }
        }
    }
}
