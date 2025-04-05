﻿namespace Wice;

public partial class FastTextBox : TextBox
{
    private TextContainer _container;
    private IComObject<IDWriteTextLayout>? _layout;
    private int _currentLine;
    private string? _renderedText;

    // occurs on any thread
    public event EventHandler<LoadEventArgs>? Loading;
    public event EventHandler<LoadEventArgs>? Loaded;

    public FastTextBox()
        : base()
    {
        _container = new TextContainer(this, null);
    }

    protected override bool TransformMaxed => HasFallback;
    protected override string RenderedText => _renderedText ?? string.Empty;
    protected virtual bool HasFallback { get; set; } // true if we're using TextBox code

    [Category(CategoryLive)]
    public virtual bool LoadingWasCancelled { get; protected set; }

    [Category(CategoryLayout)]
    public override string Text { get => _container.Text; set => SetText(value); }

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

    [Category(CategoryBehavior)]
    public override bool IsFocusable
    {
        get => base.IsFocusable;
        set
        {
            if (value)
                throw new NotSupportedException();

            base.IsFocusable = value;
        }
    }

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

    [Category(CategoryBehavior)]
    public virtual int FallbackLineCountThreshold { get; set; } = 5000; // for less than 5000 lines, we fallback to base TextBox

    [Category(CategoryBehavior)]
    public virtual int DeferredParsingLineCountThreshold { get; set; } = 10000; // for more than 10000 lines, we continue parsing the text in a background thread

    protected virtual void OnLoading(object sender, LoadEventArgs e) => Loading?.Invoke(this, e);
    protected virtual void OnLoaded(object sender, LoadEventArgs e) => Loaded?.Invoke(this, e);

    protected virtual void ValidateProperties()
    {
        if (FallbackLineCountThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(FallbackLineCountThreshold));

        if (DeferredParsingLineCountThreshold <= 0 || DeferredParsingLineCountThreshold <= FallbackLineCountThreshold)
            throw new ArgumentOutOfRangeException(nameof(DeferredParsingLineCountThreshold));
    }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        ValidateProperties();
        var size = MeasureWithPadding(constraint, c => _container.EnsureLinesParsed(c));
        if (HasFallback)
            return base.MeasureCore(constraint);

        return size;
    }

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        if (!HasFallback)
        {
            var floatLine = Math.Max(0, -finalRect.top / _container.ParsedMetrics.height);
            var line = floatLine.CeilingI();
            if (line == _currentLine)
                return;

            _currentLine = line;
            _layout?.Dispose();
            _layout = null;
        }
        base.ArrangeCore(finalRect);
    }

    protected override IComObject<IDWriteTextLayout>? CheckLayout(bool throwIfNull)
    {
        var layout = _layout;
        if ((layout == null || layout.IsDisposed) && throwIfNull)
            throw new WiceException("0023: Operation on '" + Name + "' of type '" + GetType().FullName + "' is invalid as it was not measured.");

        return layout;
    }

    // here maxHeight is the height of the text extent, but we want the height of the text window
    protected override IComObject<IDWriteTextLayout>? GetLayout(float maxWidth, float maxHeight)
    {
        if (HasFallback)
            return base.GetLayout(maxWidth, maxHeight);

        maxHeight = Math.Min(maxHeight, ParentsAbsoluteClipRect?.Height ?? 0);
        _layout?.Dispose();
        _layout = null;
        var lastLine = _currentLine;
        do
        {
            var text = _container.GetText(_currentLine, lastLine);
            if (text == null)
                return null;

            var layout = Application.Current.ResourceManager.CreateTextLayout(GetFormat(), text, 0, maxWidth, maxHeight);
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

        Interlocked.Exchange(ref _container, new TextContainer(this, text));
    }

    private sealed class TextContainer(FastTextBox visual, string? text)
    {
        public const char NotUnicode = '\uFFFF';

        private DWRITE_WORD_WRAPPING _parsedWrapping;
        private D2D_SIZE_F _parsedConstraint;
        private D2D_SIZE_F _parsedSize;
        public FastTextBox Visual = visual;
        public string Text = text ?? string.Empty;
        public Line[]? Lines;
        public Task? LoadingLines;
        public D2D_SIZE_F ParsedMetrics;

        // compute or estimate char width & line height
        private D2D_SIZE_F ComputeMetrics()
        {
            const string sample = "The quick brown fox jumps over the lazy dog";
            var format = Visual.GetFormat();
            using var layout = Application.Current.ResourceManager.CreateTextLayout(format, sample);
            var metrics = layout.GetMetrics1();
            return new D2D_SIZE_F(metrics.Base.width / sample.Length, metrics.Base.height);
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

            if (_parsedConstraint == constraint && _parsedWrapping == wrapping && !refresh)
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

                    lines.Add(line);
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

                    if (LoadingLines == null && lines.Count == Visual.DeferredParsingLineCountThreshold)
                    {
                        var start = i;
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
                                    lines.Add(line2);
                                }

                                Lines = [.. lines];

                                _parsedConstraint = constraint;
                                _parsedWrapping = wrapping;
                                _parsedSize = new D2D_SIZE_F(maxCharactersPerLine * ParsedMetrics.width, Lines.Length * ParsedMetrics.height);

                                e.LoadedLines = lines.Count;

                                Visual.OnLoaded(Visual, e);
                                Visual.Invalidate(VisualPropertyInvalidateModes.Measure);
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
                            lines.Add(line);
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
                            lines.Add(line);

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
                lines.Add(line);
            }

            Lines = [.. lines];

            _parsedConstraint = constraint;
            _parsedWrapping = wrapping;

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
                return Text[firstLine.Position..];

            var length = Lines[lastLineIndex].Position - firstLine.Position + Lines[lastLineIndex].Length;
            return Text.Substring(firstLine.Position, length);
        }
    }

    private readonly struct Line(int position, int length)
    {
        public int Position { get; } = position;
        public int Length { get; } = length;

        public override string ToString() => Position + " (" + Length + ")";
    }
}
