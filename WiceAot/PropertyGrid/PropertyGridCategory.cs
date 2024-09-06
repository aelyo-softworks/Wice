namespace Wice.PropertyGrid;

public partial class PropertyGridCategory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject, IComparable, IComparable<PropertyGridCategory<T>>
{
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridCategory<T>), nameof(Name), VisualPropertyInvalidateModes.Render, null);
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridCategory<T>), nameof(DisplayName), VisualPropertyInvalidateModes.Render);
    public static VisualProperty IsExpandedProperty { get; } = VisualProperty.Add(typeof(PropertyGridCategory<T>), nameof(IsExpanded), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridCategory<T>), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);

    public const string MiscCategoryName = "Misc";
    public const string MainCategoryName = "Main";

    public PropertyGridCategory(PropertyGridCategorySource<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        IsExpanded = true;
    }

    public PropertyGridCategorySource<T> Source { get; }
    public virtual ObservableCollection<PropertyGridProperty<T>> Properties { get; } = [];
    public string? DisplayName { get => (string?)GetPropertyValue(DisplayNameProperty); set => SetPropertyValue(DisplayNameProperty, value); }
    public int SortOrder { get => (int)GetPropertyValue(SortOrderProperty)!; set => SetPropertyValue(SortOrderProperty, value); }
    public bool IsExpanded { get => (bool)GetPropertyValue(IsExpandedProperty)!; set => SetPropertyValue(IsExpandedProperty, value); }

    public override string? Name
    {
        get => (string?)GetPropertyValue(DisplayNameProperty);
        set
        {
            if (SetPropertyValue(DisplayNameProperty, value))
            {
                var obj = Source.Grid.SelectedObject;
                if (obj != null)
                {
                    var options = obj.GetType().GetCustomAttributes<PropertyGridCategoryOptionsAttribute>().FirstOrDefault(c => c.Name.EqualsIgnoreCase(Name));
                    if (options != null)
                    {
                        IsExpanded = options.IsExpanded;
                        if (options.SortOrder != 0)
                        {
                            SortOrder = options.SortOrder;
                        }
                    }
                }
            }
        }
    }

    public override string ToString() => Name ?? string.Empty;

    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridCategory<T>);
    public virtual int CompareTo(PropertyGridCategory<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var cmp = -SortOrder.CompareTo(other.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
