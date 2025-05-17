﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using DirectN;

namespace Wice.Samples.Gallery.Utilities
{
    public class RtfFormatter : CodeColorizerBase
    {
        private readonly static ConcurrentDictionary<string, ColorIndex> _scopeNames = new ConcurrentDictionary<string, ColorIndex>(StringComparer.OrdinalIgnoreCase);

        public RtfFormatter(TextWriter writer, string fontName = null, IDictionary<string, D3DCOLORVALUE> colors = null, D3DCOLORVALUE? defaultColor = null)
            : base(null, null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            Writer = writer;
            fontName = fontName ?? "Consolas";
            Writer.WriteLine(@"{\rtf1\ansi\deff0{\fonttbl{\f0" + fontName + "}}");

            // write color table
            Writer.Write(@"{\colortbl;");

            defaultColor = defaultColor ?? D3DCOLORVALUE.Black;
            DefaultColor = defaultColor.Value;
            var defColors = GetDefaultColors(DefaultColor);
            var values = Enum.GetValues(typeof(ColorIndex));
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

                if (i > 0)
                {
                    var field = typeof(ScopeName).GetField(names[i], BindingFlags.Static | BindingFlags.Public);
                    _scopeNames[(string)field.GetValue(null)] = (ColorIndex)values.GetValue(i);
                }
            }

            Writer.WriteLine('}');
        }

        public D3DCOLORVALUE DefaultColor { get; }

        private static Dictionary<string, D3DCOLORVALUE> GetDefaultColors(D3DCOLORVALUE defaultColor)
        {
            var dic = new Dictionary<string, D3DCOLORVALUE>(StringComparer.OrdinalIgnoreCase);
            dic[ColorIndex.Default.ToString()] = defaultColor;
            dic[ColorIndex.Keyword.ToString()] = D3DCOLORVALUE.Blue;
            dic[ColorIndex.Comment.ToString()] = D3DCOLORVALUE.FromArgb(0, 128, 0);
            dic[ColorIndex.String.ToString()] = D3DCOLORVALUE.FromArgb(128, 0, 0);
            dic[ColorIndex.StringCSharpVerbatim.ToString()] = D3DCOLORVALUE.FromArgb(128, 0, 0);
            dic[ColorIndex.Number.ToString()] = D3DCOLORVALUE.FromArgb(128, 0, 0);
            dic[ColorIndex.ClassName.ToString()] = D3DCOLORVALUE.FromArgb(43, 145, 175);
            dic[ColorIndex.BuiltinFunction.ToString()] = D3DCOLORVALUE.Olive;
            return dic;
        }

        private static ColorIndex GetScopeColorIndex(string scopeName)
        {
            if (scopeName == null)
                return ColorIndex.Default;

            if (!_scopeNames.TryGetValue(scopeName, out var index))
                return ColorIndex.Default;

            return index;
        }

        // we currently only use the ones defined in ColorCode for C#, they *must* match
        private enum ColorIndex
        {
            Default,
            Keyword,
            Comment,
            String,
            StringCSharpVerbatim,
            Number,
            ClassName,
            BuiltinFunction,
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

            if (DefaultColor != D3DCOLORVALUE.Black)
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
