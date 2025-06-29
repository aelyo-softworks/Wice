namespace Wice.PropertyGrid;

public class PropertyGridCategorySource : BaseObject
{
    public PropertyGridCategorySource(PropertyGrid grid)
    {
        if (grid == null)
            throw new ArgumentNullException(nameof(grid));

        Grid = grid;
        AddCategories();
    }

    public PropertyGrid Grid { get; }
    public virtual ObservableCollection<PropertyGridCategory> Categories { get; } = [];

    public virtual void AddCategories()
    {
        Categories.Clear();
        var source = Grid.Source;
        if (source == null)
            return;

        if (Grid.GroupByCategory)
        {
            var dic = new Dictionary<string, List<PropertyGridProperty>>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in source.Properties)
            {
                var name = (prop.Category.Nullify() ?? Grid.UnspecifiedCategoryName) ?? PropertyGridCategory.MiscCategoryName;
                if (!dic.TryGetValue(name, out var props))
                {
                    props = [];
                    dic.Add(name, props);
                }

                props.Add(prop);
            }

            var list = new List<PropertyGridCategory>();
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
            var cat = new PropertyGridCategory(this)
            {
                Name = "All"
            };
            var list = new List<PropertyGridProperty>();
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

    protected virtual PropertyGridCategory CreateCategory() => new(this);
}
