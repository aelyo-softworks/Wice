namespace Wice.Utilities;

/// <summary>
/// A spatial quadtree that stores items of type <typeparamref name="T"/> associated with
/// axis-aligned rectangular bounds. Supports insertion, movement, removal, intersection queries,
/// and enumeration over all items currently in the tree.
/// </summary>
/// <typeparam name="T">
/// The non-nullable item type stored in the quadtree. Items are compared using
/// <see cref="EqualityComparer"/>.
/// </typeparam>
public class QuadTree<T> : IQuadTree<T> where T : notnull
{
    private readonly Quadrant _root;
    private readonly Dictionary<T, Quadrant> _table;

    /// <summary>
    /// Initializes a new quadtree over the specified world <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The world bounds covered by this quadtree.</param>
    public QuadTree(D2D_RECT_F bounds)
        : this(bounds, null)
    {
    }

    /// <summary>
    /// Initializes a new quadtree over the specified world <paramref name="bounds"/> and comparer.
    /// </summary>
    /// <param name="bounds">The world bounds covered by this quadtree.</param>
    /// <param name="equalityComparer">
    /// The equality comparer used to compare items of type <typeparamref name="T"/>. If <see langword="null"/>,
    /// <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    public QuadTree(D2D_RECT_F bounds, IEqualityComparer<T>? equalityComparer = null)
    {
        EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        _table = new Dictionary<T, Quadrant>(EqualityComparer);
        Bounds = bounds;
        _root = new Quadrant(this, null, bounds);
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

        var parent = _root.Insert(node, bounds);
        _table.Add(node, parent);
    }

    /// <summary>
    /// Moves an existing item to a new <paramref name="newBounds"/>.
    /// If the item is not found, it is inserted as a new item.
    /// </summary>
    /// <param name="node">The item to move.</param>
    /// <param name="newBounds">The new bounds for the item.</param>
    public virtual void Move(T node, D2D_RECT_F newBounds)
    {
        ExceptionExtensions.ThrowIfNull(node, nameof(node));
        if (newBounds.IsEmpty)
            throw new ArgumentException(null, nameof(newBounds));

        if (_table.TryGetValue(node, out var parent))
        {
            parent?.RemoveNode(node);
            _table.Remove(node);
        }
        Insert(node, newBounds);
    }

    /// <summary>
    /// Removes the specified <paramref name="node"/> from the quadtree.
    /// </summary>
    /// <param name="node">The item to remove.</param>
    /// <returns><see langword="true"/> if the item was found and removed; otherwise, <see langword="false"/>.</returns>
    public virtual bool Remove(T node)
    {
        ExceptionExtensions.ThrowIfNull(node, nameof(node));
        if (_table.TryGetValue(node, out var parent))
        {
            parent?.RemoveNode(node);
            _table.Remove(node);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether any items in the quadtree intersect with the given <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The query bounds.</param>
    /// <returns><see langword="true"/> if at least one item intersects; otherwise, <see langword="false"/>.</returns>
    public bool IntersectsWithNodes(D2D_RECT_F bounds) => _root.GetIntersectingNodes(bounds).Any();

    /// <summary>
    /// Returns all items whose bounds intersect with the given <paramref name="bounds"/>.
    /// </summary>
    /// <param name="bounds">The query bounds.</param>
    /// <returns>An enumerable of items intersecting the query bounds.</returns>
    public virtual IEnumerable<T> GetIntersectingNodes(D2D_RECT_F bounds) => _root.GetIntersectingNodes(bounds).Select(node => node.Node);

    /// <summary>
    /// Returns an enumerator that iterates through all items stored in the quadtree.
    /// </summary>
    public IEnumerator<T> GetEnumerator() => _table.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal class QuadNode(T node, D2D_RECT_F bounds)
    {
        public D2D_RECT_F Bounds = bounds;
        public QuadNode? Next;
        public T Node = node;
        public override string ToString() => Node + " => " + Bounds;
    }

    internal class Quadrant(IQuadTree<T> quadTree, Quadrant? parent, D2D_RECT_F bounds)
    {
        public Quadrant? Parent = parent;
        public D2D_RECT_F Bounds = bounds;
        private QuadNode? _nodes;
        private readonly IQuadTree<T> _quadTree = quadTree;
        private Quadrant? _topLeft;
        private Quadrant? _topRight;
        private Quadrant? _bottomLeft;
        private Quadrant? _bottomRight;

        public IEnumerable<QuadNode> Nodes
        {
            get
            {
                var node = _nodes;
                do
                {
                    if (node == null)
                        yield break;

                    yield return node;
                    node = node.Next;
                    if (node == _nodes)
                        break;
                }
                while (true);
            }
        }

        public IEnumerable<QuadNode> AllNodes
        {
            get
            {
                foreach (var node in Nodes)
                {
                    yield return node;
                }

                if (_topLeft != null)
                {
                    foreach (var node in _topLeft.AllNodes)
                    {
                        yield return node;
                    }
                }

                if (_topRight != null)
                {
                    foreach (var node in _topRight.AllNodes)
                    {
                        yield return node;
                    }
                }

                if (_bottomLeft != null)
                {
                    foreach (var node in _bottomLeft.AllNodes)
                    {
                        yield return node;
                    }
                }

                if (_bottomRight != null)
                {
                    foreach (var node in _bottomRight.AllNodes)
                    {
                        yield return node;
                    }
                }
            }
        }

        public Quadrant Insert(T node, D2D_RECT_F bounds)
        {
            if (bounds.IsEmpty)
                throw new ArgumentException(null, nameof(bounds));

            var w = Bounds.Width / 2;
            if (w == 0)
            {
                w = 1;
            }

            var h = Bounds.Height / 2;
            if (h == 0)
            {
                h = 1;
            }

            var topLeft = D2D_RECT_F.Sized(Bounds.left, Bounds.top, w, h);
            var topRight = D2D_RECT_F.Sized(Bounds.left + w, Bounds.top, w, h);
            var bottomLeft = D2D_RECT_F.Sized(Bounds.left, Bounds.top + h, w, h);
            var bottomRight = D2D_RECT_F.Sized(Bounds.left + w, Bounds.top + h, w, h);

            Quadrant? child = null;
            if (topLeft.Contains(bounds))
            {
                _topLeft ??= new Quadrant(_quadTree, this, topLeft);
                child = _topLeft;
            }
            else if (topRight.Contains(bounds))
            {
                _topRight ??= new Quadrant(_quadTree, this, topRight);
                child = _topRight;
            }
            else if (bottomLeft.Contains(bounds))
            {
                _bottomLeft ??= new Quadrant(_quadTree, this, bottomLeft);
                child = _bottomLeft;
            }
            else if (bottomRight.Contains(bounds))
            {
                _bottomRight ??= new Quadrant(_quadTree, this, bottomRight);
                child = _bottomRight;
            }

            if (child != null)
                return child.Insert(node, bounds);

            var n = new QuadNode(node, bounds);
            var nodes = _nodes;
            if (nodes == null)
            {
                n.Next = n;
            }
            else
            {
                n.Next = nodes.Next;
                nodes.Next = n;
            }

            _nodes = n;
            return this;
        }

        public IEnumerable<QuadNode> GetIntersectingNodes(D2D_RECT_F bounds)
        {
            var w = Bounds.Width / 2;
            var h = Bounds.Height / 2;

            var topLeft = D2D_RECT_F.Sized(Bounds.left, Bounds.top, w, h);
            var topRight = D2D_RECT_F.Sized(Bounds.left + w, Bounds.top, w, h);
            var bottomLeft = D2D_RECT_F.Sized(Bounds.left, Bounds.top + h, w, h);
            var bottomRight = D2D_RECT_F.Sized(Bounds.left + w, Bounds.top + h, w, h);

            if (_topLeft != null && topLeft.IntersectsWith(bounds))
            {
                foreach (var node in _topLeft.GetIntersectingNodes(bounds))
                {
                    yield return node;
                }
            }

            if (_topRight != null && topRight.IntersectsWith(bounds))
            {
                foreach (var node in _topRight.GetIntersectingNodes(bounds))
                {
                    yield return node;
                }
            }

            if (_bottomLeft != null && bottomLeft.IntersectsWith(bounds))
            {
                foreach (var node in _bottomLeft.GetIntersectingNodes(bounds))
                {
                    yield return node;
                }
            }

            if (_bottomRight != null && bottomRight.IntersectsWith(bounds))
            {
                foreach (var node in _bottomRight.GetIntersectingNodes(bounds))
                {
                    yield return node;
                }
            }

            foreach (var node in GetIntersectingNodes(_nodes, bounds))
            {
                yield return node;
            }
        }

        private static IEnumerable<QuadNode> GetIntersectingNodes(QuadNode? last, D2D_RECT_F bounds)
        {
            if (last != null)
            {
                var n = last;
                do
                {
                    n = n.Next;
                    if (n == null)
                        break;

                    if (n.Bounds.IntersectsWith(bounds))
                    {
                        yield return n;
                    }
                }
                while (n != last);
            }
        }

        public bool RemoveNode(T node)
        {
            var rc = false;
            var nodes = _nodes;
            if (nodes != null)
            {
                var p = nodes;
                while (p.Next != null && !_quadTree.EqualityComparer.Equals(p.Next.Node, node) && p.Next != nodes)
                {
                    p = p.Next;
                }

                if (p.Next != null && _quadTree.EqualityComparer.Equals(p.Next.Node, node))
                {
                    rc = true;
                    var n = p.Next;
                    if (p == n)
                    {
                        _nodes = null;
                    }
                    else
                    {
                        if (nodes == n)
                        {
                            _nodes = p;
                        }
                        p.Next = n.Next;
                    }
                }
            }
            return rc;
        }
    }
}
