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
/// <remarks>
/// - Partitioning: The tree lazily subdivides the initial <see cref="Bounds"/> into four equal
///   child quadrants. An item is stored in the deepest quadrant that fully contains its bounds;
///   otherwise it is stored at the current quadrant node.
/// - Coordinate space: All operations are expressed in the coordinate system of <see cref="Bounds"/>.
/// - Duplicates: Item uniqueness is determined by <see cref="EqualityComparer"/>.
/// - Thread-safety: This type is not thread-safe. External synchronization is required for concurrent access.
/// </remarks>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bounds"/> is empty.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="newBounds"/> is empty.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/>.</exception>
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

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Represents a node stored at a quadrant, linking items in a circular singly linked list.
    /// </summary>
    /// <param name="node">The stored item.</param>
    /// <param name="bounds">The stored item's bounds.</param>
    internal class QuadNode(T node, D2D_RECT_F bounds)
    {
        /// <summary>The bounds associated with <see cref="Node"/>.</summary>
        public D2D_RECT_F Bounds = bounds;

        /// <summary>Pointer to the next node in the circular list (or self if this is the only node).</summary>
        public QuadNode? Next;

        /// <summary>The stored item.</summary>
        public T Node = node;

        /// <inheritdoc />
        public override string ToString() => Node + " => " + Bounds;
    }

    /// <summary>
    /// Represents a quadrant in the quadtree. Quadrants are created lazily as needed
    /// and can hold items locally and/or have child quadrants.
    /// </summary>
    /// <param name="quadTree">The owning quadtree.</param>
    /// <param name="parent">The parent quadrant, or <see langword="null"/> for the root.</param>
    /// <param name="bounds">The quadrant bounds.</param>
    internal class Quadrant(IQuadTree<T> quadTree, Quadrant? parent, D2D_RECT_F bounds)
    {
        /// <summary>Gets or sets the parent quadrant (or <see langword="null"/> for the root).</summary>
        public Quadrant? Parent = parent;

        /// <summary>Gets or sets the bounds of this quadrant.</summary>
        public D2D_RECT_F Bounds = bounds;

        private QuadNode? _nodes;
        private readonly IQuadTree<T> _quadTree = quadTree;

        private Quadrant? _topLeft;
        private Quadrant? _topRight;
        private Quadrant? _bottomLeft;
        private Quadrant? _bottomRight;

        /// <summary>
        /// Enumerates the nodes stored directly in this quadrant (not including children).
        /// </summary>
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

        /// <summary>
        /// Enumerates the nodes stored in this quadrant and recursively in all child quadrants.
        /// </summary>
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

        /// <summary>
        /// Inserts an item into this quadrant or into one of its children if the bounds
        /// fully fit within a child quadrant; otherwise stores it locally.
        /// </summary>
        /// <param name="node">The item to insert.</param>
        /// <param name="bounds">The item bounds.</param>
        /// <returns>The quadrant that directly stores the item.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bounds"/> is empty.</exception>
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

        /// <summary>
        /// Enumerates nodes that intersect with the specified <paramref name="bounds"/> within this quadrant
        /// and its children.
        /// </summary>
        /// <param name="bounds">The query bounds.</param>
        /// <returns>An enumerable of intersecting <see cref="QuadNode"/> instances.</returns>
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

        /// <summary>
        /// Iterates over the circular node list starting after <paramref name="last"/>, yielding those
        /// whose bounds intersect with <paramref name="bounds"/>.
        /// </summary>
        /// <param name="last">The last node in the circular list (anchor), or <see langword="null"/>.</param>
        /// <param name="bounds">The query bounds.</param>
        /// <returns>An enumerable of intersecting nodes.</returns>
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

        /// <summary>
        /// Removes the specified <paramref name="node"/> from this quadrant's local node list.
        /// </summary>
        /// <param name="node">The item to remove.</param>
        /// <returns><see langword="true"/> if the node was found and removed; otherwise, <see langword="false"/>.</returns>
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
