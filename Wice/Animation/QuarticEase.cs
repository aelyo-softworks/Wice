namespace Wice.Animation
{
    public class QuarticEase : IEasingFunction
    {
        public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime * normalizedTime;
    }
}
