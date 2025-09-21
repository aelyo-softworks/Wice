namespace Wice;

/// <summary>
/// Represents a column definition in a <see cref="Grid"/>.
/// Provides navigation to adjacent columns and enumerates all cells that belong to this column.
/// </summary>
public partial class GridColumn : GridDimension
{
    /// <summary>
    /// Initializes a new, unattached <see cref="GridColumn"/> instance.
    /// </summary>
    public GridColumn()
    {
    }

    /// <summary>
    /// Gets the zero-based index of this column within its parent grid;
    /// returns -1 when no parent is set or when the column is not found in the parent's collection.
    /// </summary>
    public override int Index => Parent != null ? Parent.Columns.IndexOf(this) : -1;

    /// <summary>
    /// Gets the next <see cref="GridDimension"/> (column) in the parent grid, or <see langword="null"/> if this is the last column
    /// or the column is not attached to a grid.
    /// </summary>
    public override GridDimension? Next
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                return null;

            var index = Index;
            if (index < 0 || (index + 1) >= parent.Columns.Count)
                return null;

            return parent.Columns[index + 1];
        }
    }

    /// <summary>
    /// Gets the previous <see cref="GridDimension"/> (column) in the parent grid, or <see langword="null"/> if this is the first column
    /// or the column is not attached to a grid.
    /// </summary>
    public override GridDimension? Previous
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                return null;

            var index = Index;
            if (index <= 0 || (index - 1) >= parent.Columns.Count)
                return null;

            return parent.Columns[index - 1];
        }
    }

    /// <summary>
    /// Enumerates all cell <see cref="Visual"/>s that are placed in this column across all rows of the parent grid.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<Visual> Cells
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                yield break;

            var index = Index;
            if (index < 0)
                yield break;

            foreach (var cell in parent.GetCells(columnIndex: index))
            {
                yield return cell;
            }
        }
    }

    /// <summary>
    /// Returns a string that represents the current column, prefixed with <c>"C:"</c>,
    /// followed by the base <see cref="GridDimension.ToString()"/> representation.
    /// </summary>
    public override string ToString() => "C:" + base.ToString();
}
