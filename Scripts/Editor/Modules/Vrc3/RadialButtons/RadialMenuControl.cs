#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons
{
    public class RadialMenuControl : RadialMenuDynamic
    {
        private readonly RadialMenu _menu;
        private readonly Vrc3Param _parameter;
        private readonly Vrc3Param[] _subParameters;
        private readonly VRCExpressionsMenu _subMenu;
        private readonly VRCExpressionsMenu.Control.Label[] _subLabels;

        public RadialMenuControl(RadialMenu menu, VRCExpressionsMenu.Control control) : base(control.name, control.icon, control.type, control.value)
        {
            _menu = menu;
            _subMenu = control.subMenu;
            _subLabels = control.labels;
            _parameter = menu.GetParam(control.parameter.name);
            _subParameters = control.subParameters.Select(parameter => menu.GetParam(parameter.name)).ToArray();
        }

        public RadialMenuControl(RadialMenu menu, string name, Texture2D icon, ControlType type, float activeValue, Vrc3Param param, Vrc3Param[] subParams, VRCExpressionsMenu subMenu, VRCExpressionsMenu.Control.Label[] subLabels) : base(name, icon, type, activeValue)
        {
            _menu = menu;
            _subMenu = subMenu;
            _parameter = param;
            _subLabels = subLabels;
            _subParameters = subParams;
            if (_parameter == null && _subParameters.All(vrc3Param => vrc3Param == null)) TextColor = Color.gray;
        }

        public override void OnClickStart()
        {
            switch (Type)
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
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnClickEnd()
        {
            switch (Type)
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
                default: throw new ArgumentOutOfRangeException();
            }
        }

        internal void SetControlValue() => SetValue(ActiveValue);

        internal void SetValue(float value) => _parameter?.Set(_menu.RadialMenus, value);

        internal void SetSubValue(int index, float value) => _subParameters[index]?.Set(_menu.RadialMenus, value);

        public VRCExpressionsMenu.Control.Label[] GetSubLabels() => _subLabels;

        internal string GetSubParameterName(int index) => _subParameters[index].Name;

        protected override float GetValue() => _parameter?.Get() ?? 0;

        internal override float GetSubValue(int index) => _subParameters[index]?.Get() ?? 0;
    }
}
#endif