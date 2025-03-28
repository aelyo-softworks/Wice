namespace Wice;

// note shape has no size by itself (Measure not override)
public partial class Shape : Visual
{
    public static VisualProperty StrokeBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(Shape), nameof(StrokeBrush), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeThicknessProperty { get; } = VisualProperty.Add<float>(typeof(Shape), nameof(StrokeThickness), VisualPropertyInvalidateModes.Measure);
    public static VisualProperty StrokeDashArrayProperty { get; } = VisualProperty.Add<float[]>(typeof(Shape), nameof(StrokeDashArray), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeStartCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeStartCap), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeMiterLimitProperty { get; } = VisualProperty.Add(typeof(Shape), nameof(StrokeMiterLimit), VisualPropertyInvalidateModes.Render, 1f);
    public static VisualProperty StrokeLineJoinProperty { get; } = VisualProperty.Add<CompositionStrokeLineJoin>(typeof(Shape), nameof(StrokeLineJoin), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeEndCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeEndCap), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeDashOffsetProperty { get; } = VisualProperty.Add<float>(typeof(Shape), nameof(StrokeDashOffset), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StrokeDashCapProperty { get; } = VisualProperty.Add<CompositionStrokeCap>(typeof(Shape), nameof(StrokeDashCap), VisualPropertyInvalidateModes.Render);
    public static VisualProperty IsStrokeNonScalingProperty { get; } = VisualProperty.Add<bool>(typeof(Shape), nameof(IsStrokeNonScaling), VisualPropertyInvalidateModes.Render);

    protected override BaseObjectCollection<Visual> CreateChildren() => new(0);
    public new ShapeVisual? CompositionVisual => (ShapeVisual?)base.CompositionVisual;
    protected override ContainerVisual? CreateCompositionVisual() => Window?.Compositor?.CreateShapeVisual();

    // to be consistent with CompositionSpriteShape's FillBrush property
    [Category(CategoryRender)]
    public CompositionBrush? FillBrush { get => RenderBrush; set => RenderBrush = value; }

    [Category(CategoryRender)]
    public CompositionBrush? StrokeBrush { get => (CompositionBrush?)GetPropertyValue(StrokeBrushProperty); set => SetPropertyValue(StrokeBrushProperty, value); }

    [Category(CategoryRender)]
    public float StrokeThickness { get => (float)GetPropertyValue(StrokeThicknessProperty)!; set => SetPropertyValue(StrokeThicknessProperty, value); }

    [Category(CategoryRender)]
    public float[] StrokeDashArray { get => (float[])GetPropertyValue(StrokeDashArrayProperty)!; set => SetPropertyValue(StrokeDashArrayProperty, value); }

    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeStartCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeStartCapProperty)!; set => SetPropertyValue(StrokeStartCapProperty, value); }

    [Category(CategoryRender)]
    public float StrokeMiterLimit { get => (float)GetPropertyValue(StrokeMiterLimitProperty)!; set => SetPropertyValue(StrokeMiterLimitProperty, value); }

    [Category(CategoryRender)]
    public CompositionStrokeLineJoin StrokeLineJoin { get => (CompositionStrokeLineJoin)GetPropertyValue(StrokeLineJoinProperty)!; set => SetPropertyValue(StrokeLineJoinProperty, value); }

    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeEndCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeEndCapProperty)!; set => SetPropertyValue(StrokeEndCapProperty, value); }

    [Category(CategoryRender)]
    public float StrokeDashOffset { get => (float)GetPropertyValue(StrokeDashOffsetProperty)!; set => SetPropertyValue(StrokeDashOffsetProperty, value); }

    [Category(CategoryRender)]
    public CompositionStrokeCap StrokeDashCap { get => (CompositionStrokeCap)GetPropertyValue(StrokeDashCapProperty)!; set => SetPropertyValue(StrokeDashCapProperty, value); }

    [Category(CategoryRender)]
    public bool IsStrokeNonScaling { get => (bool)GetPropertyValue(IsStrokeNonScalingProperty)!; set => SetPropertyValue(IsStrokeNonScalingProperty, value); }

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
