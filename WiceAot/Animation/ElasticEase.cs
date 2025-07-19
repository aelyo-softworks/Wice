namespace Wice.Animation;

public class ElasticEase : IEasingFunction
{
    public ElasticEase()
    {
        Oscillations = 3;
        Springiness = 3;
    }

    public int Oscillations { get; set; }
    public float Springiness { get; set; }

    public float Ease(float normalizedTime)
    {
        var oscillations = Math.Max(0.0, Oscillations);
        var springiness = Math.Max(0.0, Springiness);
        double expo;
        if (springiness == 0)
        {
            expo = normalizedTime;
        }
        else
        {
            expo = (Math.Exp(springiness * normalizedTime) - 1) / (Math.Exp(springiness) - 1);
        }

        return (float)(expo * Math.Sin((Math.PI * 2 * oscillations + Math.PI * 0.5) * normalizedTime));
    }
}
