namespace Wice;

/// <summary>
/// A panel-like visual that arranges its visible children in a primary direction and wraps to a new line
/// when there is no more space. The primary axis is controlled by <see cref="Orientation"/>:
/// - Horizontal: items flow left-to-right, wrapping to a new row.
/// - Vertical: items flow top-to-bottom, wrapping to a new column.
/// </summary>
public partial class Wrap : Visual
{
    /// <summary>
    /// Gets the visual property backing <see cref="Orientation"/>.
    /// Changing the orientation triggers a measure pass.
    /// </summary>
    public static VisualProperty OrientationProperty { get; } = VisualProperty.Add(typeof(Wrap), nameof(Orientation), VisualPropertyInvalidateModes.Measure, Orientation.Vertical);

    /// <summary>
    /// Gets the visual property backing <see cref="ItemWidth"/>.
    /// Setting a value triggers a parent measure pass.
    /// </summary>
    public static VisualProperty ItemWidthProperty { get; } = VisualProperty.Add(typeof(Wrap), nameof(ItemWidth), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Gets the visual property backing <see cref="ItemHeight"/>.
    /// Setting a value triggers a parent measure pass.
    /// </summary>
    public static VisualProperty ItemHeightProperty { get; } = VisualProperty.Add(typeof(Wrap), nameof(ItemHeight), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Gets or sets the primary flow direction.
    /// Horizontal flows items left-to-right and wraps to a new row.
    /// Vertical flows items top-to-bottom and wraps to a new column.
    /// </summary>
    [Category(CategoryLayout)]
    public Orientation Orientation { get => (Orientation)GetPropertyValue(OrientationProperty)!; set => SetPropertyValue(OrientationProperty, value); }

    /// <summary>
    /// Gets or sets a fixed item width (DIPs). Use <see cref="float.NaN"/> to use each child's desired width.
    /// </summary>
    [Category(CategoryLayout)]
    public float ItemWidth { get => (float)GetPropertyValue(ItemWidthProperty)!; set => SetPropertyValue(ItemWidthProperty, value); }

    /// <summary>
    /// Gets or sets a fixed item height (DIPs). Use <see cref="float.NaN"/> to use each child's desired height.
    /// </summary>
    [Category(CategoryLayout)]
    public float ItemHeight { get => (float)GetPropertyValue(ItemHeightProperty)!; set => SetPropertyValue(ItemHeightProperty, value); }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var orientation = Orientation;
        var curLineSize = new UVSize(orientation);
        var panelSize = new UVSize(orientation);
        var uvConstraint = new UVSize(orientation, constraint.width, constraint.height);
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var itemWidthSet = itemWidth.IsSet();
        var itemHeightSet = itemHeight.IsSet();

        var childConstraint = new D2D_SIZE_F(itemWidthSet ? itemWidth : constraint.width, itemHeightSet ? itemHeight : constraint.height);

        var children = VisibleChildren.Where(c => c.Parent != null).ToArray();
        foreach (var child in children)
        {
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;

            var sz = new UVSize(orientation, itemWidthSet ? itemWidth : childSize.width, itemHeightSet ? itemHeight : childSize.height);
            if ((curLineSize.U + sz.U) > uvConstraint.U)
            {
                // Wrap to next line.
                panelSize.U = Math.Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V;
                curLineSize = sz;

                // Extremely wide/tall single item: place it on its own line/column.
                if (sz.U > uvConstraint.U)
                {
                    panelSize.U = Math.Max(sz.U, panelSize.U);
                    panelSize.V += sz.V;
                    curLineSize = new UVSize(orientation);
                }
            }
            else
            {
                // Accumulate within the current line.
                curLineSize.U += sz.U;
                curLineSize.V = Math.Max(sz.V, curLineSize.V);
            }
        }

        // Account for the final line.
        panelSize.U = Math.Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V;
        return new D2D_SIZE_F(panelSize.Width, panelSize.Height);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var orientation = Orientation;
        var firstInLine = 0;
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var accumulatedV = 0f;
        var itemU = orientation == Orientation.Horizontal ? itemWidth : itemHeight;
        var curLineSize = new UVSize(orientation);
        var uvFinalSize = new UVSize(orientation, finalRect.Width, finalRect.Height);
        var itemWidthSet = itemWidth.IsSet();
        var itemHeightSet = itemHeight.IsSet();
        var useItemU = orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
        var children = VisibleChildren.ToArray();

        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child.Parent == null)
                continue; // skip detached children

            var childSize = child.DesiredSize;
            var sz = new UVSize(orientation, itemWidthSet ? itemWidth : childSize.width, itemHeightSet ? itemHeight : childSize.height);

            if ((curLineSize.U + sz.U) > uvFinalSize.U)
            {
                // Arrange the current line, then start a new one.
                ArrangeLine(orientation, children, accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU);

                accumulatedV += curLineSize.V;
                curLineSize = sz;

                // Extremely wide/tall single item: arrange it alone on a line/column.
                if (sz.U > uvFinalSize.U)
                {
                    ArrangeLine(orientation, children, accumulatedV, sz.V, i, ++i, useItemU, itemU);
                    accumulatedV += sz.V;
                    curLineSize = new UVSize(orientation);
                }
                firstInLine = i;
            }
            else
            {
                curLineSize.U += sz.U;
                curLineSize.V = Math.Max(sz.V, curLineSize.V);
            }
        }

        // Arrange the trailing line.
        if (firstInLine < children.Length)
        {
            ArrangeLine(orientation, children, accumulatedV, curLineSize.V, firstInLine, children.Length, useItemU, itemU);
        }
    }

    private static void ArrangeLine(Orientation orientation, Visual[] children, float v, float lineV, int start, int end, bool useItemU, float itemU)
    {
        var u = 0f;
        var isHorizontal = orientation == Orientation.Horizontal;

        for (var i = start; i < end; i++)
        {
            var child = children[i];
            var childSize = child.DesiredSize;
            var sz = new UVSize(orientation, childSize.width, childSize.height);
            var layoutSlotU = useItemU ? itemU : sz.U;
            child.Arrange(D2D_RECT_F.Sized(isHorizontal ? u : v, isHorizontal ? v : u, isHorizontal ? layoutSlotU : lineV, isHorizontal ? lineV : layoutSlotU));
            u += layoutSlotU;
        }
    }

    private struct UVSize(Orientation orientation)
    {
        public float U = 0;
        public float V = 0;
        public Orientation Orientation = orientation;

        public UVSize(Orientation orientation, float width, float height)
            : this(orientation)
        {
            Width = width;
            Height = height;
        }

        public float Width { readonly get => Orientation == Orientation.Horizontal ? U : V; set { if (Orientation == Orientation.Horizontal) U = value; else V = value; } }
        public float Height { readonly get => Orientation == Orientation.Horizontal ? V : U; set { if (Orientation == Orientation.Horizontal) V = value; else U = value; } }
    }
}
