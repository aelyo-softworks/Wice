#if NETFRAMEWORK
using IGraphicsEffectSource = Windows.Graphics.Effects.IGraphicsEffectSource;
#else
using WinRT;
#endif

namespace Wice.Effects;

/// <summary>
/// Builds a CompositionEffectBrush that emulates the Fluent Acrylic material.
/// </summary>
/// <remarks>
/// - Supports two rendering paths:
///   1) Windows Acrylic (HostBackdrop) blur when "useWindowsAcrylic" is true.
///   2) Manual Gaussian blur on Backdrop when "useWindowsAcrylic" is false.
/// - Supports a legacy composition graph for pre-19H1 systems lacking luminosity blend behavior.
/// - Encodes parameters into the CompositionObject.Comment to enable state recovery (e.g., <see cref="GetTintColor"/>).
/// </remarks>
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

        // Same approach as WinUI Acrylic (see linked source). Some drivers/platforms may show quirks.
        var luminosityBlendEffect = new BlendEffect
        {
            // Intended LUMINOSITY, COLOR used here to mitigate platform-specific issues.
            Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_COLOR,
            Background = blurredSource,
            Foreground = luminosityColorEffect
        };

        var colorBlendEffect = new BlendEffect
        {
            Mode = D2D1_BLEND_MODE.D2D1_BLEND_MODE_LUMINOSITY,
            Background = luminosityBlendEffect,
            Foreground = tintColorEffect
        };

        return colorBlendEffect;
    }

    private static float GetTintOpacityModifier(D3DCOLORVALUE tintColor)
    {
#if NETFRAMEWORK
        if (!WinRTUtilities.Is19H1OrHigher)
            return 1f;
#else
        if (!Utilities.Extensions.Is19H1OrHigher)
            return 1f;
#endif

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
        Wice.Utilities.ExceptionExtensions.ThrowIfNull(device, nameof(device));

        const string name = "NoiseAsset_256X256.png";

        using var im = ResourcesUtilities.GetWicBitmapSource(Assembly.GetExecutingAssembly(), n => n.EndsWith(name));
        if (im == null)
            throw new WiceException("0025: Cannot find embedded noise resource '" + name + "'.");

        var noiseDrawingSurface = device.CreateDrawingSurface(im.GetWinSize(), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
        using var interop = noiseDrawingSurface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using var dc = interop.BeginDraw();
        dc.Clear(D3DCOLORVALUE.Transparent);
        using (var bmp = dc.CreateBitmapFromWicBitmap(im))
        {
            dc.DrawBitmap(bmp);
        }

        interop.EndDraw();
        var brush = device.Compositor.CreateSurfaceBrush(noiseDrawingSurface);
        brush.Stretch = CompositionStretch.None;
        brush.HorizontalAlignmentRatio = 0;
        brush.VerticalAlignmentRatio = 0;
        return brush;
    }

    /// <summary>
    /// Creates a CompositionEffectBrush that renders the Acrylic material using the specified tint and behavior.
    /// </summary>
    /// <param name="device">Composition graphics device used to create effects and surfaces.</param>
    /// <param name="tintColor">Base tint color of the Acrylic material.</param>
    /// <param name="tintOpacity">Opacity of the tint (0..1). Combined with internal modifiers on 19H1+.</param>
    /// <param name="tintLuminosityOpacity">Optional override for the luminosity opacity. When set, disables automatic modifier logic.</param>
    /// <param name="useLegacyEffect">Forces the legacy composition graph (pre-19H1 behavior).</param>
    /// <param name="useWindowsAcrylic">
    /// If true, uses the system HostBackdrop blur (Acrylic-like). Requires enabling blur behind the window
    /// (e.g., a call to Window.EnableBlurBehind()) or the brush may render black.
    /// When false, uses a manual Gaussian blur over Backdrop.
    /// </param>
    /// <returns>A configured CompositionEffectBrush ready to be set as a SpriteVisual brush.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="device"/> is null.</exception>
    /// <exception cref="WiceException">Propagated if the noise resource cannot be created or found.</exception>
    public static CompositionEffectBrush CreateAcrylicBrush(
        CompositionGraphicsDevice device,
        D3DCOLORVALUE tintColor,
        float tintOpacity,
        float? tintLuminosityOpacity = null,
        bool useLegacyEffect = false,
        bool useWindowsAcrylic = true)
    {
        Wice.Utilities.ExceptionExtensions.ThrowIfNull(device, nameof(device));

        var effectiveTintColor = GetEffectiveTintColor(tintColor, tintOpacity, tintLuminosityOpacity);
        var luminosityColor = GetEffectiveLuminosityColor(tintColor, tintOpacity, tintLuminosityOpacity);

        var acrylicBrush = CreateAcrylicBrushWorker(device.Compositor, effectiveTintColor, luminosityColor, useLegacyEffect, useWindowsAcrylic);

        acrylicBrush.SetSourceParameter("Noise", CreateNoiseBrush(device));
        acrylicBrush.Properties.InsertColor("TintColor.Color", effectiveTintColor.ToColor());

#if NETFRAMEWORK
        if (!useLegacyEffect && WinRTUtilities.Is19H1OrHigher)
#else
        if (!useLegacyEffect && Utilities.Extensions.Is19H1OrHigher)
#endif
        {
            acrylicBrush.Properties.InsertColor("LuminosityColor.Color", luminosityColor.ToColor());
        }

        // We use Comment as a key for retrieving brush parameters and for equality checks.
        acrylicBrush.Comment = typeof(AcrylicBrush).Name + tintColor.HtmlString + "\0" + tintOpacity.ToString(CultureInfo.InvariantCulture) + "\0" + tintLuminosityOpacity?.ToString(CultureInfo.InvariantCulture) + "\0" + (useLegacyEffect ? "1" : "0") + "\0" + (useWindowsAcrylic ? "1" : "0");
        return acrylicBrush;
    }

    /// <summary>
    /// Attempts to extract the original tint color encoded in the brush's Comment metadata.
    /// </summary>
    /// <param name="brush">An Acrylic brush previously created by <see cref="CreateAcrylicBrush"/>.</param>
    /// <returns>
    /// The parsed tint color if the Comment field contains a valid encoded color; otherwise, null.
    /// </returns>
    public static D3DCOLORVALUE? GetTintColor(CompositionEffectBrush brush)
    {
        if (brush == null || brush.Comment == null)
            return null;

        if (!brush.Comment.StartsWith(typeof(AcrylicBrush).Name))
            return null;

#if NETFRAMEWORK
        var htmlString = brush.Comment.Substring(typeof(AcrylicBrush).Name.Length);
#else
        var htmlString = brush.Comment[typeof(AcrylicBrush).Name.Length..];
#endif
        var pos = htmlString.IndexOf('\0');
        if (pos > 0)
        {
#if NETFRAMEWORK
            if (D3DCOLORVALUE.TryParseFromName(htmlString.Substring(0, pos), out var colorValue))
#else
            if (D3DCOLORVALUE.TryParseFromName(htmlString[..pos], out var colorValue))
#endif
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
        Wice.Utilities.ExceptionExtensions.ThrowIfNull(compositor, nameof(compositor));

        var effectFactory = CreateAcrylicBrushCompositionEffectFactory(compositor, initialTintColor, initialLuminosityColor, useLegacyEffect, useWindowsAcrylic);
        var acrylicBrush = effectFactory.CreateBrush();

        if (useWindowsAcrylic)
        {
            // Note: requires Window.EnableBlurBehind(); otherwise the brush may render black.
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

    private static CompositionEffectFactory CreateAcrylicBrushCompositionEffectFactory(
                Compositor compositor,
                D3DCOLORVALUE initialTintColor,
                D3DCOLORVALUE initialLuminosityColor,
                bool useLegacyEffect = false,
                bool useWindowsAcrylic = true
                )
    {
        Wice.Utilities.ExceptionExtensions.ThrowIfNull(compositor, nameof(compositor));

        var tintColorEffect = new FloodEffect
        {
            Name = "TintColor",
            Color = initialTintColor
        };

        var animatedProperties = new List<string>
        {
            "TintColor.Color"
        };

        var backdropEffectSourceParameter = new CompositionEffectSourceParameter("Backdrop");

        IGraphicsEffectSource blurredSource;
        if (useWindowsAcrylic)
        {
#if NETFRAMEWORK
            blurredSource = backdropEffectSourceParameter.ComCast<IGraphicsEffectSource>();
#else
            blurredSource = backdropEffectSourceParameter.As<IGraphicsEffectSource>();
#endif
        }
        else
        {
            var gaussianBlurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BorderMode = D2D1_BORDER_MODE.D2D1_BORDER_MODE_HARD,
                StandardDeviation = _blurRadius,
#if NETFRAMEWORK
                Source = backdropEffectSourceParameter.ComCast<IGraphicsEffectSource>()
#else
                Source = backdropEffectSourceParameter.As<IGraphicsEffectSource>()
#endif
            };
            blurredSource = gaussianBlurEffect;
        }

#if NETFRAMEWORK
        if (!WinRTUtilities.Is19H1OrHigher)
#else
        if (!Utilities.Extensions.Is19H1OrHigher)
#endif
        {
            useLegacyEffect = true;
        }

        var tintOutput = useLegacyEffect ?
            CombineNoiseWithTintEffectLegacy(blurredSource, tintColorEffect) :
            CombineNoiseWithTintEffectLuminosity(blurredSource, tintColorEffect, initialLuminosityColor, animatedProperties);

        // Ensure noise covers all surface by wrapping edges.
        var noiseBorderEffect = new BorderEffect
        {
            EdgeModeX = D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_WRAP,
            EdgeModeY = D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_WRAP,
#if NETFRAMEWORK
            Source = new CompositionEffectSourceParameter("Noise").ComCast<IGraphicsEffectSource>()
#else
            Source = new CompositionEffectSourceParameter("Noise").As<IGraphicsEffectSource>()
#endif
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

        return compositor.CreateEffectFactory(blendEffectOuter, animatedProperties);
    }
}
