﻿namespace Wice;

/// <summary>
/// A dock-based custom title bar visual that emulates the system caption area.
/// Hosts a title text and standard window caption buttons (Minimize, Maximize/Restore, Close),
/// aligns them using <see cref="Dock"/> semantics, and integrates with the <see cref="Window"/>.
/// </summary>
public partial class TitleBar : Dock
{
    private bool _isMain;

    /// <summary>
    /// Occurs after the title bar updates its layout-related metrics and button sizes.
    /// </summary>
    public event EventHandler<ValueEventArgs<SIZE>>? Updated;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleBar"/> class.
    /// Sets the dock position to <see cref="DockType.Top"/>, creates caption buttons and the title,
    /// and adds them to <see cref="Visual.Children"/> in the expected dock order.
    /// </summary>
    public TitleBar()
    {
        Canvas.SetTop(this, 0);
        SetDockType(this, DockType.Top);
        CloseButton = CreateCloseButton();
        if (CloseButton != null)
        {
#if DEBUG
            CloseButton.Name = nameof(CloseButton);
#endif
            SetDockType(CloseButton, DockType.Right);
            Children.Add(CloseButton);
        }

        MaxButton = CreateMaxButton();
        if (MaxButton != null)
        {
#if DEBUG
            MaxButton.Name = nameof(MaxButton);
#endif

            SetDockType(MaxButton, DockType.Right);
            Children.Add(MaxButton);
        }

        MinButton = CreateMinButton();
        if (MinButton != null)
        {
#if DEBUG
            MinButton.Name = nameof(MinButton);
#endif

            SetDockType(MinButton, DockType.Right);
            Children.Add(MinButton);
        }

        Title = CreateTitle();
        if (Title != null)
        {
            Title.Margin = GetWindowTheme().TitleBarMargin;
            Title.FontSize = GetWindowTheme().TitleBarFontSize;
            Title.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
#if DEBUG
            Title.Name = nameof(Title);
#endif
            SetDockType(Title, DockType.Left);
            Children.Add(Title);

            TitlePadding = CreatePadding();
            if (TitlePadding != null)
            {

#if DEBUG
                TitlePadding.Name = nameof(TitlePadding);
#endif
                SetDockType(TitlePadding, DockType.Left);
                Children.Add(TitlePadding);
            }
        }
    }

    /// <summary>
    /// Gets the text box that displays the window title.
    /// </summary>
    [Browsable(false)]
    public TextBox? Title { get; }

    /// <summary>
    /// Gets a padding visual placed adjacent to <see cref="Title"/> to reserve space
    /// when toggling visibility without affecting docking of neighbors.
    /// </summary>
    [Browsable(false)]
    public Visual? TitlePadding { get; } // so we can say Title.IsVisible = false

    /// <summary>
    /// Gets the minimize caption button visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MinButton { get; }

    /// <summary>
    /// Gets the maximize/restore caption button visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MaxButton { get; }

    /// <summary>
    /// Gets the close caption button visual.
    /// </summary>
    [Browsable(false)]
    public Visual? CloseButton { get; }

    /// <summary>
    /// Enumerates child visuals excluding the standard elements
    /// (<see cref="TitlePadding"/>, <see cref="Title"/>, <see cref="CloseButton"/>, <see cref="MinButton"/>, <see cref="MaxButton"/>).
    /// </summary>
    [Browsable(false)]
    public virtual IEnumerable<Visual> OtherVisuals
    {
        get
        {
            foreach (var child in Children)
            {
                if (child != TitlePadding &&
                    child != Title &&
                    child != CloseButton &&
                    child != MinButton &&
                    child != MaxButton)
                    yield return child;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this title bar instance is the main title bar for the window.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool IsMain
    {
        get => _isMain;
        set
        {
            if (_isMain == value)
                return;

            _isMain = value;
            SetWindowMain();
        }
    }

    private void SetWindowMain()
    {
        var window = Window;
        if (window == null)
            return;

        if (_isMain)
        {
            window.MainTitleBar = this;
            window.Style |= WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_MINIMIZEBOX;
        }
        else
        {
            if (window.MainTitleBar == this)
            {
                window.MainTitleBar = null;
            }
        }
    }

    /// <summary>
    /// Creates the title text box.
    /// </summary>
    /// <returns>A new <see cref="TextBox"/> instance used to display the title.</returns>
    protected virtual TextBox CreateTitle() => new();

    /// <summary>
    /// Creates the padding visual placed next to the title.
    /// </summary>
    /// <returns>A new <see cref="Visual"/> serving as title padding.</returns>
    protected virtual Visual CreatePadding() => new();

    /// <summary>
    /// Creates the minimize button visual.
    /// </summary>
    /// <returns>
    /// A new <see cref="TitleBarButton"/> configured as <see cref="TitleBarButtonType.Minimize"/>.
    /// </returns>
    protected virtual Visual CreateMinButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Minimize };

    /// <summary>
    /// Creates the maximize/restore button visual.
    /// </summary>
    /// <returns>
    /// A new <see cref="TitleBarButton"/> configured as <see cref="TitleBarButtonType.Maximize"/>.
    /// </returns>
    protected virtual Visual CreateMaxButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Maximize };

    /// <summary>
    /// Creates the close button visual.
    /// </summary>
    /// <returns>
    /// A new <see cref="TitleBarButton"/> configured as <see cref="TitleBarButtonType.Close"/>.
    /// </returns>
    protected virtual Visual CreateCloseButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Close };

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var window = Window;
        if (window != null)
        {
            var buttonSize = TitleBarButton.GetDpiAdjustedCaptionButtonSize(window).ToD2D_SIZE_F();
            Height = buttonSize.height;
        }

        var size = base.MeasureCore(constraint);
        return size;
    }

    /// <summary>
    /// Raises the <see cref="Updated"/> event.
    /// </summary>
    /// <param name="sender">The origin of the update.</param>
    /// <param name="e">The update size payload (DPI-adjusted caption size).</param>
    protected virtual void OnUpdated(object sender, ValueEventArgs<SIZE> e) => Updated?.Invoke(sender, e);

    /// <summary>
    /// Performs non-client style hit testing for a frameless window scenario.
    /// </summary>
    /// <param name="bounds">The window-space bounds of the hit test probe.</param>
    /// <returns>
    /// A matching <see cref="HT"/> value when a caption button is hit or the caption area itself,
    /// otherwise <see langword="null"/> when the hit occurs on an "other" child visual.
    /// </returns>
    protected virtual internal HT? HitTest(D2D_RECT_F bounds)
    {
        if (CloseButton?.AbsoluteRenderBounds.Contains(bounds) == true)
            return HT.HTCLOSE;

        if (MaxButton?.AbsoluteRenderBounds.Contains(bounds) == true)
            return HT.HTMAXBUTTON;

        if (MinButton?.AbsoluteRenderBounds.Contains(bounds) == true)
            return HT.HTMINBUTTON;

        foreach (var other in OtherVisuals)
        {
            if (other.AbsoluteRenderBounds.Contains(bounds))
                return null;
        }

        return HT.HTCAPTION;
    }

    /// <summary>
    /// Updates visual state (sizes, brushes, title text, and button types) to reflect the current
    /// window DPI and zoomed state, and raises <see cref="Updated"/>.
    /// </summary>
    protected virtual internal void Update()
    {
        var window = Window;
        if (window == null || Compositor == null)
            return;

        var size = TitleBarButton.GetDpiAdjustedCaptionButtonSize(window);
        var buttonSize = size.ToD2D_SIZE_F();
        Height = buttonSize.height;

        if (CloseButton != null)
        {
            CloseButton.Height = buttonSize.height;
            CloseButton.Width = buttonSize.width;
            CloseButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        }

        if (MinButton != null)
        {
            MinButton.Height = buttonSize.height;
            MinButton.Width = buttonSize.width;
            MinButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
        }

        var zoomed = false;
        if (Title != null)
        {
            if (Parent is ITitleBarParent tbp)
            {
                Title.Text = tbp.Title;
                zoomed = tbp.IsZoomed;
            }
            else if (IsMain)
            {
                Title.Text = window.Title;
                zoomed = window.IsZoomed;
            }

            Title.Margin = GetWindowTheme().TitleBarMargin;
            Title.FontSize = GetWindowTheme().TitleBarFontSize;
        }

        if (MaxButton != null)
        {
            MaxButton.Height = buttonSize.height;
            MaxButton.Width = buttonSize.width;
            if (MaxButton is TitleBarButton tbb)
            {
                tbb.ButtonType = zoomed ? TitleBarButtonType.Restore : TitleBarButtonType.Maximize;
            }
            MaxButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
        }

        foreach (var child in OtherVisuals.OfType<ButtonBase>().Where(b => b.UpdateFromTitleBar))
        {
            child.Height = buttonSize.height;
            child.Width = buttonSize.width;
            child.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
        }

        OnUpdated(this, new ValueEventArgs<SIZE>(size));
    }

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Update();
        SetWindowMain();
    }
}
