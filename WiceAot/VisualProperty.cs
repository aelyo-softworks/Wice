namespace Wice;

/// <summary>
/// Describes a dynamic property specialized for <see cref="Visual"/> instances,
/// augmenting <see cref="BaseObjectProperty"/> with invalidation semantics for
/// the layout/render pipeline and a helper to resolve values up the visual tree.
/// </summary>
public class VisualProperty : BaseObjectProperty
{
    /// <summary>
    /// Registers a new <see cref="VisualProperty"/> for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">The CLR type of the property value.</typeparam>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="modes">The invalidation modes to apply when the property changes.</param>
    /// <param name="defaultValue">The default value (optional).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <param name="options">Behavior flags (e.g., UI-thread requirements).</param>
    /// <returns>The registered property descriptor.</returns>
    public static VisualProperty Add<
#if !NETFRAMEWORK
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    T>(Type declaringType, string name, VisualPropertyInvalidateModes modes, T? defaultValue = default, ConvertDelegate? convert = null, ChangingDelegate? changing = null, ChangedDelegate? changed = null, BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread) => Add(declaringType, name, typeof(T), modes, defaultValue, convert, changing, changed, options);

    /// <summary>
    /// Registers a new <see cref="VisualProperty"/> for <paramref name="declaringType"/> using the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="type">The CLR type of the property value.</param>
    /// <param name="modes">The invalidation modes to apply when the property changes.</param>
    /// <param name="defaultValue">The default value (optional).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <param name="options">Behavior flags (e.g., UI-thread requirements).</param>
    /// <returns>The registered property descriptor.</returns>
    public static VisualProperty Add(
        Type declaringType,
        string name,
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        Type type,
        VisualPropertyInvalidateModes modes,
        object? defaultValue = null,
        ConvertDelegate? convert = null,
        ChangingDelegate? changing = null,
        ChangedDelegate? changed = null,
        BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread
        ) => Add(new VisualProperty(declaringType, name, type, modes, defaultValue, convert, changing, changed, options));

    /// <summary>
    /// Registers the given <paramref name="property"/> with the global registry.
    /// </summary>
    /// <param name="property">The property descriptor to add.</param>
    /// <returns>The same instance for chaining.</returns>
    public static VisualProperty Add(VisualProperty property) => (VisualProperty)Add((BaseObjectProperty)property);

    private VisualPropertyInvalidateModes _invalidateModes;

    /// <summary>
    /// Initializes a new instance of <see cref="VisualProperty"/>.
    /// </summary>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="type">The CLR type of the property value.</param>
    /// <param name="modes">The invalidation modes to apply when the property changes.</param>
    /// <param name="defaultValue">The default value (optional).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <param name="options">Behavior flags (e.g., UI-thread requirements).</param>
    public VisualProperty(
        Type declaringType,
        string name,
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        Type type,
        VisualPropertyInvalidateModes modes,
        object? defaultValue = null,
        ConvertDelegate? convert = null,
        ChangingDelegate? changing = null,
        ChangedDelegate? changed = null,
        BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread)
        : base(declaringType, name, type, defaultValue, convert, changing, changed, options)
    {
        ExceptionExtensions.ThrowIfNull(declaringType, nameof(declaringType));
        InvalidateModes = modes;
    }

    /// <summary>
    /// Gets or sets the invalidation modes that should be triggered when this property's value changes.
    /// </summary>
    public virtual VisualPropertyInvalidateModes InvalidateModes { get => _invalidateModes; set { if (IsFrozen) throw new InvalidOperationException(); _invalidateModes = value; } }

    /// <summary>
    /// Gets the effective value of this property for the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The visual on which to resolve the value.</param>
    /// <param name="recursive">
    /// When <see langword="true"/>, climbs the visual tree through <see cref="Visual.Parent"/> until a value is found;
    /// when <see langword="false"/>, returns only the locally stored value or the default.
    /// </param>
    /// <returns>
    /// The stored value if present; otherwise, the first ancestor value (when <paramref name="recursive"/> is true);
    /// otherwise the converted <see cref="BaseObjectProperty.DefaultValue"/>.
    /// </returns>
    public virtual object? GetValue(Visual target, bool recursive = true)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        if (((IPropertyOwner)target).TryGetPropertyValue(this, out var value))
            return value;

        if (!recursive || target.Parent == null)
            return DefaultValue;

        return GetValue(target.Parent, true);
    }

    /// <summary>
    /// Maps a single <see cref="InvalidateMode"/> to the corresponding <see cref="VisualPropertyInvalidateModes"/> flag.
    /// </summary>
    /// <param name="mode">The basic invalidate mode.</param>
    /// <returns>The equivalent visual property invalidation flag.</returns>
    public static VisualPropertyInvalidateModes ToInvalidateModes(InvalidateMode mode) => mode switch
    {
        InvalidateMode.Render => VisualPropertyInvalidateModes.Render,
        InvalidateMode.Arrange => VisualPropertyInvalidateModes.Arrange,
        InvalidateMode.Measure => VisualPropertyInvalidateModes.Measure,
        InvalidateMode.None => VisualPropertyInvalidateModes.None,
        _ => throw new NotSupportedException(),
    };

    /// <summary>
    /// Computes the parent-oriented <see cref="InvalidateMode"/> implied by <paramref name="modes"/>.
    /// </summary>
    /// <param name="modes">The visual property invalidation flags.</param>
    /// <returns>
    /// The parent invalidation mode, preferring Measure over Arrange over Render when multiple flags are present;
    /// otherwise <see cref="InvalidateMode.None"/>.
    /// </returns>
    public static InvalidateMode GetParentInvalidateMode(VisualPropertyInvalidateModes modes)
    {
        if (modes.HasFlag(VisualPropertyInvalidateModes.ParentMeasure))
            return InvalidateMode.Measure;

        if (modes.HasFlag(VisualPropertyInvalidateModes.ParentArrange))
            return InvalidateMode.Arrange;

        if (modes.HasFlag(VisualPropertyInvalidateModes.ParentRender))
            return InvalidateMode.Render;

        return InvalidateMode.None;
    }

    /// <summary>
    /// Computes the effective self <see cref="InvalidateMode"/> implied by <paramref name="modes"/>.
    /// </summary>
    /// <param name="modes">The visual property invalidation flags.</param>
    /// <returns>
    /// The self invalidation mode, preferring Measure over Arrange over Render when multiple flags are present
    /// (including their Parent* counterparts); otherwise <see cref="InvalidateMode.None"/>.
    /// </returns>
    public static InvalidateMode GetInvalidateMode(VisualPropertyInvalidateModes modes)
    {
        if (modes.HasFlag(VisualPropertyInvalidateModes.Measure) || modes.HasFlag(VisualPropertyInvalidateModes.ParentMeasure))
            return InvalidateMode.Measure;

        if (modes.HasFlag(VisualPropertyInvalidateModes.Arrange) || modes.HasFlag(VisualPropertyInvalidateModes.ParentArrange))
            return InvalidateMode.Arrange;

        if (modes.HasFlag(VisualPropertyInvalidateModes.Render) || modes.HasFlag(VisualPropertyInvalidateModes.ParentRender))
            return InvalidateMode.Render;

        return InvalidateMode.None;
    }

    /// <summary>
    /// Returns a string representation including the invalidation modes.
    /// </summary>
    public override string ToString() => base.ToString() + " im: " + InvalidateModes;
}
