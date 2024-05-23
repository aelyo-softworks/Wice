namespace Wice.Animation;

public class ExponentialEase : IEasingFunction
{
    public ExponentialEase()
    {
        Exponent = 2;
    }

    public float Exponent { get; set; }

    public float Ease(float normalizedTime)
    {
        var factor = Exponent;
        if (factor == 0)
            return normalizedTime;

        return (float)((Math.Exp(factor * normalizedTime) - 1) / (Math.Exp(factor) - 1));
    }
}
