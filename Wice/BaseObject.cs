using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using DirectN;
using Wice.Utilities;
using ZeroDep;

namespace Wice
{
    public abstract class BaseObject : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, INotifyDataErrorInfo, IPropertyOwner
    {
        public const string CategoryBase = "Base";
        public const string CategoryLive = "Live"; // some computed

        private static int _id;
        private static readonly ConcurrentDictionary<int, BaseObject> _objectsById = new ConcurrentDictionary<int, BaseObject>();

        public static BaseObject GetById(int id)
        {
            _objectsById.TryGetValue(id, out var value);
            return value;
        }

#if DEBUG
        public class Change
        {
            public static BaseObjectProperty InvalidateMarker = BaseObjectProperty.Add<VisualPropertyInvalidateModes>(typeof(Window), "Invalidate");

            private static readonly ConcurrentList<Change> _changes = new ConcurrentList<Change>();

            public Change(Type type, int objectId, BaseObjectProperty property, object value)
            {
                Index = _changes.Count;
                Type = type;
                ObjectId = objectId;
                Property = property;
                Value = value;
                _changes.Add(this);
            }

            public int Index;
            public Type Type;
            public int ObjectId;
            public BaseObjectProperty Property;
            public object Value;

            public override string ToString()
            {
                if (Property == InvalidateMarker)
                    return Index + "/" + ObjectId + " | " + nameof(InvalidateMarker);

                return Index + "/" + ObjectId + " | " + Type + " | " + Property + " | " + Value;
            }

            public bool IsSameAs(Change other)
            {
                if (other == null)
                    return false;

                return Type == other.Type && Property == other.Property;
            }

            public static IReadOnlyList<Change> Changes => _changes;
        }
#endif

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        private string _name;
        private Lazy<string> _fullName;

        protected BaseObject()
        {
#if DEBUG
            _name = GetType().Name;
#endif
            _fullName = new Lazy<string>(GetFullName, true);
            Values = new ConcurrentDictionary<int, object>();
            Id = Interlocked.Increment(ref _id);
            _objectsById.AddOrUpdate(Id, this, (k, o) => this);
        }

        [Browsable(false)]
        [Json(IgnoreWhenSerializing = true, IgnoreWhenDeserializing = true)]
        public int Id { get; }

        [Browsable(false)]
        [Json(IgnoreWhenSerializing = true, IgnoreWhenDeserializing = true)]
        public string FullName => _fullName.Value;

        [Category(CategoryBase)]
        public virtual string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;
                _fullName = new Lazy<string>(GetFullName, true);
                OnPropertyChanged();
            }
        }

        protected virtual string GetFullName() => Name;

        protected ConcurrentDictionary<int, object> Values { get; }
        protected virtual bool RaiseOnPropertyChanging { get; set; }
        protected virtual bool RaiseOnPropertyChanged { get; set; }
        protected virtual bool RaiseOnErrorsChanged { get; set; }

#if DEBUG
        public override string ToString() => Name.Nullify() ?? GetType().Name;
#else
        public override string ToString() => Name;
#endif

        protected virtual void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) => ErrorsChanged?.Invoke(sender, e);
        protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs e) => PropertyChanging?.Invoke(sender, e);
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);
        protected void OnErrorsChanged([CallerMemberName] string propertyName = null) => OnErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        protected void OnPropertyChanging([CallerMemberName] string propertyName = null) => OnPropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        protected virtual IEnumerable GetErrors(string propertyName) { yield break; }
        protected string Error => GetError(null);
        protected virtual string GetError(string propertyName)
        {
            var errors = GetErrors(propertyName);
            if (errors == null)
                return null;

            var error = string.Join(Environment.NewLine, errors.Cast<object>().Select(e => string.Format("{0}", e)));
            return !string.IsNullOrEmpty(error) ? error : null;
        }

        protected virtual bool AreValuesEqual(object value1, object value2)
        {
            if (value1 == null)
                return value2 == null;

            if (value2 == null)
                return false;

            return value1.Equals(value2);
        }

        private sealed class ObjectComparer : IEqualityComparer<object>
        {
            private readonly BaseObject _bo;

            public ObjectComparer(BaseObject bo)
            {
                _bo = bo;
            }

            public new bool Equals(object x, object y) => _bo.AreValuesEqual(x, y);
            public int GetHashCode(object obj) => (obj?.GetHashCode()).GetValueOrDefault();
        }

        protected virtual bool AreErrorsEqual(IEnumerable errors1, IEnumerable errors2)
        {
            if (errors1 == null && errors2 == null)
                return true;

            var dic = new Dictionary<object, int>(new ObjectComparer(this));
            IEnumerable<object> left = errors1 != null ? errors1.Cast<object>() : Enumerable.Empty<object>();
            foreach (var obj in left)
            {
                if (dic.ContainsKey(obj))
                {
                    dic[obj]++;
                }
                else
                {
                    dic.Add(obj, 1);
                }
            }

            if (errors2 == null)
                return dic.Count == 0;

            foreach (var obj in errors2)
            {
                if (dic.ContainsKey(obj))
                {
                    dic[obj]--;
                }
                else
                    return false;
            }
            return dic.Values.All(c => c == 0);
        }

        protected virtual bool MergeProperties(BaseObject source, BaseObjectSetOptions options = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var changed = false;
            foreach (var kv in source.Values)
            {
                var prop = BaseObjectProperty.GetById(kv.Key);
                if (prop == null)
                    continue;

                if (SetPropertyValue(prop, kv.Value, options))
                {
                    changed = true;
                }
            }

            if (source.Name != null && Name != source.Name)
            {
                Name = source.Name;
                changed = true;
            }
            return changed;
        }

        protected virtual bool IsPropertyValueSet(BaseObjectProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            property = BaseObjectProperty.GetFinal(GetType(), property);
            return Values.ContainsKey(property.Id);
        }

        protected virtual object GetPropertyValue(BaseObjectProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (!TryGetPropertyValue(property, out var value))
                return property.DefaultValue;

            return value;
        }

        protected virtual bool TryGetPropertyValue(BaseObjectProperty property, out object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            property = BaseObjectProperty.GetFinal(GetType(), property);
            return Values.TryGetValue(property.Id, out value);
        }

        protected bool ResetPropertyValue(BaseObjectProperty property) => ResetPropertyValue(property, out _);
        protected virtual bool ResetPropertyValue(BaseObjectProperty property, out object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            property = BaseObjectProperty.GetFinal(GetType(), property);
            return Values.TryRemove(property.Id, out value);
        }

        protected virtual bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            property = BaseObjectProperty.GetFinal(GetType(), property);
            if (property.Convert != null)
            {
                value = property.Convert(this, value);
            }

            IEnumerable oldErrors = null;
            var raiseOnErrorsChanged = RaiseOnErrorsChanged;
            if (options?.DontRaiseOnErrorsChanged == true)
            {
                raiseOnErrorsChanged = false;
            }

            if (raiseOnErrorsChanged)
            {
                oldErrors = GetErrors(property.Name);
            }

            var raiseOnPropertyChanged = RaiseOnPropertyChanged;
            var forceRaiseOnPropertyChanged = options?.ForceRaiseOnPropertyChanged == true;
            if (options?.DontRaiseOnPropertyChanged == true)
            {
                raiseOnPropertyChanged = false;
                forceRaiseOnPropertyChanged = false;
            }

            var changed = true;
            object old = null;
            var finalProp = Values.AddOrUpdate(property.Id, value, (k, o) =>
            {
                old = o;
                var testEquality = !(options?.DontTestValuesForEquality == true);
                if (testEquality && AreValuesEqual(value, o))
                {
                    changed = false;
                    return o;
                }

                if (property.Changing != null)
                {
                    if (!property.Changing(this, value, o))
                    {
                        changed = false;
                        return o;
                    }
                }

                var raiseOnPropertyChanging = RaiseOnPropertyChanging;
                if (options?.DontRaiseOnPropertyChanging == true)
                {
                    raiseOnPropertyChanging = false;
                }

                if (raiseOnPropertyChanging)
                {
                    var e = new PropertyChangingEventArgs(property.Name);
                    OnPropertyChanging(this, e);
                }

                return value;
            });

#if DEBUG
            if (changed)
            {
                new Change(GetType(), Id, property, value);
            }
#endif
            if (changed && property.Changed != null)
            {
                property.Changed(this, value, old);
            }

            if ((changed && raiseOnPropertyChanged) || forceRaiseOnPropertyChanged)
            {
                var e = new PropertyChangedEventArgs(property.Name);
                OnPropertyChanged(this, e);

                var otherChanges = property.PropertyNameChanges;
                if (!otherChanges.IsEmpty())
                {
                    foreach (var otherName in otherChanges)
                    {
                        if (!string.IsNullOrWhiteSpace(otherName))
                        {
                            OnPropertyChanged(this, new PropertyChangedEventArgs(otherName));
                        }
                    }
                }

                if (options?.ForceRaiseOnErrorsChanged == true)
                {
                    OnErrorsChanged(this, new DataErrorsChangedEventArgs(property.Name));
                }
                else if (raiseOnErrorsChanged)
                {
                    var newErrors = GetErrors(property.Name);
                    if (!AreErrorsEqual(oldErrors, newErrors))
                    {
                        OnErrorsChanged(this, new DataErrorsChangedEventArgs(property.Name));
                    }
                }
            }

            return changed;
        }

        string IDataErrorInfo.this[string columnName] => GetError(columnName);
        bool INotifyDataErrorInfo.HasErrors => ((IDataErrorInfo)this).Error != null;
        string IDataErrorInfo.Error => Error;
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) => GetErrors(propertyName);
        bool IPropertyOwner.TryGetPropertyValue(BaseObjectProperty property, out object value) => TryGetPropertyValue(property, out value);
        bool IPropertyOwner.SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options) => SetPropertyValue(property, value, options);
        object IPropertyOwner.GetPropertyValue(BaseObjectProperty property) => GetPropertyValue(property);
        bool IPropertyOwner.IsPropertyValueSet(BaseObjectProperty property) => IsPropertyValueSet(property);
        bool IPropertyOwner.ResetPropertyValue(BaseObjectProperty property, out object value) => ResetPropertyValue(property, out value);

        public static T Deserialize<T>(string filePath, JsonOptions options = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (!IOUtilities.FileExists(filePath))
                return default;

            using (var reader = new StreamReader(filePath))
            {
                return Deserialize<T>(reader, options);
            }
        }

        public static T Deserialize<T>(Stream stream, JsonOptions options = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new StreamReader(stream))
            {
                return Deserialize<T>(reader, options);
            }
        }

        public static T Deserialize<T>(TextReader reader, JsonOptions options = null)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return Json.Deserialize<T>(reader, options);
        }

        public virtual void Serialize(TextWriter writer, JsonOptions options = null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            Json.SerializeFormatted(writer, this, options);
        }

        public virtual void Serialize(Stream stream, JsonOptions options = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                Serialize(writer, options);
            }
        }

        public virtual void Serialize(string filePath, JsonOptions options = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            IOUtilities.FileEnsureDirectory(filePath);
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                Serialize(writer, options);
            }
        }
    }
}
