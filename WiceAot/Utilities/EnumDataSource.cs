namespace Wice.Utilities;

public class EnumDataSource : DataSource, IEnumerable<EnumBitValue>
{
    private EnumDataSource(IEnumerable<EnumBitValue> values)
        : base(values)
    {
    }

    public new IEnumerable<EnumBitValue> Source => (IEnumerable<EnumBitValue>)base.Source;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<EnumBitValue> GetEnumerator() => Source?.GetEnumerator();

    public static EnumDataSource FromValue(object value)
    {
        if (value == null)
            return null;

        return FromType(value.GetType(), value);
    }

    public static EnumDataSource FromType(Type type, object value = null, bool? forceFlags = null)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

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

    private static IEnumerable<EnumBitValue> GetEnumValues(Type type, object value)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => !f.IsSpecialName))
        {
            var browsable = field.GetCustomAttribute<BrowsableAttribute>();
            if (browsable != null && !browsable.Browsable)
                continue;

            var ev = new EnumBitValue(field.GetValue(null));
            ev.Name = field.Name;
            ev.BitValue = field.GetValue(null);
            ev.DisplayName = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (ev.DisplayName == null)
            {
                ev.DisplayName = Decamelizer.Decamelize(ev.Name);
            }
            yield return ev;
        }
    }

    private static IEnumerable<EnumBitValue> GetValues(Type type, object value)
    {
        foreach (var ev in GetEnumValues(type, value))
        {
            ev.IsSelected = ev.BitValue.Equals(value);
            yield return ev;
        }
    }

    private static IEnumerable<EnumBitValue> GetFlagsValues(Type type, object value)
    {
        var ulv = Conversions.EnumToUInt64(value);
        // note: this suppose the enum is simple (no twice the same value, no value combination)
        foreach (var ev in GetEnumValues(type, value))
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
