using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;
using Wice.Samples.Gallery.Resources;

namespace Wice.Samples.Gallery.Pages
{
    public class InputsPage : Page
    {
        public InputsPage()
        {
            Title.Text = I18n.T("page.inputs");
            Title.HoverForegroundBrush = new SolidColorBrush(Application.CurrentTheme.ButtonColor);

            var desc = new TextBox();
            SetDockType(desc, DockType.Top);
            desc.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            desc.Text = I18n.T("page.inputs.desc");
            Children.Add(desc);
        }
    }
}
