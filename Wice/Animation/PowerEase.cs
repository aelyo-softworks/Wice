using System;

namespace Wice.Animation
{
    public class PowerEase : IEasingFunction
    {
        public PowerEase()
        {
            Power = 2;
        }

        public float Power { get; set; }

        public float Ease(float normalizedTime)
        {
            var power = Math.Max(0, Power);
            return (float)Math.Pow(normalizedTime, power);
        }
    }
}
