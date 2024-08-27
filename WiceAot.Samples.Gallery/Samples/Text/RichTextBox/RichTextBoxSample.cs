namespace Wice.Samples.Gallery.Samples.Text.RichTextBox;

// RichTextBox visual sits on a COM object so we must dispose it on the same thread that created it
public abstract class RichTextBoxSample : Sample, IDisposable
{
    private bool disposedValue;

    protected RichTextBoxSample()
    {
    }

    protected Wice.RichTextBox Rtb { get; } = new Wice.RichTextBox();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                Rtb?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }

    // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RichTextBoxSample()
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
