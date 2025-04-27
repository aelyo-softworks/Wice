namespace Wice.Samples.Gallery.Samples.Media.WebView;

public class WebViewSample : Sample, IDisposable
{
    private bool _disposedValue;
    private Wice.WebView? _webView;

    public override string Description => "A Microsoft Edge (Chromium) based visual that hosts HTML content.";

    public override void Layout(Visual parent)
    {
        var webView = new Wice.WebView();
        _webView = webView; // remove from display
        parent.Children.Add(webView);
        Dock.SetDockType(webView, DockType.Top); // remove from display
        webView.Width = 900;
        webView.Height = 500;
        webView.Margin = 10;
        webView.SourceUri = "https://www.bing.com";
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                _webView?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~WebViewSample()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
