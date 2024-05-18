namespace Wice.Utilities;

public interface IQuadTree<T> : IEnumerable<T> where T : notnull
{
    IEqualityComparer<T> EqualityComparer { get; }
}
