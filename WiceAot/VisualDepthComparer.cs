namespace Wice;

/// <summary>
/// Compares <see cref="Visual"/> instances by their <see cref="Visual.ViewOrder"/> to determine drawing/input depth.
/// </summary>
public class VisualDepthComparer : IComparer<Visual>
{
    /// <summary>
    /// Compares two <see cref="Visual"/> instances by <see cref="Visual.ViewOrder"/> in descending order.
    /// </summary>
    /// <param name="x">The first visual to compare. Must not be <c>null</c>.</param>
    /// <param name="y">The second visual to compare. Must not be <c>null</c>.</param>
    /// <returns>
    /// A value less than zero if <paramref name="x"/> should be ordered before <paramref name="y"/> (i.e., has a higher <see cref="Visual.ViewOrder"/>);
    /// zero if they are equal; greater than zero otherwise.
    /// </returns>
    public virtual int Compare(Visual? x, Visual? y)
    {
        ExceptionExtensions.ThrowIfNull(x, nameof(x));
        ExceptionExtensions.ThrowIfNull(y, nameof(y));
        var cmp = -x!.ViewOrder.CompareTo(y!.ViewOrder);
#if DEBUG
        //Application.Trace("x ► " + x.FullName + " y ► " + y.FullName + " => " + cmp);
#endif
        return cmp;
    }
}
