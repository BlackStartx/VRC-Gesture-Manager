using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void MyToolbar(ref GmgToolbarHeader id, GmgToolbarRow[] rows)
        {
            if (id == null) id = new GmgToolbarHeader(rows);

            var selected = GUILayout.Toolbar(id.GetSelected(), id.GetTitles());
            id.SetSelected(selected);
            id.ShowSelected();
        }

        public static IEnumerable<EditorWindow> GetInspectorWindows()
        {
            return Resources.FindObjectsOfTypeAll<EditorWindow>().Where(window => window.titleContent.text == "Inspector");
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

        /*
         * Classes...
         * Classes...
         * Classes...
         */

        public class GmgToolbarHeader
        {
            private int _selected;
            private readonly string[] _titles;
            private readonly GmgToolbarRow[] _rows;

            public GmgToolbarHeader(GmgToolbarRow[] rows)
            {
                _selected = 0;
                if (rows.Length != 0) rows[0].OnStartFocus(false);
                _titles = rows.Select(row => row.GetTitle()).ToArray();
                _rows = rows;
            }

            public int GetSelected()
            {
                return _selected;
            }

            public void SetSelected(int newSelected)
            {
                if (_selected != newSelected) _rows[newSelected].OnStartFocus(true);
                _selected = newSelected;
            }

            public string[] GetTitles()
            {
                return _titles;
            }

            public void ShowSelected()
            {
                _rows[_selected].Show();
            }
        }

        public class GmgToolbarRow
        {
            private readonly string _title;
            private readonly bool _focusOnStart;
            private readonly Action _willShow;
            private readonly Action _onStartFocus;

            private GmgToolbarRow(string title, Action willShow, Action onStartFocus, bool focusOnStart = true)
            {
                _title = title;
                _willShow = willShow;
                _onStartFocus = onStartFocus;
                _focusOnStart = focusOnStart;
            }

            public GmgToolbarRow(string title, Action willShow) : this(title, willShow, () => { })
            {
            }

            public void Show()
            {
                _willShow();
            }

            public void OnStartFocus(bool bySelect)
            {
                if (!_focusOnStart && !bySelect) return;
                _onStartFocus();
            }

            internal string GetTitle()
            {
                return _title;
            }
        }
    }
}