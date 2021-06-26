using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DirectN;

namespace Wice
{
    public class Dock : Visual
    {
        private readonly Dictionary<Visual, Docked> _docked = new Dictionary<Visual, Docked>();

        public static VisualProperty DockTypeProperty = VisualProperty.Add(typeof(Dock), nameof(DockType), VisualPropertyInvalidateModes.Measure, DockType.Left);
        public static VisualProperty LastChildFillProperty = VisualProperty.Add(typeof(Dock), nameof(LastChildFill), VisualPropertyInvalidateModes.Measure, true);
        public static VisualProperty AllowOverlapProperty = VisualProperty.Add(typeof(Dock), nameof(AllowOverlap), VisualPropertyInvalidateModes.Measure, false);

        public static Orientation GetOrientation(DockType type) => type == DockType.Bottom || type == DockType.Top ? Orientation.Vertical : Orientation.Horizontal;

        public static void SetDockType(IPropertyOwner properties, DockType type)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            properties.SetPropertyValue(DockTypeProperty, type);
        }

        public static DockType GetDockType(IPropertyOwner properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            return (DockType)properties.GetPropertyValue(DockTypeProperty);
        }

        internal Visual _lastChild;

        [Category(CategoryLayout)]
        public bool LastChildFill { get => (bool)GetPropertyValue(LastChildFillProperty); set => SetPropertyValue(LastChildFillProperty, value); }

        [Category(CategoryLayout)]
        public bool AllowOverlap { get => (bool)GetPropertyValue(AllowOverlapProperty); set => SetPropertyValue(AllowOverlapProperty, value); }

        public Visual GetAt(Visual visual, DockType type)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            if (!_docked.TryGetValue(visual, out var docked))
                return null;

            switch (type)
            {
                case DockType.Top:
                    return docked.Top;

                case DockType.Bottom:
                    return docked.Bottom;

                case DockType.Right:
                    return docked.Right;

                case DockType.Left:
                    return docked.Left;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            _lastChild = null;
            var width = 0f;
            var height = 0f;
            var childrenWidth = 0f;
            var childrenHeight = 0f;
            var children = VisibleChildren.ToArray();
            foreach (var child in children)
            {
                var childConstraint = new D2D_SIZE_F(Math.Max(0, constraint.width - childrenWidth), Math.Max(0, constraint.height - childrenHeight));
                child.Measure(childConstraint);
                var childSize = child.DesiredSize;
                if (childSize.IsZero)
                    continue;

                var dock = GetDockType(child);
                switch (dock)
                {
                    case DockType.Bottom:
                    case DockType.Top:
                        childrenHeight += childSize.height;
                        width = Math.Max(width, childrenWidth + childSize.width);
                        break;

                    case DockType.Right:
                    case DockType.Left:
                        childrenWidth += childSize.width;
                        height = Math.Max(height, childrenHeight + childSize.height);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            width = Math.Max(width, childrenWidth);
            height = Math.Max(height, childrenHeight);

            return new D2D_SIZE_F(width, height);
            //var finalSize = constraint;
            //if (!finalSize.width.IsSet() || HorizontalAlignment != Alignment.Stretch)
            //{
            //    finalSize.width = width;
            //}

            //if (!finalSize.height.IsSet() || VerticalAlignment != Alignment.Stretch)
            //{
            //    finalSize.height = height;
            //}
            //return finalSize;
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            var finalSize = finalRect.Size;
            _docked.Clear();
            var left = 0f;
            var top = 0f;
            var right = 0f;
            var bottom = 0f;

            Visual lastLeft = null;
            Visual lastTop = null;
            Visual lastRight = null;
            Visual lastBottom = null;
            var children = VisibleChildren.ToArray();
            var allowOverlap = AllowOverlap;
            var lastChildFill = LastChildFill;
            var noFillCount = children.Length - (lastChildFill ? 1 : 0);
            for (var i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var dock = GetDockType(child);
                var childSize = child.DesiredSize;
                var rc = D2D_RECT_F.Sized(left, top, Math.Max(0, finalSize.width - (left + right)), Math.Max(0, finalSize.height - (top + bottom)));

                if (dock == DockType.Top || dock == DockType.Bottom)
                {
                    switch (child.HorizontalAlignment)
                    {
                        case Alignment.Center:
                            rc.left += (finalSize.width - childSize.width) / 2;
                            rc.Width = childSize.width;
                            break;

                        case Alignment.Near:
                            rc.Width = childSize.width;
                            break;

                        case Alignment.Far:
                            rc.left += finalSize.width - childSize.width;
                            rc.Width = childSize.width;
                            break;

                        case Alignment.Stretch:
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    switch (child.VerticalAlignment)
                    {
                        case Alignment.Center:
                            rc.top += (finalSize.height - childSize.height) / 2;
                            rc.Height = childSize.height;
                            break;

                        case Alignment.Near:
                            rc.Height = childSize.height;
                            break;

                        case Alignment.Far:
                            rc.top += finalSize.height - childSize.height;
                            rc.Height = childSize.height;
                            break;

                        case Alignment.Stretch:
                            // do nothing
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }

                //if (child.HorizontalAlignment != Alignment.Stretch)
                //{
                //    rc.Width = childSize.width;
                //}

                //if (child.VerticalAlignment != Alignment.Stretch)
                //{
                //    rc.Height = childSize.height;
                //}

                if (i < noFillCount)
                {
                    var docked = new Docked();
                    _docked[child] = docked;
                    switch (dock)
                    {
                        case DockType.Right:
                            right += childSize.width;
                            rc.left = Math.Max(0, finalSize.width - right);

                            if (!allowOverlap)
                            {
                                rc.left = Math.Max(rc.left, left);
                            }

                            rc.Width = childSize.width;

                            setDocked(docked, child);
                            lastRight = child;
                            break;

                        case DockType.Top:
                            top += childSize.height;
                            rc.Height = childSize.height;

                            setDocked(docked, child);
                            lastTop = child;
                            break;

                        case DockType.Bottom:
                            bottom += childSize.height;
                            rc.top = Math.Max(0, finalSize.height - bottom);

                            if (!allowOverlap)
                            {
                                rc.top = Math.Max(rc.top, top);
                            }

                            rc.Height = childSize.height;

                            setDocked(docked, child);
                            lastBottom = child;
                            break;

                        case DockType.Left:
                            left += childSize.width;
                            rc.Width = childSize.width;

                            setDocked(docked, child);
                            lastLeft = child;
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }

                if (rc.IsEmpty)
                {
                    // this is a hack to "remove" this child if it was empty, to force it not to show
                    child.Arrange(new D2D_RECT_F(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue));
                }
                else
                {
                    child.Arrange(rc);
                }
                _lastChild = child;
            }

            if (lastChildFill && _lastChild != null)
            {
                var docked = new Docked();
                _docked[_lastChild] = docked;
                setDocked(docked, _lastChild);
            }

            void setDocked(Docked docked, Visual c)
            {
                if (lastLeft != null)
                {
                    docked.Left = lastLeft;
                    _docked[lastLeft].Right = c;
                }

                if (lastTop != null)
                {
                    docked.Top = lastTop;
                    _docked[lastTop].Bottom = c;
                }

                if (lastRight != null)
                {
                    docked.Right = lastRight;
                    _docked[lastRight].Left = c;
                }

                if (lastBottom != null)
                {
                    docked.Bottom = lastBottom;
                    _docked[lastBottom].Top = c;
                }
            }
        }

        private class Docked
        {
            public Visual Left;
            public Visual Right;
            public Visual Top;
            public Visual Bottom;

            public override string ToString() => "L:" + Left + " T:" + Top + " R:" + Right + " B:" + Bottom;
        }
    }
}
