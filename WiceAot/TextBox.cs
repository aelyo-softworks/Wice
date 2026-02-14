namespace Wice;

/// <summary>
/// Represents a visual that allows the user to input, edit, and display text.
/// </summary>
public partial class TextBox : RenderVisual, ITextFormat, ITextBoxProperties, IValueable, IPasswordCapable, IDisposable, IImmVisual
{
    /// <summary>
    /// Gets the foreground brush used to draw the text.
    /// </summary>
    public static VisualProperty ForegroundBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(TextBox), nameof(ForegroundBrush), VisualPropertyInvalidateModes.Render, Theme.Default.TextBoxForegroundColor);

    /// <summary>
    /// Gets the foreground brush used when the mouse is over the control.
    /// </summary>
    public static VisualProperty HoverForegroundBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(TextBox), nameof(HoverForegroundBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets the brush used to draw the selection highlight.
    /// </summary>
    public static VisualProperty SelectionBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(TextBox), nameof(SelectionBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets or sets the font family name for the entire text when not overridden by ranges.
    /// </summary>
    public static VisualProperty FontFamilyNameProperty { get; } = VisualProperty.Add<string>(typeof(TextBox), nameof(FontFamilyName), VisualPropertyInvalidateModes.Measure);

    /// <summary>Gets or sets the default font weight.</summary>
    public static VisualProperty FontWeightProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(FontWeight), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL);

    /// <summary>Gets or sets the default font stretch.</summary>
    public static VisualProperty FontStretchProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(FontStretch), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL);

    /// <summary>Gets or sets the default font style.</summary>
    public static VisualProperty FontStyleProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(FontStyle), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL);

    /// <summary>Gets or sets the paragraph alignment (vertical alignment within the layout box).</summary>
    public static VisualProperty ParagraphAlignmentProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(ParagraphAlignment), VisualPropertyInvalidateModes.Measure, DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR);

    /// <summary>Gets or sets the text alignment (horizontal alignment within the layout box).</summary>
    public static VisualProperty AlignmentProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(Alignment), VisualPropertyInvalidateModes.Measure, DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING);

    /// <summary>Gets or sets the flow direction for vertical layout scenarios.</summary>
    public static VisualProperty FlowDirectionProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(FlowDirection), VisualPropertyInvalidateModes.Measure, DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM);

    /// <summary>Gets or sets the reading direction.</summary>
    public static VisualProperty ReadingDirectionProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(ReadingDirection), VisualPropertyInvalidateModes.Measure, DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT);

    /// <summary>Gets or sets the word wrapping mode.</summary>
    public static VisualProperty WordWrappingProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(WordWrapping), VisualPropertyInvalidateModes.Measure, DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);

    /// <summary>Gets or sets the trimming behavior when text overflows.</summary>
    public static VisualProperty TrimmingGranularityProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(TrimmingGranularity), VisualPropertyInvalidateModes.Measure, DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_NONE);

    /// <summary>Gets or sets whether the last line is allowed to wrap.</summary>
    public static VisualProperty IsLastLineWrappingEnabledProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(IsLastLineWrappingEnabled), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>Gets or sets the line spacing configuration.</summary>
    public static VisualProperty LineSpacingProperty { get; } = VisualProperty.Add<DWRITE_LINE_SPACING?>(typeof(TextBox), nameof(LineSpacing), VisualPropertyInvalidateModes.Measure, null);

    /// <summary>Gets or sets the optical alignment applied by DirectWrite (hanging punctuation, etc.).</summary>
    public static VisualProperty OpticalAlignmentProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(OpticalAlignment), VisualPropertyInvalidateModes.Measure, DWRITE_OPTICAL_ALIGNMENT.DWRITE_OPTICAL_ALIGNMENT_NONE);

    /// <summary>Gets or sets the vertical glyph orientation used for vertical text.</summary>
    public static VisualProperty VerticalGlyphOrientationProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(VerticalGlyphOrientation), VisualPropertyInvalidateModes.Measure, DWRITE_VERTICAL_GLYPH_ORIENTATION.DWRITE_VERTICAL_GLYPH_ORIENTATION_DEFAULT);

    /// <summary>
    /// Gets or sets the plain text content of the control.
    /// </summary>
    public static VisualProperty TextProperty { get; } = VisualProperty.Add<string>(typeof(TextBox), nameof(Text), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);

    /// <summary>Gets or sets the DirectWrite font collection used for layout.</summary>
    public static VisualProperty FontCollectionProperty { get; } = VisualProperty.Add<IComObject<IDWriteFontCollection>>(typeof(TextBox), nameof(FontCollection), VisualPropertyInvalidateModes.Measure);

    /// <summary>Gets or sets the font fallback chain.</summary>
    public static VisualProperty FontFallbackProperty { get; } = VisualProperty.Add<IComObject<IDWriteFontFallback>>(typeof(TextBox), nameof(FontFallback), VisualPropertyInvalidateModes.Measure);

    /// <summary>Gets or sets Direct2D draw options used when drawing the text layout.</summary>
    public static VisualProperty DrawOptionsProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(DrawOptions), VisualPropertyInvalidateModes.Render, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);

    /// <summary>Gets or sets the text antialias mode applied during drawing (unless overridden by <see cref="TextRenderingParameters"/>).</summary>
    public static VisualProperty AntiAliasingModeProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(AntiAliasingMode), VisualPropertyInvalidateModes.Render, D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_DEFAULT);

    /// <summary>
    /// Gets or sets whether the visual is editable. When true, caret, selection and input are enabled.
    /// </summary>
    public static VisualProperty IsEditableProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(IsEditable), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Gets or sets whether Ctrl+wheel/Shift+wheel zooming (pan/scale) is enabled.
    /// </summary>
    public static VisualProperty IsWheelZoomEnabledProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(IsWheelZoomEnabled), VisualPropertyInvalidateModes.Render, false);

    /// <summary>Gets or sets whether the Tab key inserts a tab character (otherwise moves focus).</summary>
    public static VisualProperty AcceptsTabProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(AcceptsTab), VisualPropertyInvalidateModes.None, false);

    /// <summary>Gets or sets whether the Return/Enter key inserts a newline.</summary>
    public static VisualProperty AcceptsReturnProperty { get; } = VisualProperty.Add(typeof(TextBox), nameof(AcceptsReturn), VisualPropertyInvalidateModes.None, false);

    /// <summary>Gets or sets the base font size used by the layout.</summary>
    public static VisualProperty FontSizeProperty { get; } = VisualProperty.Add<float?>(typeof(TextBox), nameof(FontSize), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Gets or sets the text rendering parameters. When set, they are applied to the D2D device context before drawing.
    /// </summary>
    public static VisualProperty TextRenderingParametersProperty { get; } = VisualProperty.Add<TextRenderingParameters>(typeof(TextBox), nameof(TextRenderingParameters), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets or sets the optional password masking character.
    /// </summary>
    public static VisualProperty PasswordCharProperty { get; } = VisualProperty.Add<char?>(typeof(TextBox), nameof(PasswordCharacter), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Gets or sets whether the text rendering is clipped to the visual bounds (padding-aware).
    /// </summary>
    public static VisualProperty ClipTextProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ClipText), VisualPropertyInvalidateModes.Render, true);

    private readonly Lock _rangesLock = new();
    private readonly ConcurrentDictionary<FontRangeType, FontRanges> _ranges = new();

    // cache
    private IComObject<IDWriteTextLayout>? _layout;
    private float _maxWidth;
    private float _maxHeight;
    private string? _text;
    private bool _rendered;

    // many edit code here is taken from the PadWrite sample https://github.com/microsoft/Windows-classic-samples/tree/main/Samples/Win7Samples/multimedia/DirectWrite/PadWrite
    private uint _charAnchor;
    private uint _charPosition;
    private uint _charPositionOffset;
    private CaretFormat? _caretFormat;
    private D2D_POINT_2F _origin;

    private float _lastCaretX;
    private bool _selecting;
    private FontRanges[]? _finalRanges;
    private readonly UndoStack<UndoState> _undoStack = new();
    private bool _textHasChanged;
    private bool _disposedValue;

    private event EventHandler<ValueEventArgs>? _valueChanged;
    event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { _valueChanged += value; } remove { _valueChanged -= value; } }

    /// <summary>
    /// Occurs when <see cref="Text"/> changes. Emission can be gated by <see cref="RaiseTextChanged"/>
    /// and <see cref="TextChangedTrigger"/>.
    /// </summary>
    public event EventHandler<ValueEventArgs<string>>? TextChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="TextBox"/> with <see cref="RaiseTextChanged"/> set to <see langword="true"/>.
    /// </summary>
    public TextBox()
    {
        RaiseTextChanged = true;
    }

    bool IValueable.CanChangeValue { get => IsEditable && IsEnabled; set => IsEditable = value; }
    object IValueable.Value => Text;
    bool IValueable.TrySetValue(object? value)
    {
        if (value is string str)
        {
            Text = str;
        }
        else
        {
            Text = string.Format("{0}", value);
        }
        return true;
    }

    private sealed class UndoState
    {
        public static UndoState From(TextBox tb)
        {
            var state = new UndoState
            {
                Text = tb.Text,
                CharAnchor = tb._charAnchor,
                CharPosition = tb._charPosition,
                CharPositionOffset = tb._charPositionOffset,
                CaretFormat = tb._caretFormat,
                Origin = tb._origin
            };
            return state;
        }

        public string Text = string.Empty;
        public uint CharAnchor;
        public uint CharPosition;
        public uint CharPositionOffset;
        public CaretFormat? CaretFormat;
        public D2D_POINT_2F Origin;

        public void Apply(TextBox tb)
        {
            tb.Text = Text;
            tb._charAnchor = CharAnchor;
            tb._charPosition = CharPosition;
            tb._charPositionOffset = CharPositionOffset;
            tb._caretFormat = CaretFormat;
            tb._origin = Origin;
        }

        public override string ToString() => Text;
    }

    private void Reset()
    {
        Interlocked.Exchange(ref _layout, null)?.Dispose();
        _rendered = false;
        _text = null;
        _maxHeight = 0;
        _maxWidth = 0;
    }

    /// <inheritdoc/>
    protected override bool FallbackToTransparentBackground => true;

    /// <inheritdoc/>
    protected override bool ShouldRender => !_rendered;

    /// <summary>
    /// Gets the text used for rendering and hit-testing. Defaults to <see cref="Text"/>; when in password mode,
    /// returns the masked string.
    /// </summary>
    protected virtual string RenderedText => Text;

    /// <summary>
    /// Gets or sets whether <see cref="TextChanged"/> is raised automatically when <see cref="Text"/> changes.
    /// </summary>
    [Browsable(false)]
    public virtual bool RaiseTextChanged { get; set; }

    /// <summary>
    /// Gets or sets when <see cref="TextChanged"/> is emitted relative to focus/return key events.
    /// </summary>
    [Browsable(false)]
    public virtual EventTrigger TextChangedTrigger { get; set; }

    /// <summary>Gets or sets the brush used to draw the text.</summary>
    [Category(CategoryLayout)]
    public Brush ForegroundBrush { get => (Brush)GetPropertyValue(ForegroundBrushProperty)!; set => SetPropertyValue(ForegroundBrushProperty, value); }

    /// <summary>Gets or sets the brush used to draw the text when the mouse is over the control.</summary>
    [Category(CategoryLayout)]
    public Brush? HoverForegroundBrush { get => (Brush)GetPropertyValue(HoverForegroundBrushProperty)!; set => SetPropertyValue(HoverForegroundBrushProperty, value); }

    /// <summary>Gets or sets the selection brush used to fill the selected range background.</summary>
    [Category(CategoryLayout)]
    public Brush SelectionBrush { get => (Brush)GetPropertyValue(SelectionBrushProperty)!; set => SetPropertyValue(SelectionBrushProperty, value); }

    /// <summary>Gets or sets the default font family name.</summary>
    [Category(CategoryLayout)]
    public string? FontFamilyName { get => (string?)GetPropertyValue(FontFamilyNameProperty)!; set => SetPropertyValue(FontFamilyNameProperty, value); }

    /// <summary>Gets or sets the default font weight.</summary>
    [Category(CategoryLayout)]
    public DWRITE_FONT_WEIGHT FontWeight { get => (DWRITE_FONT_WEIGHT)GetPropertyValue(FontWeightProperty)!; set => SetPropertyValue(FontWeightProperty, value); }

    /// <summary>Gets or sets the default font style.</summary>
    [Category(CategoryLayout)]
    public DWRITE_FONT_STYLE FontStyle { get => (DWRITE_FONT_STYLE)GetPropertyValue(FontStyleProperty)!; set => SetPropertyValue(FontStyleProperty, value); }

    /// <summary>Gets or sets the default font stretch.</summary>
    [Category(CategoryLayout)]
    public DWRITE_FONT_STRETCH FontStretch { get => (DWRITE_FONT_STRETCH)GetPropertyValue(FontStretchProperty)!; set => SetPropertyValue(FontStretchProperty, value); }

    /// <summary>Gets or sets the paragraph alignment inside the layout box.</summary>
    [Category(CategoryLayout)]
    public DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get => (DWRITE_PARAGRAPH_ALIGNMENT)GetPropertyValue(ParagraphAlignmentProperty)!; set => SetPropertyValue(ParagraphAlignmentProperty, value); }

    /// <summary>Gets or sets the horizontal text alignment inside the layout box.</summary>
    [Category(CategoryLayout)]
    public DWRITE_TEXT_ALIGNMENT Alignment { get => (DWRITE_TEXT_ALIGNMENT)GetPropertyValue(AlignmentProperty)!; set => SetPropertyValue(AlignmentProperty, value); }

    /// <summary>Gets or sets the flow direction for vertical text.</summary>
    [Category(CategoryLayout)]
    public DWRITE_FLOW_DIRECTION FlowDirection { get => (DWRITE_FLOW_DIRECTION)GetPropertyValue(FlowDirectionProperty)!; set => SetPropertyValue(FlowDirectionProperty, value); }

    /// <summary>Gets or sets the reading direction.</summary>
    [Category(CategoryLayout)]
    public DWRITE_READING_DIRECTION ReadingDirection { get => (DWRITE_READING_DIRECTION)GetPropertyValue(ReadingDirectionProperty)!; set => SetPropertyValue(ReadingDirectionProperty, value); }

    /// <summary>Gets or sets the word wrapping mode.</summary>
    [Category(CategoryLayout)]
    public DWRITE_WORD_WRAPPING WordWrapping { get => (DWRITE_WORD_WRAPPING)GetPropertyValue(WordWrappingProperty)!; set => SetPropertyValue(WordWrappingProperty, value); }

    /// <summary>Gets or sets the trimming granularity used when text overflows.</summary>
    [Category(CategoryLayout)]
    public DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get => (DWRITE_TRIMMING_GRANULARITY)GetPropertyValue(TrimmingGranularityProperty)!; set => SetPropertyValue(TrimmingGranularityProperty, value); }

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string Text { get => (string?)GetPropertyValue(TextProperty)! ?? string.Empty; set => SetPropertyValue(TextProperty, value); }

    /// <summary>Gets or sets the font collection for layout.</summary>
    [Category(CategoryLayout)]
    public IComObject<IDWriteFontCollection>? FontCollection { get => (IComObject<IDWriteFontCollection>?)GetPropertyValue(FontCollectionProperty)!; set => SetPropertyValue(FontCollectionProperty, value); }

    /// <summary>Gets or sets the Direct2D draw options applied when drawing the layout.</summary>
    [Category(CategoryLayout)]
    public D2D1_DRAW_TEXT_OPTIONS DrawOptions { get => (D2D1_DRAW_TEXT_OPTIONS)GetPropertyValue(DrawOptionsProperty)!; set => SetPropertyValue(DrawOptionsProperty, value); }

    /// <summary>Gets or sets the text anti-aliasing mode when drawing.</summary>
    [Category(CategoryLayout)]
    public D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get => (D2D1_TEXT_ANTIALIAS_MODE)GetPropertyValue(AntiAliasingModeProperty)!; set => SetPropertyValue(AntiAliasingModeProperty, value); }

    /// <summary>Gets or sets the optical alignment (DirectWrite) behavior.</summary>
    [Category(CategoryLayout)]
    public DWRITE_OPTICAL_ALIGNMENT OpticalAlignment { get => (DWRITE_OPTICAL_ALIGNMENT)GetPropertyValue(OpticalAlignmentProperty)!; set => SetPropertyValue(OpticalAlignmentProperty, value); }

    /// <summary>Gets or sets the vertical glyph orientation (for vertical text scenarios).</summary>
    [Category(CategoryLayout)]
    public DWRITE_VERTICAL_GLYPH_ORIENTATION VerticalGlyphOrientation { get => (DWRITE_VERTICAL_GLYPH_ORIENTATION)GetPropertyValue(VerticalGlyphOrientationProperty)!; set => SetPropertyValue(VerticalGlyphOrientationProperty, value); }

    /// <summary>Gets or sets the line spacing values.</summary>
    [Category(CategoryLayout)]
    public DWRITE_LINE_SPACING? LineSpacing { get => (DWRITE_LINE_SPACING?)GetPropertyValue(LineSpacingProperty)!; set => SetPropertyValue(LineSpacingProperty, value); }

    /// <summary>Gets or sets the font fallback sequence used by the layout.</summary>
    [Category(CategoryLayout)]
    public IComObject<IDWriteFontFallback>? FontFallback { get => (IComObject<IDWriteFontFallback>?)GetPropertyValue(FontFallbackProperty)!; set => SetPropertyValue(FontFallbackProperty, value); }

    /// <summary>Gets or sets whether the last line of text can wrap.</summary>
    [Category(CategoryLayout)]
    public bool IsLastLineWrappingEnabled { get => (bool)GetPropertyValue(IsLastLineWrappingEnabledProperty)!; set => SetPropertyValue(IsLastLineWrappingEnabledProperty, value); }

    /// <summary>Gets or sets whether the visual is editable.</summary>
    [Category(CategoryBehavior)]
    public virtual bool IsEditable { get => (bool)GetPropertyValue(IsEditableProperty)!; set => SetPropertyValue(IsEditableProperty, value); }

    /// <summary>Gets or sets whether mouse wheel panning/zoom is enabled (with Ctrl/Shift modifiers).</summary>
    [Category(CategoryBehavior)]
    public bool IsWheelZoomEnabled { get => (bool)GetPropertyValue(IsWheelZoomEnabledProperty)!; set => SetPropertyValue(IsWheelZoomEnabledProperty, value); }

    /// <summary>Gets or sets whether the Tab key inserts a tab character (otherwise moves focus).</summary>
    [Category(CategoryBehavior)]
    public bool AcceptsTab { get => (bool)GetPropertyValue(AcceptsTabProperty)!; set => SetPropertyValue(AcceptsTabProperty, value); }

    /// <summary>Gets or sets whether the Return key inserts a newline (otherwise may trigger TextChanged on Return when configured).</summary>
    [Category(CategoryBehavior)]
    public bool AcceptsReturn { get => (bool)GetPropertyValue(AcceptsReturnProperty)!; set => SetPropertyValue(AcceptsReturnProperty, value); }

    /// <summary>Gets or sets the base font size for the layout.</summary>
    [Category(CategoryLayout)]
    public float? FontSize { get => (float?)GetPropertyValue(FontSizeProperty); set => SetPropertyValue(FontSizeProperty, value); }

    /// <summary>Gets or sets the DirectWrite/Direct2D rendering parameters applied before drawing.</summary>
    [Category(CategoryLayout)]
    public TextRenderingParameters TextRenderingParameters { get => (TextRenderingParameters)GetPropertyValue(TextRenderingParametersProperty)!; set => SetPropertyValue(TextRenderingParametersProperty, value); }

    /// <summary>
    /// Gets or sets the optional password masking character. When set, the display masks text while preserving <see cref="Text"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public char? PasswordCharacter { get => (char?)GetPropertyValue(PasswordCharProperty); set => SetPropertyValue(PasswordCharProperty, value); }

    /// <summary>Gets or sets whether text rendering is clipped to the visual bounds.</summary>
    [Category(CategoryRender)]
    public bool ClipText { get => (bool)GetPropertyValue(ClipTextProperty)!; set => SetPropertyValue(ClipTextProperty, value); }

    /// <summary>Sets the font weight over the entire text or provided range(s).</summary>
    public void SetFontWeight(DWRITE_FONT_WEIGHT weight) => SetFontWeight(weight, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontWeight(DWRITE_FONT_WEIGHT)"/>
    public void SetFontWeight(DWRITE_FONT_WEIGHT weight, DWRITE_TEXT_RANGE range) => SetFontWeight(weight, [range]);
    /// <summary>Sets the font weight for the specified ranges.</summary>
    public virtual void SetFontWeight(DWRITE_FONT_WEIGHT weight, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontWeight, weight, ranges);

    /// <summary>Sets the font stretch over the entire text or provided range(s).</summary>
    public void SetFontStretch(DWRITE_FONT_STRETCH stretch) => SetFontStretch(stretch, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontStretch(DWRITE_FONT_STRETCH)"/>
    public void SetFontStretch(DWRITE_FONT_STRETCH stretch, DWRITE_TEXT_RANGE range) => SetFontStretch(stretch, [range]);
    /// <summary>Sets the font stretch for the specified ranges.</summary>
    public virtual void SetFontStretch(DWRITE_FONT_STRETCH stretch, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontStretch, stretch, ranges);

    /// <summary>Sets the font style over the entire text or provided range(s).</summary>
    public void SetFontStyle(DWRITE_FONT_STYLE style) => SetFontStyle(style, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontStyle(DWRITE_FONT_STYLE)"/>
    public void SetFontStyle(DWRITE_FONT_STYLE style, DWRITE_TEXT_RANGE range) => SetFontStyle(style, [range]);
    /// <summary>Sets the font style for the specified ranges.</summary>
    public virtual void SetFontStyle(DWRITE_FONT_STYLE style, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontStyle, style, ranges);

    /// <summary>Sets the font size over the entire text or provided range(s).</summary>
    public void SetFontSize(float size) => SetFontSize(size, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontSize(float)"/>
    public void SetFontSize(float size, DWRITE_TEXT_RANGE range) => SetFontSize(size, [range]);
    /// <summary>Sets the font size for the specified ranges.</summary>
    public virtual void SetFontSize(float size, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontSize, size, ranges);

    /// <summary>Sets the font family name over the entire text or provided range(s).</summary>
    public void SetFontFamilyName(string name) => SetFontFamilyName(name, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontFamilyName(string)"/>
    public void SetFontFamilyName(string name, DWRITE_TEXT_RANGE range) => SetFontFamilyName(name, [range]);
    /// <summary>Sets the font family name for the specified ranges.</summary>
    public virtual void SetFontFamilyName(string name, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontFamilyName, name, ranges);

    /// <summary>Sets the font collection over the entire text or provided range(s).</summary>
    public void SetFontCollection(string collection) => SetFontCollection(collection, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetFontCollection(string)"/>
    public void SetFontCollection(string collection, DWRITE_TEXT_RANGE range) => SetFontCollection(collection, [range]);
    /// <summary>Sets the font collection for the specified ranges.</summary>
    public virtual void SetFontCollection(string collection, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontCollection, collection, ranges);

    /// <summary>Applies or removes strikethrough over the entire text or provided range(s).</summary>
    public void SetStrikethrough(bool strikethrough) => SetStrikethrough(strikethrough, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetStrikethrough(bool)"/>
    public void SetStrikethrough(bool strikethrough, DWRITE_TEXT_RANGE range) => SetStrikethrough(strikethrough, [range]);
    /// <summary>Applies or removes strikethrough for the specified ranges.</summary>
    public virtual void SetStrikethrough(bool strikethrough, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Strikethrough, strikethrough, ranges);

    /// <summary>Applies or removes underline over the entire text or provided range(s).</summary>
    public void SetUnderline(bool underline) => SetUnderline(underline, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetUnderline(bool)"/>
    public void SetUnderline(bool underline, DWRITE_TEXT_RANGE range) => SetUnderline(underline, [range]);
    /// <summary>Applies or removes underline for the specified ranges.</summary>
    public virtual void SetUnderline(bool underline, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Underline, underline, ranges);

    /// <summary>Sets the locale name over the entire text or provided range(s).</summary>
    public void SetLocaleName(string name) => SetLocaleName(name, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetLocaleName(string)"/>
    public void SetLocaleName(string name, DWRITE_TEXT_RANGE range) => SetLocaleName(name, [range]);
    /// <summary>Sets the locale name for the specified ranges.</summary>
    public virtual void SetLocaleName(string name, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.LocaleName, name, ranges);

    /// <summary>Sets the typography object over the entire text or provided range(s).</summary>
    public void SetTypography(IDWriteTypography? typography) => SetTypography(typography, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetTypography(IDWriteTypography?)"/>
    public void SetTypography(IDWriteTypography? typography, DWRITE_TEXT_RANGE range) => SetTypography(typography, [range]);
    /// <summary>Sets the typography object for the specified ranges.</summary>
    public virtual void SetTypography(IDWriteTypography? typography, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Typography, typography, ranges);

    /// <summary>Sets the inline object over the entire text or provided range(s).</summary>
    public void SetInlineObject(IDWriteInlineObject inlineObject) => SetInlineObject(inlineObject, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetInlineObject(IDWriteInlineObject)"/>
    public void SetInlineObject(IDWriteInlineObject inlineObject, DWRITE_TEXT_RANGE range) => SetInlineObject(inlineObject, [range]);
    /// <summary>Sets the inline object for the specified ranges.</summary>
    public virtual void SetInlineObject(IDWriteInlineObject inlineObject, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.InlineObject, inlineObject, ranges);

    /// <summary>Sets a drawing effect (brush/object) over the entire text or provided range(s).</summary>
    public void SetDrawingEffect(object drawingEffect) => SetDrawingEffect(drawingEffect, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetDrawingEffect(object)"/>
    public void SetDrawingEffect(object drawingEffect, DWRITE_TEXT_RANGE range) => SetDrawingEffect(drawingEffect, [range]);
    /// <summary>Sets a drawing effect for the specified ranges.</summary>
    public virtual void SetDrawingEffect(object drawingEffect, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.DrawingEffect, drawingEffect, ranges);

    /// <summary>Sets a solid color over the entire text or provided range(s).</summary>
    public void SetSolidColor(D3DCOLORVALUE color) => SetSolidColor(color, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetSolidColor(D3DCOLORVALUE)"/>
    public void SetSolidColor(D3DCOLORVALUE color, DWRITE_TEXT_RANGE range) => SetSolidColor(color, [range]);
    /// <summary>Sets a solid color for the specified ranges.</summary>
    public virtual void SetSolidColor(D3DCOLORVALUE color, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.SolidColor, color, ranges);

    /// <summary>Enables or disables pair kerning over the entire text or provided range(s).</summary>
    public void SetPairKerning(bool pairKerning) => SetPairKerning(pairKerning, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetPairKerning(bool)"/>
    public void SetPairKerning(bool pairKerning, DWRITE_TEXT_RANGE range) => SetPairKerning(pairKerning, [range]);
    /// <summary>Sets pair kerning for the specified ranges.</summary>
    public virtual void SetPairKerning(bool pairKerning, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.PairKerning, pairKerning, ranges);

    /// <summary>
    /// Sets character spacing (tracking) for the entire text or provided range(s).
    /// </summary>
    public void SetCharacterSpacing(float leadingSpacing, float trailingSpacing, float minimumAdvanceWidth) => SetCharacterSpacing(leadingSpacing, trailingSpacing, minimumAdvanceWidth, new DWRITE_TEXT_RANGE(0));
    /// <inheritdoc cref="SetCharacterSpacing(float, float, float)"/>
    public void SetCharacterSpacing(float leadingSpacing, float trailingSpacing, float minimumAdvanceWidth, DWRITE_TEXT_RANGE range) => SetCharacterSpacing(leadingSpacing, trailingSpacing, minimumAdvanceWidth, [range]);
    /// <summary>Sets character spacing for the specified ranges.</summary>
    public virtual void SetCharacterSpacing(float leadingSpacing, float trailingSpacing, float minimumAdvanceWidth, DWRITE_TEXT_RANGE[] ranges)
        => SetFontRangeValue(FontRangeType.CharacterSpacing, new CharacterSpacing
        {
            leadingSpacing = leadingSpacing,
            trailingSpacing = trailingSpacing,
            minimumAdvanceWidth = minimumAdvanceWidth
        }, ranges);

    private string PasswordText
    {
        get
        {
            var pw = PasswordCharacter;
            if (!pw.HasValue)
                return Text;

            var text = Text;
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(pw.Value, text.Length);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var text = base.ToString() + " '" + Text?.Replace('\r', '⏎').Replace("\n", string.Empty) + "'";
        return text.TrimWithEllipsis() ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override void SetCompositionBrush(CompositionBrush? brush)
    {
        // we use or own system
        //base.SetCompositionBrush(brush);
    }

    /// <summary>
    /// Commits any pending text changes and raises the <see cref="TextChanged"/> event if applicable.
    /// </summary>
    public virtual void CommitChanges()
    {
        if (_textHasChanged && RaiseTextChanged)
        {
            OnTextChanged(this, new ValueEventArgs<string>(Text));
        }
        _textHasChanged = false;
    }

    /// <summary>
    /// Internal text change handler honoring <see cref="RaiseTextChanged"/> and <see cref="TextChangedTrigger"/>.
    /// </summary>
    protected virtual void DoOnTextChanged(object sender, ValueEventArgs<string> e)
    {
        _textHasChanged = true;
        if (!RaiseTextChanged)
            return;

        if (TextChangedTrigger != EventTrigger.LostFocus &&
            TextChangedTrigger != EventTrigger.LostFocusOrReturnPressed)
        {
            OnTextChanged(sender, e);
        }
    }

    /// <summary>
    /// Raises <see cref="TextChanged"/> and the internal <see cref="IValueable.ValueChanged"/> event, then clears the pending flag.
    /// </summary>
    protected virtual void OnTextChanged(object sender, ValueEventArgs<string> e)
    {
        TextChanged?.Invoke(sender, e);
        _valueChanged?.Invoke(sender, e);
        _textHasChanged = false;
    }

    /// <inheritdoc/>
    public override void Invalidate(VisualPropertyInvalidateModes modes, InvalidateReason? reason = null)
    {
        if (modes != VisualPropertyInvalidateModes.None)
        {
            _rendered = false;
        }
        base.Invalidate(modes, reason);
    }

    /// <inheritdoc/>
    protected internal override void IsFocusedChanged(bool newValue)
    {
        base.IsFocusedChanged(newValue);
        if (!newValue)
        {
            HideCaret();
            if (_textHasChanged)
            {
                OnTextChanged(this, new ValueEventArgs<string>(Text));
                _textHasChanged = false;
            }
        }
        else if (TextChangedTrigger == EventTrigger.LostFocus)
        {
            _textHasChanged = false;
        }
    }

    /// <inheritdoc/>
    protected override void IsMouseOverChanged(bool newValue)
    {
        base.IsMouseOverChanged(newValue);
        var hover = HoverForegroundBrush;
        if (hover != null)
        {
            Invalidate(VisualPropertyInvalidateModes.Render, new PropertyInvalidateReason(IsMouseOverProperty));
        }

        Cursor = newValue && IsEditable && IsEnabled ? Cursor.IBeam : null;
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        _rendered = false;
        if (property == TextProperty)
        {
            DoOnTextChanged(this, new ValueEventArgs<string>((string)value!));
            return true;
        }

        if (property == IsEditableProperty)
        {
            if (IsEditable)
            {
                Edit();
                IsFocusable = true;
            }
            else
            {
                _selecting = false;
                StopEdit();
            }
            return true;
        }

        if (property == IsEnabledProperty)
        {
            if (IsEnabled)
            {
                ShowCaret();
            }
            else
            {
                HideCaret();
            }
            return true;
        }

        if (property == FontSizeProperty || property == FontFamilyNameProperty || property == FontCollectionProperty ||
            property == ParagraphAlignmentProperty || property == AlignmentProperty || property == FlowDirectionProperty ||
            property == ReadingDirectionProperty || property == WordWrappingProperty || property == TrimmingGranularityProperty ||
            property == PasswordCharProperty || property == IsLastLineWrappingEnabledProperty || property == LineSpacingProperty ||
            property == OpticalAlignmentProperty || property == VerticalGlyphOrientationProperty)
        {
            Interlocked.Exchange(ref _layout, null)?.Dispose();
        }
        return true;
    }

    /// <inheritdoc/>
    protected override void OnFocusedChanged(object? sender, ValueEventArgs<bool> e)
    {
        base.OnFocusedChanged(sender, e);

        if (IsFocused)
        {
            ShowCaret();
            SetCaretLocation();
        }
        else
        {
            _selecting = false;
            HideCaret();
        }
    }

    private void SetCaretLocation() => DoWhenRendered(() => Window?.RunTaskOnMainThread(() => DoSetCaretLocation()));

    /// <summary>
    /// Positions the shared window caret visual to match the current text insertion point.
    /// </summary>
    protected virtual void DoSetCaretLocation()
    {
        if (!IsFocused || !IsEditable || !IsEnabled)
            return;

        var caret = Window?.Caret;
        if (caret != null)
        {
            var padding = Padding;
            var rc = GetCaretRect();
            rc.Move(_origin);
            caret.Width = rc.Width;
            caret.Height = rc.Height;

            var ar = AbsoluteRenderRect;
            var x = ar.left + rc.left;
            var y = ar.top + rc.top;
            if (padding.left.IsSet() && padding.left > 0)
            {
                x += padding.left;
            }

            if (padding.top.IsSet() && padding.top > 0)
            {
                y += padding.top;
            }

            caret.Location = new D2D_POINT_2F(x, y);
            caret.IsVisible = true;
            Window?.NativeIfCreated?.SetImmCompositionWindowPosition(new POINT(x, y));
        }
    }

    /// <summary>Shows the caret when focused, editable, and enabled.</summary>
    protected virtual void ShowCaret()
    {
        if (IsFocused && IsEditable && IsEnabled)
        {
            var caret = Window?.Caret;
            caret?.IsShown = true;
        }
    }

    /// <summary>Hides the caret.</summary>
    protected virtual void HideCaret()
    {
        var caret = Window?.Caret;
        caret?.IsShown = false;
    }

    /// <summary>Stops edit mode (currently hides the caret).</summary>
    protected virtual void StopEdit() => HideCaret();

    /// <summary>Starts edit mode (shows caret and syncs caret formatting).</summary>
    protected virtual void Edit()
    {
        ShowCaret();
        UpdateCaretFormatting();
    }

    DWRITE_TEXT_ALIGNMENT ITextFormat.Alignment
    {
        get
        {
            // handle the justified + no wrap => infinity case https://stackoverflow.com/a/7370330/403671
            var aligment = Alignment;
            if (aligment != DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED)
                return aligment;

            if (WordWrapping != DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP)
                return aligment;

            // go back to default
            return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING;
        }
        set => Alignment = value;
    }

    void IPasswordCapable.SetPasswordCharacter(char character) => PasswordCharacter = character;

    /// <inheritdoc/>
    bool IPasswordCapable.IsPasswordModeEnabled
    {
        get => PasswordCharacter.HasValue;
        set
        {
            if (value)
            {
                if (PasswordCharacter.HasValue)
                    return;

                PasswordCharacter = '●'; // ○ ◌‬ ● ◯ ❍ ✪
                return;
            }
            PasswordCharacter = null;
        }
    }

    /// <summary>
    /// Tests whether a window-space position falls on a given <paramref name="range"/> within the layout.
    /// </summary>
    public bool IsPositionOverRange(D2D_POINT_2F position, DWRITE_TEXT_RANGE range) => IsPositionOverRange(position.x, position.y, range);

    /// <summary>
    /// Tests whether a window-space position falls on a given <paramref name="range"/> within the layout.
    /// </summary>
    public bool IsPositionOverRange(float x, float y, DWRITE_TEXT_RANGE range)
    {
        var ht = HitTestPoint(x, y, out _, out var inside);
        return inside && ht != null && range.Contains(ht.Value.textPosition);
    }

    /// <summary>
    /// Hit-tests a window-space point into the text layout and returns the metrics at that point.
    /// </summary>
    public DWRITE_HIT_TEST_METRICS? HitTestPoint(float x, float y) => HitTestPoint(x, y, out _, out _);

    /// <summary>
    /// Hit-tests a window-space point into the text layout and returns the metrics and hit information.
    /// </summary>
    public DWRITE_HIT_TEST_METRICS? HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
    {
        isTrailingHit = false;
        isInside = false;
        var layout = CheckLayout(false);
        if (layout == null)
            return null;

        layout.Object.HitTestPoint(x, y, out var trailingHit, out var inside, out var metrics).ThrowOnError();
        isTrailingHit = trailingHit;
        isInside = inside;
        return metrics;
    }

    internal IComObject<IDWriteTextFormat> GetFormat()
    {
        var format = Application.CurrentResourceManager.GetTextFormat(GetWindowTheme(), this)!;
        return format;
    }

    /// <summary>
    /// Builds or retrieves a cached DirectWrite text layout sized to <paramref name="maxWidth"/> x <paramref name="maxHeight"/>.
    /// </summary>
    protected virtual IComObject<IDWriteTextLayout>? GetLayout(float maxWidth, float maxHeight)
    {
#if DEBUG
#if NETFRAMEWORK
        if (maxWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth));

        if (maxHeight < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHeight));
#else
        ArgumentOutOfRangeException.ThrowIfNegative(maxWidth);
        ArgumentOutOfRangeException.ThrowIfNegative(maxHeight);
#endif
#endif
        if (float.IsPositiveInfinity(maxWidth))
        {
            maxWidth = float.MaxValue;
        }
        else
        {
            maxWidth = Math.Max(0, maxWidth - _origin.x);
        }

        if (float.IsPositiveInfinity(maxHeight))
        {
            maxHeight = float.MaxValue;
        }
        else
        {
            maxHeight = Math.Max(0, maxHeight - _origin.y);
        }

        var text = PasswordText;
        if (_layout != null && !_layout.IsDisposed && maxWidth == _maxWidth && maxHeight == _maxHeight && text == _text)
            return _layout;

        _rendered = false;
        _layout?.Dispose();
        _layout = Application.CurrentResourceManager.CreateTextLayout(GetFormat(), text, 0, maxWidth, maxHeight);

        if (_layout.Object is IDWriteTextLayout2 layout2)
        {
            layout2.SetLastLineWrapping(IsLastLineWrappingEnabled).ThrowOnError();
            layout2.SetOpticalAlignment(OpticalAlignment).ThrowOnError();
            layout2.SetVerticalGlyphOrientation(VerticalGlyphOrientation).ThrowOnError();
            var ff = FontFallback;
            if (ff != null)
            {
                layout2.SetFontFallback(ff.Object).ThrowOnError();
            }

            var ls = LineSpacing;
            if (ls != null)
            {
                if (_layout.Object is IDWriteTextLayout3 layout3)
                {
                    layout3.SetLineSpacing(ls.Value).ThrowOnError();
                }
            }
        }

        ApplyRanges();
        _maxWidth = maxWidth;
        _maxHeight = maxHeight;
        _text = text;
        UpdateCaretFormatting();
        return _layout;
    }

    private void ApplyRanges()
    {
        if (_finalRanges == null)
            return;

        var layout = CheckLayout(false);
        if (layout == null)
            return;

        IDWriteTextLayout1? layout1 = null;
        if (_finalRanges.Any(p => p.Type == FontRangeType.PairKerning || p.Type == FontRangeType.CharacterSpacing))
        {
            layout1 = layout.Object as IDWriteTextLayout1;
        }

        foreach (var fr in _finalRanges)
        {
            switch (fr.Type)
            {
                case FontRangeType.FontWeight:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetFontWeight((DWRITE_FONT_WEIGHT)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.FontStretch:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetFontStretch((DWRITE_FONT_STRETCH)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.FontStyle:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetFontStyle((DWRITE_FONT_STYLE)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.LocaleName:
                    foreach (var range in fr.Ranges)
                    {
#if NETFRAMEWORK
                        layout.Object.SetLocaleName((string)range.Value, range.Range).ThrowOnError();
#else
                        layout.Object.SetLocaleName(PWSTR.From((string)range.Value!), range.Range).ThrowOnError();
#endif
                    }
                    break;

                case FontRangeType.Strikethrough:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetStrikethrough((bool)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.Underline:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetUnderline((bool)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.FontSize:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetFontSize((float)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.FontFamilyName:
                    foreach (var range in fr.Ranges)
                    {
#if NETFRAMEWORK
                        layout.Object.SetFontFamilyName((string)range.Value, range.Range).ThrowOnError();
#else
                        layout.Object.SetFontFamilyName(PWSTR.From((string)range.Value!), range.Range).ThrowOnError();
#endif
                    }
                    break;

                case FontRangeType.FontCollection:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetFontCollection((IDWriteFontCollection)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.DrawingEffect:
                    foreach (var range in fr.Ranges)
                    {
#if NETFRAMEWORK
                        layout.Object.SetDrawingEffect(range.Value, range.Range).ThrowOnError();
#else
                        ComObject.WithComInstance(range.Value, ptr =>
                        {
                            layout.Object.SetDrawingEffect(ptr, range.Range).ThrowOnError();
                        });
#endif
                    }
                    break;

                case FontRangeType.Typography:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetTypography((IDWriteTypography)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.InlineObject:
                    foreach (var range in fr.Ranges)
                    {
                        layout.Object.SetInlineObject((IDWriteInlineObject)range.Value!, range.Range).ThrowOnError();
                    }
                    break;

                case FontRangeType.SolidColor:
                    // needs device context
                    break;

                case FontRangeType.PairKerning:
                    if (layout1 != null)
                    {
                        foreach (var range in fr.Ranges)
                        {
                            layout1.SetPairKerning((bool)range.Value!, range.Range).ThrowOnError();
                        }
                    }
                    break;

                case FontRangeType.CharacterSpacing:
                    if (layout1 != null)
                    {
                        foreach (var range in fr.Ranges)
                        {
                            var cs = (CharacterSpacing)range.Value!;
                            layout1.SetCharacterSpacing(cs.leadingSpacing, cs.trailingSpacing, cs.minimumAdvanceWidth, range.Range).ThrowOnError();
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }

    private sealed class CharacterSpacing
    {
        public float leadingSpacing;
        public float trailingSpacing;
        public float minimumAdvanceWidth;
    }

    private void ApplyRanges(RenderContext context)
    {
        if (_finalRanges == null)
            return;

        var layout = CheckLayout(false);
        if (layout == null)
            return;

        foreach (var fr in _finalRanges)
        {
            switch (fr.Type)
            {
                case FontRangeType.SolidColor:
                    foreach (var range in fr.Ranges)
                    {
                        var color = (D3DCOLORVALUE)range.Value!;
                        using var brush = context.DeviceContext?.CreateSolidColorBrush(color);
                        if (brush != null)
                        {
#if NETFRAMEWORK
                            layout.Object.SetDrawingEffect(brush.Object, range.Range).ThrowOnError();
#else
                            ComObject.WithComInstance(brush, unk =>
                            {
                                layout.Object.SetDrawingEffect(unk, range.Range).ThrowOnError();
                            });
#endif
                        }
                    }
                    break;
            }
        }
    }

    internal D2D_SIZE_F MeasureWithPadding(D2D_SIZE_F constraint, Func<D2D_SIZE_F, D2D_SIZE_F> action)
    {
        var padding = Padding;
        var leftPadding = padding.left.IsSet() && padding.left > 0;
        if (leftPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.left);
        }
        var topPadding = padding.top.IsSet() && padding.top > 0;
        if (topPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.top);
        }
        var rightPadding = padding.right.IsSet() && padding.right > 0;
        if (rightPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.right);
        }
        var bottomPadding = padding.bottom.IsSet() && padding.bottom > 0;
        if (bottomPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.bottom);
        }

        var size = action(constraint);
        // note: always return integral w & h when computed by dwrite
        var width = size.width.Ceiling();
        var height = size.height.Ceiling();

        if (IsEditable && IsFocused)
        {
            var caret = Window?.Caret;
            if (caret != null)
            {
                var rc = GetCaretRect();
                caret.Width = rc.Width;
                caret.Height = rc.Height;
            }

            if (caret?.Width.IsSet() == true)
            {
                width = Math.Max(width, caret.Width);
            }

            if (caret?.Height.IsSet() == true)
            {
                height = Math.Max(height, caret.Height);
            }
        }

        if (leftPadding)
        {
            width += padding.left;
        }

        if (topPadding)
        {
            height += padding.top;
        }

        if (rightPadding)
        {
            width += padding.right;
        }

        if (bottomPadding)
        {
            height += padding.bottom;
        }

        return new D2D_SIZE_F(width, height);
    }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => MeasureWithPadding(constraint, c =>
    {
        var layout = GetLayout(c.width, c.height);
        if (layout == null)
            throw new InvalidOperationException();

        var metrics = layout.GetMetrics1();
#if NETFRAMEWORK
        return new D2D_SIZE_F(metrics.widthIncludingTrailingWhitespace, metrics.height);
#else
        return new D2D_SIZE_F(metrics.Base.widthIncludingTrailingWhitespace, metrics.Base.height);
#endif
    });

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect) => SetCaretLocation();

    private IComObject<ID2D1Brush> GetSelectionBrush(RenderContext context, IComObject<ID2D1Brush> brush)
    {
        var selection = SelectionBrush?.GetBrush(context);
        if (selection != null)
            return selection;

        if (brush == null || brush.Object is not ID2D1SolidColorBrush colorBrush)
            return context.CreateSolidColorBrush(GetWindowTheme().TextBoxSelectionColor);

        var bg = AscendantsBackgroundColor;
        if (!bg.HasValue)
            return context.CreateSolidColorBrush(GetWindowTheme().TextBoxSelectionColor);

#if NETFRAMEWORK
        colorBrush.GetColor(out var color);
#else
        var color = colorBrush.GetColor();
#endif

        // get hue (angle) in the "middle" of two other colors
        var backHsl = Hsl.From(bg.Value);
        var foreHsl = Hsl.From(color);

        var angle1 = backHsl.Hue - foreHsl.Hue;
        if (angle1 < 0)
        {
            angle1 += 360;
        }

        var angle2 = foreHsl.Hue - backHsl.Hue;
        if (angle2 < 0)
        {
            angle2 += 360;
        }

        float angle;
        if (angle1 > angle2)
        {
            if (backHsl.Hue < foreHsl.Hue)
            {
                angle = foreHsl.Hue + angle1 / 2;
            }
            else
            {
                angle = backHsl.Hue + angle1 / 2;
            }
        }
        else
        {
            if (foreHsl.Hue < backHsl.Hue)
            {
                angle = backHsl.Hue + angle2 / 2;
            }
            else
            {
                angle = foreHsl.Hue + angle2 / 2;
            }
        }

        if (angle < 0)
        {
            angle += 360;
        }
        else if (angle >= 360)
        {
            angle -= 360;
        }

        var hsl = new Hsl(angle, Math.Max(foreHsl.Saturation, backHsl.Saturation), (foreHsl.Brightness + backHsl.Brightness) / 2);
        var middle = hsl.ToD3DCOLORVALUE();

        return context.CreateSolidColorBrush(middle);
    }

    /// <inheritdoc/>
    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);

        IComObject<ID2D1Brush>? brush;
        if (IsMouseOver)
        {
            brush = HoverForegroundBrush?.GetBrush(context) ?? ForegroundBrush?.GetBrush(context);
        }
        else
        {
            brush = ForegroundBrush?.GetBrush(context);
        }

        if (brush == null)
        {
            //Application.Trace(this + " has no brush defined.");
            return;
        }

        var padding = Padding;
        var origin = _origin;
        var rr = RelativeRenderRect.Size;
        var clip = new D2D_RECT_F(rr);

        if (padding.left.IsSet() && padding.left > 0)
        {
            origin.x += padding.left;
            rr.width = Math.Max(0, rr.width - padding.left);
            clip.left += padding.left;
        }

        if (padding.top.IsSet() && padding.top > 0)
        {
            origin.y += padding.top;
            rr.height = Math.Max(0, rr.height - padding.top);
            clip.top += padding.top;
        }

        if (padding.right.IsSet() && padding.right > 0)
        {
            rr.width = Math.Max(0, rr.width - padding.right);
            clip.right = Math.Max(clip.left, clip.right - padding.right);
        }

        if (padding.bottom.IsSet() && padding.bottom > 0)
        {
            rr.height = Math.Max(0, rr.height - padding.bottom);
            clip.bottom = Math.Max(clip.top, clip.bottom - padding.bottom);
        }

        var layout = GetLayout(rr.width, rr.height);
        if (layout == null)
            return;

        ApplyRanges(context);
        var clipText = ClipText;
        if (clipText)
        {
            context.DeviceContext.PushAxisAlignedClip(clip);
        }

        try
        {
            // draw selection
            var caretRange = GetSelectionRange();
            if (caretRange.length > 0)
            {
#if NETFRAMEWORK
                layout.Object.HitTestTextRange(
                      caretRange.startPosition,
                      caretRange.length,
                      origin.x,
                      origin.y,
                      null,
                      0,
                      out var actualHitTestCount
                      ); // no error check
#else
                layout.Object.HitTestTextRange(
                    caretRange.startPosition,
                    caretRange.length,
                    origin.x,
                    origin.y,
                    0,
                    0,
                    out var actualHitTestCount
                    ); // no error check
#endif

                if (actualHitTestCount > 0)
                {
                    var hitTestMetrics = new DWRITE_HIT_TEST_METRICS[actualHitTestCount];
#if NETFRAMEWORK
                    layout.Object.HitTestTextRange(
                        caretRange.startPosition,
                        caretRange.length,
                        origin.x,
                        origin.y,
                        hitTestMetrics,
                        hitTestMetrics.Length,
                        out _
                        ).ThrowOnError();
#else
                    layout.Object.HitTestTextRange(
                        caretRange.startPosition,
                        caretRange.length,
                        origin.x,
                        origin.y,
                        hitTestMetrics.AsPointer(),
                        hitTestMetrics.Length(),
                        out _
                        ).ThrowOnError();
#endif

                    // Note that an ideal layout will return fractional values, so you may see slivers between the selection ranges,
                    // due to the per-primitive antialiasing of the edges unless it is disabled (better for performance anyway).
                    context.DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
                    foreach (var htm in hitTestMetrics)
                    {
                        var highlightRect = new D2D_RECT_F(htm.left, htm.top, htm.left + htm.width, htm.top + htm.height);
                        context.DeviceContext.Object.FillRectangle(highlightRect, GetSelectionBrush(context, brush).Object);
                    }

                    context.DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
                }
            }

            var trpMode = false;
            var trp = TextRenderingParameters;
            if (trp != null && Window != null)
            {
                trpMode = trp.Mode.HasValue;
                trp.Set(Window.MonitorHandle, context.DeviceContext.Object);
            }

            var options = DrawOptions;
            var aa = AntiAliasingMode;
            if (!trpMode && aa != D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_DEFAULT)
            {
                context.DeviceContext.WithTextAntialiasMode(aa, () =>
                {
                    context.DeviceContext.DrawTextLayout(origin, layout.Object, brush.Object, options);
                });
            }
            else
            {
                context.DeviceContext.DrawTextLayout(origin, layout.Object, brush.Object, options);
            }
        }
        finally
        {
            if (clipText)
            {
                context.DeviceContext.PopAxisAlignedClip();
            }
        }
        _rendered = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyUp(object? sender, KeyEventArgs e)
    {
        base.OnKeyUp(sender, e);
        if (IsFocused && IsEditable)
        {
            Window?.Caret?.StartBlinking();
        }
    }

    private void PostponeCaret()
    {
        if (IsFocused && IsEditable)
        {
            var caret = Window?.Caret;
            if (caret != null)
            {
                caret.IsShown = true;
                caret.StartBlinking();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(sender, e);
        var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        var control = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        var key = e.Key;

        if (!IsEnabled || !IsFocused)
            return;

        // paste is special as it uses an STA thread first
        if (IsEditable && ((control && key == VIRTUAL_KEY.VK_V) || (shift && key == VIRTUAL_KEY.VK_INSERT)))
        {
            PasteFromClipboard();
            PostponeCaret();
            e.Handled = true;
            return;
        }

        if (key == VIRTUAL_KEY.VK_LEFT || key == VIRTUAL_KEY.VK_RIGHT || key == VIRTUAL_KEY.VK_UP || key == VIRTUAL_KEY.VK_DOWN ||
            key == VIRTUAL_KEY.VK_HOME || key == VIRTUAL_KEY.VK_END || key == VIRTUAL_KEY.VK_PRIOR || key == VIRTUAL_KEY.VK_NEXT ||
            key == VIRTUAL_KEY.VK_C || key == VIRTUAL_KEY.VK_X || key == VIRTUAL_KEY.VK_A ||
            key == VIRTUAL_KEY.VK_Y || key == VIRTUAL_KEY.VK_Z ||
            (key == VIRTUAL_KEY.VK_TAB && AcceptsTab) || (key == VIRTUAL_KEY.VK_RETURN && (AcceptsReturn || TextChangedTrigger == EventTrigger.LostFocusOrReturnPressed)) ||
            key == VIRTUAL_KEY.VK_BACK || key == VIRTUAL_KEY.VK_DELETE || key == VIRTUAL_KEY.VK_INSERT)
        {
            OnKeyDown(e, shift, control);
            if (e.Handled)
            {
                PostponeCaret();
            }
        }
    }

    private void OnKeyDown(KeyEventArgs e, bool shift, bool control)
    {
        var absolutePosition = _charPosition + _charPositionOffset;

        switch (e.Key)
        {
            case VIRTUAL_KEY.VK_LEFT:
                if (SetSelection(control ? TextBoxSetSelection.LeftWord : TextBoxSetSelection.Left, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_RIGHT:
                if (SetSelection(control ? TextBoxSetSelection.RightWord : TextBoxSetSelection.Right, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_UP:
                if (SetSelection(TextBoxSetSelection.Up, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_DOWN:
                if (SetSelection(TextBoxSetSelection.Down, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_PRIOR:
                if (SetSelection(TextBoxSetSelection.PageUp, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_NEXT:
                if (SetSelection(TextBoxSetSelection.PageDown, 1, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_HOME:
                if (SetSelection(control ? TextBoxSetSelection.First : TextBoxSetSelection.Home, 0, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_END:
                if (SetSelection(control ? TextBoxSetSelection.Last : TextBoxSetSelection.End, 0, shift))
                {
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_C:
                if (control)
                {
                    CopyToClipboard();
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_X:
                if (control)
                {
                    CopyToClipboard();
                    if (IsEditable)
                    {
                        DeleteSelection();
                    }
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_A:
                if (control)
                {
                    SetSelection(TextBoxSetSelection.All, 0, true);
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_Y:
                if (control)
                {
                    Redo();
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_Z:
                if (control)
                {
                    Undo();
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_TAB:
                if (AcceptsTab && !control) // ctrl + tab is used to change focus
                {
                    DeleteSelection();
                    InsertTextAt(absolutePosition, "\t", _caretFormat);
                    SetSelection(TextBoxSetSelection.AbsoluteLeading, absolutePosition + 1, false, false);
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_INSERT:
                if (control)
                {
                    CopyToClipboard();
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_RETURN:
                if (IsEditable)
                {
                    if (!AcceptsReturn)
                    {
                        if (_textHasChanged && TextChangedTrigger == EventTrigger.LostFocusOrReturnPressed)
                        {
                            OnTextChanged(this, new ValueEventArgs<string>(Text));
                            _textHasChanged = false;
                        }
                        break;
                    }

                    // Insert CR/LF pair
                    DeleteSelection();
                    InsertTextAt(absolutePosition, Environment.NewLine, _caretFormat);
                    SetSelection(TextBoxSetSelection.AbsoluteLeading, (uint)(absolutePosition + Environment.NewLine.Length), false, false);
                    e.Handled = true;
                }
                break;

            case VIRTUAL_KEY.VK_BACK:
                if (!IsEditable)
                    break;

                // Erase back one character (less than a character though).
                // Since layout's hit-testing always returns a whole cluster, we do the surrogate pair detection here directly.
                // Otherwise there would be no way to delete just the diacritic following a base character.

                if (absolutePosition != _charAnchor)
                {
                    // delete the selected text
                    DeleteSelection();
                }
                else if (absolutePosition > 0)
                {
                    var text = Text ?? string.Empty;
                    uint count = 1;

                    do
                    {
                        // need special case for surrogate pairs and CR/LF pair.
                        var pos = (int)(absolutePosition - count);
                        if ((pos - 1) >= 0 && pos < text.Length)
                        {
                            var charBackOne = text[pos];
                            var charBackTwo = text[pos - 1];
                            if ((IsLowSurrogate(charBackOne) && IsHighSurrogate(charBackTwo)) || (charBackOne == '\n' && charBackTwo == '\r'))
                            {
                                count++;
                            }
                        }

                        // remove ZWJ https://en.wikipedia.org/wiki/Zero-width_joiner
                        var zwj = false;
                        pos = (int)(absolutePosition - count - 1);
                        while (pos >= 0 && pos < text.Length && text[pos] == 0x200D)
                        {
                            count++;
                            pos = (int)(absolutePosition - count - 1);
                            zwj = true;
                        }
                        if (!zwj)
                            break;

                        // continue to next char
                        count++;
                    }
                    while (true);

                    SetSelection(TextBoxSetSelection.LeftChar, count, false);
                    RemoveTextAt(_charPosition, count);
                }

                e.Handled = true;
                break;

            case VIRTUAL_KEY.VK_DELETE:
                if (!IsEditable)
                    break;

                // Delete following cluster.
                if (absolutePosition != _charAnchor)
                {
                    // Delete all the selected text.
                    DeleteSelection();
                }
                else
                {
                    // Get the size of the following cluster.
                    var layout = CheckLayout(false);
                    if (layout != null)
                    {
                        layout.Object.HitTestTextPosition(
                            absolutePosition,
                            false,
                            out _,
                            out _,
                            out var hitTestMetrics
                            ).ThrowOnError();

                        RemoveTextAt(hitTestMetrics.textPosition, hitTestMetrics.length);
                        SetSelection(TextBoxSetSelection.AbsoluteLeading, hitTestMetrics.textPosition, false);
                    }
                }

                e.Handled = true;
                break;
        }
    }

    // 0xD800 <= ch <= 0xDBFF
    private static bool IsHighSurrogate(uint ch) => (ch & 0xFC00) == 0xD800;

    // 0xDC00 <= ch <= 0xDFFF
    private static bool IsLowSurrogate(uint ch) => (ch & 0xFC00) == 0xDC00;

    private void SetOriginX(float x)
    {
#if DEBUG
        if (!x.IsSet())
            throw new ArgumentException(null, nameof(x));
#endif
        _origin.x = Math.Min(0, x);
    }

    private void SetOriginY(float y)
    {
#if DEBUG
        if (!y.IsSet())
            throw new ArgumentException(null, nameof(y));
#endif

        _origin.y = Math.Min(0, y);
    }

    private float GetFontSize() => Application.CurrentResourceManager.GetFontSize(GetWindowTheme(), FontSize);

    /// <inheritdoc/>
    protected override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        base.OnMouseWheel(sender, e);
        if (!IsEnabled || !IsWheelZoomEnabled)
            return;

        var control = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        if (control && shift)
        {
            var fsize = GetFontSize();
            var size = Math.Max(1, fsize + e.Delta);
            if (size != fsize)
            {
                e.Handled = true;
                FontSize = size;
            }
            return;
        }

        var offset = e.Delta * GetFontSize();
        e.Handled = true;
        if (control)
        {
            SetOriginX(_origin.x + offset);
        }
        else
        {
            SetOriginY(_origin.y + offset);
        }

        Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        base.OnMouseLeave(sender, e);
        _selecting = false;
    }

    /// <inheritdoc/>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        base.OnMouseButtonDown(sender, e);
        if (!IsEnabled)
            return;

        if (e.Button == MouseButton.Left)
        {
            _selecting = true;
            Window?.CaptureMouse(this);
            var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
            SetSelectionFromPoint(e, shift);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        base.OnMouseButtonUp(sender, e);
        if (e.Button == MouseButton.Left)
        {
            Window.ReleaseMouseCapture();
            _selecting = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(object? sender, MouseEventArgs e)
    {
        base.OnMouseMove(sender, e);
        if (!IsEnabled)
            return;

        if (_selecting)
        {
            SetSelectionFromPoint(e, true);
        }
    }

    private static bool HandleChar(KeyPressEventArgs e) =>
        //if (e.UTF32Character == '\t' && AcceptsTab)
        //    return true;

        e.UTF16Character >= ' ';

    /// <inheritdoc/>
    protected override void OnKeyPress(object? sender, KeyPressEventArgs e)
    {
        base.OnKeyPress(sender, e);
        if (!HandleChar(e))
            return;

        if (!IsEditable || !IsEnabled || !IsFocused)
            return;

        e.Handled = true;
        DeleteSelection();
        InsertTextAt(_charPosition + _charPositionOffset, new string(e.Characters), _caretFormat);
        SetSelection(TextBoxSetSelection.Right, (uint)e.Characters.Length, false, false);
        PostponeCaret();
    }

    /// <summary>
    /// Inserts text at the current caret or at an explicit <paramref name="position"/>.
    /// </summary>
    /// <param name="text">The text to insert.</param>
    /// <param name="position">Optional absolute character index. When null, uses caret position.</param>
    public void InsertText(string text, int? position = null)
    {
        if (!position.HasValue)
        {
            InsertTextAt(_charPosition + _charPositionOffset, text, _caretFormat);
            return;
        }

        if (position.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(position));

        InsertTextAt((uint)position.Value, text, _caretFormat);
    }

    /// <summary>
    /// Removes text starting at <paramref name="position"/> for <paramref name="lengthToRemove"/> characters
    /// (or to the end when null).
    /// </summary>
    public void RemoveText(int position, int? lengthToRemove = null)
    {
#if NETFRAMEWORK
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(position));
#else
        ArgumentOutOfRangeException.ThrowIfNegative(position);
#endif

        RemoveTextAt((uint)position, (uint?)lengthToRemove);
    }

    /// <summary>
    /// Changes selection/caret using one of the <see cref="TextBoxSetSelection"/> modes.
    /// </summary>
    /// <param name="mode">The movement/selection mode.</param>
    /// <param name="advance">Optional count used by some modes (e.g., characters to move).</param>
    /// <param name="extend">Whether to extend the selection (Shift-like behavior).</param>
    public void Select(TextBoxSetSelection mode, int? advance = 0, bool extend = false) => SetSelection(mode, (uint?)advance, extend);

    /// <summary>
    /// Undoes the last edit operation if available.
    /// </summary>
    public virtual bool Undo()
    {
        if (!_undoStack.TryUndo(UndoState.From(this), out var undo))
            return false;

        undo.Apply(this);
        return true;
    }

    /// <summary>
    /// Redoes the last undone edit operation if available.
    /// </summary>
    public virtual bool Redo()
    {
        if (!_undoStack.TryRedo(UndoState.From(this), out var redo))
            return false;

        redo.Apply(this);
        return true;
    }

    /// <summary>
    /// Copies the current selection to the clipboard if non-empty.
    /// </summary>
    public virtual void CopyToClipboard()
    {
        var text = GetSelectionText();
        if (string.IsNullOrEmpty(text))
            return;

        TaskUtilities.RunWithSTAThread(() => Clipboard.SetText(text));
    }

    /// <summary>
    /// Pastes text from the clipboard, honoring <see cref="AcceptsReturn"/> to optionally strip newlines.
    /// Replaces the current selection if any.
    /// </summary>
    public virtual void PasteFromClipboard()
    {
        var text = TaskUtilities.RunWithSTAThread(Clipboard.GetText).Result;
        if (text == null)
            return;

        if (!AcceptsReturn)
        {
            var pos = text.IndexOfAny(['\r', '\n']);
            if (pos == 0)
                return;

            if (pos > 0)
            {
#if NETFRAMEWORK
                text = text.Substring(0, pos);
#else
                text = text[..pos];
#endif
            }
        }

        DeleteSelection();
        InsertTextAt(_charPosition + _charPositionOffset, text);
        SetSelection(TextBoxSetSelection.RightChar, (uint)text.Length, false);
    }

    //private int MirrorXCoordinate(int x, float paddingRight)
    //{ ... }

    private void InsertTextAt(uint position, string textToInsert, CaretFormat? caretFormat = null)
    {
        var text = Text ?? string.Empty;
        var oldLayout = _layout;
        var oldTextLength = text.Length;
        position = (uint)Math.Min(position, text.Length);
        text = text.Insert((int)position, textToInsert);

        if (_layout != null)
        {
            var newLayout = RecreateLayout(text);

            CopyGlobalProperties(oldLayout!.Object, newLayout!.Object);

            // For each property, get the position range and apply it to the old text.
            if (position == 0)
            {
                // Inserted text
                CopySinglePropertyRange(oldLayout.Object, 0, newLayout.Object, 0, (uint)textToInsert.Length);

                // The rest of the text
                CopyRangedProperties(oldLayout.Object, 0, (uint)oldTextLength, (uint)textToInsert.Length, newLayout.Object);
            }
            else
            {
                // 1st block
                CopyRangedProperties(oldLayout.Object, 0, position, 0, newLayout.Object);

                // Inserted text
                CopySinglePropertyRange(oldLayout.Object, position - 1, newLayout.Object, position, (uint)textToInsert.Length, caretFormat);

                // Last block (if it exists)
                CopyRangedProperties(oldLayout.Object, position, (uint)oldTextLength, (uint)textToInsert.Length, newLayout.Object);
            }

            // Copy trailing end.
            CopySinglePropertyRange(oldLayout.Object, (uint)oldTextLength, newLayout.Object, (uint)text.Length, uint.MaxValue);

            oldLayout.Dispose();
            _layout = newLayout;
        }

        SetText(text);
    }

    private void RemoveTextAt(uint position, uint? length)
    {
        var text = Text ?? string.Empty;
        var oldLayout = _layout;
        var oldTextLength = text.Length;

        var lengthToRemove = length ?? (uint)(text.Length - position);
        text = text.Remove((int)position, (int)lengthToRemove);

        var newLayout = RecreateLayout(text);

        CopyGlobalProperties(oldLayout!.Object, newLayout!.Object);

        if (position == 0)
        {
            // The rest of the text
            CopyRangedProperties(oldLayout.Object, lengthToRemove, (uint)oldTextLength, lengthToRemove, newLayout.Object, true);
        }
        else
        {
            // 1st block
            CopyRangedProperties(oldLayout.Object, 0, position, 0, newLayout.Object, true);

            // Last block (if it exists, we increment past the deleted text)
            CopyRangedProperties(oldLayout.Object, position + lengthToRemove, (uint)oldTextLength, lengthToRemove, newLayout.Object, true);
        }
        CopySinglePropertyRange(oldLayout.Object, (uint)oldTextLength, newLayout.Object, (uint)text.Length, uint.MaxValue);

        oldLayout.Dispose();
        _layout = newLayout;
        SetText(text);
    }

    private IComObject<IDWriteTextLayout>? RecreateLayout(string text)
    {
        var layout = CheckLayout(true)!;
        var w = layout.Object.GetMaxWidth();
        var h = layout.Object.GetMaxHeight();
        return Application.CurrentResourceManager.GetTextLayout(layout.Object, text, maxWidth: w, maxHeight: h);
    }

    private static void CopyGlobalProperties(IDWriteTextLayout oldLayout, IDWriteTextLayout newLayout)
    {
        // Copies global properties that are not range based.
        newLayout.SetTextAlignment(oldLayout.GetTextAlignment());
        newLayout.SetParagraphAlignment(oldLayout.GetParagraphAlignment());
        newLayout.SetWordWrapping(oldLayout.GetWordWrapping());
        newLayout.SetReadingDirection(oldLayout.GetReadingDirection());
        newLayout.SetFlowDirection(oldLayout.GetFlowDirection());
        newLayout.SetIncrementalTabStop(oldLayout.GetIncrementalTabStop());

        oldLayout.GetTrimming(out var trimmingOptions, out var inlineObject);
        if (inlineObject != null)
        {
            newLayout.SetTrimming(trimmingOptions, inlineObject);
#if NETFRAMEWORK
            Marshal.ReleaseComObject(inlineObject);
#else
            inlineObject.FinalRelease();
#endif
        }

        oldLayout.GetLineSpacing(out var lineSpacingMethod, out var lineSpacing, out var baseline);
        newLayout.SetLineSpacing(lineSpacingMethod, lineSpacing, baseline);

        if (oldLayout is IDWriteTextLayout2 layout2 && newLayout is IDWriteTextLayout2 newLayout2)
        {
            newLayout2.SetLastLineWrapping(layout2.GetLastLineWrapping());
            newLayout2.SetOpticalAlignment(layout2.GetOpticalAlignment());
            newLayout2.SetVerticalGlyphOrientation(layout2.GetVerticalGlyphOrientation());
            layout2.GetFontFallback(out var fontFallback);
            if (fontFallback != null)
            {
                newLayout2.SetFontFallback(fontFallback);
#if NETFRAMEWORK
                Marshal.ReleaseComObject(fontFallback);
#else
                fontFallback.FinalRelease();
#endif
            }

            if (oldLayout is IDWriteTextLayout3 layout3 && newLayout is IDWriteTextLayout3 newLayout3)
            {
                if (layout3.GetLineSpacing(out var spacing).IsSuccess)
                {
                    newLayout3.SetLineSpacing(spacing);
                }
            }
        }
    }

    private static uint CalculateRangeLengthAt(IDWriteTextLayout layout, uint pos)
    {
        // Determines the length of a block of similarly formatted properties.
        // Use the first getter to get the range to increment the current position.
        var incrementAmount = new DWRITE_TEXT_RANGE(pos, 1);
        unsafe
        {
            layout.GetFontWeight(pos, out _, (nint)(&incrementAmount));
        }
        return incrementAmount.length - (pos - incrementAmount.startPosition);
    }

    private void CopyRangedProperties(IDWriteTextLayout oldLayout, uint startPos, uint endPos, uint newLayoutTextOffset, IDWriteTextLayout newLayout, bool isOffsetNegative = false)
    {
        // Copies properties that set on ranges.
        var currentPos = startPos;
        while (currentPos < endPos)
        {
            var rangeLength = CalculateRangeLengthAt(oldLayout, currentPos);
            rangeLength = Math.Min(rangeLength, endPos - currentPos);
            if (isOffsetNegative)
            {
                CopySinglePropertyRange(oldLayout, currentPos, newLayout, currentPos - newLayoutTextOffset, rangeLength);
            }
            else
            {
                CopySinglePropertyRange(oldLayout, currentPos, newLayout, currentPos + newLayoutTextOffset, rangeLength);
            }
            currentPos += rangeLength;
        }
    }

    private void CopySinglePropertyRange(IDWriteTextLayout oldLayout, uint startPosForOld, IDWriteTextLayout newLayout, uint startPosForNew, uint length, CaretFormat? caretFormat = null)
    {
        // Copies a single range of similar properties, from one old layout to a new one.
        var range = new DWRITE_TEXT_RANGE(startPosForNew, Math.Min(length, uint.MaxValue - startPosForNew));

        // font collection
        if (oldLayout.GetFontCollection(startPosForOld, out var fontCollection, IntPtr.Zero).IsSuccess)
        {
            newLayout.SetFontCollection(fontCollection, range);
#if NETFRAMEWORK
            Marshal.ReleaseComObject(fontCollection);
#else
            fontCollection.FinalRelease();
#endif
        }

        if (caretFormat != null)
        {
#if NETFRAMEWORK
            newLayout.SetFontFamilyName(caretFormat.fontFamilyName, range);
            newLayout.SetLocaleName(caretFormat.localeName, range);
#else
            newLayout.SetFontFamilyName(PWSTR.From(caretFormat.fontFamilyName), range);
            newLayout.SetLocaleName(PWSTR.From(caretFormat.localeName), range);
#endif
            newLayout.SetFontWeight(caretFormat.fontWeight, range);
            newLayout.SetFontStyle(caretFormat.fontStyle, range);
            newLayout.SetFontStretch(caretFormat.fontStretch, range);
            newLayout.SetFontSize(caretFormat.fontSize, range);
            newLayout.SetUnderline(caretFormat.hasUnderline, range);
            newLayout.SetStrikethrough(caretFormat.hasStrikethrough, range);

            if (newLayout is IDWriteTextLayout2 newLayout2)
            {
                newLayout2.SetOpticalAlignment(caretFormat.opticalAlignment);
                newLayout2.SetLastLineWrapping(caretFormat.isLastLineWrapping);
                newLayout2.SetVerticalGlyphOrientation(caretFormat.verticalGlyphOrientation);
                if (caretFormat.lineSpacing != null)
                {
                    if (newLayout is IDWriteTextLayout3 newLayout3)
                    {
                        newLayout3.SetLineSpacing(caretFormat.lineSpacing.Value);
                    }
                }
            }
        }
        else
        {
            // font family
            var fontFamilyName = oldLayout.GetFontFamilyName(startPosForOld).Nullify();
            if (fontFamilyName != null)
            {
#if NETFRAMEWORK
                newLayout.SetFontFamilyName(fontFamilyName, range);
#else
                newLayout.SetFontFamilyName(PWSTR.From(fontFamilyName), range);
#endif
            }

            // weight/width/slope
            if (oldLayout.GetFontWeight(startPosForOld, out var weight, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetFontWeight(weight, range);
            }

            if (oldLayout.GetFontStyle(startPosForOld, out var style, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetFontStyle(style, range);
            }

            if (oldLayout.GetFontStretch(startPosForOld, out var stretch, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetFontStretch(stretch, range);
            }

            // font size
            if (oldLayout.GetFontSize(startPosForOld, out var fontSize, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetFontSize(fontSize, range);
            }
            else
            {
                newLayout.SetFontSize(GetFontSize(), range);
            }

            // underline and strikethrough
            if (oldLayout.GetUnderline(startPosForOld, out var value, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetUnderline(value, range);
            }

            if (oldLayout.GetStrikethrough(startPosForOld, out value, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetStrikethrough(value, range);
            }

            // locale
            var locale = oldLayout.GetLocaleName(startPosForOld).Nullify();
            if (locale != null)
            {
#if NETFRAMEWORK
                newLayout.SetLocaleName(locale, range);
#else
                newLayout.SetLocaleName(PWSTR.From(locale), range);
#endif
            }
        }

        // drawing effect
        if (oldLayout.GetDrawingEffect(startPosForOld, out var drawingEffect, IntPtr.Zero).IsSuccess)
        {
            newLayout.SetDrawingEffect(drawingEffect, range);
#if NETFRAMEWORK
            if (drawingEffect != null)
            {
                Marshal.ReleaseComObject(drawingEffect);
            }
#else
            if (drawingEffect != 0)
            {
                Marshal.Release(drawingEffect);
            }
#endif
        }

        // inline object
        if (oldLayout.GetInlineObject(startPosForOld, out var inlineObject, IntPtr.Zero).IsSuccess)
        {
            newLayout.SetInlineObject(inlineObject, range);
#if NETFRAMEWORK
            if (inlineObject != null)
            {
                Marshal.ReleaseComObject(inlineObject);
            }
#else
            inlineObject.FinalRelease();
#endif
        }

        // typography
        if (oldLayout.GetTypography(startPosForOld, out var typography, IntPtr.Zero).IsSuccess)
        {
            newLayout.SetTypography(typography, range);
#if NETFRAMEWORK
            if (typography != null)
            {
                Marshal.ReleaseComObject(typography);
            }
#else
            typography.FinalRelease();
#endif
        }
    }

    private void DeleteSelection()
    {
        var selectionRange = GetSelectionRange();
        if (selectionRange.length <= 0)
            return;

        RemoveTextAt(selectionRange.startPosition, selectionRange.length);
        SetSelection(TextBoxSetSelection.AbsoluteLeading, selectionRange.startPosition, false);
    }

    // x and y are client coords
    private bool SetSelectionFromPoint(MouseEventArgs e, bool extendSelection)
    {
        var layout = CheckLayout(false);
        if (layout == null)
            return false;

        // Returns the text position corresponding to the mouse x,y.
        // If hitting the trailing side of a cluster, return the leading edge of the following text position.

        // Remap display coordinates to actual.
        var pos = e.GetPosition(this);

        var padding = Padding;
        pos.y -= (int)padding.top;

        // MirrorXCoordinate
        // On RTL builds, coordinates may need to be restored to or converted from Cartesian coordinates, where x increases positively to the right.
        var style = (Window?.ExtendedStyle).GetValueOrDefault();
        if (style.HasFlag(WINDOW_EX_STYLE.WS_EX_LAYOUTRTL))
        {
            var rect = Window!.ClientRect;
            pos.x = (int)(rect.right - pos.x - 1 - padding.right);
        }
        else
        {
            pos.x -= (int)padding.left;
        }

        pos.x -= (int)_origin.x;
        pos.y -= (int)_origin.y;

        layout.Object.HitTestPoint(
            pos.x,
            pos.y,
            out var isTrailingHit,
            out _,
            out var caretMetrics
            ).ThrowOnError();

        // Update current selection according to click or mouse drag.
        return SetSelection(isTrailingHit ? TextBoxSetSelection.AbsoluteTrailing : TextBoxSetSelection.AbsoluteLeading, caretMetrics.textPosition, extendSelection);
    }

    private void SetText(string text)
    {
        var state = UndoState.From(this);
        _undoStack.Do(state);
        Text = text;
    }

    private bool SetSelection(TextBoxSetSelection moveMode, uint? advance, bool extendSelection, bool updateCaretFormat = true)
    {
        CheckRunningAsMainThread();
        // Moves the caret relatively or absolutely, optionally extending the selection range (for example, when shift is held).
        uint line;// = uint.MaxValue; // current line number, needed by a few modes
        uint absolutePosition = _charPosition + _charPositionOffset;
        uint oldAbsolutePosition = absolutePosition;
        uint oldCaretAnchor = _charAnchor;
        DWRITE_HIT_TEST_METRICS hitTestMetrics;
        DWRITE_LINE_METRICS[] lineMetrics;
#if NETFRAMEWORK
        bool isTrailingHit;
#else
        BOOL isTrailingHit;
#endif

        var text = RenderedText ?? string.Empty;
        switch (moveMode)
        {
            case TextBoxSetSelection.Left:
                _charPosition += _charPositionOffset;
                if (_charPosition > 0)
                {
                    --_charPosition;
                    AlignCaretToNearestCluster(false, true);

                    // special check for CR/LF pair
                    absolutePosition = _charPosition + _charPositionOffset;
                    if (absolutePosition >= 1
                        && absolutePosition < text.Length
                        && text[(int)(absolutePosition - 1)] == '\r'
                        && text[(int)absolutePosition] == '\n')
                    {
                        _charPosition = absolutePosition - 1;
                        AlignCaretToNearestCluster(false, true);
                    }
                }
                break;

            case TextBoxSetSelection.Right:
                _charPosition = absolutePosition;
                AlignCaretToNearestCluster(true, true);

                // special check for CR/LF pair
                absolutePosition = _charPosition + _charPositionOffset;
                if (absolutePosition >= 1
                    && absolutePosition < text.Length
                    && text[(int)(absolutePosition - 1)] == '\r'
                    && text[(int)absolutePosition] == '\n')
                {
                    _charPosition = absolutePosition + 1;
                    AlignCaretToNearestCluster(false, true);
                }
                break;

            case TextBoxSetSelection.LeftChar:
                _charPosition = absolutePosition;
                _charPosition -= Math.Min(advance ?? 0, absolutePosition);
                _charPositionOffset = 0;
                break;

            case TextBoxSetSelection.RightChar:
                _charPosition = absolutePosition + (advance ?? 0);
                _charPositionOffset = 0;
                {
                    // Use hit-testing to limit text position.
                    var layout1 = CheckLayout(false);
                    if (layout1 != null)
                    {
                        layout1.Object.HitTestTextPosition(_charPosition, false, out _, out _, out hitTestMetrics).ThrowOnError();
                        _charPosition = Math.Min(_charPosition, hitTestMetrics.textPosition + hitTestMetrics.length);
                    }
                }
                break;

            case TextBoxSetSelection.Up:
            case TextBoxSetSelection.Down:
                // Retrieve the line metrics to figure out what line we are on.
                lineMetrics = GetLineMetrics();

                GetLineFromPosition(lineMetrics, _charPosition, out line, out var linePosition);

                // Move up a line or down
                if (moveMode == TextBoxSetSelection.Up)
                {
                    if (line <= 0)
                        break; // already top line

                    line--;
                    linePosition -= lineMetrics[line].length;
                }
                else
                {
                    linePosition += lineMetrics[line].length;
                    line++;
                    if (line >= lineMetrics.Length)
                        break; // already bottom line
                }

                // To move up or down, we need three hit-testing calls to determine:
                // 1. The x of where we currently are.
                // 2. The y of the new line.
                // 3. New text position from the determined x and y.
                // This is because the characters are variable size.

                // Get x of current text position
                var layout = CheckLayout(false);
                if (layout != null)
                {
                    layout.Object.HitTestTextPosition(
                        _charPosition,
                        _charPositionOffset > 0, // trailing if nonzero, else leading edge
                        out float caretX,
                        out _,
                        out _
                        ).ThrowOnError();

                    // Get y of new position
                    layout.Object.HitTestTextPosition(
                        linePosition,
                        false, // leading edge
                        out _,
                        out float caretY,
                        out _
                        ).ThrowOnError();

                    // Now get text position of new x,y.
                    layout.Object.HitTestPoint(
                        Math.Max(caretX, _lastCaretX), // use last horizontal caret position (like many editors, not notepad)
                        caretY,
                        out isTrailingHit,
                        out _,
                        out hitTestMetrics
                        ).ThrowOnError();

                    _charPosition = hitTestMetrics.textPosition;
                    _charPositionOffset = (uint)(isTrailingHit ? ((hitTestMetrics.length > 0) ? 1 : 0) : 0);
                }
                break;

            case TextBoxSetSelection.PageUp:
            case TextBoxSetSelection.PageDown:
                D2D_RECT_F pos;
                var sv = GetViewerParent();
                if (sv != null)
                {
                    // page is viewer's height if under a scroll viewer
                    pos = sv.Viewer.RelativeRenderRect - Margin;
                }
                else
                {
                    pos = RelativeRenderRect - Margin;
                }

                var crc = GetCaretRect();
                var top = crc.top + (moveMode == TextBoxSetSelection.PageUp ? -pos.Height : +pos.Height);
                var layout2 = CheckLayout(false);
                if (layout2 != null)
                {
                    layout2.Object.HitTestPoint(
                        Math.Max(crc.left, _lastCaretX),
                        top,
                        out isTrailingHit,
                        out _,
                        out hitTestMetrics
                        ).ThrowOnError();

                    _charPosition = hitTestMetrics.textPosition;
                    _charPositionOffset = (uint)(isTrailingHit ? ((hitTestMetrics.length > 0) ? 1 : 0) : 0);
                }
                break;

            case TextBoxSetSelection.LeftWord:
            case TextBoxSetSelection.RightWord:
                // To navigate by whole words, we look for the canWrapLineAfter flag in the cluster metrics.
                // First need to know how many clusters there are.
                var layout3 = CheckLayout(false);
                if (layout3 == null)
                    break;

#if NETFRAMEWORK
                layout3.Object.GetClusterMetrics(null, 0, out var clusterCount); // don't check error by design
#else
                layout3.Object.GetClusterMetrics(0, 0, out var clusterCount); // don't check error by design
#endif
                if (clusterCount == 0)
                    break;

                // Now we actually read them.
                var clusterMetrics = new DWRITE_CLUSTER_METRICS[clusterCount];
#if NETFRAMEWORK
                layout3.Object.GetClusterMetrics(clusterMetrics, (int)clusterCount, out _).ThrowOnError();
#else
                layout3.Object.GetClusterMetrics(clusterMetrics.AsPointer(), clusterCount, out _).ThrowOnError();
#endif

                _charPosition = absolutePosition;

                uint clusterPosition = 0;
                var oldCaretPosition = _charPosition;

                if (moveMode == TextBoxSetSelection.LeftWord)
                {
                    // Read through the clusters, keeping track of the farthest valid stopping point just before the old position.
                    _charPosition = 0;
                    _charPositionOffset = 0; // leading edge
                    for (uint cluster = 0; cluster < clusterCount; ++cluster)
                    {
                        clusterPosition += clusterMetrics[cluster].length;
#if NETFRAMEWORK
                        if (clusterMetrics[cluster].canWrapLineAfter != 0)
#else
                        if (clusterMetrics[cluster].canWrapLineAfter)
#endif
                        {
                            if (clusterPosition >= oldCaretPosition)
                                break;

                            // Update in case we pass this point next loop.
                            _charPosition = clusterPosition;
                        }
                    }
                }
                else // SetSelectionModeRightWord
                {
                    // Read through the clusters, looking for the first stopping point after the old position.
                    for (uint cluster = 0; cluster < clusterCount; ++cluster)
                    {
                        var clusterLength = clusterMetrics[cluster].length;
                        _charPosition = clusterPosition;
                        _charPositionOffset = clusterLength; // trailing edge
#if NETFRAMEWORK
                        if (clusterPosition >= oldCaretPosition && clusterMetrics[cluster].canWrapLineAfter != 0)
#else
                        if (clusterPosition >= oldCaretPosition && clusterMetrics[cluster].canWrapLineAfter)
#endif
                            break; // first stopping point after old position.

                        clusterPosition += clusterLength;
                    }
                }
                break;

            case TextBoxSetSelection.Home:
            case TextBoxSetSelection.End:
                // Retrieve the line metrics to know first and last positionon the current line.
                lineMetrics = GetLineMetrics();

                GetLineFromPosition(lineMetrics, _charPosition, out line, out _charPosition);

                _charPositionOffset = 0;
                if (moveMode == TextBoxSetSelection.End)
                {
                    // Place the caret at the last character on the line, excluding line breaks. In the case of wrapped lines, newlineLength will be 0.
                    var lineLength = lineMetrics[line].length - lineMetrics[line].newlineLength;
                    _charPositionOffset = Math.Min(lineLength, 1u);
                    _charPosition += lineLength - _charPositionOffset;
                    AlignCaretToNearestCluster(true);
                }
                break;

            case TextBoxSetSelection.First:
                _charPosition = 0;
                _charPositionOffset = 0;
                break;

            case TextBoxSetSelection.All:
                _charAnchor = 0;
                extendSelection = true;
                last();
                break;

            case TextBoxSetSelection.Last:
                last();
                break;

                void last()
                {
                    _charPosition = uint.MaxValue;
                    _charPositionOffset = 0;
                    AlignCaretToNearestCluster(true);
                }

            case TextBoxSetSelection.AbsoluteLeading:
                _charPosition = advance ?? 0;
                _charPositionOffset = 0;
                break;

            case TextBoxSetSelection.AbsoluteTrailing:
                _charPosition = advance ?? 0;
                AlignCaretToNearestCluster(true);
                break;
        }

        absolutePosition = _charPosition + _charPositionOffset;

        if (!extendSelection)
        {
            _charAnchor = absolutePosition;
        }

        var caretMoved = absolutePosition != oldAbsolutePosition || _charAnchor != oldCaretAnchor;
        if (!caretMoved)
            return false;

        if (moveMode != TextBoxSetSelection.Up && moveMode != TextBoxSetSelection.Down)
        {
            // remember max last horizontal position (mimic many editors, not like notepad/standard editbox)
            CheckLayout(false)?.Object.HitTestTextPosition(
                    _charPosition,
                    _charPositionOffset > 0,
                    out _lastCaretX,
                    out _,
                    out _
                    ).ThrowOnError();
        }

        UpdateOrigin();

        // update the caret formatting attributes
        if (updateCaretFormat)
        {
            UpdateCaretFormatting();
        }

        SetCaretLocation();
        Invalidate(VisualPropertyInvalidateModes.Render, new InvalidateReason(GetType()));
        return true;
    }

    private void UpdateOrigin()
    {
        if (RelativeRenderRect.IsInvalid)
            return;

        if (!IsEditable)
            return;

        // caretRc is absolute position in view
        var caretRc = GetCaretRect();
        var rr = RelativeRenderRect.Size;
        var padding = Padding;

        var leftPadding = padding.left.IsSet() && padding.left > 0;
        var topPadding = padding.top.IsSet() && padding.top > 0;
        var rightPadding = padding.right.IsSet() && padding.right > 0;
        var bottomPadding = padding.bottom.IsSet() && padding.bottom > 0;

        if (caretRc.bottom > (rr.height - _origin.y - (bottomPadding ? padding.bottom : 0) - (topPadding ? padding.top : 0)))
        {
            SetOriginY(rr.height - caretRc.bottom - (bottomPadding ? padding.bottom : 0) - (topPadding ? padding.top : 0));
        }

        if (caretRc.top < (-_origin.y + (topPadding ? padding.top : 0)))
        {
            SetOriginY(-caretRc.top + (topPadding ? padding.top : 0));
        }

        if (caretRc.right > (rr.width - _origin.x - (rightPadding ? padding.right : 0) - (leftPadding ? padding.left : 0)))
        {
            SetOriginX(rr.width - caretRc.right - (rightPadding ? padding.right : 0) - (leftPadding ? padding.left : 0));
        }

        if (caretRc.left < (-_origin.x + (leftPadding ? padding.left : 0)))
        {
            SetOriginX(-caretRc.left + (leftPadding ? padding.left : 0));
        }


        var sv = GetViewerParent();
        if (sv != null)
        {
            var ar = sv.Viewer.ArrangedRect;
            if (sv.IsVerticalScrollBarVisible)
            {
                if (caretRc.top < sv.VerticalOffset)
                {
                    sv.VerticalOffset = caretRc.top;
                }

                if (caretRc.bottom > sv.VerticalOffset + ar.Height - (bottomPadding ? padding.bottom : 0) - (topPadding ? padding.top : 0))
                {
                    sv.VerticalOffset = caretRc.bottom - ar.Height + (bottomPadding ? padding.bottom : 0) + (topPadding ? padding.top : 0);
                }
            }

            if (sv.IsHorizontalScrollBarVisible)
            {
                if (caretRc.left < sv.HorizontalOffset)
                {
                    sv.HorizontalOffset = caretRc.left;
                }

                if (caretRc.right > sv.HorizontalOffset + ar.Width - (rightPadding ? padding.right : 0) - (leftPadding ? padding.left : 0))
                {
                    sv.HorizontalOffset = caretRc.right - ar.Width + (rightPadding ? padding.right : 0) + (leftPadding ? padding.left : 0);
                }
            }
            return;
        }

        var par = Parent?.ArrangedRect;
        if (par != null)
        {
            if (caretRc.bottom > par.Value.Height - _origin.y - (bottomPadding ? padding.bottom : 0) - (topPadding ? padding.top : 0))
            {
                SetOriginY(par.Value.Height - (bottomPadding ? padding.bottom : 0) - (topPadding ? padding.top : 0) - caretRc.bottom);
            }
        }
    }

    private IViewerParent? GetViewerParent() => Parent is Viewer viewer ? viewer.Parent as ScrollViewer : null;

    /// <summary>
    /// Captures the formatting at the caret position so that new input inherits the preceding style.
    /// </summary>
    protected virtual void UpdateCaretFormatting()
    {
        var layout = CheckLayout(false);
        if (layout == null)
            return;

        var currentPos = _charPosition + _charPositionOffset;
        if (currentPos > 0)
        {
            --currentPos; // Always adopt the trailing properties.
        }

        _caretFormat ??= new CaretFormat();

        _caretFormat.fontFamilyName = layout.Object.GetFontFamilyName(currentPos);
        _caretFormat.localeName = layout.Object.GetLocaleName(currentPos);

        layout.Object.GetFontWeight(currentPos, out _caretFormat.fontWeight, IntPtr.Zero).ThrowOnError();
        layout.Object.GetFontStyle(currentPos, out _caretFormat.fontStyle, IntPtr.Zero).ThrowOnError();
        layout.Object.GetFontStretch(currentPos, out _caretFormat.fontStretch, IntPtr.Zero).ThrowOnError();
        layout.Object.GetFontSize(currentPos, out _caretFormat.fontSize, IntPtr.Zero).ThrowOnError();
#if NETFRAMEWORK
        layout.Object.GetUnderline(currentPos, out var caretFormatHasUnderline, IntPtr.Zero).ThrowOnError();
        _caretFormat.hasUnderline = caretFormatHasUnderline;
        layout.Object.GetStrikethrough(currentPos, out var caretFormathasStrikethrough, IntPtr.Zero).ThrowOnError();
        _caretFormat.hasStrikethrough = caretFormathasStrikethrough;
#else
        layout.Object.GetUnderline(currentPos, out _caretFormat.hasUnderline, IntPtr.Zero).ThrowOnError();
        layout.Object.GetStrikethrough(currentPos, out _caretFormat.hasStrikethrough, IntPtr.Zero).ThrowOnError();
#endif
        layout.Object.GetDrawingEffect(currentPos, out var drawingEffect, IntPtr.Zero).ThrowOnError();

        if (layout.Object is IDWriteTextLayout2 layout2)
        {
            _caretFormat.isLastLineWrapping = layout2.GetLastLineWrapping();
            _caretFormat.opticalAlignment = layout2.GetOpticalAlignment();
            _caretFormat.verticalGlyphOrientation = layout2.GetVerticalGlyphOrientation();

            if (layout.Object is IDWriteTextLayout3 layout3)
            {
                if (layout3.GetLineSpacing(out var lineSpacing).IsSuccess)
                {
                    _caretFormat.lineSpacing = lineSpacing;
                }
            }
        }

        _caretFormat.color = D3DCOLORVALUE.Gray;

#if NETFRAMEWORK
        if (drawingEffect != null)
        {
            var brush = drawingEffect as ID2D1SolidColorBrush;
            brush.GetColor(out var color);
            _caretFormat.color = color;
            Marshal.ReleaseComObject(drawingEffect);
        }
#else
        if (drawingEffect != 0)
        {
            using var brush = ComObject.FromPointer<ID2D1SolidColorBrush>(drawingEffect);
            if (brush != null)
            {
                _caretFormat.color = brush.Object.GetColor();
            }
        }
#endif
    }

    private D2D_RECT_F GetCaretRect()
    {
        // Gets the current caret position (in untransformed space).
        var caretX = 0f;
        var caretY = 0f;
        var caretMetrics = new DWRITE_HIT_TEST_METRICS
        {
            height = GetFontSize() // just for default
        };

        var layout = CheckLayout(false);
        if (layout != null)
        {
            // Translate text character offset to point x,y.
            layout.Object.HitTestTextPosition(
                _charPosition,
                _charPositionOffset > 0, // trailing if nonzero, else leading edge
                out caretX,
                out caretY,
                out caretMetrics
                ).ThrowOnError();

            // If a selection exists, draw the caret using the line size rather than the font size.
            var selection = GetSelectionRange();
            if (selection.length > 0)
            {
                var metrics = new DWRITE_HIT_TEST_METRICS[1];
#if NETFRAMEWORK
                layout.Object.HitTestTextRange(
                     _charPosition,
                     0, // length
                     0, // x
                     0, // y
                     metrics,
                     metrics.Length,
                     out _
                     ).ThrowOnError();
#else
                layout.Object.HitTestTextRange(
                    _charPosition,
                    0, // length
                    0, // x
                    0, // y
                    metrics.AsPointer(),
                    metrics.Length(),
                    out _
                    ).ThrowOnError();
#endif

                caretMetrics = metrics[0];
                caretY = caretMetrics.top;
            }
        }

        var caretWidth = Window?.Caret?.FinalWidth ?? 2f;
        var caretHeight = caretMetrics.height;
        var padding = Padding;

        // depending on padding, caret may be invisible
        var maxCaretHeight = RelativeRenderRect.Height;
        if (padding.top.IsSet() && padding.top > 0)
        {
            maxCaretHeight = Math.Max(0, maxCaretHeight - padding.top);
        }

        if (padding.bottom.IsSet() && padding.bottom > 0)
        {
            maxCaretHeight = Math.Max(0, maxCaretHeight - padding.bottom);
        }

        caretHeight = Math.Min(caretHeight, maxCaretHeight);

        var rc = new D2D_RECT_F
        {
            left = caretX - caretWidth / 2f
        };
        rc.right = rc.left + caretWidth;
        rc.top = caretY;
        rc.bottom = caretY + caretHeight; // preserve original height rules
        return rc;
    }

    /// <summary>
    /// Returns the currently selected text, or an empty string if nothing is selected.
    /// </summary>
    public string GetSelectionText()
    {
        var selection = GetSelectionRange();
        if (selection.length == 0)
            return string.Empty;

        return RenderedText?.Substring((int)selection.startPosition, (int)selection.length) ?? string.Empty;
    }

    /// <summary>
    /// Returns the current selection range as <see cref="DWRITE_TEXT_RANGE"/>, normalized (start &lt;= end) and clamped to text length.
    /// </summary>
    public DWRITE_TEXT_RANGE GetSelectionRange()
    {
        // Returns a valid range of the current selection, regardless of whether the caret or anchor is first.
        var caretBegin = _charAnchor;
        var caretEnd = _charPosition + _charPositionOffset;
        if (caretBegin > caretEnd)
        {
            (caretEnd, caretBegin) = (caretBegin, caretEnd);
        }

        // Limit to actual text length.
        var textLength = (uint)(RenderedText?.Length).GetValueOrDefault();
        caretBegin = Math.Min(caretBegin, textLength);
        caretEnd = Math.Min(caretEnd, textLength);
        return new DWRITE_TEXT_RANGE(caretBegin, caretEnd - caretBegin);
    }

    /// <summary>
    /// Ensures a valid DirectWrite layout exists. Throws when not measured and <paramref name="throwIfNull"/> is true.
    /// </summary>
    protected virtual IComObject<IDWriteTextLayout>? CheckLayout(bool throwIfNull = true)
    {
        var layout = _layout;
        if (layout == null || layout.IsDisposed)
        {
            if (throwIfNull)
                throw new WiceException("0023: Operation on '" + Name + "' of type '" + GetType().FullName + "' is invalid as it was not measured.");

            return layout;
        }

        return layout;
    }

    private DWRITE_LINE_METRICS[] GetLineMetrics()
    {
        var layout = CheckLayout()!;

        // Retrieves the line metrics, used for caret navigation, up/down and home/end.
        var textMetrics = layout.GetMetrics1();

#if NETFRAMEWORK
        var lineMetrics = new DWRITE_LINE_METRICS[textMetrics.lineCount];
        layout.Object.GetLineMetrics(lineMetrics, lineMetrics.Length, out _).ThrowOnError();
#else
        var lineMetrics = new DWRITE_LINE_METRICS[textMetrics.Base.lineCount];
        layout.Object.GetLineMetrics(lineMetrics.AsPointer(), lineMetrics.Length(), out _).ThrowOnError();
#endif
        return lineMetrics;
    }

    private static void GetLineFromPosition(DWRITE_LINE_METRICS[] lineMetrics, uint textPosition, out uint lineOut, out uint linePositionOut)
    {
        // Given the line metrics, determines the current line and starting text position of that line by summing up the lengths.
        // When the startingline position is beyond the given text position, we have our line.
        uint line = 0;
        uint linePosition = 0;
        uint nextLinePosition = 0;
        for (; line < lineMetrics.Length; ++line)
        {
            linePosition = nextLinePosition;
            nextLinePosition = linePosition + lineMetrics[line].length;

            // The next line is beyond the desired text position, so it must be in the current line
            if (nextLinePosition > textPosition)
                break;
        }

        linePositionOut = linePosition;
        lineOut = Math.Min(line, (uint)(lineMetrics.Length - 1));
    }

    private void AlignCaretToNearestCluster(bool isTrailingHit = false, bool skipZeroWidth = false)
    {
        // Uses hit-testing to align the current caret position to a whole cluster, rather than residing in the middle of a base character + diacritic, surrogate pair, or character + UVS.
        // Align the caret to the nearest whole cluster.
        var hitTestMetrics = new DWRITE_HIT_TEST_METRICS { length = 1 };
        var layout = CheckLayout(false);
        layout?.Object.HitTestTextPosition(_charPosition, false, out _, out _, out hitTestMetrics).ThrowOnError();

        // The caret position itself is always the leading edge.
        // An additional offset indicates a trailing edge when non-zero.
        // This offset comes from the number of code-units in the selected cluster or surrogate pair.
        _charPosition = hitTestMetrics.textPosition;
        _charPositionOffset = isTrailingHit ? hitTestMetrics.length : 0;

        // For invisible, zero-width characters (like line breaks and formatting characters), force leading edge of the next position.
        if (skipZeroWidth && hitTestMetrics.width == 0)
        {
            _charPosition += _charPositionOffset;
            _charPositionOffset = 0;
        }
    }

    private enum FontRangeType : short
    {
        FontWeight,
        FontStretch,
        FontStyle,
        FontSize,
        FontFamilyName,
        FontCollection,
        LocaleName,
        Strikethrough,
        Underline,
        Typography,
        InlineObject,
        DrawingEffect,
        SolidColor,
        PairKerning,
        CharacterSpacing
    }

    private void SetFontRangeValue(FontRangeType type, object? value, DWRITE_TEXT_RANGE[] ranges)
    {
        ExceptionExtensions.ThrowIfNull(ranges, nameof(ranges));

        if (!_ranges.TryGetValue(type, out var fontRanges))
        {
            fontRanges = new FontRanges
            {
                Type = type
            };
            fontRanges = _ranges.AddOrUpdate(type, fontRanges, (k, o) => o);
        }

        FontRanges[]? list = null;
        lock (_rangesLock)
        {
            var changed = false;
            foreach (var range in ranges)
            {
                var newFontRange = new FontRange { Range = range, Value = value };

                // quick out cases
                if (range.startPosition >= fontRanges.EndPosition)
                {
                    fontRanges.Ranges.Add(newFontRange);
                    changed = true;
                    break;
                }

                if (range.EndPosition < fontRanges.StartPosition)
                {
                    fontRanges.Ranges.Insert(0, newFontRange);
                    changed = true;
                    break;
                }

                var currentRange = range;
                var merged = false;
                var array = fontRanges.Ranges.ToArray();
                var i = 0;
                int? insertPos = null;
                for (; i < array.Length; i++)
                {
                    var fontRange = array[i];
                    if (currentRange.EndPosition < fontRange.Range.startPosition)
                        break; // insert before this one

                    if (currentRange.startPosition > fontRange.Range.EndPosition)
                        continue; // pass this one

                    if (fontRange.Range.startPosition > currentRange.startPosition && fontRange.Range.EndPosition < currentRange.EndPosition)
                    {
                        // old range is included, remove it, whether the value is the same or not
                        fontRanges.Ranges.Remove(fontRange);
                        changed = true;
                        continue;
                    }

                    if (fontRange.Value?.Equals(value) == true)
                    {
                        if (currentRange.startPosition >= fontRange.Range.startPosition)
                        {
                            // merge both ranges
                            currentRange = DWRITE_TEXT_RANGE.FromTo(fontRange.Range.startPosition, Math.Max(fontRange.Range.EndPosition, currentRange.EndPosition));
                            if (fontRange.Range != currentRange)
                            {
                                fontRange.Range = currentRange;
                                changed = true;
                            }

                            // current range is already inserted
                            merged = true;
                            continue;
                        }

                        if (currentRange.EndPosition >= fontRange.Range.startPosition)
                        {
                            currentRange = DWRITE_TEXT_RANGE.FromTo(currentRange.startPosition, Math.Max(fontRange.Range.EndPosition, currentRange.EndPosition));
                            if (fontRange.Range != currentRange)
                            {
                                fontRange.Range = currentRange;
                                changed = true;
                            }

                            // current range is already inserted
                            merged = true;
                            continue;
                        }
                    }

                    changed = true; // we always change value
                    if (currentRange.startPosition == fontRange.Range.startPosition)
                    {
                        if (currentRange.EndPosition >= fontRange.Range.EndPosition)
                        {
                            // overlap with same start, udate value
                            fontRange.Range = currentRange;
                            fontRange.Value = value;

                            // current range is already inserted
                            merged = true;
                            continue;
                        }

                        // reduce range
                        fontRange.Range = DWRITE_TEXT_RANGE.FromTo(currentRange.EndPosition, fontRange.Range.EndPosition);

                        // add ourself, break
                        break;
                    }

                    if (currentRange.startPosition > fontRange.Range.startPosition)
                    {
                        if (currentRange.EndPosition < fontRange.Range.EndPosition)
                        {
                            // add another range
                            fontRanges.Ranges.Insert(i + 1, new FontRange { Range = DWRITE_TEXT_RANGE.FromTo(currentRange.EndPosition, fontRange.Range.EndPosition), Value = fontRange.Value });

                            // reduce range
                            fontRange.Range = DWRITE_TEXT_RANGE.FromTo(fontRange.Range.startPosition, currentRange.startPosition);

                            // break = >add ourself 
                            insertPos = i + 1;
                            break;
                        }

                        // reduce range
                        fontRange.Range = DWRITE_TEXT_RANGE.FromTo(fontRange.Range.startPosition, currentRange.startPosition);

                        // continue with current range, remember insertion pos
                        insertPos = i + 1;
                        continue;
                    }

                    if (currentRange.EndPosition >= fontRange.Range.startPosition)
                    {
                        // reduce range (keep its value
                        fontRange.Range = DWRITE_TEXT_RANGE.FromTo(currentRange.EndPosition, fontRange.Range.EndPosition);

                        // continue with current range, remember insertion pos
                        insertPos = i;
                        continue;
                    }
                }

                if (!merged)
                {
                    var index = insertPos ?? i;
                    if (index > fontRanges.Ranges.Count)
                    {
                        fontRanges.Ranges.Add(newFontRange);
                    }
                    else
                    {
                        fontRanges.Ranges.Insert(index, newFontRange);
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                list = [.. _ranges.Values];
            }
        }

        if (list != null)
        {
            _finalRanges = list;
            Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
        }
    }

    private sealed class FontRange
    {
        public DWRITE_TEXT_RANGE Range;
        public object? Value;

        public bool EqualsValue(object? value)
        {
            if (value is null)
                return Value is null;

            return value.Equals(Value);
        }

        public override string ToString() => Range + " => " + Value;
    }

    private sealed class FontRanges
    {
        public FontRangeType Type;
        public List<FontRange> Ranges = []; // note this should always sorted by construction

        public uint StartPosition
        {
            get
            {
                if (Ranges.Count == 0)
                    return 0;

                return Ranges[0].Range.startPosition;
            }
        }

        public uint EndPosition
        {
            get
            {
                if (Ranges.Count == 0)
                    return 0;

#if NETFRAMEWORK
                return Ranges[Ranges.Count - 1].Range.EndPosition;
#else
                return Ranges[^1].Range.EndPosition;
#endif
            }
        }
    }

    private sealed class CaretFormat
    {
        // the important range based properties for the current caret.
        // note these are stored outside the layout, since the current caret actually has a format, independent of the text it lies between.
        public string? fontFamilyName;
        public string? localeName;
        public float fontSize;
        public DWRITE_FONT_WEIGHT fontWeight;
        public DWRITE_FONT_STRETCH fontStretch;
        public DWRITE_FONT_STYLE fontStyle;
        public D3DCOLORVALUE color;
        public BOOL hasUnderline;
        public BOOL hasStrikethrough;
        public BOOL isLastLineWrapping;
        public DWRITE_OPTICAL_ALIGNMENT opticalAlignment;
        public DWRITE_VERTICAL_GLYPH_ORIENTATION verticalGlyphOrientation;
        public DWRITE_LINE_SPACING? lineSpacing;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Reset();
    }

    bool IImmVisual.SetImmCompositionWindowPosition(Window window) => SetImmCompositionWindowPosition(window);

    /// <summary>
    /// Allows derived classes to override IME composition window positioning. Return true to handle, false to use default behavior.
    /// </summary>
    protected virtual bool SetImmCompositionWindowPosition(Window window)
    {
        if (!IsEditable)
            return true; // do nothing if not editable

        // let default behavior handle it
        return false;
    }

    /// <summary>
    /// Releases managed/unmanaged resources. Disposes the cached text layout.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                Interlocked.Exchange(ref _layout, null)?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the visual and suppresses finalization.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
