namespace Wice.PropertyGrid;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PropertyGridCategoryOptionsAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
    public bool IsExpanded { get; set; } = true;
    public int SortOrder { get; set; }
}
