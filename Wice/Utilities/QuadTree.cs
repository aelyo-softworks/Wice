using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DirectN;

namespace Wice.Utilities
{
    public class QuadTree<T> : IQuadTree<T>
    {
        private readonly Quadrant _root;
        private readonly Dictionary<T, Quadrant> _table;

        public QuadTree(D2D_RECT_F bounds)
            : this(bounds, null)
        {
        }

        public QuadTree(D2D_RECT_F bounds, IEqualityComparer<T> equalityComparer = null)
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
            using (var sw = new System.IO.StringWriter())
            {
                Dump(sw);
                return sw.ToString();
            }
        }

        public void Dump(System.IO.TextWriter writer)
        {
            foreach (var node in _root.AllNodes)
            {
                writer.WriteLine("N: " + node);
            }
        }
#endif

        public virtual void Insert(T node, D2D_RECT_F bounds)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (bounds.IsEmpty)
                throw new ArgumentException(null, nameof(bounds));

            var parent = _root.Insert(node, bounds);
            _table.Add(node, parent);
        }

        public virtual void Move(T node, D2D_RECT_F newBounds)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

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
            if (node == null)
                throw new ArgumentNullException(nameof(node));

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

        internal class QuadNode
        {
            public D2D_RECT_F Bounds;
            public QuadNode Next;
            public T Node;

            public override string ToString() => Node + " => " + Bounds;
        }

        internal class Quadrant
        {
            public Quadrant Parent;
            public D2D_RECT_F Bounds;
            private QuadNode _nodes;
            private readonly IQuadTree<T> _quadTree;

            private Quadrant _topLeft;
            private Quadrant _topRight;
            private Quadrant _bottomLeft;
            private Quadrant _bottomRight;

            public Quadrant(IQuadTree<T> quadTree, Quadrant parent, D2D_RECT_F bounds)
            {
                _quadTree = quadTree;
                Parent = parent;
                Bounds = bounds;
            }

            public IEnumerable<QuadNode> Nodes
            {
                get
                {
                    var node = _nodes;
                    if (node == null)
                        yield break;

                    do
                    {
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

                Quadrant child = null;
                if (topLeft.Contains(bounds))
                {
                    if (_topLeft == null)
                    {
                        _topLeft = new Quadrant(_quadTree, this, topLeft);
                    }
                    child = _topLeft;
                }
                else if (topRight.Contains(bounds))
                {
                    if (_topRight == null)
                    {
                        _topRight = new Quadrant(_quadTree, this, topRight);
                    }
                    child = _topRight;
                }
                else if (bottomLeft.Contains(bounds))
                {
                    if (_bottomLeft == null)
                    {
                        _bottomLeft = new Quadrant(_quadTree, this, bottomLeft);
                    }
                    child = _bottomLeft;
                }
                else if (bottomRight.Contains(bounds))
                {
                    if (_bottomRight == null)
                    {
                        _bottomRight = new Quadrant(_quadTree, this, bottomRight);
                    }
                    child = _bottomRight;
                }

                if (child != null)
                    return child.Insert(node, bounds);

                var n = new QuadNode { Node = node, Bounds = bounds };
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

            private static IEnumerable<QuadNode> GetIntersectingNodes(QuadNode last, D2D_RECT_F bounds)
            {
                if (last != null)
                {
                    var n = last;
                    do
                    {
                        n = n.Next;
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
                    while (!_quadTree.EqualityComparer.Equals(p.Next.Node, node) && p.Next != nodes)
                    {
                        p = p.Next;
                    }

                    if (_quadTree.EqualityComparer.Equals(p.Next.Node, node))
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
}
