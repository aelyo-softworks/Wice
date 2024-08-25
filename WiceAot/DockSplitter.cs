namespace Wice;

public class DockSplitter : Visual
{
    public static VisualProperty SizeProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Size), VisualPropertyInvalidateModes.Measure, 5f);

    public DockSplitter()
    {
    }

    private Dock? ParentDock => Parent as Dock;

    [Category(CategoryBehavior)]
    public DockType DockType { get; private set; }

    [Category(CategoryBehavior)]
    public Orientation Orientation { get; private set; }

    [Category(CategoryBehavior)]
    public float Size { get => (float)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        var parent = Parent;
        var index = parent.Children.IndexOf(this);
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

    protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);

    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        OnMouseDrag(e);
        base.OnMouseDrag(sender, e);
    }

    private void OnMouseDrag(DragEventArgs e)
    {
        var state = (SplitDragState)e.State;
        if (state._prev == null || state._next == null)
            return;

        var delta = Orientation == Orientation.Horizontal ? state.DeltaX : state.DeltaY;
        if (delta != 0)
        {
            var oldSize = state._nextRenderSize + state._previousRenderSize;
            var newPrevSize = Math.Min(Math.Max(0, state._previousRenderSize + delta), oldSize);

            if (Orientation == Orientation.Horizontal)
            {
                if (state._prev.MaxWidth.IsSet() && state._prev.MaxWidth > 0)
                {
                    newPrevSize = Math.Min(newPrevSize, state._prev.MaxWidth);
                }

                if (state._prev.MinWidth.IsSet() && state._prev.MinWidth > 0)
                {
                    newPrevSize = Math.Max(newPrevSize, state._prev.MinWidth);
                }

                var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
                var newNextSize = plannedNextSize;
                if (state._next.MaxWidth.IsSet() && state._next.MaxWidth > 0)
                {
                    newNextSize = Math.Min(newNextSize, state._next.MaxWidth);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (state._next.MinWidth.IsSet() && state._next.MinWidth > 0)
                {
                    newNextSize = Math.Max(newNextSize, state._next.MinWidth);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (!state._prevIsLastChild)
                {
                    state._prev.Width = newPrevSize;
                }

                if (!state._nextIsLastChild)
                {
                    state._next.Width = newNextSize;
                }
            }
            else
            {
                if (state._prev.MaxHeight.IsSet() && state._prev.MaxHeight > 0)
                {
                    newPrevSize = Math.Min(newPrevSize, state._prev.MaxHeight);
                }

                if (state._prev.MinHeight.IsSet() && state._prev.MinHeight > 0)
                {
                    newPrevSize = Math.Max(newPrevSize, state._prev.MinHeight);
                }

                var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
                var newNextSize = plannedNextSize;
                if (state._next.MaxHeight.IsSet() && state._next.MaxHeight > 0)
                {
                    newNextSize = Math.Min(newNextSize, state._next.MaxHeight);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (state._next.MinHeight.IsSet() && state._next.MinHeight > 0)
                {
                    newNextSize = Math.Max(newNextSize, state._next.MinHeight);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (!state._prevIsLastChild)
                {
                    state._prev.Height = newPrevSize;
                }

                if (!state._nextIsLastChild)
                {
                    state._next.Height = newNextSize;
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

    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == VIRTUAL_KEY.VK_ESCAPE)
        {
            if (CancelDragMove(e) is SplitDragState state)
            {
                if (state._prev != null)
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        state._prev.Width = state._previousRenderSize;
                    }
                    else
                    {
                        state._prev.Height = state._previousRenderSize;
                    }
                }

                if (state._next != null)
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        state._next.Width = state._nextRenderSize;
                    }
                    else
                    {
                        state._next.Height = state._nextRenderSize;
                    }
                }
            }
        }
        base.OnKeyDown(sender, e);
    }

    private Visual? GetPrevVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Left : DockType.Top);
    private Visual? GetNextVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Right : DockType.Bottom);

    private sealed class SplitDragState : DragState
    {
        public float _previousRenderSize;
        public float _nextRenderSize;
        public Visual? _prev;
        public Visual? _next;
        public bool _nextIsLastChild;
        public bool _prevIsLastChild;

        public SplitDragState(DockSplitter visual, MouseButtonEventArgs e)
            : base(visual, e)
        {
            var lastChildFill = visual.ParentDock?.LastChildFill == true;

            _prev = visual.GetPrevVisual();
            if (_prev != null)
            {
                _prevIsLastChild = lastChildFill && _prev == visual.ParentDock?._lastChild;
                if (visual.Orientation == Orientation.Horizontal)
                {
                    _previousRenderSize = _prev.ArrangedRect.Width;
                }
                else
                {
                    _previousRenderSize = _prev.ArrangedRect.Height;
                }
            }

            _next = visual.GetNextVisual();
            if (_next != null)
            {
                _nextIsLastChild = lastChildFill && _next == visual.ParentDock?._lastChild;
                if (visual.Orientation == Orientation.Horizontal)
                {
                    _nextRenderSize = _next.ArrangedRect.Width;
                }
                else
                {
                    _nextRenderSize = _next.ArrangedRect.Height;
                }
            }
        }
    }
}
