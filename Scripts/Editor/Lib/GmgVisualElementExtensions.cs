using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Lib
{
    public static class GmgVisualElementExtensions
    {
        public static T MyAdd<T>(this VisualElement visualElement, T element) where T : VisualElement
        {
            visualElement.Add(element);
            return element;
        }

        public static T With<T>(this T visualElement, VisualElement child) where T : VisualElement
        {
            visualElement.Add(child);
            return visualElement;
        }

        public static T MyBorder<T>(this T visualElement, float width, float radius, Color color) where T : VisualElement
        {
            visualElement.style.borderBottomRightRadius = radius;
            visualElement.style.borderBottomLeftRadius = radius;
            visualElement.style.borderTopRightRadius = radius;
            visualElement.style.borderTopLeftRadius = radius;
            visualElement.style.borderBottomWidth = width;
            visualElement.style.borderRightWidth = width;
            visualElement.style.borderLeftWidth = width;
            visualElement.style.borderTopWidth = width;
            visualElement.style.borderBottomColor = color;
            visualElement.style.borderRightColor = color;
            visualElement.style.borderLeftColor = color;
            visualElement.style.borderTopColor = color;
            return visualElement;
        }

        public static void SetVisibility<T>(this T visualElement, bool f) where T : VisualElement
        {
            visualElement.style.visibility = new StyleEnum<Visibility>(f ? Visibility.Visible : Visibility.Hidden);
            visualElement.style.display = new StyleEnum<DisplayStyle>(f ? DisplayStyle.Flex : DisplayStyle.None);
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