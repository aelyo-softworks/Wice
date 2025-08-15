namespace Wice;

/// <summary>
/// Represents a row dimension in a <see cref="Grid"/>. Provides navigation to sibling rows,
/// a zero-based <see cref="Index"/> within the parent grid, and enumeration of the row's cells.
/// </summary>
public partial class GridRow : GridDimension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GridRow"/> class.
    /// </summary>
    public GridRow()
    {
    }

    /// <summary>
    /// Gets the zero-based index of this row within its <see cref="Grid"/> parent,
    /// or -1 when the row is not attached to a grid.
    /// </summary>
    public override int Index => Parent != null ? Parent.Rows.IndexOf(this) : -1;

    /// <summary>
    /// Gets the next row in the parent grid, or <see langword="null"/> when this is the last row
    /// or when the row is not attached to a grid.
    /// </summary>
    [Browsable(false)]
    public override GridDimension? Next
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                return null;

            var index = Index;
            if (index < 0 || (index + 1) >= parent.Rows.Count)
                return null;

            return parent.Rows[index + 1];
        }
    }

    /// <summary>
    /// Gets the previous row in the parent grid, or <see langword="null"/> when this is the first row
    /// or when the row is not attached to a grid.
    /// </summary>
    [Browsable(false)]
    public override GridDimension? Previous
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                return null;

            var index = Index;
            if (index <= 0 || (index - 1) >= parent.Rows.Count)
                return null;

            return parent.Rows[index - 1];
        }
    }

    /// <summary>
    /// Enumerates all cell visuals that are positioned in this row of the parent <see cref="Grid"/>.
    /// </summary>
    /// <remarks>
    /// Enumeration yields no items when the row is not attached to a grid or its index is invalid.
    /// </remarks>
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

            foreach (var cell in parent.GetCells(rowIndex: index))
            {
                yield return cell;
            }
        }
    }

    /// <summary>
    /// Returns a string representation prefixed with <c>"R:"</c>, followed by the base dimension string.
    /// </summary>
    /// <returns>A human-readable string for diagnostics.</returns>
    public override string ToString() => "R:" + base.ToString();
}
