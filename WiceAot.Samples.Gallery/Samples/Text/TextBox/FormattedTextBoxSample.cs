namespace Wice.Samples.Gallery.Samples.Text.TextBox;

public class FormattedTextBoxSample : Sample
{
    public override string Description => "A non editable text box that demonstrates the use of advanced DirectWrite techniques.";
    public override int SortOrder => 3;

    public override void Layout(Visual parent)
    {
        var tb = new Wice.TextBox
        {
            FontFamilyName = "Gabriola",
            FontSize = parent.Window!.DipsToPixels(72),
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            Padding = D2D_RECT_F.Thickness(
                parent.Window!.DipsToPixels(10),
                parent.Window!.DipsToPixels(10),
                parent.Window!.DipsToPixels(10),
                parent.Window!.DipsToPixels(50)
            ),
            ClipText = false,
            IsFocusable = true,
            Text = "Hello World using   DirectWrite!"
        };
        Dock.SetDockType(tb, DockType.Top);

        // format the "DirectWrite" substring to be of font size 100.
        tb.SetFontSize(parent.Window!.DipsToPixels(100), new DWRITE_TEXT_RANGE(
            20,     // Index where "DirectWrite" appears.
            6       // Length of the substring "Direct" in "DirectWrite".
        ));

        // format the word "DWrite" to be underlined.
        tb.SetUnderline(true, new DWRITE_TEXT_RANGE(
            20,     // index where "DirectWrite" appears.
            11      // length of the substring "DirectWrite".
        ));

        // format the word "DWrite" to be bold.
        tb.SetFontWeight(DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD, new DWRITE_TEXT_RANGE(
            20,     // index where "DirectWrite" appears.
            11      // length of the substring "DirectWrite".
        ));

        // declare a typography stylistic set.
        var typography = new Typography(new DWRITE_FONT_FEATURE
        {
            nameTag = DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_STYLISTIC_SET_7,
            parameter = 1
        });

        tb.SetTypography(typography.DWriteTypography.Object);

        parent.Children.Add(tb);
    }
}
