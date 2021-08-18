using System.Collections.Generic;
using System.ComponentModel;

namespace Wice
{
    public class GridRow : GridDimension
    {
        public GridRow()
        {
        }

        public override int Index => Parent != null ? Parent.Rows.IndexOf(this) : -1;

        public override GridDimension Next
        {
            get
            {
                var index = Index;
                if (index < 0 || (index + 1) >= Parent.Rows.Count)
                    return null;

                return Parent.Rows[index + 1];
            }
        }

        public override GridDimension Previous
        {
            get
            {
                var index = Index;
                if (index <= 0 || (index - 1) >= Parent.Rows.Count)
                    return null;

                return Parent.Rows[index - 1];
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

                foreach (var cell in Parent.GetCells(rowIndex: index))
                {
                    yield return cell;
                }
            }
        }

        public override string ToString() => "R:" + base.ToString();
    }
}
