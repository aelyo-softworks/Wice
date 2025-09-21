namespace Wice;

/// <summary>
/// A draggable splitter used within a <see cref="Dock"/> to resize two adjacent docked children.
/// </summary>
public partial class DockSplitter : Visual
{
    /// <summary>
    /// Dynamic property storing the splitter thickness (in DIPs). Changing this requests a new measure pass.
    /// Default value is 5.
    /// </summary>
    public static VisualProperty SizeProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Size), VisualPropertyInvalidateModes.Measure, 5f);

    /// <summary>
    /// Raised when a drag operation is committed (typically on left mouse button release).
    /// </summary>
    public event EventHandler? Commit;

    /// <summary>
    /// Raised when a drag operation is canceled (e.g., by pressing Escape).
    /// </summary>
    public event EventHandler? Cancel;

    private Dock? ParentDock => Parent as Dock;

    /// <summary>
    /// Gets the resolved docking side for this splitter relative to its siblings.
    /// Determined when the splitter is attached to its parent.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual DockType DockType { get; protected set; }

    /// <summary>
    /// Gets the resize orientation controlled by this splitter:
    /// - <see cref="Orientation.Horizontal"/> means it resizes widths (vertical splitter bar).
    /// - <see cref="Orientation.Vertical"/> means it resizes heights (horizontal splitter bar).
    /// Determined when attached to the parent dock.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual Orientation Orientation { get; protected set; }

    /// <summary>
    /// Gets or sets an extra tolerance (in DIPs) applied around the visual bounds for hit testing to make the splitter easier to grab.
    /// If 0, the default bounds are used.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual float HitTestTolerance { get; set; }

    /// <summary>
    /// Gets or sets the splitter thickness (in DIPs).
    /// Applied to <see cref="Visual.Width"/> for horizontal orientation (vertical bar), or to
    /// <see cref="Visual.Height"/> for vertical orientation (horizontal bar). The value is clamped to at least 1 DIP.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual float Size { get => (float)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    /// <summary>
    /// Creates the drag state storing initial sizes and neighbor visuals.
    /// </summary>
    /// <param name="e">Mouse button event that initiated the drag.</param>
    /// <returns>A <see cref="SplitDragState"/> bound to this splitter.</returns>
    protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);

    /// <summary>
    /// Invokes the <see cref="Commit"/> event.
    /// </summary>
    protected virtual void OnCommit(object sender, EventArgs e) => Commit?.Invoke(sender, e);

    /// <summary>
    /// Invokes the <see cref="Cancel"/> event.
    /// </summary>
    protected virtual void OnCancel(object sender, EventArgs e) => Cancel?.Invoke(sender, e);

    /// <summary>
    /// Called when the visual is attached to a parent. Determines <see cref="DockType"/>, <see cref="Orientation"/>,
    /// updates the cursor, sets the splitter thickness on <see cref="Visual.Width"/> or <see cref="Visual.Height"/>,
    /// and assigns the dock type on this visual.
    /// </summary>
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

    /// <summary>
    /// Returns the area considered for hit testing. Inflates the default bounds by <see cref="HitTestTolerance"/> when set.
    /// </summary>
    /// <param name="defaultBounds">The base hit-test rectangle computed by the framework.</param>
    /// <returns>The possibly inflated rectangle used for hit testing.</returns>
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

    /// <summary>
    /// Drag callback invoked during a mouse move with capture.
    /// </summary>
    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        OnMouseDrag(e);
        base.OnMouseDrag(sender, e);
    }

    /// <summary>
    /// Performs the resize logic for the dock neighbors during a drag operation.
    /// Honors min/max constraints of both sides and avoids directly modifying the size
    /// of a last child when <see cref="Dock.LastChildFill"/> is active.
    /// </summary>
    /// <param name="e">Drag event containing delta and state.</param>
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
    /// Commits the current drag operation when the left mouse button is released.
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
    /// Cancels the current drag operation, restoring the original sizes of the adjacent docked visuals,
    /// and raises <see cref="Cancel"/>.
    /// </summary>
    /// <param name="e">Event context (key/mouse).</param>
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

    /// <summary>
    /// Handles Escape to cancel the drag operation.
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
    /// Gets the previous sibling docked on the appropriate side relative to this splitter.
    /// </summary>
    protected Visual? GetPrevVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Left : DockType.Top);

    /// <summary>
    /// Gets the next sibling docked on the appropriate side relative to this splitter.
    /// </summary>
    protected Visual? GetNextVisual() => ParentDock?.GetAt(this, Orientation == Orientation.Horizontal ? DockType.Right : DockType.Bottom);

    /// <summary>
    /// Drag state capturing the neighbors and their initial render sizes for a splitter drag operation.
    /// </summary>
    public class SplitDragState : DragState
    {
        /// <summary>
        /// Initializes a new drag state for a <see cref="DockSplitter"/>, capturing neighbors and their sizes.
        /// </summary>
        /// <param name="visual">The splitter being dragged.</param>
        /// <param name="e">The initiating mouse event.</param>
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

        /// <summary>
        /// Gets the previous visual's render size along the controlled axis at drag start
        /// (width for horizontal orientation, height for vertical orientation).
        /// </summary>
        public virtual float PreviousRenderSize { get; protected set; }

        /// <summary>
        /// Gets the next visual's render size along the controlled axis at drag start
        /// (width for horizontal orientation, height for vertical orientation).
        /// </summary>
        public virtual float NextRenderSize { get; protected set; }

        /// <summary>
        /// Gets the previous sibling visual relative to the splitter.
        /// </summary>
        public virtual Visual? Prev { get; protected set; }

        /// <summary>
        /// Gets the next sibling visual relative to the splitter.
        /// </summary>
        public virtual Visual? Next { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the next sibling is the dock's last child while <see cref="Dock.LastChildFill"/> is active.
        /// When true, the next visual's size is not set directly by the drag logic.
        /// </summary>
        public virtual bool NextIsLastChild { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the previous sibling is the dock's last child while <see cref="Dock.LastChildFill"/> is active.
        /// When true, the previous visual's size is not set directly by the drag logic.
        /// </summary>
        public virtual bool PrevIsLastChild { get; protected set; }
    }
}
