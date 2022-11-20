using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;
using This = BlackStartX.GestureManager.Runtime.VisualElements.GmgTmpRichTextElement;

namespace BlackStartX.GestureManager.Runtime.VisualElements
{
    public class GmgTmpRichTextElement : VisualElement
    {
        private static readonly Regex RegexStringPattern = new Regex("(.*?<[^<>]+>|.+)", RegexOptions.Compiled);
        private static readonly Regex RegexTokenPattern = new Regex("<[^<>]+>", RegexOptions.Compiled);
        private static readonly Regex RegexDigitPattern = new Regex("[^\\d]+", RegexOptions.Compiled);
        private static readonly char[] NumSeparator = { 'e', 'p', ',', '%' };
        private static readonly char[] Separator = { ' ', '=' };

        private const string VOffset = "voffset";
        private const string SuxAttribute = "8";
        private const float DefaultFontSize = 24f;
        private const float DefaultScale = 0.5f;

        private string _text;

        private Data _data;

        private readonly VisualElement _textHolder;
        private StyleColor ColorStyle() => _data.ColorStyle(style.color);
        private StyleLength FontSize() => _data.SizeStyle(style.fontSize);

        private float Font => style.fontSize.value.value;
        private This This => this; // this

        private class Data
        {
            private static Dictionary<string, Color> TextMeshProColorNames => new Dictionary<string, Color>
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

            public FontStyle FontStyle => Italic == 0 && Bold == 0 ? FontStyle.Normal : Italic == 0 ? FontStyle.Bold : Bold == 0 ? FontStyle.Italic : FontStyle.BoldAndItalic;
            public StyleColor ColorStyle(StyleColor fallback) => PeekOrDefault(TextColor) ?? fallback;
            public StyleLength SizeStyle(StyleLength fallback) => PeekOrDefault(Size) ?? fallback;
            public StyleColor MarkStyle => new StyleColor(PeekOrDefault(Mark) ?? Color.clear);

            internal readonly Stack<StyleLength?> Unsupported = new Stack<StyleLength?>();
            internal readonly Stack<StyleLength?> Size = new Stack<StyleLength?>();
            internal readonly Stack<Color?> TextColor = new Stack<Color?>();
            internal readonly Stack<Color?> Mark = new Stack<Color?>();
            internal int Italic;
            internal int Bold;

            private static bool HandleList<T>(Stack<T> list, bool close, string attribute, This element, Func<Stack<T>, string, This, bool> tryAdd)
            {
                if (!close) return tryAdd(list, attribute, element);
                if (list.Count > 0) list.Pop();
                return true;
            }

            private static T PeekOrDefault<T>(Stack<T> stack, T defaultValue = default) => stack.Count > 0 ? stack.Peek() : defaultValue;

            public static bool HandleColorList(Stack<Color?> list, bool close, string attribute, This element) => HandleList(list, close, attribute, element, TryAddColor);

            public static bool HandleStyleLengthList(Stack<StyleLength?> list, bool close, string attribute, This element) => HandleList(list, close, attribute, element, TryAddStyleLength);

            private static bool TryAddStyleLength(Stack<StyleLength?> list, string attribute, This element)
            {
                if (string.IsNullOrEmpty(attribute) || !TmpNumberAttribute(attribute, element, out var length)) return false;
                list.Push(length);
                return true;
            }

            private static bool TryAddColor(Stack<Color?> list, string attribute, This element)
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
                SetUp(_text);
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
            foreach (var splitString in RegexStringPattern.Matches(input).OfType<Match>().Select(match => match.Value)) AddToken(splitString);
        }

        private void AddToken(string tokenInput, float v = 100f)
        {
            var child = new TextElement
            {
                style = { unityFontStyleAndWeight = _data.FontStyle, color = ColorStyle(), backgroundColor = _data.MarkStyle, fontSize = FontSize() },
                pickingMode = PickingMode.Ignore, text = RegexTokenPattern.Split(tokenInput)[0] + EvaluateToken(RegexTokenPattern.Match(tokenInput).Value)
            };
            if (string.IsNullOrEmpty(child.text)) return;
            child.style.width = new StyleLength(v);
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
    }
}