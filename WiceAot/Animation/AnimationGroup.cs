namespace Wice.Animation;

/// <summary>
/// Composite animation that groups multiple <see cref="Animation"/> instances and forwards ticks to each child.
/// </summary>
public partial class AnimationGroup : Animation
{
    /// <summary>
    /// Initializes a new instance of <see cref="AnimationGroup"/>.
    /// </summary>
    public AnimationGroup()
    {
        Children = CreateChildren();
        Children.IsReadOnly = true;
    }

    /// <summary>
    /// Gets the child animations contained in this group.
    /// </summary>
    public BaseObjectCollection<Animation> Children { get; }

    /// <summary>
    /// Creates the collection that will store child animations.
    /// </summary>
    /// <returns>
    /// A <see cref="BaseObjectCollection{T}"/> to be used as <see cref="Children"/>. The default
    /// implementation returns an empty collection expression.
    /// </returns>
    protected virtual BaseObjectCollection<Animation> CreateChildren() => [];

    /// <summary>
    /// Advances the group by one tick by ticking each child animation.
    /// </summary>
    protected internal override void OnTick()
    {
        foreach (var animation in Children)
        {
            animation.OnTick();
        }
    }
}
