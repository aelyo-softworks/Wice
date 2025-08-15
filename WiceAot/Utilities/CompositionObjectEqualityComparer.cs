namespace Wice.Utilities;

/// <summary>
/// Provides structural equality comparison for <see cref="CompositionObject"/> instances.
/// </summary>
/// <remarks>
/// - Supports the following types:
///   - <see cref="CompositionColorBrush"/>: compares <see cref="CompositionColorBrush.Color"/>.
///   - <see cref="CompositionEffectBrush"/>: compares <see cref="CompositionEffectBrush.Comment"/> (case-insensitive).
///   - <see cref="CompositionLinearGradientBrush"/>: compares geometry and gradient stops.
///   - <see cref="CompositionRadialGradientBrush"/> (Windows 10, 1903+): compares geometry and gradient stops.
///   - <see cref="DropShadow"/>: compares blur radius, color, mask, offset, opacity, and source policy.
/// - For unsupported <see cref="CompositionObject"/> types, <see cref="Equals(CompositionObject?, CompositionObject?)"/> throws <see cref="NotImplementedException"/>.
/// - <see cref="GetHashCode(CompositionObject)"/> delegates to <see cref="object.GetHashCode"/> on the input object and does not compute a structural hash.
///   As a result, equal objects by this comparer might produce different hash codes. Avoid using this comparer as a key selector in hashed collections
///   unless this behavior is acceptable.
/// </remarks>
public class CompositionObjectEqualityComparer : IEqualityComparer<CompositionObject>
{
    /// <summary>
    /// Initializes the <see cref="CompositionObjectEqualityComparer"/> type and assigns the <see cref="Default"/> instance.
    /// </summary>
    static CompositionObjectEqualityComparer()
    {
        Default = new CompositionObjectEqualityComparer();
    }

    /// <summary>
    /// Gets the default singleton instance of the comparer.
    /// </summary>
    public static CompositionObjectEqualityComparer Default { get; }

    /// <summary>
    /// Returns a hash code for the specified <see cref="CompositionObject"/>.
    /// </summary>
    /// <param name="obj">The composition object. Must not be <c>null</c>.</param>
    /// <returns>The object's hash code as provided by <see cref="object.GetHashCode"/>.</returns>
    /// <remarks>
    /// This is not a structural hash. It may not align with the equality defined in <see cref="Equals(CompositionObject?, CompositionObject?)"/>.
    /// </remarks>
    public int GetHashCode(CompositionObject obj) => obj.GetHashCode();

    /// <summary>
    /// Determines whether two <see cref="CompositionObject"/> instances are equal based on their type and relevant properties.
    /// </summary>
    /// <param name="x">The first composition object.</param>
    /// <param name="y">The second composition object.</param>
    /// <returns><c>true</c> if the objects are considered equal; otherwise, <c>false</c>.</returns>
    /// <exception cref="NotImplementedException">Thrown when the objects are of a type not explicitly handled by this comparer.</exception>
    /// <remarks>
    /// - If both are <c>null</c>, returns <c>true</c>.
    /// - If one is <c>null</c>, returns <c>false</c>.
    /// - If they are reference-equal, returns <c>true</c>.
    /// - If types differ, returns <c>false</c>.
    /// - For supported types, compares key properties as documented on the class.
    /// </remarks>
    public virtual bool Equals(CompositionObject? x, CompositionObject? y)
    {
        if (x == null)
            return y == null;

        if (y == null)
            return false;

        if (x == y)
            return true;

        if (x.GetType() != y.GetType())
            return false;

        // CompositionColorBrush: compare by Color
        if (x is CompositionColorBrush xb)
            return xb.Color.Equals(((CompositionColorBrush)y).Color);

        // CompositionEffectBrush: compare by Comment (best available discriminator)
        // there's no other way but use comment
        if (x is CompositionEffectBrush xef)
            return xef.Comment.EqualsIgnoreCase(((CompositionEffectBrush)y).Comment);

        // CompositionLinearGradientBrush: compare geometry and gradient stops
        if (x is CompositionLinearGradientBrush xlb)
        {
            var ylb = (CompositionLinearGradientBrush)y;
            return xlb.StartPoint.Equals(ylb.StartPoint) && xlb.EndPoint.Equals(ylb.EndPoint)
                && equalsGradientBrush(xlb, ylb);
        }

#if NET
        // CompositionRadialGradientBrush (Windows 10, 1903+): compare geometry and gradient stops
        if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
        {
            if (x is CompositionRadialGradientBrush xrg)
            {
                var yrg = (CompositionRadialGradientBrush)y;
                return xrg.GradientOriginOffset.Equals(yrg.GradientOriginOffset) && xrg.EllipseRadius.Equals(yrg.EllipseRadius) && xrg.EllipseCenter.Equals(yrg.EllipseCenter)
                    && equalsGradientBrush(xrg, yrg);
            }
        }
#endif

        // DropShadow: compare by relevant visual properties
        if (x is DropShadow xds)
        {
            var yds = (DropShadow)y;
            return xds.BlurRadius == yds.BlurRadius && xds.Color.Equals(yds.Color) && Equals(xds.Mask, yds.Mask) &&
                xds.Offset.Equals(yds.Offset) && xds.Opacity == yds.Opacity && xds.SourcePolicy == yds.SourcePolicy;
        }

        // Not yet supported type
        throw new NotImplementedException();

        // Helper: compare common CompositionGradientBrush state and color stops
        bool equalsGradientBrush(CompositionGradientBrush b1, CompositionGradientBrush b2) => b1.AnchorPoint.Equals(b2.AnchorPoint) &&
            b1.CenterPoint.Equals(b2.CenterPoint) && b1.ExtendMode == b2.ExtendMode && b1.InterpolationSpace == b2.InterpolationSpace &&
            b1.MappingMode == b2.MappingMode && b1.Offset.Equals(b2.Offset) && b2.RotationAngle == b2.RotationAngle &&
            b1.Scale.Equals(b2.Scale) && b1.TransformMatrix.Equals(b2.TransformMatrix) && equalsColorStops(b1.ColorStops, b2.ColorStops);

        // Helper: compare two gradient color stops
        bool equalsColorStop(CompositionColorGradientStop c1, CompositionColorGradientStop c2) => c1.Color.Equals(c2.Color) && c1.Offset.Equals(c2.Offset);

        // Helper: compare two collections of gradient color stops preserving order
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
