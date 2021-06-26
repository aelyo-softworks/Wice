using DirectN;

namespace Wice
{
    public class SymbolHeader : Header
    {
        public new TextBox Icon => (TextBox)base.Icon;

        protected sealed override Visual CreateIcon()
        {
            var tb = new TextBox();
            tb.FontFamilyName = Application.CurrentTheme.SymbolFontName;
            tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
            return tb;
        }
    }
}
