using Windows.UI.Composition;

namespace Wice;

public class Caret : Border
{
#if DEBUG
    public static VisualProperty BlinkProperty {get; } = VisualProperty.Add(typeof(Caret), nameof(Blink), VisualPropertyInvalidateModes.Render, true);
#else
    public static VisualProperty BlinkProperty {get; } = VisualProperty.Add(typeof(Caret), nameof(Blink), VisualPropertyInvalidateModes.Render, true);
#endif
    public static VisualProperty IsShownProperty {get; } = VisualProperty.Add(typeof(Caret), nameof(IsShown), VisualPropertyInvalidateModes.Render, false);

    private WindowTimer _blinkTimer;

    public Caret()
    {
        DisableKeyEvents = true;
        DisablePointerEvents = true;
        BlinkTime = (int)WindowsFunctions.GetCaretBlinkTime();
        if (BlinkTime == 0 || BlinkTime == int.MaxValue) // we always want to display a caret
        {
            // default seems 530
            // https://github.com/microsoft/terminal/blob/master/src/interactivity/onecore/SystemConfigurationProvider.hpp#L43
            BlinkTime = 530;
        }
        IsVisible = false;
        ZIndex = int.MaxValue;

        //Opacity = 0.5f;
        //Width = 5;

        DoWhenAttachedToComposition(() => RenderBrush = Compositor.CreateColorBrush(Application.CurrentTheme.CaretColor.ToColor()));
    }

    // no key/mouse at all for caret
    [Category(CategoryBehavior)]
    public virtual int BlinkTime { get; protected set; }

    [Category(CategoryBehavior)]
    public bool Blink { get => (bool)GetPropertyValue(BlinkProperty); set => SetPropertyValue(BlinkProperty, value); }

    [Category(CategoryLayout)]
    public bool IsShown { get => (bool)GetPropertyValue(IsShownProperty); set => SetPropertyValue(IsShownProperty, value); }

    [Category(CategoryLayout)]
    public float FinalWidth
    {
        get
        {
            if (TryGetPropertyValue(WidthProperty, out var obj) && obj is float value)
                return value;

            WindowsFunctions.SystemParametersInfo(WindowsConstants.SPI_GETCARETWIDTH, 0, out var width, false);
            return width;
        }
    }

    [Category(CategoryLayout)]
    public D2D_POINT_2F Location
    {
        get => new(Canvas.GetLeft(this), Canvas.GetTop(this));
        set { Canvas.SetLeft(this, value.x); Canvas.SetTop(this, value.y); DoWhenMeasured(ArrangeWithParent); }
    }

    // this would be too costly, we manage our own life
    protected internal override VisualPropertyInvalidateModes GetParentInvalidateModes(InvalidateMode mode, VisualPropertyInvalidateModes defaultParentModes, InvalidateReason reason)
    {
        if (!LastMeasureSize.HasValue)
            return defaultParentModes;

        return VisualPropertyInvalidateModes.None;
    }

    // this presumes parent is a Window (which is a Canvas)
    private void ArrangeWithParent()
    {
        //Application.Trace(this + " Ds:" + DesiredSize);
        if (DesiredSize.IsInvalid)
            return;

        var childRect = Canvas.GetRect(DesiredSize, this);
        Arrange(childRect);
    }

    public virtual void StopBlinking()
    {
        try
        {
            _blinkTimer?.Change(Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public virtual void StartBlinking()
    {
        if (Blink)
        {
            _blinkTimer?.Change(BlinkTime);
        }
    }

    private void OnCaretBlink()
    {
        if (!Blink || !IsShown)
            return;

        IsVisible = !IsVisible;
        StartBlinking();
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsShownProperty)
        {
            if ((bool)value)
            {
                IsVisible = true;
                StartBlinking();
            }
            else
            {
                WindowsFunctions.DestroyCaret(); // mostly for UI automation support
                StopBlinking();
                IsVisible = false;
            }
            return true;
        }
        return true;
    }

    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        if (Parent is not Wice.Window)
            throw new InvalidOperationException();

        Interlocked.Exchange(ref _blinkTimer, null)?.Dispose();
        _blinkTimer = new WindowTimer(Window, OnCaretBlink); // not started
        base.OnAttachedToParent(sender, e);
    }

    protected override void OnDetachingFromParent(object? sender, EventArgs e)
    {
        Interlocked.Exchange(ref _blinkTimer, null)?.Dispose();
        base.OnDetachingFromParent(sender, e);
    }
}
