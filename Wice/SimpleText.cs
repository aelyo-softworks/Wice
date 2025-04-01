using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Threading;
using DirectN;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class SimpleText : RenderVisual, ITextFormat, ITextBoxProperties, IValueable
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
        public static VisualProperty IsWheelZoomEnabledProperty = VisualProperty.Add(typeof(TextBox), nameof(IsWheelZoomEnabled), VisualPropertyInvalidateModes.Render, false);
        public static VisualProperty FontSizeProperty = VisualProperty.Add<float?>(typeof(TextBox), nameof(FontSize), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty TextRenderingParametersProperty = VisualProperty.Add<TextRenderingParameters>(typeof(TextBox), nameof(TextRenderingParameters), VisualPropertyInvalidateModes.Render);

        event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { UIExtensions.AddEvent(ref _valueChanged, value); } remove { UIExtensions.RemoveEvent(ref _valueChanged, value); } }
        public event EventHandler<ValueEventArgs<string>> TextChanged;

        private EventHandler<ValueEventArgs> _valueChanged;
        private TextContainer _container;

        public SimpleText()
        {
            RaiseTextChanged = true;
            _container = new TextContainer(this, null);
        }

        bool IValueable.CanChangeValue { get => false; set { } }
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

        [Browsable(false)]
        public virtual bool RaiseTextChanged { get; set; }

        [Browsable(false)]
        public virtual EventTrigger TextChangedTrigger { get; set; }

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
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Text { get => _container.Text; set => SetText(value); }

        [Category(CategoryLayout)]
        public IComObject<IDWriteFontCollection> FontCollection { get => (IComObject<IDWriteFontCollection>)GetPropertyValue(FontCollectionProperty); set => SetPropertyValue(FontCollectionProperty, value); }

        [Category(CategoryLayout)]
        public D2D1_DRAW_TEXT_OPTIONS DrawOptions { get => (D2D1_DRAW_TEXT_OPTIONS)GetPropertyValue(DrawOptionsProperty); set => SetPropertyValue(DrawOptionsProperty, value); }

        [Category(CategoryLayout)]
        public D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get => (D2D1_TEXT_ANTIALIAS_MODE)GetPropertyValue(AntiAliasingModeProperty); set => SetPropertyValue(AntiAliasingModeProperty, value); }

        [Category(CategoryBehavior)]
        public bool IsWheelZoomEnabled { get => (bool)GetPropertyValue(IsWheelZoomEnabledProperty); set => SetPropertyValue(IsWheelZoomEnabledProperty, value); }

        [Category(CategoryLayout)]
        public float? FontSize { get => (float?)GetPropertyValue(FontSizeProperty); set => SetPropertyValue(FontSizeProperty, value); }

        [Category(CategoryLayout)]
        public TextRenderingParameters TextRenderingParameters { get => (TextRenderingParameters)GetPropertyValue(TextRenderingParametersProperty); set => SetPropertyValue(TextRenderingParametersProperty, value); }

        public override string ToString()
        {
            var text = base.ToString() + " '" + Text?.Replace('\r', '⏎').Replace("\n", string.Empty) + "'";
            return text.TrimWithEllipsis();
        }

        protected override void SetCompositionBrush(CompositionBrush brush)
        {
            // we use or own system
            //base.SetCompositionBrush(brush);
        }

        private void SetText(string text)
        {
            if (_container.Text == text)
                return;

            Interlocked.Exchange(ref _container, new TextContainer(this, text));
        }

        private sealed class TextContainer
        {
            public const char NotUnicode = '\uFFFF';

            public TextContainer(SimpleText visual, string text)
            {
                Visual = visual;
                Text = text ?? string.Empty;
            }

            public SimpleText Visual { get; }
            public string Text { get; }
            public List<Line> Lines { get; } = new List<Line>();

            public D2D_SIZE_F Measure(D2D_SIZE_F constraint)
            {
                Lines.Clear();
                var wrapping = Visual.WordWrapping;
                if (wrapping != DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP && constraint.width.IsNotSet())
                {
                    wrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP;
                }

                var x = 0f;
                var y = 0f;
                var position = 0;
                var length = 0;
                switch (wrapping)
                {
                    case DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP:
                        break;

                    case DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP:
                        for (var i = 0; i < Text.Length; i++)
                        {
                            var c = Text[i];
                            var n = i + 1 < Text.Length ? Text[i + 1] : NotUnicode;

                            if (c == '\n' || (c == '\r' && n == '\n'))
                            {
                                var line = new Line(position, length);
                                Lines.Add(line);
                            }
                            else
                            {
                                length++;
                            }
                        }

                        if (length > 0)
                        {
                            var line = new Line(position, length);
                            Lines.Add(line);
                        }
                        break;

                    case DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_EMERGENCY_BREAK:
                        break;

                    case DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD:
                        break;

                    case DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER:
                        break;
                }

                return new D2D_SIZE_F(x, y);
            }

            public override string ToString() => Text;
        }

        private sealed class Line
        {
            public Line(int position, int length)
            {
                Position = position;
                Length = length;
            }

            public int Position { get; }
            public int Length { get; }

            public override string ToString() => Position + " (" + Length + ")";
        }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            return _container.Measure(constraint);
        }
    }
}
