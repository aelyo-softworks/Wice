namespace Wice.Animation;

public class QuadraticEase : IEasingFunction
{
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime;
}
