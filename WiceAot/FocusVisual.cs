namespace Wice;

// only (re)created if window loses focus
public partial class FocusVisual : Border//, IModalVisual
{
    public FocusVisual()
    {
        DisableKeyEvents = true;
        DisablePointerEvents = true;
    }

    //public virtual bool IsModal => false;
    public new Window? Parent => (Window?)base.Parent;

    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        if (Parent is null)
            throw new InvalidOperationException();

        base.OnAttachedToParent(sender, e);
    }

    protected internal override VisualPropertyInvalidateModes GetParentInvalidateModes(InvalidateMode mode, VisualPropertyInvalidateModes defaultParentModes, InvalidateReason reason) => VisualPropertyInvalidateModes.None;

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        var im = ((property as VisualProperty)?.InvalidateModes).GetValueOrDefault();

        // kinda hack to avoid invalidating parent (window)
        var mode = VisualProperty.GetInvalidateMode(im);
        if (mode == InvalidateMode.Arrange)
        {
            ArrangeWithParent();
        }
        else if (mode == InvalidateMode.Measure)
        {
            if (Parent != null)
            {
                Measure(Parent.DesiredSize);
                ArrangeWithParent();
            }
        }

        return true;
    }

    // this presumes parent is a Window (which is a Canvas)
    private void ArrangeWithParent()
    {
        if (DesiredSize.IsInvalid)
            return;

        var childRect = Canvas.GetRect(DesiredSize, this);
        Arrange(childRect);
    }

    protected virtual internal void OnUpdateFocus(Visual newFocusedVisual)
    {
        ArgumentNullException.ThrowIfNull(newFocusedVisual);
        if (!newFocusedVisual.IsActuallyVisible)
            throw new ArgumentException(null, nameof(newFocusedVisual));

        if (newFocusedVisual.AbsoluteRenderRect.IsInvalid)
            throw new ArgumentException(null, nameof(newFocusedVisual));

        var focused = newFocusedVisual;
        var parent = newFocusedVisual as IFocusableParent;
        if (parent != null && parent.FocusableVisual != null)
        {
            focused = parent.FocusableVisual;
        }

        if (parent != null && parent.FocusVisualShapeType != null)
        {
            if (Child == null || !parent.FocusVisualShapeType.IsAssignableFrom(Child.GetType()))
            {
                Child = (SingleShape)Activator.CreateInstance(parent.FocusVisualShapeType)!;
            }
        }
        else
        {
            if (Child is not RoundedRectangle)
            {
                Child = new RoundedRectangle();
            }
        }

        Child.DisableKeyEvents = true;
        Child.DisablePointerEvents = true;


#if DEBUG
        Child.Name = nameof(FocusVisual) + ".child";
#endif

        var ar = focused.AbsoluteRenderRect;

        //Application.Trace("focused: " + focused + " ar: " + ar);
        var offset = parent?.FocusOffset ?? Application.CurrentTheme.FocusOffset;

        var l = Canvas.GetLeft(this);
        var t = Canvas.GetTop(this);
        var w = Width;
        var h = Height;

        Canvas.SetLeft(this, ar.left - 1 + offset);
        Canvas.SetTop(this, ar.top - 1 + offset);
        Width = Math.Max(0, ar.Width - offset * 2);
        Height = Math.Max(0, ar.Height - offset * 2);

        //ZIndex = int.MaxValue;
        //Application.Trace("this: " + this + " rc: " + Canvas.GetLeft(this) + " " + Canvas.GetTop(this) + " " + Width + " " + Height);
    }

    protected override void Render()
    {
        base.Render();

        var fv = Window?.FocusedVisual;
        if (fv == null || Compositor == null || CompositionVisual == null)
            return;

        // clip to focused' parent
        var ar = fv.AbsoluteRenderRect;
        var pclip = fv.ParentsAbsoluteClipRect;

        var clip = Compositor.CreateInsetClip();
        if (pclip.HasValue && ar.IsValid)
        {
            clip.LeftInset = Math.Max(0, pclip.Value.left - ar.left);
            clip.TopInset = Math.Max(0, pclip.Value.top - ar.top);
            clip.RightInset = Math.Max(0, ar.right - pclip.Value.right);
            clip.BottomInset = Math.Max(0, ar.bottom - pclip.Value.bottom);
        }

        CompositionVisual.Clip = clip;

        if (Child is SingleShape singleShape)
        {
            singleShape.StrokeBrush = Compositor.CreateColorBrush(Application.CurrentTheme.FocusColor.ToColor());
            singleShape.StrokeThickness = Application.CurrentTheme.FocusThickness;
            singleShape.StrokeDashArray = Application.CurrentTheme.FocusDashArray;
        }

        //Application.Trace("ar: " + ar);
    }
}
