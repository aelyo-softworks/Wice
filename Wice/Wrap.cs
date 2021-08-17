using System;
using System.ComponentModel;
using System.Linq;
using DirectN;

namespace Wice
{
    public class Wrap : Visual
    {
        public static VisualProperty OrientationProperty = VisualProperty.Add(typeof(Wrap), nameof(Orientation), VisualPropertyInvalidateModes.Measure, Orientation.Vertical);
        public static VisualProperty ItemWidthProperty = VisualProperty.Add(typeof(Wrap), nameof(ItemWidth), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);
        public static VisualProperty ItemHeightProperty = VisualProperty.Add(typeof(Wrap), nameof(ItemHeight), VisualPropertyInvalidateModes.ParentMeasure, float.NaN, ValidateWidthOrHeight);

        [Category(CategoryLayout)]
        public Orientation Orientation { get => (Orientation)GetPropertyValue(OrientationProperty); set => SetPropertyValue(OrientationProperty, value); }

        [Category(CategoryLayout)]
        public float ItemWidth { get => (float)GetPropertyValue(ItemWidthProperty); set => SetPropertyValue(ItemWidthProperty, value); }

        [Category(CategoryLayout)]
        public float ItemHeight { get => (float)GetPropertyValue(ItemHeightProperty); set => SetPropertyValue(ItemHeightProperty, value); }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var orientation = Orientation;
            var curLineSize = new UVSize(orientation);
            var panelSize = new UVSize(orientation);
            var uvConstraint = new UVSize(orientation, constraint.width, constraint.height);
            var itemWidth = ItemWidth;
            var itemHeight = ItemHeight;
            var itemWidthSet = itemWidth.IsSet();
            var itemHeightSet = itemHeight.IsSet();

            var childConstraint = new D2D_SIZE_F(itemWidthSet ? itemWidth : constraint.width, itemHeightSet ? itemHeight : constraint.height);

            var children = VisibleChildren.ToArray();
            foreach (var child in children)
            {
                child.Measure(childConstraint);
                var childSize = child.DesiredSize;

                var sz = new UVSize(orientation, itemWidthSet ? itemWidth : childSize.width, itemHeightSet ? itemHeight : childSize.height);
                if ((curLineSize.U + sz.U) > uvConstraint.U)
                {
                    panelSize.U = Math.Max(curLineSize.U, panelSize.U);
                    panelSize.V += curLineSize.V;
                    curLineSize = sz;

                    if (sz.U > uvConstraint.U)
                    {
                        panelSize.U = Math.Max(sz.U, panelSize.U);
                        panelSize.V += sz.V;
                        curLineSize = new UVSize(orientation);
                    }
                }
                else
                {
                    curLineSize.U += sz.U;
                    curLineSize.V = Math.Max(sz.V, curLineSize.V);
                }
            }

            panelSize.U = Math.Max(curLineSize.U, panelSize.U);
            panelSize.V += curLineSize.V;
            return new D2D_SIZE_F(panelSize.Width, panelSize.Height);
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            var orientation = Orientation;
            var firstInLine = 0;
            var itemWidth = ItemWidth;
            var itemHeight = ItemHeight;
            var accumulatedV = 0f;
            var itemU = orientation == Orientation.Horizontal ? itemWidth : itemHeight;
            var curLineSize = new UVSize(orientation);
            var uvFinalSize = new UVSize(orientation, finalRect.Width, finalRect.Height);
            var itemWidthSet = itemWidth.IsSet();
            var itemHeightSet = itemHeight.IsSet();
            var useItemU = orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
            var children = VisibleChildren.ToArray();

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var childSize = child.DesiredSize;
                var sz = new UVSize(orientation, itemWidthSet ? itemWidth : childSize.width, itemHeightSet ? itemHeight : childSize.height);

                if ((curLineSize.U + sz.U) > uvFinalSize.U)
                {
                    ArrangeLine(orientation, children, accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU);

                    accumulatedV += curLineSize.V;
                    curLineSize = sz;

                    if (sz.U > uvFinalSize.U)
                    {
                        ArrangeLine(orientation, children, accumulatedV, sz.V, i, ++i, useItemU, itemU);
                        accumulatedV += sz.V;
                        curLineSize = new UVSize(orientation);
                    }
                    firstInLine = i;
                }
                else
                {
                    curLineSize.U += sz.U;
                    curLineSize.V = Math.Max(sz.V, curLineSize.V);
                }
            }

            if (firstInLine < children.Length)
            {
                ArrangeLine(orientation, children, accumulatedV, curLineSize.V, firstInLine, children.Length, useItemU, itemU);
            }
        }

        private static void ArrangeLine(Orientation orientation, Visual[] children, float v, float lineV, int start, int end, bool useItemU, float itemU)
        {
            var u = 0f;
            var isHorizontal = orientation == Orientation.Horizontal;

            for (var i = start; i < end; i++)
            {
                var child = children[i];
                var childSize = child.DesiredSize;
                var sz = new UVSize(orientation, childSize.width, childSize.height);
                var layoutSlotU = useItemU ? itemU : sz.U;
                child.Arrange(D2D_RECT_F.Sized(isHorizontal ? u : v, isHorizontal ? v : u, isHorizontal ? layoutSlotU : lineV, isHorizontal ? lineV : layoutSlotU));
                u += layoutSlotU;
            }
        }

        private struct UVSize
        {
            public float U;
            public float V;
            public Orientation Orientation;

            public UVSize(Orientation orientation)
            {
                U = 0;
                V = 0;
                Orientation = orientation;
            }

            public UVSize(Orientation orientation, float width, float height)
                : this(orientation)
            {
                Width = width;
                Height = height;
            }

            public float Width { get => Orientation == Orientation.Horizontal ? U : V; set { if (Orientation == Orientation.Horizontal) U = value; else V = value; } }
            public float Height { get => Orientation == Orientation.Horizontal ? V : U; set { if (Orientation == Orientation.Horizontal) V = value; else U = value; } }
        }
    }
}
