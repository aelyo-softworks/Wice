namespace Wice.Animation;

public class CubicEase : IEasingFunction
{
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime;
}
