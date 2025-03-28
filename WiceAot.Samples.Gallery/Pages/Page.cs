namespace Wice.Samples.Gallery.Pages;

public abstract class Page : Titled
{
    protected Page()
    {
        Title.Text = HeaderText;
    }

    // determine a default name using the class name
    public virtual string TypeName
    {
        get
        {
            const string postfix = nameof(Page);
            var typeName = GetType().Name;
            if (typeName.Length > postfix.Length && typeName.EndsWith(postfix))
                return typeName[..^postfix.Length];

            return typeName;
        }
    }

    public virtual string HeaderText => TypeName;
    public virtual string ToolTipText => "The " + HeaderText + " page";
    public virtual DockType DockType => DockType.Top;

    public abstract int SortOrder { get; }
    public abstract string IconText { get; }

    public static IEnumerable<Page> Pages
    {
        get
        {
            yield return new AboutPage();
            yield return new CollectionsPage();
            yield return new EffectsPage();
            yield return new HomePage();
            yield return new InputPage();
            yield return new LayoutPage();
            yield return new MediaPage();
            yield return new MiscPage();
            yield return new TextPage();
            yield return new WindowsPage();
        }
    }

    public static IEnumerable<Page> GetPages() => Pages.OrderBy(p => p.SortOrder);
}
