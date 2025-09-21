namespace Wice;

/// <summary>
/// A borderless, non-activating popup window that can be positioned relative to a <see cref="Visual"/> target
/// using a configurable <see cref="PlacementMode"/>. The popup can optionally follow its target on arrange/move,
/// support click-through behavior, and compute coordinates either in screen or window space.
/// </summary>
public partial class PopupWindow : Window
{
    /// <summary>
    /// Identifies the <see cref="PlacementTarget"/> property. Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty PlacementTargetProperty { get; } = VisualProperty.Add<Visual>(typeof(PopupWindow), nameof(PlacementTarget), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Identifies the <see cref="PlacementMode"/> property with default <see cref="Wice.PlacementMode.Relative"/>.
    /// Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty PlacementModeProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(PlacementMode), VisualPropertyInvalidateModes.Measure, PlacementMode.Relative);

    /// <summary>
    /// Identifies the <see cref="HorizontalOffset"/> property (default 0). Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Identifies the <see cref="VerticalOffset"/> property (default 0). Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(VerticalOffset), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Identifies the <see cref="CustomPlacementFunc"/> property. Changing it triggers an arrange invalidation.
    /// </summary>
    public static VisualProperty CustomPlacementFuncProperty { get; } = VisualProperty.Add<Func<PlacementParameters, D2D_POINT_2F>>(typeof(PopupWindow), nameof(CustomPlacementFunc), VisualPropertyInvalidateModes.Arrange);

    /// <summary>
    /// Identifies the <see cref="FollowPlacementTarget"/> property (default false). Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty FollowPlacementTargetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(FollowPlacementTarget), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Identifies the <see cref="ClickThrough"/> property (default false). This does not invalidate layout/render.
    /// </summary>
    public static VisualProperty ClickThroughProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(ClickThrough), VisualPropertyInvalidateModes.None, false);

    /// <summary>
    /// Identifies the <see cref="UseRounding"/> property (default true). Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty UseRoundingProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(UseRounding), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// Identifies the <see cref="UseScreenCoordinates"/> property (default true). Changing it triggers a layout measure invalidation.
    /// </summary>
    public static VisualProperty UseScreenCoordinatesProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(UseScreenCoordinates), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// When true, subscribes to the target arrange and target window move events so that the popup recomputes
    /// placement as the target changes size/position. Default is false.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool FollowPlacementTarget { get => (bool)GetPropertyValue(FollowPlacementTargetProperty)!; set => SetPropertyValue(FollowPlacementTargetProperty, value); }

    /// <summary>
    /// When true, mouse button events received by this popup are translated into the target window's coordinate
    /// space and forwarded, allowing the popup to be visually present while letting clicks pass through.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool ClickThrough { get => (bool)GetPropertyValue(ClickThroughProperty)!; set => SetPropertyValue(ClickThroughProperty, value); }

    /// <summary>
    /// When true, the computed popup coordinates are rounded to whole pixels to avoid sub-pixel blurriness.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool UseRounding { get => (bool)GetPropertyValue(UseRoundingProperty)!; set => SetPropertyValue(UseRoundingProperty, value); }

    /// <summary>
    /// When true, placement computations use screen coordinates (via client-to-screen transforms) instead of window/composition space.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool UseScreenCoordinates { get => (bool)GetPropertyValue(UseScreenCoordinatesProperty)!; set => SetPropertyValue(UseScreenCoordinatesProperty, value); }

    /// <summary>
    /// The visual relative to which the popup is positioned. When not set, <see cref="Place(PlacementParameters)"/>
    /// will fall back to the popup's <see cref="Visual.Parent"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public Visual? PlacementTarget { get => (Visual?)GetPropertyValue(PlacementTargetProperty); set => SetPropertyValue(PlacementTargetProperty, value); }

    /// <summary>
    /// Specifies how to compute the popup location relative to the <see cref="PlacementTarget"/>.
    /// For custom placement, set <see cref="CustomPlacementFunc"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public PlacementMode PlacementMode { get => (PlacementMode)GetPropertyValue(PlacementModeProperty)!; set => SetPropertyValue(PlacementModeProperty, value); }

    /// <summary>
    /// Additional horizontal delta applied to the computed position.
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty)!; set => SetPropertyValue(HorizontalOffsetProperty, value); }

    /// <summary>
    /// Additional vertical delta applied to the computed position.
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty)!; set => SetPropertyValue(VerticalOffsetProperty, value); }

    /// <summary>
    /// Optional function invoked when <see cref="PlacementMode.Custom"/> is selected to compute the popup position.
    /// </summary>
    [Browsable(false)]
    public Func<PlacementParameters, D2D_POINT_2F>? CustomPlacementFunc { get => (Func<PlacementParameters, D2D_POINT_2F>?)GetPropertyValue(CustomPlacementFuncProperty); set => SetPropertyValue(CustomPlacementFuncProperty, value); }

    /// <summary>
    /// Initializes a new <see cref="PopupWindow"/> with no frame, popup style, no resize, and the NOACTIVATE and
    /// NOREDIRECTIONBITMAP extended styles for a lightweight, non-activating popup.
    /// </summary>
    public PopupWindow()
    {
        IsBackground = true;
        WindowsFrameMode = WindowsFrameMode.None;
        Style = WINDOW_STYLE.WS_POPUP;
        IsResizable = false;
        ExtendedStyle |= WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP;
    }

    /// <summary>
    /// Overrides mouse button handling to optionally forward to the placement target's window when
    /// <see cref="ClickThrough"/> is enabled. Coordinates are translated from this window to the target window.
    /// </summary>
    /// <param name="msg">The message identifier.</param>
    /// <param name="e">Mouse button event data.</param>
    internal override void OnMouseButtonEvent(uint msg, MouseButtonEventArgs e)
    {
        base.OnMouseButtonEvent(msg, e);
        if (ClickThrough)
        {
            var win = PlacementTarget?.Window;
            if (win != null)
            {
                var winRect = win.WindowRect;
                var thisRect = WindowRect;
                var winEvt = new MouseButtonEventArgs(e.X + (thisRect.left - winRect.left), e.Y + (thisRect.top - winRect.top), e.Keys, e.Button);
                win.OnMouseButtonEvent(msg, winEvt);
            }
        }
    }

    /// <summary>
    /// Shows the popup, mapping <see cref="SHOW_WINDOW_CMD.SW_SHOW"/> to
    /// <see cref="SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE"/> when the window has <see cref="WINDOW_EX_STYLE.WS_EX_NOACTIVATE"/>.
    /// </summary>
    /// <param name="command">The show command to use (default SW_SHOW).</param>
    /// <returns>true if the window became visible; otherwise false.</returns>
    public override bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW)
    {
        if (ExtendedStyle.HasFlag(WINDOW_EX_STYLE.WS_EX_NOACTIVATE) && command == SHOW_WINDOW_CMD.SW_SHOW)
        {
            command = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE;
        }
        return base.Show(command);
    }

    /// <summary>
    /// Extends base property setting to manage follow/unfollow wiring when <see cref="FollowPlacementTarget"/>
    /// or <see cref="PlacementTarget"/> changes. Unsubscribes before change and conditionally subscribes after.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set behavior flags.</param>
    /// <returns>true if the value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == FollowPlacementTargetProperty || property == PlacementTargetProperty)
        {
            UnfollowTarget();
        }

        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == FollowPlacementTargetProperty)
        {
            if ((bool)value!)
            {
                FollowTarget();
            }
        }

        if (property == PlacementTargetProperty)
        {
            if (value != null && FollowPlacementTarget)
            {
                FollowTarget();
            }
        }

        return true;
    }

    private void UnfollowTarget()
    {
        var target = PlacementTarget;
        if (target == null)
            return;

        target.Arranged -= OnTargetArranged;
        target.DoWhenDetachingFromComposition(() =>
        {
            if (target.Window != null)
            {
                target.Window.Moved -= OnTargetWindowMoved;
            }
        });
    }

    private void FollowTarget()
    {
        var target = PlacementTarget;
        if (target == null)
            return;

        target.Arranged += OnTargetArranged;
        target.DoWhenDetachingFromComposition(() =>
        {
            if (target.Window != null)
            {
                target.Window.Moved += OnTargetWindowMoved;
            }
        });
    }

    private void OnTargetArranged(object? sender, EventArgs e) => Invalidate(VisualPropertyInvalidateModes.Render, new InvalidateReason(GetType()));
    private void OnTargetWindowMoved(object? sender, EventArgs e) => Invalidate(VisualPropertyInvalidateModes.Render, new InvalidateReason(GetType()));

    /// <summary>
    /// Creates the <see cref="PlacementParameters"/> snapshot used by <see cref="Place(PlacementParameters)"/>.
    /// Derived types can override to amend parameters before placement is computed.
    /// </summary>
    /// <returns>A fully populated <see cref="PlacementParameters"/> instance for this popup.</returns>
    protected virtual PlacementParameters CreatePlacementParameters()
    {
        var parameters = new PlacementParameters(this)
        {
            UseScreenCoordinates = UseScreenCoordinates,
            CustomFunc = CustomPlacementFunc,
            HorizontalOffset = HorizontalOffset,
            VerticalOffset = VerticalOffset,
            Mode = PlacementMode,
            Target = PlacementTarget,
            UseRounding = UseRounding
        };
        return parameters;
    }

    /// <summary>
    /// After rendering, computes the desired position using <see cref="Place(PlacementParameters)"/> and moves the native
    /// window if a position is available.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">Event arguments.</param>
    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        var parameters = CreatePlacementParameters();
        var pt = Place(parameters);
        if (pt.IsSet)
        {
            var tp = pt.ToPOINT();
            Move(tp.x, tp.y);
        }
    }

    /// <summary>
    /// Computes a popup location based on the provided <paramref name="parameters"/>.
    /// </summary>
    /// <param name="parameters">Placement inputs including target, mode, offsets, rounding, and coordinate space.</param>
    /// <returns>
    /// A <see cref="D2D_POINT_2F"/> representing the top-left position for the popup. When
    /// <see cref="PlacementParameters.UseRounding"/> is true, coordinates are rounded to whole pixels.
    /// </returns>
    public static D2D_POINT_2F Place(PlacementParameters parameters)
    {
        ExceptionExtensions.ThrowIfNull(parameters, nameof(parameters));
        var target = parameters.Target ?? parameters.Visual.Parent;
        var left = 0f;
        var top = 0f;

        if (parameters.Mode == PlacementMode.Custom && parameters.CustomFunc != null)
        {
            var pt = parameters.CustomFunc(parameters);
            left = pt.x;
            top = pt.y;
        }
        else if (parameters.Mode == PlacementMode.Mouse)
        {
            var pt = NativeWindow.GetCursorPosition();
            left = pt.x;
            top = pt.y;
        }
        else if (target != null)
        {
            D2D_POINT_2F leftTop;
            D2D_POINT_2F rightBottom;
            var tr = target.AbsoluteRenderRect;
            if (parameters.UseScreenCoordinates && target.Window != null)
            {
                leftTop = target.Window.ClientToScreen(tr.LeftTop.ToPOINT()).ToD2D_POINT_2F();
                rightBottom = target.Window.ClientToScreen(tr.RightBottom.ToPOINT()).ToD2D_POINT_2F();
            }
            else
            {
                leftTop = tr.LeftTop;
                rightBottom = tr.RightBottom;
            }

            D2D_RECT_F visualBounds;
            if (parameters.Visual is IContentParent contentParent)
            {
                visualBounds = contentParent.Content.AbsoluteRenderBounds;
            }
            else
            {
                visualBounds = parameters.Visual.AbsoluteRenderBounds;
            }

            switch (parameters.Mode)
            {
                case PlacementMode.Center:
                    left = leftTop.x + (tr.Width - visualBounds.Width) / 2;
                    top = leftTop.y + (tr.Height - visualBounds.Height) / 2;
                    break;

                case PlacementMode.OuterBottomLeft:
                    left = leftTop.x;
                    top = rightBottom.y;
                    break;

                case PlacementMode.OuterRightTop:
                    top = leftTop.y;
                    left = rightBottom.x;
                    break;

                case PlacementMode.OuterLeftTop:
                    left = leftTop.x - visualBounds.Width;
                    top = leftTop.y;
                    break;

                case PlacementMode.OuterTopLeft:
                    left = leftTop.x;
                    top = leftTop.y - visualBounds.Height;
                    break;

                case PlacementMode.OuterLeftTopCorner:
                    left = leftTop.x - visualBounds.Width;
                    top = leftTop.y - visualBounds.Height;
                    break;

                case PlacementMode.OuterRightTopCorner:
                    left = rightBottom.x;
                    top = leftTop.y - visualBounds.Height;
                    break;

                case PlacementMode.OuterLeftBottomCorner:
                    left = rightBottom.x;
                    top = rightBottom.y;
                    break;

                case PlacementMode.OuterRightBottomCorner:
                    left = leftTop.x - visualBounds.Width;
                    top = rightBottom.y;
                    break;

                case PlacementMode.OuterTopCenter:
                    left = leftTop.x + (tr.Width - visualBounds.Width) / 2;
                    top = leftTop.y - visualBounds.Height;
                    break;

                case PlacementMode.OuterTopRight:
                    left = rightBottom.x - visualBounds.Width;
                    top = leftTop.y - visualBounds.Height;
                    break;

                case PlacementMode.OuterRightCenter:
                    top = leftTop.y + (tr.Height - visualBounds.Height) / 2;
                    left = rightBottom.x;
                    break;

                case PlacementMode.OuterRigthBottom:
                    top = rightBottom.y - visualBounds.Height;
                    left = rightBottom.x;
                    break;

                case PlacementMode.OuterBottomCenter:
                    left = leftTop.x + (tr.Width - visualBounds.Width) / 2;
                    top = rightBottom.y;
                    break;

                case PlacementMode.OuterBottomRight:
                    left = rightBottom.x - visualBounds.Width;
                    top = rightBottom.y;
                    break;

                case PlacementMode.OuterLeftCenter:
                    top = leftTop.y + (tr.Height - visualBounds.Height) / 2;
                    left = leftTop.x - visualBounds.Width;
                    break;

                case PlacementMode.OuterLeftBottom:
                    top = rightBottom.y - visualBounds.Height;
                    left = leftTop.x - visualBounds.Width;
                    break;

                case PlacementMode.InnerTopCenter:
                    left = leftTop.x + (tr.Width - visualBounds.Width) / 2;
                    top = leftTop.y;
                    break;

                case PlacementMode.InnerTopRight:
                    left = rightBottom.x - visualBounds.Width;
                    top = leftTop.y;
                    break;

                case PlacementMode.InnerLeftCenter:
                    left = leftTop.x;
                    top = leftTop.y + (tr.Height - visualBounds.Height) / 2;
                    break;

                case PlacementMode.InnerRightCenter:
                    left = rightBottom.x - visualBounds.Width;
                    top = leftTop.y + (tr.Height - visualBounds.Height) / 2;
                    break;

                case PlacementMode.InnerBottomLeft:
                    left = leftTop.x;
                    top = rightBottom.y - visualBounds.Height;
                    break;

                case PlacementMode.InnerBottomCenter:
                    left = leftTop.x + (tr.Width - visualBounds.Width) / 2;
                    top = rightBottom.y - visualBounds.Height;
                    break;

                case PlacementMode.InnerBottomRight:
                    left = rightBottom.x - visualBounds.Width;
                    top = rightBottom.y - visualBounds.Height;
                    break;

                case PlacementMode.Relative:
                    left = leftTop.x;
                    top = leftTop.y;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        if (parameters.UseRounding)
            return new D2D_POINT_2F(left.Round(), top.Round());

        return new D2D_POINT_2F(left, top);
    }
}
