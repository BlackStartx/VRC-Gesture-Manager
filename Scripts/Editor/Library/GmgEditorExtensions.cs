using UnityEditor;
using UnityEditor.UIElements;

namespace BlackStartX.GestureManager.Editor.Library
{
    public static class GmgEditorExtensions
    {
        public static void MySetAntiAliasing(this EditorWindow window, int antiAliasing)
        {
            if (!window || window.GetAntiAliasing() == antiAliasing) return;

            window.SetAntiAliasing(antiAliasing);
            // Dumb workaround method to trigger the internal MakeParentsSettingsMatchMe() method on the EditorWindow.
            window.minSize = window.minSize;
        }
    }
}