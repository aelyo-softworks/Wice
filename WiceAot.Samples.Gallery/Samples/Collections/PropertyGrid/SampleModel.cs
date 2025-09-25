#nullable enable
namespace Wice.Samples.Gallery.Samples.Collections.PropertyGrid;

public class AutoObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly Dictionary<string, object?> _values = [];

    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    protected virtual T? GetPropertyValue<T>(T? defaultValue = default, [CallerMemberName] string? name = null)
    {
        if (!_values.TryGetValue(name!, out var value))
            return default;

#if NETFRAMEWORK
        return Conversions.ChangeType(value, defaultValue);
#else
        return DirectN.Extensions.Utilities.Conversions.ChangeType(value, defaultValue);
#endif
    }

    protected virtual bool SetPropertyValue(object? value, [CallerMemberName] string? name = null)
    {
        name.ThrowIfNull(nameof(name));
        if (_values.TryGetValue(name!, out var existing))
        {
            if (value == null)
            {
                if (existing == null)
                    return false;
            }
            else if (value.Equals(existing))
                return false;
        }

        _values[name!] = value;
        OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        return true;
    }
}

public class SampleCustomer : AutoObject
{
    public static SampleCustomer Instance { get; } = new();

    public SampleCustomer()
    {
        Id = Guid.NewGuid();
        ListOfStrings = ["string 1", "string 2"];

        ArrayOfStrings = [.. ListOfStrings];
        CreationDateAndTime = DateTime.Now;
        DateOfBirth = new DateTime(1966, 03, 24);
        Description = "press button to edit...";
        ByteArray1 = [1, 2, 3, 4, 5, 6, 7, 8];
        WebSite = "http://www.aelyo.com";
        Status = SampleStatus.Valid;
        Addresses = [new SampleAddress { Line1 = "2018 156th Avenue NE", City = "Bellevue", State = "WA", ZipCode = 98007, Country = "USA" }];
        DaysOfWeek = SampleDaysOfWeek.WeekDays;
        PercentageOfSatisfaction = 50;
        PreferredColorName = "DodgerBlue";
        SampleNullableBooleanDropDownList = false;
        SampleBooleanDropDownList = true;
        MultiEnumString = "First, Second";
        SubObject = SampleAddress.Parse("1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA");
        LastName = "Héllo";
    }

    [DisplayName("Guid (see menu on right-click)")]
    public Guid Id { get => GetPropertyValue<Guid>(); set => SetPropertyValue(value); }

    [Category("Dates and Times")]
    public DateTime CreationDateAndTime { get => GetPropertyValue<DateTime>(); set => SetPropertyValue(value); }

    [DisplayName("Sub Object (Address)")]
    public SampleAddress? SubObject
    {
        get => GetPropertyValue<SampleAddress>();
        set
        {
            // because it's a sub object we want to update the property grid
            // when inner properties change
            var so = SubObject;
            if (so != null)
            {
                so.PropertyChanged -= OnSubObjectPropertyChanged;
            }

            if (SetPropertyValue(value))
            {
                so = SubObject;
                if (so != null)
                {
                    so.PropertyChanged += OnSubObjectPropertyChanged;
                }

                // these two properties are coupled
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SubObjectAsObject)));
            }
        }
    }

    private void OnSubObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(SubObject)));
        OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(SubObjectAsObject)));
    }

    [DisplayName("Sub Object (Address as Object)")]
    public SampleAddress? SubObjectAsObject { get => SubObject; set => SubObject = value; }

    [PropertyGridPropertyOptions(SortOrder = 10)]
    public string? FirstName { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 20)]
    public string? LastName { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [Category("Dates and Times")]
    [PropertyGridPropertyOptions(SortOrder = 40)]
    public DateTime DateOfBirth { get => GetPropertyValue<DateTime>(); set => SetPropertyValue(value); }

    [Category("Enums")]
    [PropertyGridPropertyOptions(SortOrder = 30)]
    public SampleGender Gender { get => GetPropertyValue<SampleGender>(); set => SetPropertyValue(value); }

    [Category("Enums")]
    public SampleStatus Status
    {
        get => GetPropertyValue<SampleStatus>();
        set
        {
            if (SetPropertyValue(value))
            {
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(StatusColor)));
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(StatusColorString)));
            }
        }
    }

    [DisplayName("Status (colored enum)")]
    [ReadOnly(true)]
    [Category("Enums")]
    public SampleStatus StatusColor { get => Status; set => Status = value; }

    [DisplayName("Status (enum as string list)")]
    [Category("Enums")]
#if NETFRAMEWORK
    public string StatusColorString { get => Status.ToString(); set => Status = Conversions.ChangeType<SampleStatus>(value); }
#else
    public string StatusColorString { get => Status.ToString(); set => Status = DirectN.Extensions.Utilities.Conversions.ChangeType<SampleStatus>(value); }
#endif

    [Category("Enums")]
    public string? MultiEnumString { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [Category("Enums")]
    public string? MultiEnumStringWithDisplay { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [Category("Dates and Times")]
    [Description("This is the timespan tooltip")]
    public TimeSpan TimeSpan { get => GetPropertyValue<TimeSpan>(); set => SetPropertyValue(value); }


#if NETFRAMEWORK
    [PropertyGridPropertyOptions(EditorType = typeof(PasswordEditorCreator))]
#else
    [PropertyGridPropertyOptions(EditorType = typeof(PasswordEditorCreator<SampleCustomer>))]
#endif
    [Category("Security")]
    [DisplayName("Password")]
    public string? Password { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [Browsable(false)]
    public string? NotBrowsable { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [DisplayName("Description (multi-line)")]
    [PropertyGridDynamicProperty(Name = "Foreground", Value = "White")]
    [PropertyGridDynamicProperty(Name = "Background", Value = "Black")]
    public string? Description { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridDynamicProperty(Name = "Format", Value = "0x{0}")]
    [ReadOnly(true)]
    [DisplayName("Byte Array (hex format)")]
    public byte[]? ByteArray1 { get => GetPropertyValue<byte[]>(); set => SetPropertyValue(value); }

    [ReadOnly(true)]
    [DisplayName("Byte Array (press button for hex dump)")]
    public byte[]? ByteArray2 { get => ByteArray1; set => ByteArray1 = value; }

    [DisplayName("Web Site (custom sort order)")]
    public string? WebSite { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [Category("Collections")]
    public string[]? ArrayOfStrings { get => GetPropertyValue<string[]>(); set => SetPropertyValue(value); }

    [Category("Collections")]
    public List<string>? ListOfStrings { get => GetPropertyValue<List<string>>(); set => SetPropertyValue(value); }

    [DisplayName("Addresses (custom editor)")]
    [Category("Collections")]
    public ObservableCollection<SampleAddress>? Addresses { get => GetPropertyValue<ObservableCollection<SampleAddress>>(); set => SetPropertyValue(value); }

    [DisplayName("Days Of Week (multi-valued enum)")]
    [Category("Enums")]
    public SampleDaysOfWeek DaysOfWeek { get => GetPropertyValue<SampleDaysOfWeek>(); set => SetPropertyValue(value); }

    [DisplayName("Percentage Of Satisfaction (int)")]
    public int PercentageOfSatisfactionInt { get => GetPropertyValue(0, "PercentageOfSatisfaction"); set => SetPropertyValue(value, nameof(PercentageOfSatisfaction)); }

    [DisplayName("Percentage Of Satisfaction (double)")]
    public double PercentageOfSatisfaction
    {
        get => GetPropertyValue<double>();
        set
        {
            if (SetPropertyValue(value))
            {
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(PercentageOfSatisfactionInt)));
            }
        }
    }

    [DisplayName("Preferred Color Name (custom editor)")]
    public string? PreferredColorName { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [DisplayName("Point (auto type converter)")]
    public SamplePoint Point { get => GetPropertyValue<SamplePoint>(); set => SetPropertyValue(value); }

    [DisplayName("Nullable Int32 (supports empty string)")]
    public int? NullableInt32 { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

    [DisplayName("Boolean (Checkbox)")]
    [Category("Booleans")]
    public bool SampleBoolean { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }

    [DisplayName("ReadOnly Boolean")]
    [Category("Booleans")]
    [ReadOnly(true)]
    public bool ReadOnlyBoolean { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }

    [DisplayName("Boolean (Checkbox three states)")]
    [Category("Booleans")]
    public bool? SampleNullableBoolean { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

    [DisplayName("Boolean (DropDownList)")]
    [Category("Booleans")]
    public bool SampleBooleanDropDownList { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }

    [DisplayName("Boolean (DropDownList 3 states)")]
    [Category("Booleans")]
    public bool? SampleNullableBooleanDropDownList { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }
}

[TypeConverter(typeof(SampleAddressConverter))]
public class SampleAddress : AutoObject
{
    [PropertyGridPropertyOptions(SortOrder = 10)]
    public string? Line1 { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 20)]
    public string? Line2 { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 30)]
    public int? ZipCode { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 40)]
    public string? City { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 50)]
    public string? State { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    [PropertyGridPropertyOptions(SortOrder = 60)]
    public string? Country { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

    // poor man's one line comma separated USA postal address parser
    public static SampleAddress Parse(string text)
    {
        var address = new SampleAddress();
        if (text != null)
        {
            var split = text.Split([','], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                var zip = 0;
                var index = -1;
                string? state = null;
                for (var i = 0; i < split.Length; i++)
                {
                    if (TryFindStateZip(split[i], out state, out zip))
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    address.DistributeOverProperties(split, 0, int.MaxValue, nameof(Line1), nameof(Line2), nameof(City), nameof(Country));
                }
                else
                {
                    address.ZipCode = zip;
                    address.State = state;
                    address.DistributeOverProperties(split, 0, index, nameof(Line1), nameof(Line2), nameof(City));
                    if (string.IsNullOrWhiteSpace(address.City) && address.Line2 != null)
                    {
                        address.City = address.Line2;
                        address.Line2 = null;
                    }
                    address.DistributeOverProperties(split, index + 1, int.MaxValue, nameof(Country));
                }
            }
        }
        return address;
    }

    private static bool TryFindStateZip(string text, out string? state, out int zip)
    {
        state = null;
        var zipText = text;
        var pos = text.LastIndexOfAny([' ']);
        if (pos >= 0)
        {
#if NETFRAMEWORK
            zipText = text.Substring(pos + 1).Trim();
#else
            zipText = text[(pos + 1)..].Trim();
#endif
        }

        if (!int.TryParse(zipText, out zip) || zip <= 0)
            return false;

#if NETFRAMEWORK
        state = text.Substring(0, pos).Trim();
#else
        state = text[..pos].Trim();
#endif
        return true;
    }

    private void DistributeOverProperties(string[] split, int offset, int max, params string[] properties)
    {
        for (var i = 0; i < properties.Length; i++)
        {
            if (offset + i >= split.Length || offset + i >= max)
                return;

            var s = split[offset + i].Trim();
            if (s.Length == 0)
                continue;

            SetPropertyValue(s, properties[i]);
        }
    }

    public override string ToString()
    {
        const string sep = ", ";
        var sb = new StringBuilder();
        AppendJoin(sb, Line1, string.Empty);
        AppendJoin(sb, Line2, sep);
        AppendJoin(sb, City, sep);
        AppendJoin(sb, State, sep);
        if (ZipCode.HasValue)
        {
            AppendJoin(sb, ZipCode.Value.ToString(), " ");
        }
        AppendJoin(sb, Country, sep);
        return sb.ToString();
    }

    private static void AppendJoin(StringBuilder sb, string? value, string sep)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var s = sb.ToString();
#if NETFRAMEWORK
        if (!s.EndsWith(" ") && !s.EndsWith(",") && !s.EndsWith(Environment.NewLine))
#else
        if (!s.EndsWith(' ') && !s.EndsWith(',') && !s.EndsWith(Environment.NewLine))
#endif
        {
            sb.Append(sep);
        }
        sb.Append(value);
    }
}

[Flags]
public enum SampleDaysOfWeek
{
    NoDay = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
    WeekDays = Monday | Tuesday | Wednesday | Thursday | Friday
}

public enum SampleGender
{
    Male,
    Female
}

public enum SampleStatus
{
    [PropertyGridDynamicProperty(Name = "Foreground", Value = "Black")]
    [PropertyGridDynamicProperty(Name = "Background", Value = "Orange")]
    Unknown,

    [PropertyGridDynamicProperty(Name = "Foreground", Value = "White")]
    [PropertyGridDynamicProperty(Name = "Background", Value = "Red")]
    Invalid,

    [PropertyGridDynamicProperty(Name = "Foreground", Value = "White")]
    [PropertyGridDynamicProperty(Name = "Background", Value = "Green")]
    Valid
}

public class SampleAddressConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
            return SampleAddress.Parse(s);

        return base.ConvertFrom(context, culture, value);
    }
}

public class SamplePointConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            var v = s.Split([';']);
            return new SamplePoint(int.Parse(v[0]), int.Parse(v[1]));
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
            return ((SamplePoint)value!).X + ";" + ((SamplePoint)value).Y;

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

[TypeConverter(typeof(SamplePointConverter))]
public struct SamplePoint
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public SamplePoint(int x, int y)
        : this()
    {
        X = x;
        Y = y;
    }
}
