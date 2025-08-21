namespace Wice;

/// <summary>
/// A layout container that arranges its visible children by docking them to its edges:
/// Left, Top, Right, Bottom, with an optional last child filling the remaining space.
/// </summary>
/// <remarks>
/// Behavior overview:
/// - Dock order is the visual order of <see cref="VisibleChildren"/>; each child consumes space from the current
///   layout rectangle according to its <c>DockType</c>.
/// - Horizontal-alignment is honored for Top/Bottom docked children; Vertical-alignment is honored for Left/Right.
/// - When <see cref="AllowOverlap"/> is false, opposing edge docks will not overlap previously docked edges.
/// - When <see cref="LastChildFill"/> is true, the last visible child fills any remaining area.
/// Relationship graph:
/// - During arrange, the container tracks neighbor relationships (left/right/top/bottom) for each child
///   which can be queried via <see cref="GetAt(Visual, DockType)"/>.
/// </remarks>
public partial class Dock : Visual
{
    private readonly Dictionary<Visual, Docked> _docked = [];

    /// <summary>
    /// Attached property used on child visuals to indicate which edge they dock to when placed in a <see cref="Dock"/>.
    /// Default is <see cref="DockType.Left"/>.
    /// </summary>
    public static VisualProperty DockTypeProperty { get; } = VisualProperty.Add(typeof(Dock), nameof(DockType), VisualPropertyInvalidateModes.Measure, DockType.Left);

    /// <summary>
    /// Gets or sets whether the last visible child should fill the remaining space after all other children are arranged.
    /// Default is true.
    /// </summary>
    public static VisualProperty LastChildFillProperty { get; } = VisualProperty.Add(typeof(Dock), nameof(LastChildFill), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// Gets or sets whether opposing docks are allowed to overlap the remaining layout area.
    /// When false (default), the container prevents overlap by clamping against consumed edges.
    /// </summary>
    public static VisualProperty AllowOverlapProperty { get; } = VisualProperty.Add(typeof(Dock), nameof(AllowOverlap), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Maps a <see cref="DockType"/> to its primary <see cref="Orientation"/>.
    /// </summary>
    /// <param name="type">Dock side.</param>
    /// <returns>Vertical for Top/Bottom, otherwise Horizontal.</returns>
    public static Orientation GetOrientation(DockType type) => type == DockType.Bottom || type == DockType.Top ? Orientation.Vertical : Orientation.Horizontal;

    /// <summary>
    /// Sets the attached <see cref="DockType"/> on a child.
    /// </summary>
    /// <param name="properties">Target child implementing <see cref="IPropertyOwner"/>.</param>
    /// <param name="type">Dock side to set.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="properties"/> is null.</exception>
    public static void SetDockType(IPropertyOwner properties, DockType type)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        properties.SetPropertyValue(DockTypeProperty, type);
    }

    /// <summary>
    /// Gets the attached <see cref="DockType"/> from a child.
    /// </summary>
    /// <param name="properties">Target child implementing <see cref="IPropertyOwner"/>.</param>
    /// <returns>The configured dock side; defaults to <see cref="DockType.Left"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="properties"/> is null.</exception>
    public static DockType GetDockType(IPropertyOwner properties)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        return (DockType)properties.GetPropertyValue(DockTypeProperty)!;
    }

    internal Visual? _lastChild;

    /// <summary>
    /// When true, the last visible child fills the remaining space; otherwise it is arranged like others.
    /// </summary>
    [Category(CategoryLayout)]
    public bool LastChildFill { get => (bool)GetPropertyValue(LastChildFillProperty)!; set => SetPropertyValue(LastChildFillProperty, value); }

    /// <summary>
    /// When true, opposing docks may overlap the remaining layout area; when false, they are constrained to avoid overlap.
    /// </summary>
    [Category(CategoryLayout)]
    public bool AllowOverlap { get => (bool)GetPropertyValue(AllowOverlapProperty)!; set => SetPropertyValue(AllowOverlapProperty, value); }

    /// <summary>
    /// Gets the nearest neighbor of a child in a given dock direction established during the last arrange pass.
    /// </summary>
    /// <param name="visual">The child visual to query.</param>
    /// <param name="type">The neighbor side to retrieve.</param>
    /// <returns>The adjacent visual for that side, or null if none.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="visual"/> is null.</exception>
    public Visual? GetAt(Visual visual, DockType type)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        if (!_docked.TryGetValue(visual, out var docked))
            return null;

        return type switch
        {
            DockType.Top => docked.Top,
            DockType.Bottom => docked.Bottom,
            DockType.Right => docked.Right,
            DockType.Left => docked.Left,
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Measures desired size by simulating docking and accumulating consumed width/height per orientation.
    /// </summary>
    /// <param name="constraint">Available size including margin.</param>
    /// <returns>The desired size required by docked children.</returns>
    /// <remarks>
    /// Children are measured in order with the remaining space after previously measured siblings in their orientation.
    /// </remarks>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        _lastChild = null;
        var width = 0f;
        var height = 0f;
        var childrenWidth = 0f;
        var childrenHeight = 0f;
        var children = VisibleChildren.ToArray();

        foreach (var child in children.Where(c => c.Parent != null))
        {
            var childConstraint = new D2D_SIZE_F(Math.Max(0, constraint.width - childrenWidth), Math.Max(0, constraint.height - childrenHeight));
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;
            if (childSize.IsZero)
                continue;

            var dock = GetDockType(child);
            switch (dock)
            {
                case DockType.Bottom:
                case DockType.Top:
                    childrenHeight += childSize.height;
                    width = Math.Max(width, childrenWidth + childSize.width);
                    break;

                case DockType.Right:
                case DockType.Left:
                    childrenWidth += childSize.width;
                    height = Math.Max(height, childrenHeight + childSize.height);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        width = Math.Max(width, childrenWidth);
        height = Math.Max(height, childrenHeight);
        return new D2D_SIZE_F(width, height);
    }

    /// <summary>
    /// Arranges children by docking them to the requested edges, honoring alignment and overlap rules.
    /// </summary>
    /// <param name="finalRect">Final rectangle allocated by the parent, without margin.</param>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var finalSize = finalRect.Size;
        _docked.Clear();
        var left = 0f;
        var top = 0f;
        var right = 0f;
        var bottom = 0f;

        Visual? lastLeft = null;
        Visual? lastTop = null;
        Visual? lastRight = null;
        Visual? lastBottom = null;
        var children = VisibleChildren.ToArray();
        var allowOverlap = AllowOverlap;
        var lastChildFill = LastChildFill;
        var noFillCount = children.Length - (lastChildFill ? 1 : 0);
        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child.Parent == null)
                continue; // skip detached children

            var dock = GetDockType(child);
            var childSize = child.DesiredSize;
            var rc = D2D_RECT_F.Sized(left, top, Math.Max(0, finalSize.width - (left + right)), Math.Max(0, finalSize.height - (top + bottom)));

            // Apply alignment within the current band for the docked side.
            if (dock == DockType.Top || dock == DockType.Bottom)
            {
                switch (child.HorizontalAlignment)
                {
                    case Alignment.Center:
                        rc.left += (finalSize.width - childSize.width) / 2;
                        rc.Width = childSize.width;
                        break;

                    case Alignment.Near:
                        rc.Width = childSize.width;
                        break;

                    case Alignment.Far:
                        rc.left += finalSize.width - childSize.width;
                        rc.Width = childSize.width;
                        break;

                    case Alignment.Stretch:
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                switch (child.VerticalAlignment)
                {
                    case Alignment.Center:
                        rc.top += (finalSize.height - childSize.height) / 2;
                        rc.Height = childSize.height;
                        break;

                    case Alignment.Near:
                        rc.Height = childSize.height;
                        break;

                    case Alignment.Far:
                        rc.top += finalSize.height - childSize.height;
                        rc.Height = childSize.height;
                        break;

                    case Alignment.Stretch:
                        // do nothing
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            if (i < noFillCount)
            {
                var docked = new Docked();
                _docked[child] = docked;
                switch (dock)
                {
                    case DockType.Right:
                        right += childSize.width;
                        rc.left = Math.Max(0, finalSize.width - right);

                        if (!allowOverlap)
                        {
                            rc.left = Math.Max(rc.left, left);
                        }
                        else
                        {
                            rc.Width = childSize.width;
                        }

                        setDocked(docked, child);
                        lastRight = child;
                        break;

                    case DockType.Top:
                        top += childSize.height;
                        rc.Height = childSize.height;

                        setDocked(docked, child);
                        lastTop = child;
                        break;

                    case DockType.Bottom:
                        bottom += childSize.height;
                        rc.top = Math.Max(0, finalSize.height - bottom);

                        if (!allowOverlap)
                        {
                            rc.top = Math.Max(rc.top, top);
                        }
                        else
                        {
                            rc.Height = childSize.height;
                        }

                        setDocked(docked, child);
                        lastBottom = child;
                        break;

                    case DockType.Left:
                        left += childSize.width;
                        rc.Width = childSize.width;

                        setDocked(docked, child);
                        lastLeft = child;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            if (rc.IsEmpty)
            {
                // This is a hack to "remove" this child if it was empty, to force it not to show
                child.Arrange(new D2D_RECT_F(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue));
            }
            else
            {
                child.Arrange(rc);
            }
            _lastChild = child;
        }

        // If last child should fill remaining space, also record its neighbor links
        if (lastChildFill && _lastChild != null)
        {
            var docked = new Docked();
            _docked[_lastChild] = docked;
            setDocked(docked, _lastChild);
        }

        void setDocked(Docked docked, Visual c)
        {
            if (lastLeft != null)
            {
                docked.Left = lastLeft;
                _docked[lastLeft].Right = c;
            }

            if (lastTop != null)
            {
                docked.Top = lastTop;
                _docked[lastTop].Bottom = c;
            }

            if (lastRight != null)
            {
                docked.Right = lastRight;
                _docked[lastRight].Left = c;
            }

            if (lastBottom != null)
            {
                docked.Bottom = lastBottom;
                _docked[lastBottom].Top = c;
            }
        }
    }

    private sealed class Docked
    {
        public Visual? Left;
        public Visual? Right;
        public Visual? Top;
        public Visual? Bottom;

        public override string ToString() => "L:" + Left + " T:" + Top + " R:" + Right + " B:" + Bottom;
    }
}
