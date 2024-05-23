namespace Wice.Animation;

public class QuinticEase : IEasingFunction
{
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime * normalizedTime * normalizedTime;
}
