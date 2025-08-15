namespace Wice;

/// <summary>
/// Represents an individual page within a <see cref="Tabs"/> control.
/// Hosts a single <see cref="Visual"/> as content and exposes a <see cref="Header"/>
/// used by the tabs strip for selection and presentation.
/// </summary>
/// <remarks>
/// Behavior:
/// - The page is owned by a single <see cref="Tabs"/> instance (see <see cref="Tab"/>).
/// - <see cref="Content"/> changes are delegated to the owning <see cref="Tabs"/> via
///   <see cref="Tabs.OnPageContentChanged(TabPage, Visual?, Visual?)"/> for layout/visual updates.
/// - <see cref="IsSelectable"/> changes are delegated to the owning <see cref="Tabs"/> via
///   <see cref="Tabs.OnPageIsSelectableChanged(TabPage, bool)"/> to update selection logic.
/// - <see cref="Header"/> is created by <see cref="CreateHeader"/> during construction and
///   is configured to measure to its content with disabled inner text interaction.
/// </remarks>
public partial class TabPage : BaseObject
{
    /// <summary>
    /// Identifies the <see cref="Content"/> property.
    /// Changing this property triggers <see cref="ContentChanged(Visual?, Visual?)"/> which is
    /// forwarded to the owning <see cref="Tabs"/> instance (if any).
    /// </summary>
    public static BaseObjectProperty ContentProperty { get; } = BaseObjectProperty.Add<Visual>(typeof(TabPage), nameof(Content), changed: ContentChanged);

    /// <summary>
    /// Identifies the <see cref="IsSelectable"/> property.
    /// Default value is <see langword="true"/>. Changing this property triggers
    /// <see cref="IsSelectableChanged(bool)"/> which is forwarded to the owning <see cref="Tabs"/>.
    /// </summary>
    public static BaseObjectProperty IsSelectableProperty { get; } = BaseObjectProperty.Add(typeof(TabPage), nameof(IsSelectable), true, changed: IsSelectableChanged);

    /// <summary>
    /// Identifies the <see cref="Data"/> property.
    /// Changing this property invalidates measure on consumers similar to <see cref="Visual.Data"/>.
    /// </summary>
    public static VisualProperty DataProperty { get; } = VisualProperty.Add<object>(typeof(TabPage), nameof(Data), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Static changed handler that dispatches to the instance-level <see cref="ContentChanged(Visual?, Visual?)"/>.
    /// </summary>
    private static void ContentChanged(BaseObject obj, object? newValue, object? oldValue) => ((TabPage)obj).ContentChanged((Visual?)newValue, (Visual?)oldValue);

    /// <summary>
    /// Static changed handler that dispatches to the instance-level <see cref="IsSelectableChanged(bool)"/>.
    /// </summary>
    private static void IsSelectableChanged(BaseObject obj, object? newValue, object? oldValue) => ((TabPage)obj).IsSelectableChanged((bool)newValue!);

    /// <summary>
    /// Initializes a new <see cref="TabPage"/>, creating and configuring the <see cref="Header"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="CreateHeader"/> returns null.</exception>
    public TabPage()
    {
        Header = CreateHeader();
        Header.MeasureToContent = DimensionOptions.WidthAndHeight;
        Header.Text.IsEnabled = false;
        if (Header == null)
            throw new InvalidOperationException();
    }

    /// <summary>
    /// Gets the owning <see cref="Tabs"/> container, or <see langword="null"/> when not attached.
    /// </summary>
    public Tabs? Tab { get; internal set; }

    /// <summary>
    /// Gets the zero-based index of this page in <see cref="Tabs.Pages"/>, or -1 when unattached.
    /// </summary>
    public int Index => Tab?.Pages.IndexOf(this) ?? -1;

    /// <summary>
    /// Gets or sets the content visual hosted by this page.
    /// </summary>
    /// <remarks>
    /// Setting this property notifies the owning <see cref="Tabs"/> so it can insert/remove the visual
    /// in its visual tree using <see cref="Tabs.AddPageContent(TabPage, int, Visual)"/> and
    /// <see cref="Tabs.RemovePageContent(TabPage, int, Visual)"/>.
    /// </remarks>
    [Browsable(false)]
    public Visual? Content { get => (Visual)GetPropertyValue(ContentProperty)!; set => SetPropertyValue(ContentProperty, value); }

    /// <summary>
    /// Gets or sets whether the page can be selected by the user.
    /// </summary>
    [Browsable(false)]
    public bool IsSelectable { get => (bool)GetPropertyValue(IsSelectableProperty)!; set => SetPropertyValue(IsSelectableProperty, value); }

    /// <summary>
    /// Gets or sets arbitrary user data associated with this page.
    /// </summary>
    /// <remarks>
    /// Changing this value invalidates measure for consumers similar to <see cref="Visual.Data"/>.
    /// </remarks>
    [Browsable(false)]
    public object? Data { get => GetPropertyValue(DataProperty)!; set => SetPropertyValue(DataProperty, value); }

    /// <summary>
    /// Gets the header UI used by the tabs strip for this page.
    /// </summary>
    [Browsable(false)]
    public SymbolHeader Header { get; }

    /// <summary>
    /// Returns a string useful for diagnostics that includes the index and header text.
    /// </summary>
    public override string ToString() => $"{Index} '{Header?.Text}'";

    /// <summary>
    /// Creates the header instance used by this page.
    /// Override to customize the header type or initial configuration.
    /// </summary>
    /// <returns>A non-null <see cref="SymbolHeader"/> instance.</returns>
    protected virtual SymbolHeader CreateHeader() => new();

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Called when this page is removed from the specified <paramref name="tabs"/>.
    /// Override to perform cleanup related to the owning tabs instance.
    /// </summary>
    /// <param name="tabs">The tabs control this page was removed from.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="tabs"/> is null.</exception>
    protected internal void RemovedFromTabs(Tabs tabs) => ExceptionExtensions.ThrowIfNull(tabs, nameof(tabs));

    /// <summary>
    /// Called when this page is added to the specified <paramref name="tabs"/>.
    /// Override to perform initialization related to the owning tabs instance.
    /// </summary>
    /// <param name="tabs">The tabs control this page was added to.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="tabs"/> is null.</exception>
    protected internal void AddedToTabs(Tabs tabs) => ExceptionExtensions.ThrowIfNull(tabs, nameof(tabs));
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Instance-level handler invoked when <see cref="Content"/> changes.
    /// Forwards the change to the owning <see cref="Tabs"/> (if any) so it can update its visual tree.
    /// </summary>
    /// <param name="newValue">The new content visual.</param>
    /// <param name="oldValue">The previous content visual.</param>
    protected virtual void ContentChanged(Visual? newValue, Visual? oldValue) => Tab?.OnPageContentChanged(this, newValue, oldValue);

    /// <summary>
    /// Instance-level handler invoked when <see cref="IsSelectable"/> changes.
    /// Forwards the change to the owning <see cref="Tabs"/> (if any) so it can update selection logic.
    /// </summary>
    /// <param name="newValue">The new selectable state.</param>
    protected virtual void IsSelectableChanged(bool newValue) => Tab?.OnPageIsSelectableChanged(this, newValue);
}