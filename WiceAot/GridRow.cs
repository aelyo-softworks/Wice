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

    /// <inheritdoc/>
    public override int Index => Parent != null ? Parent.Rows.IndexOf(this) : -1;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override string ToString() => "R:" + base.ToString();
}
