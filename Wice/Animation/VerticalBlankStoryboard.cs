using System;
using DirectN;

namespace Wice.Animation
{
    public class VerticalBlankStoryboard : Storyboard
    {
        private readonly VerticalBlankTicker _ticker = new VerticalBlankTicker();

        public VerticalBlankStoryboard(Window window)
            : base(window)
        {
        }

        public virtual int TickDivider { get => _ticker.TickDivider; set => _ticker.TickDivider = value; }

        private void OnTickerTick(object sender, EventArgs e) => OnTick();

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
}
