namespace Wice;

/// <summary>
/// Modal popup dialog visual.
/// </summary>
public partial class Dialog : Popup
{
    private object? _closeButtonClickHandler;
    private Visual? _overlay;
    private bool _shown;

    /// <summary>
    /// Occurs after the dialog has been closed and removed from the tree.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Occurs when a close has been requested. Handlers can set <see cref="CancelEventArgs.Cancel"/> to prevent closing.
    /// </summary>
    public event EventHandler<CancelEventArgs>? Closing;

    /// <summary>
    /// Initializes a new <see cref="Dialog"/> with centered placement, modal/focusable behavior,
    /// a single content visual sized to its content, and an optional shadow.
    /// </summary>
    public Dialog()
    {
        ShowWindowOverlay = true;
        IsModal = true;
        IsFocusable = true;
        PlacementMode = PlacementMode.Center;

        // this should be transparent too
        Content = CreateContent();
        if (Content == null)
            throw new InvalidOperationException();

#if DEBUG
        Content.Name ??= "dialogContent";
#endif

        Content.ChildAdded += OnContentChildAdded;
        Content.ChildRemoved += OnContentChildRemoved;

        // we need a margin for the drop shadow so we need to force 1 child only
        //Content.Margin = GetWindowTheme().DialogShadowBlurRadius;
        Children.Add(Content);

        DoWhenAttachedToComposition(() => RenderShadow = CreateShadow());
    }

    /// <summary>
    /// Gets the root content visual hosted by the dialog. Defaults to a <see cref="Canvas"/> that measures to its content.
    /// </summary>
    [Browsable(false)]
    public Visual Content { get; }

    /// <summary>
    /// Gets or sets a value indicating whether a window overlay should be inserted behind the dialog when attached to a parent.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool ShowWindowOverlay { get; set; }

    /// <summary>
    /// Gets or sets an explicit overlay opacity to use instead of the theme value.
    /// Ignored when <see cref="ShowWindowOverlay"/> is false or when set to 0 or less.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual float? WindowOverlayOpacity { get; set; }

    /// <summary>
    /// Gets or sets an explicit overlay color to use instead of the theme value.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual D3DCOLORVALUE? WindowOverlayColor { get; set; }

    /// <summary>
    /// Gets or sets the dialog result, typically set by the caller or command handlers before closing.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool? Result { get; set; }

    /// <summary>
    /// Raises the <see cref="Closed"/> event.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnClosed(object? sender, EventArgs e) => Closed?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="Closing"/> event.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Cancelable event arguments.</param>
    protected virtual void OnClosing(object sender, CancelEventArgs e) => Closing?.Invoke(this, e);

    /// <summary>
    /// Forces a one-child collection for the dialog (its content) to reserve margin for the drop shadow.
    /// </summary>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(1);

    /// <summary>
    /// Creates the dialog content visual.
    /// </summary>
    /// <returns>The newly created content visual.</returns>
    protected virtual Visual CreateContent()
    {
        var content = new Canvas
        {
            // => size dialog to content
            MeasureToContent = DimensionOptions.WidthAndHeight,
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center
        };
        content.DoWhenArranged(() =>
        {
            var ar = content.ArrangedRect;
            if (Width.IsNotSet())
            {
                Width = ar.Width;
            }

            if (Height.IsNotSet())
            {
                Height = ar.Height;
            }
        });
        return content;
    }

    /// <summary>
    /// Handles ESC to attempt closing the dialog. Marks the event handled when a close occurs.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Keyboard event data.</param>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(sender, e);
        if (e.Key == VIRTUAL_KEY.VK_ESCAPE)
        {
            if (TryClose())
            {
                Close();
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Captures mouse button down to prevent click-through. Marks the event handled.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Mouse event data.</param>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        base.OnMouseButtonDown(sender, e);
        e.Handled = true; // we capture mouse. should this go into Popup instead?
    }

    /// <summary>
    /// Captures mouse button up to prevent click-through. Marks the event handled.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Mouse event data.</param>
    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        base.OnMouseButtonUp(sender, e);
        e.Handled = true; // we capture mouse. should this go into Popup instead?
    }

    /// <summary>
    /// Invokes <see cref="Closing"/> and returns whether closing is allowed.
    /// </summary>
    /// <returns>True when no handler canceled the operation; otherwise, false.</returns>
    protected virtual bool TryClose()
    {
        var e = new CancelEventArgs();
        OnClosing(this, e);
        return !e.Cancel;
    }

    /// <summary>
    /// Closes the dialog by animating removal and removing it from the visual tree.
    /// </summary>
    public virtual void Close() => Compositor?.RunScopedBatch(AnimateRemove, () =>
    {
        OnClosed(this, EventArgs.Empty);
        Remove();
    });

    /// <summary>
    /// Wires the TitleBar CloseButton click handler when a <see cref="TitleBar"/> child is added to the content.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Child event data.</param>
    protected virtual void OnContentChildAdded(object? sender, ValueEventArgs<Visual> e)
    {
        if (e.Value is TitleBar tb && tb.CloseButton != null)
        {
            _closeButtonClickHandler = tb.CloseButton.AddOnClick(OnCloseButtonClick);
        }
    }

    /// <summary>
    /// Unwires the TitleBar CloseButton click handler when a <see cref="TitleBar"/> child is removed from the content.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Child event data.</param>
    protected virtual void OnContentChildRemoved(object? sender, ValueEventArgs<Visual> e)
    {
        if (_closeButtonClickHandler != null && e.Value is TitleBar tb && tb.CloseButton != null)
        {
            tb.CloseButton.RemoveOnClick(_closeButtonClickHandler);
        }
    }

    /// <summary>
    /// Attempts to close the dialog when the TitleBar CloseButton is clicked.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnCloseButtonClick(object? sender, EventArgs e)
    {
        if (TryClose())
        {
            Close();
        }
    }

    private void AnimateRemove()
    {
        if (Compositor == null || CompositionVisual == null)
            return;

        var func = Compositor.EaseInCubic();
        var opacityAnimation = Compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Duration = GetWindowTheme().DialogCloseAnimationDuration;
        opacityAnimation.InsertKeyFrame(1f, 0.5f, func);
        CompositionVisual.StartAnimation(nameof(Windows.UI.Composition.Visual.Opacity), opacityAnimation);
    }

    /// <summary>
    /// Creates the content drop shadow for the dialog.
    /// </summary>
    /// <returns>The configured <see cref="CompositionShadow"/>, or null when no compositor is available.</returns>
    protected virtual CompositionShadow? CreateShadow()
    {
        var compositor = Compositor;
        if (compositor == null)
            return null;

        var shadow = compositor.CreateDropShadow();
        shadow.BlurRadius = GetWindowTheme().DialogShadowBlurRadius;
        shadow.Color = GetWindowTheme().DialogShadowColor.ToColor();
        return shadow;
    }

    /// <summary>
    /// Inserts an optional window overlay behind the dialog when attached to a parent.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        base.OnAttachedToParent(sender, e);

        if (ShowWindowOverlay && Compositor != null && Parent != null)
        {
            var opacity = WindowOverlayOpacity ?? GetWindowTheme().DialogWindowOverlayOpacity;
            if (opacity > 0)
            {
                var overlay = new Border
                {
#if DEBUG
                    Name = "dialogOverlay",
#endif
                    Opacity = opacity
                };

                Color color;
                if (WindowOverlayColor != null)
                {
                    color = WindowOverlayColor.Value.ToColor();
                }
                else
                {
                    color = GetWindowTheme().DialogWindowOverlayColor.ToColor();
                }

                overlay.RenderBrush = Compositor.CreateColorBrush(color);
                Parent.Children.InsertBefore(this, overlay);
                _overlay = overlay;
            }
        }
    }

    /// <summary>
    /// Removes the window overlay (if present) when detaching from parent.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnDetachingFromParent(object? sender, EventArgs e)
    {
        base.OnDetachingFromParent(sender, e);
        _overlay?.Remove();
    }

    /// <summary>
    /// Renders the dialog and triggers the one-time show (fade-in) animation.
    /// </summary>
    protected override void Render()
    {
        base.Render();
        AnimateShow();
    }

    private void AnimateShow()
    {
        // show only once (resizing window causes a reshow)
        if (_shown || CompositionVisual == null)
            return;

        var size = CompositionVisual.Size;
        if (size.X == 0 || size.Y == 0)
            return;

        _shown = true;

        SuspendCompositionUpdateParts(CompositionUpdateParts.Opacity);

        Window?.Animate(() =>
        {
            if (Compositor == null)
                return;

            var func = Compositor.EaseInCubic();

            var opacityAnimation = Compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Duration = GetWindowTheme().DialogOpenAnimationDuration;
            opacityAnimation.InsertKeyFrame(0f, 0f, func);
            opacityAnimation.InsertKeyFrame(1f, 1f, func);

            CompositionVisual.StartAnimation(nameof(Windows.UI.Composition.Visual.Opacity), opacityAnimation);
        }, () =>
        {
            ResumeCompositionUpdateParts();
        });
    }

    /// <summary>
    /// Produces placement parameters and compensates the popup position for the content's margin,
    /// so that the visible content aligns with the desired point while preserving shadow padding.
    /// </summary>
    /// <returns>The adjusted placement parameters.</returns>
    protected override PlacementParameters CreatePlacementParameters()
    {
        var parameters = base.CreatePlacementParameters();
        var offset = Content.Margin;
        parameters.HorizontalOffset -= offset.left;
        parameters.VerticalOffset -= offset.top;
        return parameters;
    }
}
