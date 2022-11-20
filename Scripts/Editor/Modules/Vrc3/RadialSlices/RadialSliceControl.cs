#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceControl : RadialSliceDynamic
    {
        private readonly float? _amplify;
        private readonly RadialMenu _menu;
        private readonly ControlType _type;
        private readonly Vrc3Param _parameter;
        private readonly Vrc3Param[] _subParameters;
        private readonly VRCExpressionsMenu _subMenu;
        private readonly VRCExpressionsMenu.Control.Label[] _subLabels;

        public RadialSliceControl(RadialMenu menu, VRCExpressionsMenu.Control control) : base(control.name, control.icon, RadialMenuUtility.GetSubIcon(control.type), RadialMenuUtility.GetDynamicType(control.type), control.value)
        {
            _menu = menu;
            _type = control.type;
            _subMenu = control.subMenu;
            _subLabels = control.labels;
            _parameter = menu.GetParam(control.parameter.name);
            _subParameters = control.subParameters == null ? Array.Empty<Vrc3Param>() : control.subParameters.Select(parameter => menu.GetParam(parameter.name)).ToArray();
        }

        public RadialSliceControl(RadialMenu menu, string name, Texture2D icon, ControlType type, float activeValue, Vrc3Param param, Vrc3Param[] subParams, VRCExpressionsMenu subMenu, VRCExpressionsMenu.Control.Label[] subLabels, float? amplify = null) : base(name, icon, RadialMenuUtility.GetSubIcon(type), RadialMenuUtility.GetDynamicType(type), activeValue)
        {
            _menu = menu;
            _type = type;
            _amplify = amplify;
            _subMenu = subMenu;
            _parameter = param;
            _subLabels = subLabels;
            _subParameters = subParams;
            if (_parameter == null && _subParameters.All(vrc3Param => vrc3Param == null)) TextColor = Color.gray;
        }

        public override void OnClickStart()
        {
            switch (_type)
            {
                case ControlType.Button:
                    _menu.PressingButton?.OnClickEnd();
                    _menu.PressingButton = this;
                    SetControlValue();
                    break;
                case ControlType.Toggle:
                case ControlType.SubMenu:
                case ControlType.TwoAxisPuppet:
                case ControlType.FourAxisPuppet:
                case ControlType.RadialPuppet:
                default: break;
            }
        }

        public override void OnClickEnd()
        {
            switch (_type)
            {
                case ControlType.Button:
                    if (_menu.PressingButton == this) _menu.PressingButton = null;
                    else return;
                    SetValue(0);
                    break;
                case ControlType.Toggle:
                    if (RadialMenuUtility.Is(GetValue(), ActiveValue)) SetValue(0);
                    else SetControlValue();
                    break;
                case ControlType.SubMenu:
                    if (!_subMenu) break;
                    SetControlValue();
                    _menu.OpenMenu(_subMenu, _parameter, ActiveValue);
                    break;
                case ControlType.TwoAxisPuppet:
                    _menu.OpenPuppet(new TwoAxisPuppet(this));
                    break;
                case ControlType.FourAxisPuppet:
                    _menu.OpenPuppet(new FourAxisPuppet(this));
                    break;
                case ControlType.RadialPuppet:
                    _menu.OpenPuppet(new RadialPuppet(this));
                    break;
            }
        }

        private float AmplifiedValue(float value) => _amplify.HasValue ? value * _amplify.Value : value;

        internal float NonAmplifiedValue(float value) => _amplify.HasValue ? value / _amplify.Value : value;

        internal void SetControlValue() => SetValue(ActiveValue);

        internal void SetValue(float value) => _parameter?.Set(_menu.Module, AmplifiedValue(value));

        internal void SetSubValue(int index, float value) => _subParameters[index]?.Set(_menu.Module, AmplifiedValue(value));

        public VRCExpressionsMenu.Control.Label[] GetSubLabels() => _subLabels;

        internal string GetSubParameterName(int index) => _subParameters[index].Name;

        protected override float GetValue() => NonAmplifiedValue(_parameter?.Get() ?? 0);

        internal override float GetSubValue(int index) => NonAmplifiedValue(_subParameters[index]?.Get() ?? 0);
    }
}
#endif