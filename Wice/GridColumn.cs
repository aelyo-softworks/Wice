using System.Collections.Generic;
using System.ComponentModel;

namespace Wice
{
    public class GridColumn : GridDimension
    {
        public GridColumn()
        {
        }

        public override int Index => Parent != null ? Parent.Columns.IndexOf(this) : -1;

        public override GridDimension Next
        {
            get
            {
                var index = Index;
                if (index < 0 || (index + 1) >= Parent.Columns.Count)
                    return null;

                return Parent.Columns[index + 1];
            }
        }

        public override GridDimension Previous
        {
            get
            {
                var index = Index;
                if (index <= 0 || (index - 1) >= Parent.Columns.Count)
                    return null;

                return Parent.Columns[index - 1];
            }
        }

        [Browsable(false)]
        public IEnumerable<Visual> Cells
        {
            get
            {
                var index = Index;
                if (index < 0)
                    yield break;

                foreach (var cell in Parent.GetCells(columnIndex: index))
                {
                    yield return cell;
                }
            }
        }

        public override string ToString() => "C:" + base.ToString();
    }
}
