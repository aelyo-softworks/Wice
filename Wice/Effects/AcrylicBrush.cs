using IGraphicsEffectSource = Windows.Graphics.Effects.IGraphicsEffectSource;

namespace Wice.Effects;

public static class AcrylicBrush
{
    private static readonly D3DCOLORVALUE _exclusionColor = D3DCOLORVALUE.FromArgb(26, 255, 255, 255);
    private const float _saturation = 1.25f;
    private const float _blurRadius = 30f;
    private const float _noiseOpacity = 0.02f;

    private static IGraphicsEffectSource CombineNoiseWithTintEffectLegacy(IGraphicsEffectSource blurredSource, IGraphicsEffectSource tintColorEffect)
    {
        var saturationEffect = new SaturationEffect
        {
            Saturation = _saturation,
            Source = blurredSource
        };

        var exclusionColorEffect = new FloodEffect
        {
            Color = _exclusionColor
        };

        var blendEffectInner = new BlendEffect
        {
            Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_EXCLUSION,
            Foreground = exclusionColorEffect,
            Background = saturationEffect
        };

        var compositeEffect = new CompositeStepEffect
        {
            Mode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            Destination = blendEffectInner,
            Source = tintColorEffect
        };

        return compositeEffect;
    }

    private static IGraphicsEffectSource CombineNoiseWithTintEffectLuminosity(
        IGraphicsEffectSource blurredSource,
        IGraphicsEffectSource tintColorEffect,
        D3DCOLORVALUE initialLuminosityColor,
        List<string> animatedProperties
        )
    {
        animatedProperties.Add("LuminosityColor.Color");

        var luminosityColorEffect = new FloodEffect
        {
            Name = "LuminosityColor",
            Color = initialLuminosityColor
        };

        // same as in https://github.com/microsoft/microsoft-ui-xaml/blob/master/dev/Materials/Acrylic/AcrylicBrush.cpp
        // seems there's some bugs around here...

        var luminosityBlendEffect = new BlendEffect
        {
            //luminosityBlendEffect.Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_LUMINOSITY;
            Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_COLOR,
            Background = blurredSource,
            Foreground = luminosityColorEffect
        };

        var colorBlendEffect = new BlendEffect
        {
            Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_LUMINOSITY,
            //colorBlendEffect.Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_COLOR;
            Background = luminosityBlendEffect,
            Foreground = tintColorEffect
        };

        return colorBlendEffect;
    }

    private static float GetTintOpacityModifier(D3DCOLORVALUE tintColor)
    {
        if (!WinRTUtilities.Is19H1OrHigher)
            return 1f;

        const float midPoint = 0.50f;
        const float whiteMaxOpacity = 0.45f;
        const float midPointMaxOpacity = 0.90f;
        const float blackMaxOpacity = 0.85f;

        var hsv = Hsv.From(tintColor);
        var opacityModifier = midPointMaxOpacity;

        if (hsv.Value != midPoint)
        {
            float lowestMaxOpacity = midPointMaxOpacity;
            float maxDeviation = midPoint;

            if (hsv.Value > midPoint)
            {
                lowestMaxOpacity = whiteMaxOpacity;
                maxDeviation = 1f - maxDeviation;
            }
            else if (hsv.Value < midPoint)
            {
                lowestMaxOpacity = blackMaxOpacity;
            }

            var maxOpacitySuppression = midPointMaxOpacity - lowestMaxOpacity;
            var deviation = Math.Abs(hsv.Value - midPoint);
            var normalizedDeviation = deviation / maxDeviation;

            if (hsv.Saturation > 0f)
            {
                maxOpacitySuppression *= Math.Max(1f - (hsv.Saturation * 2f), 0f);
            }

            var opacitySuppression = maxOpacitySuppression * normalizedDeviation;
            opacityModifier = midPointMaxOpacity - opacitySuppression;
        }

        return opacityModifier;
    }

    private static D3DCOLORVALUE GetEffectiveTintColor(D3DCOLORVALUE tintColor, float tintOpacity, float? tintLuminosityOpacity)
    {
        if (tintLuminosityOpacity.HasValue)
        {
            tintColor.a *= tintOpacity;
        }
        else
        {
            var tintOpacityModifier = GetTintOpacityModifier(tintColor);
            tintColor.a *= tintOpacity * tintOpacityModifier;
        }

        return tintColor;
    }

    private static D3DCOLORVALUE GetEffectiveLuminosityColor(D3DCOLORVALUE tintColor, float tintOpacity, float? tintLuminosityOpacity)
    {
        tintColor.a *= tintOpacity;
        return GetLuminosityColor(tintColor, tintLuminosityOpacity);
    }

    private static D3DCOLORVALUE GetLuminosityColor(D3DCOLORVALUE tintColor, float? tintLuminosityOpacity)
    {
        if (tintLuminosityOpacity.HasValue)
            return tintColor.ChangeAlpha(tintLuminosityOpacity.Value.Clamp(0f, 1f));

        const float minHsvV = 0.125f;
        const float maxHsvV = 0.965f;

        var hsvTintColor = Hsv.From(tintColor);

        var clampedHsvV = hsvTintColor.Value.Clamp(minHsvV, maxHsvV);
        var hsvLuminosityColor = new Hsv(hsvTintColor.Hue, hsvTintColor.Saturation, clampedHsvV);

        const float minLuminosityOpacity = 0.15f;
        const float maxLuminosityOpacity = 1.03f;

        var luminosityOpacityRangeMax = maxLuminosityOpacity - minLuminosityOpacity;
        var mappedTintOpacity = (tintColor.a * luminosityOpacityRangeMax) + minLuminosityOpacity;

        var color = hsvLuminosityColor.ToD3DCOLORVALUE(Math.Min(mappedTintOpacity, 1f));
        return color;
    }

    private static CompositionSurfaceBrush CreateNoiseBrush(CompositionGraphicsDevice device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        const string name = "NoiseAsset_256X256.png";
        using (var im = ResourcesUtilities.GetWicBitmapSource(Assembly.GetExecutingAssembly(), n => n.EndsWith(name)))
        {
            if (im == null)
                throw new WiceException("0025: Cannot find embedded noise resource '" + name + "'.");

            var noiseDrawingSurface = device.CreateDrawingSurface(im.GetWinSize(), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var dc = noiseDrawingSurface.BeginDraw())
            {
                dc.Clear(D3DCOLORVALUE.Transparent);
                using (var bmp = dc.CreateBitmapFromWicBitmap(im))
                {
                    dc.DrawBitmap(bmp);
                }

                noiseDrawingSurface.EndDraw();
                var brush = device.Compositor.CreateSurfaceBrush(noiseDrawingSurface);
                brush.Stretch = CompositionStretch.None;
                brush.HorizontalAlignmentRatio = 0;
                brush.VerticalAlignmentRatio = 0;
                return brush;
            }
        }
    }

    public static CompositionEffectBrush CreateAcrylicBrush(
        CompositionGraphicsDevice device,
        D3DCOLORVALUE tintColor,
        float tintOpacity,
        float? tintLuminosityOpacity = null,
        bool useLegacyEffect = false,
        bool useWindowsAcrylic = true)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        var effectiveTintColor = GetEffectiveTintColor(tintColor, tintOpacity, tintLuminosityOpacity);
        var luminosityColor = GetEffectiveLuminosityColor(tintColor, tintOpacity, tintLuminosityOpacity);

        var acrylicBrush = CreateAcrylicBrushWorker(device.Compositor, effectiveTintColor, luminosityColor, useLegacyEffect, useWindowsAcrylic);

        acrylicBrush.SetSourceParameter("Noise", CreateNoiseBrush(device));
        acrylicBrush.Properties.InsertColor("TintColor.Color", effectiveTintColor.ToColor());

        if (!useLegacyEffect && WinRTUtilities.Is19H1OrHigher)
        {
            acrylicBrush.Properties.InsertColor("LuminosityColor.Color", luminosityColor.ToColor());
        }

        // we use Comment as key, see CompositionObjectEqualityComparer
        acrylicBrush.Comment = typeof(AcrylicBrush).Name + tintColor.HtmlString + "\0" + tintOpacity.ToString(CultureInfo.InvariantCulture) + "\0" + tintLuminosityOpacity?.ToString(CultureInfo.InvariantCulture) + "\0" + (useLegacyEffect ? "1" : "0") + "\0" + (useWindowsAcrylic ? "1" : "0");
        return acrylicBrush;
    }

    public static D3DCOLORVALUE? GetTintColor(CompositionEffectBrush brush)
    {
        if (brush == null || brush.Comment == null)
            return null;

        if (!brush.Comment.StartsWith(typeof(AcrylicBrush).Name))
            return null;

        var htmlString = brush.Comment.Substring(typeof(AcrylicBrush).Name.Length);
        var pos = htmlString.IndexOf('\0');
        if (pos > 0)
        {
            if (D3DCOLORVALUE.TryParseFromName(htmlString.Substring(0, pos), out var colorValue))
                return colorValue;
        }
        return null;
    }

    private static CompositionEffectBrush CreateAcrylicBrushWorker(
        Compositor compositor,
        D3DCOLORVALUE initialTintColor,
        D3DCOLORVALUE initialLuminosityColor,
        bool useLegacyEffect = false,
        bool useWindowsAcrylic = true
        )
    {
        if (compositor == null)
            throw new ArgumentNullException(nameof(compositor));

        var effectFactory = CreateAcrylicBrushCompositionEffectFactory(compositor, initialTintColor, initialLuminosityColor, useLegacyEffect, useWindowsAcrylic);
        var acrylicBrush = effectFactory.CreateBrush();

        if (useWindowsAcrylic)
        {
            // note: this requires a call to Window.EnableBlurBehind() otherwise, brush will be black
            var hostBackdropBrush = compositor.CreateHostBackdropBrush();
            acrylicBrush.SetSourceParameter("Backdrop", hostBackdropBrush);
        }
        else
        {
            var backdropBrush = compositor.CreateBackdropBrush();
            acrylicBrush.SetSourceParameter("Backdrop", backdropBrush);
        }
        return acrylicBrush;
    }

    // https://github.com/microsoft/microsoft-ui-xaml/blob/master/dev/Materials/Acrylic/AcrylicBrush.cpp
    private static CompositionEffectFactory CreateAcrylicBrushCompositionEffectFactory(
                Compositor compositor,
                D3DCOLORVALUE initialTintColor,
                D3DCOLORVALUE initialLuminosityColor,
                bool useLegacyEffect = false,
                bool useWindowsAcrylic = true
                )
    {
        if (compositor == null)
            throw new ArgumentNullException(nameof(compositor));

        var tintColorEffect = new FloodEffect
        {
            Name = "TintColor",
            Color = initialTintColor
        };

        var animatedProperties = new List<string>();
        animatedProperties.Add("TintColor.Color");

        var backdropEffectSourceParameter = new CompositionEffectSourceParameter("Backdrop");

        IGraphicsEffectSource blurredSource;
        if (useWindowsAcrylic)
        {
            blurredSource = backdropEffectSourceParameter.ComCast<IGraphicsEffectSource>();
        }
        else
        {
            var gaussianBlurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BorderMode = D2D1_BORDER_MODE.D2D1_BORDER_MODE_HARD,
                StandardDeviation = _blurRadius,
                Source = backdropEffectSourceParameter.ComCast<IGraphicsEffectSource>()
            };
            blurredSource = gaussianBlurEffect;
        }

        if (!WinRTUtilities.Is19H1OrHigher)
        {
            useLegacyEffect = true;
        }

        var tintOutput = useLegacyEffect ?
            CombineNoiseWithTintEffectLegacy(blurredSource, tintColorEffect) :
            CombineNoiseWithTintEffectLuminosity(blurredSource, tintColorEffect, initialLuminosityColor, animatedProperties);

        // this is to make sure noise covers all surface
        var noiseBorderEffect = new BorderEffect
        {
            EdgeModeX = D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_WRAP,
            EdgeModeY = D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_WRAP,
            Source = new CompositionEffectSourceParameter("Noise").ComCast<IGraphicsEffectSource>()
        };

        var noiseOpacityEffect = new OpacityEffect
        {
            Name = "NoiseOpacity",
            Opacity = _noiseOpacity,
            Source = noiseBorderEffect
        };

        var blendEffectOuter = new CompositeStepEffect
        {
            Mode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            Destination = tintOutput,
            Source = noiseOpacityEffect
        };

        return compositor.CreateEffectFactory(blendEffectOuter.GetIGraphicsEffect(), animatedProperties);
    }
}
