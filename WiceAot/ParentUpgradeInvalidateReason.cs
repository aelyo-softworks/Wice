namespace Wice;

/// <summary>
/// An <see cref="InvalidateReason"/> indicating that a child's invalidation caused
/// the parent to upgrade its invalidation scope/modes.
/// </summary>
public class ParentUpgradeInvalidateReason
    : InvalidateReason
{
    /// <summary>
    /// Initializes a new instance of <see cref="ParentUpgradeInvalidateReason"/>.
    /// </summary>
    /// <param name="type">
    /// The originating type associated with this reason (commonly the declaring type of the property or component).
    /// </param>
    /// <param name="childType">The child visual type that triggered the parent upgrade.</param>
    /// <param name="initialModes">The initial invalidation mode requested for the parent before the upgrade.</param>
    /// <param name="finalModes">
    /// The effective visual property invalidation modes after propagation/upgrade (may include parent flags).
    /// </param>
    /// <param name="innerReason">An optional inner reason providing additional context.</param>
    public ParentUpgradeInvalidateReason(Type type, Type childType, InvalidateMode initialModes, VisualPropertyInvalidateModes finalModes, InvalidateReason? innerReason = null)
        : base(type, innerReason)
    {
        ExceptionExtensions.ThrowIfNull(childType, nameof(childType));
        ChildType = childType;
        InitialModes = initialModes;
        FinalModes = finalModes;
    }

    /// <summary>
    /// Gets the child visual type that triggered the upgrade on the parent.
    /// </summary>
    public Type ChildType { get; }

    /// <summary>
    /// Gets the initial invalidation mode that was requested for the parent before the upgrade.
    /// </summary>
    public InvalidateMode InitialModes { get; }

    /// <summary>
    /// Gets the effective visual property invalidation modes after propagation/upgrade.
    /// </summary>
    public VisualPropertyInvalidateModes FinalModes { get; }

    /// <summary>
    /// Builds the base string used by <see cref="InvalidateReason.ToString"/> to describe this reason.
    /// </summary>
    /// <returns>A concise textual representation including the child type and mode transition.</returns>
    protected override string GetBaseString() => base.GetBaseString() + "(" + ChildType.Name + ")[" + InitialModes + " -> " + FinalModes + "]";
}
