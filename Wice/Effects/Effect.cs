using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DirectN;
using Windows.Foundation;
#if NET
using IGraphicsEffectSource = DirectN.IGraphicsEffectSourceWinRT;
using IGraphicsEffect = DirectN.IGraphicsEffectWinRT;
#else
using IGraphicsEffectSource = Windows.Graphics.Effects.IGraphicsEffectSource;
using IGraphicsEffect = Windows.Graphics.Effects.IGraphicsEffect;
#endif

namespace Wice.Effects
{
    // if you find this code is an absolute mess (because of combined .NET Framework & .NET 5+ support), you're 100% right and you should talk to Microsoft because they broke everything
    // https://github.com/microsoft/CsWinRT/issues/302
    // https://github.com/microsoft/CsWinRT/issues/799
    // etc.
    public abstract class Effect : BaseObject, IGraphicsEffect, IGraphicsEffectD2D1Interop, IGraphicsEffectSource
    {
        private static readonly ConcurrentDictionary<Type, List<PropDef>> _properties = new ConcurrentDictionary<Type, List<PropDef>>();
        private readonly List<IGraphicsEffectSource> _sources;
        private readonly Lazy<IPropertyValueStatics> _statics;
        private string _name;

        protected Effect(int sourcesCount = 0)
        {
            if (sourcesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(sourcesCount));

            MaximumSourcesCount = sourcesCount;
            _sources = new List<IGraphicsEffectSource>();
            _statics = new Lazy<IPropertyValueStatics>(() => WinRTUtilities.GetActivationFactory<IPropertyValueStatics>("Windows.Foundation.PropertyValue"));
        }

        public int MaximumSourcesCount { get; }
        public override string Name { get => _name ?? string.Empty; set => _name = value; } // *must* not be null for IGraphicsEffectD2D1Interop
        public virtual bool Cached { get; set; }
        public virtual D2D1_BUFFER_PRECISION Precision { get; set; }
        public IList<IGraphicsEffectSource> Sources => _sources;

#if NET
        HRESULT IInspectable.GetIids(out int iidCount, out IntPtr iids)
        {
            iidCount = 0;
            iids = IntPtr.Zero;
            return HRESULTS.S_OK;
        }

        HRESULT IInspectable.GetRuntimeClassName(out string className)
        {
            className = null;
            return HRESULTS.S_OK;
        }

        HRESULT IInspectable.GetTrustLevel(out TrustLevel trustLevel)
        {
            trustLevel = TrustLevel.FullTrust;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectSource.GetIids(out int iidCount, out IntPtr iids)
        {
            iidCount = 0;
            iids = IntPtr.Zero;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectSource.GetRuntimeClassName(out string className)
        {
            className = null;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectSource.GetTrustLevel(out TrustLevel trustLevel)
        {
            trustLevel = TrustLevel.FullTrust;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffect.GetIids(out int iidCount, out IntPtr iids)
        {
            iidCount = 0;
            iids = IntPtr.Zero;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffect.GetRuntimeClassName(out string className)
        {
            className = null;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffect.GetTrustLevel(out TrustLevel trustLevel)
        {
            trustLevel = TrustLevel.FullTrust;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffect.get_Name(out string name)
        {
            name = Name;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffect.put_Name(string name)
        {
            Name = name;
            return HRESULTS.S_OK;
        }

#endif

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

        public Windows.Graphics.Effects.IGraphicsEffect GetIGraphicsEffect()
        {
#if NET
            var unk = Marshal.GetIUnknownForObject(this);
            return WinRT.MarshalInspectable<Windows.Graphics.Effects.IGraphicsEffect>.FromAbi(unk);

#else
            return this;
#endif
        }

        protected virtual IGraphicsEffectSource GetSource(int index)
        {
            if (index >= MaximumSourcesCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index >= _sources.Count)
                return null;

            return _sources[index];
        }

        protected virtual void SetSource(int index, IGraphicsEffectSource effect)
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

        private class PropDef
        {
            public PropertyInfo Property;
            public int Index;
            public GRAPHICS_EFFECT_PROPERTY_MAPPING Mapping;

            public override string ToString() => "#" + Index + " " + Property?.Name;

            public HRESULT GetPropertyValue(Effect effect, out IntPtr ptr)
            {
                var statics = effect._statics.Value;
                var value = Property.GetValue(effect);
                if (value == null || Convert.IsDBNull(value))
                    return statics.CreateEmpty(out ptr);

                // enums are passed as UInt32
                var type = value.GetType();
                if (type.IsEnum)
                    return statics.CreateUInt32((uint)(int)value, out ptr);

                var tc = Type.GetTypeCode(type);
                switch (tc)
                {
                    case TypeCode.Boolean:
                        return statics.CreateBoolean((bool)value, out ptr);

                    case TypeCode.Byte:
                        return statics.CreateUInt8((byte)value, out ptr);

                    case TypeCode.Char:
                        return statics.CreateChar16((char)value, out ptr);

                    case TypeCode.DateTime:
                        return statics.CreateDateTime((DateTime)value, out ptr);

                    case TypeCode.Double:
                        return statics.CreateDouble((double)value, out ptr);

                    case TypeCode.Empty:
                        return statics.CreateEmpty(out ptr);

                    case TypeCode.Int16:
                        return statics.CreateInt16((short)value, out ptr);

                    case TypeCode.Int32:
                        return statics.CreateInt32((int)value, out ptr);

                    case TypeCode.Int64:
                        return statics.CreateInt64((long)value, out ptr);

                    case TypeCode.SByte:
                        return statics.CreateUInt8((byte)(sbyte)value, out ptr);

                    case TypeCode.Single:
                        return statics.CreateSingle((float)value, out ptr);

                    case TypeCode.String:
                        return statics.CreateString((string)value, out ptr);

                    case TypeCode.UInt16:
                        return statics.CreateUInt16((ushort)value, out ptr);

                    case TypeCode.UInt32:
                        return statics.CreateUInt32((uint)value, out ptr);

                    case TypeCode.UInt64:
                        return statics.CreateUInt64((ulong)value, out ptr);

                    default:
                        if (type == typeof(Guid))
                            return statics.CreateGuid((Guid)value, out ptr);

                        if (type == typeof(Point))
                            return statics.CreatePoint((Point)value, out ptr);

                        if (type == typeof(Size))
                            return statics.CreateSize((Size)value, out ptr);

                        if (type == typeof(Rect))
                            return statics.CreateRect((Rect)value, out ptr);

                        if (type == typeof(TimeSpan))
                            return statics.CreateTimeSpan((TimeSpan)value, out ptr);

                        if (type == typeof(D2D_VECTOR_4F))
                            return statics.CreateSingleArray(((D2D_VECTOR_4F)value).ToArray(), out ptr);

                        if (type == typeof(D2D_VECTOR_2F))
                            return statics.CreateSingleArray(((D2D_VECTOR_2F)value).ToArray(), out ptr);

                        if (type == typeof(_D3DCOLORVALUE))
                            return statics.CreateSingleArray(((_D3DCOLORVALUE)value).ToArray(), out ptr);

                        if (type == typeof(D2D_MATRIX_4X4_F))
                            return statics.CreateSingleArray(((D2D_MATRIX_4X4_F)value).ToArray(), out ptr);

                        if (type == typeof(D2D_MATRIX_5X4_F))
                            return statics.CreateSingleArray(((D2D_MATRIX_5X4_F)value).ToArray(), out ptr);

                        if (value is float[] floats)
                            return statics.CreateSingleArray(floats, out ptr);

                        if (value is IInspectable inspectable)
                            return statics.CreateInspectable(inspectable, out ptr);

                        break;
                }

                ptr = IntPtr.Zero;
                return HRESULTS.E_NOTIMPL;
            }
        }

        private static List<PropDef> GetPropDefs(Type type)
        {
            if (!_properties.TryGetValue(type, out var list))
            {
                list = new List<PropDef>();
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
                {
                    var effectProp = BaseObjectProperty.GetProperties(type).OfType<EffectProperty>().FirstOrDefault(p => p.Name == prop.Name);
                    if (effectProp == null)
                        continue;

                    var def = new PropDef();
                    def.Property = prop;
                    def.Index = effectProp.Index;
                    def.Mapping = effectProp.Mapping;
                    list.Add(def);
                }
                list = _properties.AddOrUpdate(type, list, (k, o) => o);
            }
            return list;
        }

        HRESULT IGraphicsEffectD2D1Interop.GetEffectId(out Guid id)
        {
            id = Clsid;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectD2D1Interop.GetNamedPropertyMapping(string name, out uint index, out GRAPHICS_EFFECT_PROPERTY_MAPPING mapping)
        {
            //Application.Trace(this + "name:" + name);

            var defs = GetPropDefs(GetType());
            var def = defs.FirstOrDefault(p => p.Property.Name.EqualsIgnoreCase(name));
            if (def == null)
            {
                index = 0;
                mapping = 0;
                return HRESULTS.E_INVALIDARG;
            }

            index = (uint)def.Index;
            mapping = def.Mapping;
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectD2D1Interop.GetPropertyCount(out uint count)
        {
            var defs = GetPropDefs(GetType());
            count = (uint)defs.Count;
            //Application.Trace(this + " count:" + count);
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectD2D1Interop.GetProperty(uint index, out IntPtr value)
        {
            try
            {
                //Application.Trace(this + " index:" + index);
                var defs = GetPropDefs(GetType());
                if (index >= defs.Count)
                {
                    value = IntPtr.Zero;
                    //Application.Trace(this + " E_BOUNDS");
                    return HRESULTS.E_BOUNDS;
                }

                return defs[(int)index].GetPropertyValue(this, out value);
            }
            catch
            {
                value = IntPtr.Zero;
                return HRESULTS.E_FAIL;
            }
        }

        HRESULT IGraphicsEffectD2D1Interop.GetSource(uint index, out IGraphicsEffectSource source)
        {
            //Application.Trace(this + " index:" + index);
            if (index >= MaximumSourcesCount || index >= _sources.Count)
            {
                source = null;
                return HRESULTS.E_BOUNDS;
            }

            source = _sources[(int)index];
            return HRESULTS.S_OK;
        }

        HRESULT IGraphicsEffectD2D1Interop.GetSourceCount(out uint count)
        {
            if (MaximumSourcesCount == int.MaxValue)
            {
                count = (uint)_sources.Count;
            }
            else
            {
                count = (uint)MaximumSourcesCount;
            }
            //Application.Trace(this + " count:" + count);
            return HRESULTS.S_OK;
        }
    }
}
