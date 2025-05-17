namespace Wice;

public class TextRenderingParameters
{
    // IDWriteRenderingParams
    public virtual float? Gamma { get; set; }
    public virtual float? EnhancedContrast { get; set; }
    public virtual float? ClearTypeLevel { get; set; }
    public virtual DWRITE_PIXEL_GEOMETRY? PixelGeometry { get; set; }

    // IDWriteRenderingParams1
    public virtual float? GrayscaleEnhancedContrast { get; set; }

    // IDWriteRenderingParams2
    public virtual DWRITE_GRID_FIT_MODE? GridFitMode { get; set; }

    // IDWriteRenderingParams3
    // note only DWRITE_RENDERING_MODE1_NATURAL_SYMMETRIC_DOWNSAMPLED is new for Windows 10 and IDWriteRenderingParams3
    public virtual DWRITE_RENDERING_MODE1? Mode { get; set; }

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
