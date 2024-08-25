namespace Wice.Utilities;

public class ConcurrentQuadTree<T> : IQuadTree<T> where T : notnull
{
    private readonly QuadTree<T>.Quadrant _root;
    private readonly Dictionary<T, QuadTree<T>.Quadrant> _table;
    private readonly object _lock = new();

    public ConcurrentQuadTree(D2D_RECT_F bounds)
        : this(bounds, null)
    {
    }

    public ConcurrentQuadTree(D2D_RECT_F bounds, IEqualityComparer<T>? equalityComparer = null)
    {
        EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        _table = new Dictionary<T, QuadTree<T>.Quadrant>(EqualityComparer);
        Bounds = bounds;
        _root = new QuadTree<T>.Quadrant(this, null, bounds);
    }

    public D2D_RECT_F Bounds { get; }
    public IEqualityComparer<T> EqualityComparer { get; }
    public override string ToString() => Bounds.ToString();

#if DEBUG
    public string Dump()
    {
        using var sw = new StringWriter();
        Dump(sw);
        return sw.ToString();
    }

    public void Dump(TextWriter writer)
    {
        foreach (var node in _root.AllNodes)
        {
            writer.WriteLine("N: " + node);
        }
    }
#endif

    public virtual void Insert(T node, D2D_RECT_F bounds)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (bounds.IsEmpty)
            throw new ArgumentException(null, nameof(bounds));

        lock (_lock)
        {
            var parent = _root.Insert(node, bounds);
            _table.Add(node, parent);
        }
    }

    public virtual void Move(T node, D2D_RECT_F newBounds)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (newBounds.IsEmpty)
            throw new ArgumentException(null, nameof(newBounds));

        lock (_lock)
        {
            if (_table.TryGetValue(node, out var parent))
            {
                parent?.RemoveNode(node);
                _table.Remove(node);
            }
            Insert(node, newBounds);
        }
    }

    public virtual bool Remove(T node)
    {
        ArgumentNullException.ThrowIfNull(node);
        lock (_lock)
        {
            if (_table.TryGetValue(node, out var parent))
            {
                parent?.RemoveNode(node);
                _table.Remove(node);
                return true;
            }
            return false;
        }
    }

    public bool IntersectsWithNodes(D2D_RECT_F bounds)
    {
        lock (_lock)
        {
            return _root.GetIntersectingNodes(bounds).Any();
        }
    }

    public virtual IEnumerable<T> GetIntersectingNodes(D2D_RECT_F bounds)
    {
        T[] nodes;
        lock (_lock)
        {
            nodes = _root.GetIntersectingNodes(bounds).Select(node => node.Node).ToArray();
        }

        foreach (var node in nodes)
        {
            yield return node;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator()
    {
        T[] keys;
        lock (_lock)
        {
            keys = [.. _table.Keys];
        }

        foreach (var key in keys)
        {
            yield return key;
        }
    }
}
