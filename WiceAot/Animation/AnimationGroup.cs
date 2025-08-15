namespace Wice.Animation;

/// <summary>
/// Composite animation that groups multiple <see cref="Animation"/> instances and forwards ticks to each child.
/// </summary>
/// <remarks>
/// - Children are created via <see cref="CreateChildren"/> and the collection is locked as read-only in the constructor.<br/>
/// - Override <see cref="CreateChildren"/> to populate the group before it becomes read-only.<br/>
/// - <see cref="OnTick"/> delegates progression to each child without altering their individual state machines.
/// </remarks>
/// <seealso cref="Animation"/>
/// <seealso cref="Storyboard"/>
public partial class AnimationGroup : Animation
{
    /// <summary>
    /// Initializes a new instance of <see cref="AnimationGroup"/>.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="CreateChildren"/> to obtain the backing collection and then sets
    /// <see cref="BaseObjectCollection{T}.IsReadOnly"/> to true. Derived implementations should
    /// fully populate the collection inside <see cref="CreateChildren"/> before returning.
    /// </remarks>
    public AnimationGroup()
    {
        Children = CreateChildren();
        Children.IsReadOnly = true;
    }

    /// <summary>
    /// Gets the child animations contained in this group.
    /// </summary>
    /// <remarks>
    /// The collection is made read-only immediately after construction. To provide and populate
    /// children, override <see cref="CreateChildren"/> and add items before returning the collection.
    /// </remarks>
    public BaseObjectCollection<Animation> Children { get; }

    /// <summary>
    /// Creates the collection that will store child animations.
    /// </summary>
    /// <returns>
    /// A <see cref="BaseObjectCollection{T}"/> to be used as <see cref="Children"/>. The default
    /// implementation returns an empty collection expression.
    /// </returns>
    /// <remarks>
    /// Override to return a pre-populated collection. The returned collection will be marked as
    /// read-only by the constructor.
    /// </remarks>
    protected virtual BaseObjectCollection<Animation> CreateChildren() => [];

    /// <summary>
    /// Advances the group by one tick by ticking each child animation.
    /// </summary>
    /// <remarks>
    /// Enumeration is linear and non-allocating; no timing or state is modified at the group level.
    /// </remarks>
    protected internal override void OnTick()
    {
        foreach (var animation in Children)
        {
            animation.OnTick();
        }
    }
}
