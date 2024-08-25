namespace Wice.Animation;

public class SineEase : IEasingFunction
{
    public float Ease(float normalizedTime) => (float)(1 - Math.Sin(Math.PI * 0.5 * (1 - normalizedTime)));
}
