using System;

namespace Wice.Animation
{
    public class BackEase : IEasingFunction
    {
        public BackEase()
        {
            Amplitude = 1;
        }

        public float Amplitude { get; set; }

        public float Ease(float normalizedTime)
        {
            var amp = Math.Max(0, Amplitude);
            return (float)(Math.Pow(normalizedTime, 3) - normalizedTime * amp * Math.Sin(Math.PI * normalizedTime));
        }
    }
}
