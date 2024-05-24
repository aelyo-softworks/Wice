namespace Wice;

public class Grid : Visual
{
    public static VisualProperty RowProperty { get; } = VisualProperty.Add(typeof(Grid), "Row", VisualPropertyInvalidateModes.Measure, 0, convert: ValidateRowCol);
    public static VisualProperty ColumnProperty { get; } = VisualProperty.Add(typeof(Grid), "Column", VisualPropertyInvalidateModes.Measure, 0, convert: ValidateRowCol);
    public static VisualProperty RowSpanProperty { get; } = VisualProperty.Add(typeof(Grid), "RowSpan", VisualPropertyInvalidateModes.Measure, 1, convert: ValidateSpan);
    public static VisualProperty ColumnSpanProperty { get; } = VisualProperty.Add(typeof(Grid), "ColumnSpan", VisualPropertyInvalidateModes.Measure, 1, convert: ValidateSpan);

    private static object ValidateRowCol(BaseObject obj, object value)
    {
        var i = (int)value;
        if (i < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        return i;
    }

    private static object ValidateSpan(BaseObject obj, object value)
    {
        var i = (int)value;
        if (i <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        return i;
    }

    private Dictionary<GridDimension, List<Visual>>? _childrenByDimensions;

    public Grid()
    {
        Rows = new RowCollection(this)
        {
            new GridRow()
        };
        Columns = new ColumnCollection(this)
        {
            new GridColumn()
        };
    }

    [Category(CategoryLayout)]
    public BaseObjectCollection<GridRow> Rows { get; }

    [Category(CategoryLayout)]
    public BaseObjectCollection<GridColumn> Columns { get; }

    private class RowCollection : BaseObjectCollection<GridRow>
    {
        private readonly Grid _grid;

        public RowCollection(Grid grid)
        {
            _grid = grid;
        }

        //protected override void ProtectedClear()
        //{
        //    base.ProtectedClear();
        //    Add(new GridRow());
        //}

        protected override void ProtectedRemoveAt(int index)
        {
            foreach (var child in _grid.Children)
            {
                var childIndex = GetRow(child);
                // note we keep current children in the old row with their row index
                if (childIndex > index)
                {
                    SetRow(child, childIndex - 1);
                }
            }

            var item = this[index];
            base.ProtectedRemoveAt(index);
            item.Parent = null;
            if (Count == 0)
            {
                Add(new GridRow());
            }
        }

        protected override void ProtectedAdd(GridRow item, bool checkMaxChildrenCount)
        {
            base.ProtectedAdd(item, checkMaxChildrenCount);
            item.Parent = _grid;
        }

        protected override void ProtectedInsert(int index, GridRow item)
        {
            base.ProtectedInsert(index, item);
            item.Parent = _grid;
            foreach (var child in _grid.Children)
            {
                var childIndex = GetRow(child);
                if (childIndex >= index)
                {
                    SetRow(child, childIndex + 1);
                }
            }
        }
    }

    private class ColumnCollection : BaseObjectCollection<GridColumn>
    {
        private readonly Grid _grid;

        public ColumnCollection(Grid grid)
        {
            _grid = grid;
        }

        //protected override void ProtectedClear()
        //{
        //    base.ProtectedClear();
        //    Add(new GridColumn());
        //}

        protected override void ProtectedRemoveAt(int index)
        {
            foreach (var child in _grid.Children)
            {
                var childIndex = GetColumn(child);
                // note we keep current children in the old col with their col index
                if (childIndex > index)
                {
                    SetColumn(child, childIndex - 1);
                }
            }

            var item = this[index];
            base.ProtectedRemoveAt(index);
            item.Parent = null;
            if (Count == 0)
            {
                Add(new GridColumn());
            }
        }

        protected override void ProtectedAdd(GridColumn item, bool checkMaxChildrenCount)
        {
            base.ProtectedAdd(item, checkMaxChildrenCount);
            item.Parent = _grid;
        }

        protected override void ProtectedInsert(int index, GridColumn item)
        {
            base.ProtectedInsert(index, item);
            item.Parent = _grid;
            foreach (var child in _grid.Children)
            {
                var childIndex = GetColumn(child);
                if (childIndex >= index)
                {
                    SetColumn(child, childIndex + 1);
                }
            }
        }
    }

    public IEnumerable<Visual> GetCells(int? columnIndex = null, int? rowIndex = null)
    {
        foreach (var child in Children)
        {
            if (columnIndex.HasValue)
            {
                var col = GetColumn(child);
                if (col != columnIndex.Value)
                    continue;
            }

            if (rowIndex.HasValue)
            {
                var row = GetRow(child);
                if (row != rowIndex.Value)
                    continue;
            }

            yield return child;
        }
    }

    public static void SetColumn(IPropertyOwner properties, int columnIndex)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (columnIndex < 0)
            throw new ArgumentException(null, nameof(columnIndex));

        properties.SetPropertyValue(ColumnProperty, columnIndex);
    }

    public static int GetColumn(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return Math.Max(0, (int)properties.GetPropertyValue(ColumnProperty)!);
    }

    public static void SetColumnSpan(IPropertyOwner properties, int span)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (span < 1)
            throw new ArgumentException(null, nameof(span));

        // int.MaxValue means always stick to the number of columns even if it changes
        properties.SetPropertyValue(ColumnSpanProperty, span);
    }

    public static int GetColumnSpan(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return Math.Max(1, (int)properties.GetPropertyValue(ColumnSpanProperty)!);
    }

    public static void SetRowSpan(IPropertyOwner properties, int span)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (span < 1)
            throw new ArgumentException(null, nameof(span));

        // int.MaxValue means always stick to the number of rows even if it changes
        properties.SetPropertyValue(RowSpanProperty, span);
    }

    public static int GetRowSpan(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return Math.Max(1, (int)properties.GetPropertyValue(RowSpanProperty)!);
    }

    public static void SetRow(IPropertyOwner properties, int rowIndex)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (rowIndex < 0)
            throw new ArgumentException(null, nameof(rowIndex));

        properties.SetPropertyValue(RowProperty, rowIndex);
    }

    public static int GetRow(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return Math.Max(0, (int)properties.GetPropertyValue(RowProperty)!);
    }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        //Application.Trace("this: " + this + " constraint: " + constraint);
        //var padding = Padding;

        // reset all
        foreach (var col in Columns)
        {
            col.FinalStartPosition = null;
            col.DesiredSize = null;
        }

        foreach (var row in Rows)
        {
            row.FinalStartPosition = null;
            row.DesiredSize = null;
        }

        var totalColStars = Columns.Where(d => d.HasStarSize).Sum(d => d.Stars);
        var totalRowStars = Rows.Where(d => d.HasStarSize).Sum(d => d.Stars);

        _childrenByDimensions = [];

        // measure children & build dic dim => children
        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var gs = GridSet.Get(this, child);

            foreach (var col in gs.GetCols(this))
            {
                if (!_childrenByDimensions.TryGetValue(col, out var list))
                {
                    list = [];
                    _childrenByDimensions.Add(col, list);
                }
                list.Add(child);
            }

            foreach (var row in gs.GetRows(this))
            {
                if (!_childrenByDimensions.TryGetValue(row, out var list))
                {
                    list = [];
                    _childrenByDimensions.Add(row, list);
                }
                list.Add(child);
            }

            var cw = gs.GetDefinedColSize(this);
            var ch = gs.GetDefinedRowSize(this);
            var childConstraint = new D2D_SIZE_F(cw ?? constraint.width, ch ?? constraint.height);

            if (!cw.HasValue && childConstraint.width.IsSet())
            {
                var outw = gs.GetOutDefinedColSize(this);
                if (outw > 0)
                {
                    childConstraint.width = Math.Max(0, childConstraint.width - outw);
                }
            }

            if (!ch.HasValue && childConstraint.height.IsSet())
            {
                var outh = gs.GetOutDefinedRowSize(this);
                if (outh > 0)
                {
                    childConstraint.height = Math.Max(0, childConstraint.height - outh);
                }
            }

            var maxw = gs.GetMaxWidth(this, totalColStars, childConstraint.width);
            if (maxw.HasValue && maxw < childConstraint.width)
            {
                childConstraint.width = maxw.Value;
            }

            var maxh = gs.GetMaxHeight(this, totalRowStars, childConstraint.height);
            if (maxh.HasValue && maxh < childConstraint.height)
            {
                childConstraint.height = maxh.Value;
            }

            child.Measure(childConstraint);
            var childSize = child.DesiredSize;
            foreach (var col in gs.GetCols(this).Where(d => d.HasAutoSize))
            {
                if (!col.DesiredSize.HasValue)
                {
                    col.DesiredSize = childSize.width;
                }
                else
                {
                    col.DesiredSize = Math.Max(col.DesiredSize.Value, childSize.width);
                }
            }

            foreach (var row in gs.GetRows(this).Where(d => d.HasAutoSize))
            {
                if (!row.DesiredSize.HasValue)
                {
                    row.DesiredSize = childSize.height;
                }
                else
                {
                    row.DesiredSize = Math.Max(row.DesiredSize.Value, childSize.height);
                }
            }
        }

        // handle defined size cols/rows and finish others
        foreach (var col in Columns)
        {
            if (col.HasDefinedSize)
            {
                col.DesiredSize = col.Size;
            }
            else if (!col.DesiredSize.HasValue)
            {
                col.DesiredSize = 0;
            }
        }

        foreach (var row in Rows)
        {
            if (row.HasDefinedSize)
            {
                row.DesiredSize = row.Size;
            }
            else if (!row.DesiredSize.HasValue)
            {
                row.DesiredSize = 0;
            }
        }

        MeasureStarDimensions(constraint);

        // all dimensions with auto size that only contains float.NaN did take the maximum size which has the effect of artificially make the desired size too big!
        // reduce them now.
        var maxSizedCols = new List<GridColumn>();
        var othersSize = 0f;
        var stretch = HorizontalAlignment == Alignment.Stretch;
        foreach (var col in Columns)
        {
            if (stretch &&
                col.HasAutoSize &&
                _childrenByDimensions.TryGetValue(col, out var list) &&
                list.All(c => c.Width.IsNotSet()))
            {
                maxSizedCols.Add(col);
                continue;
            }
            othersSize += col.DesiredSize.Value;
        }

        if (maxSizedCols.Count > 0)
        {
            var maxSize = Math.Max(0, constraint.width - othersSize) / maxSizedCols.Count;
            if (maxSize.IsSet())
            {
                foreach (var col in maxSizedCols)
                {
                    col.DesiredSize = maxSize;
                }
            }
        }

        var maxSizedRows = new List<GridRow>();
        othersSize = 0f;
        stretch = VerticalAlignment == Alignment.Stretch;
        foreach (var row in Rows)
        {
            if (stretch &&
                row.HasAutoSize &&
                _childrenByDimensions.TryGetValue(row, out var list) &&
                list.All(c => c.Height.IsNotSet()))
            {
                maxSizedRows.Add(row);
                continue;
            }
            othersSize += row.DesiredSize.Value;
        }

        if (maxSizedRows.Count > 0)
        {
            var maxSize = Math.Max(0, constraint.height - othersSize) / maxSizedRows.Count;
            if (maxSize.IsSet())
            {
                foreach (var row in maxSizedRows)
                {
                    row.DesiredSize = maxSize;
                }
            }
        }

        var width = Columns.Sum(d => d.DesiredSize.Value);
        var height = Rows.Sum(d => d.DesiredSize.Value);
        return new D2D_SIZE_F(width, height);
    }

    private void InitializeDimensionsChildren()
    {
        _childrenByDimensions = [];
        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var gs = GridSet.Get(this, child);

            // build dic dim => children
            foreach (var col in gs.GetCols(this))
            {
                if (!_childrenByDimensions.TryGetValue(col, out var list))
                {
                    list = [];
                    _childrenByDimensions.Add(col, list);
                }
                list.Add(child);
            }

            foreach (var row in gs.GetRows(this))
            {
                if (!_childrenByDimensions.TryGetValue(row, out var list))
                {
                    list = [];
                    _childrenByDimensions.Add(row, list);
                }
                list.Add(child);
            }
        }
    }

    // handle stars cols/rows
    // stars cols/rows have all equal size
    // this equal size is determined using the max size that all children for all star rows take
    private void MeasureStarDimensions(D2D_SIZE_F constraint)
    {
        if (_childrenByDimensions == null)
        {
            InitializeDimensionsChildren();
        }

        var padding = Padding;

        var colsStars = 0f;
        var starCols = new List<GridColumn>();
        foreach (var col in Columns.Where(d => d.HasStarSize))
        {
            colsStars += col.Stars;
            starCols.Add(col);
        }

        if (starCols.Count > 0)
        {
            if (Width.IsNotSet() && constraint.width.IsSet())
            {
                var left = Math.Max(0, constraint.width - Columns.Where(d => !d.HasStarSize).Sum(d => d.DesiredSize.Value));
                var colSizeUnit = (left - padding.Width * Columns.Count) / colsStars;
                foreach (var col in starCols)
                {
                    col.DesiredSize = colSizeUnit * col.Stars;
                }
            }
            else
            {
                // find out the max size for each col and apply to others
                var max = 0f;
                var starsForMax = 0f;
                foreach (var col in starCols)
                {
                    if (!_childrenByDimensions.TryGetValue(col, out var list))
                        continue;

                    foreach (var child in list)
                    {
                        col.DesiredSize = Math.Max(col.DesiredSize.Value, child.DesiredSize.width);
                    }

                    if (col.DesiredSize.Value > max)
                    {
                        max = col.DesiredSize.Value;
                        starsForMax = col.Stars;
                    }
                }

                // we may have max = 0 here (star cols w/o children)
                foreach (var col in starCols)
                {
                    col.DesiredSize = starsForMax != 0 ? (max * col.Stars) / starsForMax : 0;
                }
            }
        }

        var rowsStars = 0f;
        var starRows = new List<GridRow>();
        foreach (var row in Rows.Where(d => d.HasStarSize))
        {
            rowsStars += row.Stars;
            starRows.Add(row);
        }

        if (starRows.Count > 0)
        {
            if (Height.IsNotSet() && constraint.height.IsSet())
            {
                var left = Math.Max(0, constraint.height - Rows.Where(d => !d.HasStarSize).Sum(d => d.DesiredSize.Value));
                var rowSizeUnit = (left - padding.Height * Rows.Count) / rowsStars;
                foreach (var row in starRows)
                {
                    row.DesiredSize = rowSizeUnit * row.Stars;
                }
            }
            else
            {
                // find out the max size for each row and apply to others
                var max = 0f;
                var starsForMax = 0f;
                foreach (var row in starRows)
                {
                    if (!_childrenByDimensions.TryGetValue(row, out var list))
                        continue;

                    foreach (var child in list)
                    {
                        row.DesiredSize = Math.Max(row.DesiredSize.Value, child.DesiredSize.height);
                    }

                    if (row.DesiredSize.Value > max)
                    {
                        max = row.DesiredSize.Value;
                        starsForMax = row.Stars;
                    }
                }

                // we may have max = 0 here (star rows w/o children)
                foreach (var row in starRows)
                {
                    row.DesiredSize = starsForMax != 0 ? (max * row.Stars) / starsForMax : 0;
                }
            }
        }
    }

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        if (Rows.Any(r => !r.DesiredSize.HasValue) || Columns.Any(c => !c.DesiredSize.HasValue))
        {
            // TODO: this is suspect
            MeasureCore(_lastMeasureSize.Value);
        }

        var finalSize = finalRect.Size;
        //Application.Trace("this: " + this + " final: " + finalSize);
        var padding = Padding;
        var position = 0f;

        // resize row/cols if needed
        // note this may change their DesiredSize
        MeasureStarDimensions(finalSize);

        foreach (var col in Columns)
        {
            position += padding.left;
            if (position > finalSize.width)
            {
                col.FinalStartPosition = null;
                // and for all next columns
            }
            else
            {
                col.FinalStartPosition = position;
                position += col.DesiredSize.Value;
                position += padding.right;
            }
            //Application.Trace(" col: " + col);
        }

        position = 0f;
        foreach (var row in Rows)
        {
            position += padding.top;
            if (position > finalSize.height)
            {
                row.FinalStartPosition = null;
                // and for all next rows
            }
            else
            {
                row.FinalStartPosition = position;
                position += row.DesiredSize.Value;
                position += padding.bottom;
            }
            //Application.Trace(" row: " + row);
        }

        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var gs = GridSet.Get(this, child);

            // row & col visible?
            if (gs.Col.FinalStartPosition.HasValue && gs.Row.FinalStartPosition.HasValue)
            {
                var colsWidth = gs.GetCols(this).Sum(d => d.DesiredSize.Value);
                var rowsHeight = gs.GetRows(this).Sum(d => d.DesiredSize.Value);

                var finalWidth = Math.Min(child.DesiredSize.width, colsWidth);
                var finalHeight = Math.Min(child.DesiredSize.height, rowsHeight);

                var rect = D2D_RECT_F.Sized(gs.Col.FinalStartPosition.Value, gs.Row.FinalStartPosition.Value, finalWidth, finalHeight);
                rect.right = Math.Min(finalSize.width, rect.right);
                rect.bottom = Math.Min(finalSize.height, rect.bottom);

                Alignment horizontalAlignment;
                if (((IPropertyOwner)child).IsPropertyValueSet(HorizontalAlignmentProperty))
                {
                    horizontalAlignment = child.HorizontalAlignment;
                }
                else if (gs.Col.DefaultAlignment.HasValue)
                {
                    horizontalAlignment = gs.Col.DefaultAlignment.Value;
                }
                else
                {
                    horizontalAlignment = child.HorizontalAlignment;
                }

                // if width is set, don't use Stretch
                if (horizontalAlignment == Alignment.Stretch && child.Width.IsSet())
                {
                    horizontalAlignment = Alignment.Near;
                }

                Alignment verticalAlignment;
                if (((IPropertyOwner)child).IsPropertyValueSet(VerticalAlignmentProperty))
                {
                    verticalAlignment = child.VerticalAlignment;
                }
                else if (gs.Row.DefaultAlignment.HasValue)
                {
                    verticalAlignment = gs.Row.DefaultAlignment.Value;
                }
                else
                {
                    verticalAlignment = child.VerticalAlignment;
                }

                // if width is set, don't use Stretch
                if (verticalAlignment == Alignment.Stretch && child.Height.IsSet())
                {
                    verticalAlignment = Alignment.Near;
                }

                var colSize = new D2D_SIZE_F(colsWidth, rowsHeight);
                rect = GetRect(colSize, horizontalAlignment, verticalAlignment, rect);

                // integralize child
                rect = D2D_RECT_F.Sized(rect.left.Floor(), rect.top.Floor(), rect.Width.Ceiling(), rect.Height.Ceiling());
                //Application.Trace(" child: " + child + " gs: " + gs + " rect: " + rect);
                child.Arrange(rect);
            }
            else
            {
                child.Arrange(new D2D_RECT_F());
            }
        }

        _childrenByDimensions.Clear();
        _childrenByDimensions = null;
    }

    private static D2D_RECT_F GetRect(D2D_SIZE_F parentSize, Alignment horizontalAlignment, Alignment verticalAlignment, D2D_RECT_F childRect)
    {
        switch (horizontalAlignment)
        {
            case Alignment.Center:
                childRect.left += (parentSize.width - childRect.Width) / 2;
                break;

            case Alignment.Far:
                childRect.left += parentSize.width - childRect.Width;
                break;

            case Alignment.Stretch:
                childRect.Width = parentSize.width;
                break;
        }

        switch (verticalAlignment)
        {
            case Alignment.Center:
                childRect.top += (parentSize.height - childRect.Height) / 2;
                break;

            case Alignment.Far:
                childRect.top += parentSize.height - childRect.Height;
                break;

            case Alignment.Stretch:
                childRect.Height = parentSize.height;
                break;
        }

        return childRect;
    }

    private GridColumn GetColumn(int index)
    {
        index = Math.Min(index, Columns.Count - 1);
        index = Math.Max(0, index);
        return Columns[index];
    }

    private GridRow GetRow(int index)
    {
        index = Math.Min(index, Rows.Count - 1);
        index = Math.Max(0, index);
        return Rows[index];
    }

    private class GridSet
    {
        public int ColIndex;
        public int ColSpan;
        public int RowIndex;
        public int RowSpan;
        public int LastColIndex => ColIndex + ColSpan - 1;
        public int LastRowIndex => RowIndex + RowSpan - 1;
        public GridColumn Col;
        public GridRow Row;
        public GridColumn LastCol;
        public GridRow LastRow;

        public float? GetMaxWidth(Grid grid, float totalColStars, float widthForStars)
        {
            var max = 0f;
            var totalStars = 0f;
            foreach (var col in GetCols(grid))
            {
                if (col.HasAutoSize)
                    return null;

                if (col.HasStarSize)
                {
                    totalStars += col.Stars;
                }
                else if (col.HasDefinedSize)
                {
                    max += col.Size;
                }
            }

            if (totalStars > 0 && totalColStars != 0)
            {
                max += widthForStars * totalStars / totalColStars;
            }
            return max;
        }

        public float? GetMaxHeight(Grid grid, float totalRowStars, float heightForStars)
        {
            var max = 0f;
            var totalStars = 0f;
            foreach (var row in GetRows(grid))
            {
                if (row.HasAutoSize)
                    return null;

                if (row.HasStarSize)
                {
                    totalStars += row.Stars;
                }
                else if (row.HasDefinedSize)
                {
                    max += row.Size;
                }
            }

            if (totalStars > 0 && totalRowStars != 0)
            {
                max += heightForStars * totalStars / totalRowStars;
            }
            return max;
        }

        public float? GetDefinedColSize(Grid grid)
        {
            var sum = 0f;
            foreach (var col in GetCols(grid))
            {
                if (!col.HasDefinedSize)
                    return null;

                sum += col.Size;
            }
            return sum;
        }

        // get sum of defined size columns out of this columns range
        public float GetOutDefinedRowSize(Grid grid)
        {
            var sum = 0f;
            var thisRows = new HashSet<GridRow>(GetRows(grid));
            foreach (var row in grid.Rows)
            {
                if (thisRows.Contains(row))
                    continue;

                if (row.HasDefinedSize)
                {
                    sum += row.Size;
                }
            }
            return sum;
        }

        // get sum of defined size rows out of this rows range
        public float GetOutDefinedColSize(Grid grid)
        {
            var sum = 0f;
            var thisCols = new HashSet<GridColumn>(GetCols(grid));
            foreach (var col in grid.Columns)
            {
                if (thisCols.Contains(col))
                    continue;

                if (col.HasDefinedSize)
                {
                    sum += col.Size;
                }
            }
            return sum;
        }

        public float? GetDefinedRowSize(Grid grid)
        {
            var sum = 0f;
            foreach (var row in GetRows(grid))
            {
                if (!row.HasDefinedSize)
                    return null;

                sum += row.Size;
            }
            return sum;
        }

        public IEnumerable<GridColumn> GetCols(Grid grid)
        {
            for (var i = ColIndex; i <= LastColIndex; i++)
            {
                yield return grid.Columns[i];
            }
        }

        public IEnumerable<GridRow> GetRows(Grid grid)
        {
            for (var i = RowIndex; i <= LastRowIndex; i++)
            {
                yield return grid.Rows[i];
            }
        }

        public override string ToString() => "C: " + ColIndex + (LastColIndex > ColIndex ? "-" + LastColIndex : null) + ",R: " + RowIndex + (LastRowIndex > RowIndex ? "-" + LastRowIndex : null);

        public static GridSet Get(Grid grid, Visual visual)
        {
            var gs = new GridSet
            {
                ColIndex = Math.Max(0, Math.Min(grid.Columns.Count - 1, GetColumn(visual)))
            };
            gs.Col = grid.GetColumn(gs.ColIndex);
            gs.RowIndex = Math.Max(0, Math.Min(grid.Rows.Count - 1, GetRow(visual)));
            gs.Row = grid.GetRow(gs.RowIndex);
            // note: col & rows can be hidden (but we have span, etc.)

            gs.ColSpan = GetColumnSpan(visual);
            if (gs.ColSpan == int.MaxValue)
            {
                gs.ColSpan = Math.Max(1, grid.Columns.Count - gs.ColIndex);
            }
            else
            {
                gs.ColSpan = Math.Max(1, Math.Min(grid.Columns.Count - gs.ColIndex, gs.ColSpan));
            }

            gs.RowSpan = GetRowSpan(visual);
            if (gs.RowSpan == int.MaxValue)
            {
                gs.RowSpan = Math.Max(1, grid.Rows.Count - gs.RowIndex);
            }
            else
            {
                gs.RowSpan = Math.Max(1, Math.Min(grid.Rows.Count - gs.RowIndex, gs.RowSpan));
            }
            gs.LastCol = grid.GetColumn(gs.LastColIndex);
            gs.LastRow = grid.GetRow(gs.LastRowIndex);
            return gs;
        }
    }
}
