﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using DirectN;

namespace Wice.Samples.Gallery.Utilities
{
    public class RtfFormatter : CodeColorizerBase
    {
        private readonly static ConcurrentDictionary<string, ColorIndex> _scopeColorIndex = new ConcurrentDictionary<string, ColorIndex>(StringComparer.OrdinalIgnoreCase);

        public RtfFormatter(TextWriter writer, string fontName = null, IDictionary<string, _D3DCOLORVALUE> colors = null, _D3DCOLORVALUE? defaultColor = null)
            : base(null, null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            Writer = writer;
            fontName = fontName ?? "Consolas";
            Writer.WriteLine(@"{\rtf1\ansi\deff0{\fonttbl{\f0" + fontName + "}}");

            // write color table
            Writer.Write(@"{\colortbl;");

            defaultColor = defaultColor ?? _D3DCOLORVALUE.Black;
            DefaultColor = defaultColor.Value;
            var defColors = GetDefaultColors(DefaultColor);
            var names = Enum.GetNames(typeof(ColorIndex));
            for (var i = 0; i < names.Length; i++)
            {
                if (colors == null || !colors.TryGetValue(names[i], out var color))
                {
                    if (!defColors.TryGetValue(names[i], out color))
                    {
                        color = DefaultColor;
                    }
                }

                Writer.Write(@"\red");
                Writer.Write(color.BR);
                Writer.Write(@"\green");
                Writer.Write(color.BG);
                Writer.Write(@"\blue");
                Writer.Write(color.BB);
                writer.Write(';');
            }

            Writer.WriteLine('}');
        }

        public _D3DCOLORVALUE DefaultColor { get; }

        private static Dictionary<string, _D3DCOLORVALUE> GetDefaultColors(_D3DCOLORVALUE defaultColor)
        {
            var dic = new Dictionary<string, _D3DCOLORVALUE>(StringComparer.OrdinalIgnoreCase);
            dic[ColorIndex.Default.ToString()] = defaultColor;
            dic[ColorIndex.Keyword.ToString()] = _D3DCOLORVALUE.Blue;
            dic[ColorIndex.Comment.ToString()] = _D3DCOLORVALUE.FromArgb(0, 128, 0);
            dic[ColorIndex.String.ToString()] = _D3DCOLORVALUE.FromArgb(128, 0, 0);
            dic[ColorIndex.StringCSharpVerbatim.ToString()] = _D3DCOLORVALUE.FromArgb(128, 0, 0);
            dic[ColorIndex.Number.ToString()] = _D3DCOLORVALUE.FromArgb(128, 0, 0);
            return dic;
        }

        private static ColorIndex GetScopeColorIndex(string name)
        {
            if (name == null)
                return ColorIndex.Default;

            if (!_scopeColorIndex.TryGetValue(name, out var index))
            {
                var values = Enum.GetValues(typeof(ColorIndex));
                var names = Enum.GetNames(typeof(ColorIndex));
                index = ColorIndex.Default;
                for (var i = 0; i < names.Length; i++)
                {
                    if (names[i].EqualsIgnoreCase(name))
                    {
                        index = (ColorIndex)values.GetValue(i);
                        break;
                    }
                }
                _scopeColorIndex[name] = index;
            }
            return index;
        }

        // we currently only use the ones defined in ColorCode for C#
        private enum ColorIndex
        {
            Default,
            Keyword,
            Comment,
            String,
            StringCSharpVerbatim,
            Number,
            XmlDocTag,
            XmlDocComment,
            PreprocessorKeyword,
        }

        private TextWriter Writer { get; }

        public void Format(string sourceCode, ILanguage Language)
        {
            languageParser.Parse(sourceCode, Language, (parsedSourceCode, captures) => Write(parsedSourceCode, captures));
            Writer.Write("}");
        }

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            if (scopes.Count > 0)
            {
                var styleInsertions = new List<TextInsertion>();

                foreach (var scope in scopes)
                {
                    GetStyleInsertionsForCapturedStyle(scope, styleInsertions);
                }

                styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));
                var offset = 0;

                Scope PreviousScope = null;
                foreach (var styleinsertion in styleInsertions)
                {
                    var text = parsedSourceCode.Substring(offset, styleinsertion.Index - offset);
                    CreateSpan(text, PreviousScope);
                    if (!string.IsNullOrWhiteSpace(styleinsertion.Text))
                    {
                        CreateSpan(text, PreviousScope);
                    }

                    offset = styleinsertion.Index;
                    PreviousScope = styleinsertion.Scope;
                }

                var remaining = parsedSourceCode.Substring(offset);
                if (remaining != "\r")
                {
                    CreateSpan(remaining, null);
                }

                return;
            }

            if (DefaultColor != _D3DCOLORVALUE.Black)
            {
                Write(parsedSourceCode, ColorIndex.Default);
            }
            else
            {
                Writer.Write(Escape(parsedSourceCode));
            }
        }

        private void Write(string text, ColorIndex index)
        {
            Writer.Write(@"{\cf");
            Writer.Write(1 + (int)index);
            Writer.Write(' ');
            Writer.Write(Escape(text));
            Writer.Write('}');
        }

        private void CreateSpan(string text, Scope scope)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var index = GetScopeColorIndex(scope?.Name);
            if (index == ColorIndex.Default)
            {
                Writer.Write(Escape(text));
                return;
            }

            Write(text, index);
        }

        private static void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
        {
            styleInsertions.Add(new TextInsertion { Index = scope.Index, Scope = scope });
            foreach (var childScope in scope.Children)
            {
                GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);
            }

            styleInsertions.Add(new TextInsertion { Index = scope.Index + scope.Length });
        }

        private static bool IsRtfSpec(char c) => c == '{' || c == '}' || c == '\\';

        private static string Escape(string text)
        {
            if (text == null)
                return null;

            var sb = new StringBuilder(text.Length);
            var escaped = false;
            foreach (var c in text)
            {
                if (IsRtfSpec(c))
                {
                    sb.Append('\\');
                    escaped = true;
                }
                else if (c == '\n')
                {
                    sb.AppendLine(@"\line");
                    escaped = true;
                    continue;
                }
                else if (c == '\r')
                {
                    escaped = true;
                    continue;
                }
                sb.Append(c);
            }
            return escaped ? sb.ToString() : text;
        }
    }
}
