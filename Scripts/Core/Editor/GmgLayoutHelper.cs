using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Editor;
using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Core.Editor
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     *
     * By the way, don't pay too much attention to the UnityFieldEnterListener, I kinda rushed it~ ^-^
     * The concept behind it it's a Field that does not change its value until the user press Enter.
     * The overcomplexity is due to Unity not being friendly with value resets... So I kinda had to workaround it.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgLayoutHelper
    {
        private static object _unityFieldEnterListenerData;
        private static string _unityFieldEnterListenerName;

        public struct Toolbar
        {
            private GUIContent[] _contents;

            public GUIContent[] Contents(IEnumerable<(string, Action)> field) => _contents ?? (_contents = field.Select(tuple => new GUIContent(tuple.Item1)).ToArray());
            public int Selected;
        }

        public static void MyToolbar(ref Toolbar toolbar, (string, Action)[] field) => MyToolbar(ref toolbar.Selected, toolbar.Contents(field), field);

        private static void MyToolbar(ref int toolbar, GUIContent[] contents, (string, Action)[] actions)
        {
            toolbar = GUILayout.Toolbar(toolbar, contents);
            actions[toolbar].Item2();
        }

        public static IEnumerable<EditorWindow> GetInspectorWindows() => Resources.FindObjectsOfTypeAll<EditorWindow>().Where(window => window.titleContent.text == "Inspector");

        public static void GuiLabel((Color? color, string text) tuple) => GuiLabel(tuple.color, tuple.text);

        public static void GuiLabel((Color? color, string text) tuple, params GUILayoutOption[] options) => GuiLabel(tuple.color, tuple.text, null, options);

        private static void GuiLabel(Color? color, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.label;
            if (color != null)
            {
                var textColor = GUI.contentColor;
                GUI.contentColor = color.Value;
                GUILayout.Label(text, style, options);
                GUI.contentColor = textColor;
            }
            else GUILayout.Label(text, style, options);
        }

        public static bool Button(string text, Color color, params GUILayoutOption[] options)
        {
            using (new GuiBackground(color)) return GUILayout.Button(text, options);
        }

        public static bool DebugButton(string label, GUILayoutOption width = null) => GUILayout.Button(label, GUILayout.Height(30), width ?? GUILayout.Width(150));

        public static bool TitleButton(string text, string button, int width = 85)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(15 + width);
                GUILayout.Label(text, GestureManagerStyles.Header);
                return GUILayout.Button(button, GestureManagerStyles.HeaderButton, GUILayout.Width(width));
            }
        }

        public static bool UnityFieldEnterListener<T1, T2>(T1 initialText, T2 argument, Rect rect, Func<Rect, T1, T1> field, Action<T2, T1> onEscape, string controlName)
        {
            if (_unityFieldEnterListenerName != controlName) _unityFieldEnterListenerData = null;
            var isEnter = Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter;
            var isEscape = Event.current.keyCode == KeyCode.Escape;
            _unityFieldEnterListenerName = controlName;
            GUI.SetNextControlName(controlName);
            GUI.FocusControl(controlName);
            if (_unityFieldEnterListenerData == null) _unityFieldEnterListenerData = initialText;
            var t = field(rect, _unityFieldEnterListenerData is T1 d ? d : default);
            if (isEnter) onEscape(argument, _unityFieldEnterListenerData is T1 data ? data : default);
            else _unityFieldEnterListenerData = t;
            if (!isEnter && !isEscape) return false;
            _unityFieldEnterListenerData = null;
            GUI.SetNextControlName(controlName);
            GUI.FocusControl(controlName);
            EditorGUI.LabelField(Rect.zero, "");
            return true;
        }

        /*
         *  Rect Utils!!!
         *  Rect Utils!!!
         *  Rect Utils!!!
         */

        private static Rect SubtractWidthRight(Rect rect, int width, out Rect rectR)
        {
            rect.width -= width;
            rectR = rect;
            rectR.width = width;
            rectR.x += rect.width;
            return rect;
        }

        /*
         *  Object Field!!!
         *  Object Field!!!
         *  Object Field!!!
         */

        private static T ObjectField<T>(string label, T unityObject) where T : UnityEngine.Object
        {
            if (label != null) return (T)EditorGUILayout.ObjectField(label, unityObject, typeof(T), true, null);
            return (T)EditorGUILayout.ObjectField(unityObject, typeof(T), true, null);
        }

        public static T ObjectField<T>(string label, T unityObject, Action<T> onObjectSet) where T : UnityEngine.Object
        {
            if (unityObject != (unityObject = ObjectField(label, unityObject))) onObjectSet(unityObject);
            return unityObject;
        }

        public static bool ButtonObjectField<T>(string label, T unityObject, char text, Action<T> onNewObjectDrop) where T : UnityEngine.Object
        {
            var rectL = SubtractWidthRight(GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.label), 20, out var rectR);
            if (unityObject != (unityObject = EditorGUI.ObjectField(rectL, label, unityObject, typeof(T), true) as T)) onNewObjectDrop?.Invoke(unityObject);
            return GUI.Button(rectR, text.ToString());
        }

        public static Color ResetColorField(string label, Color current, Color reset)
        {
            var rectL = SubtractWidthRight(GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.label), 20, out var rectR);
            return GUI.Button(rectR, "X") ? reset : EditorGUI.ColorField(rectL, label, current);
        }

        public static void Divisor(int height)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        public static Rect GetLastRect(ref Rect lastRect)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Used) return lastRect;
            return lastRect = GUILayoutUtility.GetLastRect();
        }

        public static ulong ULongField(string label, ulong value)
        {
            var backULong = value;
            return ulong.TryParse(EditorGUILayout.TextField(label, value.ToString()), out value) ? value : backULong;
        }

        public static bool ToggleRight(string label, bool active)
        {
            GUILayout.Label(label);
            SubtractWidthRight(GUILayoutUtility.GetLastRect(), 15, out var rectR);
            return GUI.Toggle(rectR, active, new GUIContent());
        }

        public static string PlaceHolderTextField(string label, string text, string placeHolder)
        {
            text = EditorGUILayout.TextField(label, text);
            if(string.IsNullOrEmpty(text)) EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), " ", placeHolder);
            return text;
        }

        /*
         * Classes...
         * Classes...
         * Classes...
         */

        internal class GuiBackground : IDisposable
        {
            private readonly Color _color;

            public GuiBackground(Color color)
            {
                _color = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose() => GUI.backgroundColor = _color;
        }

        public static void HorizontalGrid<T1, T2>(T2 t2, float width, float sizeWidth, float sizeHeight, float divisor, IList<T1> data, Action<T2, Rect, T1> gridRect) where T1 : class
        {
            var intCount = (int)(width / sizeWidth);
            if (intCount <= 0) return;
            GUILayout.BeginHorizontal();
            for (var i = 0; i < data.Count + (intCount - data.Count % intCount) % intCount; i++)
            {
                var t = i < data.Count ? data[i] : null;
                GUILayout.FlexibleSpace();
                var rect = GUILayoutUtility.GetRect(sizeWidth, sizeHeight);
                if (t != null) gridRect(t2, rect, t);

                if ((i + 1) % intCount != 0) continue;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(divisor);
                GUILayout.BeginHorizontal();
            }

            GUILayout.EndHorizontal();
        }
    }
}