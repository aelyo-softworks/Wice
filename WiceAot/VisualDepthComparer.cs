namespace Wice;

public class VisualDepthComparer : IComparer<Visual>
{
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
