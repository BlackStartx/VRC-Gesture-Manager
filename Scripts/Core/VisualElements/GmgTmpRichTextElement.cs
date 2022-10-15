using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;

namespace GestureManager.Scripts.Core.VisualElements
{
    public class GmgTmpRichTextElement : VisualElement
    {
        private static readonly Regex RegexTokenPattern = new Regex("<[^<>]+>", RegexOptions.Compiled);
        private static readonly Regex RegexStringPattern = new Regex("(.*?<[^<>]+>|.+)", RegexOptions.Compiled);

        private const string V_OFFSET = "voffset";

        private const string SUX_ATTRIBUTE = "8";

        private string _text;

        private Data _data;

        private readonly VisualElement _textHolder;

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
            public StyleColor MarkStyle => PeekOrDefault(Mark) ?? Color.clear;

            internal readonly Stack<StyleLength?> Size = new Stack<StyleLength?>();
            internal readonly Stack<Color?> TextColor = new Stack<Color?>();
            internal readonly Stack<Color?> Mark = new Stack<Color?>();
            internal int Italic;
            internal int Bold;

            private static bool HandleList<T>(Stack<T> list, bool close, string attribute, Func<Stack<T>, string, bool> tryAdd)
            {
                if (!close) return tryAdd(list, attribute);
                if (list.Count > 0) list.Pop();
                return true;
            }

            private static T PeekOrDefault<T>(Stack<T> stack, T defaultValue = default) => stack.Count > 0 ? stack.Peek() : defaultValue;

            public static bool HandleColorList(Stack<Color?> list, bool close, string attribute) => HandleList(list, close, attribute, TryAddColor);

            public static bool HandleStyleLengthList(Stack<StyleLength?> list, bool close, string attribute) => HandleList(list, close, attribute, TryAddStyleLength);

            private static bool TryAddStyleLength(Stack<StyleLength?> list, string attribute)
            {
                if (attribute == null || !int.TryParse(attribute, out var intSize)) return false;
                var item = new StyleLength(intSize);
                list.Push(item);
                return true;
            }

            private static bool TryAddColor(Stack<Color?> list, string attribute)
            {
                if (attribute == null || !ColorOf(attribute, out var color)) return false;
                list.Push(color);
                return true;
            }

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
                style.color = value;
                SetUp(_text);
            }
        }

        public GmgTmpRichTextElement()
        {
            Add(new TextElement { pickingMode = PickingMode.Ignore, text = "" });
            Add(_textHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            style.alignItems = Align.Center;
            _textHolder.style.alignItems = Align.Center;
        }

        private void SetUp(string input)
        {
            _data = new Data();
            foreach (var _ in Enumerable.Range(0, _textHolder.childCount)) _textHolder.RemoveAt(0);
            foreach (var splitString in RegexStringPattern.Matches(input).OfType<Match>().Select(match => match.Value)) AddToken(splitString);
        }

        private void AddToken(string tokenInput)
        {
            var child = new TextElement
            {
                style = { unityFontStyleAndWeight = _data.FontStyle, color = _data.ColorStyle(style.color), backgroundColor = _data.MarkStyle, fontSize = _data.SizeStyle(style.fontSize) },
                pickingMode = PickingMode.Ignore, text = RegexTokenPattern.Split(tokenInput)[0] + EvaluateToken(RegexTokenPattern.Match(tokenInput).Value)
            };
            if (string.IsNullOrEmpty(child.text)) return;
            child.style.width = 100;
            _textHolder.Add(child);
        }

        private static (string, string) GetToken(string tokenString) => GetToken(tokenString.Split(new[] { ' ', '=' }, 2));

        private static (string, string) GetToken(IReadOnlyList<string> splitData) => (splitData[0], splitData.Count > 1 ? splitData[1] : null);

        private string EvaluateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var isClose = token[1] == '/';
            var tokenString = token.Substring(isClose ? 2 : 1, token.Length - (isClose ? 3 : 2));
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
                case "mark":
                    return Data.HandleColorList(_data.Mark, isClose, attributeString) ? null : token;
                case "color":
                    return Data.HandleColorList(_data.TextColor, isClose, attributeString) ? null : token;
                case "size":
                    return Data.HandleStyleLengthList(_data.Size, isClose, attributeString) ? null : token;
                case "sup":
                    return Data.HandleStyleLengthList(_data.Size, isClose, SUX_ATTRIBUTE) ? null : token;
                case "sub":
                    return Data.HandleStyleLengthList(_data.Size, isClose, SUX_ATTRIBUTE) ? null : token;
                case V_OFFSET:
                    return null;
                case "sprite": // No... I won't implement this perfectly... This is enough~
                    return "☻";
                default: return token;
            }
        }
    }
}