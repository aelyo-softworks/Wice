namespace Wice.Effects;

public class EffectProperty : VisualProperty
{
    public static EffectProperty Define<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => new(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);

    public static EffectProperty Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, T? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => Add(declaringType, name, typeof(T), index, mapping, defaultValue, mode, converter);
    public static EffectProperty Add(Type declaringType, string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, object? defaultValue = default, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null) => Add(new EffectProperty(declaringType, name, type, index, mapping, defaultValue, mode, converter));
    public static EffectProperty Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Type declaringType, string name, int index, T defaultValue, ConvertDelegate? converter = null) => Add(new EffectProperty(declaringType, name, typeof(T), index, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, defaultValue, converter: converter));
    public static EffectProperty Add(EffectProperty property) => (EffectProperty)Add((VisualProperty)property);

    private int _index;
    private GRAPHICS_EFFECT_PROPERTY_MAPPING _mapping;

    public EffectProperty(Type declaringType, string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping = GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT, object? defaultValue = null, VisualPropertyInvalidateModes mode = VisualPropertyInvalidateModes.Render, ConvertDelegate? converter = null)
        : base(declaringType, name, type, mode, defaultValue, converter)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        Index = index;
        Mapping = mapping;
    }

    public int Index { get => _index; set { if (IsFrozen) throw new InvalidOperationException(); _index = value; } }
    public GRAPHICS_EFFECT_PROPERTY_MAPPING Mapping { get => _mapping; set { if (IsFrozen) throw new InvalidOperationException(); _mapping = value; } }

    public override string ToString() => base.ToString() + " im: " + InvalidateModes;
}
