using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;
using Wice.Samples.Gallery.Resources;

namespace Wice.Samples.Gallery.Pages
{
    public class MainPage : Page
    {
        public MainPage()
        {
            Title.Text = I18n.T("page.main");
            Title.HoverForegroundBrush = new SolidColorBrush(Application.CurrentTheme.ButtonColor);

            var desc = new TextBox();
            SetDockType(desc, DockType.Top);
            desc.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            desc.Text = I18n.T("page.main.desc");
            Children.Add(desc);
        }
    }
}
