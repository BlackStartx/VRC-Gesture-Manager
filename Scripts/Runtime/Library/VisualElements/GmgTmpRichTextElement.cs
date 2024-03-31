using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;
using This = BlackStartX.GestureManager.Library.VisualElements.GmgTmpRichTextElement;

namespace BlackStartX.GestureManager.Library.VisualElements
{
    public class GmgTmpRichTextElement : VisualElement
    {
        private static readonly Regex RegexStringPattern = new("(.*?<[^<>]+>|.+)", RegexOptions.Compiled);
        private static readonly Regex RegexTokenPattern = new("<[^<>]+>", RegexOptions.Compiled);
        private static readonly Regex RegexDigitPattern = new(@"\D+", RegexOptions.Compiled);
        private static readonly char[] NumSeparator = { 'e', 'p', ',', '%' };
        private static readonly char[] Separator = { ' ', '=' };

        private const string VOffset = "voffset";
        private const string SuxAttribute = "8";
        private const float DefaultFontSize = 24f;
        private const float DefaultScale = 0.5f;

        private string _text;
        private Data _data;

        private readonly VisualElement _textHolder;

        private float Font => style.fontSize.value.value;
        private This This => this; // this

        public string Text
        {
            set
            {
                if (_text == value) return;
                _text = value;
                SetUp(value);
            }
        }

        public Color Color
        {
            set
            {
                if (style.color == value) return;
                style.color = new StyleColor(value);
                foreach (var element in _data.ParentalColors) element.style.color = new StyleColor(value);
            }
        }

        public GmgTmpRichTextElement()
        {
            Add(new TextElement { pickingMode = PickingMode.Ignore, text = "" });
            Add(_textHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            style.alignItems = Align.Center;
            style.fontSize = DefaultFontSize * DefaultScale;
            _textHolder.style.alignItems = Align.Center;
        }

        private void SetUp(string input)
        {
            _data = new Data();
            foreach (var _ in Enumerable.Range(0, _textHolder.childCount)) _textHolder.RemoveAt(0);
            foreach (var ttString in RegexStringPattern.Matches(input).Select(match => match.Value)) TextToken(ttString);
        }

        private void TextToken(string textToken)
        {
            AddText(RegexTokenPattern.Split(textToken)[0]);
            AddText(EvaluateToken(RegexTokenPattern.Match(textToken).Value));
        }

        private void AddText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var child = new TextElement { pickingMode = PickingMode.Ignore, text = text };

            child.style.color = _data.ColorStyle(child, style.color);
            child.style.fontSize = _data.LengthStyle(style.fontSize);
            child.style.backgroundColor = _data.MarkStyle;
            child.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(_data.FontStyle);

            _textHolder.Add(child);
        }

        private static string GetNumString(string number, int count = 2) => number.Split(NumSeparator, count)[0];

        private static (string, string) GetToken(string tokenString, int count = 2) => GetToken(tokenString.Split(Separator, count));

        private static (string, string) GetToken(IReadOnlyList<string> splitData) => (splitData[0], splitData.Count > 1 ? splitData[1] : null);

        private string EvaluateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var isClose = token[1] == '/';
            var intIndex = isClose ? 2 : 1;
            var intLength = token.Length - intIndex - 1;
            var tokenString = token.Substring(intIndex, intLength);
            var (tagString, attributeString) = GetToken(tokenString);

            switch (tagString)
            {
                case "i":
                    if (!isClose) _data.Italic++;
                    else if (_data.Italic > 0) _data.Italic--;
                    return null;
                case "b":
                    if (!isClose) _data.Bold++;
                    else if (_data.Bold > 0) _data.Bold--;
                    return null;
                case "line-height":
                    return null;
                case "mark":
                    return Data.HandleColorList(_data.Mark, isClose, attributeString, This) ? null : token;
                case "sup":
                    return Data.HandleStyleLengthList(_data.Size, isClose, SuxAttribute, This) ? null : token;
                case "sub":
                    return Data.HandleStyleLengthList(_data.Size, isClose, SuxAttribute, This) ? null : token;
                case "color":
                    return Data.HandleColorList(_data.TextColor, isClose, attributeString, This) ? null : token;
                case "size":
                    return Data.HandleStyleLengthList(_data.Size, isClose, attributeString, This) ? null : token;
                case VOffset:
                    return Data.HandleStyleLengthList(_data.Unsupported, isClose, attributeString, This) ? null : token;
                case "sprite": // No... I won't implement this perfectly... This is enough~
                    return "☻";
                default: return token;
            }
        }

        private class Data
        {
            internal readonly List<VisualElement> ParentalColors = new();

            private static Dictionary<string, Color> TextMeshProColorNames => new()
            {
                { "red", Color.red },
                { "blue", Color.blue },
                { "white", Color.white },
                { "black", Color.black },
                { "green", Color.green },
                { "orange", new Color(1f, 0.5f, 0f) },
                { "yellow", new Color(1f, 0.92f, 0f) },
                { "purple", new Color(0.63f, 0.13f, 0.94f) }
            };

            public StyleColor ColorStyle(VisualElement node, StyleColor fallback) => PeekOrDefault(TextColor) ?? Parental(ParentalColors, node, fallback);
            public StyleLength LengthStyle(StyleLength fallback) => PeekOrDefault(Size) ?? fallback;

            public FontStyle FontStyle => Italic == 0 && Bold == 0 ? FontStyle.Normal : Italic == 0 ? FontStyle.Bold : Bold == 0 ? FontStyle.Italic : FontStyle.BoldAndItalic;
            public StyleColor MarkStyle => PeekOrDefault(Mark) ?? new StyleColor(Color.clear);

            internal readonly Stack<StyleLength?> Unsupported = new();
            internal readonly Stack<StyleLength?> Size = new();
            internal readonly Stack<StyleColor?> TextColor = new();
            internal readonly Stack<StyleColor?> Mark = new();
            internal int Italic;
            internal int Bold;

            private static T Parental<T>(ICollection<VisualElement> list, VisualElement child, T fallback)
            {
                list.Add(child);
                return fallback;
            }

            private static T PeekOrDefault<T>(Stack<T> stack, T defaultValue = default) => stack.Count > 0 ? stack.Peek() : defaultValue;

            public static bool HandleColorList(Stack<StyleColor?> list, bool close, string attribute, This element) => HandleList(list, close, attribute, element, TryAddColor);

            public static bool HandleStyleLengthList(Stack<StyleLength?> list, bool close, string attribute, This element) => HandleList(list, close, attribute, element, TryAddStyleLength);

            private static bool HandleList<T>(Stack<T> list, bool close, string attribute, This element, Func<Stack<T>, string, This, bool> tryAdd)
            {
                if (!close) return tryAdd(list, attribute, element);
                if (list.Count > 0) list.Pop();
                return true;
            }

            private static bool TryAddStyleLength(Stack<StyleLength?> list, string attribute, This element)
            {
                if (string.IsNullOrEmpty(attribute) || !TmpNumberAttribute(attribute, element, out var length)) return false;
                list.Push(length);
                return true;
            }

            private static bool TryAddColor(Stack<StyleColor?> list, string attribute, This element)
            {
                if (string.IsNullOrEmpty(attribute) || !ColorOf(attribute, out var color)) return false;
                list.Push(color);
                return true;
            }

            /*
             * The value of the TMP attribute is strangely calculated.
             *
             * It seems to be parsed until certain characters are met, choosing also the type of the value.
             * Those characters are:
             * - e (The parse will stop and the value will be considered em)
             * - p (The parse will stop and the value will be considered px)
             * - , (The parse will stop and the value will be considered numerical)
             * - % (The parse will stop and the value will be considered a percentage)
             *
             * Also all the other characters, if not numerical, seems to be ignored.
             *
             * Consecutive decimal values seems to... sum up .-. (I.e: 10.9.9 is equal to 10 + 0.9 + 0.9)
             */
            private static bool TmpNumberAttribute(string attribute, This element, out StyleLength size, char separator = '.', float value = 0f)
            {
                size = new StyleLength(value);
                var sString = GetNumString(attribute);
                var tChar = sString.Length < attribute.Length ? attribute[sString.Length] : ',';
                value = TmpNumberFetch(sString.Split(separator).Select(TmpNumberClean).Where(vString => !string.IsNullOrEmpty(vString)));
                switch (tChar)
                {
                    case ',':
                        value *= DefaultScale;
                        size = new StyleLength(value);
                        return true;
                    case 'e':
                        value *= element.Font;
                        size = new StyleLength(value);
                        return true;
                    case 'p':
                        var vRin = new Length(value, LengthUnit.Pixel);
                        size = new StyleLength(vRin);
                        return true;
                    case '%':
                        var vLen = new Length(value, LengthUnit.Percent);
                        size = new StyleLength(vLen);
                        return true;
                    default: return false;
                }
            }

            private static float TmpNumberFetch(IEnumerable<string> s)
            {
                // ReSharper disable PossibleMultipleEnumeration
                return s.Any() ? TmpNumberParse(s.First()) + s.Skip(1).Select(sString => $"0.{sString}").Sum(TmpNumberParse) : 0;
                // ReSharper restore PossibleMultipleEnumeration
            }

            private static string TmpNumberClean(string input) => NumberClean(input);

            private static float TmpNumberParse(string number) => float.Parse(number, CultureInfo.InvariantCulture);

            private static string NumberClean(string input, string replacement = "") => RegexDigitPattern.Replace(input, replacement);

            private static bool ColorOf(string attribute, out Color color) => TextMeshProColorNames.TryGetValue(attribute, out color) || ColorUtility.TryParseHtmlString(attribute, out color);
        }
    }
}