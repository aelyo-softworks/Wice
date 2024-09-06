namespace Wice.Effects;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract partial class Effect(uint sourcesCount = 0) : BaseObject, IGraphicsEffect, IGraphicsEffectSource, Windows.Graphics.Effects.IGraphicsEffectD2D1Interop
{
    private static readonly ConcurrentDictionary<Type, List<PropDef>> _properties = new();
    private readonly List<IGraphicsEffectSource?> _sources = [];
    private readonly Lazy<IComObject<IPropertyValueStatics>> _statics = new(() => ComObject.GetActivationFactory<IPropertyValueStatics>("Windows.Foundation.PropertyValue")!);
    private string? _name;

    public uint MaximumSourcesCount { get; } = sourcesCount;
    public override string? Name { get => _name ?? string.Empty; set => _name = value; } // *must* not be null for IGraphicsEffectD2D1Interop
    public virtual bool Cached { get; set; }
    public virtual D2D1_BUFFER_PRECISION Precision { get; set; }
    public IList<IGraphicsEffectSource?> Sources => _sources;

    public Guid Clsid
    {
        get
        {
            var att = GetType().GetCustomAttribute<GuidAttribute>();
            if (att == null)
                throw new InvalidOperationException("Type '" + GetType().FullName + "' must have a valid Guid attribute set.");

            return new Guid(att.Value);
        }
    }

    protected virtual IGraphicsEffectSource? GetSource(int index)
    {
        if (index >= MaximumSourcesCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index >= _sources.Count)
            return null;

        return _sources[index];
    }

    protected virtual void SetSource(int index, IGraphicsEffectSource? effect)
    {
        // effect can be null
        if (index >= MaximumSourcesCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index >= _sources.Count)
        {
            for (var i = 0; i < (index - _sources.Count); i++)
            {
                _sources.Add(null);
            }
            _sources.Add(effect);
        }
        else
        {
            _sources[index] = effect;
        }
    }

    private sealed class PropDef(PropertyInfo property, int index, GRAPHICS_EFFECT_PROPERTY_MAPPING mapping)
    {
        public PropertyInfo Property => property;
        public int Index => index;
        public GRAPHICS_EFFECT_PROPERTY_MAPPING Mapping => mapping;

        public override string ToString() => "#" + Index + " " + Property?.Name;

        public HRESULT GetPropertyValue(Effect effect, out nint ptr)
        {
            var statics = effect._statics.Value;
            var value = Property.GetValue(effect);
            if (value == null || Convert.IsDBNull(value))
                return statics.Object.CreateEmpty(out ptr);

            // enums are passed as UInt32
            var type = value.GetType();
            if (type.IsEnum)
                return statics.Object.CreateUInt32((uint)(int)value, out ptr);

            var tc = Type.GetTypeCode(type);
            switch (tc)
            {
                case TypeCode.Boolean:
                    return statics.Object.CreateBoolean((bool)value, out ptr);

                case TypeCode.Byte:
                    return statics.Object.CreateUInt8((byte)value, out ptr);

                case TypeCode.Char:
                    return statics.Object.CreateChar16((char)value, out ptr);

                case TypeCode.DateTime:
                    return statics.Object.CreateDateTime((DateTime)value, out ptr);

                case TypeCode.Double:
                    return statics.Object.CreateDouble((double)value, out ptr);

                case TypeCode.Empty:
                    return statics.Object.CreateEmpty(out ptr);

                case TypeCode.Int16:
                    return statics.Object.CreateInt16((short)value, out ptr);

                case TypeCode.Int32:
                    return statics.Object.CreateInt32((int)value, out ptr);

                case TypeCode.Int64:
                    return statics.Object.CreateInt64((long)value, out ptr);

                case TypeCode.SByte:
                    return statics.Object.CreateUInt8((byte)(sbyte)value, out ptr);

                case TypeCode.Single:
                    return statics.Object.CreateSingle((float)value, out ptr);

                case TypeCode.String:
                    throw new NotImplementedException();
                //return statics.Object.CreateString((string)value, out ptr);

                case TypeCode.UInt16:
                    return statics.Object.CreateUInt16((ushort)value, out ptr);

                case TypeCode.UInt32:
                    return statics.Object.CreateUInt32((uint)value, out ptr);

                case TypeCode.UInt64:
                    return statics.Object.CreateUInt64((ulong)value, out ptr);

                default:
                    if (type == typeof(Guid))
                        return statics.Object.CreateGuid((Guid)value, out ptr);

                    if (type == typeof(Point))
                        return statics.Object.CreatePoint((Point)value, out ptr);

                    if (type == typeof(Size))
                        return statics.Object.CreateSize((Size)value, out ptr);

                    if (type == typeof(Rect))
                        return statics.Object.CreateRect((Rect)value, out ptr);

                    if (type == typeof(TimeSpan))
                        return statics.Object.CreateTimeSpan((TimeSpan)value, out ptr);

                    if (type == typeof(D2D_VECTOR_4F))
                        return statics.Object.CreateSingleArray(((D2D_VECTOR_4F)value).ToArray(), out ptr);

                    if (type == typeof(D2D_VECTOR_2F))
                        return statics.Object.CreateSingleArray(((D2D_VECTOR_2F)value).ToArray(), out ptr);

                    if (type == typeof(D3DCOLORVALUE))
                        return statics.Object.CreateSingleArray(((D3DCOLORVALUE)value).ToArray(), out ptr);

                    if (type == typeof(D2D_MATRIX_4X4_F))
                        return statics.Object.CreateSingleArray(((D2D_MATRIX_4X4_F)value).ToArray(), out ptr);

                    if (type == typeof(D2D_MATRIX_5X4_F))
                        return statics.Object.CreateSingleArray(((D2D_MATRIX_5X4_F)value).ToArray(), out ptr);

                    if (value is float[] floats)
                        return statics.Object.CreateSingleArray(floats, out ptr);

                    if (value is Interop.IInspectable inspectable)
                        return statics.Object.CreateInspectable(inspectable, out ptr);

                    break;
            }

            ptr = 0;
            Application.Trace(this + " " + this + " E_NOTIMPL");
            return Constants.E_NOTIMPL;
        }
    }

    private static List<PropDef> GetPropDefs([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
    {
        if (!_properties.TryGetValue(type, out var list))
        {
            list = [];
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
            {
                var effectProp = BaseObjectProperty.GetProperties(type).OfType<EffectProperty>().FirstOrDefault(p => p.Name == prop.Name);
                if (effectProp == null)
                    continue;

                var def = new PropDef(prop, effectProp.Index, effectProp.Mapping);
                list.Add(def);
            }
            list = _properties.AddOrUpdate(type, list, (k, o) => o);
        }
        return list;
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetEffectId(out Guid id)
    {
        id = Clsid;
        return Constants.S_OK;
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetNamedPropertyMapping(PWSTR name, out uint index, out GRAPHICS_EFFECT_PROPERTY_MAPPING mapping)
    {
        //Application.Trace(this + "name:" + name);

        var n = name.ToString();
        var defs = GetPropDefs(GetType());
        var def = defs.FirstOrDefault(p => p.Property.Name.EqualsIgnoreCase(n));
        if (def == null)
        {
            index = 0;
            mapping = 0;
            Application.Trace(this + " name:'" + name + "' E_INVALIDARG");
            return Constants.E_INVALIDARG;
        }

        index = (uint)def.Index;
        mapping = def.Mapping;
        return Constants.S_OK;
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetPropertyCount(out uint count)
    {
        var defs = GetPropDefs(GetType());
        count = (uint)defs.Count;
        //Application.Trace(this + " count:" + count);
        return Constants.S_OK;
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetProperty(uint index, out nint value)
    {
        try
        {
            //Application.Trace(this + " index:" + index);
            var defs = GetPropDefs(GetType());
            if (index >= defs.Count)
            {
                value = 0;
                Application.Trace(this + " index:" + index + " E_BOUNDS");
                return Constants.E_BOUNDS;
            }

            return defs[(int)index].GetPropertyValue(this, out value);
        }
        catch
        {
            Application.Trace(this + " index:" + index + " E_FAIL");
            value = 0;
            return Constants.E_FAIL;
        }
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetSource(uint index, out IGraphicsEffectSource? source)
    {
        //Application.Trace(this + " index:" + index);
        if (index >= MaximumSourcesCount || index >= _sources.Count)
        {
            source = null;
            Application.Trace(this + " index:" + index + " E_BOUNDS");
            return Constants.E_BOUNDS;
        }

        source = _sources[(int)index];
        return Constants.S_OK;
    }

    HRESULT Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.GetSourceCount(out uint count)
    {
        if (MaximumSourcesCount == int.MaxValue)
        {
            count = (uint)_sources.Count;
        }
        else
        {
            count = MaximumSourcesCount;
        }
        //Application.Trace(this + " count:" + count);
        return Constants.S_OK;
    }
}
