
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
                return typeName[..^postfix.Length];

            return typeName;
        }
    }

    // load all samples in this assembly and folder, using reflection
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#pragma warning disable IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
    public IEnumerable<Sample> Samples => GetType().Assembly.GetTypes()
            .Where(t => typeof(Sample).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace == GetType().Namespace + "." + TypeName)
            .Select(t => (Sample)Activator.CreateInstance(t)!)
            .Where(t => t.IsEnabled)
            .OrderBy(t => t.SortOrder);
#pragma warning restore IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.

    public override string ToString() => Title;
}
