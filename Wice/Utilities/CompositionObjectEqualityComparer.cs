﻿using System;
using System.Collections.Generic;
using Windows.UI.Composition;
using DirectN;

namespace Wice.Utilities
{
    public class CompositionObjectEqualityComparer : IEqualityComparer<CompositionObject>
    {
        static CompositionObjectEqualityComparer()
        {
            Default = new CompositionObjectEqualityComparer();
        }

        public static CompositionObjectEqualityComparer Default { get; }

        public int GetHashCode(CompositionObject obj) => obj.GetHashCode();

        public virtual bool Equals(CompositionObject x, CompositionObject y)
        {
            if (x == null)
                return y == null;

            if (y == null)
                return false;

            if (x == y)
                return true;

            if (x.GetType() != y.GetType())
                return false;

            if (x is CompositionColorBrush xb)
                return xb.Color.Equals(((CompositionColorBrush)y).Color);

            // there's no other way but use comment
            if (x is CompositionEffectBrush xef)
                return xef.Comment.EqualsIgnoreCase(((CompositionEffectBrush)y).Comment);

            if (x is CompositionLinearGradientBrush xlb)
            {
                var ylb = (CompositionLinearGradientBrush)y;
                return xlb.StartPoint.Equals(ylb.StartPoint) && xlb.EndPoint.Equals(ylb.EndPoint)
                    && equalsGradientBrush(xlb, ylb);
            }

#if NET
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
#else
            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
#endif
            {
                if (x is CompositionRadialGradientBrush xrg)
                {
                    var yrg = (CompositionRadialGradientBrush)y;
                    return xrg.GradientOriginOffset.Equals(yrg.GradientOriginOffset) && xrg.EllipseRadius.Equals(yrg.EllipseRadius) && xrg.EllipseCenter.Equals(yrg.EllipseCenter)
                        && equalsGradientBrush(xrg, yrg);
                }
            }

            if (x is DropShadow xds)
            {
                var yds = (DropShadow)y;
                return xds.BlurRadius == yds.BlurRadius && xds.Color.Equals(yds.Color) && Equals(xds.Mask, yds.Mask) &&
                    xds.Offset.Equals(yds.Offset) && xds.Opacity == yds.Opacity && xds.SourcePolicy == yds.SourcePolicy;
            }

            throw new NotImplementedException();

            bool equalsGradientBrush(CompositionGradientBrush b1, CompositionGradientBrush b2) => b1.AnchorPoint.Equals(b2.AnchorPoint) &&
                b1.CenterPoint.Equals(b2.CenterPoint) && b1.ExtendMode == b2.ExtendMode && b1.InterpolationSpace == b2.InterpolationSpace &&
                b1.MappingMode == b2.MappingMode && b1.Offset.Equals(b2.Offset) && b2.RotationAngle == b2.RotationAngle &&
                b1.Scale.Equals(b2.Scale) && b1.TransformMatrix.Equals(b2.TransformMatrix) && equalsColorStops(b1.ColorStops, b2.ColorStops);

            bool equalsColorStop(CompositionColorGradientStop c1, CompositionColorGradientStop c2) => c1.Color.Equals(c2.Color) && c1.Offset.Equals(c2.Offset);
            bool equalsColorStops(CompositionColorGradientStopCollection c1, CompositionColorGradientStopCollection c2)
            {
                if (c1.Count != c2.Count)
                    return false;

                for (int i = 0; i < c1.Count; i++)
                {
                    if (!equalsColorStop(c1[i], c2[i]))
                        return false;
                }
                return true;
            }
        }
    }
}
