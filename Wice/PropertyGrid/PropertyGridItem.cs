namespace Wice.PropertyGrid
{
    public class PropertyGridItem : BaseObject
    {
        public static VisualProperty NameProperty = VisualProperty.Add<string>(typeof(PropertyGridItem), nameof(Name), VisualPropertyInvalidateModes.Render, null);
        public static VisualProperty IsZeroProperty = VisualProperty.Add(typeof(PropertyGridItem), nameof(IsZero), VisualPropertyInvalidateModes.Render, false);
        public static VisualProperty IsUnsetProperty = VisualProperty.Add(typeof(PropertyGridItem), nameof(IsUnset), VisualPropertyInvalidateModes.Render, false);
        public static VisualProperty IsCheckedProperty = VisualProperty.Add<bool?>(typeof(PropertyGridItem), nameof(IsChecked), VisualPropertyInvalidateModes.Render);
        public static VisualProperty ValueProperty = VisualProperty.Add<object>(typeof(PropertyGridItem), nameof(Value), VisualPropertyInvalidateModes.Render);

        public PropertyGridItem()
        {
            IsChecked = false;
        }

        public bool IsUnset { get => (bool)GetPropertyValue(IsUnsetProperty); set => SetPropertyValue(IsUnsetProperty, value); }
        public bool IsZero { get => (bool)GetPropertyValue(IsZeroProperty); set => SetPropertyValue(IsZeroProperty, value); }
        public override string Name { get => (string)GetPropertyValue(NameProperty); set => SetPropertyValue(NameProperty, value); }
        public object Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }
        public bool? IsChecked { get => (bool?)GetPropertyValue(IsCheckedProperty); set => SetPropertyValue(IsCheckedProperty, value); }

        public override string ToString() => Name;
    }
}
