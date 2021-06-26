using System;
using System.Threading;
using DirectN;
using Windows.UI.Composition.Core;

namespace Wice.Animation
{
    public class CompositorControllerStoryboard : Storyboard
    {
        private int _tickDivider = 1;
        private volatile bool _stop;

        public CompositorControllerStoryboard(Window window)
            : base(window)
        {
        }

        public virtual int TickDivider { get => _tickDivider; set => _tickDivider = value.Clamp(1); }

        public override void Stop()
        {
            _stop = true;
            base.Stop();
        }

        private async void Callback(object state)
        {
            Application.Trace("t:" + Thread.CurrentThread.ManagedThreadId + " init");
            var controller = (CompositorController)state;
            var smallCount = 0;
            do
            {
                if (_stop)
                    return;

                await controller.EnsurePreviousCommitCompletedAsync().AsTask().ConfigureAwait(false);
                if (_stop)
                    return;

                if (_tickDivider > 1)
                {
                    smallCount++;
                    if (smallCount != _tickDivider)
                        continue;

                    smallCount = 0;
                }

                Application.Trace("t:" + Thread.CurrentThread.ManagedThreadId + " tick");
                OnTick();
            }
            while (true);
        }

        public override void Start()
        {
            var controller = Window?.CompositorController;
            if (controller == null)
                throw new InvalidOperationException();

            base.Start();
            var t = new Thread(Callback);
            t.Name = "_ccsb_";
            t.IsBackground = true;
            t.Start(controller);
        }
    }
}
