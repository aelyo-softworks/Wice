namespace Wice;

/// <summary>
/// Represents an <see cref="InvalidateReason"/> caused by a change in a collection,
/// capturing both the child element type involved and the collection change action.
/// </summary>
public class CollectionChangedInvalidateReason
    : InvalidateReason
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionChangedInvalidateReason"/> class.
    /// </summary>
    /// <param name="type">The type associated with the invalidation source.</param>
    /// <param name="childType">The element type of the collection whose change caused the invalidation.</param>
    /// <param name="action">The collection change action that triggered this invalidation.</param>
    /// <param name="innerReason">An optional inner reason providing additional context.</param>
    public CollectionChangedInvalidateReason(Type type, Type childType, NotifyCollectionChangedAction action, InvalidateReason? innerReason = null)
        : base(type, innerReason)
    {
        ExceptionExtensions.ThrowIfNull(childType, nameof(childType));
        ChildType = childType;
        Action = action;
    }

    /// <summary>
    /// Gets the element type of the collection whose change caused the invalidation.
    /// </summary>
    public Type ChildType { get; }

    /// <summary>
    /// Gets the collection change action that triggered this invalidation.
    /// </summary>
    public NotifyCollectionChangedAction Action { get; }

    /// <summary>
    /// Builds the base descriptive string for this reason, including the action and child type.
    /// </summary>
    /// <returns>
    /// A string composed of the base reason plus the action and child type, e.g.:
    /// "Base[Add](MyChildType)".
    /// </returns>
    protected override string GetBaseString() => base.GetBaseString() + "[" + Action + "](" + ChildType.Name + ")";
}
