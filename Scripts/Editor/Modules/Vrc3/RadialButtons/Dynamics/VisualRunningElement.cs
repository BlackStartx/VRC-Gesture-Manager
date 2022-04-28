#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons.Dynamics
{
    public class VisualRunningElement : VisualElement
    {
        public VisualRunningElement(bool active)
        {
            pickingMode = PickingMode.Ignore;
            style.left = 25;
            style.top = 25;

            Add(new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    width = 50,
                    height = 50,
                    left = -25,
                    top = -25,
                    position = UIEPosition.Absolute,
                    backgroundImage = ModuleVrc3Styles.RunningParam
                }
            });

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