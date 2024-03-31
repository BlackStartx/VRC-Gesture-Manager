#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Library;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class Vrc3WeightController
    {
        internal readonly ModuleVrc3 Module;

        private readonly Vrc3WeightSlider _lWeight;
        private readonly Vrc3WeightSlider _rWeight;
        internal bool Dragging => _lWeight.Drag || _rWeight.Drag;
        internal bool Active => _lWeight.Active || _rWeight.Active;

        private static readonly GUILayoutOption[] Options = { GUILayout.ExpandWidth(true), GUILayout.Height(10) };

        public Vrc3WeightController(ModuleVrc3 module)
        {
            Module = module;
            _lWeight = new Vrc3WeightSlider(this, Vrc3DefaultParams.GestureLeftWeight);
            _rWeight = new Vrc3WeightSlider(this, Vrc3DefaultParams.GestureRightWeight);
        }

        public void RenderLeft(VisualElement root)
        {
            GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, Options);
            _lWeight.Render(root, GmgLayoutHelper.GetLastRect(ref _lWeight.Rect));
            _lWeight.ShowWeight();
        }

        public void RenderRight(VisualElement root)
        {
            GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, Options);
            _rWeight.Render(root, GmgLayoutHelper.GetLastRect(ref _rWeight.Rect));
            _rWeight.ShowWeight();
        }

        public void CheckCondition(ModuleVrc3 module, RadialMenu menu)
        {
            _lWeight.CheckCondition(module, menu);
            _rWeight.CheckCondition(module, menu);
        }

        public void StopRendering()
        {
            _lWeight.StopRendering();
            _rWeight.StopRendering();
        }

        public void DisableDrag()
        {
            _lWeight.Drag = false;
            _rWeight.Drag = false;
        }
    }
}
#endif