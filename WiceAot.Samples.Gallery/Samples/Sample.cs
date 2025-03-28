namespace Wice.Samples.Gallery.Samples;

public abstract class Sample
{
    private static readonly Lazy<ConcurrentDictionary<Type, string>> _sampleTexts = new(GetSamplesTexts, true);

    protected Sample()
    {
    }

    protected internal Compositor? Compositor { get; internal set; }
    protected internal Window? Window { get; internal set; }
    public virtual bool IsEnabled => true;
    public virtual int SortOrder => 0;
    public virtual string? Description { get; }
    public abstract void Layout(Visual parent);

    public override string ToString() => Description ?? string.Empty;

    private static ConcurrentDictionary<Type, string> GetSamplesTexts()
    {
        var dic = new ConcurrentDictionary<Type, string>();
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.samples.xml"))
        {
            if (stream != null)
            {
                var doc = new XmlDocument();
                doc.Load(stream);

                foreach (var text in doc.SelectNodes("samples/sample")!.OfType<XmlElement>())
                {
                    var ns = text.GetAttribute("namespace");
                    if (string.IsNullOrWhiteSpace(ns))
                        continue;

                    var type = Type.GetType(typeof(Program).Namespace + "." + ns, false);
                    if (type == null)
                        continue;

                    var code = text.InnerText;
                    dic[type] = code;
                }
            }
        }
        return dic;
    }

    public string? GetSampleText()
    {
        _sampleTexts.Value.TryGetValue(GetType(), out var text);
        return text;
    }
}
