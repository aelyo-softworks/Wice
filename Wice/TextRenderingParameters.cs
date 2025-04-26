using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice
{
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

        public virtual void Set(IntPtr monitorHandle, ID2D1DeviceContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var fac = Application.CurrentResourceManager.DWriteFactory.Object;
            var existing = FromMonitor(monitorHandle);
            var gamma = Gamma ?? existing.Gamma;
            var ec = EnhancedContrast ?? existing.EnhancedContrast;
            var ctl = ClearTypeLevel ?? existing.ClearTypeLevel;
            var pg = PixelGeometry ?? existing.PixelGeometry;
            var mode = Mode ?? existing.Mode;

            IDWriteRenderingParams drp = null;
            try
            {
                if (Mode.HasValue && (int)Mode.Value > (int)DWRITE_RENDERING_MODE1.DWRITE_RENDERING_MODE1_OUTLINE)
                {
                    var gec = GrayscaleEnhancedContrast ?? existing.GrayscaleEnhancedContrast;
                    var gfm = GridFitMode ?? existing.GridFitMode;
                    var fac3 = (IDWriteFactory3)fac;
                    fac3.CreateCustomRenderingParams(gamma.Value, ec.Value, gec.Value, ctl.Value, pg.Value, mode.Value, gfm.Value, out var drp3).ThrowOnError();
                    drp = drp3;
                }
                else if (GridFitMode.HasValue)
                {
                    var gec = GrayscaleEnhancedContrast ?? existing.GrayscaleEnhancedContrast;
                    var fac2 = (IDWriteFactory2)fac;
                    fac2.CreateCustomRenderingParams(gamma.Value, ec.Value, gec.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, GridFitMode.Value, out var drp2).ThrowOnError();
                    drp = drp2;
                }
                else if (GrayscaleEnhancedContrast.HasValue)
                {
                    var fac1 = (IDWriteFactory1)fac;
                    fac1.CreateCustomRenderingParams(gamma.Value, ec.Value, GrayscaleEnhancedContrast.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, out var drp1).ThrowOnError();
                    drp = drp1;
                }
                else
                {
                    Application.CurrentResourceManager.DWriteFactory.Object.CreateCustomRenderingParams(gamma.Value, ec.Value, ctl.Value, pg.Value, (DWRITE_RENDERING_MODE)mode.Value, out drp).ThrowOnError();
                }
                context.SetTextRenderingParams(drp);
            }
            finally
            {
                if (drp != null)
                {
                    Marshal.ReleaseComObject(drp);
                }
            }
        }

        public static TextRenderingParameters FromMonitor(IntPtr handle)
        {
            Application.CurrentResourceManager.DWriteFactory.Object.CreateMonitorRenderingParams(handle, out var p).ThrowOnError();
            var trp = new TextRenderingParameters();
            trp.Gamma = p.GetGamma();
            trp.EnhancedContrast = p.GetEnhancedContrast();
            trp.ClearTypeLevel = p.GetClearTypeLevel();
            trp.PixelGeometry = p.GetPixelGeometry();
            trp.Mode = (DWRITE_RENDERING_MODE1)p.GetRenderingMode();

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
}
