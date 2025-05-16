namespace Wice.Animation;

public class PropertyAnimationArguments
{
    public PropertyAnimationArguments(BaseObject target, BaseObjectProperty targetProperty, int duration)
        : this(target, targetProperty, TimeSpan.FromMilliseconds(duration))
    {
    }

    public PropertyAnimationArguments(BaseObject target, BaseObjectProperty targetProperty, TimeSpan duration)
    {
        ExceptionExtensions.ThrowIfNull(targetProperty, nameof(targetProperty));
        if (target != null)
        {
            // null target is possible
            TargetReference = new WeakReference<BaseObject>(target);
        }

        TargetProperty = targetProperty;
        Duration = duration;
    }

    public WeakReference<BaseObject>? TargetReference { get; private set; }
    public virtual BaseObjectProperty TargetProperty { get; set; }
    public virtual TimeSpan Duration { get; set; }
    public virtual bool IsValid => TargetProperty != null;
}
