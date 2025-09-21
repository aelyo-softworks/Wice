namespace Wice.Effects;

/// <summary>
/// Represents an abstract base class for effects that can be applied to graphics.
/// </summary>
/// <param name="sourcesCount">The number of effect sources.</param>
#if !NETFRAMEWORK
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
public abstract partial class Effect(uint sourcesCount = 0) : BaseObject, IGraphicsEffect, IGraphicsEffectSource, IGraphicsEffectD2D1Interop
{
    private static readonly ConcurrentDictionary<Type, List<PropDef>> _properties = new();

    private readonly List<IGraphicsEffectSource?> _sources = [];
    private readonly Lazy<IComObject<IPropertyValueStatics>> _statics = new(() =>
#if NETFRAMEWORK
        new ComObject<IPropertyValueStatics>(WinRTUtilities.GetActivationFactory<IPropertyValueStatics>("Windows.Foundation.PropertyValue"))!
#else
        ComObject.GetActivationFactory<IPropertyValueStatics>("Windows.Foundation.PropertyValue")!
#endif
    );

    private string? _name;

    /// <summary>
    /// Gets the maximum number of effect sources supported by this instance.
    /// </summary>
    public uint MaximumSourcesCount { get; } = sourcesCount;

    /// <inheritdoc/>
    public override string? Name { get => _name ?? string.Empty; set => _name = value; } // *must* not be null for IGraphicsEffectD2D1Interop

    /// <summary>
    /// Gets or sets whether the effect may cache intermediate results.
    /// </summary>
    public virtual bool Cached { get; set; }

    /// <summary>
    /// Gets or sets the precision used for effect buffers.
    /// </summary>
    public virtual D2D1_BUFFER_PRECISION Precision { get; set; }

    /// <summary>
    /// Gets the list of sources connected to this effect, up to <see cref="MaximumSourcesCount"/>.
    /// </summary>
    public IList<IGraphicsEffectSource?> Sources => _sources;

    /// <summary>
    /// Gets the CLSID of the effect, taken from the <see cref="GuidAttribute"/> on the concrete type.
    /// </summary>
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

#if NETFRAMEWORK
    /// <summary>
    /// Returns this instance as an <see cref="IGraphicsEffect"/> for .NET Framework interop.
    /// </summary>
    public IGraphicsEffect GetIGraphicsEffect() => this;
#endif

    /// <summary>
    /// Gets a source at the specified <paramref name="index"/> or <see langword="null"/> if not set.
    /// </summary>
    /// <param name="index">The zero-based source index.</param>
    /// <returns>The source at the index, or <see langword="null"/> if not set.</returns>
    protected virtual IGraphicsEffectSource? GetSource(int index)
    {
        if (index >= MaximumSourcesCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index >= _sources.Count)
            return null;

        return _sources[index];
    }

    /// <summary>
    /// Sets a source at the specified <paramref name="index"/>. The <paramref name="effect"/> may be <see langword="null"/>.
    /// </summary>
    /// <param name="index">The zero-based source index.</param>
    /// <param name="effect">The source to set (may be <see langword="null"/>).</param>
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

        /// <summary>
        /// Creates a WinRT <c>IPropertyValue</c> pointer representing the current value of <paramref name="effect"/>.<br/>
        /// Handles primitive, enum, struct, vector, matrix, and inspectable types.
        /// </summary>
        /// <param name="effect">The effect instance providing the value.</param>
        /// <param name="ptr">On success, receives the allocated COM pointer.</param>
        /// <returns>HRESULT indicating success or failure.</returns>
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

#if NETFRAMEWORK
                    if (value is IInspectable inspectable)
#else
                    if (value is Interop.IInspectable inspectable)
#endif
                        return statics.Object.CreateInspectable(inspectable, out ptr);
                    break;
            }

            ptr = 0;
            Application.Trace(this + " " + this + " E_NOTIMPL");
            return WiceCommons.E_NOTIMPL;
        }
    }

    private static List<PropDef> GetPropDefs(
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] 
#endif
        Type type)
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

    /// <summary>
    /// Gets the effect CLSID for D2D interop.
    /// </summary>
    /// <param name="id">Receives the effect class identifier.</param>
    /// <returns><see cref="WiceCommons.S_OK"/> on success.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetEffectId(out Guid id)
    {
        id = Clsid;
        return WiceCommons.S_OK;
    }

    /// <summary>
    /// Resolves a property name to its index and mapping semantics for D2D.
    /// </summary>
    /// <param name="name">The property name (HSTRING pointer).</param>
    /// <param name="index">Receives the property index.</param>
    /// <param name="mapping">Receives the mapping semantics.</param>
    /// <returns>S_OK if found; otherwise E_INVALIDARG.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetNamedPropertyMapping(PWSTR name, out uint index, out GRAPHICS_EFFECT_PROPERTY_MAPPING mapping)
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
            return WiceCommons.E_INVALIDARG;
        }

        index = (uint)def.Index;
        mapping = def.Mapping;
        return WiceCommons.S_OK;
    }

    /// <summary>
    /// Gets the count of exposed properties for D2D.
    /// </summary>
    /// <param name="count">Receives the property count.</param>
    /// <returns><see cref="WiceCommons.S_OK"/> on success.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetPropertyCount(out uint count)
    {
        var defs = GetPropDefs(GetType());
        count = (uint)defs.Count;
        //Application.Trace(this + " count:" + count);
        return WiceCommons.S_OK;
    }

    /// <summary>
    /// Gets the property value for a given <paramref name="index"/> as a WinRT <c>IPropertyValue</c> pointer.
    /// </summary>
    /// <param name="index">The zero-based property index.</param>
    /// <param name="value">Receives the COM pointer to the boxed property value.</param>
    /// <returns>S_OK on success; E_BOUNDS for invalid index; otherwise E_FAIL.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetProperty(uint index, out nint value)
    {
        try
        {
            //Application.Trace(this + " index:" + index);
            var defs = GetPropDefs(GetType());
            if (index >= defs.Count)
            {
                value = 0;
                Application.Trace(this + " index:" + index + " E_BOUNDS");
                return WiceCommons.E_BOUNDS;
            }

            return defs[(int)index].GetPropertyValue(this, out value);
        }
        catch
        {
            Application.Trace(this + " index:" + index + " E_FAIL");
            value = 0;
            return WiceCommons.E_FAIL;
        }
    }

    /// <summary>
    /// Gets a source for D2D at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based source index.</param>
    /// <param name="source">Receives the source or <see langword="null"/>.</param>
    /// <returns>S_OK on success; E_BOUNDS when out of range.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetSource(uint index, out IGraphicsEffectSource? source)
    {
        //Application.Trace(this + " index:" + index);
        if (index >= MaximumSourcesCount || index >= _sources.Count)
        {
            source = null;
            Application.Trace(this + " index:" + index + " E_BOUNDS");
            return WiceCommons.E_BOUNDS;
        }

        source = _sources[(int)index];
        return WiceCommons.S_OK;
    }

    /// <summary>
    /// Gets the number of sources exposed to D2D.
    /// </summary>
    /// <param name="count">Receives the number of sources.</param>
    /// <returns><see cref="WiceCommons.S_OK"/> on success.</returns>
    HRESULT IGraphicsEffectD2D1Interop.GetSourceCount(out uint count)
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
        return WiceCommons.S_OK;
    }
}
