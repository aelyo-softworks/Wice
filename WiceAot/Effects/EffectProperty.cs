namespace Wice.Effects;

/// <summary>
/// Describes a <see cref="VisualProperty"/> specialized for effect graphs (eg. composition/Direct2D effects),
/// augmenting the base descriptor with:
/// - a stable effect parameter <see cref="Index"/> used to serialize values into effect property bags, and
/// - a <see cref="Mapping"/> hint (<see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING"/>) describing how the value
///   should be interpreted by the underlying effect.
/// </summary>
public class EffectProperty : VisualProperty
{
#if NETFRAMEWORK
    /// <summary>
    /// Creates a new <see cref="EffectProperty"/> descriptor for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">Property name (unique per declaring type).</param>
    /// <param name="index">Effect parameter index used when serializing to an effect property bag.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes triggered when the value changes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The new, unfrozen descriptor (not yet registered).</returns>
    public static EffectProperty Define<T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate converter = null) => new(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add<T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate converter = null) => Add(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="type">CLR type stored by the property.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add(Type declaringType, string name, Type type, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, object defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate converter = null) => Add(new EffectProperty(declaringType, name, type, index, mapping, defaultValue, mode, converter));

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add<T>(Type declaringType, string name, int index, T defaultValue, ConvertDelegate converter = null) => Add(new EffectProperty(declaringType, name, typeof(T), index, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, defaultValue, converter: converter));

#else
    /// <summary>
    /// Creates a new <see cref="EffectProperty"/> descriptor for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">Property name (unique per declaring type).</param>
    /// <param name="index">Effect parameter index used when serializing to an effect property bag.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes triggered when the value changes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The new, unfrozen descriptor (not yet registered).</returns>
    public static EffectProperty Define<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => new(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => Add(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="type">CLR type stored by the property.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add(Type declaringType, string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, object? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => Add(new EffectProperty(declaringType, name, type, index, mapping, defaultValue, mode, converter));

    /// <summary>
    /// Registers a new <see cref="EffectProperty"/> for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">CLR type stored by the property.</typeparam>
    /// <param name="declaringType">Type declaring the property.</param>
    /// <param name="name">Property name.</param>
    /// <param name="index">Effect parameter index.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    /// <returns>The registered and frozen descriptor.</returns>
    public static EffectProperty Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, T defaultValue, ConvertDelegate? converter = null) => Add(new EffectProperty(declaringType, name, typeof(T), index, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, defaultValue, converter: converter));
#endif

    /// <summary>
    /// Registers the given <paramref name="property"/> into the global registry, freezing its metadata.
    /// </summary>
    /// <param name="property">The descriptor to register.</param>
    /// <returns>The same instance for chaining.</returns>
    public static EffectProperty Add(EffectProperty property) => (EffectProperty)Add((VisualProperty)property);

    private int _index;
    private GRAPHICS_EFFECT_PROPERTY_MAPPING _mapping;

    /// <summary>
    /// Initializes a new <see cref="EffectProperty"/>.
    /// </summary>
    /// <param name="declaringType">Type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">Property name (unique per declaring type).</param>
    /// <param name="type">CLR type stored by the property.</param>
    /// <param name="index">Effect parameter index used when serializing to an effect property bag.</param>
    /// <param name="mapping">Mapping hint for the underlying graphics effect; defaults to DIRECT.</param>
    /// <param name="defaultValue">Optional default value.</param>
    /// <param name="mode">Invalidation modes triggered when the value changes; defaults to Render.</param>
    /// <param name="converter">Optional conversion hook invoked before storage.</param>
    public EffectProperty(Type declaringType, string name,
#if NETFRAMEWORK
                Type type,
#else
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type,
#endif
        int index,
        GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT,
        object? defaultValue = null,
        VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render,
        ConvertDelegate? converter = null)
        : base(declaringType, name, type, mode, defaultValue, converter)
    {
        ExceptionExtensions.ThrowIfNull(declaringType, nameof(declaringType));

        Index = index;
        Mapping = mapping;
    }

    /// <summary>
    /// Gets or sets the effect parameter index used by the effect pipeline to bind this property's value.
    /// </summary>
    public int Index { get => _index; set { if (IsFrozen) throw new InvalidOperationException(); _index = value; } }

    /// <summary>
    /// Gets or sets the mapping hint describing how the underlying effect interprets this property's value.
    /// </summary>
    public GRAPHICS_EFFECT_PROPERTY_MAPPING Mapping { get => _mapping; set { if (IsFrozen) throw new InvalidOperationException(); _mapping = value; } }

    /// <summary>
    /// Returns a string representation including the invalidation modes inherited from <see cref="VisualProperty"/>.
    /// </summary>
    public override string ToString() => base.ToString() + " im: " + InvalidateModes;
}
