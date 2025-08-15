namespace Wice.PropertyGrid;

/// <summary>
/// Represents a logical category grouping for properties displayed by a <see cref="PropertyGrid{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The component type owning the properties. Annotated with
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/> to preserve public properties under trimming/AOT.
/// </typeparam>
/// <remarks>
/// - Stores UI-facing metadata via <see cref="VisualProperty"/> descriptors to participate in the visual invalidation pipeline.
/// - Sorting is performed by descending <see cref="SortOrder"/> and then by <see cref="DisplayName"/> (case-insensitive).
/// - When <see cref="Name"/> is set, category options can be pulled from <c>PropertyGridCategoryOptionsAttribute</c>
///   on the currently selected object and applied (e.g., <see cref="IsExpanded"/>, <see cref="SortOrder"/>).
/// </remarks>
public partial class PropertyGridCategory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject, IComparable, IComparable<PropertyGridCategory<T>>
{
    /// <summary>
    /// Backing dynamic property for <see cref="BaseObject.Name"/> on this type.
    /// Render invalidation is requested when changed.
    /// Default: <see langword="null"/>.
    /// </summary>
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridCategory<T>), nameof(Name), VisualPropertyInvalidateModes.Render, null);

    /// <summary>
    /// Backing dynamic property for <see cref="DisplayName"/>.
    /// Render invalidation is requested when changed.
    /// </summary>
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridCategory<T>), nameof(DisplayName), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Backing dynamic property for <see cref="IsExpanded"/>.
    /// Render invalidation is requested when changed.
    /// Default: <see langword="false"/> (overridden by constructor to <see langword="true"/>).
    /// </summary>
    public static VisualProperty IsExpandedProperty { get; } = VisualProperty.Add(typeof(PropertyGridCategory<T>), nameof(IsExpanded), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Backing dynamic property for <see cref="SortOrder"/>.
    /// Render invalidation is requested when changed.
    /// Default: 0. Higher values sort first.
    /// </summary>
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridCategory<T>), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);

    /// <summary>
    /// Default category name used when no explicit category is provided.
    /// </summary>
    public const string MiscCategoryName = "Misc";

    /// <summary>
    /// Default "main" category name that can be used to highlight primary properties.
    /// </summary>
    public const string MainCategoryName = "Main";

    /// <summary>
    /// Initializes a new category bound to the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The creator/owner providing category context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    public PropertyGridCategory(PropertyGridCategorySource<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        IsExpanded = true;
    }

    /// <summary>
    /// Gets the category source that produced and manages this category.
    /// </summary>
    public PropertyGridCategorySource<T> Source { get; }

    /// <summary>
    /// Gets the set of properties that belong to this category.
    /// </summary>
    public virtual ObservableCollection<PropertyGridProperty<T>> Properties { get; } = [];

    /// <summary>
    /// Gets or sets the user-facing category name.
    /// </summary>
    public string? DisplayName { get => (string?)GetPropertyValue(DisplayNameProperty); set => SetPropertyValue(DisplayNameProperty, value); }

    /// <summary>
    /// Gets or sets the explicit category sort order.
    /// Higher values are sorted before lower values.
    /// </summary>
    public int SortOrder { get => (int)GetPropertyValue(SortOrderProperty)!; set => SetPropertyValue(SortOrderProperty, value); }

    /// <summary>
    /// Gets or sets whether the category is expanded in the UI.
    /// </summary>
    public bool IsExpanded { get => (bool)GetPropertyValue(IsExpandedProperty)!; set => SetPropertyValue(IsExpandedProperty, value); }

    /// <summary>
    /// Gets or sets the internal name of the category.
    /// </summary>
    /// <remarks>
    /// - Get returns the same value as <see cref="DisplayName"/> (for convenience).
    /// - Set updates <see cref="DisplayName"/> and, if the grid has a selected object, applies
    ///   any <c>PropertyGridCategoryOptionsAttribute</c> that matches this name by:
    ///   - Setting <see cref="IsExpanded"/> from the attribute.
    ///   - Applying <see cref="SortOrder"/> when non-zero in the attribute.
    /// </remarks>
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

    /// <summary>
    /// Returns the category's <see cref="Name"/> or an empty string.
    /// </summary>
    public override string ToString() => Name ?? string.Empty;

    /// <summary>
    /// Compares this instance with another for ordering (non-generic).
    /// </summary>
    /// <param name="obj">Another instance.</param>
    /// <returns>An integer indicating relative order.</returns>
    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridCategory<T>);

    /// <summary>
    /// Compares this instance with another for ordering.
    /// </summary>
    /// <param name="other">The other category.</param>
    /// <returns>
    /// Negative if this precedes <paramref name="other"/>, positive if it follows, zero if equal.
    /// Ordering: by descending <see cref="SortOrder"/>, then by <see cref="DisplayName"/> case-insensitively.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <see langword="null"/>.</exception>
    public virtual int CompareTo(PropertyGridCategory<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var cmp = -SortOrder.CompareTo(other.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
