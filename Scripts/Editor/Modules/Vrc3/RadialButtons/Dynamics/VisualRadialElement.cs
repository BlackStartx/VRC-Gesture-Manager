#if VRC_SDK_VRCSDK3
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons.Dynamics
{
    public class VisualRadialElement : VisualElement
    {
        private readonly TextElement _text;

        public float Value
        {
            set => _text.text = RadialMenuUtility.RadialPercentage(value) + "%";
        }

        public VisualRadialElement(float value)
        {
            RadialMenuUtility.Prefabs.SetRadialText(this, out _text, 15);
            Value = value;
        }
    }
}
#endif