#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Vrc3Debug.Osc
{
    public class Vrc3OscDebugWindow : EditorWindow
    {
        private ModuleVrc3 _source;
        private Vector2 _scroll;

        private VisualEpContainer _holder;
        private VisualEpContainer Holder => _holder ??= new VisualEpContainer();

        internal static Vrc3OscDebugWindow Create(ModuleVrc3 source)
        {
            var instance = CreateInstance<Vrc3OscDebugWindow>();
            instance.titleContent = new GUIContent("[Debug Window] Gesture Manager");
            instance._source = source;
            instance.Show();
            return instance;
        }

        internal static Vrc3OscDebugWindow Close(Vrc3OscDebugWindow source)
        {
            source.Close();
            return null;
        }

        private void OnGUI()
        {
            if (_source == null) Close();
            else DebugGUI();
        }

        private void DebugGUI()
        {
            var isFullScreen = EditorGUIUtility.currentViewWidth > 1279;

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label("Gesture Manager - Osc Debug Window", GestureManagerStyles.Header);
            _source.DebugContext(rootVisualElement, Holder, 1, EditorGUIUtility.currentViewWidth - 60, isFullScreen);
            GUILayout.Space(25);
            GUILayout.EndScrollView();

            Holder.style.top = new StyleLength(43 - _scroll.y);
            if (_source.OscModule.ToolBar.Selected != 0) Holder.StopRendering();
        }
    }
}
#endif