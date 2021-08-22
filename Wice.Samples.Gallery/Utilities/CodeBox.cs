using System;
using System.ComponentModel;
using System.IO;
using ColorCode;
using ColorCode.Common;
using DirectN;

namespace Wice.Samples.Gallery.Utilities
{
    public class CodeBox : RichTextBox
    {
        public static VisualProperty CodeLanguageProperty = VisualProperty.Add<string>(typeof(CodeBox), nameof(CodeLanguage), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);
        public static VisualProperty CodeTextProperty = VisualProperty.Add<string>(typeof(CodeBox), nameof(CodeText), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);

        private Lazy<ILanguage> _language;

        public CodeBox()
        {
            _language = new Lazy<ILanguage>(GetLanguage, true);
        }

        [Category(CategoryLayout)]
        public string CodeLanguage { get => (string)GetPropertyValue(CodeLanguageProperty) ?? string.Empty; set => SetPropertyValue(CodeLanguageProperty, value); }

        [Category(CategoryLayout)]
        public string CodeText { get => (string)GetPropertyValue(CodeTextProperty) ?? string.Empty; set => SetPropertyValue(CodeTextProperty, value); }

        public override string HtmlText { get => base.HtmlText; set => throw new NotSupportedException(); }
        public override string RtfText { get => base.RtfText; set => throw new NotSupportedException(); }
        public override string Text { get => base.Text; set => throw new NotSupportedException(); }
        public ILanguage Language => _language.Value;

        private ILanguage GetLanguage()
        {
            var language = CodeLanguage.Nullify() ?? LanguageId.CSharp;
            return Languages.FindById(language) ?? Languages.CSharp;
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == CodeLanguageProperty)
            {
                _language = new Lazy<ILanguage>(GetLanguage, true);
            }
            else if (property == CodeTextProperty)
            {
                var sw = new StringWriter();
                var formatter = new RtfFormatter(sw);
                formatter.Format((string)value, Language);
                var text = sw.ToString();
                base.RtfText = text;
            }
            return true;
        }
    }
}