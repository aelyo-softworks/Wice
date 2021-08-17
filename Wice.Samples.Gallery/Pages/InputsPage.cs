using DirectN;
using Wice.Samples.Gallery.Resources;

namespace Wice.Samples.Gallery.Pages
{
    public class InputsPage : Page
    {
        public InputsPage()
        {
            Title.Text = I18n.T("page.inputs");
        }
        
        public override string HeaderText => I18n.T("page.inputs");
        public override string IconText => MDL2GlyphResource.Input;
        public override string ToolTipText => I18n.T("page.inputs.tt");
    }
}
