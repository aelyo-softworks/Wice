namespace Wice.Utilities;

/// <summary>
/// A thread-safe quadtree for storing items of type <typeparamref name="T"/> associated with
/// axis-aligned rectangular bounds. This type wraps the non-thread-safe <see cref="QuadTree{T}"/>
/// using a coarse-grained lock to provide safe concurrent access.
/// </summary>
/// <typeparam name="T">
/// The non-nullable item type stored in the quadtree. Items are compared using
/// <see cref="EqualityComparer"/>.
/// </typeparam>
public class ConcurrentQuadTree<T> : IQuadTree<T> where T : notnull
{
    private readonly QuadTree<T>.Quadrant _root;
    private readonly Dictionary<T, QuadTree<T>.Quadrant> _table;
#if NETFRAMEWORK
    // On .NET Framework, use a simple monitor lock.
    private readonly object _lock = new();
#else
    // On modern runtimes, use a dedicated lock object instance. The 'lock' statement
    // synchronizes via Monitor on this object (re-entrant for the owning thread).
    private readonly Lock _lock = new();
#endif

    /// <summary>
    /// Initializes a new concurrent quadtree over the specified world <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The world bounds covered by this quadtree.</param>
    public ConcurrentQuadTree(D2D_RECT_F bounds)
        : this(bounds, null)
    {
    }

    /// <summary>
    /// Initializes a new concurrent quadtree over the specified world <paramref name="bounds"/> and comparer.
    /// </summary>
    /// <param name="bounds">The world bounds covered by this quadtree.</param>
    /// <param name="equalityComparer">
    /// The equality comparer used to compare items of type <typeparamref name="T"/>. If <see langword="null"/>,
    /// <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    public ConcurrentQuadTree(D2D_RECT_F bounds, IEqualityComparer<T>? equalityComparer = null)
    {
        EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        _table = new Dictionary<T, QuadTree<T>.Quadrant>(EqualityComparer);
        Bounds = bounds;
        _root = new QuadTree<T>.Quadrant(this, null, bounds);
    }

    /// <summary>
    /// Gets the world bounds covered by this quadtree.
    /// </summary>
    public D2D_RECT_F Bounds { get; }

    /// <summary>
    /// Gets the equality comparer used to compare items of type <typeparamref name="T"/>.
    /// </summary>
    public IEqualityComparer<T> EqualityComparer { get; }

    /// <inheritdoc />
    public override string ToString() => Bounds.ToString();

#if DEBUG
    /// <summary>
    /// Returns a human-readable dump of the nodes stored in the tree (for debugging).
    /// </summary>
    public string Dump()
    {
        using var sw = new StringWriter();
        Dump(sw);
        return sw.ToString();
    }

    /// <summary>
    /// Writes a human-readable dump of the nodes stored in the tree to the given <paramref name="writer"/> (for debugging).
    /// </summary>
    /// <param name="writer">The text writer that receives the dump.</param>
    public void Dump(TextWriter writer)
    {
        foreach (var node in _root.AllNodes)
        {
            writer.WriteLine("N: " + node);
        }
    }
#endif

    /// <summary>
    /// Inserts an item into the quadtree with the specified <paramref name="bounds"/>.
    /// </summary>
    /// <param name="node">The item to insert.</param>
    /// <param name="bounds">The item bounds in the tree's coordinate space.</param>
    public virtual void Insert(T node, D2D_RECT_F bounds)
    {
        ExceptionExtensions.ThrowIfNull(node, nameof(node));
        if (bounds.IsEmpty)
            throw new ArgumentException(null, nameof(bounds));

        lock (_lock)
        {
            var parent = _root.Insert(node, bounds);
            _table.Add(node, parent);
        }
    }

    /// <summary>
    /// Moves an existing item to <paramref name="newBounds"/>. If the item is not found, it is inserted.
    /// </summary>
    /// <param name="node">The item to move.</param>
    /// <param name="newBounds">The new bounds for the item.</param>
    public virtual void Move(T node, D2D_RECT_F newBounds)
    {
        ExceptionExtensions.ThrowIfNull(node, nameof(node));
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

    /// <summary>
    /// Removes the specified <paramref name="node"/> from the quadtree.
    /// </summary>
    /// <param name="node">The item to remove.</param>
    /// <returns><see langword="true"/> if the item was found and removed; otherwise, <see langword="false"/>.</returns>
    public virtual bool Remove(T node)
    {
        ExceptionExtensions.ThrowIfNull(node, nameof(node));
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

    /// <summary>
    /// Determines whether any items in the quadtree intersect with the given <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The query bounds.</param>
    /// <returns><see langword="true"/> if at least one item intersects; otherwise, <see langword="false"/>.</returns>
    public bool IntersectsWithNodes(D2D_RECT_F bounds)
    {
        lock (_lock)
        {
            return _root.GetIntersectingNodes(bounds).Any();
        }
    }

    /// <summary>
    /// Returns all items whose bounds intersect with the given <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The query bounds.</param>
    /// <returns>An enumerable of items intersecting the query bounds.</returns>
    public virtual IEnumerable<T> GetIntersectingNodes(D2D_RECT_F bounds)
    {
        T[] nodes;
        lock (_lock)
        {
            nodes = [.. _root.GetIntersectingNodes(bounds).Select(node => node.Node)];
        }

        foreach (var node in nodes)
        {
            yield return node;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through all items stored in the quadtree.
    /// </summary>
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
