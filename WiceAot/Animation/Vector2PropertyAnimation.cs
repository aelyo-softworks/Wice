namespace Wice.Animation;

public partial class Vector2PropertyAnimation(PropertyAnimationArguments arguments, Vector2 from, Vector2 to, IEasingFunction? easingFunction = null, EasingMode easingMode = EasingMode.In) : PropertyAnimation(arguments)
{
    public Vector2 From { get; } = from;
    public Vector2 To { get; } = to;
    public IEasingFunction? EasingFunction { get; } = easingFunction;
    public EasingMode EasingMode { get; } = easingMode;

    protected override AnimationResult TryGetValue(out object value)
    {
        // special "set" case
        if (From == To)
        {
            value = To;
            return AnimationResult.Stop;
        }

        if (Storyboard == null)
        {
            value = To;
            return AnimationResult.Stop;
        }

        var watch = Storyboard.Watch;
        var elapsedTicks = watch.ElapsedTicks;
        var durationTicks = Duration.Ticks;
        if (elapsedTicks >= durationTicks)
        {
            value = To;
            //Application.Trace("ElapsedTicks: " + elapsedTicks + " To: " + durationTicks + " " + TargetPropertyName + " stop at " + value);
            return AnimationResult.Stop;
        }

        var delta = To - From;
        var normalizedTime = elapsedTicks / (float)durationTicks;
        var eased = EasingFunction.Ease(normalizedTime, EasingMode);
        value = From + eased * delta;
        //Application.Trace("ElapsedTicks: " + elapsedTicks + " To: " + durationTicks + " " + TargetProperty.Name + " value: " + value);
        return AnimationResult.Set;
    }
}
