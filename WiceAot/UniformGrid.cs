namespace Wice;

public partial class UniformGrid : RenderVisual
{
    public static VisualProperty ColumnsProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(Columns), VisualPropertyInvalidateModes.Measure, 0);
    public static VisualProperty RowsProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(Rows), VisualPropertyInvalidateModes.Measure, 0);
    public static VisualProperty RowsLineWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(RowsLineWidth), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty ColumnsLineWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(ColumnsLineWidth), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty LineStrokeWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(LineStrokeWidth), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty LineStrokeStyleProperty { get; } = VisualProperty.Add<D2D1_STROKE_STYLE_PROPERTIES?>(typeof(UniformGrid), nameof(LineStrokeStyle), VisualPropertyInvalidateModes.Render);
    public static VisualProperty LineBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(UniformGrid), nameof(LineBrush), VisualPropertyInvalidateModes.Render);

    [Category(CategoryBehavior)]
    public int Columns { get => (int)GetPropertyValue(ColumnsProperty)!; set => SetPropertyValue(ColumnsProperty, value); }

    [Category(CategoryBehavior)]
    public int Rows { get => (int)GetPropertyValue(RowsProperty)!; set => SetPropertyValue(RowsProperty, value); }

    [Category(CategoryLayout)]
    public float RowsLineWidth { get => (float)GetPropertyValue(RowsLineWidthProperty)!; set => SetPropertyValue(RowsLineWidthProperty, value); }

    [Category(CategoryLayout)]
    public float ColumnsLineWidth { get => (float)GetPropertyValue(ColumnsLineWidthProperty)!; set => SetPropertyValue(ColumnsLineWidthProperty, value); }

    [Category(CategoryLayout)]
    public float LineStrokeWidth { get => (float)GetPropertyValue(LineStrokeWidthProperty)!; set => SetPropertyValue(LineStrokeWidthProperty, value); }

    [Category(CategoryLayout)]
    public D2D1_STROKE_STYLE_PROPERTIES? LineStrokeStyle { get => (D2D1_STROKE_STYLE_PROPERTIES?)GetPropertyValue(LineStrokeStyleProperty); set => SetPropertyValue(LineStrokeStyleProperty, value); }

    [Category(CategoryLayout)]
    public Brush LineBrush { get => (Brush)GetPropertyValue(LineBrushProperty)!; set => SetPropertyValue(LineBrushProperty, value); }

    private IComObject<ID2D1StrokeStyle>? GetLinesStroke()
    {
        var style = LineStrokeStyle;
        if (!style.HasValue)
            return null;

        return Application.Current.ResourceManager.GetStrokeStyle(style.Value);
    }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var cols = Columns;
        var rows = Rows;
        if (cols == 0 || rows == 0)
            return base.MeasureCore(constraint);

        var childConstraint = new D2D_SIZE_F(constraint.width / cols, constraint.height / rows);
        var maxChildDesiredWidth = 0f;
        var maxChildDesiredHeight = 0f;

        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (maxChildDesiredWidth < childDesiredSize.width)
            {
                maxChildDesiredWidth = childDesiredSize.width;
            }

            if (maxChildDesiredHeight < childDesiredSize.height)
            {
                maxChildDesiredHeight = childDesiredSize.height;
            }
        }

        var rowsLine = RowsLineWidth;
        var colsLine = ColumnsLineWidth;
        return new D2D_SIZE_F(maxChildDesiredWidth * cols + colsLine * (cols - 1), maxChildDesiredHeight * rows + rowsLine * (rows - 1));
    }

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        //Application.Trace("rows=" + Rows);
        var cols = Columns;
        var rows = Rows;
        if (cols == 0 || rows == 0)
        {
            base.ArrangeCore(finalRect);
            return;
        }

        var rowsLine = RowsLineWidth;
        var colsLine = ColumnsLineWidth;

        var finalSize = finalRect.Size;
        var bound = finalSize.width - 1;

        var left = 0f;
        var top = 0f;
        var width = finalSize.width / cols;
        var height = finalSize.height / rows;

        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var bounds = D2D_RECT_F.Sized(left.Floor(), top.Floor(), width.Ceiling(), height.Ceiling());
            child.Arrange(bounds);
            left += width + colsLine;
            if (left >= bound)
            {
                top += height + rowsLine;
                left = 0;
            }
        }
    }

    protected override void RenderBackgroundCore(RenderContext context)
    {
        var bg = BackgroundColor;
        if (bg.HasValue)
        {
            context.DeviceContext.Clear(bg.Value);
        }
        else
        {
            if (Children.Count < Rows * Columns)
            {
                context.DeviceContext.Clear(D3DCOLORVALUE.Transparent);
            }
        }
    }
}
