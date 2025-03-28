namespace Wice.PropertyGrid;

public partial class PropertyGridCategorySource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject
{
    public PropertyGridCategorySource(PropertyGrid<T> grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        Grid = grid;
        AddCategories();
    }

    public PropertyGrid<T> Grid { get; }
    public virtual ObservableCollection<PropertyGridCategory<T>> Categories { get; } = [];

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

    protected virtual PropertyGridCategory<T> CreateCategory() => new(this);
}
