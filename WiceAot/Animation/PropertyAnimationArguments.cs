namespace Wice.Animation;

/// <summary>
/// Represents the arguments required to configure a property animation, including the target object,  the property to
/// animate, and the duration of the animation.
/// </summary>
public class PropertyAnimationArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAnimationArguments"/> class with the specified target,
    /// target property, and duration.
    /// </summary>
    /// <param name="target">The object whose property will be animated.</param>
    /// <param name="targetProperty">The property of the target object to animate.</param>
    /// <param name="duration">The duration of the animation, in milliseconds. Must be a non-negative value.</param>
    public PropertyAnimationArguments(BaseObject target, BaseObjectProperty targetProperty, int duration)
        : this(target, targetProperty, TimeSpan.FromMilliseconds(duration))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAnimationArguments"/> class with the specified target
    /// object, target property, and animation duration.
    /// </summary>
    /// <param name="target">The object to which the animation will be applied. Can be <see langword="null"/> if no specific target is
    /// required.</param>
    /// <param name="targetProperty">The property of the target object that the animation will modify. Cannot be <see langword="null"/>.</param>
    /// <param name="duration">The duration of the animation.</param>
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

    /// <summary>
    /// Gets the weak reference to the target <see cref="BaseObject"/>.
    /// </summary>
    public WeakReference<BaseObject>? TargetReference { get; }

    /// <summary>
    /// Gets or sets the target property associated with the current object.
    /// </summary>
    public virtual BaseObjectProperty TargetProperty { get; set; }

    /// <summary>
    /// Gets or sets the duration of the operation or event.
    /// </summary>
    public virtual TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets a value indicating whether the target property is valid.
    /// </summary>
    public virtual bool IsValid => TargetProperty != null;
}
