#if VRC_SDK_VRCSDK3
using System;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons.Dynamics;
using UnityEngine;
using UnityEngine.UIElements;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons
{
    public abstract class RadialMenuDynamic : RadialMenuItem
    {
        internal readonly ControlType Type;
        internal readonly float ActiveValue;

        private readonly bool _instance;
        private VisualElement _dynamicElement;
        private readonly DynamicType _dynamicType;

        private bool RunCheck => RadialMenuUtility.Is(GetValue(), ActiveValue);

        internal RadialMenuDynamic(string name, Texture2D icon, ControlType type, float activeValue) : base(name, icon, RadialMenuUtility.GetSubIcon(type))
        {
            Type = type;
            ActiveValue = activeValue;
            _dynamicType = GetDynamicType();
            _instance = _dynamicType != DynamicType.None;
        }

        private DynamicType GetDynamicType()
        {
            switch (Type)
            {
                case ControlType.Button:
                case ControlType.Toggle:
                    return DynamicType.Running;
                case ControlType.RadialPuppet:
                    return DynamicType.Radial;
                case ControlType.SubMenu:
                case ControlType.TwoAxisPuppet:
                case ControlType.FourAxisPuppet:
                    return DynamicType.None;
                default: throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
            }
        }

        protected override void CreateExtra()
        {
            if (!_instance) return;
            _dynamicElement = InstanceElement();
        }

        internal void CheckRunningUpdate()
        {
            if (!_instance) return;

            switch (_dynamicElement)
            {
                case VisualRunningElement runningElement:
                    runningElement.visible = RunCheck;
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
                    return Texture.MyAdd(new VisualRunningElement(RunCheck));
                case DynamicType.Radial:
                    return Texture.MyAdd(new VisualRadialElement(GetSubValue(0)));
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract float GetValue();

        internal abstract float GetSubValue(int index);

        private enum DynamicType
        {
            None,
            Running,
            Radial
        }
    }
}
#endif