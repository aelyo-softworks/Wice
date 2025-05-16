using System;
using System.ComponentModel;

namespace Wice
{
    public partial class TabPage : BaseObject
    {
        public TabPage()
        {
            Header = CreateHeader();
            Header.MeasureToContent = DimensionOptions.WidthAndHeight;
            Header.Text.IsEnabled = false;
            if (Header == null)
                throw new InvalidOperationException();
        }

        public Tabs? Tab { get; internal set; }
        public int Index => Tab?.Pages.IndexOf(this) ?? -1;

        [Browsable(false)]
        public Visual? Content { get; set; }

        [Browsable(false)]
        public SymbolHeader Header { get; }

        public override string ToString() => $"{Index} '{Header?.Text}'";

        protected virtual SymbolHeader CreateHeader() => new SymbolHeader();
    }
}