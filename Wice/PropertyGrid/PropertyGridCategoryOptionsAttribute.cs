using System;

namespace Wice.PropertyGrid
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class PropertyGridCategoryOptionsAttribute : Attribute
    {
        public PropertyGridCategoryOptionsAttribute(string name)
        {
            Name = name;
            IsExpanded = true;
        }

        public string Name { get; set; }
        public bool IsExpanded { get; set; }
        public int SortOrder { get; set; }
    }
}
