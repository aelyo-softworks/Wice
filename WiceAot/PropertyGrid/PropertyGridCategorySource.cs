namespace Wice.PropertyGrid;

/// <summary>
/// Builds and maintains the category list for a <see cref="PropertyGrid{T}"/> based on the grid's source.
/// </summary>
/// <typeparam name="T">
/// The component type displayed by the property grid. Marked with
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/> to preserve public properties under trimming/AOT.
/// </typeparam>
/// <remarks>
/// Behavior:
/// <list type="bullet">
/// <item>
/// <term>Grouping by category</term>
/// <description>
/// When <see cref="PropertyGrid{T}.GroupByCategory"/> is <c>true</c>, properties are grouped by their
/// <c>Category</c> metadata (falling back to <see cref="PropertyGrid{T}.UnspecifiedCategoryName"/> and then
/// <see cref="PropertyGridCategory{T}.MiscCategoryName"/>). Categories and properties are sorted before being added.
/// </description>
/// </item>
/// <item>
/// <term>Single category</term>
/// <description>
/// When grouping is disabled, all properties are placed under a single "All" category, and each property's
/// <c>SortOrder</c> is normalized to <c>0</c> so sorting is purely by name/compare logic.
/// </description>
/// </item>
/// </list>
/// </remarks>
/// <seealso cref="PropertyGrid{T}"/>
/// <seealso cref="PropertyGridCategory{T}"/>
/// <seealso cref="PropertyGridProperty{T}"/>
public partial class PropertyGridCategorySource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject
{
    /// <summary>
    /// Initializes a new <see cref="PropertyGridCategorySource{T}"/> bound to the specified grid
    /// and immediately populates <see cref="Categories"/>.
    /// </summary>
    /// <param name="grid">The owning property grid.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="grid"/> is null.</exception>
    public PropertyGridCategorySource(PropertyGrid<T> grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        Grid = grid;
        AddCategories();
    }

    /// <summary>
    /// Gets the owning property grid.
    /// </summary>
    /// <value>The grid to which this category source belongs. Never null after construction.</value>
    public PropertyGrid<T> Grid { get; }

    /// <summary>
    /// Gets the current set of categories displayed by the grid.
    /// </summary>
    /// <remarks>
    /// The collection is rebuilt by <see cref="AddCategories"/> whenever the source or grouping mode changes.
    /// Categories are sorted using <see cref="PropertyGridCategory{T}.CompareTo(PropertyGridCategory{T})"/>.
    /// </remarks>
    public virtual ObservableCollection<PropertyGridCategory<T>> Categories { get; } = [];

    /// <summary>
    /// Rebuilds <see cref="Categories"/> from the grid's current <c>Source</c>.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// <list type="number">
    /// <item>Clear existing <see cref="Categories"/>.</item>
    /// <item>Return if the grid's source is null.</item>
    /// <item>If <see cref="PropertyGrid{T}.GroupByCategory"/>:
    /// <list type="bullet">
    /// <item>Group properties by normalized category name (see fallback chain in code).</item>
    /// <item>Create one <see cref="PropertyGridCategory{T}"/> per group, set <c>Name</c> and a decamelized <c>DisplayName</c>.</item>
    /// <item>Sort each group's properties and add them to the category.</item>
    /// <item>Sort categories and add them to <see cref="Categories"/>.</item>
    /// </list>
    /// </item>
    /// <item>Else:
    /// <list type="bullet">
    /// <item>Create an "All" category.</item>
    /// <item>Normalize each property's <c>SortOrder</c> to 0 and add to a list.</item>
    /// <item>Sort the list, assign to the category, and add it to <see cref="Categories"/>.</item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    public virtual void AddCategories()
    {
        Categories.Clear();
        var source = Grid.Source;
        if (source == null)
            return;

        if (Grid.GroupByCategory)
        {
            var dic = new Dictionary<string, List<PropertyGridProperty<T>>>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in source.Properties)
            {
                var name = (prop.Category.Nullify() ?? Grid.UnspecifiedCategoryName) ?? PropertyGridCategory<T>.MiscCategoryName;
                if (!dic.TryGetValue(name, out var props))
                {
                    props = [];
                    dic.Add(name, props);
                }

                props.Add(prop);
            }

            var list = new List<PropertyGridCategory<T>>();
            foreach (var kv in dic)
            {
                var cat = CreateCategory();
                cat.Name = kv.Key;
                cat.DisplayName = Conversions.Decamelize(cat.Name);
                list.Add(cat);

                kv.Value.Sort();
                cat.Properties.AddRange(kv.Value);
            }

            list.Sort();
            Categories.AddRange(list);
        }
        else
        {
            var cat = new PropertyGridCategory<T>(this)
            {
                Name = "All"
            };
            var list = new List<PropertyGridProperty<T>>();
            foreach (var prop in source.Properties)
            {
                // in this mode, we group by name
                prop.SortOrder = 0;
                list.Add(prop);
            }

            list.Sort();
            cat.Properties.AddRange(list);
            Categories.Add(cat);
        }
    }

    /// <summary>
    /// Factory for creating a new category associated with this source.
    /// </summary>
    /// <returns>A new <see cref="PropertyGridCategory{T}"/> instance.</returns>
    protected virtual PropertyGridCategory<T> CreateCategory() => new(this);
}
