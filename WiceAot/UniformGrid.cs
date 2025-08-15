namespace Wice;

/// <summary>
/// Arranges visible children into a uniform grid of <see cref="Rows"/> by <see cref="Columns"/> cells,
/// measuring each child against the cell size and arranging them left-to-right, top-to-bottom.
/// </summary>
/// <remarks>
/// - Measurement:
///   - If <see cref="Columns"/> or <see cref="Rows"/> is 0, defers to <see cref="RenderVisual.MeasureCore(D2D_SIZE_F)"/>.
///   - Otherwise, divides the constraint into cell size (constraint/cols, constraint/rows), measures all visible children,
///     then returns the grid size as max(childDesiredWidth) * cols + inter-column spacing and
///     max(childDesiredHeight) * rows + inter-row spacing.
/// - Arrangement:
///   - Computes cell width/height from the final rect divided by columns/rows.
///   - Lays out children in reading order. After each column, adds <see cref="ColumnsLineWidth"/>; after each row, adds <see cref="RowsLineWidth"/>.
///   - Applies floor/ceiling rounding to reduce sub-pixel rendering artifacts.
/// - Rendering (background only here):
///   - Clears to <see cref="RenderVisual.BackgroundColor"/> when set.
///   - Otherwise, clears to transparent if the grid is not fully populated (<c>Children.Count &lt; Rows * Columns</c>).
/// </remarks>
/// <seealso cref="RenderVisual"/>
public partial class UniformGrid : RenderVisual
{
    /// <summary>
    /// Attached visual property that stores the number of columns in the grid.
    /// Changing this property invalidates measure on self and parent as needed.
    /// Default is 0 (disabled, behave like a simple panel).
    /// </summary>
    public static VisualProperty ColumnsProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(Columns), VisualPropertyInvalidateModes.Measure, 0);

    /// <summary>
    /// Attached visual property that stores the number of rows in the grid.
    /// Changing this property invalidates measure on self and parent as needed.
    /// Default is 0 (disabled, behave like a simple panel).
    /// </summary>
    public static VisualProperty RowsProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(Rows), VisualPropertyInvalidateModes.Measure, 0);

    /// <summary>
    /// Inter-row spacing (line width) added between arranged rows, in DIPs.
    /// Changing this property invalidates measure.
    /// Default is 0.
    /// </summary>
    public static VisualProperty RowsLineWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(RowsLineWidth), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Inter-column spacing (line width) added between arranged columns, in DIPs.
    /// Changing this property invalidates measure.
    /// Default is 0.
    /// </summary>
    public static VisualProperty ColumnsLineWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(ColumnsLineWidth), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Stroke width used when rendering grid lines (if lines are drawn by a derived class).
    /// Changing this property invalidates measure.
    /// Default is 0.
    /// </summary>
    public static VisualProperty LineStrokeWidthProperty { get; } = VisualProperty.Add(typeof(UniformGrid), nameof(LineStrokeWidth), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Stroke style used when rendering grid lines (if lines are drawn by a derived class).
    /// Changing this property invalidates render.
    /// </summary>
    public static VisualProperty LineStrokeStyleProperty { get; } = VisualProperty.Add<D2D1_STROKE_STYLE_PROPERTIES?>(typeof(UniformGrid), nameof(LineStrokeStyle), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Brush used when rendering grid lines (if lines are drawn by a derived class).
    /// Changing this property invalidates render.
    /// </summary>
    public static VisualProperty LineBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(UniformGrid), nameof(LineBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets or sets the number of columns to layout children across.
    /// Setting to 0 disables the uniform grid behavior for measurement and arrangement.
    /// </summary>
    [Category(CategoryBehavior)]
    public int Columns { get => (int)GetPropertyValue(ColumnsProperty)!; set => SetPropertyValue(ColumnsProperty, value); }

    /// <summary>
    /// Gets or sets the number of rows to layout children down.
    /// Setting to 0 disables the uniform grid behavior for measurement and arrangement.
    /// </summary>
    [Category(CategoryBehavior)]
    public int Rows { get => (int)GetPropertyValue(RowsProperty)!; set => SetPropertyValue(RowsProperty, value); }

    /// <summary>
    /// Gets or sets the spacing line width inserted between rows (in DIPs).
    /// This affects the measured size and final arrangement.
    /// </summary>
    [Category(CategoryLayout)]
    public float RowsLineWidth { get => (float)GetPropertyValue(RowsLineWidthProperty)!; set => SetPropertyValue(RowsLineWidthProperty, value); }

    /// <summary>
    /// Gets or sets the spacing line width inserted between columns (in DIPs).
    /// This affects the measured size and final arrangement.
    /// </summary>
    [Category(CategoryLayout)]
    public float ColumnsLineWidth { get => (float)GetPropertyValue(ColumnsLineWidthProperty)!; set => SetPropertyValue(ColumnsLineWidthProperty, value); }

    /// <summary>
    /// Gets or sets the stroke width to use for drawing grid lines (if implemented by a derived class).
    /// </summary>
    [Category(CategoryLayout)]
    public float LineStrokeWidth { get => (float)GetPropertyValue(LineStrokeWidthProperty)!; set => SetPropertyValue(LineStrokeWidthProperty, value); }

    /// <summary>
    /// Gets or sets the optional stroke style for grid lines (if implemented by a derived class).
    /// </summary>
    [Category(CategoryLayout)]
    public D2D1_STROKE_STYLE_PROPERTIES? LineStrokeStyle { get => (D2D1_STROKE_STYLE_PROPERTIES?)GetPropertyValue(LineStrokeStyleProperty); set => SetPropertyValue(LineStrokeStyleProperty, value); }

    /// <summary>
    /// Gets or sets the brush to use when drawing grid lines (if implemented by a derived class).
    /// </summary>
    [Category(CategoryLayout)]
    public Brush LineBrush { get => (Brush)GetPropertyValue(LineBrushProperty)!; set => SetPropertyValue(LineBrushProperty, value); }

    /// <summary>
    /// Measures the desired size of the grid based on the maximum desired size of its visible children per cell,
    /// multiplied by the number of columns/rows and including inter-row/column line widths.
    /// </summary>
    /// <param name="constraint">The available size for the grid (width/height in DIPs).</param>
    /// <returns>
    /// The desired size of the grid. If <see cref="Columns"/> or <see cref="Rows"/> is 0, defers to base measurement.
    /// </returns>
    /// <remarks>
    /// Each child is measured with a constraint equal to the cell size (constraint/cols, constraint/rows).
    /// The resulting grid desired size is:
    /// width = maxChildDesiredWidth * cols + <see cref="ColumnsLineWidth"/> * (cols - 1),
    /// height = maxChildDesiredHeight * rows + <see cref="RowsLineWidth"/> * (rows - 1).
    /// </remarks>
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
        foreach (var child in children.Where(c => c.Parent != null))
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

    /// <summary>
    /// Arranges visible children into grid cells computed from the final rectangle.
    /// Children are placed left-to-right and wrap to the next row when the row is filled.
    /// </summary>
    /// <param name="finalRect">The final content rectangle available for arranging (excluding margin).</param>
    /// <remarks>
    /// - If <see cref="Columns"/> or <see cref="Rows"/> is 0, delegates to base arrangement.
    /// - Cell size is <c>finalSize / (cols, rows)</c>.
    /// - After each placed child, advances by cell width plus <see cref="ColumnsLineWidth"/>.
    /// - After each row, advances by cell height plus <see cref="RowsLineWidth"/> and resets column position.
    /// - Uses floor/ceiling to reduce sub-pixel artifacts on bounds.
    /// </remarks>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
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
        foreach (var child in children.Where(c => c.Parent != null))
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

    /// <summary>
    /// Clears the device context to <see cref="RenderVisual.BackgroundColor"/> when set.
    /// If not set and the grid is not fully populated (<c>Children.Count &lt; Rows * Columns</c>), clears to transparent.
    /// Otherwise, does not clear (preserves existing content).
    /// </summary>
    /// <param name="context">The render context for the current draw pass.</param>
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
