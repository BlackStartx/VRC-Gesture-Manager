using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgLayoutHelper
    {
        private static FieldInfo _scrollField;
        private static FieldInfo ScrollField(EditorWindow window) => _scrollField ?? (_scrollField = window.GetType().GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.NonPublic));

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

        private static IEnumerable<EditorWindow> GetInspectorWindows()
        {
            return Resources.FindObjectsOfTypeAll<EditorWindow>().Where(window => window.titleContent.text == "Inspector");
        }

        public static EditorWindow GetCustomEditorInspectorWindow(UnityEditor.Editor editor)
        {
            foreach (var inspectorWindow in GetInspectorWindows())
            {
                var trackerObject = inspectorWindow.GetType().GetProperty("tracker", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(inspectorWindow);
                if (trackerObject?.GetType().GetProperty("activeEditors")?.GetValue(trackerObject) is UnityEditor.Editor[] editors && editors.Contains(editor)) return inspectorWindow;
            }

            return null;
        }

        public static Vector2 GetScroll(EditorWindow inspector)
        {
            var scrollObject = ScrollField(inspector)?.GetValue(inspector);
            return scrollObject is Vector2 scroll ? scroll : new Vector2(0, 0);
        }

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

        /*
         *  Object Field!!!
         *  Object Field!!!
         *  Object Field!!!
         */

        public static T ObjectField<T>(string label, T unityObject, Action<T> onObjectSet) where T : UnityEngine.Object
        {
            return ObjectField(label, unityObject, onObjectSet, (oldObject, newObject) => onObjectSet(newObject), oldObject => onObjectSet(null));
        }

        private static T ObjectField<T>(string label, T unityObject, Action<T> onObjectSet, Action<T, T> onObjectChange, Action<T> onObjectRemove) where T : UnityEngine.Object
        {
            var oldObject = unityObject;

            unityObject = (T)(label != null ? EditorGUILayout.ObjectField(label, unityObject, typeof(T), true, null) : EditorGUILayout.ObjectField(unityObject, typeof(T), true, null));
            if (oldObject == unityObject) return unityObject;
            if (!oldObject) onObjectSet(unityObject);
            else if (!unityObject) onObjectRemove(oldObject);
            else onObjectChange(oldObject, unityObject);

            return unityObject;
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