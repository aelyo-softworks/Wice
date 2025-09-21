namespace Wice;

/// <summary>
/// A draggable splitter visual used inside a <see cref="Grid"/> to resize adjacent rows or columns.
/// </summary>
public partial class GridSplitter : Visual
{
    /// <summary>
    /// Gets the parent grid, if any.
    /// </summary>
    public new Grid? Parent => base.Parent as Grid;

    /// <summary>
    /// Gets the orientation in which the splitter resizes dimensions.
    /// Horizontal means it splits rows (north/south resize); Vertical splits columns (west/east resize).
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual Orientation? Orientation { get; protected set; }

    /// <summary>
    /// Gets the grid dimension (row/column) that the splitter is associated with.
    /// The actual resized dimensions are <see cref="GridDimension.Previous"/> and <see cref="GridDimension.Next"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual GridDimension? Dimension { get; protected set; }

    /// <summary>
    /// Raised when a drag operation completes (mouse released) and sizes are committed.
    /// </summary>
    public event EventHandler? Commit;

    /// <summary>
    /// Raised when a drag operation is canceled (e.g., via ESC) and sizes are restored.
    /// </summary>
    public event EventHandler? Cancel;

    /// <summary>
    /// Called when attached to a parent. Initializes orientation-dependent state and cursor.
    /// </summary>
    /// <param name="sender">The parent visual.</param>
    /// <param name="e">Event data.</param>
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        base.OnAttachedToParent(sender, e);
        UpdateProperties();
    }

    /// <summary>
    /// Creates the drag state used to track original sizes and render sizes during a drag operation.
    /// </summary>
    /// <param name="e">Mouse button args that initiated the drag.</param>
    protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);

    /// <summary>
    /// Raises the <see cref="Commit"/> event.
    /// </summary>
    protected virtual void OnCommit(object sender, EventArgs e) => Commit?.Invoke(sender, e);

    /// <summary>
    /// Raises the <see cref="Cancel"/> event.
    /// </summary>
    protected virtual void OnCancel(object sender, EventArgs e) => Cancel?.Invoke(sender, e);

    /// <summary>
    /// Resolves the target <see cref="Dimension"/> from the parent grid based on <see cref="Orientation"/> and
    /// the splitter's grid position (row/column). Also sets span and cursor, and ensures an auto size in the
    /// cross-axis using the theme's default splitter thickness.
    /// </summary>
    protected virtual void UpdateProperties()
    {
        Dimension = null;
        if (Parent == null || !Orientation.HasValue)
            return;

        var orientation = Orientation.GetValueOrDefault();
        if (orientation == Wice.Orientation.Horizontal)
        {
            if (Parent.Rows.Count < 2)
                return;

            var index = Math.Max(0, Math.Min(Grid.GetRow(this), Parent.Rows.Count - 2));
            Dimension = Parent.Rows[index];
            Grid.SetColumnSpan(this, int.MaxValue);
            Cursor = Cursor.SizeNS;
            if (Height.IsNotSet())
            {
                Height = GetWindowTheme().DefaultSplitterSize;
            }
        }
        else
        {
            if (Parent.Columns.Count < 2)
                return;

            var index = Math.Max(0, Math.Min(Grid.GetColumn(this), Parent.Columns.Count - 2));
            Dimension = Parent.Columns[index];
            Grid.SetRowSpan(this, int.MaxValue);
            Cursor = Cursor.SizeWE;
            if (Width.IsNotSet())
            {
                Width = GetWindowTheme().DefaultSplitterSize;
            }
        }

        // Force Auto size for the dimension the splitter sits on and center default alignment for children there.
        Dimension.Size = float.NaN;
        Dimension.DefaultAlignment = Alignment.Center;
    }

    /// <summary>
    /// Intercepts updates to grid attached properties in order to set <see cref="Orientation"/> and refresh internal state.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Set options.</param>
    /// <returns>True when the stored value changed.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == Grid.RowProperty)
        {
            Orientation = Wice.Orientation.Horizontal;
            ResetPropertyValue(Grid.ColumnProperty);
            UpdateProperties();
        }
        else if (property == Grid.ColumnProperty)
        {
            Orientation = Wice.Orientation.Vertical;
            ResetPropertyValue(Grid.RowProperty);
            UpdateProperties();
        }
        return true;
    }

    /// <summary>
    /// Handles mouse drag by delegating to the type-specific overload, then calls base.
    /// </summary>
    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        OnMouseDrag(e);
        base.OnMouseDrag(sender, e);
    }

    /// <summary>
    /// Adjusts star sizes of the two dimensions adjacent to the splitter while honoring Min/Max constraints.
    /// Converts all other star dimensions in the same axis to their current final size to preserve layout ratios.
    /// </summary>
    /// <param name="e">Drag event args containing the <see cref="SplitDragState"/>.</param>
    protected virtual void OnMouseDrag(DragEventArgs e)
    {
        var prev = Dimension?.Previous;
        var next = Dimension?.Next;
        if (prev == null || next == null)
            return;

        if (!Orientation.HasValue)
            return;

        var orientation = Orientation.GetValueOrDefault();
        var state = (SplitDragState)e.State;
        var delta = orientation == Wice.Orientation.Horizontal ? state.DeltaY : state.DeltaX;

        if (delta != 0 && prev.DesiredSize.HasValue)
        {
            var oldSize = state.NextRenderSize + state.PreviousRenderSize;
            var newPrevSize = Math.Min(Math.Max(0, state.PreviousRenderSize + delta), oldSize);

            var prevMax = prev.MaxSize;
            if (prevMax.IsSet() && prevMax > 0)
            {
                newPrevSize = Math.Min(newPrevSize, prevMax);
            }

            var prevMin = prev.MinSize;
            if (prevMin.IsSet() && prevMin > 0)
            {
                newPrevSize = Math.Max(newPrevSize, prevMin);
            }

            var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
            var newNextSize = plannedNextSize;
            if (next.MaxSize.IsSet() && next.MaxSize > 0)
            {
                newNextSize = Math.Min(newNextSize, next.MaxSize);
                if (newNextSize != plannedNextSize) // incompatible constraints
                    return;
            }

            if (next.MinSize.IsSet() && next.MinSize > 0)
            {
                newNextSize = Math.Max(newNextSize, next.MinSize);
                if (newNextSize != plannedNextSize) // incompatible constraints
                    return;
            }

            IEnumerable<GridDimension> dimensions;
            if (Parent != null && Dimension != null)
            {
                if (orientation == Wice.Orientation.Horizontal)
                {
                    dimensions = Parent.Rows;
                }
                else
                {
                    dimensions = Parent.Columns;
                }

                foreach (var dimension in dimensions)
                {
                    if (dimension == Dimension.Previous)
                    {
                        dimension.Stars = newPrevSize;
                    }
                    else if (dimension == Dimension.Next)
                    {
                        dimension.Stars = newNextSize;
                    }
                    else if (dimension.HasStarSize)
                    {
                        // Freeze other star dimensions to their current final size
                        dimension.Stars = dimension.FinalSize!.Value;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Starts a drag move when the left mouse button is pressed.
    /// </summary>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            DragMove(e);
        }
        base.OnMouseButtonDown(sender, e);
    }

    /// <summary>
    /// Commits the drag on left mouse button release.
    /// </summary>
    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            OnCommit(this, e);
        }
        base.OnMouseButtonUp(sender, e);
    }

    /// <summary>
    /// Cancels the current drag operation (if any), restoring the previous sizes/stars of the adjacent dimensions,
    /// and raises <see cref="Cancel"/>.
    /// </summary>
    /// <param name="e">Event args (e.g., key or mouse).</param>
    protected virtual void CancelDrag(EventArgs e)
    {
        if (CancelDragMove(e) is SplitDragState state && Dimension != null)
        {
            var prev = Dimension.Previous;
            if (prev != null)
            {
                prev.Size = state.PreviousSize;
                prev.Stars = state.PreviousStars;
            }

            var next = Dimension.Next;
            if (next != null)
            {
                next.Size = state.NextSize;
                next.Stars = state.NextStars;
            }

            OnCancel(this, e);
        }
    }

    /// <summary>
    /// Handles ESC to cancel the in-progress drag.
    /// </summary>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == VIRTUAL_KEY.VK_ESCAPE)
        {
            CancelDrag(e);
        }
        base.OnKeyDown(sender, e);
    }

    /// <summary>
    /// Drag state capturing original sizes and render sizes for the two dimensions adjacent to the splitter.
    /// </summary>
    public class SplitDragState : DragState
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SplitDragState"/> capturing the baseline values
        /// for adjacent dimensions at the start of the drag.
        /// </summary>
        /// <param name="visual">The owning <see cref="GridSplitter"/>.</param>
        /// <param name="e">The mouse event that started the drag.</param>
        public SplitDragState(GridSplitter visual, MouseButtonEventArgs e)
            : base(visual, e)
        {
            var prev = visual.Dimension?.Previous;
            if (prev != null)
            {
                PreviousSize = prev.Size;
                PreviousStars = prev.Stars;
                PreviousRenderSize = prev.DesiredSize.GetValueOrDefault();
            }

            var next = visual.Dimension?.Next;
            if (next != null)
            {
                NextSize = next.Size;
                NextStars = next.Stars;
                NextRenderSize = next.DesiredSize.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets the original fixed size of the previous dimension (may be NaN for Auto).
        /// </summary>
        public virtual float PreviousSize { get; protected set; }

        /// <summary>
        /// Gets the original star factor of the previous dimension.
        /// </summary>
        public virtual float PreviousStars { get; protected set; }

        /// <summary>
        /// Gets the render-time desired size of the previous dimension at drag start.
        /// </summary>
        public virtual float PreviousRenderSize { get; protected set; }

        /// <summary>
        /// Gets the original fixed size of the next dimension (may be NaN for Auto).
        /// </summary>
        public virtual float NextSize { get; protected set; }

        /// <summary>
        /// Gets the original star factor of the next dimension.
        /// </summary>
        public virtual float NextStars { get; protected set; }

        /// <summary>
        /// Gets the render-time desired size of the next dimension at drag start.
        /// </summary>
        public virtual float NextRenderSize { get; protected set; }
    }
}
