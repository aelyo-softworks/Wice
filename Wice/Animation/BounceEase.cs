using System;

namespace Wice.Animation
{
    public class BounceEase : IEasingFunction
    {
        public BounceEase()
        {
            Bounces = 3;
            Bounciness = 2;
        }

        public int Bounces { get; set; }
        public float Bounciness { get; set; }

        public float Ease(float normalizedTime)
        {
            var bounces = Math.Max(0, Bounces);
            var bounciness = Math.Min(1.001, (double)Bounciness);

            var pow = Math.Pow(bounciness, bounces);
            var oneMinusBounciness = 1 - bounciness;

            var sumOfUnits = (1 - pow) / oneMinusBounciness + pow * 0.5;
            var unitAtT = normalizedTime * sumOfUnits;

            var bounceAtT = Math.Log(-unitAtT * (1 - bounciness) + 1, bounciness);
            var start = Math.Floor(bounceAtT);
            var end = start + 1;

            var startTime = (1 - Math.Pow(bounciness, start)) / (oneMinusBounciness * sumOfUnits);
            var endTime = (1 - Math.Pow(bounciness, end)) / (oneMinusBounciness * sumOfUnits);

            var midTime = (startTime + endTime) * 0.5;
            var timeRelativeToPeak = normalizedTime - midTime;
            var radius = midTime - startTime;
            var amplitude = Math.Pow(1 / bounciness, bounces - start);

            return (float)(-amplitude / (radius * radius) * (timeRelativeToPeak - radius) * (timeRelativeToPeak + radius));
        }
    }
}
