using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Wice.Samples.Gallery.Samples.Media.PdfView;

public class PdfViewSample : Sample, IDisposable
{
    private bool _disposedValue;
    private Wice.PdfView? _pdfView;

    public override string Description => "A visual that hosts PDF content with transparency support.";

    public override void Layout(Visual parent)
    {
        var dock = new Dock(); // remove from display
        Dock.SetDockType(dock, DockType.Top); // remove from display
        parent.Children.Add(dock); // remove from display

        var pdfView = new Wice.PdfView();
        _pdfView = pdfView; // remove from display
        pdfView.HorizontalAlignment = Alignment.Center;
        Dock.SetDockType(pdfView, DockType.Top); // remove from display
        pdfView.Height = 600;
        pdfView.Margin = 10;
        pdfView.SourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.sample.pdf");

        var buttons = new Stack { Orientation = Orientation.Horizontal, HorizontalAlignment = Alignment.Stretch };
        Dock.SetDockType(buttons, DockType.Top); // remove from display
        dock.Children.Add(buttons);

        var load = new Button { HorizontalAlignment = Alignment.Near, Margin = D2D_RECT_F.Thickness(5) };
        load.Text.Text = "Load PDF File...";
        load.Click += async (s, e) =>
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");

            InitializeWithWindow.Initialize(picker, Window!.Handle);
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await Window!.RunTaskOnMainThread(() => pdfView.SourceFilePath = file.Path);
            }
        };
        buttons.Children.Add(load);

        var next = new Button { HorizontalAlignment = Alignment.Near, Margin = D2D_RECT_F.Thickness(5) };
        next.Text.Text = "Next Page";
        next.Click += (s, e) => pdfView.CurrentPage += 1;
        buttons.Children.Add(next);

        var prev = new Button { HorizontalAlignment = Alignment.Near, Margin = D2D_RECT_F.Thickness(5) };
        prev.Text.Text = "Previous Page";
        prev.Click += (s, e) => pdfView.CurrentPage += -1;
        buttons.Children.Add(prev);

        var pagesCount = new TextBox { VerticalAlignment = Alignment.Center, Margin = D2D_RECT_F.Thickness(5) };
        buttons.Children.Add(pagesCount);

        dock.Children.Add(pdfView);
        pdfView.PageChanged += (s, e) => Window!.RunTaskOnMainThread(() =>
        {
            pagesCount.Text = $"Page {1 + pdfView.CurrentPage}/{pdfView.PagesCount}";
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                _pdfView?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
