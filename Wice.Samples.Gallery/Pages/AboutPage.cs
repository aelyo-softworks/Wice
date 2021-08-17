using System.Reflection;
using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class AboutPage : Page
    {
        public AboutPage()
        {
            // load logo from embedded resource
            var logo = new Image();
            logo.HorizontalAlignment = Alignment.Near;
            logo.VerticalAlignment = Alignment.Near;
            logo.Stretch = Stretch.None;
            logo.Source = Application.Current.ResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), typeof(Program).Namespace + ".Resources.aelyo_flat.png");
            Children.Add(logo);
        }

        public override string HeaderText => base.HeaderText + " Wice";
        public override string ToolTipText => HeaderText;
        public override string IconText => MDL2GlyphResource.Info;
    }
}
