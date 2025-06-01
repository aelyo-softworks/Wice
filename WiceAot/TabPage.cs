namespace Wice;

public partial class TabPage : BaseObject
{
    public static BaseObjectProperty ContentProperty { get; } = BaseObjectProperty.Add<Visual>(typeof(TabPage), nameof(Content), changed: ContentChanged);
    public static BaseObjectProperty IsSelectableProperty { get; } = BaseObjectProperty.Add(typeof(TabPage), nameof(IsSelectable), true, changed: IsSelectableChanged);
    public static VisualProperty DataProperty { get; } = VisualProperty.Add<object>(typeof(TabPage), nameof(Data), VisualPropertyInvalidateModes.Measure);

    private static void ContentChanged(BaseObject obj, object? newValue, object? oldValue) => ((TabPage)obj).ContentChanged((Visual?)newValue, (Visual?)oldValue);
    private static void IsSelectableChanged(BaseObject obj, object? newValue, object? oldValue) => ((TabPage)obj).IsSelectableChanged((bool)newValue!);

    public TabPage()
    {
        Header = CreateHeader();
        Header.MeasureToContent = DimensionOptions.WidthAndHeight;
        Header.Text.IsEnabled = false;
        if (Header == null)
            throw new InvalidOperationException();
    }

    public Tabs? Tab { get; internal set; }
    public int Index => Tab?.Pages.IndexOf(this) ?? -1;

    [Browsable(false)]
    public Visual? Content { get => (Visual)GetPropertyValue(ContentProperty)!; set => SetPropertyValue(ContentProperty, value); }

    [Browsable(false)]
    public bool IsSelectable { get => (bool)GetPropertyValue(IsSelectableProperty)!; set => SetPropertyValue(IsSelectableProperty, value); }

    [Browsable(false)]
    public object? Data { get => GetPropertyValue(DataProperty)!; set => SetPropertyValue(DataProperty, value); }

    [Browsable(false)]
    public SymbolHeader Header { get; }

    public override string ToString() => $"{Index} '{Header?.Text}'";

    protected virtual SymbolHeader CreateHeader() => new();

#pragma warning disable CA1822 // Mark members as static
    protected internal void RemovedFromTabs(Tabs tabs) => ExceptionExtensions.ThrowIfNull(tabs, nameof(tabs));
    protected internal void AddedToTabs(Tabs tabs) => ExceptionExtensions.ThrowIfNull(tabs, nameof(tabs));
#pragma warning restore CA1822 // Mark members as static

    protected virtual void ContentChanged(Visual? newValue, Visual? oldValue) => Tab?.OnPageContentChanged(this, newValue, oldValue);
    protected virtual void IsSelectableChanged(bool newValue) => Tab?.OnPageIsSelectableChanged(this, newValue);
}