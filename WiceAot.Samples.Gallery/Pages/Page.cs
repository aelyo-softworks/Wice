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
#if NETFRAMEWORK
                return typeName.Substring(0, typeName.Length - postfix.Length);
#else
                return typeName[..^postfix.Length];
#endif

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
