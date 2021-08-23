using System;
using System.ComponentModel;
using DirectN;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class Dialog : Popup
    {
        private object _closeButtonClickHandler;
        private Visual _overlay;
        private bool _shown;

        public event EventHandler Closed;
        public event EventHandler<CancelEventArgs> Closing;

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
            Content.Name = "dialogContent";
#endif

            Content.ChildAdded += OnContentChildAdded;
            Content.ChildRemoved += OnContentChildRemoved;

            // we need a margin for the drop shadow so we need to force 1 child only
            //Content.Margin = Application.CurrentTheme.DialogShadowBlurRadius;
            Children.Add(Content);

            DoWhenAttachedToComposition(() => RenderShadow = CreateShadow());
        }

        [Browsable(false)]
        public Visual Content { get; }

        [Category(CategoryBehavior)]
        public virtual bool ShowWindowOverlay { get; set; }

        [Category(CategoryLayout)]
        public virtual float? WindowOverlayOpacity { get; set; }

        [Category(CategoryLayout)]
        public virtual _D3DCOLORVALUE? WindowOverlayColor { get; set; }

        [Category(CategoryBehavior)]
        public virtual bool? Result { get; set; }

        protected virtual void OnClosed(object sender, EventArgs e) => Closed?.Invoke(this, e);
        protected virtual void OnClosing(object sender, CancelEventArgs e) => Closing?.Invoke(this, e);

        protected override BaseObjectCollection<Visual> CreateChildren() => new BaseObjectCollection<Visual>(1);
        protected virtual Visual CreateContent()
        {
            var content = new Canvas();

            // => size dialog to content
            content.MeasureToContent = DimensionOptions.WidthAndHeight;
            content.HorizontalAlignment = Alignment.Center;
            content.VerticalAlignment = Alignment.Center;
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

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(sender, e);
            if (e.Key == VirtualKeys.Escape)
            {
                if (TryClose())
                {
                    Close();
                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseButtonDown(sender, e);
            e.Handled = true; // we capture mouse. should this go into Popup instead?
        }

        protected override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseButtonUp(sender, e);
            e.Handled = true; // we capture mouse. should this go into Popup instead?
        }

        protected virtual bool TryClose()
        {
            var e = new CancelEventArgs();
            OnClosing(this, e);
            return !e.Cancel;
        }

        public virtual void Close() => Compositor?.RunScopedBatch(AnimateRemove, () =>
        {
            OnClosed(this, EventArgs.Empty);
            Remove();
        });

        protected virtual void OnContentChildAdded(object sender, ValueEventArgs<Visual> e)
        {
            if (e.Value is TitleBar tb)
            {
                _closeButtonClickHandler = tb.CloseButton.AddOnClick(OnCloseButtonClick);
            }
        }

        protected virtual void OnContentChildRemoved(object sender, ValueEventArgs<Visual> e)
        {
            if (_closeButtonClickHandler != null && e.Value is TitleBar tb)
            {
                tb.CloseButton.RemoveOnClick(_closeButtonClickHandler);
            }
        }

        protected virtual void OnCloseButtonClick(object sender, EventArgs e)
        {
            if (TryClose())
            {
                Close();
            }
        }

        private void AnimateRemove()
        {
            var func = Compositor.EaseInCubic();
            var opacityAnimation = Compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Duration = Application.CurrentTheme.DialogCloseAnimationDuration;
            opacityAnimation.InsertKeyFrame(1f, 0.5f, func);
            CompositionVisual.StartAnimation(nameof(Windows.UI.Composition.Visual.Opacity), opacityAnimation);
        }

        protected virtual CompositionShadow CreateShadow()
        {
            var compositor = Compositor;
            if (compositor == null)
                return null;

            var shadow = compositor.CreateDropShadow();
            shadow.BlurRadius = Application.CurrentTheme.DialogShadowBlurRadius;
            shadow.Color = Application.CurrentTheme.DialogShadowColor;
            return shadow;
        }

        protected override void OnAttachedToParent(object sender, EventArgs e)
        {
            base.OnAttachedToParent(sender, e);

            if (ShowWindowOverlay)
            {
                var opacity = WindowOverlayOpacity ?? Application.CurrentTheme.DialogWindowOverlayOpacity;
                if (opacity > 0)
                {
                    var overlay = new Border();
#if DEBUG
                    overlay.Name = "dialogOverlay";
#endif
                    overlay.Opacity = opacity;
                    overlay.RenderBrush = Compositor.CreateColorBrush(WindowOverlayColor ?? Application.CurrentTheme.DialogWindowOverlayColor);
                    Parent.Children.InsertBefore(this, overlay);
                    _overlay = overlay;
                }
            }
        }

        protected override void OnDetachingFromParent(object sender, EventArgs e)
        {
            base.OnDetachingFromParent(sender, e);
            _overlay?.Remove();
        }

        protected override void Render()
        {
            base.Render();
            AnimateShow();
        }

        private void AnimateShow()
        {
            // show only once (resizing window causes a reshow)
            if (_shown)
                return;

            var size = CompositionVisual.Size;
            if (size.X == 0 || size.Y == 0)
                return;

            _shown = true;

            SuspendCompositionUpdateParts(CompositionUpdateParts.Opacity);

            Window.Animate(() =>
            {
                var func = Compositor.EaseInCubic();

                var opacityAnimation = Compositor.CreateScalarKeyFrameAnimation();
                opacityAnimation.Duration = Application.CurrentTheme.DialogOpenAnimationDuration;
                opacityAnimation.InsertKeyFrame(0f, 0f, func);
                opacityAnimation.InsertKeyFrame(1f, 1f, func);

                CompositionVisual.StartAnimation(nameof(Windows.UI.Composition.Visual.Opacity), opacityAnimation);
            }, () =>
            {
                ResumeCompositionUpdateParts();
            });
        }

        protected override PlacementParameters CreatePlacementParameters()
        {
            var parameters = base.CreatePlacementParameters();
            var offset = Content.Margin;
            parameters.HorizontalOffset -= offset.left;
            parameters.VerticalOffset -= offset.top;
            return parameters;
        }
    }
}
