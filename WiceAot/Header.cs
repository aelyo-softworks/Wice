namespace Wice;

/// <summary>
/// Represents a header visual composed of an icon, text, an optional close button, and a selectable state.
/// Provides a toggleable selection via a chevron button (animated rotation) or direct mouse click,
/// integrates with access keys, and updates styles based on enabled/disabled and theme/DPI changes.
/// </summary>
public partial class Header : Canvas, IAccessKeyParent, ISelectable
{
    /// <summary>
    /// Identifies the <see cref="IsSelected"/> property.
    /// Changing this property triggers a measure invalidation, selection visuals update,
    /// chevron animation, tooltip update, and may raise <see cref="IsSelectedChanged"/>.
    /// </summary>
    public static VisualProperty IsSelectedProperty { get; } = VisualProperty.Add(typeof(Header), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Identifies the <see cref="SelectedBrush"/> property.
    /// Changing this property triggers a render invalidation.
    /// </summary>
    public static VisualProperty SelectedBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(Header), nameof(SelectedBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Occurs when <see cref="IsSelected"/> changes after internal processing is complete.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    /// <summary>
    /// Occurs when the selected (chevron) button is clicked or when the header is clicked.
    /// </summary>
    public event EventHandler? SelectedButtonClick;

    /// <summary>
    /// Occurs when the optional close button is clicked.
    /// </summary>
    public event EventHandler? CloseButtonClick;

    private readonly List<AccessKey> _accessKeys = [];

    /// <summary>
    /// Initializes a new instance of <see cref="Header"/> and constructs its visual tree.
    /// </summary>
    public Header()
    {
        RaiseIsSelectedChanged = true;
        AutoSelect = true;
        Cursor = Cursor.Hand;
        IsFocusable = true;

        Selection = CreateSelection();
        SetLeft(Selection, 0);
        if (Selection == null)
            throw new InvalidOperationException();

#if DEBUG
        Selection.Name = "headerSelection";
#endif
        Children.Add(Selection);
        Selection.IsVisible = false;
        Selection.DoWhenAttachedToComposition(() => Selection.RenderBrush = Compositor!.CreateColorBrush(Selection.GetWindowTheme().SelectedColor.ToColor()));

        Panel = CreatePanel();
        if (Panel == null)
            throw new InvalidOperationException();
#if DEBUG
        Panel.Name = "headerPanel";
#endif
        Children.Add(Panel);

        Icon = CreateIcon();
        if (Icon == null)
            throw new InvalidOperationException();

#if DEBUG
        Icon.Name = "headerIcon";
#endif
        Panel.Children.Add(Icon);

        SelectedButton = CreateSelectedButton();
        if (SelectedButton == null)
            throw new InvalidOperationException();

#if DEBUG
        SelectedButton.Name = "headerSelectedButton";
#endif
        Dock.SetDockType(SelectedButton, DockType.Right);
        Panel.Children.Add(SelectedButton);

        SelectedButtonText = new TextBox
        {
#if DEBUG
            Name = "headerSelectedButtonText",
#endif
            IsEnabled = false,
            FontFamilyName = GetWindowTheme().SymbolFontName,
            Text = MDL2GlyphResource.ChevronDown,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        SelectedButton.Child = SelectedButtonText;
        SelectedButton.Click += (s, e) => OnSelectedButtonClick(e);

        // from something like that
        // C:\Windows\WinSxS\amd64_microsoft-windows-shell32.resources_31bf3856ad364e35_10.0.19041.964_en-us_378b74c637bce83b\shell32.dll.mui
#if NETFRAMEWORK
        var close = WindowsUtilities.LoadString("shell32.dll", 12851);
        var open = WindowsUtilities.LoadString("shell32.dll", 12850);
#else
        var close = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 12851);
        var open = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 12850);
#endif
        SelectedButton.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, IsSelected ? close : open);

        Text = CreateText();
        if (Text == null)
            throw new InvalidOperationException();

#if DEBUG
        Text.Name = "headerText";
#endif
        Panel.Children.Add(Text);
        Text.TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER;
        Text.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;

        CloseButton = CreateCloseButton();
        if (CloseButton != null)
        {
#if DEBUG
            CloseButton.Name ??= nameof(CloseButton);
#endif
            Panel.Children.Add(CloseButton);
            CloseButton.DoWhenAttachedToComposition(() => CloseButton.RenderBrush = null);
            CloseButton.IsVisible = false;
            CloseButton.Icon.Text = MDL2GlyphResource.Cancel;
            CloseButton.Click += (s, e) => OnCloseButtonClick(this, e);
            CloseButton.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, close);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether clicking the header or selected button
    /// automatically toggles <see cref="IsSelected"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool AutoSelect { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this header is selected.
    /// Triggers selection indicator visibility or chevron animation and raises <see cref="IsSelectedChanged"/> when enabled.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IsSelected { get => (bool)GetPropertyValue(IsSelectedProperty)!; set => SetPropertyValue(IsSelectedProperty, value); }

    /// <summary>
    /// Gets or sets the brush applied to the header background when <see cref="IsSelected"/> is true.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush SelectedBrush { get => (CompositionBrush)GetPropertyValue(SelectedBrushProperty)!; set => SetPropertyValue(SelectedBrushProperty, value); }

    /// <summary>
    /// Gets the selection visual (left border) shown when <see cref="IsSelected"/> is true.
    /// </summary>
    [Browsable(false)]
    public Visual Selection { get; }

    /// <summary>
    /// Gets the panel hosting the header contents (dock layout).
    /// </summary>
    [Browsable(false)]
    public Visual Panel { get; }

    /// <summary>
    /// Gets the icon visual.
    /// </summary>
    [Browsable(false)]
    public Visual Icon { get; }

    /// <summary>
    /// Gets the text visual.
    /// </summary>
    [Browsable(false)]
    public TextBox Text { get; }

    /// <summary>
    /// Gets the selected (chevron) button.
    /// </summary>
    [Browsable(false)]
    public ButtonBase SelectedButton { get; }

    /// <summary>
    /// Gets the chevron text displayed in <see cref="SelectedButton"/>.
    /// </summary>
    [Browsable(false)]
    public TextBox SelectedButtonText { get; }

    /// <summary>
    /// Gets the optional close button (may be null when not provided by subclasses).
    /// </summary>
    [Browsable(false)]
    public Button? CloseButton { get; }

    /// <summary>
    /// Gets the access keys handled by this header. When focused and enabled, a matching key invokes the selected button.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual IList<AccessKey> AccessKeys => _accessKeys;

    /// <summary>
    /// Factory for the selection indicator visual.
    /// </summary>
    protected virtual Visual CreateSelection() => new Border();

    /// <summary>
    /// Factory for the main content panel.
    /// </summary>
    protected virtual Visual CreatePanel() => new Dock();

    /// <summary>
    /// Factory for the icon visual.
    /// </summary>
    protected virtual Visual CreateIcon() => new();

    /// <summary>
    /// Factory for the title text.
    /// </summary>
    protected virtual TextBox CreateText() => new();

    /// <summary>
    /// Factory for the selected (chevron) button.
    /// </summary>
    protected virtual ButtonBase CreateSelectedButton() => new();

    /// <summary>
    /// Factory for the chevron text hosted by <see cref="SelectedButton"/>.
    /// </summary>
    protected virtual TextBox CreateSelectedButtonText() => new();

    /// <summary>
    /// Factory for the optional close button. Default returns a new button; subclasses can return null to hide it.
    /// </summary>
    protected virtual Button? CreateCloseButton() => new();

    /// <summary>
    /// Gets or sets whether <see cref="IsSelectedChanged"/> should be raised when <see cref="IsSelected"/> updates.
    /// </summary>
    bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }

    /// <summary>
    /// Gets or sets whether <see cref="IsSelectedChanged"/> should be raised upon selection change.
    /// </summary>
    protected virtual bool RaiseIsSelectedChanged { get; set; }

    /// <summary>
    /// Raises <see cref="IsSelectedChanged"/>.
    /// </summary>
    protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="SelectedButtonClick"/>.
    /// </summary>
    protected virtual void OnSelectedButtonClick(object sender, EventArgs e) => SelectedButtonClick?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="CloseButtonClick"/>.
    /// </summary>
    protected virtual void OnCloseButtonClick(object sender, EventArgs e) => CloseButtonClick?.Invoke(sender, e);

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        base.MeasureCore(constraint);
        return Panel.DesiredSize;
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsSelectedProperty)
        {
            if (SelectedButton.IsVisible)
            {
                var target = SelectedButton.Child;
                if (target != null)
                {
                    var rr = target.RelativeRenderRect;
                    if (rr.IsValid && target.CompositionVisual != null)
                    {
                        var size = target.RelativeRenderRect.Size;
                        target.CompositionVisual.CenterPoint = new Vector3(size.width / 2, size.height / 2, 0);
                        target.SuspendCompositionUpdateParts(CompositionUpdateParts.RotationAngleInDegrees);

                        var compositor = Compositor;
                        compositor?.RunScopedBatch(() =>
                            {
                                var animation = compositor.CreateScalarKeyFrameAnimation();
                                animation.Duration = GetWindowTheme().SelectedAnimationDuration;
                                animation.InsertKeyFrame(0, 0);
                                animation.InsertKeyFrame(1, IsSelected ? 180 : -180, compositor.CreateLinearEasingFunction());
                                target.CompositionVisual.StartAnimation(nameof(ContainerVisual.RotationAngleInDegrees), animation);
                            }, () =>
                            {
                                target.ResumeCompositionUpdateParts();
                                if (SelectedButton.Child is TextBox tb)
                                {
                                    tb.Text = IsSelected ? MDL2GlyphResource.ChevronUp : MDL2GlyphResource.ChevronDown;
                                }
                            });
                    }
                }
            }
            else
            {
                Selection.IsVisible = IsSelected;
                Selection.Width = GetWindowTheme().HeaderSelectionWidth;
            }

            if (RaiseIsSelectedChanged)
            {
                OnIsSelectedChanged(this, new ValueEventArgs<bool>(IsSelected));
            }
        }
        else if (property == IsEnabledProperty)
        {
            IsFocusable = (bool)value!;
            UpdateStyle();
        }

        return true;
    }

    /// <summary>
    /// Applies enabled/disabled visual style (opacity and cursor) according to the current theme.
    /// </summary>
    protected virtual void UpdateStyle()
    {
        Opacity = IsEnabled ? 1f : GetWindowTheme().DisabledOpacityRatio;
        Cursor = IsEnabled ? Cursor.Hand : null;
    }

    /// <inheritdoc/>
    protected override void RenderBrushes()
    {
        base.RenderBrushes();
        if (IsSelected)
        {
            var sb = SelectedBrush;
            if (sb != null)
            {
                SetCompositionBrush(sb);
            }
        }
    }

    void IAccessKeyParent.OnAccessKey(KeyEventArgs e) => OnAccessKey(e);

    /// <summary>
    /// Handles access key input when focused and enabled. Invokes the selected button if any access key matches.
    /// </summary>
    /// <param name="e">Key event data.</param>
    protected virtual void OnAccessKey(KeyEventArgs e)
    {
        if (AccessKeys == null || !IsEnabled || !IsFocused)
            return;

        foreach (var ak in AccessKeys)
        {
            if (ak.Matches(e))
            {
                OnSelectedButtonClick(e);
                e.Handled = true;
                return;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        OnSelectedButtonClick(e);
        base.OnMouseButtonDown(sender, e);
    }

    /// <summary>
    /// Invokes <see cref="SelectedButtonClick"/> and toggles <see cref="IsSelected"/> when <see cref="AutoSelect"/> is true.
    /// </summary>
    /// <param name="e">Event args that originated the click.</param>
    protected virtual void OnSelectedButtonClick(EventArgs e)
    {
        OnSelectedButtonClick(this, e);
        if (AutoSelect)
        {
            IsSelected = !IsSelected;
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Updates theme-dependent sizes and margins when theme or DPI changes.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Theme/DPI event data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        var theme = GetWindowTheme();
        Selection.Width = theme.HeaderSelectionWidth;
        Panel.Margin = theme.HeaderPanelMargin;
        if (CloseButton != null)
        {
            CloseButton.Margin = theme.HeaderCloseButtonMargin;
        }
    }
}
