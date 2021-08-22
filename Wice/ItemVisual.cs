using System;
using System.ComponentModel;

namespace Wice
{
    public class ItemVisual : Box, IOneChildParent, IFocusableParent, ISelectable
    {
        public static VisualProperty IsSelectedProperty = VisualProperty.Add(typeof(ItemVisual), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);

        public event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;

        private INotifyPropertyChanged _notifyPropertyChanged;

        public ItemVisual()
        {
            RaiseIsSelectedChanged = true;
        }

        bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }
        protected virtual bool RaiseIsSelectedChanged { get; set; }

        [Browsable(false)]
        public virtual DataBinder DataBinder { get; set; }

        [Category(CategoryBehavior)]
        public bool IsSelected { get => (bool)GetPropertyValue(IsSelectedProperty); set => SetPropertyValue(IsSelectedProperty, value); }

        Visual IFocusableParent.FocusableVisual => Child;
        Type IFocusableParent.FocusVisualShapeType => null;
        float? IFocusableParent.FocusOffset => null;

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == IsSelectedProperty)
            {
                if (RaiseIsSelectedChanged)
                {
                    OnIsSelectedChanged(this, new ValueEventArgs<bool>((bool)value));
                }
            }
            else if (property == DataProperty)
            {
                if (_notifyPropertyChanged != null)
                {
                    _notifyPropertyChanged.PropertyChanged -= OnDataPropertyChanged;
                    _notifyPropertyChanged = null;
                }

                _notifyPropertyChanged = value as INotifyPropertyChanged;
                if (_notifyPropertyChanged != null)
                {
                    _notifyPropertyChanged.PropertyChanged += OnDataPropertyChanged;
                }
            }

            return true;
        }

        private void OnDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vb = DataBinder?.DataItemVisualBinder;
            if (vb != null)
            {
                var ctx = new DataBindContext(_notifyPropertyChanged);
                ctx.DataVisual = Child;
                vb(ctx);
            }
        }

        protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Parent?.IsEnabled == false)
                return;

            Application.Trace("ItemVisual");
            //e.Handled = true;
            IsSelected = !IsSelected;
            Child?.Focus();
            base.OnMouseButtonDown(sender, e);
        }
    }
}
