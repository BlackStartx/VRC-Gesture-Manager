#if VRC_SDK_VRCSDK3
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics
{
    public class VisualRadialElement : VisualElement
    {
        private readonly RadialSliceControl.RadialSettings _settings;
        private readonly TextElement _text;

        public VisualRadialElement(RadialSliceControl.RadialSettings settings, float value)
        {
            RadialMenuUtility.Prefabs.SetRadialText(this, out _text, 15);
            _settings = settings;
            SetValue(value);
        }

        public void SetValue(float value) => _text.text = _settings.Display(value);
    }
}
#endif