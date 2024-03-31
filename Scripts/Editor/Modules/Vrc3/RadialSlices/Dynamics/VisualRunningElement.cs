#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics
{
    public class VisualRunningElement : VisualElement
    {
        public VisualRunningElement(bool active)
        {
            pickingMode = PickingMode.Ignore;
            style.width = 50;
            style.height = 50;
            style.position = UIEPosition.Absolute;
            style.backgroundImage = ModuleVrc3Styles.RunningParam;

            visible = active;

            experimental.animation.Start(25f, 200f, int.MaxValue, Animation);
        }

        private void Animation(VisualElement element, float val)
        {
            var eulerVector = transform.rotation.eulerAngles;
            eulerVector.z += 2f;
            transform.rotation = Quaternion.Euler(eulerVector);
        }
    }
}
#endif