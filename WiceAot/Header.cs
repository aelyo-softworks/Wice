﻿namespace Wice;

public class Header : Canvas, IAccessKeyParent, ISelectable
{
    public static VisualProperty IsSelectedProperty { get; } = VisualProperty.Add(typeof(Header), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);

    public event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;

    private readonly List<AccessKey> _accessKeys = [];

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
        Selection.Name = nameof(Selection);
#endif
        Children.Add(Selection);
        Selection.IsVisible = false;
        Selection.DoWhenAttachedToComposition(() => Selection.RenderBrush = Compositor.CreateColorBrush(Application.CurrentTheme.SelectedColor.ToColor()));

        Panel = CreatePanel();
        Panel.Margin = 10;
        if (Panel == null)
            throw new InvalidOperationException();
#if DEBUG
        Panel.Name = "headerChild";
#endif
        Children.Add(Panel);

        Icon = CreateIcon();
        if (Icon == null)
            throw new InvalidOperationException();

#if DEBUG
        Icon.Name = nameof(Icon);
#endif
        Panel.Children.Add(Icon);

        SelectedButton = CreateSelectedButton();
        if (SelectedButton == null)
            throw new InvalidOperationException();

#if DEBUG
        SelectedButton.Name = nameof(SelectedButton);
#endif
        Dock.SetDockType(SelectedButton, DockType.Right);
        Panel.Children.Add(SelectedButton);

        SelectedButtonText = new TextBox
        {
#if DEBUG
            Name = "selectedButtonText",
#endif
            FontFamilyName = Application.CurrentTheme.SymbolFontName,
            Text = MDL2GlyphResource.ChevronDown,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        SelectedButton.Child = SelectedButtonText;
        SelectedButton.Click += (s, e) => OnSelectedButtonClick(e);

        // from something like that
        // C:\Windows\WinSxS\amd64_microsoft-windows-shell32.resources_31bf3856ad364e35_10.0.19041.964_en-us_378b74c637bce83b\shell32.dll.mui
        var close = WindowsUtilities.LoadString("shell32.dll", 12851);
        var open = WindowsUtilities.LoadString("shell32.dll", 12850);
        SelectedButton.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, IsSelected ? close : open);

        Text = CreateText();
        if (Text == null)
            throw new InvalidOperationException();

#if DEBUG
        Text.Name = nameof(Text);
#endif
        Panel.Children.Add(Text);
        Text.TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER;
        Text.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
    }

    [Category(CategoryBehavior)]
    public bool AutoSelect { get; set; }

    [Category(CategoryBehavior)]
    public bool IsSelected { get => (bool)GetPropertyValue(IsSelectedProperty)!; set => SetPropertyValue(IsSelectedProperty, value); }

    [Browsable(false)]
    public Visual Selection { get; }

    [Browsable(false)]
    public Visual Panel { get; }

    [Browsable(false)]
    public Visual Icon { get; }

    [Browsable(false)]
    public TextBox Text { get; }

    [Browsable(false)]
    public ButtonBase SelectedButton { get; }

    [Browsable(false)]
    public TextBox SelectedButtonText { get; }

    [Category(CategoryBehavior)]
    public virtual IList<AccessKey> AccessKeys => _accessKeys;

    protected virtual Visual CreateSelection() => new Border();
    protected virtual Visual CreatePanel() => new Dock();
    protected virtual Visual CreateIcon() => new();
    protected virtual TextBox CreateText() => new();
    protected virtual ButtonBase CreateSelectedButton() => new();
    protected virtual TextBox CreateSelectedButtonText() => new();

    bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }
    protected virtual bool RaiseIsSelectedChanged { get; set; }

    protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsSelectedProperty)
        {
            if (RaiseIsSelectedChanged)
            {
                OnIsSelectedChanged(this, new ValueEventArgs<bool>(IsSelected));
            }

            if (SelectedButton.IsVisible)
            {
                var target = SelectedButton.Child;
                var rr = target.RelativeRenderRect;
                if (rr.IsValid)
                {
                    var size = target.RelativeRenderRect.Size;
                    target.CompositionVisual.CenterPoint = new Vector3(size.width / 2, size.height / 2, 0);
                    target.SuspendCompositionUpdateParts(CompositionUpdateParts.RotationAngleInDegrees);

                    var compositor = Compositor;
                    if (compositor != null)
                    {
                        compositor.RunScopedBatch(() =>
                        {
                            var animation = compositor.CreateScalarKeyFrameAnimation();
                            animation.Duration = Application.CurrentTheme.SelectedAnimationDuration;
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
                Selection.Width = Application.CurrentTheme.HeaderSelectionWidth;
            }
        }

        return true;
    }

    void IAccessKeyParent.OnAccessKey(KeyEventArgs e)
    {
        if (AccessKeys != null)
        {
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
    }

    protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        OnSelectedButtonClick(e);
        base.OnMouseButtonDown(sender, e);
    }

    protected virtual void OnSelectedButtonClick(EventArgs e)
    {
        if (AutoSelect)
        {
            IsSelected = !IsSelected;
        }
    }
}
