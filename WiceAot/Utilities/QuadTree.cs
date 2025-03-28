namespace Wice.Utilities;

public class QuadTree<T> : IQuadTree<T> where T : notnull
{
    private readonly Quadrant _root;
    private readonly Dictionary<T, Quadrant> _table;

    public QuadTree(D2D_RECT_F bounds)
        : this(bounds, null)
    {
    }

    public QuadTree(D2D_RECT_F bounds, IEqualityComparer<T>? equalityComparer = null)
    {
        EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        _table = new Dictionary<T, Quadrant>(EqualityComparer);
        Bounds = bounds;
        _root = new Quadrant(this, null, bounds);
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

        var parent = _root.Insert(node, bounds);
        _table.Add(node, parent);
    }

    public virtual void Move(T node, D2D_RECT_F newBounds)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (newBounds.IsEmpty)
            throw new ArgumentException(null, nameof(newBounds));

        if (_table.TryGetValue(node, out var parent))
        {
            parent?.RemoveNode(node);
            _table.Remove(node);
        }
        Insert(node, newBounds);
    }

    public virtual bool Remove(T node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (_table.TryGetValue(node, out var parent))
        {
            parent?.RemoveNode(node);
            _table.Remove(node);
            return true;
        }
        return false;
    }

    public bool IntersectsWithNodes(D2D_RECT_F bounds) => _root.GetIntersectingNodes(bounds).Any();
    public virtual IEnumerable<T> GetIntersectingNodes(D2D_RECT_F bounds) => _root.GetIntersectingNodes(bounds).Select(node => node.Node);
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
