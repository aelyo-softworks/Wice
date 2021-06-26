using Wice.Utilities;

namespace Wice.Animation
{
    // same idea as
    // https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/easing-functions
    public class SinglePropertyAnimation : PropertyAnimation
    {
        public SinglePropertyAnimation(PropertyAnimationArguments arguments, float from, float to, IEasingFunction easingFunction = null, EasingMode easingMode = EasingMode.In)
            : base(arguments)
        {
            From = from;
            To = to;
            EasingFunction = easingFunction;
            EasingMode = easingMode;
        }

        public float From { get; }
        public float To { get; }
        public IEasingFunction EasingFunction { get; }
        public EasingMode EasingMode { get; }

        protected override AnimationResult TryGetValue(out object value)
        {
            // special "set" case
            if (From == To)
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
}
