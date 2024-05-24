namespace Wice;

public class GridRow : GridDimension
{
    public GridRow()
    {
    }

    public override int Index => Parent != null ? Parent.Rows.IndexOf(this) : -1;

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

    public override string ToString() => "R:" + base.ToString();
}
