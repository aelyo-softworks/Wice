namespace Wice.Samples.Gallery.Samples.Text.Fonts;

public class CustomFontSample : Sample
{
    public override string Description => "Showing glyphs from a custom font loaded from a WOFF2 file";

    public override void Layout(Visual parent)
    {
        var coll = GetCustomCollection();
        var tb = new Wice.TextBox
        {
            Text = _text,
            FontFamilyName = coll.GetFamilies().First().GetNames()[0].String,
            FontCollection = coll,
            FontSize = 40,
            MaxWidth = 500,
            WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER
        };
        Dock.SetDockType(tb, DockType.Top);
        parent.Children.Add(tb);
    } //

    // precomputed text containing characters supported by the font
    private static string _text = string.Empty;

    // don't dispose these too early, it will be used by the font family and must outlive it.
    private static PackedFontFileLoader? _fontFileLoader;
    private static IComObject<IDWriteFontCollection>? _customCollection;

    // code to load the custom font collection from the WOFF2 file.
    // this is done only once and the collection is cached for future use.
    private static IComObject<IDWriteFontCollection> GetCustomCollection()
    {
        if (_customCollection == null)
        {
            // we need a DirectWrite Factory 5 to load WOFF2 fonts.
            var fac = Application.CurrentResourceManager.DWriteFactory.As<IDWriteFactory5>()!;
            _fontFileLoader ??= new PackedFontFileLoader(fac, DWRITE_CONTAINER_TYPE.DWRITE_CONTAINER_TYPE_WOFF2);

            // this WOFF2 font file was downloaded from https://github.com/microsoft/fluentui-system-icons/fonts and added as an embedded resource to the project.
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.FluentSystemIcons-Regular.woff2")!;
            using var memory = new MemoryStream((int)stream.Length);
            stream.CopyTo(memory);
            using var file = _fontFileLoader.CreateCustomFontFileReference(memory.ToArray());

            // create a font set builder, add the font file to it and create a font set
            using var builder = fac.CreateFontSetBuilder();
            builder.AddFontFile(file);
            using var set = builder.CreateFontSet();

            // derive a font collection from the font set, this will be used by the font family to display the text
            _customCollection = fac.CreateFontCollectionFromFontSet(set);

            // build a string containing characters supported by the font
            using var font = set.GetFonts()[0];
            using var face = font.CreateFontFace();
            var ranges = face.GetUnicodeRanges();

            var sb = new StringBuilder();
            foreach (var range in ranges)
            {
                for (uint c = range.first; c <= range.last; c++)
                {
                    if (c < char.MaxValue)
                    {
                        sb.Append((char)c);
                    }
                    else
                    {
                        var codePoint = char.ConvertFromUtf32((int)c);
                        sb.Append(codePoint);
                    }
                    if (sb.Length >= 200) // stop at some point, just need to demo it
                        break;
                }
            }
            _text = sb.ToString();
        }
        return _customCollection;
    }
}
