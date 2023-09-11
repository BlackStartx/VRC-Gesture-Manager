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
#if UNITY_2022_1_OR_NEWER // TEMP FIX FOR UNITY 2022 pivot change~
            style.transformOrigin = new StyleTransformOrigin();
#endif                    // TEMP FIX FOR UNITY 2022 pivot change~
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