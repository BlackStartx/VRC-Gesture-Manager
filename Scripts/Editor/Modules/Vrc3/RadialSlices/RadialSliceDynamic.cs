#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using UnityEngine.UIElements;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public abstract class RadialSliceDynamic : RadialSliceBase
    {
        internal readonly float ActiveValue;

        private VisualElement _dynamicElement;
        private readonly DynamicType _dynamicType;

        private bool RunCheck => RadialMenuUtility.Is(GetValue(), ActiveValue);

        internal RadialSliceDynamic(string name, Texture2D icon, Texture2D subIcon, DynamicType type, float activeValue) : base(name, icon, subIcon)
        {
            _dynamicType = type;
            ActiveValue = activeValue;
        }

        protected override void CreateExtra()
        {
            if ((_dynamicElement = InstanceElement()) == null) return;
            Texture.Add(_dynamicElement);
            OnValueChanged(_dynamicElement.visible);
        }

        internal void CheckRunningUpdate()
        {
            if (_dynamicElement == null) return;

            switch (_dynamicElement)
            {
                case VisualRunningElement runningElement:
                    if (runningElement.visible != (runningElement.visible = RunCheck)) OnValueChanged(runningElement.visible);
                    break;
                case VisualRadialElement sliderElement:
                    sliderElement.Value = GetSubValue(0);
                    break;
            }
        }

        private VisualElement InstanceElement()
        {
            switch (_dynamicType)
            {
                case DynamicType.None:
                    return null;
                case DynamicType.Running:
                    return new VisualRunningElement(RunCheck);
                case DynamicType.Radial:
                    return new VisualRadialElement(GetSubValue(0));
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract float GetValue();

        internal abstract float GetSubValue(int index);

        protected virtual void OnValueChanged(bool active)
        {
        }

        public enum DynamicType
        {
            None,
            Running,
            Radial
        }
    }
}
#endif