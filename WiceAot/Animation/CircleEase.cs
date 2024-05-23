namespace Wice.Animation;

public class CircleEase : IEasingFunction
{
    public float Ease(float normalizedTime)
    {
        normalizedTime = Math.Max(0, Math.Min(1, normalizedTime));
        return (float)(1 - Math.Sqrt(1 - normalizedTime * normalizedTime));
    }
}
