using System;
using System.ComponentModel;
using Wice.Utilities;
using static Wice.EnumListBox;

namespace Wice
{
    public class EnumListBox : ListBox, IValueable, IBindList
    {
        public static VisualProperty ValueProperty = VisualProperty.Add<object>(typeof(EnumListBox), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: EnumTypeCheck);

        public event EventHandler<ValueEventArgs> ValueChanged;

        internal interface IBindList
        {
            Type Type { get; set; }
            bool NeedBind { get; set; }
        }

        internal static object EnumTypeCheck(BaseObject obj, object value)
        {
            if (value != null)
            {
                var type = value.GetType();
                if (!type.IsEnum)
                    throw new ArgumentException(null, nameof(value));

                var elb = (IBindList)obj;
                if (elb.Type != type)
                {
                    elb.Type = type;
                    elb.NeedBind = true;
                }
            }

            return value;
        }

        [Category(CategoryBehavior)]
        public object Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

        bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
        object IValueable.Value => Value;
        bool IValueable.TrySetValue(object value)
        {
            if (value == null)
                return false;

            var type = value.GetType();
            if (!type.IsEnum)
                return false;

            Value = value;
            return true;
        }

        Type IBindList.Type { get; set; }
        bool IBindList.NeedBind { get; set; }

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
            if (SelectedItem?.Data is EnumBitValue value)
            {
                Value = value.Value;
                return;
            }
        }

        protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == ValueProperty)
            {
                OnValueChanged(this, new ValueEventArgs(value));
                var ibl = (IBindList)this;
                if (ibl.NeedBind)
                {
                    DataSource = EnumDataSource.FromValue(Value);
                    ibl.NeedBind = false;
                }
            }
            return true;
        }
    }
}
