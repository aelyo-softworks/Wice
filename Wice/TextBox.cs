using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public partial class TextBox : RenderVisual, ITextFormat, ITextBoxProperties, IValueable, IPasswordCapable
    {
        public static VisualProperty ForegroundBrushProperty = VisualProperty.Add<Brush>(typeof(TextBox), nameof(ForegroundBrush), VisualPropertyInvalidateModes.Render, Application.CurrentTheme.TextBoxForegroundColor);
        public static VisualProperty HoverForegroundBrushProperty = VisualProperty.Add<Brush>(typeof(TextBox), nameof(HoverForegroundBrush), VisualPropertyInvalidateModes.Render);
        public static VisualProperty SelectionBrushProperty = VisualProperty.Add<Brush>(typeof(TextBox), nameof(SelectionBrush), VisualPropertyInvalidateModes.Render);
        public static VisualProperty FontFamilyNameProperty = VisualProperty.Add<string>(typeof(TextBox), nameof(FontFamilyName), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty FontWeightProperty = VisualProperty.Add(typeof(TextBox), nameof(FontWeight), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL);
        public static VisualProperty FontStretchProperty = VisualProperty.Add(typeof(TextBox), nameof(FontStretch), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL);
        public static VisualProperty FontStyleProperty = VisualProperty.Add(typeof(TextBox), nameof(FontStyle), VisualPropertyInvalidateModes.Measure, DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL);
        public static VisualProperty ParagraphAlignmentProperty = VisualProperty.Add(typeof(TextBox), nameof(ParagraphAlignment), VisualPropertyInvalidateModes.Measure, DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR);
        public static VisualProperty AlignmentProperty = VisualProperty.Add(typeof(TextBox), nameof(Alignment), VisualPropertyInvalidateModes.Measure, DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING);
        public static VisualProperty FlowDirectionProperty = VisualProperty.Add(typeof(TextBox), nameof(FlowDirection), VisualPropertyInvalidateModes.Measure, DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM);
        public static VisualProperty ReadingDirectionProperty = VisualProperty.Add(typeof(TextBox), nameof(ReadingDirection), VisualPropertyInvalidateModes.Measure, DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT);
        public static VisualProperty WordWrappingProperty = VisualProperty.Add(typeof(TextBox), nameof(WordWrapping), VisualPropertyInvalidateModes.Measure, DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);
        public static VisualProperty TrimmingGranularityProperty = VisualProperty.Add(typeof(TextBox), nameof(TrimmingGranularity), VisualPropertyInvalidateModes.Measure, DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_NONE);
        public static VisualProperty TextProperty = VisualProperty.Add<string>(typeof(TextBox), nameof(Text), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);
        public static VisualProperty FontCollectionProperty = VisualProperty.Add<IComObject<IDWriteFontCollection>>(typeof(TextBox), nameof(FontCollection), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty DrawOptionsProperty = VisualProperty.Add(typeof(TextBox), nameof(DrawOptions), VisualPropertyInvalidateModes.Render, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        public static VisualProperty AntiAliasingModeProperty = VisualProperty.Add(typeof(TextBox), nameof(AntiAliasingMode), VisualPropertyInvalidateModes.Render, D2D1_TEXT_ANTIALIAS_MODE.D2D1_TEXT_ANTIALIAS_MODE_DEFAULT);
        public static VisualProperty IsEditableProperty = VisualProperty.Add(typeof(TextBox), nameof(IsEditable), VisualPropertyInvalidateModes.Render, false);
        public static VisualProperty IsWheelZoomEnabledProperty = VisualProperty.Add(typeof(TextBox), nameof(IsWheelZoomEnabled), VisualPropertyInvalidateModes.Render, false);
        public static VisualProperty AcceptsTabProperty = VisualProperty.Add(typeof(TextBox), nameof(AcceptsTab), VisualPropertyInvalidateModes.None, false);
        public static VisualProperty AcceptsReturnProperty = VisualProperty.Add(typeof(TextBox), nameof(AcceptsReturn), VisualPropertyInvalidateModes.None, false);
        public static VisualProperty FontSizeProperty = VisualProperty.Add<float?>(typeof(TextBox), nameof(FontSize), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty TextRenderingParametersProperty = VisualProperty.Add<TextRenderingParameters>(typeof(TextBox), nameof(TextRenderingParameters), VisualPropertyInvalidateModes.Render);
        public static VisualProperty PasswordCharProperty = VisualProperty.Add<char?>(typeof(TextBox), nameof(PasswordCharacter), VisualPropertyInvalidateModes.Measure);

        private readonly object _rangesLock = new object();
        private readonly ConcurrentDictionary<FontRangeType, FontRanges> _ranges = new ConcurrentDictionary<FontRangeType, FontRanges>();

        // cache
        private IComObject<IDWriteTextLayout> _layout;
        private float _maxWidth;
        private float _maxHeight;
        private string _text;
        private bool _rendered;

        // many edit code here is taken from the PadWrite sample https://github.com/pauldotknopf/WindowsSDK7-Samples/tree/master/multimedia/DirectWrite/PadWrite
        private uint _charAnchor;
        private uint _charPosition;
        private uint _charPositionOffset;
        private CaretFormat _caretFormat;
        private D2D_POINT_2F _origin;

        private float _lastCaretX;
        private bool _selecting;
        private FontRanges[] _finalRanges;
        private TextBoxRenderMode _renderMode;
        private readonly UndoStack<UndoState> _undoStack = new UndoStack<UndoState>();
        private EventHandler<ValueEventArgs> _valueChanged;
        private bool _textHasChanged;

        event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { UIExtensions.AddEvent(ref _valueChanged, value); } remove { UIExtensions.RemoveEvent(ref _valueChanged, value); } }
        public event EventHandler<ValueEventArgs<string>> TextChanged;

        public TextBox()
        {
            RaiseTextChanged = true;
        }

        bool IValueable.CanChangeValue { get => IsEditable && IsEnabled; set => IsEditable = value; }
        object IValueable.Value => Text;
        bool IValueable.TrySetValue(object value)
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

        private class UndoState
        {
            public static UndoState From(TextBox tb)
            {
                var state = new UndoState();
                state.Text = tb.Text;
                state.CharAnchor = tb._charAnchor;
                state.CharPosition = tb._charPosition;
                state.CharPositionOffset = tb._charPositionOffset;
                state.CaretFormat = tb._caretFormat;
                state.Origin = tb._origin;
                return state;
            }

            public string Text;
            public uint CharAnchor;
            public uint CharPosition;
            public uint CharPositionOffset;
            public CaretFormat CaretFormat;
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

        protected override bool FallbackToTransparentBackground => true;
        protected override bool ShouldRender => !_rendered;

        [Browsable(false)]
        public virtual bool RaiseTextChanged { get; set; }

        [Browsable(false)]
        public virtual EventTrigger TextChangedTrigger { get; set; }

        [Category(CategoryBehavior)]
        public virtual TextBoxRenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                if (_renderMode == value)
                    return;

                _rendered = false;
                _renderMode = value;
            }
        }

        [Category(CategoryLayout)]
        public Brush ForegroundBrush { get => (Brush)GetPropertyValue(ForegroundBrushProperty); set => SetPropertyValue(ForegroundBrushProperty, value); }

        [Category(CategoryLayout)]
        public Brush HoverForegroundBrush { get => (Brush)GetPropertyValue(HoverForegroundBrushProperty); set => SetPropertyValue(HoverForegroundBrushProperty, value); }

        [Category(CategoryLayout)]
        public Brush SelectionBrush { get => (Brush)GetPropertyValue(SelectionBrushProperty); set => SetPropertyValue(SelectionBrushProperty, value); }

        [Category(CategoryLayout)]
        public string FontFamilyName { get => (string)GetPropertyValue(FontFamilyNameProperty); set => SetPropertyValue(FontFamilyNameProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_FONT_WEIGHT FontWeight { get => (DWRITE_FONT_WEIGHT)GetPropertyValue(FontWeightProperty); set => SetPropertyValue(FontWeightProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_FONT_STYLE FontStyle { get => (DWRITE_FONT_STYLE)GetPropertyValue(FontStyleProperty); set => SetPropertyValue(FontStyleProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_FONT_STRETCH FontStretch { get => (DWRITE_FONT_STRETCH)GetPropertyValue(FontStretchProperty); set => SetPropertyValue(FontStretchProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get => (DWRITE_PARAGRAPH_ALIGNMENT)GetPropertyValue(ParagraphAlignmentProperty); set => SetPropertyValue(ParagraphAlignmentProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_TEXT_ALIGNMENT Alignment { get => (DWRITE_TEXT_ALIGNMENT)GetPropertyValue(AlignmentProperty); set => SetPropertyValue(AlignmentProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_FLOW_DIRECTION FlowDirection { get => (DWRITE_FLOW_DIRECTION)GetPropertyValue(FlowDirectionProperty); set => SetPropertyValue(FlowDirectionProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_READING_DIRECTION ReadingDirection { get => (DWRITE_READING_DIRECTION)GetPropertyValue(ReadingDirectionProperty); set => SetPropertyValue(ReadingDirectionProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_WORD_WRAPPING WordWrapping { get => (DWRITE_WORD_WRAPPING)GetPropertyValue(WordWrappingProperty); set => SetPropertyValue(WordWrappingProperty, value); }

        [Category(CategoryLayout)]
        public DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get => (DWRITE_TRIMMING_GRANULARITY)GetPropertyValue(TrimmingGranularityProperty); set => SetPropertyValue(TrimmingGranularityProperty, value); }

        [Category(CategoryLayout)]
        public string Text { get => (string)GetPropertyValue(TextProperty) ?? string.Empty; set => SetPropertyValue(TextProperty, value); }

        [Category(CategoryLayout)]
        public IComObject<IDWriteFontCollection> FontCollection { get => (IComObject<IDWriteFontCollection>)GetPropertyValue(FontCollectionProperty); set => SetPropertyValue(FontCollectionProperty, value); }

        [Category(CategoryLayout)]
        public D2D1_DRAW_TEXT_OPTIONS DrawOptions { get => (D2D1_DRAW_TEXT_OPTIONS)GetPropertyValue(DrawOptionsProperty); set => SetPropertyValue(DrawOptionsProperty, value); }

        [Category(CategoryLayout)]
        public D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get => (D2D1_TEXT_ANTIALIAS_MODE)GetPropertyValue(AntiAliasingModeProperty); set => SetPropertyValue(AntiAliasingModeProperty, value); }

        [Category(CategoryBehavior)]
        public bool IsEditable { get => (bool)GetPropertyValue(IsEditableProperty); set => SetPropertyValue(IsEditableProperty, value); }

        [Category(CategoryBehavior)]
        public bool IsWheelZoomEnabled { get => (bool)GetPropertyValue(IsWheelZoomEnabledProperty); set => SetPropertyValue(IsWheelZoomEnabledProperty, value); }

        [Category(CategoryBehavior)]
        public bool AcceptsTab { get => (bool)GetPropertyValue(AcceptsTabProperty); set => SetPropertyValue(AcceptsTabProperty, value); }

        [Category(CategoryBehavior)]
        public bool AcceptsReturn { get => (bool)GetPropertyValue(AcceptsReturnProperty); set => SetPropertyValue(AcceptsReturnProperty, value); }

        [Category(CategoryLayout)]
        public float? FontSize { get => (float?)GetPropertyValue(FontSizeProperty); set => SetPropertyValue(FontSizeProperty, value); }

        [Category(CategoryLayout)]
        public TextRenderingParameters TextRenderingParameters { get => (TextRenderingParameters)GetPropertyValue(TextRenderingParametersProperty); set => SetPropertyValue(TextRenderingParametersProperty, value); }

        [Category(CategoryBehavior)]
        public char? PasswordCharacter { get => (char?)GetPropertyValue(PasswordCharProperty); set => SetPropertyValue(PasswordCharProperty, value); }

        public void SetFontWeight(DWRITE_FONT_WEIGHT weight) => SetFontWeight(weight, new DWRITE_TEXT_RANGE(0));
        public void SetFontWeight(DWRITE_FONT_WEIGHT weight, DWRITE_TEXT_RANGE range) => SetFontWeight(weight, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontWeight(DWRITE_FONT_WEIGHT weight, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontWeight, weight, ranges);

        public void SetFontStretch(DWRITE_FONT_STRETCH stretch) => SetFontStretch(stretch, new DWRITE_TEXT_RANGE(0));
        public void SetFontStretch(DWRITE_FONT_STRETCH stretch, DWRITE_TEXT_RANGE range) => SetFontStretch(stretch, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontStretch(DWRITE_FONT_STRETCH stretch, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontStretch, stretch, ranges);

        public void SetFontStyle(DWRITE_FONT_STYLE style) => SetFontStyle(style, new DWRITE_TEXT_RANGE(0));
        public void SetFontStyle(DWRITE_FONT_STYLE style, DWRITE_TEXT_RANGE range) => SetFontStyle(style, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontStyle(DWRITE_FONT_STYLE style, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontStyle, style, ranges);

        public void SetFontSize(float size) => SetFontSize(size, new DWRITE_TEXT_RANGE(0));
        public void SetFontSize(float size, DWRITE_TEXT_RANGE range) => SetFontSize(size, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontSize(float size, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontSize, size, ranges);

        public void SetFontFamilyName(string name) => SetFontFamilyName(name, new DWRITE_TEXT_RANGE(0));
        public void SetFontFamilyName(string name, DWRITE_TEXT_RANGE range) => SetFontFamilyName(name, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontFamilyName(string name, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontFamilyName, name, ranges);

        public void SetFontCollection(string collection) => SetFontCollection(collection, new DWRITE_TEXT_RANGE(0));
        public void SetFontCollection(string collection, DWRITE_TEXT_RANGE range) => SetFontCollection(collection, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetFontCollection(string collection, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.FontCollection, collection, ranges);

        public void SetStrikethrough(bool strikethrough) => SetStrikethrough(strikethrough, new DWRITE_TEXT_RANGE(0));
        public void SetStrikethrough(bool strikethrough, DWRITE_TEXT_RANGE range) => SetStrikethrough(strikethrough, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetStrikethrough(bool strikethrough, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Strikethrough, strikethrough, ranges);

        public void SetUnderline(bool underline) => SetUnderline(underline, new DWRITE_TEXT_RANGE(0));
        public void SetUnderline(bool underline, DWRITE_TEXT_RANGE range) => SetUnderline(underline, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetUnderline(bool underline, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Underline, underline, ranges);

        public void SetLocaleName(string name) => SetLocaleName(name, new DWRITE_TEXT_RANGE(0));
        public void SetLocaleName(string name, DWRITE_TEXT_RANGE range) => SetLocaleName(name, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetLocaleName(string name, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.LocaleName, name, ranges);

        public void SetTypography(IDWriteTypography typography) => SetTypography(typography, new DWRITE_TEXT_RANGE(0));
        public void SetTypography(IDWriteTypography typography, DWRITE_TEXT_RANGE range) => SetTypography(typography, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetTypography(IDWriteTypography typography, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.Typography, typography, ranges);

        public void SetInlineObject(IDWriteInlineObject inlineObject) => SetInlineObject(inlineObject, new DWRITE_TEXT_RANGE(0));
        public void SetInlineObject(IDWriteInlineObject inlineObject, DWRITE_TEXT_RANGE range) => SetInlineObject(inlineObject, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetInlineObject(IDWriteInlineObject inlineObject, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.InlineObject, inlineObject, ranges);

        public void SetDrawingEffect(object drawingEffect) => SetDrawingEffect(drawingEffect, new DWRITE_TEXT_RANGE(0));
        public void SetDrawingEffect(object drawingEffect, DWRITE_TEXT_RANGE range) => SetDrawingEffect(drawingEffect, new DWRITE_TEXT_RANGE[] { range });
        public virtual void SetDrawingEffect(object drawingEffect, DWRITE_TEXT_RANGE[] ranges) => SetFontRangeValue(FontRangeType.DrawingEffect, drawingEffect, ranges);

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

        public override string ToString()
        {
            var text = base.ToString() + " '" + Text?.Replace('\r', '⏎').Replace("\n", string.Empty) + "'";
            return text.TrimWithEllipsis();
        }

        protected virtual void DoOnTextChanged(object sender, ValueEventArgs<string> e)
        {
            _textHasChanged = true;
            if (!RaiseTextChanged)
                return;

            if (TextChangedTrigger != EventTrigger.LostFocus)
            {
                OnTextChanged(sender, e);
            }
        }

        protected virtual void OnTextChanged(object sender, ValueEventArgs<string> e)
        {
            TextChanged?.Invoke(sender, e);
            _valueChanged?.Invoke(sender, e);
            _textHasChanged = false;
        }

        public override void Invalidate(VisualPropertyInvalidateModes modes, InvalidateReason reason)
        {
            if (modes != VisualPropertyInvalidateModes.None)
            {
                _rendered = false;
            }
            base.Invalidate(modes, reason);
        }

        protected override void OnDetachingFromComposition(object sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Reset();
        }

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
        }

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

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            _rendered = false;
            if (property == TextProperty)
            {
                DoOnTextChanged(this, new ValueEventArgs<string>((string)value));
                return true;
            }

            if (property == IsEditableProperty)
            {
                if (IsEditable)
                {
                    Edit(); // TODO : ??
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
                property == PasswordCharProperty)
            {
                Interlocked.Exchange(ref _layout, null)?.Dispose();
            }
            return true;
        }

        protected override void OnFocusedChanged(object sender, ValueEventArgs<bool> e)
        {
            base.OnFocusedChanged(sender, e);
            //#if DEBUG
            //            if (System.Diagnostics.Debugger.IsAttached)
            //            {
            //                if (!IsFocused)
            //                {
            //                    _selecting = false;
            //                }
            //                return;
            //            }
            //#endif

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

        private void SetCaretLocation() => DoWhenRendered(() => Window.RunTaskOnMainThread(() => DoSetCaretLocation()));
        private void DoSetCaretLocation()
        {
            if (!IsFocused || !IsEditable || !IsEnabled)
                return;

            var caret = Window.Caret;
            if (caret != null)
            {
                var padding = Padding;
                var rc = GetCaretRect();
                rc.Move(_origin);
                caret.Width = rc.Width;
                caret.Height = rc.Height;

                var ar = AbsoluteRenderRect;
                //Application.Trace(this + " ar: " + ar);
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
                //Application.Trace(this + " origin: " + _origin + " caret.Location: " + caret.Location);
            }
        }

        private void ShowCaret()
        {
            if (IsFocused && IsEditable && IsEnabled)
            {
                var caret = Window.Caret;
                if (caret != null)
                {
                    caret.IsShown = true;
                }
            }
        }

        private void HideCaret()
        {
            var caret = Window?.Caret;
            if (caret != null)
            {
                caret.IsShown = false;
            }
        }

        private void StopEdit()
        {
            HideCaret();
        }

        private void Edit()
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

        private IComObject<IDWriteTextFormat> GetFormat()
        {
            var format = Application.Current.ResourceManager.GetTextFormat(this);
            //Application.Trace(this + " fontsize:" + format.Object.GetFontSize());
            return format;
        }

        private IComObject<IDWriteTextLayout> GetLayout(float maxWidth, float maxHeight)
        {
#if DEBUG
            if (maxWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(maxWidth));

            if (maxHeight < 0)
                throw new ArgumentOutOfRangeException(nameof(maxHeight));
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
            _layout = Application.Current.ResourceManager.CreateTextLayout(GetFormat(), text, 0, maxWidth, maxHeight);
            //Application.Trace(this + " CreateTextLayout max:" + maxWidth + "x" + maxHeight);
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

            foreach (var fr in _finalRanges)
            {
                switch (fr.Type)
                {
                    case FontRangeType.FontWeight:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontWeight((DWRITE_FONT_WEIGHT)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.FontStretch:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontStretch((DWRITE_FONT_STRETCH)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.FontStyle:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontStyle((DWRITE_FONT_STYLE)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.LocaleName:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetLocaleName((string)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.Strikethrough:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetStrikethrough((bool)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.Underline:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetUnderline((bool)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.FontSize:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontSize((float)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.FontFamilyName:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontFamilyName((string)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.FontCollection:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetFontCollection((IDWriteFontCollection)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.DrawingEffect:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetDrawingEffect(range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.Typography:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetTypography((IDWriteTypography)range.Value, range.Range).ThrowOnError();
                        }
                        break;

                    case FontRangeType.InlineObject:
                        foreach (var range in fr.Ranges)
                        {
                            _layout.Object.SetInlineObject((IDWriteInlineObject)range.Value, range.Range).ThrowOnError();
                        }
                        break;
                }
            }
        }

        // note we don't honor width & height = float.PositiveInfinity if layout is not null (which is generally the case)
        // if float.MaxValue is not set, we always report the text metrics (the place we take)
        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            if (RenderMode == TextBoxRenderMode.DrawText)
            {
                _rendered = false;
                return new D2D_SIZE_F();
            }

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

            var layout = GetLayout(constraint.width, constraint.height);
            var metrics = layout.GetMetrics1();

            // note: always return integral w & h when computed by dwrite
            var width = metrics.widthIncludingTrailingWhitespace.Ceiling();
            var height = metrics.height.Ceiling();

            if (IsEditable && IsFocused)
            {
                var caret = Window.Caret;
                if (caret != null)
                {
                    var rc = GetCaretRect();
                    caret.Width = rc.Width;
                    caret.Height = rc.Height;
                }

                if (caret.Width.IsSet())
                {
                    width = Math.Max(width, caret.Width);
                }

                if (caret.Height.IsSet())
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

            //Application.Trace("w:" + width + " h:" + height);
            return new D2D_SIZE_F(width, height);
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            //Application.Trace(this.ToString());

            if (RenderMode == TextBoxRenderMode.DrawText)
            {
                _rendered = false;
                return;
            }

            var padding = Padding;
            if (padding.left.IsSet() && padding.left > 0)
            {
                finalRect.left += padding.left;
            }

            if (padding.top.IsSet() && padding.top > 0)
            {
                finalRect.top += padding.top;
            }

            if (padding.right.IsSet() && padding.right > 0)
            {
                finalRect.right = Math.Max(finalRect.left, finalRect.right - padding.right);
            }

            if (padding.bottom.IsSet() && padding.bottom > 0)
            {
                finalRect.bottom = Math.Max(finalRect.top, finalRect.bottom - padding.bottom);
            }

            var finalSize = finalRect.Size;
            var layout = GetLayout(finalSize.width, finalSize.height)?.Object;
            var metrics = layout.GetMetrics1();

            var size = new D2D_SIZE_F(metrics.widthIncludingTrailingWhitespace, metrics.height);
            if (HorizontalAlignment == Wice.Alignment.Stretch)
            {
                size.width = finalSize.width;
            }

            if (VerticalAlignment == Wice.Alignment.Stretch)
            {
                size.height = finalSize.height;
            }

            // when resized, make sure we display the maximum we can, so adjust the origin
            //if ((size.height - _origin.y) >= metrics.height)
            //{
            //    SetOriginY(size.height - metrics.height);
            //}

            //if ((size.width - _origin.x) >= metrics.width)
            //{
            //    SetOriginX(size.width - metrics.width);
            //}
            SetCaretLocation();
        }

        private void EnsureCaretWidthVisible(ref D2D_RECT_F rc, float viewWidth)
        {
            Application.Trace("crc: " + rc + " w:" + viewWidth);

            // if caret is on far right, make sure it's displayed with its width
            if (rc.right > viewWidth)
            {
                Application.Trace("crc: " + rc + " w:" + viewWidth);
                rc.Move(new D2D_VECTOR_2F(viewWidth - rc.right, 0));
                Application.Trace("=> crc: " + rc);
            }

            // if caret is on far left, make sure it's displayed with its width
            if (rc.left < 0)
            {
                rc.Move(new D2D_VECTOR_2F(-rc.left, 0));
            }
        }

        private IComObject<ID2D1Brush> GetSelectionBrush(RenderContext context, IComObject<ID2D1Brush> brush)
        {
            var selection = SelectionBrush?.GetBrush(context);
            if (selection != null)
                return selection;

            if (brush == null || !(brush.Object is ID2D1SolidColorBrush colorBrush))
                return context.CreateSolidColorBrush(Application.CurrentTheme.TextBoxSelectionColor);

            var bg = AscendantsBackgroundColor;
            if (!bg.HasValue)
                return context.CreateSolidColorBrush(Application.CurrentTheme.TextBoxSelectionColor);

            colorBrush.GetColor(out var color);

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

        protected internal override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);

            IComObject<ID2D1Brush> brush;
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
                Application.Trace(this + " has no brush defined.");
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

            if (RenderMode == TextBoxRenderMode.DrawText)
            {
                var text = PasswordText;
                if (string.IsNullOrEmpty(text))
                {
                    Application.Trace(this + " has no text defined.");
                    return;
                }

                context.DeviceContext.DrawText(text, GetFormat(), new D2D_RECT_F(rr), brush, DrawOptions, DWRITE_MEASURING_MODE.DWRITE_MEASURING_MODE_NATURAL);
                _rendered = true;
                return;
            }

            var layout = GetLayout(rr.width, rr.height);

            context.DeviceContext.PushAxisAlignedClip(clip);
            try
            {
                // draw selection
                var caretRange = GetSelectionRange();
                if (caretRange.length > 0)
                {
                    layout.Object.HitTestTextRange(
                        caretRange.startPosition,
                        caretRange.length,
                        origin.x,
                        origin.y,
                        null,
                        0,
                        out var actualHitTestCount
                        ); // no error check

                    if (actualHitTestCount > 0)
                    {
                        var hitTestMetrics = new DWRITE_HIT_TEST_METRICS[actualHitTestCount];
                        layout.Object.HitTestTextRange(
                            caretRange.startPosition,
                            caretRange.length,
                            origin.x,
                            origin.y,
                            hitTestMetrics,
                            hitTestMetrics.Length,
                            out _
                            ).ThrowOnError();

                        // Note that an ideal layout will return fractional values, so you may see slivers between the selection ranges,
                        // due to the per-primitive antialiasing of the edges unless it is disabled (better for performance anyway).
                        context.DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
                        foreach (var htm in hitTestMetrics)
                        {
                            var highlightRect = new D2D_RECT_F(htm.left, htm.top, htm.left + htm.width, htm.top + htm.height);
                            context.DeviceContext.Object.FillRectangle(ref highlightRect, GetSelectionBrush(context, brush).Object);
                        }

                        context.DeviceContext.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
                    }
                }

                var trpMode = false;
                var trp = TextRenderingParameters;
                if (trp != null)
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
                context.DeviceContext.PopAxisAlignedClip();
            }
            _rendered = true;
        }

        protected override void OnKeyUp(object sender, KeyEventArgs e)
        {
            base.OnKeyUp(sender, e);
            if (IsFocused)
            {
                var caret = Window.Caret;
                if (caret != null)
                {
                    caret.StartBlinking();
                }
            }
        }

        private void PostponeCaret()
        {
            if (IsFocused)
            {
                var caret = Window.Caret;
                if (caret != null)
                {
                    caret.IsShown = true;
                    caret.StartBlinking();
                }
            }
        }

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(sender, e);
            if (!IsEditable || !IsEnabled || !IsFocused)
                return;

            var shift = NativeWindow.IsKeyPressed(VirtualKeys.ShiftKey);
            var control = NativeWindow.IsKeyPressed(VirtualKeys.ControlKey);
            var key = e.Key;

            // paste is special as it uses an STA thread first
            if (IsEditable && ((control && key == VirtualKeys.V) || (shift && key == VirtualKeys.Insert)))
            {
                PasteFromClipboard();
                PostponeCaret();
                e.Handled = true;
                return;
            }

            if (key == VirtualKeys.Left || key == VirtualKeys.Right || key == VirtualKeys.Up || key == VirtualKeys.Down ||
                key == VirtualKeys.Home || key == VirtualKeys.End || key == VirtualKeys.PageUp || key == VirtualKeys.PageDown ||
                key == VirtualKeys.C || key == VirtualKeys.X || key == VirtualKeys.A ||
                key == VirtualKeys.Y || key == VirtualKeys.Z ||
                (key == VirtualKeys.Tab && AcceptsTab) || (key == VirtualKeys.Return && AcceptsReturn) ||
                key == VirtualKeys.Back || key == VirtualKeys.Delete || key == VirtualKeys.Insert)
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
                case VirtualKeys.Left:
                    if (SetSelection(control ? TextBoxSetSelection.LeftWord : TextBoxSetSelection.Left, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Right:
                    if (SetSelection(control ? TextBoxSetSelection.RightWord : TextBoxSetSelection.Right, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Up:
                    if (SetSelection(TextBoxSetSelection.Up, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Down:
                    if (SetSelection(TextBoxSetSelection.Down, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.PageUp:
                    if (SetSelection(TextBoxSetSelection.PageUp, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.PageDown:
                    if (SetSelection(TextBoxSetSelection.PageDown, 1, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Home:
                    if (SetSelection(control ? TextBoxSetSelection.First : TextBoxSetSelection.Home, 0, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.End:
                    if (SetSelection(control ? TextBoxSetSelection.Last : TextBoxSetSelection.End, 0, shift))
                    {
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.C:
                    if (control)
                    {
                        CopyToClipboard();
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.X:
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

                case VirtualKeys.A:
                    if (control)
                    {
                        SetSelection(TextBoxSetSelection.All, 0, true);
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Y:
                    if (control)
                    {
                        Redo();
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Z:
                    if (control)
                    {
                        Undo();
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Tab:
                    if (AcceptsTab && !control) // ctrl + tab is used to change focus
                    {
                        DeleteSelection();
                        InsertTextAt(absolutePosition, "\t", _caretFormat);
                        SetSelection(TextBoxSetSelection.AbsoluteLeading, (uint)(absolutePosition + 1), false, false);
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Insert:
                    if (control)
                    {
                        CopyToClipboard();
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Return:
                    if (IsEditable)
                    {
                        // Insert CR/LF pair
                        DeleteSelection();
                        InsertTextAt(absolutePosition, Environment.NewLine, _caretFormat);
                        SetSelection(TextBoxSetSelection.AbsoluteLeading, (uint)(absolutePosition + Environment.NewLine.Length), false, false);
                        e.Handled = true;
                    }
                    break;

                case VirtualKeys.Back:
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

                case VirtualKeys.Delete:
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
                        _layout.Object.HitTestTextPosition(
                            absolutePosition,
                            false,
                            out _,
                            out _,
                            out var hitTestMetrics
                            ).ThrowOnError();

                        RemoveTextAt(hitTestMetrics.textPosition, hitTestMetrics.length);
                        SetSelection(TextBoxSetSelection.AbsoluteLeading, hitTestMetrics.textPosition, false);
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

        private float GetFontSize() => Application.Current.ResourceManager.GetFontSize(FontSize);

        protected override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnMouseWheel(sender, e);
            if (!IsEnabled || !IsWheelZoomEnabled)
                return;

            var control = NativeWindow.IsKeyPressed(VirtualKeys.ControlKey);
            var shift = NativeWindow.IsKeyPressed(VirtualKeys.ShiftKey);
            if (control && shift)
            {
                var fsize = GetFontSize();
                var size = Math.Max(1, fsize + e.Delta);
                if (size != fsize)
                {
                    e.Handled = true;
                    FontSize = size;
                    //if (Parent is IScrollView)
                    //{
                    //    Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
                    //}
                }
                return;
            }

            //if (Parent is IScrollView)
            //    return;

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

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseButtonDown(sender, e);
            if (!IsEnabled)
                return;

            if (e.Button == MouseButton.Left)
            {
                _selecting = true;
                //e.Handled = true;
                Window?.CaptureMouse(this);
                var shift = NativeWindow.IsKeyPressed(VirtualKeys.ShiftKey);
                SetSelectionFromPoint(e, shift);
            }
        }

        protected override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseButtonUp(sender, e);
            if (e.Button == MouseButton.Left)
            {
                Window?.ReleaseMouseCapture(this);
                _selecting = false;
            }
        }

        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);
            if (!IsEnabled)
                return;

            //#if DEBUG
            //            if (System.Diagnostics.Debugger.IsAttached)
            //            {
            //                _selecting = false;
            //            }
            //#endif

            if (_selecting)
            {
                SetSelectionFromPoint(e, true);
            }
        }

        private bool HandleChar(KeyPressEventArgs e)
        {
            //if (e.UTF32Character == '\t' && AcceptsTab)
            //    return true;

            return e.UTF16Character >= ' ';
        }

        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
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

        public void RemoveText(int position, int? lengthToRemove = null)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            RemoveTextAt((uint)position, (uint?)lengthToRemove);
        }

        public void Select(TextBoxSetSelection mode, int? advance = 0, bool extend = false)
        {
            SetSelection(mode, (uint?)advance, extend);
        }

        public virtual bool Undo()
        {
            if (!_undoStack.TryUndo(UndoState.From(this), out var undo))
                return false;

            undo.Apply(this);
            return true;
        }

        public virtual bool Redo()
        {
            if (!_undoStack.TryRedo(UndoState.From(this), out var redo))
                return false;

            redo.Apply(this);
            return true;
        }

        public virtual void CopyToClipboard()
        {
            var selection = GetSelectionRange();
            if (selection.length == 0)
                return;

            var text = Text?.Substring((int)selection.startPosition, (int)selection.length);
            if (string.IsNullOrEmpty(text))
                return;

            TaskUtilities.RunWithSTAThread(() => System.Windows.Forms.Clipboard.SetText(text));
        }

        public virtual void PasteFromClipboard()
        {
            var text = TaskUtilities.RunWithSTAThread(() => System.Windows.Forms.Clipboard.GetText()).Result;
            if (text == null)
                return;

            if (!AcceptsReturn)
            {
                var pos = text.IndexOfAny(new[] { '\r', '\n' });
                if (pos == 0)
                    return;

                if (pos > 0)
                {
                    text = text.Substring(0, pos);
                }
            }

            DeleteSelection();
            InsertTextAt(_charPosition + _charPositionOffset, text);
            SetSelection(TextBoxSetSelection.RightChar, (uint)text.Length, false);
        }

        private float MirrorXCoordinate(float x)
        {
            // On RTL builds, coordinates may need to be restored to or converted from Cartesian coordinates, where x increases positively to the right.
            var style = Window.ExtendedStyle;
            if (style.HasFlag(WS_EX.WS_EX_LAYOUTRTL))
            {
                var rect = Window.ClientRect;
                return rect.right - x - 1;
            }
            return x;
        }

        private void InsertTextAt(uint position, string textToInsert, CaretFormat caretFormat = null)
        {
            var text = Text ?? string.Empty;
            var oldLayout = _layout;
            var oldTextLength = text.Length;
            position = (uint)Math.Min(position, text.Length);
            text = text.Insert((int)position, textToInsert);

            if (_layout != null)
            {
                var newLayout = RecreateLayout(text);

                CopyGlobalProperties(oldLayout.Object, newLayout.Object);

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

            CopyGlobalProperties(oldLayout.Object, newLayout.Object);

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

        private IComObject<IDWriteTextLayout> RecreateLayout(string text)
        {
            var w = _layout.Object.GetMaxWidth();
            var h = _layout.Object.GetMaxHeight();
            return Application.Current.ResourceManager.GetTextLayout(_layout.Object, text, maxWidth: w, maxHeight: h);
        }

        private void CopyGlobalProperties(IDWriteTextLayout oldLayout, IDWriteTextLayout newLayout)
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
                newLayout.SetTrimming(ref trimmingOptions, inlineObject);
                Marshal.ReleaseComObject(inlineObject);
            }

            oldLayout.GetLineSpacing(out var lineSpacingMethod, out var lineSpacing, out var baseline);
            newLayout.SetLineSpacing(lineSpacingMethod, lineSpacing, baseline);
        }

        private uint CalculateRangeLengthAt(IDWriteTextLayout layout, uint pos)
        {
            // Determines the length of a block of similarly formatted properties.
            // Use the first getter to get the range to increment the current position.
            var incrementAmount = new DWRITE_TEXT_RANGE(pos, 1);
            using (var mem = new ComMemory(incrementAmount))
            {
                layout.GetFontWeight(pos, out var weight, mem.Pointer);
            }

            return incrementAmount.length - (pos - incrementAmount.startPosition);
        }

        private void CopyRangedProperties(IDWriteTextLayout oldLayout, uint startPos, uint endPos, uint newLayoutTextOffset, IDWriteTextLayout newLayout, bool isOffsetNegative = false)
        {
            // Copies properties that set on ranges.
            var currentPos = startPos;
            while (currentPos < endPos)
            {
                uint rangeLength = CalculateRangeLengthAt(oldLayout, currentPos);
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

        private void CopySinglePropertyRange(IDWriteTextLayout oldLayout, uint startPosForOld, IDWriteTextLayout newLayout, uint startPosForNew, uint length, CaretFormat caretFormat = null)
        {
            // Copies a single range of similar properties, from one old layout to a new one.
            var range = new DWRITE_TEXT_RANGE(startPosForNew, Math.Min(length, uint.MaxValue - startPosForNew));

            // font collection
            if (oldLayout.GetFontCollection(startPosForOld, out var fontCollection, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetFontCollection(fontCollection, range);
                Marshal.ReleaseComObject(fontCollection);
            }

            if (caretFormat != null)
            {
                newLayout.SetFontFamilyName(caretFormat.fontFamilyName, range);
                newLayout.SetLocaleName(caretFormat.localeName, range);
                newLayout.SetFontWeight(caretFormat.fontWeight, range);
                newLayout.SetFontStyle(caretFormat.fontStyle, range);
                newLayout.SetFontStretch(caretFormat.fontStretch, range);
                newLayout.SetFontSize(caretFormat.fontSize, range);
                newLayout.SetUnderline(caretFormat.hasUnderline, range);
                newLayout.SetStrikethrough(caretFormat.hasStrikethrough, range);
            }
            else
            {
                // font family
                var fontFamilyName = oldLayout.GetFontFamilyName(startPosForOld).Nullify();
                if (fontFamilyName != null)
                {
                    newLayout.SetFontFamilyName(fontFamilyName, range);
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
                    newLayout.SetLocaleName(locale, range);
                }
            }

            // drawing effect
            if (oldLayout.GetDrawingEffect(startPosForOld, out var drawingEffect, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetDrawingEffect(drawingEffect, range);
                if (drawingEffect != null)
                {
                    Marshal.ReleaseComObject(drawingEffect);
                }
            }

            // inline object
            if (oldLayout.GetInlineObject(startPosForOld, out var inlineObject, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetInlineObject(inlineObject, range);
                if (inlineObject != null)
                {
                    Marshal.ReleaseComObject(inlineObject);
                }
            }

            // typography
            if (oldLayout.GetTypography(startPosForOld, out var typography, IntPtr.Zero).IsSuccess)
            {
                newLayout.SetTypography(typography, range);
                if (typography != null)
                {
                    Marshal.ReleaseComObject(typography);
                }
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
            if (_layout == null || _layout.IsDisposed)
                return false;

            // Returns the text position corresponding to the mouse x,y.
            // If hitting the trailing side of a cluster, return the leading edge of the following text position.

            // Remap display coordinates to actual.
            var pos = e.GetPosition(this);
            pos.x = MirrorXCoordinate(pos.x);
            pos.x -= _origin.x;
            pos.y -= _origin.y;

            _layout.Object.HitTestPoint(
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
            Application.CheckRunningAsMainThread();
            // Moves the caret relatively or absolutely, optionally extending the selection range (for example, when shift is held).
            uint line;// = uint.MaxValue; // current line number, needed by a few modes
            uint absolutePosition = _charPosition + _charPositionOffset;
            uint oldAbsolutePosition = absolutePosition;
            uint oldCaretAnchor = _charAnchor;
            DWRITE_HIT_TEST_METRICS hitTestMetrics;
            DWRITE_LINE_METRICS[] lineMetrics;
            float caretX;
            float caretY;
            bool isTrailingHit;

            var text = Text ?? string.Empty;
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
                        _layout.Object.HitTestTextPosition(_charPosition, false, out _, out _, out hitTestMetrics).ThrowOnError();
                        _charPosition = Math.Min(_charPosition, hitTestMetrics.textPosition + hitTestMetrics.length);
                    }
                    break;

                case TextBoxSetSelection.Up:
                case TextBoxSetSelection.Down:
                    // Retrieve the line metrics to figure out what line we are on.
                    lineMetrics = GetLineMetrics();

                    GetLineFromPosition(lineMetrics, (uint)lineMetrics.Length, _charPosition, out line, out var linePosition);

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
                    _layout.Object.HitTestTextPosition(
                        _charPosition,
                        _charPositionOffset > 0, // trailing if nonzero, else leading edge
                        out caretX,
                        out _,
                        out _
                        ).ThrowOnError();

                    // Get y of new position
                    _layout.Object.HitTestTextPosition(
                        linePosition,
                        false, // leading edge
                        out _,
                        out caretY,
                        out _
                        ).ThrowOnError();

                    // Now get text position of new x,y.
                    _layout.Object.HitTestPoint(
                        Math.Max(caretX, _lastCaretX), // use last horizontal caret position (like many editors, not notepad)
                        caretY,
                        out isTrailingHit,
                        out _,
                        out hitTestMetrics
                        ).ThrowOnError();

                    _charPosition = hitTestMetrics.textPosition;
                    _charPositionOffset = (uint)(isTrailingHit ? ((hitTestMetrics.length > 0) ? 1 : 0) : 0);
                    break;

                case TextBoxSetSelection.PageUp:
                case TextBoxSetSelection.PageDown:
                    var pos = RelativeRenderRect - Margin;
                    var crc = GetCaretRect();
                    var top = crc.top + (moveMode == TextBoxSetSelection.PageUp ? -pos.Height : +pos.Height);
                    _layout.Object.HitTestPoint(
                        Math.Max(crc.left, _lastCaretX),
                        top,
                        out isTrailingHit,
                        out _,
                        out hitTestMetrics
                        ).ThrowOnError();

                    _charPosition = hitTestMetrics.textPosition;
                    _charPositionOffset = (uint)(isTrailingHit ? ((hitTestMetrics.length > 0) ? 1 : 0) : 0);
                    break;

                case TextBoxSetSelection.LeftWord:
                case TextBoxSetSelection.RightWord:
                    // To navigate by whole words, we look for the canWrapLineAfter flag in the cluster metrics.
                    // First need to know how many clusters there are.
                    _layout.Object.GetClusterMetrics(null, 0, out var clusterCount); // don't check error by design
                    if (clusterCount == 0)
                        break;

                    // Now we actually read them.
                    var clusterMetrics = new DWRITE_CLUSTER_METRICS[clusterCount];
                    _layout.Object.GetClusterMetrics(clusterMetrics, (int)clusterCount, out _).ThrowOnError();

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
                            if (clusterMetrics[cluster].canWrapLineAfter != 0)
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
                            uint clusterLength = clusterMetrics[cluster].length;
                            _charPosition = clusterPosition;
                            _charPositionOffset = clusterLength; // trailing edge
                            if (clusterPosition >= oldCaretPosition && clusterMetrics[cluster].canWrapLineAfter != 0)
                                break; // first stopping point after old position.

                            clusterPosition += clusterLength;
                        }
                    }
                    break;

                case TextBoxSetSelection.Home:
                case TextBoxSetSelection.End:
                    // Retrieve the line metrics to know first and last positionon the current line.
                    lineMetrics = GetLineMetrics();

                    GetLineFromPosition(lineMetrics, (uint)lineMetrics.Length, _charPosition, out line, out _charPosition);

                    _charPositionOffset = 0;
                    if (moveMode == TextBoxSetSelection.End)
                    {
                        // Place the caret at the last character on the line, excluding line breaks. In the case of wrapped lines, newlineLength will be 0.
                        uint lineLength = lineMetrics[line].length - lineMetrics[line].newlineLength;
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

            if (moveMode != TextBoxSetSelection.Up && moveMode != TextBoxSetSelection.Down && _layout != null && !_layout.IsDisposed)
            {
                // remember max last horizontal position (mimic many editors, not like notepad/standard editbox)
                _layout.Object.HitTestTextPosition(
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

            //EnsureCaretWidthVisible(ref caretRc, rr.width);

            //if (Parent is IScrollView scrollView)
            //{
            //    var margin = Margin;
            //    var view = scrollView.ViewSize;
            //    if (caretRc.bottom + margin.top > (scrollView.VerticalOffset + view.height))
            //    {
            //        scrollView.VerticalOffset = caretRc.bottom + margin.top - view.height;
            //    }
            //    else if ((caretRc.top + margin.top) < scrollView.VerticalOffset)
            //    {
            //        scrollView.VerticalOffset = caretRc.top + margin.top;
            //    }

            //    if ((caretRc.right + margin.left) >= (scrollView.HorizontalOffset + view.width))
            //    {
            //        scrollView.HorizontalOffset = caretRc.right + margin.left - view.width;
            //    }
            //    else if ((caretRc.left + margin.left) < scrollView.HorizontalOffset)
            //    {
            //        scrollView.HorizontalOffset = caretRc.left + margin.left;
            //    }
            //}
            //else
            {
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
            }
        }

        private void UpdateCaretFormatting()
        {
            if (_layout == null || _layout.IsDisposed)
                return;

            uint currentPos = _charPosition + _charPositionOffset;

            if (currentPos > 0)
            {
                --currentPos; // Always adopt the trailing properties.
            }

            _caretFormat = _caretFormat ?? new CaretFormat();

            _caretFormat.fontFamilyName = _layout.Object.GetFontFamilyName(currentPos);
            _caretFormat.localeName = _layout.Object.GetLocaleName(currentPos);

            _layout.Object.GetFontWeight(currentPos, out _caretFormat.fontWeight, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetFontStyle(currentPos, out _caretFormat.fontStyle, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetFontStretch(currentPos, out _caretFormat.fontStretch, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetFontSize(currentPos, out _caretFormat.fontSize, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetUnderline(currentPos, out _caretFormat.hasUnderline, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetStrikethrough(currentPos, out _caretFormat.hasStrikethrough, IntPtr.Zero).ThrowOnError();
            _layout.Object.GetDrawingEffect(currentPos, out var drawingEffect, IntPtr.Zero).ThrowOnError();

            // TODO! change this color
            _caretFormat.color = _D3DCOLORVALUE.Pink;
            if (drawingEffect != null)
            {
                var brush = drawingEffect as ID2D1SolidColorBrush;
                brush.GetColor(out var color);
                _caretFormat.color = color;
                Marshal.ReleaseComObject(drawingEffect);
            }
        }

        private D2D_RECT_F GetCaretRect()
        {
            // Gets the current caret position (in untransformed space).
            var caretX = 0f;
            var caretY = 0f;
            var caretMetrics = new DWRITE_HIT_TEST_METRICS();
            caretMetrics.height = GetFontSize(); // just for default
            if (_layout != null)
            {
                // Translate text character offset to point x,y.
                _layout.Object.HitTestTextPosition(
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
                    _layout.Object.HitTestTextRange(
                        _charPosition,
                        0, // length
                        0, // x
                        0, // y
                        metrics,
                        metrics.Length,
                        out _
                        ).ThrowOnError();

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

            var rc = new D2D_RECT_F();
            rc.left = caretX - caretWidth / 2f;
            rc.right = rc.left + caretWidth;
            rc.top = caretY;
            rc.bottom = caretY + caretHeight;
            //Application.Trace("caretWidth=" + caretWidth + " rc=" + rc + " rcf=" + rc.ToFloorCeiling());
            return rc;
        }

        private DWRITE_TEXT_RANGE GetSelectionRange()
        {
            // Returns a valid range of the current selection, regardless of whether the caret or anchor is first.
            var caretBegin = _charAnchor;
            var caretEnd = _charPosition + _charPositionOffset;
            if (caretBegin > caretEnd)
            {
                var tmp = caretBegin;
                caretBegin = caretEnd;
                caretEnd = tmp;
            }

            // Limit to actual text length.
            var textLength = (uint)(Text?.Length).GetValueOrDefault();
            caretBegin = Math.Min(caretBegin, textLength);
            caretEnd = Math.Min(caretEnd, textLength);
            return new DWRITE_TEXT_RANGE(caretBegin, caretEnd - caretBegin);
        }

        private void CheckLayout()
        {
            if (_layout == null)
                throw new WiceException("0023: Operation on '" + Name + "' of type '" + GetType().FullName + "' is invalid as it was not measured.");
        }

        private DWRITE_LINE_METRICS[] GetLineMetrics()
        {
            CheckLayout();

            // Retrieves the line metrics, used for caret navigation, up/down and home/end.
            var textMetrics = _layout.GetMetrics1();
            var lineMetrics = new DWRITE_LINE_METRICS[textMetrics.lineCount];
            _layout.Object.GetLineMetrics(lineMetrics, lineMetrics.Length, out _).ThrowOnError();
            return lineMetrics;
        }

        private void GetLineFromPosition(DWRITE_LINE_METRICS[] lineMetrics, uint lineCount, uint textPosition, out uint lineOut, out uint linePositionOut)
        {
            // Given the line metrics, determines the current line and starting text position of that line by summing up the lengths.
            // When the startingline position is beyond the given text position, we have our line.
            uint line = 0;
            uint linePosition = 0;
            uint nextLinePosition = 0;
            for (; line < lineCount; ++line)
            {
                linePosition = nextLinePosition;
                nextLinePosition = linePosition + lineMetrics[line].length;

                // The next line is beyond the desired text position, so it must be in the current line
                if (nextLinePosition > textPosition)
                    break;
            }

            linePositionOut = linePosition;
            lineOut = Math.Min(line, lineCount - 1);
        }

        private void AlignCaretToNearestCluster(bool isTrailingHit = false, bool skipZeroWidth = false)
        {
            // Uses hit-testing to align the current caret position to a whole cluster, rather than residing in the middle of a base character + diacritic, surrogate pair, or character + UVS.
            // Align the caret to the nearest whole cluster.
            var hitTestMetrics = new DWRITE_HIT_TEST_METRICS { length = 1 };
            if (_layout != null && !_layout.IsDisposed)
            {
                _layout.Object.HitTestTextPosition(_charPosition, false, out _, out _, out hitTestMetrics).ThrowOnError();
            }

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
        }

        private void SetFontRangeValue(FontRangeType type, object value, DWRITE_TEXT_RANGE[] ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            if (!_ranges.TryGetValue(type, out var fontRanges))
            {
                fontRanges = new FontRanges();
                fontRanges.Type = type;
                fontRanges = _ranges.AddOrUpdate(type, fontRanges, (k, o) => o);
            }

            FontRanges[] list = null;
            lock (_rangesLock)
            {
                bool changed = false;
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
                    int i = 0;
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

                        if (fontRange.Value.Equals(value))
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
                    list = _ranges.Values.ToArray();
                }
            }

            if (list != null)
            {
                _finalRanges = list;
                Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
            }
        }

        private class FontRange
        {
            public DWRITE_TEXT_RANGE Range;
            public object Value;

            public bool EqualsValue(object value)
            {
                if (value is null)
                    return Value is null;

                return value.Equals(Value);
            }

            public override string ToString() => Range + " => " + Value;
        }

        private class FontRanges
        {
            public FontRangeType Type;
            public List<FontRange> Ranges = new List<FontRange>(); // note this should always sorted by construction

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

                    return Ranges[Ranges.Count - 1].Range.EndPosition;
                }
            }
        }

        private class CaretFormat
        {
            // the important range based properties for the current caret.
            // note these are stored outside the layout, since the current caret actually has a format, independent of the text it lies between.
            public string fontFamilyName;
            public string localeName;
            public float fontSize;
            public DWRITE_FONT_WEIGHT fontWeight;
            public DWRITE_FONT_STRETCH fontStretch;
            public DWRITE_FONT_STYLE fontStyle;
            public _D3DCOLORVALUE color;
            public bool hasUnderline;
            public bool hasStrikethrough;
        }
    }
}
