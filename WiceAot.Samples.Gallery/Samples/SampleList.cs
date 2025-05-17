namespace Wice.Samples.Gallery.Samples;

public abstract class SampleList
{
    protected SampleList()
    {
    }

    public abstract string IconText { get; }

    public virtual bool IsEnabled => true;
    public virtual string Title => TypeName;
    public virtual string SubTitle => "The " + Title + " visual.";
    public virtual string Description => SubTitle;
    public virtual string TypeName
    {
        get
        {
            const string postfix = nameof(SampleList);
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

    protected abstract IEnumerable<Sample> Types { get; }
    public IEnumerable<Sample> Samples => Types.Where(t => t.IsEnabled).OrderBy(t => t.SortOrder);

    public override string ToString() => Title;
}
