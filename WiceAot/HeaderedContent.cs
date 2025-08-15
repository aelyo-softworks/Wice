namespace Wice;

/// <summary>
/// A stack-based container composed of a <see cref="Header"/> and a scrollable <see cref="ScrollViewer"/>.
/// Toggles the visibility of the content area by animating the <see cref="Viewer"/>'s height when
/// the header selection changes.
/// </summary>
/// <remarks>
/// Behavior:
/// - Hosts exactly two children: <see cref="Header"/> and <see cref="Viewer"/> (in that order).
/// - The single logical content is exposed as <see cref="Child"/> and is forwarded to <see cref="Viewer.Child"/>.
/// - When <see cref="Header.IsSelected"/> changes, the control animates <see cref="Viewer.Height"/> from/to 0
///   using a <see cref="TimerStoryboard"/> and a <see cref="SinglePropertyAnimation"/> with the current theme's
///   <c>SelectedAnimationDuration</c>.
/// - The expansion height is <see cref="OpenHeight"/> when provided; otherwise the measured
///   <see cref="Visual.DesiredSize"/> of the content (height).
/// </remarks>
public partial class HeaderedContent : Stack, IOneChildParent, IDisposable
{
    /// <summary>
    /// Reusable storyboard driving the height animation of <see cref="Viewer"/>.
    /// Created lazily when a <see cref="Window"/> is available.
    /// </summary>
    private TimerStoryboard? _sb;

    /// <summary>
    /// Initializes a new instance of <see cref="HeaderedContent"/>.
    /// </summary>
    /// <remarks>
    /// - Creates the <see cref="Header"/> via <see cref="CreateHeader"/> and adds it to <see cref="Visual.Children"/>.
    /// - Subscribes to <c>Header.IsSelectedChanged</c> to animate content open/close.
    /// - Creates the <see cref="Viewer"/> via <see cref="CreateViewer"/> (default height = 0) and adds it to children.
    /// - In DEBUG builds, assigns <see cref="BaseObject.Name"/> defaults for easier diagnostics.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when header or viewer creation returns null.</exception>
    public HeaderedContent()
    {
        Header = CreateHeader();
        if (Header == null)
            throw new InvalidOperationException();

#if DEBUG
        Header.Name ??= nameof(Header);
#endif
        Children.Add(Header);
        Header.IsSelectedChanged += (s, e) => OnHeaderIsSelectedChanged(e.Value);

        Viewer = CreateViewer();
        if (Viewer == null)
            throw new InvalidOperationException();

#if DEBUG
        Viewer.Name ??= nameof(Viewer);
#endif
        Children.Add(Viewer);
    }

    /// <summary>
    /// Gets the header element that drives the open/close state via its selection.
    /// </summary>
    [Browsable(false)]
    public Header Header { get; }

    /// <summary>
    /// Gets the scroll viewer that hosts the single content visual and whose height is animated.
    /// </summary>
    [Browsable(false)]
    public ScrollViewer Viewer { get; }

    /// <summary>
    /// Gets or sets the single content visual. This value is forwarded to <see cref="Viewer.Child"/>.
    /// </summary>
    [Browsable(false)]
    public Visual? Child { get => Viewer.Child; set => Viewer.Child = value; }

    /// <summary>
    /// Gets or sets the height to use when the content is opened.
    /// If not set or not valid, falls back to <see cref="Visual.DesiredSize"/> of <see cref="Child"/> (height).
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual float? OpenHeight { get; set; }

    /// <summary>
    /// Factory that creates the header instance. Override to customize header visuals/behavior.
    /// </summary>
    protected virtual Header CreateHeader() => new();

    /// <summary>
    /// Factory that creates the viewer instance. Override to customize viewer configuration.
    /// Default height is 0 so the content starts collapsed.
    /// </summary>
    protected virtual ScrollViewer CreateViewer() => new() { Height = 0 };

    /// <summary>
    /// Optimizes children collection capacity to two entries (header + viewer).
    /// </summary>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(2);

    /// <summary>
    /// Handles the header selection change by animating the content area open/closed.
    /// </summary>
    /// <param name="value">True to open (expand to target height); false to close (collapse to 0).</param>
    /// <remarks>
    /// Algorithm:
    /// 1) If <see cref="Child"/> is null, do nothing.
    /// 2) Determine target size:
    ///    - Use <see cref="OpenHeight"/> when set and valid; else use <c>Child.DesiredSize.height</c>.
    /// 3) Prepare storyboard:
    ///    - Create a <see cref="TimerStoryboard"/> once per <see cref="Window"/>; otherwise stop and clear existing children.
    /// 4) Compute from/to:
    ///    - From uses current <see cref="Viewer.Height"/> when set, otherwise defaults to 0 (opening) or target size (closing).
    ///    - To is 0 for closing, or the target size for opening.
    /// 5) Create a <see cref="SinglePropertyAnimation"/> targeting <see cref="Visual.Height"/> with the theme duration.
    /// 6) Add to storyboard and start.
    /// </remarks>
    protected virtual void OnHeaderIsSelectedChanged(bool value)
    {
        var child = Child;
        if (child == null)
            return;

        float size;
        var height = OpenHeight;
        if (!height.HasValue || !height.Value.IsSet())
        {
            size = child.DesiredSize.height;
        }
        else
        {
            size = height.Value;
        }

        if (_sb == null)
        {
            if (Window != null)
            {
                _sb = new TimerStoryboard(Window);
            }
        }
        else
        {
            _sb.Stop();
            _sb.Children.Clear();
        }

        float to;
        var from = Viewer.Height;
        if (value)
        {
            to = size;
            if (!from.IsSet())
            {
                from = 0;
            }
        }
        else
        {
            to = 0;
            if (!from.IsSet())
            {
                from = size;
            }
        }

        var animation = new SinglePropertyAnimation(new PropertyAnimationArguments(Viewer, HeightProperty, GetWindowTheme().SelectedAnimationDuration), from, to);

        if (_sb != null)
        {
            _sb.Children.Add(animation);
            _sb.Start();
        }
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Performs the actual dispose logic.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>; false when from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sb?.Dispose();
        }
    }
}
