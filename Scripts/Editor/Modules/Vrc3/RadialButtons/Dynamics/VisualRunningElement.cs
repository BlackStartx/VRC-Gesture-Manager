#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons.Dynamics
{
    public class VisualRunningElement : VisualElement
    {
        public VisualRunningElement(bool active)
        {
            style.left = 25;
            style.top = 25;

            Add(new VisualElement
            {
                style =
                {
                    width = 50,
                    height = 50,
                    left = -25,
                    top = -25,
                    position = Position.Absolute,
                    backgroundImage = ModuleVrc3Styles.RunningParam
                }
            });

            visible = active;

            experimental.animation.Start(25f, 200f, int.MaxValue, (b, val) =>
            {
                var eulerVector = transform.rotation.eulerAngles;
                eulerVector.z += 2f;
                transform.rotation = Quaternion.Euler(eulerVector);
            });
        }
    }
}
#endif