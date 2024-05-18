using System.CodeDom.Compiler;
using System.Security.Cryptography;
using System.Xml.Serialization;

#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
#pragma warning disable IDE0056 // Use index operator
#pragma warning disable IDE0054 // Use compound assignment

namespace ZeroDep;

/// <summary>
/// A utility class to serialize and deserialize JSON.
/// </summary>
public static class Json
{
    private const string _null = "null";
    private const string _true = "true";
    private const string _false = "false";
    private const string _zeroArg = "{0}";
    private const string _dateStartJs = "new Date(";
    private const string _dateEndJs = ")";
    private const string _dateStart = @"""\/Date(";
    private const string _dateStart2 = @"/Date(";
    private const string _dateEnd = @")\/""";
    private const string _dateEnd2 = @")/";
    private const string _roundTripFormat = "R";
    private const string _enumFormat = "D";
    private const string _x4Format = "{0:X4}";
    private const string _d2Format = "D2";
    private const string _scriptIgnore = "ScriptIgnore";

    private static readonly string[] _dateFormatsUtc = ["yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", "yyyy'-'MM'-'dd'T'HH':'mm'Z'", "yyyyMMdd'T'HH':'mm':'ss'Z'"];
    private static readonly DateTime _minDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly long _minDateTimeTicks = _minDateTime.Ticks;

    /// <summary>
    /// Serializes the specified object. Supports anonymous and dynamic types.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">Options to use for serialization.</param>
    /// <returns>
    /// A JSON representation of the serialized object.
    /// </returns>
    public static string Serialize(object? value, JsonOptions? options = null)
    {
        using var writer = new StringWriter();
        Serialize(writer, value, options);
        return writer.ToString();
    }

    /// <summary>
    /// Serializes the specified object to the specified TextWriter. Supports anonymous and dynamic types.
    /// </summary>
    /// <param name="writer">The output writer. May not be null.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">Options to use for serialization.</param>
    public static void Serialize(TextWriter writer, object? value, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        options = options ?? new JsonOptions();
        var objectGraph = options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        var jsonp = options.JsonPCallback.Nullify();
        if (jsonp != null)
        {
            writer.Write(options.JsonPCallback);
            writer.Write('(');
        }

        WriteValue(writer, value, objectGraph, options);
        if (jsonp != null)
        {
            writer.Write(')');
            writer.Write(';');
        }
    }

    /// <summary>
    /// Deserializes an object from the specified text.
    /// </summary>
    /// <param name="text">The text text.</param>
    /// <param name="targetType">The required target type.</param>
    /// <param name="options">Options to use for deserialization.</param>
    /// <returns>
    /// An instance of an object representing the input data.
    /// </returns>
    public static object? Deserialize(string? text, Type? targetType = null, JsonOptions? options = null)
    {
        if (text == null)
        {
            if (targetType == null)
                return null;

            if (!targetType.IsValueType)
                return null;

            return CreateInstance(null, targetType, 0, options, text);
        }

        var reader = new StringReader(text);
        return Deserialize(reader, targetType, options);
    }

    /// <summary>
    /// Deserializes an object from the specified TextReader.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="reader">The input reader. May not be null.</param>
    /// <param name="options">Options to use for deserialization.</param>
    /// <returns>
    /// An instance of an object representing the input data.
    /// </returns>
    public static T? Deserialize<T>(TextReader reader, JsonOptions? options = null) => (T?)Deserialize(reader, typeof(T), options);

    /// <summary>
    /// Deserializes an object from the specified TextReader.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="text">The text to deserialize.</param>
    /// <param name="options">Options to use for deserialization.</param>
    /// <returns>
    /// An instance of an object representing the input data.
    /// </returns>
    public static T? Deserialize<T>(string text, JsonOptions? options = null) => (T?)Deserialize(text, typeof(T), options);

    /// <summary>
    /// Deserializes an object from the specified TextReader.
    /// </summary>
    /// <param name="reader">The input reader. May not be null.</param>
    /// <param name="targetType">The required target type.</param>
    /// <param name="options">Options to use for deserialization.</param>
    /// <returns>
    /// An instance of an object representing the input data.
    /// </returns>
    public static object? Deserialize(TextReader reader, Type? targetType = null, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        options = options ?? new JsonOptions();
        if (targetType == null || targetType == typeof(object))
            return ReadValue(reader, options);

        var value = ReadValue(reader, options);
        if (value == null)
        {
            if (targetType.IsValueType)
                return CreateInstance(null, targetType, 0, options, value);

            return null;
        }

        return ChangeType(null, value, targetType, options);
    }

    /// <summary>
    /// Deserializes data from the specified text and populates a specified object instance.
    /// </summary>
    /// <param name="text">The text to deserialize.</param>
    /// <param name="target">The object instance to populate.</param>
    /// <param name="options">Options to use for deserialization.</param>
    public static void DeserializeToTarget(string? text, object target, JsonOptions? options = null)
    {
        if (text == null)
            return;

        using (var reader = new StringReader(text))
        {
            DeserializeToTarget(reader, target, options);
        }
    }

    /// <summary>
    /// Deserializes data from the specified TextReader and populates a specified object instance.
    /// </summary>
    /// <param name="reader">The input reader. May not be null.</param>
    /// <param name="target">The object instance to populate.</param>
    /// <param name="options">Options to use for deserialization.</param>
    public static void DeserializeToTarget(TextReader reader, object target, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(target);
        var value = ReadValue(reader, options);
        Apply(value, target, options);
    }

    /// <summary>
    /// Applies the content of an array or dictionary to a target object.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="options">Options to use.</param>
    public static void Apply(object? input, object? target, JsonOptions? options = null)
    {
        options = options ?? new JsonOptions();
        if (target is Array array && !array.IsReadOnly)
        {
            Apply(input as IEnumerable, array, options);
            return;
        }

        if (input is IDictionary dic)
        {
            Apply(dic, target, options);
            return;
        }

        if (target != null)
        {
            var lo = GetListObject(target.GetType(), options, target, input, null, null);
            if (lo != null)
            {
                lo.List = target;
                ApplyToListTarget(target, input as IEnumerable, lo, options);
                return;
            }
        }
    }

    private static object? CreateInstance(object? target, Type type, int elementsCount, JsonOptions? options, object? value)
    {
        try
        {
            if (options?.CreateInstanceCallback != null)
            {
                var og = new Dictionary<object, object?>
                {
                    ["elementsCount"] = elementsCount,
                    ["value"] = value
                };

                var e = new JsonEventArgs(null, type, og, options, null, target)
                {
                    EventType = JsonEventType.CreateInstance
                };
                options.CreateInstanceCallback(e);
                if (e.Handled)
                    return e.Value;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType()!;
                return Array.CreateInstance(elementType, elementsCount);
            }

            if (type.IsInterface)
            {
                var elementType = GetGenericListElementType(type);
                if (elementType != null)
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                var elementTypes = GetGenericDictionaryElementType(type);
                if (elementTypes != null)
                    return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(elementTypes))!;

                elementType = GetListElementType(type);
                if (elementType != null)
                    return new List<object>(elementsCount);
            }
            return Activator.CreateInstance(type)!;
        }
        catch (Exception e)
        {
            HandleException(new JsonException("JSO0001: JSON error detected. Cannot create an instance of the '" + type.Name + "' type.", e), options);
            return null;
        }
    }

    private static Type? GetGenericListElementType(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(IList<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(ICollection<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
                return iface.GetGenericArguments()[0];
        }
        return null;
    }

    private static Type? GetListElementType(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(IList))
                return iface.GetGenericArguments()[0];
        }
        return null;
    }

    private static Type[]? GetGenericDictionaryElementType(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return iface.GetGenericArguments();

            if (iface.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                return iface.GetGenericArguments();
        }
        return null;
    }

    private static ListObject? GetListObject(Type type, JsonOptions? options, object? target, object? value, IDictionary? dictionary, string? key)
    {
        if (options?.GetListObjectCallback != null)
        {
            var og = new Dictionary<object, object?>
            {
                ["dictionary"] = dictionary,
                ["type"] = type
            };

            var e = new JsonEventArgs(null, value, og, options, key, target)
            {
                EventType = JsonEventType.GetListObject
            };
            options.GetListObjectCallback(e);
            if (e.Handled)
            {
                og.TryGetValue("type", out var outType);
                return outType as ListObject;
            }
        }

        if (type == typeof(byte[]))
            return null;

        if (typeof(IList).IsAssignableFrom(type))
            return new IListObject(); // also handles arrays

        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
                return (ListObject)Activator.CreateInstance(typeof(ICollectionTObject<>).MakeGenericType(type.GetGenericArguments()[0]))!;
        }

        foreach (var iface in type.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                iface.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
                return (ListObject)Activator.CreateInstance(typeof(ICollectionTObject<>).MakeGenericType(iface.GetGenericArguments()[0]))!;
        }
        return null;
    }

    /// <summary>
    /// Defines an object that handles list deserialization.
    /// </summary>
    public abstract class ListObject
    {
        /// <summary>
        /// Gets or sets the list object.
        /// </summary>
        /// <value>
        /// The list.
        /// </value>
        public virtual object? List { get; set; }

        /// <summary>
        /// Clears the list object.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Adds a value to the list object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        public abstract void Add(object? value, JsonOptions? options = null);

        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <value>
        /// The context. May be null.
        /// </value>
        public virtual IDictionary<string, object?>? Context => null;
    }

    private static void ApplyToListTarget(object? target, IEnumerable? input, ListObject list, JsonOptions? options)
    {
        if (list.List == null)
            return;

        if (list.Context != null)
        {
            list.Context["action"] = "init";
            list.Context["target"] = target;
            list.Context["input"] = input;
            list.Context["options"] = options;
        }

        if (input != null)
        {
            var array = list.List as Array;
            var max = 0;
            var i = 0;
            if (array != null)
            {
                i = array.GetLowerBound(0);
                max = array.GetUpperBound(0);
            }

            var itemType = GetItemType(list.List.GetType());
            foreach (var value in input)
            {
                if (array != null)
                {
                    if ((i - 1) == max)
                        break;

                    array.SetValue(ChangeType(target, value, itemType, options), i++);
                }
                else
                {
                    var cvalue = ChangeType(target, value, itemType, options);
                    if (list.Context != null)
                    {
                        list.Context["action"] = "add";
                        list.Context["itemType"] = itemType;
                        list.Context["value"] = value;
                        list.Context["cvalue"] = cvalue;

                        if (!list.Context.TryGetValue("cvalue", out var newcvalue))
                            continue;

                        cvalue = newcvalue;
                    }

                    list.Add(cvalue, options);
                }
            }
        }
        else
        {
            if (list.Context != null)
            {
                list.Context["action"] = "clear";
            }
            list.Clear();
        }

        list.Context?.Clear();
    }

    private static void Apply(IEnumerable? input, Array? target, JsonOptions options)
    {
        if (target == null || target.Rank != 1)
            return;

        var elementType = target.GetType().GetElementType()!;
        var i = 0;
        if (input != null)
        {
            foreach (var value in input)
            {
                target.SetValue(ChangeType(target, value, elementType, options), i++);
            }
        }
        else
        {
            Array.Clear(target, 0, target.Length);
        }
    }

    private static bool AreValuesEqual(object? o1, object? o2)
    {
        if (ReferenceEquals(o1, o2))
            return true;

        if (o1 == null)
            return o2 == null;

        return o1.Equals(o2);
    }

    private static bool TryGetObjectDefaultValue(Attribute att, out object? value)
    {
        if (att is JsonAttribute jsa && jsa.HasDefaultValue)
        {
            value = jsa.DefaultValue;
            return true;
        }

        if (att is DefaultValueAttribute dva)
        {
            value = dva.Value;
            return true;
        }

        value = null;
        return false;
    }

    private static string? GetObjectName(Attribute att)
    {
        if (att is JsonAttribute jsa && !string.IsNullOrEmpty(jsa.Name))
            return jsa.Name;

        if (att is XmlAttributeAttribute xaa && !string.IsNullOrEmpty(xaa.AttributeName))
            return xaa.AttributeName;

        if (att is XmlElementAttribute xea && !string.IsNullOrEmpty(xea.ElementName))
            return xea.ElementName;

        return null;
    }

    private static bool TryGetObjectDefaultValue(MemberInfo mi, out object? value)
    {
        var atts = mi.GetCustomAttributes(true);
        if (atts != null)
        {
            foreach (var att in atts.Cast<Attribute>())
            {
                if (TryGetObjectDefaultValue(att, out value))
                    return true;
            }
        }
        value = null;
        return false;
    }

    private static string GetObjectName(MemberInfo mi, string defaultName)
    {
        var atts = mi.GetCustomAttributes(true);
        if (atts != null)
        {
            foreach (var att in atts.Cast<Attribute>())
            {
                var name = GetObjectName(att);
                if (name != null)
                    return name;
            }
        }
        return defaultName;
    }

    private static bool TryGetObjectDefaultValue(PropertyDescriptor pd, out object? value)
    {
        foreach (var att in pd.Attributes.Cast<Attribute>())
        {
            if (TryGetObjectDefaultValue(att, out value))
                return true;
        }

        value = null;
        return false;
    }

    private static string GetObjectName(PropertyDescriptor pd, string defaultName)
    {
        foreach (var att in pd.Attributes.Cast<Attribute>())
        {
            var name = GetObjectName(att);
            if (name != null)
                return name;
        }
        return defaultName;
    }

    private static bool HasScriptIgnore(PropertyDescriptor pd)
    {
        if (pd.Attributes == null)
            return false;

        foreach (var att in pd.Attributes)
        {
            if (att.GetType().Name == null)
                continue;

            if (att.GetType().Name.StartsWith(_scriptIgnore))
                return true;
        }
        return false;
    }

    private static bool HasScriptIgnore(MemberInfo mi)
    {
        var atts = mi.GetCustomAttributes(true);
        if (atts == null || atts.Length == 0)
            return false;

        foreach (var obj in atts)
        {
#pragma warning disable IDE0083 // Use pattern matching
            if (!(obj is Attribute att))
#pragma warning restore IDE0083 // Use pattern matching
                continue;

            if (att.GetType().Name == null)
                continue;

            if (att.GetType().Name.StartsWith(_scriptIgnore))
                return true;
        }
        return false;
    }

    private static void Apply(IDictionary? dictionary, object? target, JsonOptions? options)
    {
        if (dictionary == null || target == null)
            return;

        if (target is IDictionary dicTarget)
        {
            var itemType = GetItemType(dicTarget.GetType());
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key == null)
                    continue;

                if (itemType == typeof(object))
                {
                    dicTarget[entry.Key] = entry.Value;
                }
                else
                {
                    dicTarget[entry.Key] = ChangeType(target, entry.Value, itemType, options);
                }
            }
            return;
        }

        var def = TypeDef.Get(target.GetType(), options);

        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key == null)
                continue;

            var entryKey = string.Format(CultureInfo.InvariantCulture, "{0}", entry.Key);
            var entryValue = entry.Value;
            if (options?.MapEntryCallback != null)
            {
                var og = new Dictionary<object, object?>
                {
                    ["dictionary"] = dictionary
                };

                var e = new JsonEventArgs(null, entryValue, og, options, entryKey, target)
                {
                    EventType = JsonEventType.MapEntry
                };
                options.MapEntryCallback(e);
                if (e.Handled)
                    continue;

                entryKey = e.Name;
                entryValue = e.Value;
            }

            def.ApplyEntry(dictionary, target, entryKey, entryValue, options);
        }
    }

    private static JsonAttribute? GetJsonAttribute(MemberInfo pi)
    {
        var atts = pi.GetCustomAttributes(true);
        if (atts == null || atts.Length == 0)
            return null;

        foreach (var obj in atts)
        {
#pragma warning disable IDE0083 // Use pattern matching
            if (!(obj is Attribute att))
#pragma warning restore IDE0083 // Use pattern matching
                continue;

            if (att is JsonAttribute xatt)
                return xatt;

        }
        return null;
    }

    /// <summary>
    /// Gets the type of elements in a collection type.
    /// </summary>
    /// <param name="collectionType">The collection type.</param>
    /// <returns>The element type or typeof(object) if it was not determined.</returns>
    public static Type GetItemType(Type collectionType)
    {
        ArgumentNullException.ThrowIfNull(collectionType);

        foreach (var iface in collectionType.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return iface.GetGenericArguments()[1];

            if (iface.GetGenericTypeDefinition() == typeof(IList<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(ICollection<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return iface.GetGenericArguments()[0];
        }
        return typeof(object);
    }

    /// <summary>
    /// Returns a System.Object with a specified type and whose value is equivalent to a specified input object.
    /// If an error occurs, a computed default value of the target type will be returned.
    /// </summary>
    /// <param name="value">The input object. May be null.</param>
    /// <param name="conversionType">The target type. May not be null.</param>
    /// <param name="options">The options to use.</param>
    /// <returns>
    /// An object of the target type whose value is equivalent to input value.
    /// </returns>
    public static object? ChangeType(object? value, Type conversionType, JsonOptions options) => ChangeType(null, value, conversionType, options);

    /// <summary>
    /// Returns a System.Object with a specified type and whose value is equivalent to a specified input object.
    /// If an error occurs, a computed default value of the target type will be returned.
    /// </summary>
    /// <param name="target">The target. May be null.</param>
    /// <param name="value">The input object. May be null.</param>
    /// <param name="conversionType">The target type. May not be null.</param>
    /// <param name="options">The options to use.</param>
    /// <returns>
    /// An object of the target type whose value is equivalent to input value.
    /// </returns>
    public static object? ChangeType(object? target, object? value, Type conversionType, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(conversionType);
        if (conversionType == typeof(object))
            return value;

        options = options ?? new JsonOptions();
        if (value is not string)
        {
            if (conversionType.IsArray)
            {
                if (value is IEnumerable en)
                {
                    var elementType = conversionType.GetElementType()!;
                    var list = new List<object?>();
                    foreach (var obj in en)
                    {
                        list.Add(ChangeType(target, obj, elementType, options));
                    }

                    var array = Array.CreateInstance(elementType, list.Count);
                    if (array != null)
                    {
                        Array.Copy(list.ToArray(), array, list.Count);
                    }
                    return array;
                }
            }

            var lo = GetListObject(conversionType, options, target, value, null, null);
            if (lo != null)
            {
                if (value is IEnumerable en)
                {
                    lo.List = CreateInstance(target, conversionType, en is ICollection coll ? coll.Count : 0, options, value);
                    ApplyToListTarget(target, en, lo, options);
                    return lo.List;
                }
            }
        }

        if (value is IDictionary dic)
        {
            var instance = CreateInstance(target, conversionType, 0, options, value);
            if (instance != null)
            {
                Apply(dic, instance, options);
            }
            return instance;
        }

        if (conversionType == typeof(byte[]) && value is string str)
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.ByteArrayAsBase64))
            {
                try
                {
                    return Convert.FromBase64String(str);
                }
                catch (Exception e)
                {
                    HandleException(new JsonException("JSO0013: JSON deserialization error with a base64 array as string.", e), options);
                    return null;
                }
            }
        }

        if (conversionType == typeof(DateTime))
        {
            if (value is DateTime)
                return value;

            var svalue = string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (!string.IsNullOrEmpty(svalue))
            {
                if (TryParseDateTime(svalue, options.DateTimeStyles, out var dt))
                    return dt;
            }
        }

        if (conversionType == typeof(TimeSpan))
        {
            var svalue = string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (!string.IsNullOrEmpty(svalue))
            {
                if (long.TryParse(svalue, out var ticks))
                    return new TimeSpan(ticks);
            }
        }

        return Conversions.ChangeType(value, conversionType, null, null);
    }

    private static object?[]? ReadArray(TextReader reader, JsonOptions? options)
    {
        if (!ReadWhitespaces(reader))
            return null;

        reader.Read();
        var list = new List<object?>();
        do
        {
            var value = ReadValue(reader, options, true, out var arrayEnd);
            if (!Convert.IsDBNull(value))
            {
                list.Add(value);
            }
            if (arrayEnd)
                return [.. list];

            if (reader.Peek() < 0)
            {
                HandleException(GetExpectedCharacterException(GetPosition(reader), ']'), options);
                return [.. list];
            }

        }
        while (true);
    }

    private static JsonException GetExpectedCharacterException(long pos, char c)
    {
        if (pos < 0)
            return new JsonException("JSO0002: JSON deserialization error detected. Expecting '" + c + "' character."); ;

        return new JsonException("JSO0003: JSON deserialization error detected at position " + pos + ". Expecting '" + c + "' character.");
    }

    private static JsonException GetUnexpectedCharacterException(long pos, char c)
    {
        if (pos < 0)
            return new JsonException("JSO0004: JSON deserialization error detected. Unexpected '" + c + "' character."); ;

        return new JsonException("JSO0005: JSON deserialization error detected at position " + pos + ". Unexpected '" + c + "' character.");
    }

    private static JsonException GetExpectedHexaCharacterException(long pos)
    {
        if (pos < 0)
            return new JsonException("JSO0006: JSON deserialization error detected. Expecting hexadecimal character."); ;

        return new JsonException("JSO0007: JSON deserialization error detected at position " + pos + ". Expecting hexadecimal character.");
    }

    private static JsonException GetEofException(char c) => new JsonException("JSO0012: JSON deserialization error detected at end of text. Expecting '" + c + "' character.");

    private static long GetPosition(TextReader reader)
    {
        if (reader == null)
            return -1;

        if (reader is StreamReader sr && sr.BaseStream != null)
        {
            try
            {
                return sr.BaseStream.Position;
            }
            catch
            {
                return -1;
            }
        }

        if (reader is StringReader str)
        {
            var fi = typeof(StringReader).GetField("_pos", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
                return (int)fi.GetValue(str)!;
        }
        return -1;
    }

    private static Dictionary<string, object?>? ReadDictionary(TextReader reader, JsonOptions? options)
    {
        if (!ReadWhitespaces(reader))
            return null;

        reader.Read();
        var dictionary = new Dictionary<string, object?>();
        do
        {
            var i = reader.Peek();
            if (i < 0)
            {
                HandleException(GetEofException('}'), options);
                return dictionary;
            }

            var c = (char)reader.Read();
            switch (c)
            {
                case '}':
                    return dictionary;

                case '"':
                    var text = ReadString(reader, options);
                    if (!ReadWhitespaces(reader))
                    {
                        HandleException(GetExpectedCharacterException(GetPosition(reader), ':'), options);
                        return dictionary;
                    }

                    c = (char)reader.Peek();
                    if (c != ':')
                    {
                        HandleException(GetExpectedCharacterException(GetPosition(reader), ':'), options);
                        return dictionary;
                    }

                    reader.Read();
                    dictionary[text ?? string.Empty] = ReadValue(reader, options);
                    break;

                case ',':
                    break;

                case '\r':
                case '\n':
                case '\t':
                case ' ':
                    break;

                default:
                    HandleException(GetUnexpectedCharacterException(GetPosition(reader), c), options);
                    return dictionary;
            }
        }
        while (true);
    }

    private static string? ReadString(TextReader reader, JsonOptions? options)
    {
        var sb = new StringBuilder();
        do
        {
            var i = reader.Peek();
            if (i < 0)
            {
                HandleException(GetEofException('"'), options);
                return null;
            }

            var c = (char)reader.Read();
            if (c == '"')
                break;

            if (c == '\\')
            {
                i = reader.Peek();
                if (i < 0)
                {
                    HandleException(GetEofException('"'), options);
                    return null;
                }

                var next = (char)reader.Read();
                switch (next)
                {
                    case 'b':
                        sb.Append('\b');
                        break;

                    case 't':
                        sb.Append('\t');
                        break;

                    case 'n':
                        sb.Append('\n');
                        break;

                    case 'f':
                        sb.Append('\f');
                        break;

                    case 'r':
                        sb.Append('\r');
                        break;

                    case '/':
                    case '\\':
                    case '"':
                        sb.Append(next);
                        break;

                    case 'u': // unicode
                        var us = ReadX4(reader, options);
                        sb.Append((char)us);
                        break;

                    default:
                        sb.Append(c);
                        sb.Append(next);
                        break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        while (true);
        return sb.ToString();
    }

    private static object? ReadValue(TextReader reader, JsonOptions? options) => ReadValue(reader, options, false, out var _);
    private static object? ReadValue(TextReader reader, JsonOptions? options, bool arrayMode, out bool arrayEnd)
    {
        arrayEnd = false;
        // 1st chance type is determined by format
        int i;
        do
        {
            i = reader.Peek();
            if (i < 0)
                return null;

            if (i == 10 || i == 13 || i == 9 || i == 32)
            {
                reader.Read();
            }
            else
                break;
        }
        while (true);

        var c = (char)i;
        if (c == '"')
        {
            reader.Read();
            var s = ReadString(reader, options);
            if (s != null && options?.SerializationOptions.HasFlag(JsonSerializationOptions.AutoParseDateTime) == true)
            {
                if (TryParseDateTime(s, options.DateTimeStyles, out var dt))
                    return dt;
            }
            return s;
        }

        if (c == '{')
        {
            var dic = ReadDictionary(reader, options);
            if (options?.SerializationOptions.HasFlag(JsonSerializationOptions.UseISerializable) == true)
                throw new NotSupportedException();

            return dic;
        }

        if (c == '[')
            return ReadArray(reader, options);

        if (c == 'n')
            return ReadNew(reader, options, out arrayEnd);

        // handles the null/true/false cases
        if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '+')
            return ReadNumberOrLiteral(reader, options, out arrayEnd);

        if (arrayMode && (c == ']'))
        {
            reader.Read();
            arrayEnd = true;
            return DBNull.Value; // marks array end
        }

        if (arrayMode && (c == ','))
        {
            reader.Read();
            return DBNull.Value; // marks array end
        }

        HandleException(GetUnexpectedCharacterException(GetPosition(reader), c), options);
        return null;
    }

    private static object? ReadNew(TextReader reader, JsonOptions? options, out bool arrayEnd)
    {
        arrayEnd = false;
        var sb = new StringBuilder();
        do
        {
            var i = reader.Peek();
            if (i < 0)
                break;

            if (((char)i) == '}')
                break;

            var c = (char)reader.Read();
            if (c == ',')
                break;

            if (c == ']')
            {
                arrayEnd = true;
                break;
            }

            sb.Append(c);
        }
        while (true);

        var text = sb.ToString();
        if (string.Compare(_null, text.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
            return null;

        if (text.StartsWith(_dateStartJs) && text.EndsWith(_dateEndJs))
        {
            if (long.TryParse(text.AsSpan(_dateStartJs.Length, text.Length - _dateStartJs.Length - _dateEndJs.Length), out var l))
                return new DateTime((l * 10000) + _minDateTimeTicks, DateTimeKind.Utc);
        }

        HandleException(GetUnexpectedCharacterException(GetPosition(reader), text[0]), options);
        return null;
    }

    private static object? ReadNumberOrLiteral(TextReader reader, JsonOptions? options, out bool arrayEnd)
    {
        arrayEnd = false;
        var sb = new StringBuilder();
        do
        {
            var i = reader.Peek();
            if (i < 0)
                break;

            if (((char)i) == '}')
                break;

            var c = (char)reader.Read();
            if (char.IsWhiteSpace(c) || c == ',')
                break;

            if (c == ']')
            {
                arrayEnd = true;
                break;
            }

            sb.Append(c);
        }
        while (true);

        var text = sb.ToString();
        if (string.Compare(_null, text, StringComparison.OrdinalIgnoreCase) == 0)
            return null;

        if (string.Compare(_true, text, StringComparison.OrdinalIgnoreCase) == 0)
            return true;

        if (string.Compare(_false, text, StringComparison.OrdinalIgnoreCase) == 0)
            return false;

        if (text.LastIndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return d;
        }
        else
        {
            if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var de))
                    return de;
            }
            else
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    return i;

                if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                    return l;

                if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var de))
                    return de;
            }
        }

        HandleException(GetUnexpectedCharacterException(GetPosition(reader), text[0]), options);
        return null;
    }

    /// <summary>
    /// Converts the JSON string representation of a date time to its DateTime equivalent.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>A DateTime value if the text was converted successfully; otherwise, null.</returns>
    public static DateTime? TryParseDateTime(string text)
    {
        if (!TryParseDateTime(text, out var dt))
            return null;

        return dt;
    }

    /// <summary>
    /// Converts the JSON string representation of a date time to its DateTime equivalent.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="styles">The styles to use.</param>
    /// <returns>A DateTime value if the text was converted successfully; otherwise, null.</returns>
    public static DateTime? TryParseDateTime(string text, DateTimeStyles styles)
    {
        if (!TryParseDateTime(text, styles, out var dt))
            return null;

        return dt;
    }

    /// <summary>
    /// Converts the JSON string representation of a date time to its DateTime equivalent.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="dt">When this method returns, contains the DateTime equivalent.</param>
    /// <returns>true if the text was converted successfully; otherwise, false.</returns>
    public static bool TryParseDateTime(string text, out DateTime dt) => TryParseDateTime(text, JsonOptions._defaultDateTimeStyles, out dt);

    /// <summary>
    /// Converts the JSON string representation of a date time to its DateTime equivalent.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="styles">The styles to use.</param>
    /// <param name="dt">When this method returns, contains the DateTime equivalent.</param>
    /// <returns>
    /// true if the text was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParseDateTime(string text, DateTimeStyles styles, out DateTime dt)
    {
        dt = DateTime.MinValue;
        if (text == null)
            return false;

        if (text.Length > 2)
        {
            if (text[0] == '"' && text[text.Length - 1] == '"')
            {
                using (var reader = new StringReader(text))
                {
                    reader.Read(); // skip "
                    var options = new JsonOptions
                    {
                        ThrowExceptions = false
                    };
                    text = ReadString(reader, options) ?? string.Empty;
                }
            }
        }

        if (text.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
        {
            if (DateTime.TryParseExact(text, _dateFormatsUtc, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dt))
                return true;
        }

        var offsetHours = 0;
        var offsetMinutes = 0;
        var kind = DateTimeKind.Utc;
        const int len = 19;

        // s format length is 19, as in '2012-02-21T17:07:14'
        // so we can do quick checks
        // this portion of code is needed because we assume UTC and the default DateTime parse behavior is not that (even with AssumeUniversal)
        if (text.Length >= len &&
            text[4] == '-' &&
            text[7] == '-' &&
            (text[10] == 'T' || text[10] == 't') &&
            text[13] == ':' &&
            text[16] == ':')
        {
            if (DateTime.TryParseExact(text, "o", null, DateTimeStyles.AssumeUniversal, out dt))
                return true;

            var tz = text.Substring(len).IndexOfAny(['+', '-']);
            var text2 = text;
            if (tz >= 0)
            {
                tz += len;
                var offset = text.Substring(tz + 1).Trim();
                if (int.TryParse(offset, out int i))
                {
                    kind = DateTimeKind.Local;
                    offsetHours = i / 100;
                    offsetMinutes = i % 100;
                    if (text[tz] == '-')
                    {
                        offsetHours = -offsetHours;
                        offsetMinutes = -offsetMinutes;
                    }
                    text2 = text.Substring(0, tz);
                }
            }

            if (tz >= 0)
            {
                if (DateTime.TryParseExact(text2, "s", null, DateTimeStyles.AssumeLocal, out dt))
                {
                    if (offsetHours != 0)
                    {
                        dt = dt.AddHours(offsetHours);
                    }

                    if (offsetMinutes != 0)
                    {
                        dt = dt.AddMinutes(offsetMinutes);
                    }
                    return true;
                }
            }
            else
            {
                if (DateTime.TryParseExact(text, "s", null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dt))
                    return true;
            }
        }

        // 01234567890123456
        // 20150525T15:50:00
        if (text.Length == 17)
        {
            if ((text[8] == 'T' || text[8] == 't') && text[11] == ':' && text[14] == ':')
            {
                _ = int.TryParse(text.AsSpan(0, 4), out var year);
                _ = int.TryParse(text.AsSpan(4, 2), out var month);
                _ = int.TryParse(text.AsSpan(6, 2), out var day);
                _ = int.TryParse(text.AsSpan(9, 2), out var hour);
                _ = int.TryParse(text.AsSpan(12, 2), out var minute);
                _ = int.TryParse(text.AsSpan(15, 2), out var second);
                if (month > 0 && month < 13 &&
                    day > 0 && day < 32 &&
                    year >= 0 &&
                    hour >= 0 && hour < 24 &&
                    minute >= 0 && minute < 60 &&
                    second >= 0 && second < 60)
                {
                    try
                    {
                        dt = new DateTime(year, month, day, hour, minute, second);
                        return true;
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }
        }

        // read this http://weblogs.asp.net/bleroy/archive/2008/01/18/dates-and-json.aspx
        string? ticks = null;
        if (text.StartsWith(_dateStartJs) && text.EndsWith(_dateEndJs))
        {
            ticks = text.Substring(_dateStartJs.Length, text.Length - _dateStartJs.Length - _dateEndJs.Length).Trim();
        }
        else if (text.StartsWith(_dateStart2, StringComparison.OrdinalIgnoreCase) && text.EndsWith(_dateEnd2, StringComparison.OrdinalIgnoreCase))
        {
            ticks = text.Substring(_dateStart2.Length, text.Length - _dateEnd2.Length - _dateStart2.Length).Trim();
        }

        if (!string.IsNullOrEmpty(ticks))
        {
            var startIndex = (ticks[0] == '-') || (ticks[0] == '+') ? 1 : 0;
            var pos = ticks.IndexOfAny(['+', '-'], startIndex);
            if (pos >= 0)
            {
                var neg = ticks[pos] == '-';
                var offset = ticks.Substring(pos + 1).Trim();
                ticks = ticks.Substring(0, pos).Trim();
                if (int.TryParse(offset, out var i))
                {
                    kind = DateTimeKind.Local;
                    offsetHours = i / 100;
                    offsetMinutes = i % 100;
                    if (neg)
                    {
                        offsetHours = -offsetHours;
                        offsetMinutes = -offsetMinutes;
                    }
                }
            }

            if (long.TryParse(ticks, NumberStyles.Number, CultureInfo.InvariantCulture, out var l))
            {
                dt = new DateTime((l * 10000) + _minDateTimeTicks, kind);
                if (offsetHours != 0)
                {
                    dt = dt.AddHours(offsetHours);
                }

                if (offsetMinutes != 0)
                {
                    dt = dt.AddMinutes(offsetMinutes);
                }
                return true;
            }
        }

        // don't parse pure timespan style XX:YY:ZZ
        if (text.Length == 8 && text[2] == ':' && text[5] == ':')
        {
            dt = DateTime.MinValue;
            return false;
        }

        return DateTime.TryParse(text, null, styles, out dt);
    }

    private static void HandleException(Exception ex, JsonOptions? options)
    {
        if (options != null && !options.ThrowExceptions)
        {
            options.AddException(ex);
            return;
        }
        throw ex;
    }

    private static byte GetHexValue(TextReader reader, char c, JsonOptions? options)
    {
        c = char.ToLower(c);
        if (c < '0')
        {
            HandleException(GetExpectedHexaCharacterException(GetPosition(reader)), options);
            return 0;
        }

        if (c <= '9')
            return (byte)(c - '0');

        if (c < 'a')
        {
            HandleException(GetExpectedHexaCharacterException(GetPosition(reader)), options);
            return 0;
        }

        if (c <= 'f')
            return (byte)(c - 'a' + 10);

        HandleException(GetExpectedHexaCharacterException(GetPosition(reader)), options);
        return 0;
    }

    private static ushort ReadX4(TextReader reader, JsonOptions? options)
    {
        var u = 0;
        for (var i = 0; i < 4; i++)
        {
            u *= 16;
            if (reader.Peek() < 0)
            {
                HandleException(new JsonException("JSO0008: JSON deserialization error detected at end of stream. Expecting hexadecimal character."), options);
                return 0;
            }

            u += GetHexValue(reader, (char)reader.Read(), options);
        }
        return (ushort)u;
    }

    private static bool ReadWhitespaces(TextReader reader) => ReadWhile(reader, char.IsWhiteSpace);
    private static bool ReadWhile(TextReader reader, Predicate<char> cont)
    {
        do
        {
            var i = reader.Peek();
            if (i < 0)
                return false;

            if (!cont((char)i))
                return true;

            reader.Read();
        }
        while (true);
    }

    /// <summary>
    /// Defines an interface for setting or getting options.
    /// </summary>
    public interface IOptionsHolder
    {
        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        JsonOptions Options { get; set; }
    }

    /// <summary>
    /// Defines an interface for quick access to a type member.
    /// </summary>
    public interface IMemberAccessor
    {
        /// <summary>
        /// Gets a component value.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The value.</returns>
        object? Get(object component);

        /// <summary>
        /// Sets a component's value.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">The value to set.</param>
        void Set(object component, object? value);
    }

    /// <summary>
    /// Defines a type's member.
    /// </summary>
    public class MemberDefinition
    {
        private Type _type;
        private string _name;
        private string? _wireName;
        private string? _escapedWireName;
        private IMemberAccessor? _accessor;

        public MemberDefinition(Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);
            _type = type;
            _name = name;
        }

        /// <summary>
        /// Gets or sets the member name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name
        {
            get => _name;
            set
            {
                ArgumentException.ThrowIfNullOrEmpty(value);
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the name used for serialization and deserialiation.
        /// </summary>
        /// <value>
        /// The name used during serialization and deserialization.
        /// </value>
        public virtual string? WireName
        {
            get => _wireName;
            set
            {
                ArgumentException.ThrowIfNullOrEmpty(value);
                _wireName = value;
            }
        }

        /// <summary>
        /// Gets or sets the escaped name used during serialization and deserialiation.
        /// </summary>
        /// <value>
        /// The escaped name used during serialization and deserialiation.
        /// </value>
        public virtual string? EscapedWireName
        {
            get => _escapedWireName;
            set
            {
                ArgumentException.ThrowIfNullOrEmpty(value);
                _escapedWireName = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has default value.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default value; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasDefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public virtual object? DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the accessor.
        /// </summary>
        /// <value>
        /// The accessor.
        /// </value>
        public virtual IMemberAccessor? Accessor { get => _accessor; set => _accessor = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Gets or sets the member type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual Type Type { get => _type; set => _type = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name ?? string.Empty;

        /// <summary>
        /// Gets or creates a member instance.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="elementsCount">The elements count.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new or existing instance.</returns>
        public virtual object? GetOrCreateInstance(object target, int elementsCount, JsonOptions? options = null)
        {
            object? targetValue;
            if (options?.SerializationOptions.HasFlag(JsonSerializationOptions.ContinueOnValueError) == true)
            {
                try
                {
                    targetValue = Accessor?.Get(target);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                targetValue = Accessor?.Get(target);
            }

            if (targetValue == null || (targetValue is Array array && array.GetLength(0) < elementsCount))
            {
                targetValue = CreateInstance(target, Type, elementsCount, options, targetValue);
                if (targetValue != null)
                {
                    Accessor?.Set(target, targetValue);
                }
            }
            return targetValue;
        }

        /// <summary>
        /// Applies the dictionary entry to this member.
        /// </summary>
        /// <param name="dictionary">The input dictionary.</param>
        /// <param name="target">The target object.</param>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <param name="options">The options.</param>
        public virtual void ApplyEntry(IDictionary dictionary, object target, string? key, object? value, JsonOptions? options = null)
        {
            if (options?.ApplyEntryCallback != null)
            {
                var og = new Dictionary<object, object?>
                {
                    ["dictionary"] = dictionary,
                    ["member"] = this
                };

                var e = new JsonEventArgs(null, value, og, options, key, target)
                {
                    EventType = JsonEventType.ApplyEntry
                };
                options.ApplyEntryCallback(e);
                if (e.Handled)
                    return;

                value = e.Value;
            }

            if (value is IDictionary dic)
            {
                var targetValue = GetOrCreateInstance(target, dic.Count, options);
                Apply(dic, targetValue, options);
                return;

            }

            var lo = GetListObject(Type, options, target, value, dictionary, key);
            if (lo != null)
            {
                if (value is IEnumerable enumerable)
                {
                    lo.List = GetOrCreateInstance(target, enumerable is ICollection coll ? coll.Count : 0, options);
                    ApplyToListTarget(target, enumerable, lo, options);
                    return;
                }
            }


            var cvalue = ChangeType(target, value, Type, options);
            Accessor?.Set(target, cvalue);
        }

        /// <summary>
        /// Determines whether the specified value is equal to the zero value for its type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if the specified value is equal to the zero value.</returns>
        public virtual bool IsNullDateTimeValue(object? value) => value == null || DateTime.MinValue.Equals(value);

        /// <summary>
        /// Determines whether the specified value is equal to the zero value for its type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if the specified value is equal to the zero value.</returns>
        public virtual bool IsZeroValue(object? value)
        {
            if (value == null)
                return false;

            var type = value.GetType();
            if (type != Type)
                return false;

            return IsZeroValueType(value);
        }

        /// <summary>
        /// Determines if a value equals the default value.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <returns>true if both values are equal; false otherwise.</returns>
        public virtual bool EqualsDefaultValue(object? value) => AreValuesEqual(DefaultValue, value);

        /// <summary>
        /// Removes a deserialization member.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <param name="member">The member. May not be null.</param>
        /// <returns>true if item is successfully removed; otherwise, false.</returns>
        public static bool RemoveDeserializationMember(Type type, JsonOptions options, MemberDefinition member)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(member);
            options = options ?? new JsonOptions();
            return TypeDef.RemoveDeserializationMember(type, options, member);
        }

        /// <summary>
        /// Removes a serialization member.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <param name="member">The member. May not be null.</param>
        /// <returns>true if item is successfully removed; otherwise, false.</returns>
        public static bool RemoveSerializationMember(Type type, JsonOptions options, MemberDefinition member)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(member);
            options = options ?? new JsonOptions();
            return TypeDef.RemoveSerializationMember(type, options, member);
        }

        /// <summary>
        /// Adds a deserialization member.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <param name="member">The member. May not be null.</param>
        /// <returns>true if item is successfully added; otherwise, false.</returns>
        public static void AddDeserializationMember(Type type, JsonOptions options, MemberDefinition member)
        {
            ArgumentNullException.ThrowIfNull(type);

            ArgumentNullException.ThrowIfNull(member);

            options = options ?? new JsonOptions();
            TypeDef.AddDeserializationMember(type, options, member);
        }

        /// <summary>
        /// Adds a serialization member.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <param name="member">The member. May not be null.</param>
        /// <returns>true if item is successfully added; otherwise, false.</returns>
        public static void AddSerializationMember(Type type, JsonOptions options, MemberDefinition member)
        {
            ArgumentNullException.ThrowIfNull(type);

            ArgumentNullException.ThrowIfNull(member);

            options = options ?? new JsonOptions();
            TypeDef.AddSerializationMember(type, options, member);
        }

        /// <summary>
        /// Gets the serialization members for a given type.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <returns>A list of serialization members.</returns>
        public static MemberDefinition[] GetSerializationMembers(Type type, JsonOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(type);

            options = options ?? new JsonOptions();
            return TypeDef.GetSerializationMembers(type, options);
        }

        /// <summary>
        /// Gets the deserialization members for a given type.
        /// </summary>
        /// <param name="type">The type. May not be null.</param>
        /// <param name="options">The options. May be null.</param>
        /// <returns>A list of deserialization members.</returns>
        public static MemberDefinition[] GetDeserializationMembers(Type type, JsonOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(type);

            options = options ?? new JsonOptions();
            return TypeDef.GetDeserializationMembers(type, options);
        }

        /// <summary>
        /// Run a specified action, using the member definition lock.
        /// </summary>
        /// <typeparam name="T">The action input type.</typeparam>
        /// <param name="action">The action. May not be null.</param>
        /// <param name="state">The state. May be null.</param>
        public static void UsingLock<T>(Action<T> action, T state)
        {
            ArgumentNullException.ThrowIfNull(action);

            TypeDef.Lock(action, state);
        }
    }

    /// <summary>
    /// Writes a value to a JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="value">The value to writer.</param>
    /// <param name="objectGraph">A graph of objects to track cyclic serialization.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteValue(TextWriter writer, object? value, IDictionary<object, object?>? objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        if (options.WriteValueCallback != null)
        {
            var e = new JsonEventArgs(writer, value, objectGraph, options)
            {
                EventType = JsonEventType.WriteValue
            };
            options.WriteValueCallback(e);
            if (e.Handled)
                return;
        }

        if ((value == null) || Convert.IsDBNull(value))
        {
            writer.Write(_null);
            return;
        }

        if (value is string s)
        {
            WriteString(writer, s);
            return;
        }

        if (value is bool b)
        {
            writer.Write(b ? _true : _false);
            return;
        }

        if (value is float f)
        {
            if (float.IsInfinity(f) || float.IsNaN(f))
            {
                writer.Write(_null);
                return;
            }

            writer.Write(f.ToString(_roundTripFormat, CultureInfo.InvariantCulture));
            return;
        }

        if (value is double d)
        {
            if (double.IsInfinity(d) || double.IsNaN(d))
            {
                writer.Write(_null);
                return;
            }

            writer.Write(d.ToString(_roundTripFormat, CultureInfo.InvariantCulture));
            return;
        }

        if (value is char c)
        {
            if (c == '\0')
            {
                writer.Write(_null);
                return;
            }
            WriteString(writer, c.ToString());
            return;
        }

        if (value is Enum @enum)
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.EnumAsText))
            {
                WriteString(writer, value.ToString());
            }
            else
            {
                writer.Write(@enum.ToString(_enumFormat));
            }
            return;
        }

        if (value is TimeSpan ts)
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.TimeSpanAsText))
            {
                WriteString(writer, ts.ToString("g", CultureInfo.InvariantCulture));
            }
            else
            {
                writer.Write(ts.Ticks);
            }
            return;
        }

        if (value is DateTimeOffset dto)
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatJs))
            {
                writer.Write(_dateStartJs);
                writer.Write((dto.ToUniversalTime().Ticks - _minDateTimeTicks) / 10000);
                writer.Write(_dateEndJs);
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateTimeOffsetFormatCustom) && !string.IsNullOrEmpty(options.DateTimeOffsetFormat))
            {
                WriteString(writer, dto.ToUniversalTime().ToString(options.DateTimeOffsetFormat));
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatIso8601))
            {
                WriteString(writer, dto.ToUniversalTime().ToString("s"));
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatRoundtripUtc))
            {
                WriteString(writer, dto.ToUniversalTime().ToString("o"));
            }
            else
            {
                writer.Write(_dateStart);
                writer.Write((dto.ToUniversalTime().Ticks - _minDateTimeTicks) / 10000);
                writer.Write(_dateEnd);
            }
            return;
        }
        // read this http://weblogs.asp.net/bleroy/archive/2008/01/18/dates-and-json.aspx

        if (value is DateTime dt)
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatJs))
            {
                writer.Write(_dateStartJs);
                writer.Write((dt.ToUniversalTime().Ticks - _minDateTimeTicks) / 10000);
                writer.Write(_dateEndJs);
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatCustom) && !string.IsNullOrEmpty(options.DateTimeFormat))
            {
                WriteString(writer, dt.ToUniversalTime().ToString(options.DateTimeFormat));
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatIso8601))
            {
                writer.Write('"');
                writer.Write(EscapeString(dt.ToUniversalTime().ToString("s"))!, options);
                AppendTimeZoneUtcOffset(writer, dt);
                writer.Write('"');
            }
            else if (options.SerializationOptions.HasFlag(JsonSerializationOptions.DateFormatRoundtripUtc))
            {
                WriteString(writer, dt.ToUniversalTime().ToString("o"));
            }
            else
            {
                writer.Write(_dateStart);
                writer.Write((dt.ToUniversalTime().Ticks - _minDateTimeTicks) / 10000);
                AppendTimeZoneUtcOffset(writer, dt);
                writer.Write(_dateEnd);
            }
            return;
        }

        if (value is int || value is uint || value is short || value is ushort ||
            value is long || value is ulong || value is byte || value is sbyte ||
            value is decimal)
        {
            writer.Write(string.Format(CultureInfo.InvariantCulture, _zeroArg, value));
            return;
        }

        if (value is Guid guid)
        {
            if (options.GuidFormat != null)
            {
                WriteUnescapedString(writer, guid.ToString(options.GuidFormat));
            }
            else
            {
                WriteUnescapedString(writer, guid.ToString());
            }
            return;
        }

        var uri = value as Uri;
        if (uri != null)
        {
            WriteString(writer, uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            return;
        }

        if (value is Array array)
        {
            WriteArray(writer, array, objectGraph, options);
            return;
        }

        if (objectGraph.ContainsKey(value))
        {
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.ContinueOnCycle))
            {
                writer.Write(_null);
                return;
            }

            HandleException(new JsonException("JSO0009: Cyclic JSON serialization detected."), options);
            return;
        }

        objectGraph.Add(value, null);
        options.SerializationLevel++;

        if (value is IDictionary dictionary)
        {
            WriteDictionary(writer, dictionary, objectGraph, options);
            options.SerializationLevel--;
            return;
        }

        // ExpandoObject falls here
        if (TypeDef.IsKeyValuePairEnumerable(value.GetType(), out var _, out var _))
        {
            WriteDictionary(writer, new KeyValueTypeDictionary(value), objectGraph, options);
            options.SerializationLevel--;
            return;
        }

        if (value is IEnumerable enumerable)
        {
            WriteEnumerable(writer, enumerable, objectGraph, options);
            options.SerializationLevel--;
            return;
        }

        if (options.SerializationOptions.HasFlag(JsonSerializationOptions.StreamsAsBase64))
        {
            if (value is Stream stream)
            {
                WriteBase64Stream(writer, stream, objectGraph, options);
                options.SerializationLevel--;
                return;
            }
        }

        WriteObject(writer, value, objectGraph, options);
        options.SerializationLevel--;
    }

    /// <summary>
    /// Writes a stream to a JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="stream">The stream. May not be null.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    /// <returns>The number of written bytes.</returns>
    public static long WriteBase64Stream(TextWriter writer, Stream stream, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(stream);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        var total = 0L;
        if (writer is StreamWriter sw && sw.BaseStream != null)
        {
            sw.Flush();
            return WriteBase64Stream(stream, sw.BaseStream, options);
        }

        if (writer is IndentedTextWriter itw && itw.InnerWriter != null)
        {
            itw.Flush();
            return WriteBase64Stream(itw.InnerWriter, stream, objectGraph, options);
        }

        using (var ms = new MemoryStream())
        {
            var bytes = new byte[options.FinalStreamingBufferChunkSize];
            do
            {
                var read = stream.Read(bytes, 0, bytes.Length);
                if (read == 0)
                    break;

                ms.Write(bytes, 0, read);
                total += read;
            }
            while (true);

            writer.Write('"');
            writer.Write(Convert.ToBase64String(ms.ToArray()));
            writer.Write('"');
            return total;
        }
    }

    private static void SetOptions(object obj, JsonOptions options)
    {
        if (obj is IOptionsHolder holder)
        {
            holder.Options = options;
        }
    }

    private static long WriteBase64Stream(Stream inputStream, Stream outputStream, JsonOptions options)
    {
        outputStream.WriteByte((byte)'"');
        // don't dispose this stream or it will dispose the outputStream as well
        var b64 = new CryptoStream(outputStream, new ToBase64Transform(), CryptoStreamMode.Write);
        var total = 0L;
        var bytes = new byte[options.FinalStreamingBufferChunkSize];
        do
        {
            var read = inputStream.Read(bytes, 0, bytes.Length);
            if (read == 0)
                break;

            b64.Write(bytes, 0, read);
            total += read;
        }
        while (true);

        b64.FlushFinalBlock();
        b64.Flush();
        outputStream.WriteByte((byte)'"');
        return total;
    }

    private static bool InternalIsKeyValuePairEnumerable(Type type, out Type? keyType, out Type? valueType)
    {
        keyType = null;
        valueType = null;
        foreach (var t in type.GetInterfaces())
        {
            if (t.IsGenericType)
            {
                if (typeof(IEnumerable<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                {
                    var args = t.GetGenericArguments();
                    if (args.Length == 1)
                    {
                        var kvp = args[0];
                        if (kvp.IsGenericType && typeof(KeyValuePair<,>).IsAssignableFrom(kvp.GetGenericTypeDefinition()))
                        {
                            var kvpArgs = kvp.GetGenericArguments();
                            if (kvpArgs.Length == 2)
                            {
                                keyType = kvpArgs[0];
                                valueType = kvpArgs[1];
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    private static void AppendTimeZoneUtcOffset(TextWriter writer, DateTime dt)
    {
        if (dt.Kind != DateTimeKind.Utc)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(dt);
            writer.Write((offset.Ticks >= 0) ? '+' : '-');
            writer.Write(Math.Abs(offset.Hours).ToString(_d2Format));
            writer.Write(Math.Abs(offset.Minutes).ToString(_d2Format));
        }
    }

    /// <summary>
    /// Writes an enumerable to a JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="array">The array. May not be null.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteArray(TextWriter writer, Array array, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(array);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        if (options.SerializationOptions.HasFlag(JsonSerializationOptions.ByteArrayAsBase64))
        {
            if (array is byte[] bytes)
            {
                using (var ms = new MemoryStream(bytes))
                {
                    ms.Position = 0;
                    WriteBase64Stream(writer, ms, objectGraph, options);
                }
                return;
            }
        }

        WriteArray(writer, array, objectGraph, options, []);
    }

    private static void WriteArray(TextWriter writer, Array array, IDictionary<object, object?> objectGraph, JsonOptions options, int[] indices)
    {
        var newIndices = new int[indices.Length + 1];
        for (var i = 0; i < indices.Length; i++)
        {
            newIndices[i] = indices[i];
        }

        writer.Write('[');
        for (var i = 0; i < array.GetLength(indices.Length); i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }
            newIndices[indices.Length] = i;

            if (array.Rank == newIndices.Length)
            {
                WriteValue(writer, array.GetValue(newIndices), objectGraph, options);
            }
            else
            {
                WriteArray(writer, array, objectGraph, options, newIndices);
            }
        }
        writer.Write(']');
    }

    /// <summary>
    /// Writes an enumerable to a JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="enumerable">The enumerable. May not be null.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteEnumerable(TextWriter writer, IEnumerable enumerable, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(enumerable);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        writer.Write('[');
        var first = true;
        foreach (var value in enumerable)
        {
            if (!first)
            {
                writer.Write(',');
            }
            else
            {
                first = false;
            }
            WriteValue(writer, value, objectGraph, options);
        }
        writer.Write(']');
    }

    /// <summary>
    /// Writes a dictionary to a JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="dictionary">The dictionary. May not be null.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteDictionary(TextWriter writer, IDictionary dictionary, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(dictionary);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        writer.Write('{');
        var first = true;
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key == null)
                continue;

            var entryKey = string.Format(CultureInfo.InvariantCulture, "{0}", entry.Key);
            if (!first)
            {
                writer.Write(',');
            }
            else
            {
                first = false;
            }

            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.WriteKeysWithoutQuotes))
            {
                writer.Write(EscapeString(entryKey));
            }
            else
            {
                WriteString(writer, entryKey);
            }

            writer.Write(':');
            WriteValue(writer, entry.Value, objectGraph, options);
        }
        writer.Write('}');
    }

    /// <summary>
    /// Writes an object to the JSON writer.
    /// </summary>
    /// <param name="writer">The writer. May not be null.</param>
    /// <param name="value">The object to serialize. May not be null.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteObject(TextWriter writer, object value, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseISerializable))
            throw new NotSupportedException();

        writer.Write('{');

        if (options.BeforeWriteObjectCallback != null)
        {
            var e = new JsonEventArgs(writer, value, objectGraph, options)
            {
                EventType = JsonEventType.BeforeWriteObject
            };
            options.BeforeWriteObjectCallback(e);
            if (e.Handled)
                return;
        }

        var type = value.GetType();
        var def = TypeDef.Get(type, options);
        def.WriteValues(writer, value, objectGraph, options);

        if (options.AfterWriteObjectCallback != null)
        {
            var e = new JsonEventArgs(writer, value, objectGraph, options)
            {
                EventType = JsonEventType.AfterWriteObject
            };
            options.AfterWriteObjectCallback(e);
        }

        writer.Write('}');
    }

    /// <summary>
    /// Determines whether the specified value is a value type and is equal to zero.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>true if the specified value is a value type and is equal to zero; false otherwise.</returns>
    public static bool IsZeroValueType(object value)
    {
        if (value == null)
            return false;

        var type = value.GetType();
        if (!type.IsValueType)
            return false;

        return value.Equals(Activator.CreateInstance(type));
    }

    /// <summary>
    /// Writes a name/value pair to a JSON writer.
    /// </summary>
    /// <param name="writer">The input writer. May not be null.</param>
    /// <param name="name">The name. null values will be converted to empty values.</param>
    /// <param name="value">The value.</param>
    /// <param name="objectGraph">The object graph.</param>
    /// <param name="options">The options to use.</param>
    public static void WriteNameValue(TextWriter writer, string? name, object? value, IDictionary<object, object?> objectGraph, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        name = name ?? string.Empty;
        options = options ?? new JsonOptions();
        objectGraph = objectGraph ?? options.FinalObjectGraph;
        SetOptions(objectGraph, options);

        if (options.SerializationOptions.HasFlag(JsonSerializationOptions.WriteKeysWithoutQuotes))
        {
            writer.Write(EscapeString(name));
        }
        else
        {
            WriteString(writer, name);
        }

        writer.Write(':');
        WriteValue(writer, value, objectGraph, options);
    }

    /// <summary>
    /// Writes a string to a JSON writer.
    /// </summary>
    /// <param name="writer">The input writer. May not be null.</param>
    /// <param name="text">The text.</param>
    public static void WriteString(TextWriter writer, string? text)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (text == null)
        {
            writer.Write(_null);
            return;
        }

        writer.Write('"');
        writer.Write(EscapeString(text));
        writer.Write('"');
    }

    /// <summary>
    /// Writes a string to a JSON writer.
    /// </summary>
    /// <param name="writer">The input writer. May not be null.</param>
    /// <param name="text">The text.</param>
    public static void WriteUnescapedString(TextWriter writer, string? text)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (text == null)
        {
            writer.Write(_null);
            return;
        }

        writer.Write('"');
        writer.Write(text);
        writer.Write('"');
    }

    private static void AppendCharAsUnicode(StringBuilder sb, char c)
    {
        sb.Append('\\');
        sb.Append('u');
        sb.AppendFormat(CultureInfo.InvariantCulture, _x4Format, (ushort)c);
    }

    /// <summary>
    /// Serializes an object with format. Note this is more for debugging purposes as it's not designed to be fast.
    /// </summary>
    /// <param name="value">The JSON object. May be null.</param>
    /// <param name="options">The options to use. May be null.</param>
    /// <returns>A string containing the formatted object.</returns>
    public static string SerializeFormatted(object value, JsonOptions? options = null)
    {
        using var sw = new StringWriter();
        SerializeFormatted(sw, value, options);
        return sw.ToString();
    }

    /// <summary>
    /// Serializes an object with format. Note this is more for debugging purposes as it's not designed to be fast.
    /// </summary>
    /// <param name="writer">The output writer. May not be null.</param>
    /// <param name="value">The JSON object. May be null.</param>
    /// <param name="options">The options to use. May be null.</param>
    public static void SerializeFormatted(TextWriter writer, object value, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        options = options ?? new JsonOptions();
        var serialized = Serialize(value, options);
        var deserialized = Deserialize(serialized, typeof(object), options);
        WriteFormatted(writer, deserialized, options);
    }

    /// <summary>
    /// Writes a JSON deserialized object formatted.
    /// </summary>
    /// <param name="jsonObject">The JSON object. May be null.</param>
    /// <param name="options">The options to use. May be null.</param>
    /// <returns>A string containing the formatted object.</returns>
    public static string WriteFormatted(object jsonObject, JsonOptions? options = null)
    {
        using var sw = new StringWriter();
        WriteFormatted(sw, jsonObject, options);
        return sw.ToString();
    }

    /// <summary>
    /// Writes a JSON deserialized object formatted.
    /// </summary>
    /// <param name="writer">The output writer. May not be null.</param>
    /// <param name="jsonObject">The JSON object. May be null.</param>
    /// <param name="options">The options to use. May be null.</param>
    public static void WriteFormatted(TextWriter writer, object? jsonObject, JsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        options = options ?? new JsonOptions();
        IndentedTextWriter itw;
        if (options?.FormattingTab != null)
        {
            itw = new IndentedTextWriter(writer, options.FormattingTab);
        }
        else
        {
            itw = new IndentedTextWriter(writer);
        }
        WriteFormatted(itw, jsonObject, options);
    }

    private static void WriteFormatted(IndentedTextWriter writer, object? jsonObject, JsonOptions? options)
    {
        if (jsonObject is DictionaryEntry entry)
        {
            var entryKey = string.Format(CultureInfo.InvariantCulture, "{0}", entry.Key);
            if (options?.SerializationOptions.HasFlag(JsonSerializationOptions.WriteKeysWithoutQuotes) == true)
            {
                writer.Write(entryKey);
                writer.Write(": ");
            }
            else
            {
                writer.Write('"');
                writer.Write(entryKey);
                writer.Write("\": ");
            }

            writer.Indent++;
            WriteFormatted(writer, entry.Value, options);
            writer.Indent--;
            return;
        }

        if (jsonObject is IDictionary dictionary)
        {
            writer.WriteLine('{');
            var first = true;
            writer.Indent++;
            foreach (DictionaryEntry entry2 in dictionary)
            {
                if (!first)
                {
                    writer.WriteLine(',');
                }
                else
                {
                    first = false;
                }

                WriteFormatted(writer, entry2, options);
            }

            writer.Indent--;
            writer.WriteLine();
            writer.Write('}');
            return;
        }

        if (jsonObject is string s)
        {
            WriteString(writer, s);
            return;
        }

        if (jsonObject is IEnumerable enumerable)
        {
            writer.WriteLine('[');
            var first = true;
            writer.Indent++;
            foreach (var obj in enumerable)
            {
                if (!first)
                {
                    writer.WriteLine(',');
                }
                else
                {
                    first = false;
                }

                WriteFormatted(writer, obj, options);
            }

            writer.Indent--;
            writer.WriteLine();
            writer.Write(']');
            return;
        }

        WriteValue(writer, jsonObject, null, options);
    }

    /// <summary>
    /// Escapes a string using JSON representation.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>A JSON-escaped string.</returns>
    public static string? EscapeString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var builder = new StringBuilder();
        var startIndex = 0;
        var count = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if ((c == '\r') ||
                (c == '\t') ||
                (c == '"') ||
                (c == '\'') ||
                (c == '<') ||
                (c == '>') ||
                (c == '\\') ||
                (c == '\n') ||
                (c == '\b') ||
                (c == '\f') ||
                (c < ' '))
            {
                if (count > 0)
                {
                    builder.Append(value, startIndex, count);
                }
                startIndex = i + 1;
                count = 0;
            }

            switch (c)
            {
                case '<':
                case '>':
                case '\'':
                    AppendCharAsUnicode(builder, c);
                    continue;

                case '\\':
                    builder.Append(@"\\");
                    continue;

                case '\b':
                    builder.Append(@"\b");
                    continue;

                case '\t':
                    builder.Append(@"\t");
                    continue;

                case '\n':
                    builder.Append(@"\n");
                    continue;

                case '\f':
                    builder.Append(@"\f");
                    continue;

                case '\r':
                    builder.Append(@"\r");
                    continue;

                case '"':
                    builder.Append("\\\"");
                    continue;
            }

            if (c < ' ')
            {
                AppendCharAsUnicode(builder, c);
            }
            else
            {
                count++;
            }
        }

        if (count > 0)
        {
            builder.Append(value, startIndex, count);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Gets a nullified string value from a dictionary by its path.
    /// This is useful to get a string value from the object that the untyped Deserialize method returns which is often of IDictionary&lt;string, object&gt; type.
    /// </summary>
    /// <param name="dictionary">The input dictionary.</param>
    /// <param name="path">The path, composed of dictionary keys separated by a . character. May not be null.</param>
    /// <returns>
    /// The nullified string value or null if not found.
    /// </returns>
    public static string? GetNullifiedStringValueByPath(this IDictionary<string, object>? dictionary, string path)
    {
        if (dictionary == null)
            return null;

        if (!TryGetValueByPath(dictionary, path, out object? obj))
            return null;

        return Conversions.ChangeType<string>(obj).Nullify();
    }

    /// <summary>
    /// Gets a value from a dictionary by its path.
    /// This is useful to get a value from the object that the untyped Deserialize method returns which is often of IDictionary&lt;string, object&gt; type.
    /// </summary>
    /// <typeparam name="T">The final type to which to convert the retrieved value.</typeparam>
    /// <param name="dictionary">The input dictionary.</param>
    /// <param name="path">The path, composed of dictionary keys separated by a . character. May not be null.</param>
    /// <param name="value">The value to retrieve.</param>
    /// <returns>
    /// true if the value parameter was retrieved successfully; otherwise, false.
    /// </returns>
    public static bool TryGetValueByPath<T>(this IDictionary<string, object> dictionary, string path, out T? value)
    {
        if (dictionary == null)
        {
            value = default;
            return false;
        }

        if (!TryGetValueByPath(dictionary, path, out object? obj))
        {
            value = default;
            return false;
        }

        return Conversions.TryChangeType(obj, out value);
    }

    /// <summary>
    /// Gets a value from a dictionary by its path.
    /// This is useful to get a value from the object that the untyped Deserialize method returns which is often of IDictionary&lt;string, object&gt; type.
    /// </summary>
    /// <param name="dictionary">The input dictionary.</param>
    /// <param name="path">The path, composed of dictionary keys separated by a . character. May not be null.</param>
    /// <param name="value">The value to retrieve.</param>
    /// <returns>
    /// true if the value parameter was retrieved successfully; otherwise, false.
    /// </returns>
    public static bool TryGetValueByPath(this IDictionary<string, object>? dictionary, string path, out object? value)
    {
        ArgumentNullException.ThrowIfNull(path);
        value = null;
        if (dictionary == null)
            return false;

        var segments = path.Split('.');
        var current = dictionary;
        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i].Nullify();
            if (segment == null)
                return false;

            if (!current.TryGetValue(segment, out var newElement))
                return false;

            // last?
            if (i == segments.Length - 1)
            {
                value = newElement;
                return true;
            }
            current = newElement as IDictionary<string, object>;
            if (current == null)
                break;
        }
        return false;
    }

    private static T? GetAttribute<T>(this PropertyDescriptor descriptor) where T : Attribute => GetAttribute<T>(descriptor.Attributes);
    private static T? GetAttribute<T>(this AttributeCollection attributes) where T : Attribute
    {
        foreach (var att in attributes)
        {
            if (typeof(T).IsAssignableFrom(att.GetType()))
                return (T)att;
        }
        return null;
    }

    private static bool EqualsIgnoreCase(this string? str, string? text, bool trim = false)
    {
        if (trim)
        {
            str = str.Nullify();
            text = text.Nullify();
        }

        if (str == null)
            return text == null;

        if (text == null)
            return false;

        if (str.Length != text.Length)
            return false;

        return string.Compare(str, text, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private static string? Nullify(this string? str)
    {
        if (str == null)
            return null;

        if (string.IsNullOrWhiteSpace(str))
            return null;

        var t = str.Trim();
        return t.Length == 0 ? null : t;
    }

    private sealed class KeyValueTypeEnumerator(object value) : IDictionaryEnumerator
    {
        private readonly IEnumerator _enumerator = ((IEnumerable)value).GetEnumerator();
        private PropertyInfo? _keyProp;
        private PropertyInfo? _valueProp;

        public DictionaryEntry Entry
        {
            get
            {
                if (_keyProp == null)
                {
                    _keyProp = _enumerator.Current.GetType().GetProperty("Key")!;
                    _valueProp = _enumerator.Current.GetType().GetProperty("Value")!;
                }
                return new DictionaryEntry(_keyProp.GetValue(_enumerator.Current, null)!, _valueProp?.GetValue(_enumerator.Current, null));
            }
        }

        public object Key => Entry.Key;
        public object? Value => Entry.Value;
        public object Current => Entry;

        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }

    private sealed class KeyValueTypeDictionary(object value) : IDictionary
    {
        private readonly KeyValueTypeEnumerator _enumerator = new KeyValueTypeEnumerator(value);

        public int Count => throw new NotSupportedException();
        public bool IsSynchronized => throw new NotSupportedException();
        public object SyncRoot => throw new NotSupportedException();
        public bool IsFixedSize => throw new NotSupportedException();
        public bool IsReadOnly => throw new NotSupportedException();
        public ICollection Keys => throw new NotSupportedException();
        public ICollection Values => throw new NotSupportedException();
        public object? this[object? key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public void Add(object key, object? value) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(object key) => throw new NotSupportedException();
        public IDictionaryEnumerator GetEnumerator() => _enumerator;
        public void Remove(object key) => throw new NotSupportedException();
        public void CopyTo(Array array, int index) => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }

    private sealed class KeyValueType
    {
        public Type? KeyType;
        public Type? ValueType;
    }

    private sealed class TypeDef
    {
        private static readonly Dictionary<string, TypeDef> _defs = [];
        private static readonly Dictionary<Type, KeyValueType> _iskvpe = [];
        private static readonly object _lock = new object();

        private readonly List<MemberDefinition> _serializationMembers = [];
        private readonly List<MemberDefinition> _deserializationMembers = [];
        private readonly Type _type;

        private TypeDef(Type type, JsonOptions options)
        {
            _type = type;
            IEnumerable<MemberDefinition> members;
            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseReflection))
            {
                members = EnumerateDefinitionsUsingReflection(true, type, options);
            }
            else
            {
                members = EnumerateDefinitionsUsingTypeDescriptors(true, type, options);
            }
            _serializationMembers = new List<MemberDefinition>(options.FinalizeSerializationMembers(type, members));

            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseReflection))
            {
                members = EnumerateDefinitionsUsingReflection(false, type, options);
            }
            else
            {
                members = EnumerateDefinitionsUsingTypeDescriptors(false, type, options);
            }
            _deserializationMembers = new List<MemberDefinition>(options.FinalizeDeserializationMembers(type, members));
        }

        private MemberDefinition? GetDeserializationMember(string? key)
        {
            if (key == null)
                return null;

            foreach (var def in _deserializationMembers)
            {
                if (string.Compare(def.WireName, key, StringComparison.OrdinalIgnoreCase) == 0)
                    return def;
            }
            return null;
        }

        public void ApplyEntry(IDictionary dictionary, object target, string? key, object? value, JsonOptions? options)
        {
            var member = GetDeserializationMember(key);
            if (member == null)
                return;

            member.ApplyEntry(dictionary, target, key, value, options);
        }

        public void WriteValues(TextWriter writer, object component, IDictionary<object, object?> objectGraph, JsonOptions options)
        {
            var first = true;
            foreach (var member in _serializationMembers)
            {
                var nameChanged = false;
                var name = member.WireName;
                var value = member.Accessor?.Get(component);
                if (options.WriteNamedValueObjectCallback != null)
                {
                    var e = new JsonEventArgs(writer, value, objectGraph, options, name, component)
                    {
                        EventType = JsonEventType.WriteNamedValueObject,
                        First = first
                    };
                    options.WriteNamedValueObjectCallback(e);
                    first = e.First;
                    if (e.Handled)
                        continue;

                    nameChanged = name != e.Name;
                    name = e.Name;
                    value = e.Value;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.SkipNullPropertyValues))
                {
                    if (value == null)
                        continue;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.SkipZeroValueTypes))
                {
                    if (member.IsZeroValue(value))
                        continue;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.SkipNullDateTimeValues))
                {
                    if (member.IsNullDateTimeValue(value))
                        continue;
                }

                var skipDefaultValues = options.SerializationOptions.HasFlag(JsonSerializationOptions.SkipDefaultValues);
                if (skipDefaultValues && member.HasDefaultValue)
                {
                    if (member.EqualsDefaultValue(value))
                        continue;
                }

                if (!first)
                {
                    writer.Write(',');
                }
                else
                {
                    first = false;
                }

                if (nameChanged)
                {
                    WriteNameValue(writer, name, value, objectGraph, options);
                }
                else
                {
                    if (options.SerializationOptions.HasFlag(JsonSerializationOptions.WriteKeysWithoutQuotes))
                    {
                        writer.Write(member.EscapedWireName);
                    }
                    else
                    {
                        writer.Write('"');
                        writer.Write(member.EscapedWireName);
                        writer.Write('"');
                    }

                    writer.Write(':');
                    WriteValue(writer, value, objectGraph, options);
                }
            }
        }

        public override string ToString() => _type?.AssemblyQualifiedName ?? string.Empty;

        private static string GetKey(Type type, JsonOptions options) => type.AssemblyQualifiedName + '\0' + options.GetCacheKey();
        private static TypeDef UnlockedGet(Type type, JsonOptions? options)
        {
            options ??= new JsonOptions();
            var key = GetKey(type, options);
            if (!_defs.TryGetValue(key, out var ta))
            {
                ta = new TypeDef(type, options);
                _defs.Add(key, ta);
            }
            return ta;
        }

        public static void Lock<T>(Action<T> action, T state)
        {
            lock (_lock)
            {
                action(state);
            }
        }

        public static bool RemoveDeserializationMember(Type type, JsonOptions? options, MemberDefinition member)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                return ta._deserializationMembers.Remove(member);
            }
        }

        public static bool RemoveSerializationMember(Type type, JsonOptions? options, MemberDefinition member)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                return ta._serializationMembers.Remove(member);
            }
        }

        public static void AddDeserializationMember(Type type, JsonOptions? options, MemberDefinition member)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                ta._deserializationMembers.Add(member);
            }
        }

        public static void AddSerializationMember(Type type, JsonOptions? options, MemberDefinition member)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                ta._serializationMembers.Add(member);
            }
        }

        public static MemberDefinition[] GetDeserializationMembers(Type type, JsonOptions? options)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                return [.. ta._deserializationMembers];
            }
        }

        public static MemberDefinition[] GetSerializationMembers(Type type, JsonOptions? options)
        {
            lock (_lock)
            {
                var ta = UnlockedGet(type, options);
                return [.. ta._serializationMembers];
            }
        }

        public static TypeDef Get(Type type, JsonOptions? options)
        {
            lock (_lock)
            {
                return UnlockedGet(type, options);
            }
        }

        public static bool IsKeyValuePairEnumerable(Type type, out Type? keyType, out Type? valueType)
        {
            lock (_lock)
            {
                if (!_iskvpe.TryGetValue(type, out var kv))
                {
                    kv = new KeyValueType();
                    InternalIsKeyValuePairEnumerable(type, out kv.KeyType, out kv.ValueType);
                    _iskvpe.Add(type, kv);
                }

                keyType = kv.KeyType;
                valueType = kv.ValueType;
                return kv.KeyType != null;
            }
        }

        private static IEnumerable<MemberDefinition> EnumerateDefinitionsUsingReflection(bool serialization, Type type, JsonOptions options)
        {
            foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseJsonAttribute))
                {
                    var ja = GetJsonAttribute(info);
                    if (ja != null)
                    {
                        if (serialization && ja.IgnoreWhenSerializing)
                            continue;

                        if (!serialization && ja.IgnoreWhenDeserializing)
                            continue;
                    }
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseXmlIgnore))
                {
                    if (info.IsDefined(typeof(XmlIgnoreAttribute), true))
                        continue;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseScriptIgnore))
                {
                    if (HasScriptIgnore(info))
                        continue;
                }

                if (serialization)
                {
                    if (!info.CanRead)
                        continue;

                    var getMethod = info.GetGetMethod();
                    if (getMethod == null || getMethod.GetParameters().Length > 0)
                        continue;
                }
                // else we don't test the set method, as some properties can still be deserialized (collections)

                var name = GetObjectName(info, info.Name);
                var ma = new MemberDefinition(info.PropertyType, info.Name);
                if (serialization)
                {
                    ma.WireName = name;
                    ma.EscapedWireName = EscapeString(name);
                }
                else
                {
                    ma.WireName = name;
                }

                ma.HasDefaultValue = TryGetObjectDefaultValue(info, out var defaultValue);
                ma.DefaultValue = defaultValue;
                ma.Accessor = (IMemberAccessor)Activator.CreateInstance(typeof(PropertyInfoAccessor<,>).MakeGenericType(info.DeclaringType!, info.PropertyType), info)!;
                yield return ma;
            }

            if (options.SerializationOptions.HasFlag(JsonSerializationOptions.SerializeFields))
            {
                foreach (var info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseJsonAttribute))
                    {
                        var ja = GetJsonAttribute(info);
                        if (ja != null)
                        {
                            if (serialization && ja.IgnoreWhenSerializing)
                                continue;

                            if (!serialization && ja.IgnoreWhenDeserializing)
                                continue;
                        }
                    }

                    if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseXmlIgnore))
                    {
                        if (info.IsDefined(typeof(XmlIgnoreAttribute), true))
                            continue;
                    }

                    if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseScriptIgnore))
                    {
                        if (HasScriptIgnore(info))
                            continue;
                    }

                    var name = GetObjectName(info, info.Name);
                    var ma = new MemberDefinition(info.FieldType, info.Name);
                    if (serialization)
                    {
                        ma.WireName = name;
                        ma.EscapedWireName = EscapeString(name);
                    }
                    else
                    {
                        ma.WireName = name;
                    }

                    ma.HasDefaultValue = TryGetObjectDefaultValue(info, out var defaultValue);
                    ma.DefaultValue = defaultValue;
                    ma.Accessor = (IMemberAccessor)Activator.CreateInstance(typeof(FieldInfoAccessor), info)!;
                    yield return ma;
                }
            }
        }

        private static IEnumerable<MemberDefinition> EnumerateDefinitionsUsingTypeDescriptors(bool serialization, Type type, JsonOptions options)
        {
            foreach (var descriptor in TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>())
            {
                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseJsonAttribute))
                {
                    var ja = descriptor.GetAttribute<JsonAttribute>();
                    if (ja != null)
                    {
                        if (serialization && ja.IgnoreWhenSerializing)
                            continue;

                        if (!serialization && ja.IgnoreWhenDeserializing)
                            continue;
                    }
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseXmlIgnore))
                {
                    if (descriptor.GetAttribute<XmlIgnoreAttribute>() != null)
                        continue;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.UseScriptIgnore))
                {
                    if (HasScriptIgnore(descriptor))
                        continue;
                }

                if (options.SerializationOptions.HasFlag(JsonSerializationOptions.SkipGetOnly) && descriptor.IsReadOnly)
                    continue;

                var name = GetObjectName(descriptor, descriptor.Name);
                var ma = new MemberDefinition(descriptor.PropertyType, descriptor.Name);
                if (serialization)
                {
                    ma.WireName = name;
                    ma.EscapedWireName = EscapeString(name);
                }
                else
                {
                    ma.WireName = name;
                }

                ma.HasDefaultValue = TryGetObjectDefaultValue(descriptor, out var defaultValue);
                ma.DefaultValue = defaultValue;
                ma.Accessor = (IMemberAccessor)Activator.CreateInstance(typeof(PropertyDescriptorAccessor), descriptor)!;
                yield return ma;
            }
        }
    }

    /// <summary>
    /// A utility class to compare object by their reference.
    /// </summary>
    public sealed class ReferenceComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// Gets the instance of the ReferenceComparer class.
        /// </summary>
        public static readonly ReferenceComparer Instance = new ReferenceComparer();

        private ReferenceComparer()
        {
        }

        bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);
        int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class ICollectionTObject<T> : ListObject
    {
        private ICollection<T?>? _coll;

        public override object? List
        {
            get => base.List;
            set
            {
                base.List = value;
                _coll = (ICollection<T?>)value!;
            }
        }

        public override void Clear() => _coll?.Clear();
        public override void Add(object? value, JsonOptions? options = null)
        {
            if (value == null && typeof(T).IsValueType)
            {
                HandleException(new JsonException("JSO0014: JSON error detected. Cannot add null to a collection of '" + typeof(T) + "' elements."), options);
            }

            _coll?.Add((T?)value);
        }
    }

    private sealed class IListObject : ListObject
    {
        private IList? _list;

        public override object? List
        {
            get => base.List;
            set
            {
                base.List = value;
                _list = (IList)value!;
            }
        }

        public override void Clear() => _list?.Clear();
        public override void Add(object? value, JsonOptions? options = null) => _list?.Add(value);
    }

    private sealed class FieldInfoAccessor(FieldInfo fi) : IMemberAccessor
    {
        public object? Get(object component) => fi.GetValue(component);
        public void Set(object component, object? value) => fi.SetValue(component, value);
    }

    private sealed class PropertyDescriptorAccessor(PropertyDescriptor pd) : IMemberAccessor
    {
        public object? Get(object component) => pd.GetValue(component);
        public void Set(object component, object? value)
        {
            if (pd.IsReadOnly)
                return;

            pd.SetValue(component, value);
        }
    }

    // note: Funcs & Action<T> needs .NET 4+
    private delegate TResult JFunc<T, TResult>(T arg);
    private delegate void JAction<T1, T2>(T1 arg1, T2 arg2);

    private sealed class PropertyInfoAccessor<TComponent, TMember> : IMemberAccessor
    {
        private readonly JFunc<TComponent, TMember>? _get;
        private readonly JAction<TComponent, TMember?>? _set;

        public PropertyInfoAccessor(PropertyInfo pi)
        {
            var get = pi.GetGetMethod();
            if (get != null)
            {
                _get = (JFunc<TComponent, TMember>)Delegate.CreateDelegate(typeof(JFunc<TComponent, TMember>), get);
            }

            var set = pi.GetSetMethod();
            if (set != null)
            {
                _set = (JAction<TComponent, TMember?>)Delegate.CreateDelegate(typeof(JAction<TComponent, TMember?>), set);
            }
        }

        public object? Get(object component)
        {
            if (_get == null)
                return null;

            return _get((TComponent)component);
        }

        public void Set(object component, object? value)
        {
            if (_set == null)
                return;

            _set((TComponent)component, (TMember?)value);
        }
    }

    private static class Conversions
    {
        private static readonly char[] _enumSeparators = [',', ';', '+', '|', ' '];
        private static readonly string[] _dateFormatsUtc = ["yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", "yyyy'-'MM'-'dd'T'HH':'mm'Z'", "yyyyMMdd'T'HH':'mm':'ss'Z'"];

        private static bool IsValid(DateTime dt) => dt != DateTime.MinValue && dt != DateTime.MaxValue && dt.Kind != DateTimeKind.Unspecified;

        public static T? ChangeType<T>(object? input, T? defaultValue = default, IFormatProvider? provider = null)
        {
            if (!TryChangeType(input, provider, out T? value))
                return defaultValue;

            return value;
        }

        public static bool TryChangeType<T>(object? input, out T? value) => TryChangeType(input, null, out value);
        public static bool TryChangeType<T>(object? input, IFormatProvider? provider, out T? value)
        {
            if (!TryChangeType(input, typeof(T), provider, out var tvalue))
            {
                value = default;
                return false;
            }

            value = (T?)tvalue;
            return true;
        }

        public static object? ChangeType(object? input, Type conversionType, object? defaultValue = null, IFormatProvider? provider = null)
        {
            if (!TryChangeType(input, conversionType, provider, out var value))
            {
                if (TryChangeType(defaultValue, conversionType, provider, out var def))
                    return def;

                if (IsReallyValueType(conversionType))
                    return Activator.CreateInstance(conversionType);

                return null;
            }

            return value;
        }

        public static bool TryChangeType(object? input, Type conversionType, out object? value) => TryChangeType(input, conversionType, null, out value);
        public static bool TryChangeType(object? input, Type conversionType, IFormatProvider? provider, out object? value)
        {
            ArgumentNullException.ThrowIfNull(conversionType);
            if (conversionType == typeof(object))
            {
                value = input;
                return true;
            }

            if (IsNullable(conversionType))
            {
                if (input == null)
                {
                    value = null;
                    return true;
                }

                var type = conversionType.GetGenericArguments()[0];
                if (TryChangeType(input, type, provider, out var vtValue))
                {
                    var nt = typeof(Nullable<>).MakeGenericType(type);
                    value = Activator.CreateInstance(nt, vtValue);
                    return true;
                }

                value = null;
                return false;
            }

            value = IsReallyValueType(conversionType) ? Activator.CreateInstance(conversionType) : null;
            if (input == null)
                return !IsReallyValueType(conversionType);

            var inputType = input.GetType();
            if (conversionType.IsAssignableFrom(inputType))
            {
                value = input;
                return true;
            }

            if (conversionType.IsEnum)
                return EnumTryParse(conversionType, input, out value);

            if (inputType.IsEnum)
            {
                var tc = Type.GetTypeCode(inputType);
                if (conversionType == typeof(int))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (int)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (int)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (int)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (int)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (int)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (int)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (int)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(short))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (short)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (short)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (short)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (short)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (short)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (short)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (short)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(long))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (long)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (long)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (long)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (long)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (long)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (long)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (long)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(uint))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (uint)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (uint)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (uint)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (uint)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (uint)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (uint)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (uint)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(ushort))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (ushort)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (ushort)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (ushort)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (ushort)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (ushort)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (ushort)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (ushort)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(ulong))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (ulong)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (ulong)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (ulong)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (ulong)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (ulong)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (ulong)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (ulong)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(byte))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (byte)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (byte)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (byte)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (byte)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (byte)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (byte)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (byte)(sbyte)input;
                            return true;
                    }
                    return false;
                }

                if (conversionType == typeof(sbyte))
                {
                    switch (tc)
                    {
                        case TypeCode.Int32:
                            value = (sbyte)(int)input;
                            return true;

                        case TypeCode.Int16:
                            value = (sbyte)(short)input;
                            return true;

                        case TypeCode.Int64:
                            value = (sbyte)(long)input;
                            return true;

                        case TypeCode.UInt32:
                            value = (sbyte)(uint)input;
                            return true;

                        case TypeCode.UInt16:
                            value = (sbyte)(ushort)input;
                            return true;

                        case TypeCode.UInt64:
                            value = (sbyte)(ulong)input;
                            return true;

                        case TypeCode.Byte:
                            value = (sbyte)(byte)input;
                            return true;

                        case TypeCode.SByte:
                            value = (sbyte)input;
                            return true;
                    }
                    return false;
                }
            }

            if (conversionType == typeof(Guid))
            {
                var svalue = string.Format(provider, "{0}", input).Nullify();
                if (svalue != null && Guid.TryParse(svalue, out var guid))
                {
                    value = guid;
                    return true;
                }
                return false;
            }

            if (conversionType == typeof(Uri))
            {
                var svalue = string.Format(provider, "{0}", input).Nullify();
                if (svalue != null && Uri.TryCreate(svalue, UriKind.RelativeOrAbsolute, out var uri))
                {
                    value = uri;
                    return true;
                }
                return false;
            }

            if (conversionType == typeof(nint))
            {
                if (nint.Size == 8)
                {
                    if (TryChangeType(input, provider, out long l))
                    {
                        value = new nint(l);
                        return true;
                    }
                }
                else if (TryChangeType(input, provider, out int i))
                {
                    value = new nint(i);
                    return true;
                }
                return false;
            }

            if (conversionType == typeof(int))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((int)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((int)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((int)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((int)(byte)input);
                    return true;
                }
            }

            if (conversionType == typeof(long))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((long)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((long)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((long)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((long)(byte)input);
                    return true;
                }

                if (inputType == typeof(TimeSpan))
                {
                    value = ((TimeSpan)input).Ticks;
                    return true;
                }
            }

            if (conversionType == typeof(short))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((short)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((short)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((short)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((short)(byte)input);
                    return true;
                }
            }

            if (conversionType == typeof(sbyte))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((sbyte)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((sbyte)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((sbyte)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((sbyte)(byte)input);
                    return true;
                }
            }

            if (conversionType == typeof(uint))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((uint)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((uint)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((uint)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((uint)(sbyte)input);
                    return true;
                }
            }

            if (conversionType == typeof(ulong))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((ulong)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((ulong)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((ulong)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((ulong)(sbyte)input);
                    return true;
                }
            }

            if (conversionType == typeof(ushort))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((ushort)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((ushort)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((ushort)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((ushort)(sbyte)input);
                    return true;
                }
            }

            if (conversionType == typeof(byte))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((byte)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((byte)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((byte)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((byte)(sbyte)input);
                    return true;
                }
            }

            if (conversionType == typeof(DateTime))
            {
                if (inputType == typeof(long))
                {
                    value = new DateTime((long)input, DateTimeKind.Utc);
                    return true;
                }

                if (inputType == typeof(DateTimeOffset))
                {
                    value = ((DateTimeOffset)input).DateTime;
                    return true;
                }
            }

            if (conversionType == typeof(DateTimeOffset))
            {
                if (inputType == typeof(long))
                {
                    value = new DateTimeOffset(new DateTime((long)input, DateTimeKind.Utc));
                    return true;
                }

                if (inputType == typeof(DateTime))
                {
                    var dt = (DateTime)input;
                    if (IsValid(dt))
                    {
                        value = new DateTimeOffset((DateTime)input);
                        return true;
                    }
                }
            }

            if (conversionType == typeof(TimeSpan))
            {
                if (inputType == typeof(long))
                {
                    value = new TimeSpan((long)input);
                    return true;
                }

                if (inputType == typeof(DateTime))
                {
                    value = ((DateTime)value!).TimeOfDay;
                    return true;
                }

                if (inputType == typeof(DateTimeOffset))
                {
                    value = ((DateTimeOffset)value!).TimeOfDay;
                    return true;
                }

                if (TryChangeType(input, provider, out string? sv) && TimeSpan.TryParse(sv, provider, out var ts))
                {
                    value = ts;
                    return true;
                }
            }

            var isGenericList = IsGenericList(conversionType, out var elementType);
            if (conversionType.IsArray || isGenericList)
            {
                if (input is IEnumerable enumerable)
                {
                    if (!isGenericList)
                    {
                        elementType = conversionType.GetElementType();
                    }

                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType!))!;
                    var count = 0;
                    foreach (var obj in enumerable)
                    {
                        count++;
                        if (TryChangeType(obj, elementType!, provider, out var element))
                        {
                            list.Add(element);
                        }
                    }

                    // at least one was converted
                    if (count > 0 && list.Count > 0)
                    {
                        if (isGenericList)
                        {
                            value = list;
                        }
                        else
                        {
                            value = list.GetType()?.GetMethod(nameof(List<object>.ToArray))?.Invoke(list, null)!;
                        }
                        return true;
                    }
                }
            }

            if (conversionType == typeof(CultureInfo) || conversionType == typeof(IFormatProvider))
            {
                try
                {
                    if (input is int lcid)
                    {
                        value = CultureInfo.GetCultureInfo(lcid);
                        return true;
                    }
                    else
                    {
                        var si = input?.ToString();
                        if (si != null)
                        {
                            if (int.TryParse(si, out lcid))
                            {
                                value = CultureInfo.GetCultureInfo(lcid);
                                return true;
                            }

                            value = CultureInfo.GetCultureInfo(si);
                            return true;
                        }
                    }
                }
                catch
                {
                    // do nothing, wrong culture, etc.
                }
                return false;
            }

            if (conversionType == typeof(bool))
            {
                if (true.Equals(input))
                {
                    value = true;
                    return true;
                }

                if (false.Equals(input))
                {
                    value = false;
                    return true;
                }

                var svalue = string.Format(provider, "{0}", input).Nullify();
                if (svalue == null)
                    return false;

                if (bool.TryParse(svalue, out var b))
                {
                    value = b;
                    return true;
                }

                if (svalue.EqualsIgnoreCase("y") || svalue.EqualsIgnoreCase("yes"))
                {
                    value = true;
                    return true;
                }

                if (svalue.EqualsIgnoreCase("n") || svalue.EqualsIgnoreCase("no"))
                {
                    value = false;
                    return true;
                }

                if (TryChangeType(input, out long bl))
                {
                    value = bl != 0;
                    return true;
                }
                return false;
            }

            // in general, nothing is convertible to anything but one of these, IConvertible is 100% stupid thing
            bool isWellKnownConvertible()
            {
                return conversionType == typeof(short) || conversionType == typeof(int) ||
                    conversionType == typeof(string) || conversionType == typeof(byte) ||
                    conversionType == typeof(char) || conversionType == typeof(DateTime) ||
                    conversionType == typeof(DBNull) || conversionType == typeof(decimal) ||
                    conversionType == typeof(double) || conversionType.IsEnum ||
                    conversionType == typeof(short) || conversionType == typeof(int) ||
                    conversionType == typeof(long) || conversionType == typeof(sbyte) ||
                    conversionType == typeof(bool) || conversionType == typeof(float) ||
                    conversionType == typeof(ushort) || conversionType == typeof(uint) ||
                    conversionType == typeof(ulong);
            }

            if (isWellKnownConvertible() && input is IConvertible convertible)
            {
                try
                {
                    value = convertible.ToType(conversionType, provider);
                    if (value is DateTime dt && !IsValid(dt))
                        return false;

                    return true;
                }
                catch
                {
                    // continue;
                }
            }

            if (input != null)
            {
                var inputConverter = TypeDescriptor.GetConverter(input);
                if (inputConverter != null)
                {
                    if (inputConverter.CanConvertTo(conversionType))
                    {
                        try
                        {
                            value = inputConverter.ConvertTo(null, provider as CultureInfo, input, conversionType);
                            return true;
                        }
                        catch
                        {
                            // continue;
                        }
                    }
                }
            }

            var converter = TypeDescriptor.GetConverter(conversionType);
            if (converter != null)
            {
                if (converter.CanConvertTo(conversionType))
                {
                    try
                    {
                        value = converter.ConvertTo(null, provider as CultureInfo, input, conversionType);
                        return true;
                    }
                    catch
                    {
                        // continue;
                    }
                }

                if (converter.CanConvertFrom(inputType))
                {
                    try
                    {
                        value = converter.ConvertFrom(null, provider as CultureInfo, input!);
                        return true;
                    }
                    catch
                    {
                        // continue;
                    }
                }
            }

            if (conversionType == typeof(string))
            {
                value = string.Format(provider, "{0}", input);
                return true;
            }

            return false;
        }

        public static ulong EnumToUInt64(object value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var typeCode = Convert.GetTypeCode(value);
#pragma warning disable IDE0010 // Add missing cases
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (typeCode)
#pragma warning restore IDE0066 // Convert switch statement to expression
#pragma warning restore IDE0010 // Add missing cases
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);

                //case TypeCode.String:
                default:
                    return ChangeType<ulong>(value, 0, CultureInfo.InvariantCulture);
            }
        }

        private static bool StringToEnum(Type type, string[] names, Array values, string input, out object value)
        {
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i].EqualsIgnoreCase(input))
                {
                    value = values.GetValue(i)!;
                    return true;
                }
            }

            for (var i = 0; i < values.GetLength(0); i++)
            {
                var valuei = values.GetValue(i)!;
                if (input.Length > 0 && input[0] == '-')
                {
                    var ul = (long)EnumToUInt64(valuei);
                    if (ul.ToString().EqualsIgnoreCase(input))
                    {
                        value = valuei;
                        return true;
                    }
                }
                else
                {
                    var ul = EnumToUInt64(valuei);
                    if (ul.ToString().EqualsIgnoreCase(input))
                    {
                        value = valuei;
                        return true;
                    }
                }
            }

            if (char.IsDigit(input[0]) || input[0] == '-' || input[0] == '+')
            {
                var obj = EnumToObject(type, input);
                if (obj == null)
                {
                    value = Activator.CreateInstance(type)!;
                    return false;
                }

                value = obj;
                return true;
            }

            value = Activator.CreateInstance(type)!;
            return false;
        }

        public static object EnumToObject(Type enumType, object value)
        {
            ArgumentNullException.ThrowIfNull(enumType);
            ArgumentNullException.ThrowIfNull(value);
            if (!enumType.IsEnum)
                throw new ArgumentException(null, nameof(enumType));

            var underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType == typeof(long))
                return Enum.ToObject(enumType, ChangeType<long>(value));

            if (underlyingType == typeof(ulong))
                return Enum.ToObject(enumType, ChangeType<ulong>(value));

            if (underlyingType == typeof(int))
                return Enum.ToObject(enumType, ChangeType<int>(value));

            if ((underlyingType == typeof(uint)))
                return Enum.ToObject(enumType, ChangeType<uint>(value));

            if (underlyingType == typeof(short))
                return Enum.ToObject(enumType, ChangeType<short>(value));

            if (underlyingType == typeof(ushort))
                return Enum.ToObject(enumType, ChangeType<ushort>(value));

            if (underlyingType == typeof(byte))
                return Enum.ToObject(enumType, ChangeType<byte>(value));

            if (underlyingType == typeof(sbyte))
                return Enum.ToObject(enumType, ChangeType<sbyte>(value));

            throw new ArgumentException(null, nameof(enumType));
        }

        public static object ToEnum(string text, Type enumType)
        {
            ArgumentNullException.ThrowIfNull(enumType);
            EnumTryParse(enumType, text, out var value);
            return value;
        }

        // Enum.TryParse is not supported by all .NET versions the same way
        public static bool EnumTryParse(Type type, object input, out object value)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (!type.IsEnum)
                throw new ArgumentException(null, nameof(type));

            if (input == null)
            {
                value = Activator.CreateInstance(type)!;
                return false;
            }

            var stringInput = string.Format(CultureInfo.InvariantCulture, "{0}", input);
            stringInput = stringInput.Nullify();
            if (stringInput == null)
            {
                value = Activator.CreateInstance(type)!;
                return false;
            }

            if (stringInput.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (ulong.TryParse(stringInput.AsSpan(2), NumberStyles.HexNumber, null, out var ulx))
                {
                    value = ToEnum(ulx.ToString(CultureInfo.InvariantCulture), type);
                    return true;
                }
            }

            var names = Enum.GetNames(type);
            if (names.Length == 0)
            {
                value = Activator.CreateInstance(type)!;
                return false;
            }

            var values = Enum.GetValues(type);
            // some enums like System.CodeDom.MemberAttributes *are* flags but are not declared with Flags...
            if (!type.IsDefined(typeof(FlagsAttribute), true) && stringInput.IndexOfAny(_enumSeparators) < 0)
                return StringToEnum(type, names, values, stringInput, out value);

            // multi value enum
            var tokens = stringInput.Split(_enumSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                value = Activator.CreateInstance(type)!;
                return false;
            }

            ulong ul = 0;
            foreach (var tok in tokens)
            {
                var token = tok.Nullify(); // NOTE: we don't consider empty tokens as errors
                if (token == null)
                    continue;

                if (!StringToEnum(type, names, values, token, out var tokenValue))
                {
                    value = Activator.CreateInstance(type)!;
                    return false;
                }

                ulong tokenUl;
#pragma warning disable IDE0010 // Add missing cases
#pragma warning disable IDE0066 // Convert switch statement to expression
                switch (Convert.GetTypeCode(tokenValue))
#pragma warning restore IDE0066 // Convert switch statement to expression
#pragma warning restore IDE0010 // Add missing cases
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                        tokenUl = (ulong)Convert.ToInt64(tokenValue, CultureInfo.InvariantCulture);
                        break;

                    default:
                        tokenUl = Convert.ToUInt64(tokenValue, CultureInfo.InvariantCulture);
                        break;
                }

                ul |= tokenUl;
            }
            value = Enum.ToObject(type, ul);
            return true;
        }

        public static bool IsGenericList(Type type, [NotNullWhen(true)] out Type? elementType)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        private static bool IsReallyValueType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return type.IsValueType && !IsNullable(type);
        }

        public static bool IsNullable(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}

/// <summary>
/// Define options for JSON.
/// </summary>
public class JsonOptions
{
    private readonly List<Exception> _exceptions = [];
    internal static DateTimeStyles _defaultDateTimeStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonOptions" /> class.
    /// </summary>
    public JsonOptions()
    {
        SerializationOptions = JsonSerializationOptions.Default;
        ThrowExceptions = true;
        DateTimeStyles = _defaultDateTimeStyles;
        FormattingTab = " ";
        StreamingBufferChunkSize = ushort.MaxValue;
        MaximumExceptionsCount = 100;
    }

    /// <summary>
    /// Gets a value indicating the current serialization level.
    /// </summary>
    /// <value>
    /// The current serialization level.
    /// </value>
    public int SerializationLevel { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether exceptions can be thrown during serialization or deserialization.
    /// If this is set to false, exceptions will be stored in the Exceptions collection.
    /// However, if the number of exceptions is equal to or higher than MaximumExceptionsCount, an exception will be thrown.
    /// </summary>
    /// <value>
    /// <c>true</c> if exceptions can be thrown on serialization or deserialization; otherwise, <c>false</c>.
    /// </value>
    public virtual bool ThrowExceptions { get; set; }

    /// <summary>
    /// Gets or sets the maximum exceptions count.
    /// </summary>
    /// <value>
    /// The maximum exceptions count.
    /// </value>
    public virtual int MaximumExceptionsCount { get; set; }

    /// <summary>
    /// Gets or sets the JSONP callback. It will be added as wrapper around the result.
    /// Check this article for more: http://en.wikipedia.org/wiki/JSONP
    /// </summary>
    /// <value>
    /// The JSONP callback name.
    /// </value>
    public virtual string? JsonPCallback { get; set; }

    /// <summary>
    /// Gets or sets the guid format.
    /// </summary>
    /// <value>
    /// The guid format.
    /// </value>
    public virtual string? GuidFormat { get; set; }

    /// <summary>
    /// Gets or sets the date time format.
    /// </summary>
    /// <value>
    /// The date time format.
    /// </value>
    public virtual string? DateTimeFormat { get; set; }

    /// <summary>
    /// Gets or sets the date time offset format.
    /// </summary>
    /// <value>
    /// The date time offset format.
    /// </value>
    public virtual string? DateTimeOffsetFormat { get; set; }

    /// <summary>
    /// Gets or sets the date time styles.
    /// </summary>
    /// <value>
    /// The date time styles.
    /// </value>
    public virtual DateTimeStyles DateTimeStyles { get; set; }

    /// <summary>
    /// Gets or sets the size of the streaming buffer chunk. Minimum value is 512.
    /// </summary>
    /// <value>
    /// The size of the streaming buffer chunk.
    /// </value>
    public virtual int StreamingBufferChunkSize { get; set; }

    /// <summary>
    /// Gets or sets the formatting tab string.
    /// </summary>
    /// <value>
    /// The formatting tab.
    /// </value>
    public virtual string? FormattingTab { get; set; }

    /// <summary>
    /// Gets the deseralization exceptions. Will be empty if ThrowExceptions is set to false.
    /// </summary>
    /// <value>
    /// The list of deseralization exceptions.
    /// </value>
    public virtual Exception[] Exceptions => [.. _exceptions];

    /// <summary>
    /// Finalizes the serialization members from an initial setup of members.
    /// </summary>
    /// <param name="type">The input type. May not be null.</param>
    /// <param name="members">The members. May not be null.</param>
    /// <returns>A non-null list of members.</returns>
    public virtual IEnumerable<Json.MemberDefinition> FinalizeSerializationMembers(Type type, IEnumerable<Json.MemberDefinition> members) => members;

    /// <summary>
    /// Finalizes the deserialization members from an initial setup of members.
    /// </summary>
    /// <param name="type">The input type. May not be null.</param>
    /// <param name="members">The members. May not be null.</param>
    /// <returns>A non-null list of members.</returns>
    public virtual IEnumerable<Json.MemberDefinition> FinalizeDeserializationMembers(Type type, IEnumerable<Json.MemberDefinition> members) => members;

    /// <summary>
    /// Gets or sets the serialization options.
    /// </summary>
    /// <value>The serialization options.</value>
    public virtual JsonSerializationOptions SerializationOptions { get; set; }

    /// <summary>
    /// Gets or sets a write value callback.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? WriteValueCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called before an object (not a value) is serialized.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? BeforeWriteObjectCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called before an object (not a value) is serialized.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? AfterWriteObjectCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called before an object field or property is serialized.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? WriteNamedValueObjectCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called before an instance of an object is created.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? CreateInstanceCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called during deserialization, before a dictionary entry is mapped to a target object.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? MapEntryCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called during deserialization, before a dictionary entry is applied to a target object.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? ApplyEntryCallback { get; set; }

    /// <summary>
    /// Gets or sets a callback that is called during deserialization, to deserialize a list object.
    /// </summary>
    /// <value>The callback.</value>
    public virtual JsonCallback? GetListObjectCallback { get; set; }

    /// <summary>
    /// Gets or sets a utility class that will store an object graph to avoid serialization cycles.
    /// If null, a Dictionary&lt;object, object&gt; using an object reference comparer will be used.
    /// </summary>
    /// <value>The object graph instance.</value>
    public virtual IDictionary<object, object?>? ObjectGraph { get; set; }

    /// <summary>
    /// Adds an exception to the list of exceptions.
    /// </summary>
    /// <param name="error">The exception to add.</param>
    public virtual void AddException(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (_exceptions.Count >= MaximumExceptionsCount)
            throw new JsonException("JSO0015: Two many JSON errors detected (" + _exceptions.Count + ").", error);

        _exceptions.Add(error);
    }

    internal int FinalStreamingBufferChunkSize => Math.Max(512, StreamingBufferChunkSize);
    internal IDictionary<object, object?> FinalObjectGraph => ObjectGraph ?? new Dictionary<object, object?>(Json.ReferenceComparer.Instance);

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>A newly created insance of this class with all values copied.</returns>
    public virtual JsonOptions Clone()
    {
        var clone = new JsonOptions
        {
            AfterWriteObjectCallback = AfterWriteObjectCallback,
            ApplyEntryCallback = ApplyEntryCallback,
            BeforeWriteObjectCallback = BeforeWriteObjectCallback,
            CreateInstanceCallback = CreateInstanceCallback,
            DateTimeFormat = DateTimeFormat,
            DateTimeOffsetFormat = DateTimeOffsetFormat,
            DateTimeStyles = DateTimeStyles,
            FormattingTab = FormattingTab,
            GetListObjectCallback = GetListObjectCallback,
            GuidFormat = GuidFormat,
            MapEntryCallback = MapEntryCallback,
            MaximumExceptionsCount = MaximumExceptionsCount,
            SerializationOptions = SerializationOptions,
            StreamingBufferChunkSize = StreamingBufferChunkSize,
            ThrowExceptions = ThrowExceptions,
            WriteNamedValueObjectCallback = WriteNamedValueObjectCallback,
            WriteValueCallback = WriteValueCallback
        };
        clone._exceptions.AddRange(_exceptions);
        return clone;
    }

    /// <summary>
    /// Gets a key that can be used for type cache.
    /// </summary>
    /// <returns>A cache key.</returns>
    public virtual string GetCacheKey() => ((int)SerializationOptions).ToString();
}

/// <summary>
/// Defines a callback delegate to customize JSON serialization and deserialization.
/// </summary>
public delegate void JsonCallback(JsonEventArgs e);

/// <summary>
/// Defines a type of JSON event.
/// </summary>
public enum JsonEventType
{
    /// <summary>
    /// An unspecified type of event.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The write value event type.
    /// </summary>
    WriteValue,

    /// <summary>
    /// The before write object event type.
    /// </summary>
    BeforeWriteObject,

    /// <summary>
    /// The after write object event type.
    /// </summary>
    AfterWriteObject,

    /// <summary>
    /// The write named value object event type.
    /// </summary>
    WriteNamedValueObject,

    /// <summary>
    /// The create instance event type.
    /// </summary>
    CreateInstance,

    /// <summary>
    /// The map entry event type.
    /// </summary>
    MapEntry,

    /// <summary>
    /// The apply entry event type.
    /// </summary>
    ApplyEntry,

    /// <summary>
    /// The get list object event type.
    /// </summary>
    GetListObject,
}

/// <summary>
/// Provides data for a JSON event.
/// </summary>
public class JsonEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonEventArgs"/> class.
    /// </summary>
    /// <param name="writer">The writer currently in use.</param>
    /// <param name="value">The value on the stack.</param>
    /// <param name="objectGraph">The current serialization object graph.</param>
    /// <param name="options">The options currently in use.</param>
    /// <param name="name">The field or property name.</param>
    /// <param name="component">The component holding the value.</param>
    public JsonEventArgs(TextWriter? writer, object? value, IDictionary<object, object?> objectGraph, JsonOptions options, string? name = null, object? component = null)
    {
        ArgumentNullException.ThrowIfNull(objectGraph);
        Options = options;
        Writer = writer;
        ObjectGraph = objectGraph;
        Value = value;
        Name = name;
        Component = component;
    }

    /// <summary>
    /// Gets the options currently in use.
    /// </summary>
    /// <value>The options.</value>
    public JsonOptions Options { get; }

    /// <summary>
    /// Gets the writer currently in use.
    /// </summary>
    /// <value>The writer.</value>
    public TextWriter? Writer { get; }

    /// <summary>
    /// Gets the current serialization object graph.
    /// </summary>
    /// <value>The object graph.</value>
    public IDictionary<object, object?> ObjectGraph { get; }

    /// <summary>
    /// Gets the component holding the value. May be null.
    /// </summary>
    /// <value>The component.</value>
    public virtual object? Component { get; }

    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    /// <value>
    /// The type of the event.
    /// </value>
    public virtual JsonEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonEventArgs"/> is handled.
    /// An handled object can be skipped, not written to the stream. If the object is written, First must be set to false, otherwise it must not be changed.
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public virtual bool Handled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object being handled is first in the list.
    /// If the object is handled and written to the stream, this must be set to false after the stream is written.
    /// If the object is skipped, it must not be changed.
    /// </summary>
    /// <value><c>true</c> if this is the first object; otherwise, <c>false</c>.</value>
    public virtual bool First { get; set; }

    /// <summary>
    /// Gets or sets the value on the stack.
    /// </summary>
    /// <value>The value.</value>
    public virtual object? Value { get; set; }

    /// <summary>
    /// Gets or sets the name on the stack. The Name can be a property or field name when serializing objects. May be null.
    /// </summary>
    /// <value>The value.</value>
    public virtual string? Name { get; set; }
}

/// <summary>
/// Provides options for JSON.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class JsonAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAttribute"/> class.
    /// </summary>
    public JsonAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAttribute"/> class.
    /// </summary>
    /// <param name="name">The name to use for JSON serialization and deserialization.</param>
    public JsonAttribute(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }

    /// <summary>
    /// Gets or sets the name to use for JSON serialization and deserialization.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore this instance's owner when serializing.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance's owner must be ignored when serializing; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreWhenSerializing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore this instance's owner when deserializing.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance's owner must be ignored when deserializing; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreWhenDeserializing { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <value>
    /// The default value.
    /// </value>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance has a default value. In this case, it's defined by the DefaultValue property.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance has default value; otherwise, <c>false</c>.
    /// </value>
    public bool HasDefaultValue { get; set; }
}

/// <summary>
/// Define options for JSON serialization.
/// </summary>
[Flags]
public enum JsonSerializationOptions
{
    /// <summary>
    /// No option.
    /// Use Type Descriptor (including custom ones) when serializing custom objects.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Use pure reflection when serializing custom objects.
    /// </summary>
    UseReflection = 0x1,

    /// <summary>
    /// Avoid fields and properties marked with the XmlIgnore attribute.
    /// </summary>
    UseXmlIgnore = 0x2,

    /// <summary>
    /// Use the format defined in the DateTimeFormat property of the JsonOptions class.
    /// </summary>
    DateFormatCustom = 0x4,

    /// <summary>
    /// Serializes fields.
    /// </summary>
    SerializeFields = 0x8,

    /// <summary>
    /// Use the ISerializable interface. Not supported anymore.
    /// </summary>
    UseISerializable = 0x10,

    /// <summary>
    /// Use the [new Date(utc milliseconds)] format.
    /// Note this format is not generally supported by browsers native JSON parsers.
    /// </summary>
    DateFormatJs = 0x20,

    /// <summary>
    /// Use the ISO 8601 string format ('s' DateTime format).
    /// </summary>
    DateFormatIso8601 = 0x40,

    /// <summary>
    /// Avoid fields and properties marked with the ScriptIgnore attribute.
    /// </summary>
    UseScriptIgnore = 0x80,

    /// <summary>
    /// Use the ISO 8601 roundtrip string format ('o' DateTime format).
    /// </summary>
    DateFormatRoundtripUtc = 0x100,

    /// <summary>
    /// Serialize enum values as text.
    /// </summary>
    EnumAsText = 0x200,

    /// <summary>
    /// Continue serialization if a cycle was detected.
    /// </summary>
    ContinueOnCycle = 0x400,

    /// <summary>
    /// Continue serialization if getting a value throws error.
    /// </summary>
    ContinueOnValueError = 0x800,

    /// <summary>
    /// Don't serialize properties with a null value.
    /// </summary>
    SkipNullPropertyValues = 0x1000,

    /// <summary>
    /// Use the format defined in the DateTimeOffsetFormat property of the JsonOptions class.
    /// </summary>
    DateTimeOffsetFormatCustom = 0x2000,

    /// <summary>
    /// Don't serialize null date time values.
    /// </summary>
    SkipNullDateTimeValues = 0x4000,

    /// <summary>
    /// Automatically parse date time.
    /// </summary>
    AutoParseDateTime = 0x8000,

    /// <summary>
    /// Write dictionary keys without quotes.
    /// </summary>
    WriteKeysWithoutQuotes = 0x10000,

    /// <summary>
    /// Serializes byte arrays as base 64 strings.
    /// </summary>
    ByteArrayAsBase64 = 0x20000,

    /// <summary>
    /// Serializes streams as base 64 strings.
    /// </summary>
    StreamsAsBase64 = 0x40000,

    /// <summary>
    /// Don't serialize value type with a zero value.
    /// </summary>
    SkipZeroValueTypes = 0x80000,

    /// <summary>
    /// Use the JSON attribute.
    /// </summary>
    UseJsonAttribute = 0x100000,

    /// <summary>
    /// Don't serialize values equal to the default member (property, field) value, if defined.
    /// </summary>
    SkipDefaultValues = 0x200000,

    /// <summary>
    /// Serialize TimeSpan values as text.
    /// </summary>
    TimeSpanAsText = 0x400000,

    /// <summary>
    /// Skip members with get only method.
    /// </summary>
    SkipGetOnly = 0x800000,

    /// <summary>
    /// The default value.
    /// </summary>
    Default = UseXmlIgnore | UseScriptIgnore | SerializeFields | AutoParseDateTime | UseJsonAttribute | SkipGetOnly | SkipDefaultValues | SkipZeroValueTypes | SkipNullPropertyValues | SkipNullDateTimeValues,
}

/// <summary>
/// The exception that is thrown when a JSON error occurs.
/// </summary>
[Serializable]
public class JsonException : Exception
{
    /// <summary>
    /// The commn error prefix.
    /// </summary>
    public const string Prefix = "JSO";

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException"/> class.
    /// </summary>
    public JsonException()
        : base(Prefix + "0001: JSON exception.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JsonException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
    public JsonException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    public JsonException(Exception innerException)
        : base(null, innerException)
    {
    }

    /// <summary>
    /// Gets the errror code.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The error code.</returns>
    public static int GetCode(string message)
    {
        if (message == null)
            return -1;

        if (!message.StartsWith(Prefix, StringComparison.Ordinal))
            return -1;

        var pos = message.IndexOf(':', Prefix.Length);
        if (pos < 0)
            return -1;

        return int.TryParse(message.AsSpan(Prefix.Length, pos - Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var i) ? i : -1;
    }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    /// <value>
    /// The error code.
    /// </value>
    public int Code => GetCode(Message);
}

#pragma warning restore IDE0054 // Use compound assignment
#pragma warning restore IDE0056 // Use index operator
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
#pragma warning restore IDE0057 // Use range operator
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0063 // Use simple 'using' statement

