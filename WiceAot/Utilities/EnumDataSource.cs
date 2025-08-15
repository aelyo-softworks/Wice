namespace Wice.Utilities;

/// <summary>
/// Provides a <see cref="DataSource"/> specialized for enumerations, exposing each enum member
/// as an <see cref="EnumBitValue"/> and computing selection state based on a provided enum value.
/// </summary>
/// <remarks>
/// - For regular enums, <see cref="FromType(Type, object?, bool?)"/> marks as selected the single member equal to <paramref name="value"/>.
/// - For <see cref="FlagsAttribute"/> enums, it marks as selected each member whose bit is set in <paramref name="value"/> (zero is skipped).
/// - Browsability: enum fields decorated with <see cref="System.ComponentModel.BrowsableAttribute"/> set to <c>false</c> are excluded.
/// - Display: enum fields decorated with <see cref="System.ComponentModel.DescriptionAttribute"/> use that text; otherwise the name is de-camelized via <c>Conversions.Decamelize</c>.
/// - Trimming: members that perform reflection are annotated with <see cref="System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute"/> to assist the linker.
/// </remarks>
/// <seealso cref="EnumBitValue"/>
/// <seealso cref="DataSource"/>
public partial class EnumDataSource : DataSource, IEnumerable<EnumBitValue>
{
    /// <summary>
    /// Initializes a new instance wrapping the provided sequence of <see cref="EnumBitValue"/>.
    /// </summary>
    /// <param name="values">The sequence of enum items to expose.</param>
    private EnumDataSource(IEnumerable<EnumBitValue> values)
        : base(values)
    {
    }

    /// <summary>
    /// Gets the typed source enumeration of <see cref="EnumBitValue"/>.
    /// </summary>
    public new IEnumerable<EnumBitValue> Source => (IEnumerable<EnumBitValue>)base.Source!;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns a typed enumerator over the <see cref="Source"/>.
    /// </summary>
    public IEnumerator<EnumBitValue> GetEnumerator() => Source.GetEnumerator();

    /// <summary>
    /// Creates an <see cref="EnumDataSource"/> from an enum instance.
    /// </summary>
    /// <param name="value">An enum value. If <c>null</c>, returns <c>null</c>.</param>
    /// <returns>
    /// An <see cref="EnumDataSource"/> that enumerates the members of <paramref name="value"/>'s type and reflects its selection state,
    /// or <c>null</c> when <paramref name="value"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This forwards to <see cref="FromType(Type, object?, bool?)"/> using <see cref="object.GetType"/> of <paramref name="value"/>.
    /// The pragma around the call suppresses trimming warnings because the type is discovered at runtime.
    /// </remarks>
    public static EnumDataSource? FromValue(object? value)
    {
        if (value == null)
            return null;

#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        return FromType(value.GetType(), value);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
    }

    /// <summary>
    /// Creates an <see cref="EnumDataSource"/> for the specified enum <paramref name="type"/>.
    /// </summary>
    /// <param name="type">An enum <see cref="Type"/>. Must be an enumeration type.</param>
    /// <param name="value">
    /// Optional enum value used to compute selection state. When <c>null</c>, a default value is created via <see cref="Activator.CreateInstance(Type)"/>.
    /// </param>
    /// <param name="forceFlags">
    /// Optional override to force flags behavior:
    /// - <c>true</c> treats the enum as [Flags] regardless of attributes.
    /// - <c>false</c> treats it as a regular enum.
    /// - <c>null</c> (default) auto-detects via <c>Conversions.IsFlagsEnum(type)</c>.
    /// </param>
    /// <returns>An <see cref="EnumDataSource"/> over the enum members.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="type"/> is not an enum, or when <paramref name="value"/> is not assignable to <paramref name="type"/>.
    /// </exception>
    public static EnumDataSource FromType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
        Type type,
        object? value = null,
        bool? forceFlags = null)
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

    /// <summary>
    /// Enumerates all visible enum members for the given <paramref name="type"/> and materializes them as <see cref="EnumBitValue"/>.
    /// </summary>
    /// <param name="type">An enum <see cref="Type"/>. Its public static fields are inspected.</param>
    /// <returns>
    /// A sequence of <see cref="EnumBitValue"/> instances in declaration order (excluding special-name members and those with
    /// <see cref="System.ComponentModel.BrowsableAttribute.Browsable"/> set to <c>false</c>).
    /// </returns>
    /// <remarks>
    /// - <see cref="EnumBitValue.DisplayName"/> uses <see cref="System.ComponentModel.DescriptionAttribute"/> when present,
    ///   otherwise it is derived by <c>Conversions.Decamelize</c>.
    /// - <see cref="EnumBitValue.BitValue"/> is set to the raw enum value; <see cref="EnumBitValue.Value"/> remains the same construction value.
    /// </remarks>
    private static IEnumerable<EnumBitValue> GetEnumValues(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
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

    /// <summary>
    /// Produces <see cref="EnumBitValue"/> items for a regular (non-flags) enum and marks as selected the item equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="type">An enum <see cref="Type"/>.</param>
    /// <param name="value">The enum value to compare against. May be <c>null</c> to select none.</param>
    private static IEnumerable<EnumBitValue> GetValues(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type,
        object? value)
    {
        foreach (var ev in GetEnumValues(type))
        {
            ev.IsSelected = ev.BitValue?.Equals(value) == true;
            yield return ev;
        }
    }

    /// <summary>
    /// Produces <see cref="EnumBitValue"/> items for a [Flags] enum and marks as selected each item whose bit is set in <paramref name="value"/>.
    /// </summary>
    /// <param name="type">A flags enum <see cref="Type"/>.</param>
    /// <param name="value">The enum value used to extract set bits. When <c>null</c>, yields nothing.</param>
    /// <remarks>
    /// - Zero-valued entries are skipped from selection evaluation (still yielded).
    /// - Assumes a simple enum definition (no duplicated values and no predefined combined values).
    /// - Bit checks are performed using <c>Conversions.EnumToUInt64</c> and <see cref="EnumBitValue.UInt64BitValue"/>.
    /// </remarks>
    private static IEnumerable<EnumBitValue> GetFlagsValues(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type,
        object? value)
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
