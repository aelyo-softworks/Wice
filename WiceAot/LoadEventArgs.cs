namespace Wice;

public class LoadEventArgs : CancelEventArgs
{
    public virtual int NextEventBatchSize { get; set; } = 10000;
    public virtual int LoadedLines { get; set; }
}
