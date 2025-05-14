namespace Wice.Samples.Gallery.Samples.Media.WebView;

public class WebViewSample : Sample
{
    public override string Description => "A Microsoft Edge (Chromium) based visual that hosts HTML content.";

    public override void Layout(Visual parent)
    {
        var webView = new Wice.WebView();
        parent.Children.Add(webView);
        Dock.SetDockType(webView, DockType.Top); // remove from display
        webView.Width = 900;
        webView.Height = 500;
        webView.Margin = 10;
        webView.SourceUri = "https://www.bing.com";
    }
}
