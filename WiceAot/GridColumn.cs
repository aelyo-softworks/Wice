namespace Wice;

public partial class GridColumn : GridDimension
{
    public GridColumn()
    {
    }

    public override int Index => Parent != null ? Parent.Columns.IndexOf(this) : -1;

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

    public override string ToString() => "C:" + base.ToString();
}
