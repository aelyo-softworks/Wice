namespace Wice;

public partial class PopupWindow : Window
{
    public static VisualProperty PlacementTargetProperty { get; } = VisualProperty.Add<Visual>(typeof(PopupWindow), nameof(PlacementTarget), VisualPropertyInvalidateModes.Measure);
    public static VisualProperty PlacementModeProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(PlacementMode), VisualPropertyInvalidateModes.Measure, PlacementMode.Relative);
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(VerticalOffset), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty CustomPlacementFuncProperty { get; } = VisualProperty.Add<Func<PlacementParameters, D2D_POINT_2F>>(typeof(PopupWindow), nameof(CustomPlacementFunc), VisualPropertyInvalidateModes.Arrange);
    public static VisualProperty FollowPlacementTargetProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(FollowPlacementTarget), VisualPropertyInvalidateModes.Measure, false);
    public static VisualProperty ClickThroughProperty { get; } = VisualProperty.Add(typeof(PopupWindow), nameof(ClickThrough), VisualPropertyInvalidateModes.None, false);

    public bool FollowPlacementTarget { get => (bool)GetPropertyValue(FollowPlacementTargetProperty)!; set => SetPropertyValue(FollowPlacementTargetProperty, value); }
    public bool ClickThrough { get => (bool)GetPropertyValue(ClickThroughProperty)!; set => SetPropertyValue(ClickThroughProperty, value); }
    public Visual? PlacementTarget { get => (Visual?)GetPropertyValue(PlacementTargetProperty); set => SetPropertyValue(PlacementTargetProperty, value); }
    public PlacementMode PlacementMode { get => (PlacementMode)GetPropertyValue(PlacementModeProperty)!; set => SetPropertyValue(PlacementModeProperty, value); }
    public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty)!; set => SetPropertyValue(HorizontalOffsetProperty, value); }
    public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty)!; set => SetPropertyValue(VerticalOffsetProperty, value); }
    public Func<PlacementParameters, D2D_POINT_2F>? CustomPlacementFunc { get => (Func<PlacementParameters, D2D_POINT_2F>?)GetPropertyValue(CustomPlacementFuncProperty); set => SetPropertyValue(CustomPlacementFuncProperty, value); }

    public PopupWindow()
    {
        IsBackground = true;
        WindowsFrameMode = WindowsFrameMode.None;
        Style = WINDOW_STYLE.WS_POPUP;
        IsResizable = false;
        ExtendedStyle |= WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP;
    }

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

    public override bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW)
    {
        if (ExtendedStyle.HasFlag(WINDOW_EX_STYLE.WS_EX_NOACTIVATE) && command == SHOW_WINDOW_CMD.SW_SHOW)
        {
            command = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE;
        }
        return base.Show(command);
    }

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

    protected virtual PlacementParameters CreatePlacementParameters()
    {
        var parameters = new PlacementParameters(this)
        {
            UseScreenCoordinates = true,
            CustomFunc = CustomPlacementFunc,
            HorizontalOffset = HorizontalOffset,
            VerticalOffset = VerticalOffset,
            Mode = PlacementMode,
            Target = PlacementTarget
        };
        return parameters;
    }

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

    public static D2D_POINT_2F Place(PlacementParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
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
                case PlacementMode.Absolute:
                    left = parameters.HorizontalOffset;
                    top = parameters.VerticalOffset;
                    break;

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
                    // case PlacementMode.Relative:
                    left = leftTop.x;
                    top = leftTop.y;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        if (parameters.Mode != PlacementMode.Absolute)
        {
            // TODO: this is not good, it must be rewritten according to directions
            left += parameters.HorizontalOffset;
            top += parameters.VerticalOffset;
        }

        if (parameters.UseRounding)
            return new D2D_POINT_2F(left.Round(), top.Round());

        return new D2D_POINT_2F(left, top);
    }
}
