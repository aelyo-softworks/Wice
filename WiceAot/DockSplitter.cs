namespace Wice;

public partial class DockSplitter : Visual
{
    public static VisualProperty SizeProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Size), VisualPropertyInvalidateModes.Measure, 5f);

    public event EventHandler? Commit;
    public event EventHandler? Cancel;

    private Dock? ParentDock => Parent as Dock;

    [Category(CategoryBehavior)]
    public virtual DockType DockType { get; protected set; }

    [Category(CategoryBehavior)]
    public virtual Orientation Orientation { get; protected set; }

    [Category(CategoryBehavior)]
    public virtual float HitTestTolerance { get; set; }

    [Category(CategoryBehavior)]
    public virtual float Size { get => (float)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);
    protected virtual void OnCommit(object sender, EventArgs e) => Commit?.Invoke(sender, e);
    protected virtual void OnCancel(object sender, EventArgs e) => Cancel?.Invoke(sender, e);
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        var parent = Parent;
        var index = parent!.Children.IndexOf(this);
        DockType = index <= 0 ? DockType.Left : Dock.GetDockType(parent.Children[index - 1]);
        Orientation = Dock.GetOrientation(DockType);
        Cursor = Orientation == Orientation.Horizontal ? Cursor.SizeWE : Cursor.SizeNS;
        Dock.SetDockType(this, DockType);

        var size = Math.Max(1, Size);
        if (Orientation == Orientation.Horizontal)
        {
            Width = size;
        }
        else
        {
            Height = size;
        }
        base.OnAttachedToParent(sender, e);
    }

    protected override D2D_RECT_F GetHitTestBounds(D2D_RECT_F defaultBounds)
    {
        var bounds = base.GetHitTestBounds(defaultBounds);
        if (HitTestTolerance != 0)
        {
            bounds.left -= HitTestTolerance;
            bounds.top -= HitTestTolerance;
            bounds.right += HitTestTolerance;
            bounds.bottom += HitTestTolerance;
        }
        return bounds;
    }

    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        OnMouseDrag(e);
        base.OnMouseDrag(sender, e);
    }

    protected virtual void OnMouseDrag(DragEventArgs e)
    {
        var state = (SplitDragState)e.State;
        if (state.Prev == null || state.Next == null)
            return;

        var delta = Orientation == Orientation.Horizontal ? state.DeltaX : state.DeltaY;
        if (delta != 0)
        {
            var oldSize = state.NextRenderSize + state.PreviousRenderSize;
            var newPrevSize = Math.Min(Math.Max(0, state.PreviousRenderSize + delta), oldSize);

            if (Orientation == Orientation.Horizontal)
            {
                if (state.Prev.MaxWidth.IsSet() && state.Prev.MaxWidth > 0)
                {
                    newPrevSize = Math.Min(newPrevSize, state.Prev.MaxWidth);
                }

                if (state.Prev.MinWidth.IsSet() && state.Prev.MinWidth > 0)
                {
                    newPrevSize = Math.Max(newPrevSize, state.Prev.MinWidth);
                }

                var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
                var newNextSize = plannedNextSize;
                if (state.Next.MaxWidth.IsSet() && state.Next.MaxWidth > 0)
                {
                    newNextSize = Math.Min(newNextSize, state.Next.MaxWidth);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (state.Next.MinWidth.IsSet() && state.Next.MinWidth > 0)
                {
                    newNextSize = Math.Max(newNextSize, state.Next.MinWidth);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (!state.PrevIsLastChild)
                {
                    state.Prev.Width = newPrevSize;
                }

                if (!state.NextIsLastChild)
                {
                    state.Next.Width = newNextSize;
                }
            }
            else
            {
                if (state.Prev.MaxHeight.IsSet() && state.Prev.MaxHeight > 0)
                {
                    newPrevSize = Math.Min(newPrevSize, state.Prev.MaxHeight);
                }

                if (state.Prev.MinHeight.IsSet() && state.Prev.MinHeight > 0)
                {
                    newPrevSize = Math.Max(newPrevSize, state.Prev.MinHeight);
                }

                var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
                var newNextSize = plannedNextSize;
                if (state.Next.MaxHeight.IsSet() && state.Next.MaxHeight > 0)
                {
                    newNextSize = Math.Min(newNextSize, state.Next.MaxHeight);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (state.Next.MinHeight.IsSet() && state.Next.MinHeight > 0)
                {
                    newNextSize = Math.Max(newNextSize, state.Next.MinHeight);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (!state.PrevIsLastChild)
                {
                    state.Prev.Height = newPrevSize;
                }

                if (!state.NextIsLastChild)
                {
                    state.Next.Height = newNextSize;
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
        if (CancelDragMove(e) is SplitDragState state)
        {
            if (state.Prev != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    state.Prev.Width = state.PreviousRenderSize;
                }
                else
                {
                    state.Prev.Height = state.PreviousRenderSize;
                }
            }

            if (state.Next != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    state.Next.Width = state.NextRenderSize;
                }
                else
                {
                    state.Next.Height = state.NextRenderSize;
                }
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

    protected Visual? GetPrevVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Left : DockType.Top);
    protected Visual? GetNextVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Right : DockType.Bottom);

    public class SplitDragState : DragState
    {
        public SplitDragState(DockSplitter visual, MouseButtonEventArgs e)
            : base(visual, e)
        {
            var lastChildFill = visual.ParentDock?.LastChildFill == true;

            Prev = visual.GetPrevVisual();
            if (Prev != null)
            {
                PrevIsLastChild = lastChildFill && Prev == visual.ParentDock?._lastChild;
                if (visual.Orientation == Orientation.Horizontal)
                {
                    PreviousRenderSize = Prev.ArrangedRect.Width;
                }
                else
                {
                    PreviousRenderSize = Prev.ArrangedRect.Height;
                }
            }

            Next = visual.GetNextVisual();
            if (Next != null)
            {
                NextIsLastChild = lastChildFill && Next == visual.ParentDock?._lastChild;
                if (visual.Orientation == Orientation.Horizontal)
                {
                    NextRenderSize = Next.ArrangedRect.Width;
                }
                else
                {
                    NextRenderSize = Next.ArrangedRect.Height;
                }
            }
        }

        public virtual float PreviousRenderSize { get; protected set; }
        public virtual float NextRenderSize { get; protected set; }
        public virtual Visual? Prev { get; protected set; }
        public virtual Visual? Next { get; protected set; }
        public virtual bool NextIsLastChild { get; protected set; }
        public virtual bool PrevIsLastChild { get; protected set; }
    }
}
