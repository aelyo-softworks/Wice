namespace Wice;

/// <summary>
/// A scrollable container that hosts a single child inside a <see cref="Viewer"/> and manages
/// vertical and horizontal scrolling via embedded <see cref="ScrollBar"/>s.
/// </summary>
public partial class ScrollViewer : Dock, IOneChildParent, IViewerParent, IDisposable
{
    // values from https://github.com/wine-mirror/wine/blob/master/dlls/user32/scroll.c
    // don't know where to get that from Windows api?
    private const int _repeatPeriod = 50;
    private const int _repeatDueTime = 200;

    /// <summary>
    /// Dependency property controlling the vertical scrollbar visibility policy.
    /// Default is <see cref="ScrollBarVisibility.Auto"/>.
    /// </summary>
    public static VisualProperty VerticalScrollBarVisibilityProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalScrollBarVisibility), VisualPropertyInvalidateModes.Measure, ScrollBarVisibility.Auto);

    /// <summary>
    /// Dependency property controlling the horizontal scrollbar visibility policy.
    /// Default is <see cref="ScrollBarVisibility.Disabled"/>.
    /// </summary>
    public static VisualProperty HorizontalScrollBarVisibilityProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalScrollBarVisibility), VisualPropertyInvalidateModes.Measure, ScrollBarVisibility.Disabled); // note it's not auto

    /// <summary>
    /// Dependency property selecting the scrolling layout mode.
    /// </summary>
    public static VisualProperty ScrollModeProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(ScrollMode), VisualPropertyInvalidateModes.Arrange, ScrollViewerMode.Dock);

    /// <summary>
    /// Dependency property storing the current vertical scroll offset (in DIPs).
    /// Value is clamped to [0, <see cref="VerticalMaxOffset"/>].
    /// </summary>
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalOffset), VisualPropertyInvalidateModes.Arrange, 0f, ConvertVerticalOffset);

    /// <summary>
    /// Dependency property storing the current horizontal scroll offset (in DIPs).
    /// Value is clamped to [0, <see cref="HorizontalMaxOffset"/>].
    /// </summary>
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Arrange, 0f, ConvertHorizontalOffset);

    /// <summary>
    /// Dependency property defining the unit used by line up/down (vertical) and by wheel scrolling.
    /// Default is 1 DIP.
    /// </summary>
    public static VisualProperty VerticalLineSizeProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalLineSize), VisualPropertyInvalidateModes.Arrange, 1f);

    /// <summary>
    /// Dependency property defining the unit used by line left/right (horizontal).
    /// Default is 1 DIP.
    /// </summary>
    public static VisualProperty HorizontalLineSizeProperty { get; } = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalLineSize), VisualPropertyInvalidateModes.Arrange, 1f);

    private bool _isVerticalScrollBarVisible;
    private bool _isHorizontalScrollBarVisible;
    private Timer? _timer;
    private float _verticalOffsetStart;
    private float _horizontalOffsetStart;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollViewer"/> class.
    /// </summary>
    public ScrollViewer()
    {
        VerticalScrollBar = CreateVerticalScrollBar();
        if (VerticalScrollBar == null)
            throw new InvalidOperationException();

        VerticalScrollBar.SmallDecreaseClick += OnVerticalSmallDecreaseClick;
        VerticalScrollBar.SmallIncreaseClick += OnVerticalSmallIncreaseClick;
        VerticalScrollBar.LargeDecreaseClick += OnVerticalLargeDecreaseClick;
        VerticalScrollBar.LargeIncreaseClick += OnVerticalLargeIncreaseClick;
        VerticalScrollBar.FocusIndex = 1;
        VerticalScrollBar.IsVisible = false;
#if DEBUG
        VerticalScrollBar.Name = nameof(VerticalScrollBar);
#endif
        VerticalScrollBar.Thumb.DragDelta += OnVerticalScrollBarThumbDragDelta;
        VerticalScrollBar.Thumb.DragStarted += (s, e) => { _verticalOffsetStart = VerticalOffset; };
        Children.Add(VerticalScrollBar);

        HorizontalScrollBar = CreateHorizontalScrollBar();
        if (HorizontalScrollBar == null)
            throw new InvalidOperationException();

        HorizontalScrollBar.SmallDecreaseClick += OnHorizontalSmallDecreaseClick;
        HorizontalScrollBar.SmallIncreaseClick += OnHorizontalSmallIncreaseClick;
        HorizontalScrollBar.LargeDecreaseClick += OnHorizontalLargeDecreaseClick;
        HorizontalScrollBar.LargeIncreaseClick += OnHorizontalLargeIncreaseClick;
        HorizontalScrollBar.FocusIndex = 2;
        HorizontalScrollBar.IsVisible = false;
#if DEBUG
        HorizontalScrollBar.Name = nameof(HorizontalScrollBar);
#endif
        HorizontalScrollBar.Thumb.DragDelta += OnHorizontalScrollBarThumbDragDelta;
        HorizontalScrollBar.Thumb.DragStarted += (s, e) => { _horizontalOffsetStart = HorizontalOffset; };
        Children.Add(HorizontalScrollBar);

        // viewer is the last (fill)
        Viewer = CreateViewer();
        if (Viewer == null)
            throw new InvalidOperationException();

        Viewer.FocusIndex = 0;
        Viewer.Arranged += OnViewerArranged;
        Viewer.IsWidthUnconstrained = false; // match HorizontalScrollbarVisibility set to disabled by default

        // to ensure scrollbars are above viewer (& child)
        Viewer.ZIndex = int.MinValue;
#if DEBUG
        Viewer.Name = nameof(Viewer);
#endif
        Children.Add(Viewer);
    }

    /// <summary>
    /// Gets or sets the single content <see cref="Visual"/> hosted by the inner <see cref="Viewer"/>.
    /// </summary>
    [Browsable(false)]
    public Visual? Child { get => Viewer.Child; set => Viewer.Child = value; }

    /// <summary>
    /// Gets the <see cref="Viewer"/> instance associated with this object.
    /// </summary>
    [Browsable(false)]
    public Viewer Viewer { get; }

    /// <summary>
    /// Gets the horizontal scrollbar instance managed by this viewer.
    /// </summary>
    [Browsable(false)]
    public ScrollBar HorizontalScrollBar { get; }

    /// <summary>
    /// Gets the vertical scrollbar instance managed by this viewer.
    /// </summary>
    [Browsable(false)]
    public ScrollBar VerticalScrollBar { get; }

    /// <summary>
    /// Gets the current maximum vertical scroll offset in DIPs.
    /// Computed as <c>childHeight - viewportHeight</c>; 0 when content fits.
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalMaxOffset { get; private set; }

    /// <summary>
    /// Gets the current maximum horizontal scroll offset in DIPs.
    /// Computed as <c>childWidth - viewportWidth</c>; 0 when content fits.
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalMaxOffset { get; private set; }

    /// <summary>
    /// Gets the ratio used to translate horizontal thumb movement to content offset.
    /// Equals <c>childWidth / viewportWidth</c>; returns 1 when unavailable or zero-sized.
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalRatio
    {
        get
        {
            var child = Child;
            if (child == null)
                return 1;

            var vrc = Viewer.ArrangedRect;
            if (vrc.Width == 0)
                return 1;

            var crc = child.ArrangedRect;
            return crc.Width / vrc.Width;
        }
    }

    /// <summary>
    /// Gets the ratio used to translate vertical thumb movement to content offset.
    /// Equals <c>childHeight / viewportHeight</c>; returns 1 when unavailable or zero-sized.
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalRatio
    {
        get
        {
            var child = Child;
            if (child == null)
                return 1;

            var vrc = Viewer.ArrangedRect;
            if (vrc.Width == 0)
                return 1;

            var crc = child.ArrangedRect;
            return crc.Height / vrc.Height;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the vertical scrollbar is currently visible
    /// (after applying the visibility policy and content/viewport sizes).
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsVerticalScrollBarVisible
    {
        get => _isVerticalScrollBarVisible;
        private set
        {
            if (_isVerticalScrollBarVisible == value)
                return;

            _isVerticalScrollBarVisible = value;
            VerticalScrollBar.IsVisible = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the horizontal scrollbar is currently visible
    /// (after applying the visibility policy and content/viewport sizes).
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsHorizontalScrollBarVisible
    {
        get => _isHorizontalScrollBarVisible;
        private set
        {
            if (_isHorizontalScrollBarVisible == value)
                return;

            _isHorizontalScrollBarVisible = value;
            HorizontalScrollBar.IsVisible = value;
        }
    }

    /// <summary>
    /// Gets or sets the vertical scrollbar visibility policy.
    /// When set to Disabled, the viewer constrains height and vertical scrolling is suppressed.
    /// </summary>
    [Category(CategoryBehavior)]
    public ScrollBarVisibility VerticalScrollBarVisibility { get => (ScrollBarVisibility)GetPropertyValue(VerticalScrollBarVisibilityProperty)!; set => SetPropertyValue(VerticalScrollBarVisibilityProperty, value); }

    /// <summary>
    /// Gets or sets the horizontal scrollbar visibility policy.
    /// When set to Disabled, the viewer constrains width and horizontal scrolling is suppressed.
    /// </summary>
    [Category(CategoryBehavior)]
    public ScrollBarVisibility HorizontalScrollBarVisibility { get => (ScrollBarVisibility)GetPropertyValue(HorizontalScrollBarVisibilityProperty)!; set => SetPropertyValue(HorizontalScrollBarVisibilityProperty, value); }

    /// <summary>
    /// Gets or sets the composition mode used to arrange scrollbars relative to the content.
    /// </summary>
    [Category(CategoryBehavior)]
    public ScrollViewerMode ScrollMode { get => (ScrollViewerMode)GetPropertyValue(ScrollModeProperty)!; set => SetPropertyValue(ScrollModeProperty, value); }

    /// <summary>
    /// Gets or sets the current vertical scroll offset (DIPs). Clamped to [0, <see cref="VerticalMaxOffset"/>].
    /// Assigning updates <see cref="Viewer.ChildOffsetTop"/> when the vertical scrollbar is visible.
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty)!; set => SetPropertyValue(VerticalOffsetProperty, value); }

    /// <summary>
    /// Gets or sets the current horizontal scroll offset (DIPs). Clamped to [0, <see cref="HorizontalMaxOffset"/>].
    /// Assigning updates <see cref="Viewer.ChildOffsetLeft"/> when the horizontal scrollbar is visible.
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty)!; set => SetPropertyValue(HorizontalOffsetProperty, value); }

    /// <summary>
    /// Gets or sets the unit used by vertical line scrolling (buttons/wheel).
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalLineSize { get => (float)GetPropertyValue(VerticalLineSizeProperty)!; set => SetPropertyValue(VerticalLineSizeProperty, value); }

    /// <summary>
    /// Gets or sets the unit used by horizontal line scrolling (buttons/wheel with Control).
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalLineSize { get => (float)GetPropertyValue(HorizontalLineSizeProperty)!; set => SetPropertyValue(HorizontalLineSizeProperty, value); }

    /// <summary>
    /// Creates the vertical scrollbar instance. Override to provide a customized control.
    /// </summary>
    /// <returns>A new <see cref="ScrollBar"/> configured for vertical orientation.</returns>
    protected virtual ScrollBar CreateVerticalScrollBar() => new VerticalScrollBar();

    /// <summary>
    /// Creates the horizontal scrollbar instance. Override to provide a customized control.
    /// </summary>
    /// <returns>A new <see cref="ScrollBar"/> configured for horizontal orientation.</returns>
    protected virtual ScrollBar CreateHorizontalScrollBar() => new HorizontalScrollBar();

    /// <summary>
    /// Creates the inner <see cref="Viewer"/> that hosts the single child visual.
    /// Override to provide a customized viewer.
    /// </summary>
    protected virtual Viewer CreateViewer() => new();

    private static object? ConvertVerticalOffset(BaseObject obj, object? value)
    {
        var f = (float)value!;
        if (f.IsInvalid())
            return 0f;

        var sv = (ScrollViewer)obj;
        var max = sv.VerticalMaxOffset;
        if (f > max)
        {
            f = max;
        }
        else if (f < 0)
        {
            f = 0;
        }
        return f;
    }

    private static object? ConvertHorizontalOffset(BaseObject obj, object? value)
    {
        var f = (float)value!;
        if (f.IsInvalid())
            return 0f;

        var sv = (ScrollViewer)obj;
        var max = sv.HorizontalMaxOffset;
        if (f > max)
        {
            f = max;
        }
        else if (f < 0)
        {
            f = 0;
        }
        return f;
    }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => ScrollMode switch
    {
        ScrollViewerMode.Overlay => Canvas.MeasureCore(this, constraint, DimensionOptions.WidthAndHeight),
        ScrollViewerMode.Dock => base.MeasureCore(constraint),
        _ => throw new NotSupportedException(),
    };

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var window = Window;
        if (window == null)
            return;

        // scrollviewer has lots of internal children that can trigger invalidations
        // to avoid oscillations, we need to suspend invalidations
        window.WithInvalidationsProcessingSuspended(() =>
        {
            switch (ScrollMode)
            {
                case ScrollViewerMode.Overlay:
                    Canvas.ArrangeCore(this, finalRect);
                    break;

                case ScrollViewerMode.Dock:
                    base.ArrangeCore(finalRect);
                    break;

                default:
                    throw new NotSupportedException();
            }
        });
    }

    private void OnVerticalScrollBarThumbDragDelta(object? sender, DragEventArgs e)
    {
        var ratio = VerticalRatio;
        VerticalOffset = _verticalOffsetStart + ratio * e.State.DeltaY;
    }

    private void OnHorizontalScrollBarThumbDragDelta(object? sender, DragEventArgs e)
    {
        var ratio = HorizontalRatio;
        HorizontalOffset = _horizontalOffsetStart + ratio * e.State.DeltaX;
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        var ret = base.SetPropertyValue(property, value, options);

        if (IsHorizontalScrollBarVisible && property == HorizontalOffsetProperty)
        {
            var offset = Viewer.BaseChildOffsetLeft + HorizontalOffset;
            Viewer.ChildOffsetLeft = -offset;
        }

        if (IsVerticalScrollBarVisible && property == VerticalOffsetProperty)
        {
            var offset = Viewer.BaseChildOffsetTop + VerticalOffset;
            Viewer.ChildOffsetTop = -offset;
        }

        if (!ret)
            return false;

        if (property == VerticalScrollBarVisibilityProperty)
        {
            Viewer.IsHeightUnconstrained = !value!.Equals(ScrollBarVisibility.Disabled);
        }
        else if (property == HorizontalScrollBarVisibilityProperty)
        {
            Viewer.IsWidthUnconstrained = !value!.Equals(ScrollBarVisibility.Disabled);
        }
        return true;
    }

    private void OnViewerArranged(object? sender, EventArgs e)
    {
        var child = Child;
        if (child == null)
            return;

        var viewerRect = Viewer.ArrangedRect;
        var childRect = child.ArrangedRect;
        var viewerSize = viewerRect.Size;
        var childSize = childRect.Size;

        var vv = VerticalScrollBarVisibility;
        var hv = HorizontalScrollBarVisibility;

        if (viewerSize.IsZero)
        {
            // this causes oscillation
            //IsVerticalScrollBarVisible = false;
            //IsHorizontalScrollBarVisible = false;
            return;
        }

        switch (vv)
        {
            case ScrollBarVisibility.Visible:
                IsVerticalScrollBarVisible = true;
                break;

            case ScrollBarVisibility.Hidden:
                IsVerticalScrollBarVisible = false;
                break;

            case ScrollBarVisibility.Disabled:
                break;

            case ScrollBarVisibility.Auto:
                if (!IsVerticalScrollBarVisible)
                {
                    if (viewerSize.height < childSize.height)
                    {
                        IsVerticalScrollBarVisible = true;
                    }
                }
                else
                {
                    if (viewerSize.height > childSize.height)
                    {
                        IsVerticalScrollBarVisible = false;
                    }
                }
                break;
        }

        switch (hv)
        {
            case ScrollBarVisibility.Visible:
                IsHorizontalScrollBarVisible = true;
                break;

            case ScrollBarVisibility.Hidden:
                IsHorizontalScrollBarVisible = false;
                break;

            case ScrollBarVisibility.Disabled:
                break;

            case ScrollBarVisibility.Auto:
                if (!IsHorizontalScrollBarVisible)
                {
                    if (viewerSize.width < childSize.width)
                    {
                        IsHorizontalScrollBarVisible = true;
                    }
                }
                else
                {
                    if (viewerSize.width > childSize.width)
                    {
                        IsHorizontalScrollBarVisible = false;
                    }
                }
                break;
        }

        if (IsVerticalScrollBarVisible)
        {
            if (viewerSize.height != 0 && childSize.height != 0)
            {
                var thumbPlaceHolderSize = viewerSize.height - VerticalScrollBar.SmallDecrease.Height - VerticalScrollBar.SmallIncrease.Height;
                if (IsHorizontalScrollBarVisible)
                {
                    thumbPlaceHolderSize -= HorizontalScrollBar.Height;
                }

                if (thumbPlaceHolderSize > 0)
                {
                    var ratio = childSize.height / thumbPlaceHolderSize; // > 1
                    var thumbSize = viewerSize.height / ratio;

                    // ensure thumb size has a minimum (or is invisible)
                    var minSize = VerticalScrollBar.Width;
                    var fix = 0f;
                    if (thumbSize < minSize)
                    {
                        if (thumbPlaceHolderSize >= minSize)
                        {
                            fix = (minSize - thumbSize) / 2; // remove to large decrease & large increase
                            thumbSize = minSize;
                        }
                        else
                        {
                            // thumb is invisible
                            thumbSize = 0;
                        }
                    }

                    VerticalMaxOffset = childSize.height - viewerSize.height;

                    var offset = -childRect.top;
                    offset = Math.Max(0, offset);
                    offset = Math.Min(VerticalMaxOffset, offset);
                    VerticalOffset = offset;

                    VerticalScrollBar.LargeDecrease.Height = Math.Max(0, Math.Min(offset / ratio - fix, thumbPlaceHolderSize - minSize));
                    VerticalScrollBar.LargeIncrease.Height = Math.Max(0, thumbPlaceHolderSize - thumbSize - VerticalScrollBar.LargeDecrease.Height);
                }
            }
        }
        else
        {
            Viewer.ChildOffsetTop = 0;
        }

        if (IsHorizontalScrollBarVisible)
        {
            if (viewerSize.width != 0 && childSize.width != 0)
            {
                var thumbPlaceHolderSize = viewerSize.width - HorizontalScrollBar.SmallDecrease.Width - HorizontalScrollBar.SmallIncrease.Width;
                if (IsVerticalScrollBarVisible)
                {
                    thumbPlaceHolderSize -= VerticalScrollBar.Width;
                }

                if (thumbPlaceHolderSize > 0)
                {
                    var ratio = childSize.width / thumbPlaceHolderSize; // > 1
                    var thumbSize = viewerSize.width / ratio;

                    // ensure thumb size has a minimum (or is invisible)
                    var minSize = HorizontalScrollBar.Height;
                    var fix = 0f;
                    if (thumbSize < minSize)
                    {
                        if (thumbPlaceHolderSize >= minSize)
                        {
                            fix = (minSize - thumbSize) / 2; // remove to large decrease & large increase
                            thumbSize = minSize;
                        }
                        else
                        {
                            // thumb is invisible
                            thumbSize = 0;
                        }
                    }

                    HorizontalMaxOffset = childSize.width - viewerSize.width;

                    var offset = -childRect.left;
                    offset = Math.Max(0, offset);
                    offset = Math.Min(HorizontalMaxOffset, offset);
                    HorizontalOffset = offset;

                    HorizontalScrollBar.LargeDecrease.Width = Math.Max(0, Math.Min(offset / ratio - fix, thumbPlaceHolderSize - minSize));
                    HorizontalScrollBar.LargeIncrease.Width = Math.Max(0, thumbPlaceHolderSize - thumbSize - HorizontalScrollBar.LargeDecrease.Width);
                }
            }
        }
        else
        {
            Viewer.ChildOffsetLeft = 0;
        }

        VerticalScrollBar.UpdateCorner(this);
        HorizontalScrollBar.UpdateCorner(this);
    }

    /// <summary>
    /// Handles the "small increase" click (line down) on the vertical scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnVerticalSmallIncreaseClick(object? sender, EventArgs e) => DoAction(LineDown, e);

    /// <summary>
    /// Handles the "small decrease" click (line up) on the vertical scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnVerticalSmallDecreaseClick(object? sender, EventArgs e) => DoAction(LineUp, e);

    /// <summary>
    /// Handles the "large increase" click (page down) on the vertical scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnVerticalLargeIncreaseClick(object? sender, EventArgs e) => DoAction(PageDown, e);

    /// <summary>
    /// Handles the "large decrease" click (page up) on the vertical scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnVerticalLargeDecreaseClick(object? sender, EventArgs e) => DoAction(PageUp, e);

    /// <summary>
    /// Handles the "small increase" click (line right) on the horizontal scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnHorizontalSmallIncreaseClick(object? sender, EventArgs e) => DoAction(LineRight, e);

    /// <summary>
    /// Handles the "small decrease" click (line left) on the horizontal scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnHorizontalSmallDecreaseClick(object? sender, EventArgs e) => DoAction(LineLeft, e);

    /// <summary>
    /// Handles the "large increase" click (page right) on the horizontal scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnHorizontalLargeIncreaseClick(object? sender, EventArgs e) => DoAction(PageRight, e);

    /// <summary>
    /// Handles the "large decrease" click (page left) on the horizontal scrollbar.
    /// Performs the action once and, when triggered by mouse, starts a repeat timer.
    /// </summary>
    protected virtual void OnHorizontalLargeDecreaseClick(object? sender, EventArgs e) => DoAction(PageLeft, e);

    /// <inheritdoc/>
    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Window.ReleaseMouseCapture();
            Interlocked.Exchange(ref _timer, null)?.SafeDispose();
        }
        base.OnMouseButtonUp(sender, e);
    }

    private void DoAction(Action action, EventArgs e)
    {
        action();
        if (e is MouseEventArgs)
        {
#if DEBUG
            if (Application.IsDebuggerAttached)
#endif
            {
                if (Window == null)
                    return;

                CaptureMouse();
                _timer ??= new Timer((state) => Window.RunTaskOnMainThread(action), null, _repeatDueTime, _repeatPeriod);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        float offset;
        if (NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT))
        {
            offset = e.Delta * VerticalScrollBar.Thumb.ArrangedRect.Height;
        }
        else
        {
            offset = e.Delta * VerticalLineSize;
        }
        if (offset == 0)
            return;

        if (NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL))
        {
            HorizontalOffset -= offset;
        }
        else
        {
            VerticalOffset -= offset;
        }
        e.Handled = true;
        base.OnMouseWheel(sender, e);
    }

    // https://docs.microsoft.com/en-us/windows/win32/controls/about-scroll-bars

    /// <summary>
    /// Scrolls down by <see cref="VerticalLineSize"/>.
    /// </summary>
    public virtual void LineDown() => VerticalOffset += VerticalLineSize;

    /// <summary>
    /// Scrolls up by <see cref="VerticalLineSize"/>.
    /// </summary>
    public virtual void LineUp() => VerticalOffset -= VerticalLineSize;

    /// <summary>
    /// Scrolls left by <see cref="HorizontalLineSize"/>.
    /// </summary>
    public virtual void LineLeft() => HorizontalOffset -= HorizontalLineSize;

    /// <summary>
    /// Scrolls right by <see cref="HorizontalLineSize"/>.
    /// </summary>
    public virtual void LineRight() => HorizontalOffset += HorizontalLineSize;

    /// <summary>
    /// Scrolls down by one page, using the current vertical thumb height as the page size.
    /// </summary>
    public virtual void PageDown() => VerticalOffset += VerticalScrollBar.Thumb.ArrangedRect.Height;

    /// <summary>
    /// Scrolls up by one page, using the current vertical thumb height as the page size.
    /// </summary>
    public virtual void PageUp() => VerticalOffset -= VerticalScrollBar.Thumb.ArrangedRect.Height;

    /// <summary>
    /// Scrolls left by one page, using the current horizontal thumb width as the page size.
    /// </summary>
    public virtual void PageLeft() => HorizontalOffset -= HorizontalScrollBar.Thumb.ArrangedRect.Width;

    /// <summary>
    /// Scrolls right by one page, using the current horizontal thumb width as the page size.
    /// </summary>
    public virtual void PageRight() => HorizontalOffset += HorizontalScrollBar.Thumb.ArrangedRect.Width;

    /// <summary>
    /// Disposes managed resources (auto-repeat timer). Safe to call multiple times.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Releases resources used by the <see cref="ScrollViewer"/>.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _timer, null)?.SafeDispose();
            }

            _disposedValue = true;
        }
    }
}
