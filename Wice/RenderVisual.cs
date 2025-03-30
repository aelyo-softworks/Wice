﻿using System;
using System.ComponentModel;
using System.Numerics;
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

        private float? _widthMaxed;
        private float? _heightMaxed;

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
                return colorBrush.Color.ToColor();

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

                    BackgroundColor = colorBrush.Color.ToColor();
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

            visual.DrawOnSurface(win.CompositionDevice, dc => RenderContext.WithRenderContext(dc, rc =>
            {
                var transform = dc.GetTransform();
                if (_widthMaxed.HasValue && _heightMaxed.HasValue)
                {
                    dc.SetTransform(transform * D2D_MATRIX_3X2_F.Translation(_widthMaxed.Value, _heightMaxed.Value));
                }

                var parent = Parent;
                parent.BeforeRenderChildCore(rc, this);
                RenderCore(rc);
                parent.AfterRenderChildCore(rc, this);

                if (_widthMaxed.HasValue && _heightMaxed.HasValue)
                {
                    dc.SetTransform(transform);
                }
            }, creationOptions, rect), creationOptions, rect);
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

        protected override void SetCompositionVisualSizeAndOffset(ContainerVisual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            // I don't think it's documented but experience shows sprite visuals (backed by DirectX texture) width or height
            // must be below D2D bitmap limit (which is 16384), with a slight 2px offset
            // (if we declare a visual of 16384, max DirectX, it becomes 16386 at DirectX11 level for some reason...)
            // So if with or height are over this limit, we must use a transform to scale the visual instead of DComp's offset

            var rr = RelativeRenderRect;
            var maxed = false;
            if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Size))
            {
                var max = Window.MaximumBitmapSize - 2;
                var size = rr.Size;
                if (size.width > max)
                {
                    size.width = max;
                    maxed = true;
                }

                if (size.height > max)
                {
                    size.height = max;
                    maxed = true;
                }

                visual.Size = size.ToVector2();
            }

            if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Offset))
            {
                var offset = RenderOffset + new Vector3(rr.left, rr.top, 0);
                if (maxed)
                {
                    // this must be handled by a D2D transform
                    _widthMaxed = offset.X;
                    _heightMaxed = offset.Y;
                }
                else
                {
                    visual.Offset = offset;
                }
            }
        }
    }
}
