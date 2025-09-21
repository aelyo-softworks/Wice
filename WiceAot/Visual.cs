namespace Wice;

/// <summary>
/// Base element for the Wice visual tree. Handles the full UI lifecycle:
/// layout (Measure/Arrange), rendering/composition, input routing (pointer/keyboard),
/// z-ordering, focus navigation, and parent/child relationships.
/// </summary>
public partial class Visual : BaseObject
{
    /// <summary>
    /// Category used in designers for properties participating in the layout pipeline (measure/arrange).
    /// </summary>
    public const string CategoryLayout = "Layout"; // measure & arrange phases

    /// <summary>
    /// Category used in designers for properties participating in the render pipeline.
    /// </summary>
    public const string CategoryRender = "Render"; // render phase

    /// <summary>
    /// Category used in designers for behavior and interaction properties.
    /// </summary>
    public const string CategoryBehavior = "Behavior"; // other
#if DEBUG
    /// <summary>
    /// Category used in designers for properties useful only in debug builds.
    /// </summary>
    public const string CategoryDebug = "Debug";
#endif

    /// <summary>
    /// Attached mouse cursor for this visual (null to inherit).
    /// </summary>
    public static VisualProperty CursorProperty { get; } = VisualProperty.Add<Cursor>(typeof(Visual), nameof(Cursor), VisualPropertyInvalidateModes.None);

    /// <summary>
    /// Arbitrary user data associated with this visual. Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty DataProperty { get; } = VisualProperty.Add<object>(typeof(Visual), nameof(Data), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Opacity of the visual in [0..1]. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty OpacityProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Opacity), VisualPropertyInvalidateModes.Render, 1f);

    /// <summary>
    /// Content scale factor applied during measure. Clamped to [0..+∞). Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty ZoomProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Zoom), VisualPropertyInvalidateModes.Measure, 1f, ValidateZoom);

    /// <summary>
    /// Whether this visual clips its children to its bounds. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty ClipChildrenProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ClipChildren), VisualPropertyInvalidateModes.Render, true);

    /// <summary>
    /// Whether this visual is clipped by its parent. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty ClipFromParentProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ClipFromParent), VisualPropertyInvalidateModes.Render, true);

    /// <summary>
    /// Explicit width in DIPs. Use <see cref="float.NaN"/> for auto. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty WidthProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Width), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Minimum width constraint in DIPs. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty MinWidthProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(MinWidth), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Maximum width constraint in DIPs. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty MaxWidthProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(MaxWidth), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Explicit height in DIPs. Use <see cref="float.NaN"/> for auto. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty HeightProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Height), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Minimum height constraint in DIPs. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty MinHeightProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(MinHeight), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Maximum height constraint in DIPs. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty MaxHeightProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(MaxHeight), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

    /// <summary>
    /// Logical visibility. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty IsVisibleProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsVisible), VisualPropertyInvalidateModes.ParentMeasure, true);

    /// <summary>
    /// Whether the visual can receive keyboard focus. Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty IsFocusableProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsFocusable), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Whether the visual is interactable. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty IsEnabledProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsEnabled), VisualPropertyInvalidateModes.Render, true);

    /// <summary>
    /// Enables layout rounding of arranged rectangle. Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty UseLayoutRoundingProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(UseLayoutRounding), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Whether the pointer is over this visual. Does not trigger invalidation by itself.
    /// </summary>
    public static VisualProperty IsMouseOverProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsMouseOver), VisualPropertyInvalidateModes.None, false, changed: IsMouseOverChanged); // is none ok,

    /// <summary>
    /// Outer margin (thickness) applied around the content. Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty MarginProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Margin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());

    /// <summary>
    /// Inner padding (thickness) applied inside the content. Triggers a measure pass when changed.
    /// </summary>
    public static VisualProperty PaddingProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(Padding), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());

    /// <summary>
    /// Vertical alignment used when constraining height. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty VerticalAlignmentProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(VerticalAlignment), VisualPropertyInvalidateModes.ParentMeasure, Alignment.Stretch);

    /// <summary>
    /// Horizontal alignment used when constraining width. Triggers a parent measure pass when changed.
    /// </summary>
    public static VisualProperty HorizontalAlignmentProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(HorizontalAlignment), VisualPropertyInvalidateModes.ParentMeasure, Alignment.Stretch);

    /// <summary>
    /// Rotation in degrees applied at composition time. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderRotationAngleProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(RenderRotationAngle), VisualPropertyInvalidateModes.Render, 0f);

    /// <summary>
    /// Rotation axis used with <see cref="RenderRotationAngle"/>. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderRotationAxisProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(RenderRotationAxis), VisualPropertyInvalidateModes.Render, new Vector3(0f, 0f, 1f));

    /// <summary>
    /// Scale applied at composition time. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderScaleProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(RenderScale), VisualPropertyInvalidateModes.Render, new Vector3(1f, 1f, 1f));

    /// <summary>
    /// Offset added at composition time. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderOffsetProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(RenderOffset), VisualPropertyInvalidateModes.Render, new Vector3());

    /// <summary>
    /// Primary brush used to paint this visual. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(Visual), nameof(RenderBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Optional brush used while the pointer hovers this visual. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty HoverRenderBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(Visual), nameof(HoverRenderBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Composition shadow applied to compatible visuals. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderShadowProperty { get; } = VisualProperty.Add<CompositionShadow>(typeof(Visual), nameof(RenderShadow), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Composition blending mode. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderCompositeModeProperty { get; } = VisualProperty.Add<CompositionCompositeMode>(typeof(Visual), nameof(RenderCompositeMode), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Matrix transform applied at composition time. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty RenderTransformMatrixProperty { get; } = VisualProperty.Add<Matrix4x4?>(typeof(Visual), nameof(RenderTransformMatrix), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Z-index ordering among siblings. Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty ZIndexProperty { get; } = VisualProperty.Add<int?>(typeof(Visual), nameof(ZIndex), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Optional index used to control focus navigation order among siblings during tabbing.
    /// Triggers a render pass when changed.
    /// </summary>
    public static VisualProperty FocusIndexProperty { get; } = VisualProperty.Add<int?>(typeof(Visual), nameof(FocusIndex), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Delegate used to build a tooltip's content when shown.
    /// </summary>
    public static VisualProperty ToolTipContentCreatorProperty { get; } = VisualProperty.Add<Action<ToolTip>>(typeof(Visual), nameof(ToolTipContentCreator), VisualPropertyInvalidateModes.None);

#pragma warning disable IDE0060 // Remove unused parameter
    /// <summary>
    /// Converts an empty string to null.
    /// </summary>
    /// <param name="obj">Owning object.</param>
    /// <param name="value">Incoming value.</param>
    /// <returns>Null when the string is empty; otherwise the original string.</returns>
    protected static object? NullifyString(BaseObject obj, object? value) => ((string)value!).Nullify();

    private static void IsMouseOverChanged(BaseObject obj, object? newValue, object? oldValue) => ((Visual)obj).IsMouseOverChanged((bool)newValue!);

    /// <summary>
    /// Replaces NaN coordinates in a <see cref="Vector2"/> by 0.
    /// </summary>
    protected static object? ValidateEmptyVector2(BaseObject obj, object? value)
    {
        var v2 = (Vector2)value!;
        var x = v2.X;
        var y = v2.Y;
        if (float.IsNaN(x))
        {
            x = 0;
        }

        if (float.IsNaN(y))
        {
            y = 0;
        }

        return new Vector2(x, y);
    }

    /// <summary>
    /// Validates a zoom factor, coercing NaN/∞ to 1 and values &lt; 0 to 0.
    /// </summary>
    protected static object? ValidateZoom(BaseObject obj, object? value)
    {
        var flt = (float)value!;
        if (float.IsNaN(flt) || float.IsPositiveInfinity(flt))
            return 1f;

        if (flt < 0 || float.IsNegativeInfinity(flt))
            return 0f;

        return flt;
    }

    /// <summary>
    /// Ensures a non-null string value by converting null to empty.
    /// </summary>
    protected static object? ValidateNonNullString(BaseObject obj, object? value)
    {
        var s = (string)value!;
        return s ?? string.Empty;
    }

    /// <summary>
    /// Validates width/height values, allowing <see cref="float.NaN"/> for auto and rejecting negatives/∞.
    /// </summary>
    protected static object? ValidateWidthOrHeight(BaseObject obj, object? value)
    {
        var flt = (float)value!;
        if (float.IsNaN(flt))
            return float.NaN;

        if (flt < 0 || float.IsInfinity(flt))
            throw new ArgumentOutOfRangeException(nameof(value));

        return flt;
    }

    /// <summary>
    /// Computes the scale factor to apply to content in order to fit within an available area, honoring
    /// the specified <paramref name="stretch"/> behavior and <paramref name="stretchDirection"/> constraints.
    /// </summary>
    /// <param name="availableSize">Available size to render into.</param>
    /// <param name="contentSize">Natural content size (unscaled).</param>
    /// <param name="stretch">How to scale content to fit the available size.</param>
    /// <param name="stretchDirection">Whether scaling can grow, shrink, or both.</param>
    /// <returns>A width/height scale factor. 1 means no scaling.</returns>
    public static D2D_SIZE_F GetScaleFactor(D2D_SIZE_F availableSize, D2D_SIZE_F contentSize, Stretch stretch, StretchDirection stretchDirection)
    {
        var scaleX = 1f;
        var scaleY = 1f;

        var isConstrainedWidth = !float.IsPositiveInfinity(availableSize.width);
        var isConstrainedHeight = !float.IsPositiveInfinity(availableSize.height);

        if ((stretch == Stretch.Uniform || stretch == Stretch.UniformToFill || stretch == Stretch.Fill) && (isConstrainedWidth || isConstrainedHeight))
        {
            // Compute scaling factors for both axes
            scaleX = contentSize.width == 0 ? 0 : availableSize.width / contentSize.width;
            scaleY = contentSize.height == 0 ? 0 : availableSize.height / contentSize.height;

            if (!isConstrainedWidth)
            {
                scaleX = scaleY;
            }
            else if (!isConstrainedHeight)
            {
                scaleY = scaleX;
            }
            else
            {
                switch (stretch)
                {
                    case Stretch.Uniform:
                        var minscale = scaleX < scaleY ? scaleX : scaleY;
                        scaleX = scaleY = minscale;
                        break;

                    case Stretch.UniformToFill:
                        var maxscale = scaleX > scaleY ? scaleX : scaleY;
                        scaleX = scaleY = maxscale;
                        break;
                }
            }

            switch (stretchDirection)
            {
                case StretchDirection.UpOnly:
                    if (scaleX < 1f)
                    {
                        scaleX = 1f;
                    }

                    if (scaleY < 1f)
                    {
                        scaleY = 1f;
                    }
                    break;

                case StretchDirection.DownOnly:
                    if (scaleX > 1f)
                    {
                        scaleX = 1f;
                    }

                    if (scaleY > 1f)
                    {
                        scaleY = 1f;
                    }
                    break;

                case StretchDirection.Both:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
        return new D2D_SIZE_F(scaleX, scaleY);
    }

    /// <summary>
    /// Raised when this visual is attached to its logical parent in the visual tree.
    /// </summary>
    public event EventHandler<EventArgs>? AttachedToParent;

    /// <summary>
    /// Raised just before this visual detaches from its logical parent in the visual tree.
    /// </summary>
    public event EventHandler<EventArgs>? DetachingFromParent;

    /// <summary>
    /// Raised after this visual has detached from its logical parent in the visual tree.
    /// </summary>
    public event EventHandler<EventArgs>? DetachedFromParent;

    /// <summary>
    /// Raised after this visual has been attached to the compositor tree.
    /// </summary>
    public event EventHandler<EventArgs>? AttachedToComposition;

    /// <summary>
    /// Raised just before this visual detaches from the compositor tree.
    /// </summary>
    public event EventHandler<EventArgs>? DetachingFromComposition;

    /// <summary>
    /// Raised after this visual has detached from the compositor tree.
    /// </summary>
    public event EventHandler<EventArgs>? DetachedFromComposition;

    /// <summary>
    /// Raised when the Render step completes for this visual.
    /// </summary>
    public event EventHandler<EventArgs>? Rendered;

    /// <summary>
    /// Raised when the Measure step completes for this visual.
    /// </summary>
    public event EventHandler<EventArgs>? Measured;

    /// <summary>
    /// Raised when the Arrange step completes for this visual.
    /// </summary>
    public event EventHandler<EventArgs>? Arranged;

    /// <summary>
    /// Raised when a child visual is added.
    /// </summary>
    public event EventHandler<ValueEventArgs<Visual>>? ChildAdded;

    /// <summary>
    /// Raised when a child visual is removed.
    /// </summary>
    public event EventHandler<ValueEventArgs<Visual>>? ChildRemoved;

    /// <summary>
    /// Raised when the mouse pointer enters this visual (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseEnter;

    /// <summary>
    /// Raised when the mouse pointer moves over this visual (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseMove;

    /// <summary>
    /// Raised when the mouse pointer leaves this visual (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseLeave;

    /// <summary>
    /// Raised when the mouse hovers on this visual (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseHover;

    /// <summary>
    /// Raised when the mouse wheel scrolls while this visual is hovered (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;

    /// <summary>
    /// Raised during a drag gesture with legacy mouse input.
    /// </summary>
    public event EventHandler<DragEventArgs>? MouseDrag;

    /// <summary>
    /// Raised when <see cref="IsMouseOver"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? MouseOverChanged;

    /// <summary>
    /// Raised when the focused state of this visual changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? FocusedChanged;

    /// <summary>
    /// Raised on mouse button press (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonDown;

    /// <summary>
    /// Raised on mouse button release (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonUp;

    /// <summary>
    /// Raised on mouse button double-click (legacy mouse input).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonDoubleClick;

    /// <summary>
    /// Raised during a drag gesture using pointer input.
    /// </summary>
    public event EventHandler<PointerDragEventArgs>? PointerDrag;

    /// <summary>
    /// Raised when the pointer wheel scrolls.
    /// </summary>
    public event EventHandler<PointerWheelEventArgs>? PointerWheel;

    /// <summary>
    /// Raised when a pointer enters this visual.
    /// </summary>
    public event EventHandler<PointerEnterEventArgs>? PointerEnter;

    /// <summary>
    /// Raised when a pointer leaves this visual.
    /// </summary>
    public event EventHandler<PointerLeaveEventArgs>? PointerLeave;

    /// <summary>
    /// Raised when a pointer updates its position over this visual.
    /// </summary>
    public event EventHandler<PointerPositionEventArgs>? PointerUpdate;

    /// <summary>
    /// Raised when a pointer changes contact state (pressed/released).
    /// </summary>
    public event EventHandler<PointerContactChangedEventArgs>? PointerContactChanged;

    /// <summary>
    /// Raised when a key is pressed while this visual has focus.
    /// </summary>
    public event EventHandler<KeyEventArgs>? KeyDown;

    /// <summary>
    /// Raised when a key is released while this visual has focus.
    /// </summary>
    public event EventHandler<KeyEventArgs>? KeyUp;

    /// <summary>
    /// Raised for translated text input while this visual has focus.
    /// </summary>
    public event EventHandler<KeyPressEventArgs>? KeyPress;

    /// <summary>
    /// Raised for OLE drag and drop operations.
    /// </summary>
    public event EventHandler<DragDropEventArgs>? DragDrop;

    internal D2D_SIZE_F? _lastMeasureSize;
    internal D2D_RECT_F? _lastArrangeRect;

    private static Visual? _focusRequestedVisual; // there's only one focused visual per app
    private int? _lastZIndex;
    private Visual? _parent;
    private int _level;
    private int _viewOrder;
    private D2D_SIZE_F _desiredSize;
    private D2D_RECT_F _arrangedRect;
    private D2D_RECT_F _relativeRenderRect;
    private D2D_RECT_F _absoluteRenderBounds;
    private DragState? _dragState;

    /// <summary>
    /// Initializes a new instance of <see cref="Visual"/> with default state and child collection.
    /// </summary>
    public Visual()
    {
        ResetState();
        RaiseOnPropertyChanged = true;
        Children = CreateChildren();
        Children.CollectionChanged += (s, e) => OnChildrenCollectionChanged(e);
        Level = this is Window ? 0 : 1;
        _viewOrder = this is Window ? 0 : -1;
    }

    private void ResetState(InvalidateMode? step = null)
    {
        if (!step.HasValue || step == InvalidateMode.Measure)
        {
            DesiredSize = D2D_SIZE_F.Invalid;

            ArrangedRect = D2D_RECT_F.Invalid;

            RelativeRenderRect = D2D_RECT_F.Invalid;
            AbsoluteRenderBounds = D2D_RECT_F.Invalid;

            if (!step.HasValue)
            {
                _lastArrangeRect = null;
                _lastMeasureSize = null;
                _lastZIndex = null;
            }
            return;
        }

        switch (step.Value)
        {
            case InvalidateMode.Arrange:
                ArrangedRect = D2D_RECT_F.Invalid;

                RelativeRenderRect = D2D_RECT_F.Invalid;
                AbsoluteRenderBounds = D2D_RECT_F.Invalid;
                return;

            case InvalidateMode.Render:
                RelativeRenderRect = D2D_RECT_F.Invalid;
                AbsoluteRenderBounds = D2D_RECT_F.Invalid;
                return;
        }
    }

    /// <inheritdoc/>
    public override string? Name
    {
        get => base.Name;
        set
        {
            if (base.Name == value)
                return;

            base.Name = value;
            var cv = CompositionVisual;
            if (cv != null)
            {
                cv.Comment = Name;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a drag operation is currently active for this visual.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsDragging => DragState != null;

    /// <summary>
    /// Gets a value indicating whether this visual currently owns the mouse capture.
    /// </summary>
    [Category(CategoryLive)]
    public bool HasCapturedMouse => Window.IsMouseCaptured(this) == true;

    /// <summary>
    /// Gets a value indicating whether both <see cref="Width"/> and <see cref="Height"/> are explicitly set.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsSizeSet => Width.IsSet() && Height.IsSet();

    /// <summary>
    /// Gets the set of composition properties temporarily suspended from being updated by <see cref="Render"/>.
    /// </summary>
    [Category(CategoryLive)]
    public CompositionUpdateParts SuspendedCompositionParts { get; protected set; }

    /// <summary>
    /// Gets the underlying composition visual associated with this visual when attached to a window.
    /// </summary>
    [Category(CategoryRender)]
    public ContainerVisual? CompositionVisual { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether this visual can accept a drop.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool AllowDrop { get; set; }

    /// <summary>
    /// Gets the containing window this visual is attached to, or null if not attached.
    /// </summary>
    [Browsable(false)]
    public Window? Window { get; internal set; }

    /// <summary>
    /// Gets the compositor of the containing window, or null if not attached.
    /// </summary>
    [Browsable(false)]
    public Compositor? Compositor => Window?.Compositor;

    /// <summary>
    /// Enumerates children whose <see cref="IsVisible"/> is true.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<Visual> VisibleChildren => Children.Where(c => c.IsVisible);

    /// <summary>
    /// Gets or sets the default duration used for color brush animations.
    /// When null, the theme default is used.
    /// </summary>
    [Category(CategoryBehavior)]
    public TimeSpan? ColorAnimationDuration { get; set; }

    /// <summary>
    /// Gets or sets the easing function used for color brush animations.
    /// When null, a linear easing is used.
    /// </summary>
    [Category(CategoryBehavior)]
    public CompositionEasingFunction? ColorAnimationEasingFunction { get; set; }

    /// <summary>
    /// Gets or sets whether pointer events should be ignored by this visual.
    /// </summary>
    protected internal virtual bool DisablePointerEvents { get; set; }

    /// <summary>
    /// Gets or sets whether keyboard events should be ignored by this visual.
    /// </summary>
    protected internal virtual bool DisableKeyEvents { get; set; }

    /// <summary>
    /// Gets or sets whether this visual handles pointer events before bubbling to the window.
    /// </summary>
    protected internal virtual bool HandlePointerEvents { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this visual should continue receiving input even when a modal window is shown.
    /// </summary>
    [Browsable(false)]
    public virtual bool ReceivesInputEvenWithModalShown { get; set; }

    /// <summary>
    /// Gets or sets whether this visual should be disposed automatically when detached from composition (if <see cref="IDisposable"/>).
    /// Default is true.
    /// </summary>
    [Browsable(false)]
    public virtual bool? DisposeOnDetachFromComposition { get; set; } = true; // for IDisposable Visuals only

    /// <summary>
    /// Gets or sets whether child visuals should be disposed automatically when this visual is detached from composition.
    /// When null, falls back to <see cref="DisposeOnDetachFromComposition"/>.
    /// </summary>
    [Browsable(false)]
    public virtual bool? DisposeChidrenOnDetachFromComposition { get; set; } // for IDisposable Visuals only

    /// <summary>
    /// Gets the last size used to measure this visual, or null if not measured yet.
    /// </summary>
    protected D2D_SIZE_F? LastMeasureSize => _lastMeasureSize;

    /// <summary>
    /// Gets the last rectangle used to arrange this visual, or null if not arranged yet.
    /// </summary>
    protected D2D_RECT_F? LastArrangeRect => _lastArrangeRect;

    /// <summary>
    /// Gets the current drag state when a drag gesture is active; otherwise null.
    /// </summary>
    protected DragState? DragState
    {
        get => _dragState;
        private set
        {
            if (_dragState == value)
                return;

            _dragState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDragging));
        }
    }

    private bool FinalClipFromParent
    {
        get
        {
            if (RenderShadow != null && !Values.ContainsKey(ClipFromParentProperty.Id))
                return false;

            return ClipFromParent;
        }
    }

    /// <summary>
    /// Gets the current theme in effect for the parent window or the default theme when unattached.
    /// </summary>
    public virtual Theme GetWindowTheme()
    {
        var window = Window;
        if (window == null)
            return Theme.Default;

        return window.Theme;
    }

    private void ResetViewOrders()
    {
        var w = Window;
        if (w == null)
            return;

        // we need view order for global hit/mouse/key testing
        // note we recompute the whole thing from the top here while this could maybe be optimized
        w.ResetViewOrders(0);
    }

    private int ResetViewOrders(int viewOrder)
    {
        _viewOrder = viewOrder;
        foreach (var child in Children.OrderBy(c => c.ZIndexOrDefault))
        {
            viewOrder++;
            viewOrder = child.ResetViewOrders(viewOrder);
        }
        return viewOrder;
    }

    private void ClearViewOrders()
    {
        _viewOrder = -1;
        foreach (var child in Children) // no need to order by zindex
        {
            child.ClearViewOrders();
        }
    }

    /// <summary>
    /// Gets the global view order index used for hit-testing and input routing.
    /// </summary>
    [Category(CategoryLayout)]
    public int ViewOrder => _viewOrder;

    /// <summary>
    /// Gets the depth level of this visual in the visual tree (root window = 0).
    /// </summary>
    [Category(CategoryLayout)]
    public int Level
    {
        get => _level;
        private set
        {
            if (_level == value)
                return;

            _level = value;
            foreach (var child in Children)
            {
                child.Level = _level + 1;
            }
        }
    }

    /// <summary>
    /// Gets the desired size of this visual including margins, as computed by <see cref="Measure(D2D_SIZE_F)"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_SIZE_F DesiredSize
    {
        get => _desiredSize;
        private set
        {
            if (_desiredSize == value)
                return;

            _desiredSize = value;
        }
    }

    /// <summary>
    /// Gets the arranged rectangle relative to parent including margins, as computed by <see cref="Arrange(D2D_RECT_F)"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_RECT_F ArrangedRect
    {
        get => _arrangedRect;
        private set
        {
            if (_arrangedRect == value)
                return;

            _arrangedRect = value;
        }
    }

    /// <summary>
    /// Gets the render size (width/height) computed for this visual (excludes margins).
    /// </summary>
    [Category(CategoryRender)]
    public D2D_SIZE_F RenderSize
    {
        get
        {
            if (_relativeRenderRect.IsInvalid)
                return D2D_SIZE_F.Invalid;

            return _relativeRenderRect.Size;
        }
    }

    /// <summary>
    /// Gets the render rectangle relative to the parent, excluding margins.
    /// Render overrides should not use Left/Top as the engine manages composition offsets.
    /// </summary>
    [Category(CategoryRender)]
    public D2D_RECT_F RelativeRenderRect
    {
        get => _relativeRenderRect;
        private set
        {
            if (_relativeRenderRect == value)
                return;

            _relativeRenderRect = value;
        }
    }

    /// <summary>
    /// Gets the absolute render bounds for hit-testing, possibly transformed by composition transforms.
    /// </summary>
    [Category(CategoryRender)]
    public D2D_RECT_F AbsoluteRenderBounds
    {
        get => _absoluteRenderBounds;
        private set
        {
            if (_absoluteRenderBounds == value)
                return;

            _absoluteRenderBounds = value;
        }
    }

    /// <summary>
    /// Gets the un-clipped render rectangle in absolute coordinates (window space).
    /// This is the same as <see cref="RelativeRenderRect"/> offset by the parent absolute rect.
    /// </summary>
    [Category(CategoryRender)]
    public D2D_RECT_F AbsoluteRenderRect
    {
        get
        {
            var rr = RelativeRenderRect;
            if (rr.IsInvalid)
                return D2D_RECT_F.Invalid;

            if (this is Window)
                return rr;

            var parent = Parent;
            if (parent == null)
                return D2D_RECT_F.Invalid;

            var par = parent.AbsoluteRenderRect;
            if (par.IsInvalid)
                return D2D_RECT_F.Invalid;

            return new D2D_RECT_F(par.LeftTop + rr.LeftTop, rr.Size);
        }
    }

    /// <summary>
    /// Gets the absolute offset of this visual (in composition space), accumulated through the parent chain.
    /// </summary>
    [Category(CategoryRender)]
    public Vector3 AbsoluteRenderOffset
    {
        get
        {
            var cv = CompositionVisual;
            if (cv == null)
                return Vector3.Zero;

            var parent = Parent;
            Vector3 offset;
            try
            {
                offset = cv.Offset;
                if (parent == null)
                    return offset;
            }
            catch (ObjectDisposedException)
            {
                // race condition with CompositionVisual
                return Vector3.Zero;
            }

            var po = parent.AbsoluteRenderOffset;
            return new Vector3(po.X + offset.X, po.Y + offset.Y, po.Z + offset.Z);
        }
    }

    /// <summary>
    /// Gets the active inset clip rectangle applied to the composition visual, if any.
    /// When present, the clip trims the right/bottom edges by the returned thickness values.
    /// </summary>
    [Category(CategoryRender)]
    public D2D_RECT_F? InsetClipRect
    {
        get
        {
            var cv = CompositionVisual;
            if (cv == null)
                return null;

            InsetClip? ic;
            try
            {
                ic = cv.Clip as InsetClip;
                if (ic == null)
                    return null;
            }
            catch (ObjectDisposedException)
            {
                // race condition with CompositionVisual
                return null;
            }

            return D2D_RECT_F.Thickness(ic.LeftInset, ic.TopInset, ic.RightInset, ic.BottomInset);
        }
    }

    /// <summary>
    /// Gets the logical parent visual, if any.
    /// </summary>
    [Browsable(false)]
    public Visual? Parent
    {
        get => _parent;
        private set
        {
            if (_parent == value)
                return;

            _parent = value;
            if (_parent != null)
            {
                AddToComposition();
            }
            else
            {
                Window.ReleaseMouseCapture();
                RemoveFromComposition(true, null);
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Enumerates all ancestors starting from <see cref="Parent"/> up to the root.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<Visual> AllParents
    {
        get
        {
            if (Parent == null)
                yield break;

            yield return Parent;
            foreach (var parent in Parent.AllParents)
            {
                yield return parent;
            }
        }
    }

    /// <summary>
    /// Gets the mutable collection of child visuals.
    /// </summary>
    [Browsable(false)]
    public BaseObjectCollection<Visual> Children { get; }

    /// <summary>
    /// Enumerates all descendants in depth-first order.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<Visual> AllChildren
    {
        get
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var gchild in child.AllChildren)
                {
                    yield return gchild;
                }
            }
        }
    }

#if DEBUG
    /// <summary>
    /// Gets the z-index or, when not set, the sibling index within the parent collection (debug only).
    /// </summary>
    [Category(CategoryDebug)]
    public int? ZIndexOrDefault
#else
    internal int? ZIndexOrDefault
#endif
    {
        get
        {
            var zi = ZIndex;
            if (zi.HasValue)
                return zi.Value;

            return Parent?.Children.IndexOf(this);
        }
    }

    /// <summary>
    /// Gets or sets the cursor to display when the pointer hovers this visual.
    /// </summary>
    [Browsable(false)]
    public Cursor? Cursor { get => (Cursor?)GetPropertyValue(CursorProperty); set => SetPropertyValue(CursorProperty, value); }

    /// <summary>
    /// Gets or sets arbitrary user data associated with this visual.
    /// </summary>
    [Browsable(false)]
    public object? Data { get => GetPropertyValue(DataProperty)!; set => SetPropertyValue(DataProperty, value); }

    /// <summary>
    /// Gets or sets the opacity in [0..1].
    /// </summary>
    [Category(CategoryRender)]
    public float Opacity { get => (float)GetPropertyValue(OpacityProperty)!; set => SetPropertyValue(OpacityProperty, value); }

    /// <summary>
    /// Gets or sets the content zoom factor applied during measure.
    /// </summary>
    [Category(CategoryLayout)]
    public float Zoom { get => (float)GetPropertyValue(ZoomProperty)!; set => SetPropertyValue(ZoomProperty, value); }

    /// <summary>
    /// Gets or sets explicit width (DIPs). Use <see cref="float.NaN"/> for auto.
    /// </summary>
    [Category(CategoryLayout)]
    public float Width { get => (float)GetPropertyValue(WidthProperty)!; set => SetPropertyValue(WidthProperty, value); }

    /// <summary>
    /// Gets or sets minimum width (DIPs).
    /// </summary>
    [Category(CategoryLayout)]
    public float MinWidth { get => (float)GetPropertyValue(MinWidthProperty)!; set => SetPropertyValue(MinWidthProperty, value); }

    /// <summary>
    /// Gets or sets maximum width (DIPs).
    /// </summary>
    [Category(CategoryLayout)]
    public float MaxWidth { get => (float)GetPropertyValue(MaxWidthProperty)!; set => SetPropertyValue(MaxWidthProperty, value); }

    /// <summary>
    /// Gets or sets explicit height (DIPs). Use <see cref="float.NaN"/> for auto.
    /// </summary>
    [Category(CategoryLayout)]
    public float Height { get => (float)GetPropertyValue(HeightProperty)!; set => SetPropertyValue(HeightProperty, value); }

    /// <summary>
    /// Gets or sets minimum height (DIPs).
    /// </summary>
    [Category(CategoryLayout)]
    public float MinHeight { get => (float)GetPropertyValue(MinHeightProperty)!; set => SetPropertyValue(MinHeightProperty, value); }

    /// <summary>
    /// Gets or sets maximum height (DIPs).
    /// </summary>
    [Category(CategoryLayout)]
    public float MaxHeight { get => (float)GetPropertyValue(MaxHeightProperty)!; set => SetPropertyValue(MaxHeightProperty, value); }

    /// <summary>
    /// Gets or sets the logical visibility of this visual.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsVisible { get => (bool)GetPropertyValue(IsVisibleProperty)!; set => SetPropertyValue(IsVisibleProperty, value); }

    /// <summary>
    /// Gets or sets whether this visual clips its children to its bounds.
    /// </summary>
    [Category(CategoryLayout)]
    public bool ClipChildren { get => (bool)GetPropertyValue(ClipChildrenProperty)!; set => SetPropertyValue(ClipChildrenProperty, value); }

    /// <summary>
    /// Gets or sets whether this visual is clipped by its parent.
    /// </summary>
    [Category(CategoryLayout)]
    public bool ClipFromParent { get => (bool)GetPropertyValue(ClipFromParentProperty)!; set => SetPropertyValue(ClipFromParentProperty, value); }

    /// <summary>
    /// Gets or sets the z-index ordering among siblings.
    /// </summary>
    [Category(CategoryLayout)]
    public int? ZIndex { get => (int?)GetPropertyValue(ZIndexProperty); set => SetPropertyValue(ZIndexProperty, value); }

    /// <summary>
    /// Gets or sets the ordering index used to control keyboard focus navigation among siblings.
    /// </summary>
    [Category(CategoryLayout)]
    public int? FocusIndex { get => (int?)GetPropertyValue(FocusIndexProperty); set => SetPropertyValue(FocusIndexProperty, value); }

    /// <summary>
    /// Gets or sets the outer margin applied around the content.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_RECT_F Margin { get => (D2D_RECT_F)GetPropertyValue(MarginProperty)!; set => SetPropertyValue(MarginProperty, value); }

    /// <summary>
    /// Gets or sets the inner padding applied within the content.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_RECT_F Padding { get => (D2D_RECT_F)GetPropertyValue(PaddingProperty)!; set => SetPropertyValue(PaddingProperty, value); }

    /// <summary>
    /// Gets or sets the vertical alignment used when constraining height.
    /// </summary>
    [Category(CategoryLayout)]
    public Alignment VerticalAlignment { get => (Alignment)GetPropertyValue(VerticalAlignmentProperty)!; set => SetPropertyValue(VerticalAlignmentProperty, value); }

    /// <summary>
    /// Gets or sets the horizontal alignment used when constraining width.
    /// </summary>
    [Category(CategoryLayout)]
    public Alignment HorizontalAlignment { get => (Alignment)GetPropertyValue(HorizontalAlignmentProperty)!; set => SetPropertyValue(HorizontalAlignmentProperty, value); }

    /// <summary>
    /// Gets or sets whether this visual can receive keyboard focus.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool IsFocusable { get => (bool)GetPropertyValue(IsFocusableProperty)!; set => SetPropertyValue(IsFocusableProperty, value); }

    /// <summary>
    /// Gets or sets whether this visual is enabled for interaction.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool IsEnabled { get => (bool)GetPropertyValue(IsEnabledProperty)!; set => SetPropertyValue(IsEnabledProperty, value); }

    /// <summary>
    /// Gets or sets whether layout rounding is applied to the arranged rectangle.
    /// </summary>
    [Category(CategoryLayout)]
    public bool UseLayoutRounding { get => (bool)GetPropertyValue(UseLayoutRoundingProperty)!; set => SetPropertyValue(UseLayoutRoundingProperty, value); }

    /// <summary>
    /// Gets or sets whether the pointer is currently over this visual.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsMouseOver { get => (bool)GetPropertyValue(IsMouseOverProperty)!; set => SetPropertyValue(IsMouseOverProperty, value); }

    /// <summary>
    /// Gets or sets the delegate that builds tooltip content for this visual.
    /// </summary>
    [Browsable(false)]
    public Action<ToolTip>? ToolTipContentCreator { get => (Action<ToolTip>?)GetPropertyValue(ToolTipContentCreatorProperty); set => SetPropertyValue(ToolTipContentCreatorProperty, value); }

    // composition/render specific properties

    /// <summary>
    /// Gets or sets the rotation in degrees applied at composition time.
    /// </summary>
    [Category(CategoryRender)]
    public float RenderRotationAngle { get => (float)GetPropertyValue(RenderRotationAngleProperty)!; set => SetPropertyValue(RenderRotationAngleProperty, value); }

    /// <summary>
    /// Gets or sets the axis used with <see cref="RenderRotationAngle"/>.
    /// </summary>
    [Category(CategoryRender)]
    public Vector3 RenderRotationAxis { get => (Vector3)GetPropertyValue(RenderRotationAxisProperty)!; set => SetPropertyValue(RenderRotationAxisProperty, value); }

    /// <summary>
    /// Gets or sets the composition time scale.
    /// </summary>
    [Category(CategoryRender)]
    public Vector3 RenderScale { get => (Vector3)GetPropertyValue(RenderScaleProperty)!; set => SetPropertyValue(RenderScaleProperty, value); }

    /// <summary>
    /// Gets or sets the additional offset applied at composition time.
    /// </summary>
    [Category(CategoryRender)]
    public Vector3 RenderOffset { get => (Vector3)GetPropertyValue(RenderOffsetProperty)!; set => SetPropertyValue(RenderOffsetProperty, value); }

    /// <summary>
    /// Gets or sets the matrix transform applied at composition time.
    /// </summary>
    [Category(CategoryRender)]
    public Matrix4x4? RenderTransformMatrix { get => (Matrix4x4?)GetPropertyValue(RenderTransformMatrixProperty); set => SetPropertyValue(RenderTransformMatrixProperty, value); }

    /// <summary>
    /// Gets or sets the primary brush used to paint this visual.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush? RenderBrush { get => (CompositionBrush)GetPropertyValue(RenderBrushProperty)!; set => SetPropertyValue(RenderBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush used while the pointer hovers this visual.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush? HoverRenderBrush { get => (CompositionBrush)GetPropertyValue(HoverRenderBrushProperty)!; set => SetPropertyValue(HoverRenderBrushProperty, value); }

    /// <summary>
    /// Gets or sets the composition shadow applied to the composition visual when supported.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionShadow? RenderShadow { get => (CompositionShadow)GetPropertyValue(RenderShadowProperty)!; set => SetPropertyValue(RenderShadowProperty, value); }

    /// <summary>
    /// Gets or sets the composition composite mode used to blend this visual.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionCompositeMode RenderCompositeMode { get => (CompositionCompositeMode)GetPropertyValue(RenderCompositeModeProperty)!; set => SetPropertyValue(RenderCompositeModeProperty, value); }

    /// <summary>
    /// Gets a value indicating whether this visual or any of its descendants are focused.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsFocusedOrAnyChildrenFocused => IsFocused || IsAnyChildrenFocused;

    /// <summary>
    /// Gets a value indicating whether any descendant of this visual is focused.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsAnyChildrenFocused => AllChildren.Any(c => c.IsFocused);

    /// <summary>
    /// Gets a value indicating whether this visual is the focused visual for its window.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsFocused
    {
        get
        {
            if (Window?.FocusedVisual == this)
                return true;

            if (this is Window win && win.HasFocus)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this visual is effectively visible for layout and hit testing,
    /// considering its own visibility, size, parent visibility, layout state, and composition state.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsActuallyVisible
    {
        get
        {
            if (!IsVisible)
                return false;

            if (Width == 0 || Height == 0)
                return false;

            var ar = RelativeRenderRect;
            if (ar.IsInvalid)
                return false;

            if (Parent == null)
                return true;

            if (!Parent.IsActuallyVisible)
                return false;

            if (CompositionVisual?.IsVisible != true)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Gets the intersection of all ancestors' absolute render rectangles to compute the effective clip.
    /// </summary>
    [Category(CategoryRender)]
    public D2D_RECT_F? ParentsAbsoluteClipRect
    {
        get
        {
            D2D_RECT_F? arc = null;
            foreach (var rc in ParentsAbsoluteRenderRects)
            {
                if (!arc.HasValue)
                {
                    arc = rc;
                }
                else
                {
                    arc = arc.Value.Intersect(rc);
                }
            }
            return arc;
        }
    }

    /// <summary>
    /// Enumerates absolute render rectangles of all ancestors starting from the direct parent.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<D2D_RECT_F> ParentsAbsoluteRenderRects
    {
        get
        {
            if (Parent == null)
                yield break;

            if (Parent.AbsoluteRenderRect.IsValid)
                yield return Parent.AbsoluteRenderRect;

            foreach (var rc in Parent.ParentsAbsoluteRenderRects)
            {
                yield return rc;
            }
        }
    }

    /// <summary>
    /// Removes this visual from its parent (optionally removing its subtree).
    /// </summary>
    /// <param name="deep">True to remove all descendants first; otherwise remove only this visual.</param>
    /// <returns>
    /// True when removed; false when not found; null when no parent.
    /// </returns>
    public bool? Remove(bool deep = true)
    {
        if (this is Window)
            throw new InvalidOperationException();

        if (deep)
        {
            var children = Children?.ToArray();
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Remove(deep);
                }
            }
        }

        return Parent?.Children?.Remove(this);
    }

    /// <summary>
    /// Called when <see cref="IsMouseOver"/> changes.
    /// </summary>
    /// <param name="newValue">New hover state.</param>
    protected virtual void IsMouseOverChanged(bool newValue)
    {
        OnMouseOverChanged(this, new ValueEventArgs<bool>(newValue));
        //OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsMouseOver)));

        var hover = HoverRenderBrush;
        if (hover != null && !CompositionObjectEqualityComparer.Default.Equals(hover, RenderBrush))
        {
            //Application.Trace("this: " + this + " newValue: " + newValue);
            Invalidate(VisualPropertyInvalidateModes.Render, new PropertyInvalidateReason(IsMouseOverProperty));
        }
    }

    /// <summary>
    /// Called by the window when the focused state changes.
    /// </summary>
    /// <param name="newValue">True when focused; otherwise false.</param>
    protected virtual internal void IsFocusedChanged(bool newValue)
    {
        //Application.Trace(this + " new: " + newValue);
        OnFocusedChanged(this, new ValueEventArgs<bool>(newValue));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsFocused)));
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if already attached to a parent, or once attached.
    /// </summary>
    public T? DoWhenAttachedToParent<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent != null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            AttachedToParent += onAttached;
        }
        return default;

        void onAttached(object? sender, EventArgs e)
        {
            AttachedToParent -= onAttached;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if already attached to a parent, or once attached.
    /// </summary>
    public void DoWhenAttachedToParent(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent != null)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            AttachedToParent += onAttached;
        }
        return;

        void onAttached(object? sender, EventArgs e)
        {
            AttachedToParent -= onAttached;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if detaching from parent will occur, or once detaching starts.
    /// </summary>
    public T? DoWhenDetachingFromParent<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent != null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachingFromParent += onDetaching;
        }
        return default;

        void onDetaching(object? sender, EventArgs e)
        {
            DetachingFromParent -= onDetaching;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if detaching from parent will occur, or once detaching starts.
    /// </summary>
    public void DoWhenDetachingFromParent(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent != null)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachingFromParent += onDetaching;
        }
        return;

        void onDetaching(object? sender, EventArgs e)
        {
            DetachingFromParent -= onDetaching;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if already detached from the parent, or once detached.
    /// </summary>
    public T? DoWhenDetachedFromParent<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent == null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachedFromParent += onDetached;
        }
        return default;

        void onDetached(object? sender, EventArgs e)
        {
            DetachedFromParent -= onDetached;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if already detached from the parent, or once detached.
    /// </summary>
    public void DoWhenDetachedFromParent(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Parent == null)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachedFromParent += onDetached;
        }
        return;

        void onDetached(object? sender, EventArgs e)
        {
            DetachedFromParent -= onDetached;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if attached to composition (window exists), or once attached.
    /// </summary>
    public T? DoWhenAttachedToComposition<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Window != null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            AttachedToComposition += onAttached;
        }
        return default;

        void onAttached(object? sender, EventArgs e)
        {
            AttachedToComposition -= onAttached;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if attached to composition (window exists), or once attached.
    /// </summary>
    public void DoWhenAttachedToComposition(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Window != null)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            AttachedToComposition += onAttached;
        }
        return;

        void onAttached(object? sender, EventArgs e)
        {
            AttachedToComposition -= onAttached;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if detaching from composition will occur, or once detaching starts.
    /// </summary>
    public T? DoWhenDetachingFromComposition<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Window != null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachingFromComposition += onDetaching;
        }
        return default;

        void onDetaching(object? sender, EventArgs e)
        {
            DetachingFromComposition -= onDetaching;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if detaching from composition will occur, or once detaching starts.
    /// </summary>
    public void DoWhenDetachingFromComposition(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Window != null)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachingFromComposition += onDetaching;
        }
        return;

        void onDetaching(object? sender, EventArgs e)
        {
            DetachingFromComposition -= onDetaching;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if already detached from composition (no window), or once detached.
    /// </summary>
    public T? DoWhenDetachedFromComposition<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (Window == null)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachedFromComposition += onDetached;
        }
        return default;

        void onDetached(object? sender, EventArgs e)
        {
            DetachedFromComposition -= onDetached;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if already detached from composition (no window), or once detached.
    /// </summary>
    public void DoWhenDetachedFromComposition(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly) && Window == null)
        {
            action();
            return;
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            DetachedFromComposition += onDetached;
        }
        return;

        void onDetached(object? sender, EventArgs e)
        {
            DetachedFromComposition -= onDetached;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if already measured, or once measured.
    /// </summary>
    public T? DoWhenMeasured<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (DesiredSize.IsValid)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Measured += onMeasured;
        }
        return default;

        void onMeasured(object? sender, EventArgs e)
        {
            Measured -= onMeasured;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if already measured, or once measured.
    /// </summary>
    public void DoWhenMeasured(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (DesiredSize.IsValid)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Measured += onMeasured;
        }
        return;

        void onMeasured(object? sender, EventArgs e)
        {
            Measured -= onMeasured;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if already arranged, or once arranged.
    /// </summary>
    public T? DoWhenArranged<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (ArrangedRect.IsValid)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Arranged += onArranged;
        }
        return default;

        void onArranged(object? sender, EventArgs e)
        {
            Arranged -= onArranged;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if already arranged, or once arranged.
    /// </summary>
    public void DoWhenArranged(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (ArrangedRect.IsValid)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Arranged += onArranged;
        }
        return;

        void onArranged(object? sender, EventArgs e)
        {
            Arranged -= onArranged;
            action();
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> immediately if this visual has been rendered, or once rendered.
    /// </summary>
    public T? DoWhenRendered<T>(Func<T> func, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (AbsoluteRenderBounds.IsValid)
                return func();
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Rendered += onRendered;
        }
        return default;

        void onRendered(object? sender, EventArgs e)
        {
            Rendered -= onRendered;
            func();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> immediately if this visual has been rendered, or once rendered.
    /// </summary>
    public void DoWhenRendered(Action action, VisualDoOptions options = VisualDoOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!options.HasFlag(VisualDoOptions.DeferredOnly))
        {
            if (AbsoluteRenderBounds.IsValid)
            {
                action();
                return;
            }
        }

        if (!options.HasFlag(VisualDoOptions.ImmediateOnly))
        {
            Rendered += onRendered;
        }
        return;

        void onRendered(object? sender, EventArgs e)
        {
            Rendered -= onRendered;
            action();
        }
    }

    /// <summary>
    /// Attempts to focus this visual (or its focusable child if implementing <see cref="IFocusableParent"/>).
    /// If the visual is not yet rendered, the focus request is deferred until after the render pass.
    /// </summary>
    /// <returns>
    /// true if focus is acquired; false if rejected or superseded; null if the operation is deferred.
    /// </returns>
    public virtual bool? Focus()
    {
        _focusRequestedVisual = this;
        if (Window != null && RelativeRenderRect.IsSet) // early call?
            return doFocus();

        // the focus can only be shown when the item has been rendered
        DoWhenRendered(doFocus);
        return null; // not sure

        bool? doFocus()
        {
            Window?.SetImmCompositionWindowPosition(this);

            // has any other visual taken the place?
            if (Interlocked.CompareExchange(ref _focusRequestedVisual, null, this) != this)
                return false;

            if (this is IFocusableParent focusable && focusable.FocusableVisual != null)
            {
                if (Window?.Focus(focusable.FocusableVisual) == true)
                    return true;
            }

            return Window?.Focus(this);
        }
    }

    /// <summary>
    /// Returns whether the specified <paramref name="parent"/> is an ancestor of this visual.
    /// </summary>
    public bool IsParent(Visual parent, bool deep = true)
    {
        if (parent == null)
            return false;

        var first = true;
        foreach (var p in AllParents)
        {
            if (p == parent)
                return true;

            if (first && !deep)
                return false;

            first = false;
        }
        return false;
    }

    /// <summary>
    /// Returns whether the specified <paramref name="child"/> is a descendant of this visual.
    /// </summary>
    public bool IsChild(Visual child, bool deep = true)
    {
        if (child == null)
            return false;

        var children = Children;
        if (children == null)
            return false;

        if (deep)
            return AllChildren.Any(c => c == child);

        return children.Any(c => c == child);
    }

    /// <summary>
    /// Gets a focusable visual in the specified navigation <paramref name="direction"/> relative to this visual.
    /// </summary>
    public virtual Visual? GetFocusable(FocusDirection direction)
    {
        Visual? focusable;
        switch (direction)
        {
            case FocusDirection.Previous:
                focusable = GetFocusableChildren(null).LastOrDefault();
                if (focusable != null)
                    return focusable;

                break;

            case FocusDirection.Next:
                focusable = GetFocusableChildren(null).FirstOrDefault();
                if (focusable != null)
                    return focusable;

                break;

            default:
                throw new NotSupportedException();
        }

        return Parent?.GetFocusableSibling(this, direction);
    }

    private Visual? GetFocusableSibling(Visual visual, FocusDirection direction)
    {
        var all = GetFocusableChildren(visual).ToList();
        var index = all.IndexOf(visual);
        if (all.Count > 0)
        {
            switch (direction)
            {
                case FocusDirection.Previous:
                    if (index < 0)
#if NETFRAMEWORK
                        return all[all.Count - 1];
#else
                        return all[^1];
#endif

                    if ((index - 1) >= 0)
                        return all[index - 1];

                    break;

                case FocusDirection.Next:
                    if (index < 0)
                        return all[0];

                    if ((index + 1) < all.Count)
                        return all[index + 1];

                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        if (!this.IsModal())
        {
            // try upstairs
            var focusable = Parent?.GetFocusableSibling(visual, direction);
            if (focusable != null)
                return focusable;
        }

        // nothing found upstairs? get back here if we have something
        if (index >= 0 && all.Count > 1)
        {
            switch (direction)
            {
                case FocusDirection.Previous:
                    if (index == 0 && all.Count > 1)
#if NETFRAMEWORK
                        return all[all.Count - 1];
#else
                        return all[^1];
#endif

                    break;

                case FocusDirection.Next:
                    if (index == (all.Count - 1) && all.Count > 1)
                        return all[0];

                    break;

                default:
                    throw new NotSupportedException();
            }
        }
        return null;
    }

    private IEnumerable<Visual> GetFocusableChildren(Visual? forceInclude)
    {
        foreach (var child in GetOrderedActuallyVisibleChildren(forceInclude))
        {
            if (child.IsFocusable || forceInclude == child)
            {
                yield return child;
            }

            foreach (var gchild in child.GetFocusableChildren(forceInclude))
            {
                yield return gchild;
            }
        }
    }

    private IEnumerable<Visual> GetOrderedActuallyVisibleChildren(Visual? forceInclude) => GetActuallyVisibleChildren(forceInclude).OrderBy(c => c.Item2).Select(c => c.Item1);
    private IEnumerable<Tuple<Visual, int>> GetActuallyVisibleChildren(Visual? forceInclude)
    {
        var idx = 0;
        foreach (var child in Children.Where(c => c.IsActuallyVisible || forceInclude == c))
        {
            yield return new Tuple<Visual, int>(child, child.FocusIndex ?? idx);
            idx++;
        }
    }

    private void AddToComposition()
    {
        if (Window != null)
            throw new WiceException("0009: Visual '" + Name + "' of type '" + GetType().Name + "' already has a parent Window ('" + Window.Native + "' of type '" + Window.GetType().Name + "').");

        Window = _parent?.Window;
        var attached = false;
        if (Window != null)
        {
            CompositionVisual = CreateCompositionVisual();
            if (CompositionVisual == null)
                throw new WiceException("0003: Visual '" + Name + "' of type '" + GetType().Name + "' must create a valid composition Visual.");

#if DEBUG
            CompositionVisual.Comment = Name;
#endif
            InsertCompositionVisual();
            attached = true;
        }
        // else parent may not currently be attached to composition

        foreach (var child in Children)
        {
            child.AddToComposition();
        }

        if (attached)
        {
            OnAttachedToComposition(this, EventArgs.Empty);
        }
    }

    private static void RemoveCompositionVisual(Windows.UI.Composition.Visual? visual)
    {
        if (visual == null)
            return;

        visual.Parent?.Children.Remove(visual);
        if (visual is ContainerVisual cv)
        {
            foreach (var child in cv.Children)
            {
                RemoveCompositionVisual(child);
            }
        }
        visual.Dispose();
    }

    private void RemoveFromComposition(bool includeComposition, bool? disposeOnDetachFromComposition)
    {
        ResetState();

        var oldWin = Window;
        if (oldWin != null)
        {
            oldWin.RemoveVisual(this);
            OnDetachingFromComposition(this, EventArgs.Empty);
        }

        Window = null;

        if (includeComposition)
        {
            RemoveCompositionVisual(CompositionVisual);
        }

        CompositionVisual = null;

        OnDetachedFromComposition(this, EventArgs.Empty);

        // priority is local def first
        var dispose = DisposeOnDetachFromComposition;
        if (disposeOnDetachFromComposition == null)
        {
            dispose = disposeOnDetachFromComposition;
        }

        if (dispose == true && this is IDisposable disposable)
        {
            disposable.Dispose();
        }

        var disposeChildren = DisposeChidrenOnDetachFromComposition;
        if (disposeChildren == null)
        {
            if (disposeOnDetachFromComposition != null)
            {
                disposeChildren = disposeOnDetachFromComposition;
            }
            else if (DisposeOnDetachFromComposition != null)
            {
                disposeChildren = DisposeOnDetachFromComposition;
            }
        }

        foreach (var child in Children)
        {
            child.RemoveFromComposition(false, disposeChildren);
        }
    }

    /// <summary>
    /// Creates a <see cref="DragState"/> used to track a drag gesture for this visual.
    /// </summary>
    /// <param name="e">Source mouse button args.</param>
    /// <returns>A new <see cref="DragState"/>.</returns>
    protected virtual DragState CreateDragState(MouseButtonEventArgs e) => new(this, e);

    /// <summary>
    /// Converts an absolute window position to coordinates relative to this visual.
    /// </summary>
    public POINT GetRelativePosition(int left, int top)
    {
        var arr = AbsoluteRenderRect;
        if (arr.IsInvalid)
            return new POINT();

        var pt = new POINT(left - arr.left, top - arr.top);
        return pt;
    }

    /// <summary>
    /// Converts an absolute window position to coordinates relative to this visual.
    /// </summary>
    public D2D_POINT_2F GetRelativePosition(float left, float top)
    {
        var arr = AbsoluteRenderRect;
        if (arr.IsInvalid)
            return new D2D_POINT_2F();

        var pt = new D2D_POINT_2F(left - arr.left, top - arr.top);
        return pt;
    }

    private static bool CanAnimateColor(CompositionBrush? from, CompositionBrush? to, out D3DCOLORVALUE? toColor, out D3DCOLORVALUE? fromColor)
    {
        toColor = null;
        fromColor = null;
        var toBrush = to as CompositionColorBrush;
        if (to != null && toBrush == null)
            return false;

        var fromBrush = from as CompositionColorBrush;
        if (from != null && fromBrush == null)
            return false;

        if (toBrush?.Color != null)
        {
            toColor = toBrush.Color.ToColor();
        }

        if (fromBrush?.Color != null)
        {
            fromColor = fromBrush.Color.ToColor();
        }

        return true;
    }

    private void AnimateColor(D3DCOLORVALUE to, D3DCOLORVALUE? from = null)
    {
        var compositor = Compositor;
        if (compositor == null)
            return;

        // note: we need to clone all brushes.
        // otherwise, we'll change the brushes that are stored in BaseObject directly
        // and won't be able to change them back (BaseObject's dictionary's inner objects)
        var brushes = new List<CompositionBrush>();
        if (CompositionVisual is SpriteVisual sv)
        {
            if (sv.Brush is not CompositionColorBrush cb)
            {
                // no clone since we create it
                Color color;
                if (from != null)
                {
                    color = from.Value.ToColor();
                }
                else
                {
                    color = D3DCOLORVALUE.Transparent.ToColor();
                }
                sv.Brush = compositor.CreateColorBrush(color);
            }
            else
            {
                var clone = (CompositionColorBrush)cb.Clone();
                if (from != null)
                {
                    clone.Color = from.Value.ToColor();
                }
                // else we keep the current color

                sv.Brush = clone;
            }
            brushes.Add(sv.Brush);
        }
        else if (CompositionVisual is ShapeVisual shp)
        {
            brushes.AddRange(shp.EnumerateAllStrokeBrushes().Select(b => b.Clone()));
        }

        if (brushes.Count == 0)
            return;

        var animation = compositor.CreateColorKeyFrameAnimation();
        animation.Duration = ColorAnimationDuration ?? GetWindowTheme().BrushAnimationDuration;
        animation.InsertKeyFrame(1f, to.ToColor(), ColorAnimationEasingFunction ?? compositor.CreateLinearEasingFunction());
        foreach (var brush in brushes)
        {
            brush.StartAnimation(nameof(CompositionColorBrush.Color), animation);
        }
    }

#if DEBUG
    /// <summary>
    /// Gets the full hierarchical name of this visual (debug only).
    /// </summary>
    public new string? FullName => GetFullName();
#endif

    /// <inheritdoc/>
    protected override string? GetFullName()
    {
        var parent = Parent;
        if (parent == null)
            return Name;

        if (string.IsNullOrEmpty(Name))
            return parent.FullName;

        var pf = parent.FullName;
        if (string.IsNullOrEmpty(pf))
            return Name;

        return pf + "\\" + Name;
    }

    internal void OnKeyPressEvent(KeyPressEventArgs e)
    {
        if (DisableKeyEvents)
            return;

        OnKeyPress(this, e);
    }

    internal void OnKeyEvent(KeyEventArgs e)
    {
        if (DisableKeyEvents)
            return;

        if (e.IsDown)
        {
            if (this is IAccessKeyParent ak)
            {
                ak.OnAccessKey(e);
                if (e.Handled)
                    return;
            }

            OnKeyDown(this, e);
            return;
        }
        OnKeyUp(this, e);
    }

    /// <summary>
    /// Called when a key is pressed while this visual has focus.
    /// </summary>
    protected virtual void OnKeyDown(object? sender, KeyEventArgs e) => KeyDown?.Invoke(sender, e);

    /// <summary>
    /// Called when a key is released while this visual has focus.
    /// </summary>
    protected virtual void OnKeyUp(object? sender, KeyEventArgs e) => KeyUp?.Invoke(sender, e);

    /// <summary>
    /// Called for text input while this visual has focus.
    /// </summary>
    protected virtual void OnKeyPress(object? sender, KeyPressEventArgs e) => KeyPress?.Invoke(sender, e);

    /// <summary>
    /// Initiates an OLE drag-drop operation originating from this visual.
    /// </summary>
    public virtual void DoDragDrop(IDataObject data, DROPEFFECT allowedEffects) => Window?.DoDragDrop(this, data, allowedEffects);

    internal void OnDragDrop(DragDropEventArgs e)
    {
        e.Handled = true;
        OnDragDrop(this, e);
    }

    internal void OnPointerWheelEvent(PointerWheelEventArgs e) => OnPointerWheel(this, e);

    // note WM_*BUTTON* are still called so we don't need to call them
    internal void OnPointerContactChangedEvent(PointerContactChangedEventArgs e)
    {
        if (e.IsDown)
        {
            if (e.MouseButton == MouseButton.Left)
            {
                Focus();
            }
        }
        OnPointerContactChangedEvent(this, e);

        if (e.IsUp)
        {
            if (DragState != null)
            {
                var button = e.MouseButton; // support for dragmove with X buttons
                if (button.HasValue && DragState.Button == button.Value)
                {
                    CancelDragMove(e);
                }
            }
        }
    }

    // note WM_MOUSEENTER & WM_MOUSELEAVE are sent too
    internal void OnPointerEnter(PointerEnterEventArgs e)
    {
        IsMouseOver = true;
        OnPointerEnter(this, e);
    }

    internal void OnPointerLeave(PointerLeaveEventArgs e)
    {
        IsMouseOver = false;
        OnPointerLeave(this, e);
    }

    internal void OnPointerUpdate(PointerUpdateEventArgs e)
    {
        OnPointerUpdate(this, e);
        IsMouseOver = true;

        if (DragState != null)
        {
            DragState.DeltaX = e.X - DragState.StartX;
            DragState.DeltaY = e.Y - DragState.StartY;
            var pde = new PointerDragEventArgs(e, DragState);
            OnPointerDrag(this, pde);
            if (!pde.Handled)
            {
                OnMouseDrag(this, new DragEventArgs(e.X, e.Y, (POINTER_MOD)e.PointerInfo.dwKeyStates, DragState, e));
            }
        }
    }

    internal void OnMouseWheelEvent(MouseWheelEventArgs e) => OnMouseWheel(this, e);
    internal void OnMouseButtonEvent(uint msg, MouseButtonEventArgs e)
    {
        switch (msg)
        {
            case MessageDecoder.WM_LBUTTONDOWN:
            case MessageDecoder.WM_RBUTTONDOWN:
            case MessageDecoder.WM_MBUTTONDOWN:
            case MessageDecoder.WM_XBUTTONDOWN:
                if (msg == MessageDecoder.WM_LBUTTONDOWN)
                {
                    Focus();
                }
                //Application.Trace("Down [" + this + "]: " + e.ToString());
                OnMouseButtonDown(this, e);
                Window?.HandleMouseDownRepeats(this, msg, e);
                break;

            case MessageDecoder.WM_LBUTTONUP:
            case MessageDecoder.WM_RBUTTONUP:
            case MessageDecoder.WM_MBUTTONUP:
            case MessageDecoder.WM_XBUTTONUP:
                //Application.Trace("Up [" + this + "]: " + e.ToString());
                OnMouseButtonUp(this, e);
                Window!.RemoveMouseDownRepeatTimer(e.Button);
                if (DragState != null)
                {
                    var button = Window.MessageToButton(msg, 0, out _); // support for dragmove with X buttons
                    if (button.HasValue && DragState.Button == button.Value)
                    {
                        CancelDragMove(e);
                    }
                }
                break;

            case MessageDecoder.WM_LBUTTONDBLCLK:
            case MessageDecoder.WM_RBUTTONDBLCLK:
            case MessageDecoder.WM_MBUTTONDBLCLK:
            case MessageDecoder.WM_XBUTTONDBLCLK:
                //Application.Trace("DblClck [" + this + "]: " + e.ToString());
                OnMouseButtonDoubleClick(this, e);
                break;
        }
    }

    internal void OnMouseEvent(uint msg, MouseEventArgs e)
    {
        switch (msg)
        {
            case MessageDecoder.WM_MOUSEMOVE:
                //Application.Trace("WM_MOUSEMOVE [" + this + "]: " + e.ToString());
                OnMouseMove(this, e);
                IsMouseOver = true;

                if (DragState != null)
                {
                    DragState.DeltaX = e.X - DragState.StartX;
                    DragState.DeltaY = e.Y - DragState.StartY;
                    OnMouseDrag(this, new DragEventArgs(e.X, e.Y, e.Keys, DragState, e));
                }
                break;

            case MessageDecoder.WM_MOUSEHOVER:
                //Application.Trace("WM_MOUSEHOVER [" + this + "]: " + e.ToString());
                IsMouseOver = true;
                OnMouseHover(this, e);
                break;

            case MessageDecoder.WM_MOUSELEAVE:
                //Application.Trace("WM_MOUSELEAVE [" + this + "]: " + e.ToString());
                IsMouseOver = false;
                OnMouseLeave(this, e);
                break;

            case Window.WM_MOUSEENTER:
                //Application.Trace("WM_MOUSEENTER [" + this + "]: " + e.ToString());
                IsMouseOver = true;
                OnMouseEnter(this, e);
                break;
        }
    }

    /// <summary>
    /// Called after this visual has been measured.
    /// </summary>
    protected virtual void OnMeasured(object? sender, EventArgs e) => Measured?.Invoke(sender, e);

    /// <summary>
    /// Called after this visual has been arranged.
    /// </summary>
    protected virtual void OnArranged(object? sender, EventArgs e) => Arranged?.Invoke(sender, e);

    /// <summary>
    /// Called after this visual has been rendered.
    /// </summary>
    protected virtual void OnRendered(object? sender, EventArgs e) => Rendered?.Invoke(sender, e);

    /// <summary>
    /// Called when <see cref="IsMouseOver"/> changes.
    /// </summary>
    protected virtual void OnMouseOverChanged(object? sender, ValueEventArgs<bool> e) => MouseOverChanged?.Invoke(sender, e);

    /// <summary>
    /// Called when the focused state of this visual changes.
    /// </summary>
    protected virtual void OnFocusedChanged(object? sender, ValueEventArgs<bool> e) => FocusedChanged?.Invoke(sender, e);

    /// <summary>
    /// Called when the mouse pointer moves over this visual (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseMove(object? sender, MouseEventArgs e) => MouseMove?.Invoke(sender, e);

    /// <summary>
    /// Called when the mouse pointer leaves this visual (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseLeave(object? sender, MouseEventArgs e) => MouseLeave?.Invoke(sender, e);

    /// <summary>
    /// Called when the mouse pointer enters this visual (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseEnter(object? sender, MouseEventArgs e) => MouseEnter?.Invoke(sender, e);

    /// <summary>
    /// Called when the mouse hovers on this visual (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseHover(object? sender, MouseEventArgs e) => MouseHover?.Invoke(sender, e);

    /// <summary>
    /// Called during a drag gesture with legacy mouse input.
    /// </summary>
    protected virtual void OnMouseDrag(object? sender, DragEventArgs e) => MouseDrag?.Invoke(sender, e);

    /// <summary>
    /// Called when the mouse wheel scrolls while hovering this visual (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseWheel(object? sender, MouseWheelEventArgs e) => MouseWheel?.Invoke(sender, e);

    /// <summary>
    /// Called on mouse button press (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseButtonDown(object? sender, MouseButtonEventArgs e) => MouseButtonDown?.Invoke(sender, e);

    /// <summary>
    /// Called on mouse button release (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseButtonUp(object? sender, MouseButtonEventArgs e) => MouseButtonUp?.Invoke(sender, e);

    /// <summary>
    /// Called on mouse button double-click (legacy mouse input).
    /// </summary>
    protected virtual void OnMouseButtonDoubleClick(object? sender, MouseButtonEventArgs e) => MouseButtonDoubleClick?.Invoke(sender, e);

    /// <summary>
    /// Captures the mouse to this visual.
    /// </summary>
    protected virtual void CaptureMouse() => Window?.CaptureMouse(this);

    /// <summary>
    /// Called to dispatch OLE drag-drop events to user code.
    /// </summary>
    protected virtual void OnDragDrop(object? sender, DragDropEventArgs e) => DragDrop?.Invoke(sender, e);

    /// <summary>
    /// Called during a drag gesture using pointer input.
    /// </summary>
    protected virtual void OnPointerDrag(object? sender, PointerDragEventArgs e) => PointerDrag?.Invoke(sender, e);

    /// <summary>
    /// Called when the pointer wheel scrolls.
    /// </summary>
    protected virtual void OnPointerWheel(object? sender, PointerWheelEventArgs e) => PointerWheel?.Invoke(sender, e);

    /// <summary>
    /// Called when a pointer leaves this visual.
    /// </summary>
    protected virtual void OnPointerLeave(object? sender, PointerLeaveEventArgs e) => PointerLeave?.Invoke(sender, e);

    /// <summary>
    /// Called when a pointer enters this visual.
    /// </summary>
    protected virtual void OnPointerEnter(object? sender, PointerEnterEventArgs e) => PointerEnter?.Invoke(sender, e);

    /// <summary>
    /// Called when a pointer updates its position over this visual.
    /// </summary>
    protected virtual void OnPointerUpdate(object? sender, PointerPositionEventArgs e) => PointerUpdate?.Invoke(sender, e);

    /// <summary>
    /// Called when a pointer changes contact state (pressed/released).
    /// </summary>
    protected virtual void OnPointerContactChangedEvent(object? sender, PointerContactChangedEventArgs e) => PointerContactChanged?.Invoke(sender, e);

    /// <summary>
    /// Creates the underlying composition <see cref="ContainerVisual"/> for this visual when attached to a window.
    /// Override to return a different visual type (e.g., <see cref="SpriteVisual"/> or <see cref="ShapeVisual"/>).
    /// </summary>
    protected virtual ContainerVisual? CreateCompositionVisual() => Compositor?.CreateSpriteVisual();

    /// <summary>
    /// Creates the children collection used by this visual. Override to apply collection constraints.
    /// </summary>
    protected virtual BaseObjectCollection<Visual> CreateChildren() => [];

    /// <inheritdoc/>
    protected override bool AreValuesEqual(object? value1, object? value2)
    {
        if (base.AreValuesEqual(value1, value2))
            return true;

        if (value1 is CompositionObject co1 && value2 is CompositionObject co2)
            return CompositionObjectEqualityComparer.Default.Equals(co1, co2);

        return false;
    }

    /// <summary>
    /// Allows a parent to override invalidation propagation coming from a child.
    /// </summary>
    protected virtual internal VisualPropertyInvalidateModes GetInvalidateModes(Visual childVisual, InvalidateMode childMode, VisualPropertyInvalidateModes defaultModes, InvalidateReason reason) => defaultModes;

    /// <summary>
    /// Allows a parent to override how its invalidation affects its own parent.
    /// </summary>
    protected virtual internal VisualPropertyInvalidateModes GetParentInvalidateModes(InvalidateMode mode, VisualPropertyInvalidateModes defaultParentModes, InvalidateReason reason)
    {
        //Application.Trace(this + " mode:" + mode + " defp:" + defaultParentModes + " sizeset:" + IsSizeSet);
        if (IsSizeSet)
            return defaultParentModes;

        return VisualProperty.ToInvalidateModes(mode);
    }

    /// <summary>
    /// Sets a property value with explicit invalidation modes.
    /// </summary>
    protected bool SetPropertyValue(BaseObjectProperty property, object? value, VisualPropertyInvalidateModes modes) => SetPropertyValue(property, value, new VisualSetOptions { InvalidateModes = modes });

    /// <summary>
    /// Overrides property setting to integrate with the invalidation pipeline and cursor/focus stubs.
    /// When an update requires a UI-thread call, this method enforces main thread execution.
    /// </summary>
    /// <param name="property">The property descriptor being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options; when <see cref="VisualSetOptions.InvalidateModes"/> is provided, it overrides default invalidation modes.</param>
    /// <returns>true if the stored value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property = BaseObjectProperty.GetFinal(GetType(), property);
        var im = ((property as VisualProperty)?.InvalidateModes).GetValueOrDefault();

        if (options is VisualSetOptions vso && vso.InvalidateModes.HasValue)
        {
            im = vso.InvalidateModes.Value;
        }

        if (im != VisualPropertyInvalidateModes.None)
        {
            CheckRunningAsMainThread();
        }

        if (!base.SetPropertyValue(property, value, options))
            return false;

        //#if DEBUG
        //            Application.Trace(this + " <" + property.Name + "> new value: " + value);
        //#endif

        if (property == IsFocusableProperty)
        {
            Window?.SetFocusable(this, (bool)value!);
        }

        if (property == CursorProperty)
        {
            Window?.UpdateCursor();
        }

        if (im != VisualPropertyInvalidateModes.None)
        {
            Invalidate(im, new PropertyInvalidateReason(property));
        }
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the current thread is the UI thread associated with the window.
    /// </summary>
    [Browsable(false)]
    public bool IsRunningAsMainThread
    {
        get
        {
            var win = Window;
            return win == null || win.ManagedThreadId == Environment.CurrentManagedThreadId;
        }
    }

    /// <summary>
    /// Throws a <see cref="WiceException"/> when not executing on the UI thread.
    /// </summary>
    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0029: This method must be called on the UI thread."); }

    /// <summary>
    /// Requests invalidation for this visual on the owning window according to the provided <paramref name="modes"/>.
    /// If the visual is not attached to a window, this is a no-op.
    /// </summary>
    /// <param name="modes">One or more invalidation modes to apply.</param>
    /// <param name="reason">Optional reason used for diagnostics and optimization routing.</param>
    public virtual void Invalidate(VisualPropertyInvalidateModes modes, InvalidateReason? reason = null) => Window?.Invalidate(this, modes, reason);

    /// <summary>
    /// Called before this visual is detached from its parent.
    /// </summary>
    protected virtual void OnDetachingFromParent(object? sender, EventArgs e) => DetachingFromParent?.Invoke(sender, e);

    /// <summary>
    /// Called after this visual is detached from its parent.
    /// </summary>
    protected virtual void OnDetachedFromParent(object? sender, EventArgs e) => DetachedFromParent?.Invoke(sender, e);

    /// <summary>
    /// Called before this visual is detached from the composition tree.
    /// </summary>
    protected virtual void OnDetachingFromComposition(object? sender, EventArgs e) => DetachingFromComposition?.Invoke(sender, e);

    /// <summary>
    /// Called after this visual is detached from the composition tree.
    /// </summary>
    protected virtual void OnDetachedFromComposition(object? sender, EventArgs e) => DetachedFromComposition?.Invoke(sender, e);

    /// <summary>
    /// Called when this visual is attached to its logical parent.
    /// </summary>
    protected virtual void OnAttachedToParent(object? sender, EventArgs e) => AttachedToParent?.Invoke(sender, e);

    /// <summary>
    /// Called when this visual is attached to the composition tree.
    /// </summary>
    protected virtual void OnAttachedToComposition(object? sender, EventArgs e) => AttachedToComposition?.Invoke(sender, e);

    /// <summary>
    /// Called when a child visual is added.
    /// </summary>
    protected virtual void OnChildAdded(object? sender, ValueEventArgs<Visual> e) => ChildAdded?.Invoke(sender, e);

    /// <summary>
    /// Called when a child visual is removed.
    /// </summary>
    protected virtual void OnChildRemoved(object? sender, ValueEventArgs<Visual> e) => ChildRemoved?.Invoke(sender, e);

    private void OnChildrenCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CheckRunningAsMainThread();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems == null)
                    break;

                foreach (var item in e.OldItems.OfType<Visual>())
                {
                    if (item == null)
                        continue;

                    item.OnDetachingFromParent(this, EventArgs.Empty);
                    item.Parent = null;
                    item.Level = this is Window ? 0 : 1;
                    item.ClearViewOrders();
                    OnChildRemoved(this, new ValueEventArgs<Visual>(item));
                    item.Invalidate(VisualPropertyInvalidateModes.ParentMeasure, new CollectionChangedInvalidateReason(GetType(), item.GetType(), e.Action));
                    item.OnDetachedFromParent(this, EventArgs.Empty);
                }

                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    add();
                }
                break;

            case NotifyCollectionChangedAction.Add:
                add();
                break;
        }

        void add()
        {
            if (e.NewItems == null)
                return;

            foreach (var item in e.NewItems.OfType<Visual>())
            {
                if (item == null)
                    continue;

                if (item.Parent != null)
                    throw new WiceException("0010: Item '" + item.Name + "' of type " + item.GetType().Name + " is already a children of parent '" + item.Parent.Name + "' of type " + item.Parent.GetType().Name + ".");

                if (AllParents.Contains(item))
                    throw new WiceException("0022: Cannot add item '" + item.Name + "' of type " + item.GetType().Name + " to parent '" + Name + "' of type '" + GetType().Name + "' as it would create a parenting cycle.");

                item.Parent = this;
                item.Level = Level + 1;
                item.ResetViewOrders();
                item.OnAttachedToParent(this, EventArgs.Empty);
                OnChildAdded(this, new ValueEventArgs<Visual>(item));
                item.Invalidate(VisualPropertyInvalidateModes.ParentMeasure, new CollectionChangedInvalidateReason(GetType(), item.GetType(), e.Action));
            }
        }
    }

    private IEnumerable<Visual> SiblingsWithVisual
    {
        get
        {
            var children = Parent?.Children;
            if (children == null)
                return [];

            return children.Where(c => c != this && c.CompositionVisual != null).OrderBy(c => c.ZIndexOrDefault);
        }
    }

    private void EnsureZIndex()
    {
        if (_lastZIndex == ZIndexOrDefault)
            return;

        var cv = CompositionVisual;
        if (cv == null)
            return;

        // note: we don't currently raise detached + detached events here
        cv.Parent.Children.Remove(cv);
        InsertCompositionVisual();
    }

    /// <summary>
    /// Inserts the current composition visual among siblings according to <see cref="ZIndex"/> ordering.
    /// Negative z-index first, then null, then positive (stable relative ordering).
    /// </summary>
    protected virtual void InsertCompositionVisual()
    {
        var pcv = Parent?.CompositionVisual;
        if (pcv == null)
            return;

        var cv = CompositionVisual;
        if (cv == null)
            return;

        // order is
        // 1) negative zindex values
        // 2) null zindex values
        // 3) positive zindex values
        // examples:
        // -10, -1, null, null, 12, 29
        // -2, -1, null, null
        // null, 1, 2

        var zindex = ZIndexOrDefault;
        _lastZIndex = zindex;
        if (zindex < 0)
        {
            foreach (var child in SiblingsWithVisual)
            {
                var cz = child.ZIndexOrDefault;
                if (!cz.HasValue || cz.Value >= 0 || cz.Value > zindex)
                {
                    pcv.Children.InsertBelow(cv, child.CompositionVisual);
                    return;
                }
            }

            pcv.Children.InsertAtBottom(cv);
            return;
        }

        ContainerVisual? last = null;
        if (!zindex.HasValue)
        {
            // get last null or first positive
            foreach (var child in SiblingsWithVisual)
            {
                var cz = child.ZIndexOrDefault;
                if (!cz.HasValue || cz.Value < 0)
                {
                    last = child.CompositionVisual;
                    continue;
                }
                break;
            }
        }
        else
        {
            foreach (var child in SiblingsWithVisual)
            {
                var cz = child.ZIndexOrDefault;
                if (!cz.HasValue || cz.Value < 0 || cz.Value <= zindex)
                {
                    last = child.CompositionVisual;
                    continue;
                }
                break;
            }
        }

        if (last != null)
        {
            pcv.Children.InsertAbove(cv, last);
        }
        else
        {
            pcv.Children.InsertAtTop(cv);
        }
        return;
    }

    /// <summary>
    /// Computes the final size allowed for this visual within <paramref name="finalRect"/>, honoring
    /// width/height and min/max constraints. Margin is preserved in the result.
    /// </summary>
    /// <param name="finalRect">The parent-allocated rectangle including margin.</param>
    /// <returns>A constrained rectangle including margin.</returns>
    public D2D_RECT_F ComputeConstrainedSize(D2D_RECT_F finalRect) => ComputeConstrainedSizeWithoutMargin(finalRect - Margin) + Margin;

    /// <summary>
    /// Computes the constrained rectangle excluding margin, applying width/height, min/max constraints
    /// and alignments for the inner content.
    /// </summary>
    /// <param name="childRect">The available content rectangle without margin.</param>
    /// <returns>The constrained content rectangle without margin.</returns>
    public D2D_RECT_F ComputeConstrainedSizeWithoutMargin(D2D_RECT_F childRect) // w/o margin
    {
        ConstrainWidth(ref childRect, Width);
        ConstrainHeight(ref childRect, Height);

        var mw = MaxWidth;
        if (mw.IsSet() && mw < childRect.Width)
        {
            ConstrainWidth(ref childRect, mw);
        }

        mw = MinWidth;
        if (mw.IsSet() && mw > childRect.Width)
        {
            ConstrainWidth(ref childRect, mw);
        }

        var mh = MaxHeight;
        if (mh.IsSet() && mh < childRect.Height)
        {
            ConstrainHeight(ref childRect, mh);
        }

        mh = MinHeight;
        if (mh.IsSet() && mh > childRect.Height)
        {
            ConstrainHeight(ref childRect, mh);
        }

        return childRect;
    }

    /// <summary>
    /// Arranges the content of the visual within the given final rectangle (excluding margin).
    /// Override to position children or compute internal layouts. Do not include margins.
    /// </summary>
    /// <param name="finalRect">Final rectangle available for content, without margin.</param>
    protected virtual void ArrangeCore(D2D_RECT_F finalRect) // does not include margin
    {
        if (finalRect.IsNotSet)
            throw new ArgumentException(null, nameof(finalRect));

        // do nothing by default
    }

    /// <summary>
    /// Finalizes layout for this visual. Applies constraints, calls <see cref="ArrangeCore(D2D_RECT_F)"/>,
    /// then stores the arranged rectangle including margins. Raises <see cref="Arranged"/>.
    /// </summary>
    /// <param name="finalRect">Final rectangle allocated by the parent, including margin.</param>
    public void Arrange(D2D_RECT_F finalRect) // includes margin
    {
#if DEBUG
        if (DesiredSize.IsInvalid)
        {
            // possible cause: the parent visual's MeasureCore method has been overloaded but doesn't call base.MeasureCore
            Application.Trace("Visual '" + Name + "' of type '" + GetType().FullName + "' cannot be arranged as it was not measured.");
        }
#endif

        _lastArrangeRect = finalRect;
        if (finalRect.IsNotSet)
            throw new ArgumentException(null, nameof(finalRect));

        ResetState(InvalidateMode.Arrange);
        var constrainedRect = ComputeConstrainedSizeWithoutMargin(finalRect - Margin);
        ArrangeCore(constrainedRect);

        var ar = constrainedRect + Margin;
        if (UseLayoutRounding)
        {
            LayoutRound(ref ar);
        }
        ArrangedRect = ar;

        OnArranged(this, EventArgs.Empty);
        //Application.Trace("this: " + this + " fr:" + finalRect + " ar:" + ArrangedRect);
    }

    /// <summary>
    /// Applies layout rounding to the arranged rectangle when <see cref="UseLayoutRounding"/> is true.
    /// </summary>
    protected virtual void LayoutRound(ref D2D_RECT_F rect) => rect = rect.ToRound();

    /// <summary>
    /// When overridden, returns the desired content size (without margin) under the specified constraint.
    /// Returning zeros indicates that the visual will adapt to the parent.
    /// </summary>
    /// <param name="constraint">Maximum size available for content (without margin).</param>
    /// <returns>Desired size (without margin). Defaults to 0,0.</returns>
    protected virtual D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => new(); // does not include margin

    /// <summary>
    /// Measures the desired size for this visual under the given constraint. Excludes margin when calling
    /// <see cref="MeasureCore(D2D_SIZE_F)"/>, then re-applies margin to <see cref="DesiredSize"/>.
    /// Raises <see cref="Measured"/>.
    /// </summary>
    /// <param name="constraint">Available size including margin.</param>
    public void Measure(D2D_SIZE_F constraint) // includes margin
    {
        _lastMeasureSize = constraint;
        ResetState(InvalidateMode.Measure);

        // remove margin before computing inner size
        var childConstraint = constraint - Margin;
        var value = Width;
        if (value.IsSet() && value < childConstraint.width)
        {
            childConstraint.width = value;
        }

        value = Height;
        if (value.IsSet() && value < childConstraint.height)
        {
            childConstraint.height = value;
        }

        value = MaxWidth;
        if (value.IsSet() && value < childConstraint.width)
        {
            childConstraint.width = value;
        }

        value = MinWidth;
        if (value.IsSet() && value > childConstraint.width)
        {
            childConstraint.width = value;
        }

        value = MaxHeight;
        if (value.IsSet() && value < childConstraint.height)
        {
            childConstraint.height = value;
        }

        value = MinHeight;
        if (value.IsSet() && value > childConstraint.height)
        {
            childConstraint.height = value;
        }

        var size = MeasureCore(childConstraint);
        if (size.IsNotSet || size.width < 0 || size.height < 0)
            throw new WiceException("0007: Element named '" + Name + "' of type '" + GetType().Name + "' desired size is invalid: " + size + ".");

        size.width *= Zoom;
        size.height *= Zoom;

        value = Width;
        if (value.IsSet())
        {
            size.width = value;
        }

        value = Height;
        if (value.IsSet())
        {
            size.height = value;
        }

        value = MaxWidth;
        if (value.IsSet() && value < size.width)
        {
            size.width = value;
        }

        value = MinWidth;
        if (value.IsSet() && value > size.width)
        {
            size.width = value;
        }

        value = MaxHeight;
        if (value.IsSet() && value < size.height)
        {
            size.height = value;
        }

        value = MinHeight;
        if (value.IsSet() && value > size.height)
        {
            size.height = value;
        }

        DesiredSize = size + Margin;
        OnMeasured(this, EventArgs.Empty);
    }

    private void ConstrainWidth(ref D2D_RECT_F rc, float width)
    {
        if (!width.IsSet() || rc.Width == width)
            return;

        float w;
        switch (HorizontalAlignment)
        {
            case Alignment.Center:
            case Alignment.Stretch:
                w = (rc.Width - width) / 2f;
                if (w > 0)
                {
                    rc.left += w;
                }
                break;

            case Alignment.Far:
                w = rc.Width - width;
                if (w > 0)
                {
                    rc.left += w;
                }
                break;
        }
        rc.Width = width;
    }

    private void ConstrainHeight(ref D2D_RECT_F rc, float height)
    {
        if (!height.IsSet() || rc.Height == height)
            return;

        float h;
        switch (VerticalAlignment)
        {
            case Alignment.Center:
            case Alignment.Stretch:
                h = (rc.Height - height) / 2f;
                if (h > 0)
                {
                    rc.top += h;
                }
                break;

            case Alignment.Far:
                h = rc.Height - height;
                if (h > 0)
                {
                    rc.top += h;
                }
                break;
        }
        rc.Height = height;
    }

    private bool SetVisibility(ContainerVisual cv, bool visible)
    {
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.IsVisible))
        {
            cv.IsVisible = visible;
        }

        if (!visible)
        {
            Window?.RemoveVisual(this);
        }
        return visible;
    }

    /// <summary>
    /// Called before a render pass for a given child visual. Override to apply pre-child settings.
    /// </summary>
    protected virtual internal void BeforeRenderChildCore(RenderContext context, RenderVisual child)
    {
        // do nothing by default
    }

    /// <summary>
    /// Called after a render pass for a given child visual. Override to apply post-child settings.
    /// </summary>
    protected virtual internal void AfterRenderChildCore(RenderContext context, RenderVisual child)
    {
        // do nothing by default
    }

    /// <summary>
    /// Sets the composition visual size and offset from the current <see cref="RelativeRenderRect"/>
    /// and <see cref="RenderOffset"/> unless those parts are suspended.
    /// </summary>
    /// <param name="visual">Target composition visual to configure.</param>
    protected virtual void SetCompositionVisualSizeAndOffset(ContainerVisual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        var rr = RelativeRenderRect;
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Size))
        {
            visual.Size = rr.Size.ToVector2();
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Offset))
        {
            visual.Offset = RenderOffset + new Vector3(rr.left, rr.top, 0);
        }
    }

    internal void InternalRender()
    {
        Render();
        OnRendered(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies layout and composition changes for this visual. This method must be called
    /// for all visuals (including those not visible) so that hit testing and transforms remain accurate.
    /// </summary>
    protected virtual void Render()
    {
        ResetState(InvalidateMode.Render);
        var cv = CompositionVisual;
        if (cv == null)
            return;

        if (!SetVisibility(cv, IsVisible && ArrangedRect.IsValid))
            return;

        var withoutMargin = ArrangedRect - Margin;
        var withoutMarginSize = withoutMargin.Size;
        if (!SetVisibility(cv, !withoutMarginSize.IsEmpty))
            return;

        var rr = new D2D_RECT_F(withoutMargin.left, withoutMargin.top, withoutMarginSize);
        RelativeRenderRect = rr;

        SetCompositionVisualSizeAndOffset(cv);

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Clip) && FinalClipFromParent)
        {
            InsetClip? clip = null;

            // note the Window (root parent of all) implicitely clips
            var parent = Parent;
            if (parent != null && parent.CompositionVisual != null && parent.ClipChildren)
            {
                var newWidth = cv.Size.X;
                var newHeight = cv.Size.Y;
                var offset = cv.Offset;
                var pcv = parent.CompositionVisual.Size;

                // add parent clip
                if (parent.CompositionVisual.Clip is InsetClip pclip)
                {
                    pcv.X -= pclip.RightInset;
                    pcv.Y -= pclip.BottomInset;
                }

                var deltax = offset.X + newWidth - pcv.X;
                if (deltax > 0)
                {
                    newWidth -= deltax;
                    if (!SetVisibility(cv, newWidth > 0))
                        return;
                }
                else
                {
                    deltax = 0;
                }

                var deltay = offset.Y + newHeight - pcv.Y;
                if (deltay > 0)
                {
                    newHeight -= deltay;
                    if (!SetVisibility(cv, newHeight > 0))
                        return;
                }
                else
                {
                    deltay = 0;
                }

                if (deltax != 0 || deltay != 0)
                {
                    clip = Compositor?.CreateInsetClip(0, 0, deltax, deltay);
                }
            }

            cv.Clip = clip ?? Compositor?.CreateInsetClip(); // 0,0,0,0
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.ZIndex))
        {
            EnsureZIndex();
        }

        // scale
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Scale))
        {
            cv.Scale = RenderScale;
        }

        // rotation
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.RotationAngle))
        {
            cv.RotationAngleInDegrees = RenderRotationAngle;
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.RotationAxis))
        {
            cv.RotationAxis = RenderRotationAxis;
        }

        // transform
        var tm = RenderTransformMatrix;
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.TransformMatrix))
        {
            if (tm.HasValue)
            {
                cv.TransformMatrix = tm.Value;
            }
        }

        // get bounds
        var renderBounds = AbsoluteRenderRect;
        if (renderBounds.IsSet)
        {
            // transform bounds using render/composition transformations
            // note we don't support full hittesting (rotation, geometries, etc.)

            if (cv.Scale.X != 1 || cv.Scale.Y != 1)
            {
                var tx = Matrix4x4.CreateScale(cv.Scale);
                renderBounds = renderBounds.TransformToBounds(ref tx);
            }

            if (cv.Offset.X != 0 || cv.Offset.Y != 0)
            {
                var tx = Matrix4x4.CreateTranslation(cv.Offset);
                renderBounds = renderBounds.TransformToBounds(ref tx);
            }

            if (cv.RotationAngle != 0)
            {
                var tx = Matrix4x4.CreateFromAxisAngle(cv.RotationAxis, cv.RotationAngle);
                if (tm.HasValue)
                {
                    tx *= tm.Value;
                }
                renderBounds = renderBounds.TransformToBounds(ref tx);
            }
            else
            {
                if (tm.HasValue)
                {
                    var tx = tm.Value;
                    renderBounds = renderBounds.TransformToBounds(ref tx);
                }
            }

            if (!renderBounds.IsEmpty)
            {
                var rb = GetHitTestBounds(renderBounds);
                Window?.AddVisual(this, ref rb);
            }
        }

        AbsoluteRenderBounds = renderBounds;

        // other
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Opacity))
        {
            cv.Opacity = Opacity;
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.CompositeMode))
        {
            cv.CompositeMode = RenderCompositeMode;
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Shadow))
        {
            var shadow = RenderShadow;
            if (cv is SpriteVisual sprite)
            {
                sprite.Shadow = shadow;
            }
            else if (cv is LayerVisual layer)
            {
                layer.Shadow = shadow;
            }
            else if (shadow != null)
                throw new WiceException("0011: Composition Visual of type '" + cv.GetType().Name + "' doesn't support a Shadow.");
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Brushes))
        {
            RenderBrushes();
        }
    }

    /// <summary>
    /// Allows a subclass to customize the hit test bounds; by default returns <paramref name="defaultBounds"/>.
    /// </summary>
    protected virtual D2D_RECT_F GetHitTestBounds(D2D_RECT_F defaultBounds) => defaultBounds;

    /// <summary>
    /// Chooses and applies the appropriate brush to the composition visual, honoring the hover brush and
    /// optionally animating color transitions when both brushes are color brushes.
    /// </summary>
    protected virtual void RenderBrushes()
    {
        var hover = HoverRenderBrush;
        var render = RenderBrush;

        if (IsMouseOver)
        {
            if (hover != null && CanAnimateColor(render, hover, out var to, out var from))
            {
                AnimateColor(to ?? D3DCOLORVALUE.Transparent, from);
            }
            else
            {
                if (hover != null)
                {
                    SetCompositionBrush(hover);
                }
                else
                {
                    SetCompositionBrush(render);
                }
            }
        }
        else
        {
            SetCompositionBrush(render);
        }
    }

    /// <summary>
    /// Sets the provided <paramref name="brush"/> onto the underlying composition visual.
    /// Supports SpriteVisual (Fill) and ShapeVisual (FillBrush for all shapes).
    /// </summary>
    /// <param name="brush">The brush to apply; null clears the brush where applicable.</param>
    protected virtual void SetCompositionBrush(CompositionBrush? brush)
    {
        var cv = CompositionVisual;
        if (cv == null)
            return;

        try
        {
            if (cv is SpriteVisual sprite)
            {
                sprite.Brush = brush;
                return;
            }

            if (cv is ShapeVisual shapeVisual)
            {
                foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
                {
                    shape.FillBrush = brush;
                }
                return;
            }

            if (brush == null)
                return;

            throw new WiceException("0015: A brush of type '" + brush.GetType().Name + "' cannot be set to a visual of type '" + GetType().Name + "' with a composition visual of type '" + cv.GetType().Name + "'.");
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new WiceException("0018: The compositor used is not associated with this window.", ex);
        }
    }

    /// <summary>
    /// Starts a drag move operation. Captures the mouse and returns the created <see cref="DragState"/>.
    /// </summary>
    protected virtual DragState DragMove(MouseButtonEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        if (DragState != null)
            return DragState;

        DragState = CreateDragState(e);
        CaptureMouse();
        return DragState;
    }

    /// <summary>
    /// Cancels the current drag move operation and releases mouse capture.
    /// </summary>
    protected virtual DragState? CancelDragMove(EventArgs e)
    {
        var state = DragState;
        Window.ReleaseMouseCapture();
        if (DragState is IDisposable disp)
        {
            disp.Dispose();
        }

        DragState = null;
        return state;
    }

    /// <summary>
    /// Suspends updates to the specified composition parts during the next <see cref="Render"/> calls.
    /// </summary>
    public virtual void SuspendCompositionUpdateParts(CompositionUpdateParts parts = CompositionUpdateParts.All) => SuspendedCompositionParts |= parts;

    /// <summary>
    /// Resumes updates to the specified composition parts previously suspended via <see cref="SuspendCompositionUpdateParts"/>.
    /// </summary>
    public virtual void ResumeCompositionUpdateParts(CompositionUpdateParts parts = CompositionUpdateParts.All) => SuspendedCompositionParts &= ~parts;
}
