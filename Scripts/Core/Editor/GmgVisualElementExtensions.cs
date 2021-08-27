using GestureManager.Scripts.Core.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
    public static class GmgVisualElementExtensions
    {
        public static T MyAdd<T>(this VisualElement visualElement, T child) where T : VisualElement
        {
            visualElement.Add(child);
            return child;
        }

        public static T With<T>(this T visualElement, VisualElement child) where T : VisualElement
        {
            visualElement.Add(child);
            return visualElement;
        }

        public static void MyBorder(this GmgCircleElement visualElement, float width, float radius, Color color)
        {
            visualElement.BorderColor = color;
            visualElement.BorderWidth = width;
        }

        public static T OnClickUpEvent<T>(this T visualElement, EventCallback<MouseUpEvent> action) where T : VisualElement
        {
            visualElement.RegisterCallback(action);
            return visualElement;
        }

        public static T OnClickDownEvent<T>(this T visualElement, EventCallback<MouseDownEvent> action) where T : VisualElement
        {
            visualElement.RegisterCallback(action);
            return visualElement;
        }

        public static void MySetAntiAliasing(this EditorWindow window, int antiAliasing)
        {
            if (!window || window.GetAntiAliasing() == antiAliasing) return;

            window.SetAntiAliasing(antiAliasing);
            // Dumb workaround method to trigger the internal MakeParentsSettingsMatchMe() method on the EditorWindow.
            window.minSize = window.minSize;
        }
    }
}