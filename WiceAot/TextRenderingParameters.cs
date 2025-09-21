namespace Wice;

/// <summary>
/// Represents a set of DirectWrite text rendering parameters that can be applied to a D2D device context.
/// </summary>
public class TextRenderingParameters
{
    /// <summary>
    /// Gamma correction value used for rendering text.
    /// Corresponds to <c>IDWriteRenderingParams::GetGamma</c>.
    /// </summary>
    public virtual float? Gamma { get; set; }

    /// <summary>
    /// Enhanced contrast for ClearType text.
    /// Corresponds to <c>IDWriteRenderingParams::GetEnhancedContrast</c>.
    /// </summary>
    public virtual float? EnhancedContrast { get; set; }

    /// <summary>
    /// ClearType level (the balance between ClearType and grayscale rendering).
    /// Corresponds to <c>IDWriteRenderingParams::GetClearTypeLevel</c>.
    /// </summary>
    public virtual float? ClearTypeLevel { get; set; }

    /// <summary>
    /// Pixel geometry used for ClearType rendering.
    /// Corresponds to <c>IDWriteRenderingParams::GetPixelGeometry</c>.
    /// </summary>
    public virtual DWRITE_PIXEL_GEOMETRY? PixelGeometry { get; set; }

    /// <summary>
    /// Grayscale enhanced contrast (applies to grayscale text rendering).
    /// Available starting with <c>IDWriteRenderingParams1</c>.
    /// </summary>
    public virtual float? GrayscaleEnhancedContrast { get; set; }

    /// <summary>
    /// Grid-fit mode controlling glyph fitting to pixel or grid.
    /// Available starting with <c>IDWriteRenderingParams2</c>.
    /// </summary>
    public virtual DWRITE_GRID_FIT_MODE? GridFitMode { get; set; }

    /// <summary>
    /// Text rendering mode (version 1).
    /// Available starting with <c>IDWriteRenderingParams3</c>.
    /// </summary>
    public virtual DWRITE_RENDERING_MODE1? Mode { get; set; }

    /// <summary>
    /// Creates and applies DirectWrite text rendering parameters to the specified D2D device context.
    /// </summary>
    /// <param name="monitorHandle">The monitor whose default rendering parameters are used to fill unspecified values.</param>
    /// <param name="context">The device context that will receive the text rendering parameters. Must not be null.</param>
    public virtual void Set(HMONITOR monitorHandle, ID2D1DeviceContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));

        var fac = Application.CurrentResourceManager.DWriteFactory.Object;
        var existing = FromMonitor(monitorHandle);
        var gamma = Gamma ?? existing.Gamma;
        var ec = EnhancedContrast ?? existing.EnhancedContrast;
        var ctl = ClearTypeLevel ?? existing.ClearTypeLevel;
        var pg = PixelGeometry ?? existing.PixelGeometry;
        var mode = Mode ?? existing.Mode;
        if (!gamma.HasValue || !ec.HasValue || !ctl.HasValue || !pg.HasValue || !mode.HasValue)
            return;

        ComObject<IDWriteRenderingParams>? drp = null;
        try
        {
            if ((int)mode.Value > (int)DWRITE_RENDERING_MODE1.DWRITE_RENDERING_MODE1_OUTLINE)
            {
                var gfm = GridFitMode ?? existing.GridFitMode;
                var gec = GrayscaleEnhancedContrast ?? existing.GrayscaleEnhancedContrast;
                if (gfm.HasValue && gec.HasValue)
                {
                    var fac3 = (IDWriteFactory3)fac;
                    fac3.CreateCustomRenderingParams(gamma.Value, ec.Value, gec.Value, ctl.Value, pg.Value, mode.Value, gfm.Value, out var drp3).ThrowOnError();
                    drp = new ComObject<IDWriteRenderingParams>(drp3);
                }
            }
            else if (GridFitMode.HasValue)
            {
                var gec = GrayscaleEnhancedContrast ?? existing.GrayscaleEnhancedContrast;
                if (gec.HasValue)
                {
                    var fac2 = (IDWriteFactory2)fac;
                    fac2.CreateCustomRenderingParams(gamma.Value, ec.Value, gec.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, GridFitMode.Value, out var drp2).ThrowOnError();
                    drp = new ComObject<IDWriteRenderingParams>(drp2);
                }
            }
            else if (GrayscaleEnhancedContrast.HasValue)
            {
                var fac1 = (IDWriteFactory1)fac;
                fac1.CreateCustomRenderingParams(gamma.Value, ec.Value, GrayscaleEnhancedContrast.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, out var drp1).ThrowOnError();
                drp = new ComObject<IDWriteRenderingParams>(drp1);
            }
            else
            {
                Application.CurrentResourceManager.DWriteFactory.Object.CreateCustomRenderingParams(gamma.Value, ec.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, out var drp0).ThrowOnError();
                drp = new ComObject<IDWriteRenderingParams>(drp0);
            }

            if (drp != null)
            {
                context.SetTextRenderingParams(drp.Object);
            }
        }
        finally
        {
            drp?.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the monitor-specific default DirectWrite text rendering parameters.
    /// </summary>
    /// <param name="handle">Handle to the monitor to query.</param>
    /// <returns>A <see cref="TextRenderingParameters"/> instance populated with the monitor defaults.</returns>
    public static TextRenderingParameters FromMonitor(HMONITOR handle)
    {
        Application.CurrentResourceManager.DWriteFactory.Object.CreateMonitorRenderingParams(handle, out var p).ThrowOnError();
        var trp = new TextRenderingParameters
        {
            Gamma = p.GetGamma(),
            EnhancedContrast = p.GetEnhancedContrast(),
            ClearTypeLevel = p.GetClearTypeLevel(),
            PixelGeometry = p.GetPixelGeometry(),
            Mode = (DWRITE_RENDERING_MODE1)p.GetRenderingMode()
        };

        if (p is IDWriteRenderingParams1 p1)
        {
            trp.GrayscaleEnhancedContrast = p1.GetGrayscaleEnhancedContrast();
            if (p is IDWriteRenderingParams2 p2)
            {
                trp.GridFitMode = p2.GetGridFitMode();
                if (p is IDWriteRenderingParams3 p3)
                {
                    trp.Mode = p3.GetRenderingMode1();
                }
            }
        }
        return trp;
    }
}
