#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class Vrc3WeightController
    {
        internal readonly ModuleVrc3 Module;

        private readonly Vrc3WeightSlider _lWeight;
        private readonly Vrc3WeightSlider _rWeight;
        internal bool Dragging => _lWeight.Drag || _rWeight.Drag;
        internal bool Active => _lWeight.Active || _rWeight.Active;

        public Vrc3WeightController(ModuleVrc3 module)
        {
            Module = module;
            _lWeight = new Vrc3WeightSlider(this, "GestureLeftWeight");
            _rWeight = new Vrc3WeightSlider(this, "GestureRightWeight");
        }

        public void RenderLeft(VisualElement root)
        {
            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(18));
            var rect = GmgLayoutHelper.GetLastRect(ref _lWeight.Rect);
            _lWeight.Render(root, rect);
            _lWeight.ShowWeight();
        }

        public void RenderRight(VisualElement root)
        {
            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(18));
            var rect = GmgLayoutHelper.GetLastRect(ref _rWeight.Rect);
            _rWeight.Render(root, rect);
            _rWeight.ShowWeight();
        }

        public void CheckCondition(int selectedIndex)
        {
            _lWeight.CheckCondition(selectedIndex);
            _rWeight.CheckCondition(selectedIndex);
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