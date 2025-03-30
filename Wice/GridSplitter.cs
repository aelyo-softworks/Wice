using System;
using System.Collections.Generic;
using DirectN;

namespace Wice
{
    public class GridSplitter : Visual
    {
        public new Grid Parent => base.Parent as Grid;
        public Orientation? Orientation { get; private set; }
        public GridDimension Dimension { get; private set; }

        public GridSplitter()
        {
        }

        protected override void OnAttachedToParent(object sender, EventArgs e)
        {
            base.OnAttachedToParent(sender, e);
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            Dimension = null;
            if (Parent == null || !Orientation.HasValue)
                return;

            var orientation = Orientation.GetValueOrDefault();
            if (orientation == Wice.Orientation.Horizontal)
            {
                if (Parent.Rows.Count < 2)
                    return;

                var index = Math.Max(0, Math.Min(Grid.GetRow(this), Parent.Rows.Count - 2));
                Dimension = Parent.Rows[index];
                Grid.SetColumnSpan(this, int.MaxValue);
                Cursor = Cursor.SizeNS;
                if (Height.IsNotSet())
                {
                    Height = Application.CurrentTheme.DefaultSplitterSize;
                }
            }
            else
            {
                if (Parent.Columns.Count < 2)
                    return;

                var index = Math.Max(0, Math.Min(Grid.GetColumn(this), Parent.Columns.Count - 2));
                Dimension = Parent.Columns[index];
                Grid.SetRowSpan(this, int.MaxValue);
                Cursor = Cursor.SizeWE;
                if (Width.IsNotSet())
                {
                    Width = Application.CurrentTheme.DefaultSplitterSize;
                }
            }

            Dimension.Size = float.NaN;
            Dimension.DefaultAlignment = Alignment.Center;
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == Grid.RowProperty)
            {
                Orientation = Wice.Orientation.Horizontal;
                ResetPropertyValue(Grid.ColumnProperty);
                UpdateProperties();
            }
            else if (property == Grid.ColumnProperty)
            {
                Orientation = Wice.Orientation.Vertical;
                ResetPropertyValue(Grid.RowProperty);
                UpdateProperties();
            }
            return true;
        }

        protected override DragState CreateDragState(MouseButtonEventArgs e) => new SplitDragState(this, e);

        protected override void OnMouseDrag(object sender, DragEventArgs e)
        {
            OnMouseDrag(e);
            base.OnMouseDrag(sender, e);
        }

        private void OnMouseDrag(DragEventArgs e)
        {
            var prev = Dimension?.Previous;
            var next = Dimension?.Next;
            if (prev == null || next == null)
                return;

            if (!Orientation.HasValue)
                return;

            var orientation = Orientation.GetValueOrDefault();
            var state = (SplitDragState)e.State;
            var delta = orientation == Wice.Orientation.Horizontal ? state.DeltaY : state.DeltaX;

            if (delta != 0 && prev.DesiredSize.HasValue)
            {
                var oldSize = state._nextRenderSize + state._previousRenderSize;
                var newPrevSize = Math.Min(Math.Max(0, state._previousRenderSize + delta), oldSize);

                var prevMax = prev.MaxSize;
                if (prevMax.IsSet() && prevMax > 0)
                {
                    newPrevSize = Math.Min(newPrevSize, prevMax);
                }

                var prevMin = prev.MinSize;
                if (prevMin.IsSet() && prevMin > 0)
                {
                    newPrevSize = Math.Max(newPrevSize, prevMin);
                }

                var plannedNextSize = Math.Max(0, oldSize - newPrevSize);
                var newNextSize = plannedNextSize;
                if (next.MaxSize.IsSet() && next.MaxSize > 0)
                {
                    newNextSize = Math.Min(newNextSize, next.MaxSize);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                if (next.MinSize.IsSet() && next.MinSize > 0)
                {
                    newNextSize = Math.Max(newNextSize, next.MinSize);
                    if (newNextSize != plannedNextSize) // incompatible constraints
                        return;
                }

                IEnumerable<GridDimension> dimensions;
                if (orientation == Wice.Orientation.Horizontal)
                {
                    dimensions = Parent.Rows;
                }
                else
                {
                    dimensions = Parent.Columns;
                }

                foreach (var dimension in dimensions)
                {
                    if (dimension == Dimension.Previous)
                    {
                        dimension.Stars = newPrevSize;
                    }
                    else if (dimension == Dimension.Next)
                    {
                        dimension.Stars = newNextSize;
                    }
                    else if (dimension.HasStarSize)
                    {
                        dimension.Stars = dimension.FinalSize.Value;
                    }
                }
            }
        }

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                DragMove(e);
            }
            base.OnMouseButtonDown(sender, e);
        }

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == VirtualKeys.Escape)
            {
                var state = (SplitDragState)CancelDragMove(e);
                if (state != null)
                {
                    var prev = Dimension.Previous;
                    if (prev != null)
                    {
                        prev.Size = state._previousSize;
                        prev.Stars = state._previousStars;
                    }

                    var next = Dimension.Next;
                    if (next != null)
                    {
                        next.Size = state._nextSize;
                        next.Stars = state._nextStars;
                    }
                }
            }
            base.OnKeyDown(sender, e);
        }

        private sealed class SplitDragState : DragState
        {
            public float _previousSize;
            public float _previousStars;
            public float _previousRenderSize;

            public float _nextSize;
            public float _nextStars;
            public float _nextRenderSize;

            public SplitDragState(GridSplitter visual, MouseButtonEventArgs e)
                : base(visual, e)
            {
                var prev = visual.Dimension?.Previous;
                if (prev != null)
                {
                    _previousSize = prev.Size;
                    _previousStars = prev.Stars;
                    _previousRenderSize = prev.DesiredSize.GetValueOrDefault();
                }

                var next = visual.Dimension?.Next;
                if (next != null)
                {
                    _nextSize = next.Size;
                    _nextStars = next.Stars;
                    _nextRenderSize = next.DesiredSize.GetValueOrDefault();
                }
            }
        }
    }
}
