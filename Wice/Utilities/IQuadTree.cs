using System.Collections.Generic;

namespace Wice.Utilities
{
    public interface IQuadTree<T> : IEnumerable<T>
    {
        IEqualityComparer<T> EqualityComparer { get; }
    }
}
