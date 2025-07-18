namespace Wice;

public partial class GridSplitter : Visual
{
    public new Grid? Parent => base.Parent as Grid;

    [Category(CategoryBehavior)]
    public virtual Orientation? Orientation { get; protected set; }

    [Category(CategoryBehavior)]
    public virtual GridDimension? Dimension { get; protected set; }

    public event EventHandler? Commit;
    public event EventHandler? Cancel;

    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        base.OnAttachedToParent(sender, e);
        UpdateProperties();
    }

    protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);
    protected virtual void OnCommit(object sender, EventArgs e) => Commit?.Invoke(sender, e);
    protected virtual void OnCancel(object sender, EventArgs e) => Cancel?.Invoke(sender, e);
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

        Dimension.Size = float.NaN;
        Dimension.DefaultAlignment = Alignment.Center;
    }

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

    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        OnMouseDrag(e);
        base.OnMouseDrag(sender, e);
    }

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
                        dimension.Stars = dimension.FinalSize!.Value;
                    }
                }
            }
        }
    }

    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            DragMove(e);
        }
        base.OnMouseButtonDown(sender, e);
    }

    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            OnCommit(this, e);
        }
        base.OnMouseButtonUp(sender, e);
    }

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

    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == VIRTUAL_KEY.VK_ESCAPE)
        {
            CancelDrag(e);
        }
        base.OnKeyDown(sender, e);
    }

    public class SplitDragState : DragState
    {
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

        public virtual float PreviousSize { get; protected set; }
        public virtual float PreviousStars { get; protected set; }
        public virtual float PreviousRenderSize { get; protected set; }
        public virtual float NextSize { get; protected set; }
        public virtual float NextStars { get; protected set; }
        public virtual float NextRenderSize { get; protected set; }
    }
}
