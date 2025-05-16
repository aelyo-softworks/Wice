using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using DirectN;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class Header : Canvas, IAccessKeyParent, ISelectable
    {
        public static VisualProperty IsSelectedProperty = VisualProperty.Add(typeof(Header), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);
        public static VisualProperty SelectedBrushProperty = VisualProperty.Add<CompositionBrush>(typeof(Header), nameof(SelectedBrush), VisualPropertyInvalidateModes.Render);

        public event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;
        public event EventHandler SelectedButtonClick;
        public event EventHandler CloseButtonClick;

        private readonly List<AccessKey> _accessKeys = new List<AccessKey>();

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

            SelectedButtonText = new TextBox();
#if DEBUG
            SelectedButtonText.Name = "selectedButtonText";
#endif
            SelectedButtonText.FontFamilyName = Application.CurrentTheme.SymbolFontName;
            SelectedButtonText.Text = MDL2GlyphResource.ChevronDown;
            SelectedButtonText.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
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

            CloseButton = CreateCloseButton();
            if (CloseButton != null)
            {
#if DEBUG
                CloseButton.Name = nameof(CloseButton);
#endif
                Panel.Children.Add(CloseButton);
                CloseButton.DoWhenAttachedToComposition(() => CloseButton.RenderBrush = null);
                CloseButton.IsVisible = false;
                CloseButton.Margin = D2D_RECT_F.Thickness(20, 0, 0, 0);
                CloseButton.Icon.Text = MDL2GlyphResource.Cancel;
                CloseButton.Click += (s, e) => OnCloseButtonClick(this, e);
                CloseButton.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, close);
            }
        }

        [Category(CategoryBehavior)]
        public bool AutoSelect { get; set; }

        [Category(CategoryRender)]
        public CompositionBrush SelectedBrush { get => (CompositionBrush)GetPropertyValue(SelectedBrushProperty); set => SetPropertyValue(SelectedBrushProperty, value); }

        [Category(CategoryBehavior)]
        public bool IsSelected { get => (bool)GetPropertyValue(IsSelectedProperty); set => SetPropertyValue(IsSelectedProperty, value); }

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

        [Browsable(false)]
        public Button CloseButton { get; }

        [Category(CategoryBehavior)]
        public virtual IList<AccessKey> AccessKeys => _accessKeys;

        protected virtual Visual CreateSelection() => new Border();
        protected virtual Visual CreatePanel() => new Dock();
        protected virtual Visual CreateIcon() => new Visual();
        protected virtual TextBox CreateText() => new TextBox();
        protected virtual ButtonBase CreateSelectedButton() => new ButtonBase();
        protected virtual TextBox CreateSelectedButtonText() => new TextBox();
        protected virtual Button CreateCloseButton() => new Button();

        bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }
        protected virtual bool RaiseIsSelectedChanged { get; set; }

        protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);
        protected virtual void OnSelectedButtonClick(object sender, EventArgs e) => SelectedButtonClick?.Invoke(sender, e);
        protected virtual void OnCloseButtonClick(object sender, EventArgs e) => CloseButtonClick?.Invoke(sender, e);

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == IsSelectedProperty)
            {
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

                if (RaiseIsSelectedChanged)
                {
                    OnIsSelectedChanged(this, new ValueEventArgs<bool>(IsSelected));
                }
            }

            return true;
        }

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

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
                return;

            OnSelectedButtonClick(e);
            base.OnMouseButtonDown(sender, e);
        }

        protected virtual void OnSelectedButtonClick(EventArgs e)
        {
            OnSelectedButtonClick(this, e);
            if (AutoSelect)
            {
                IsSelected = !IsSelected;
            }
        }
    }
}
