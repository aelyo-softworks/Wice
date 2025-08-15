namespace Wice.PropertyGrid;

/// <summary>
/// Configures how a category is presented within a property grid UI.
/// </summary>
/// <remarks>
/// Apply this attribute to a type or to individual properties to control the initial expansion state
/// and the relative ordering of a category identified by <see cref="Name"/>. Multiple instances are
/// allowed to configure different categories.
/// </remarks>
/// <param name="name">
/// The category name this configuration applies to. Categories with the same name share the same options.
/// </param>
/// <example>
/// // Configure a category at the type level
/// [PropertyGridCategoryOptions("Appearance", IsExpanded = true, SortOrder = 0)]
/// public sealed class ButtonOptions { }
///
/// // Configure a category for a specific property
/// public sealed class Settings
/// {
///     [PropertyGridCategoryOptions("Advanced", IsExpanded = false, SortOrder = 20)]
///     public int RetryCount { get; set; }
/// }
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PropertyGridCategoryOptionsAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets or sets the category name these options apply to.
    /// </summary>
    /// <remarks>
    /// Members associated with the same name are grouped under that category in the UI.
    /// </remarks>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets whether the category is expanded by default in the UI.
    /// </summary>
    /// <value>
    /// true to expand the category initially; otherwise, false. The default is true.
    /// </value>
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Gets or sets the relative sort order of the category within the grid.
    /// </summary>
    /// <remarks>
    /// Lower values appear before higher values. Categories without an explicit sort order may be placed
    /// after those that do, depending on the host control's behavior.
    /// </remarks>
    public int SortOrder { get; set; }
}
