namespace Wice.Utilities;

public partial class EnumDataSource : DataSource, IEnumerable<EnumBitValue>
{
    private EnumDataSource(IEnumerable<EnumBitValue> values)
        : base(values)
    {
    }

    public new IEnumerable<EnumBitValue> Source => (IEnumerable<EnumBitValue>)base.Source!;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<EnumBitValue> GetEnumerator() => Source.GetEnumerator();

    public static EnumDataSource? FromValue(object? value)
    {
        if (value == null)
            return null;

        return FromType(value.GetType(), value);
    }

    public static EnumDataSource FromType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] Type type, object? value = null, bool? forceFlags = null)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (!type.IsEnum)
            throw new ArgumentException(null, nameof(type));

        if (value == null)
        {
            value = Activator.CreateInstance(type);
        }
        else
        {
            if (!type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(null, nameof(value));
        }

        if (!forceFlags.HasValue)
        {
            if (Conversions.IsFlagsEnum(type))
                return new EnumDataSource(GetFlagsValues(type, value));

            return new EnumDataSource(GetValues(type, value));
        }

        if (forceFlags == true)
            return new EnumDataSource(GetFlagsValues(type, value));

        return new EnumDataSource(GetValues(type, value));
    }

    private static IEnumerable<EnumBitValue> GetEnumValues([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => !f.IsSpecialName))
        {
            var browsable = field.GetCustomAttribute<BrowsableAttribute>();
            if (browsable != null && !browsable.Browsable)
                continue;

            var ev = new EnumBitValue(field.GetValue(null)!, field.Name)
            {
                BitValue = field.GetValue(null)!,
                DisplayName = field.GetCustomAttribute<DescriptionAttribute>()?.Description
            };

            ev.DisplayName ??= Conversions.Decamelize(ev.Name);
            yield return ev;
        }
    }

    private static IEnumerable<EnumBitValue> GetValues([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, object? value)
    {
        foreach (var ev in GetEnumValues(type))
        {
            ev.IsSelected = ev.BitValue?.Equals(value) == true;
            yield return ev;
        }
    }

    private static IEnumerable<EnumBitValue> GetFlagsValues([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, object? value)
    {
        if (value == null)
            yield break;

        var ulv = Conversions.EnumToUInt64(value);
        // note: this suppose the enum is simple (no twice the same value, no value combination)
        foreach (var ev in GetEnumValues(type))
        {
            if (ev.IsZero)
                continue;

            if (ulv != 0)
            {
                var bv = ev.UInt64BitValue;
                if (bv != 0)
                {
                    ev.IsSelected = (ulv & bv) == bv;
                }
            }
            yield return ev;
        }
    }
}
