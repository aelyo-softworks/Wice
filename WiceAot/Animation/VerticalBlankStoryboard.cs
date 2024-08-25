namespace Wice.Animation;

public partial class VerticalBlankStoryboard(Window window) : Storyboard(window)
{
    private readonly VerticalBlankTicker _ticker = new();

    public virtual int TickDivider { get => _ticker.TickDivider; set => _ticker.TickDivider = value; }

    private void OnTickerTick(object? sender, EventArgs e) => OnTick();

    public override void Stop()
    {
        _ticker.Tick -= OnTickerTick;
        base.Stop();
    }

    public override void Start()
    {
        base.Start();
        _ticker.Tick += OnTickerTick;
        _ticker.EnsureStarted();
    }
}
