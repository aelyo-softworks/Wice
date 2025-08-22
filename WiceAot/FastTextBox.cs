namespace Wice;

/// <summary>
/// Read-only text box optimized for large content.
/// </summary>
/// <remarks>
/// - For small inputs (line count below <see cref="FallbackLineCountThreshold"/>), falls back to base <see cref="TextBox"/> behavior.
/// - For large inputs (line count reaching <see cref="DeferredParsingLineCountThreshold"/>), continues parsing on a background thread.
/// - Renders a windowed (viewport) subset of the text using a custom DirectWrite layout to reduce memory and layout cost.
/// - Disables editing and focus by design.
/// </remarks>
public partial class FastTextBox : TextBox
{
    private TextContainer _container;
    private IComObject<IDWriteTextLayout>? _layout;
    private int _currentLineNumber;
    private string? _renderedText;

    /// <summary>
    /// Raised as lines are being parsed/loaded. Can be raised from a background thread.
    /// </summary>
    public event EventHandler<LoadEventArgs>? Loading;

    /// <summary>
    /// Raised when parsing/loading completes (synchronously or asynchronously). Can be raised from a background thread.
    /// </summary>
    public event EventHandler<LoadEventArgs>? Loaded;

    /// <summary>
    /// Creates a new <see cref="FastTextBox"/>. Editing and focus are disabled.
    /// </summary>
    public FastTextBox()
        : base()
    {
        _container = new TextContainer(this, null);
        base.IsEnabled = true;
        base.IsEditable = false;
    }

    /// <summary>
    /// Indicates that the composition transform is maxed when we are in fallback mode (base <see cref="TextBox"/> handles layout).
    /// </summary>
    protected override bool TransformMaxed => HasFallback;

    /// <summary>
    /// Gets the text content currently used by the DirectWrite layout for rendering (viewport lines).
    /// </summary>
    protected override string RenderedText => _renderedText ?? string.Empty;

    /// <summary>
    /// True when the control uses base <see cref="TextBox"/> logic (fallback mode). False when using the fast viewported renderer.
    /// </summary>
    protected virtual bool HasFallback { get; set; } // true if we're using TextBox code

    /// <summary>
    /// Gets the number of parsed lines (0 when in fallback or not yet parsed).
    /// </summary>
    [Category(CategoryLive)]
    public int LineCount => _container.Lines?.Length ?? 0;

    /// <summary>
    /// Gets whether the last loading/parsing operation was canceled by a listener of <see cref="Loading"/>.
    /// </summary>
    [Category(CategoryLive)]
    public virtual bool LoadingWasCancelled { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the text has been fully or partially parsed and is ready for measuring, arranging or rendering.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsReady { get; protected set; }

    /// <summary>
    /// True while deferred parsing continues on a background thread.
    /// </summary>
    [Category(CategoryLive)]
    public bool IsLoading => _container.LoadingLines != null;

    /// <summary>
    /// Gets the 0-based index of the first fully visible line in the current viewport.
    /// </summary>
    [Category(CategoryLive)]
    public int CurrentLineNumber => _currentLineNumber;

    /// <summary>
    /// Returns the estimated single-character width and line height used during parsing and rough layout.
    /// </summary>
    [Category(CategoryLive)]
    public D2D_SIZE_F RenderMetrics => _container.ParsedMetrics;

    /// <summary>
    /// Returns the wrapping mode used while parsing the source text into lines.
    /// </summary>
    [Category(CategoryLive)]
    public DWRITE_WORD_WRAPPING RenderWrapping => _container.ParsedWrapping;

    /// <summary>
    /// Gets the <see cref="Line"/> at <see cref="CurrentLineNumber"/> when available; otherwise null.
    /// </summary>
    [Category(CategoryLive)]
    public Line? CurrentLine
    {
        get
        {
            var lines = _container.Lines;
            if (lines == null)
                return null;

            if (_currentLineNumber < 0 || _currentLineNumber + 1 > lines.Length)
                return null;

            return lines[_currentLineNumber];
        }
    }

    /// <summary>
    /// Gets or sets the control text. Setting triggers parsing and measurement invalidation.
    /// </summary>
    [Category(CategoryLayout)]
    public override string Text { get => _container.Text; set => SetText(value); }

    /// <summary>
    /// Always throws for <see cref="true"/>. This control is read-only by design.
    /// </summary>
    [Category(CategoryBehavior)]
    public override bool IsEditable
    {
        get => base.IsEditable;
        set
        {
            if (value)
                throw new NotSupportedException();

            base.IsEditable = value;
        }
    }

    /// <summary>
    /// Always throws for <see cref="true"/>. The control is always enabled internally.
    /// </summary>
    [Category(CategoryBehavior)]
    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            if (value)
                throw new NotSupportedException();

            base.IsEnabled = value;
        }
    }

    /// <summary>
    /// Line count threshold below which the control falls back to base <see cref="TextBox"/> rendering.
    /// </summary>
    /// <remarks>
    /// Default is 5000 lines. Must be non-negative.
    /// </remarks>
    [Category(CategoryBehavior)]
    public virtual int FallbackLineCountThreshold { get; set; } = 5000; // for less than 5000 lines, we fallback to base TextBox

    /// <summary>
    /// Line count threshold at which parsing is continued on a background thread.
    /// </summary>
    /// <remarks>
    /// Default is 10000 lines. Must be greater than <see cref="FallbackLineCountThreshold"/>.
    /// </remarks>
    [Category(CategoryBehavior)]
    public virtual int DeferredParsingLineCountThreshold { get; set; } = 10000; // for more than 10000 lines, we continue parsing the text in a background thread

    /// <summary>
    /// Gets the parsed <see cref="Line"/> at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Zero-based line index.</param>
    /// <returns>The line descriptor.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When lines are unavailable or <paramref name="index"/> is out of range.</exception>
    public virtual Line GetLine(int index)
    {
        if (_container.Lines == null || index < 0 || index >= _container.Lines.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _container.Lines[index];
    }

    /// <summary>
    /// Raises <see cref="Loading"/>.
    /// </summary>
    protected virtual void OnLoading(object sender, LoadEventArgs e) => Loading?.Invoke(this, e);

    /// <summary>
    /// Raises <see cref="Loaded"/>.
    /// </summary>
    protected virtual void OnLoaded(object sender, LoadEventArgs e) => Loaded?.Invoke(this, e);

    private void OnLoaded(LoadEventArgs e)
    {
        IsReady = true;
        OnLoaded(this, e);
        Invalidate(VisualPropertyInvalidateModes.Measure);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Interlocked.Exchange(ref _layout, null)?.Dispose();
        }
    }

    /// <summary>
    /// Validates threshold properties for correctness.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">When thresholds are invalid.</exception>
    protected virtual void ValidateProperties()
    {
        if (FallbackLineCountThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(FallbackLineCountThreshold));

        if (DeferredParsingLineCountThreshold <= 0 || DeferredParsingLineCountThreshold <= FallbackLineCountThreshold)
            throw new ArgumentOutOfRangeException(nameof(DeferredParsingLineCountThreshold));
    }

    /// <summary>
    /// Measures desired size using parsed lines and viewport metrics or base fallback.
    /// </summary>
    /// <param name="constraint">Measure constraint.</param>
    /// <returns>Desired size.</returns>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        ValidateProperties();
        var size = MeasureWithPadding(constraint, c => _container.EnsureLinesParsed(c));
        if (HasFallback)
            return base.MeasureCore(constraint);

        return size;
    }

    /// <summary>
    /// Updates the current top line based on the arranged rect and invalidates the cached layout when needed.
    /// </summary>
    /// <param name="finalRect">Final arranged rect.</param>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        if (!HasFallback)
        {
            var floatLine = Math.Max(0, -finalRect.top / _container.ParsedMetrics.height);
            var line = floatLine.CeilingI();
            if (line == _currentLineNumber)
                return;

            _currentLineNumber = line;
            _layout?.Dispose();
            _layout = null;
        }
        base.ArrangeCore(finalRect);
    }

    /// <summary>
    /// Ensures a valid layout exists (when using the fast path). Throws if missing and <paramref name="throwIfNull"/> is true.
    /// </summary>
    /// <param name="throwIfNull">True to throw when layout is null/invalid.</param>
    /// <returns>The current layout or null.</returns>
    /// <exception cref="WiceException">When layout is unavailable and <paramref name="throwIfNull"/> is true.</exception>
    protected override IComObject<IDWriteTextLayout>? CheckLayout(bool throwIfNull)
    {
        var layout = _layout;
        if ((layout == null || layout.IsDisposed) && throwIfNull)
            throw new WiceException("0023: Operation on '" + Name + "' of type '" + GetType().FullName + "' is invalid as it was not measured.");

        return layout;
    }

    /// <summary>
    /// Builds or retrieves a DirectWrite layout for the current viewport range.
    /// </summary>
    /// <param name="maxWidth">Max layout width.</param>
    /// <param name="maxHeight">Max layout height (text extent); will be clamped to the visible window height.</param>
    /// <returns>The layout for the current viewport, or base layout when in fallback.</returns>
    /// <remarks>
    /// This method progressively expands the range of lines from <see cref="_currentLineNumber"/> downward until the layout height fills the viewport
    /// or the end of the text is reached, disposing intermediate layouts to minimize memory usage.
    /// </remarks>
    protected override IComObject<IDWriteTextLayout>? GetLayout(float maxWidth, float maxHeight)
    {
        if (HasFallback)
            return base.GetLayout(maxWidth, maxHeight);

        maxHeight = Math.Min(maxHeight, ParentsAbsoluteClipRect?.Height ?? 0);
        _layout?.Dispose();
        _layout = null;
        var lastLine = _currentLineNumber;
        do
        {
            var text = _container.GetText(_currentLineNumber, lastLine);
            if (text == null)
                return null;

            var layout = Application.CurrentResourceManager.CreateTextLayout(GetFormat(), text, 0, maxWidth, maxHeight);
            var metrics = layout.GetMetrics1();
            if (metrics.heightIncludingTrailingWhitespace > maxHeight)
            {
                _renderedText = text;
                _layout = layout;
                break;
            }

            lastLine++;
            if (_container.Lines != null && lastLine >= _container.Lines.Length)
            {
                _renderedText = text;
                _layout = layout;
                break;
            }

            layout.Dispose();
        }
        while (true);
        return _layout;
    }

    private void SetText(string text)
    {
        if (_container.Text == text)
            return;

        IsReady = false;
        Interlocked.Exchange(ref _container, new TextContainer(this, text));
        Invalidate(VisualPropertyInvalidateModes.Measure);
    }

    /// <summary>
    /// Renders the visual element within the specified rendering context.
    /// </summary>
    /// <remarks>This method is called during the rendering process and ensures that the visual element is
    /// properly rendered. If the element is not loaded, the rendering process is invalidated and no further rendering
    /// occurs.</remarks>
    /// <param name="context">The rendering context that provides the necessary information and resources for rendering.</param>
    protected internal override void RenderCore(RenderContext context)
    {
        var ready = IsReady;
        if (!ready)
        {
            Invalidate(VisualPropertyInvalidateModes.Render);
            return;
        }

        base.RenderCore(context);
    }

    private sealed class TextContainer(FastTextBox visual, string? text)
    {
        public const char NotUnicode = '\uFFFF';

        private D2D_SIZE_F _parsedConstraint;
        private D2D_SIZE_F _parsedSize;
        private readonly Lock _lock = new();
        private bool _wasParsingDeffered;

        public FastTextBox Visual = visual;
        public string Text = text ?? string.Empty;
        public Line[]? Lines;
        public Task? LoadingLines;
        public D2D_SIZE_F ParsedMetrics;
        public DWRITE_WORD_WRAPPING ParsedWrapping;

        private D2D_SIZE_F ComputeMetrics()
        {
            const string sample = "The quick brown fox jumps over the lazy dog";
            var format = Visual.GetFormat();
            using var layout = Application.CurrentResourceManager.CreateTextLayout(format, sample);
            var metrics = layout.GetMetrics1();
#if NETFRAMEWORK
            return new D2D_SIZE_F(metrics.width / sample.Length, metrics.height);
#else
            return new D2D_SIZE_F(metrics.Base.width / sample.Length, metrics.Base.height);
#endif
        }

        public D2D_SIZE_F EnsureLinesParsed(D2D_SIZE_F constraint, bool refresh = false)
        {
            if (LoadingLines != null)
                return _parsedSize; // cache

            var wrapping = Visual.WordWrapping;
            if (wrapping != DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP && constraint.width.IsNotSet())
            {
                wrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP;
            }

            if (_parsedConstraint == constraint && ParsedWrapping == wrapping && !refresh)
                return _parsedSize;

            Visual.HasFallback = false;
            ParsedMetrics = ComputeMetrics();
            var maxCharactersPerLine = 0;
            var lines = new List<Line>();
            var position = 0;
            var length = 0;
            var text = Text;
            var batchSize = 0;
            var lastWordPosition = 0;
            var inSeparator = true;
            var e = new LoadEventArgs();
            for (var i = 0; i < text.Length; i++)
            {
                if (!loadCharacter(ref i))
                    break;
            }

            bool loadCharacter(ref int i)
            {
                var c = text[i];
                var n = i + 1 < Text.Length ? Text[i + 1] : NotUnicode;

                if (c == '\n' || (c == '\r' && n == '\n'))
                {
                    var line = new Line(position, length);
                    if (length > maxCharactersPerLine)
                    {
                        maxCharactersPerLine = length;
                    }

                    lock (_lock) lines.Add(line);
                    length = 0;
                    if (c == '\r')
                    {
                        i++;
                    }
                    position = i + 1;

                    if (LoadingLines != null)
                    {
                        batchSize++;
                        if (e.NextEventBatchSize == batchSize)
                        {
                            batchSize = 0;
                            e.LoadedLines = lines.Count;
                            Visual.OnLoading(Visual, e);
                            if (e.Cancel)
                            {
                                Visual.LoadingWasCancelled = true;
                                return false;
                            }
                        }
                    }

                    _wasParsingDeffered = LoadingLines == null && lines.Count == Visual.DeferredParsingLineCountThreshold;
                    if (_wasParsingDeffered)
                    {
                        var start = i;
                        e.LoadedLines = lines.Count;
                        Visual.OnLoading(Visual, e);
                        Visual.LoadingWasCancelled = e.Cancel;
                        if (!Visual.LoadingWasCancelled)
                        {
                            LoadingLines = Task.Run(() =>
                            {
                                for (var i2 = start; i2 < text.Length; i2++)
                                {
                                    loadCharacter(ref i2);
                                    if (Visual.LoadingWasCancelled)
                                        break;
                                }

                                if (length > 0 && !Visual.LoadingWasCancelled)
                                {
                                    var line2 = new Line(position, length);
                                    lock (_lock) lines.Add(line2);
                                }

                                lock (_lock) Lines = [.. lines];

                                _parsedConstraint = constraint;
                                ParsedWrapping = wrapping;
                                _parsedSize = new D2D_SIZE_F(maxCharactersPerLine * ParsedMetrics.width, Lines.Length * ParsedMetrics.height);

                                e.LoadedLines = lines.Count;

                                Visual.OnLoaded(e);
                                LoadingLines = null;
                            });
                        }
                        return false;
                    }
                }
                else
                {
                    length++;

                    if (wrapping == DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP)
                        return true;

                    if (wrapping == DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER)
                    {
                        if (ParsedMetrics.width * length > constraint.width)
                        {
                            var line = new Line(position, length - 1);
                            if (length - 1 > maxCharactersPerLine)
                            {
                                maxCharactersPerLine = length - 1;
                            }

                            lock (_lock) lines.Add(line);
                            length = 0;
                            position = i;
                        }
                        return true;
                    }

                    var uc = char.GetUnicodeCategory(c);
                    var isSeparator = uc == UnicodeCategory.SpaceSeparator || uc == UnicodeCategory.LineSeparator || uc == UnicodeCategory.ParagraphSeparator;

                    if (isSeparator)
                    {
                        // we currently don't support scrupulously all Direct Write wrapping possibilities here
                        if (ParsedMetrics.width * length > constraint.width)
                        {
                            var line = new Line(position, lastWordPosition - position);
                            if (length - 1 > maxCharactersPerLine)
                            {
                                maxCharactersPerLine = length - 1;
                            }

                            lock (_lock) lines.Add(line);
                            length = 0;
                            position = i;
                        }

                        inSeparator = true;
                    }
                    else
                    {
                        if (inSeparator)
                        {
                            lastWordPosition = i;
                        }
                        inSeparator = false;
                    }
                }
                return true;
            }

            if (LoadingLines == null && length > 0)
            {
                var line = new Line(position, length);
                lock (_lock) lines.Add(line);
            }

            lock (_lock) Lines = [.. lines];

            _parsedConstraint = constraint;
            ParsedWrapping = wrapping;

            // note: fallback cannot/mustnot happen if we're loading in another thread
            Visual.HasFallback = Lines.Length < Visual.FallbackLineCountThreshold;
            if (Visual.HasFallback)
            {
                Lines = null;
                _parsedSize = new D2D_SIZE_F();
            }
            else
            {
                _parsedSize = new D2D_SIZE_F(maxCharactersPerLine * ParsedMetrics.width, Lines.Length * ParsedMetrics.height);
            }

            if (!_wasParsingDeffered)
            {
                Visual.OnLoaded(e);
            }
            return _parsedSize;
        }

        public override string ToString() => Text;

        public string? GetText(int firstLineIndex, int lastLineIndex)
        {
            if (Lines == null)
                return null;

            if (firstLineIndex < 0 || firstLineIndex >= Lines.Length)
                return null;

            var firstLine = Lines[firstLineIndex];
            if (lastLineIndex >= Lines.Length)
#if NETFRAMEWORK
                return Text.Substring(firstLine.Position);
#else
                return Text[firstLine.Position..];
#endif

            var length = Lines[lastLineIndex].Position - firstLine.Position + Lines[lastLineIndex].Length;
            return Text.Substring(firstLine.Position, length);
        }
    }

    /// <summary>
    /// Represents a line in the parsed source text (offset and length).
    /// </summary>
    /// <param name="position">Zero-based absolute character index where the line starts.</param>
    /// <param name="length">Number of characters in the line (excluding newline).</param>
    public readonly struct Line(int position, int length)
    {
        /// <summary>
        /// Gets the absolute character index where the line starts.
        /// </summary>
        public int Position { get; } = position;

        /// <summary>
        /// Gets the character length of the line (excluding newline).
        /// </summary>
        public int Length { get; } = length;

        /// <summary>
        /// Returns a diagnostic string containing position and length.
        /// </summary>
        public override string ToString() => Position + " (" + Length + ")";
    }
}
