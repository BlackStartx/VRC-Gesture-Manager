#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons.Dynamics
{
    public class VisualRunningElement : VisualElement
    {
        public VisualRunningElement(bool active)
        {
            style.positionLeft = 25;
            style.positionTop = 25;

            Add(new VisualElement
            {
                style =
                {
                    width = 50,
                    height = 50,
                    positionLeft = -25,
                    positionTop = -25,
                    positionType = PositionType.Absolute,
                    backgroundImage = ModuleVrc3Styles.RunningParam
                }
            });

            visible = active;
        }

        protected override void DoRepaint(IStylePainter painter)
        {
            var eulerVector = transform.rotation.eulerAngles;
            eulerVector.z += 2f;
            transform.rotation = Quaternion.Euler(eulerVector);
            base.DoRepaint(painter);
        }
    }
}
#endif