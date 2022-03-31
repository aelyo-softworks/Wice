using System;
using System.ComponentModel;
using DirectN;
using Wice.Effects;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public abstract class RenderVisual : Visual
    {
        public static VisualProperty BackgroundColorProperty = VisualProperty.Add<_D3DCOLORVALUE?>(typeof(RenderVisual), nameof(BackgroundColor), VisualPropertyInvalidateModes.Render);

        protected RenderVisual()
        {
        }

        protected virtual tagRECT? Direct2DRenderRect => null;
        protected virtual bool ShouldRender => true;
        protected virtual bool FallbackToTransparentBackground => false;
        protected virtual bool RenderSnapToPixels => true;

        [Category(CategoryRender)]
        public _D3DCOLORVALUE? BackgroundColor { get => (_D3DCOLORVALUE?)GetPropertyValue(BackgroundColorProperty); set => SetPropertyValue(BackgroundColorProperty, value); }

        [Category(CategoryRender)]
        public _D3DCOLORVALUE? AscendantsBackgroundColor => GetAscendantsBackgroundColor(this);

        private static _D3DCOLORVALUE? GetAscendantsBackgroundColor(Visual visual)
        {
            if (visual == null)
                return null;

            if (visual is RenderVisual rv && rv.BackgroundColor.HasValue)
                return rv.BackgroundColor.Value;

            if (visual.RenderBrush is CompositionColorBrush colorBrush)
                return colorBrush.Color;

            if (visual.RenderBrush is CompositionEffectBrush effectBrush)
            {
                var color = AcrylicBrush.GetTintColor(effectBrush);
                if (color.HasValue)
                    return color.Value;
            }

            return GetAscendantsBackgroundColor(visual.Parent);
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (property == RenderBrushProperty)
            {
                var brush = (CompositionBrush)value;
                if (CompositionObjectEqualityComparer.Default.Equals(brush, RenderBrush))
                    return false;

                if (value != null)
                {
                    if (!(value is CompositionColorBrush colorBrush))
                        throw new NotSupportedException();

                    BackgroundColor = colorBrush.Color;
                    return base.SetPropertyValue(property, value, options);
                } // else continue
            }

            return base.SetPropertyValue(property, value, options);
        }

        protected override void Render()
        {
            base.Render();
            RenderD2DSurface(null, null);
        }

        protected virtual void RenderD2DSurface(SurfaceCreationOptions creationOptions = null, tagRECT? rect = null)
        {
            if (!(CompositionVisual is SpriteVisual visual))
                return;

            if (!CompositionVisual.IsVisible)
                return;

            if (SuspendedCompositionParts.HasFlag(CompositionUpdateParts.D2DSurface))
                return;

            var win = Window;
            if (win == null || !ShouldRender)
                return;

            visual.DrawOnSurface(win.CompositionDevice, dc => RenderContext.WithRenderContext(dc, rc => RenderCore(rc), creationOptions, rect), creationOptions, rect);
        }

        protected internal virtual void RenderOverChildren(RenderContext context)
        {
        }

        protected internal virtual void RenderCore(RenderContext context) => RenderBackgroundCore(context);
        protected virtual void RenderBackgroundCore(RenderContext context)
        {
            var bg = BackgroundColor;
            if (bg.HasValue)
            {
                context.DeviceContext.Clear(bg.Value);
            }
            else if (FallbackToTransparentBackground)
            {
                context.DeviceContext.Clear(_D3DCOLORVALUE.Transparent);
            }
        }
    }
}
