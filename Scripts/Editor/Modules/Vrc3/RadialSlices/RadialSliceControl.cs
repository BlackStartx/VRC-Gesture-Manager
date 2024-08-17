#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceControl : RadialSliceDynamic
    {
        internal readonly RadialSettings Settings;

        private readonly float? _amplify;
        private readonly RadialMenu _menu;
        private readonly ControlType _type;
        private readonly Vrc3Param _parameter;
        private readonly Vrc3Param[] _subParameters;
        private readonly VRCExpressionsMenu _subMenu;
        private readonly VRCExpressionsMenu.Control.Label[] _subLabels;

        private VisualRadialElement _radialElement;

        public RadialSliceControl(RadialMenu menu, VRCExpressionsMenu.Control control) : base(control.name, control.icon, RadialMenuUtility.GetSubIcon(control.type), RadialMenuUtility.GetDynamicType(control.type), control.value)
        {
            _menu = menu;
            _type = control.type;
            _subMenu = control.subMenu;
            _subLabels = control.labels;
            _parameter = menu.GetParam(control.parameter.name);
            _subParameters = control.subParameters == null ? Array.Empty<Vrc3Param>() : control.subParameters.Select(parameter => menu.GetParam(parameter.name)).ToArray();
            Settings = RadialSettings.Base;
        }

        public RadialSliceControl(RadialMenu menu, string name, Texture2D icon, ControlType type, float activeValue, Vrc3Param param, Vrc3Param[] subParams, VRCExpressionsMenu subMenu, VRCExpressionsMenu.Control.Label[] subLabels, float? amplify = null, RadialSettings settings = null) : base(name, icon, RadialMenuUtility.GetSubIcon(type), RadialMenuUtility.GetDynamicType(type), activeValue)
        {
            _menu = menu;
            _type = type;
            _amplify = amplify;
            _subMenu = subMenu;
            _parameter = param;
            _subLabels = subLabels;
            _subParameters = subParams;
            Settings = settings ?? RadialSettings.Base;
        }

        protected override void CreateExtra()
        {
            base.CreateExtra();
            if ((_radialElement = _type == ControlType.RadialPuppet ? new VisualRadialElement(Settings, GetSubValue(0)) : null) == null) return;
            Texture.Add(_radialElement);
        }

        protected internal override void CheckRunningUpdate()
        {
            base.CheckRunningUpdate();
            _radialElement?.SetValue(GetSubValue(0));
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
                    if (RadialMenuUtility.Is(FloatValue(), ActiveValue)) SetValue(0);
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
                default: return;
            }
        }

        private float AmplifiedValue(float value) => _amplify.HasValue ? value * _amplify.Value : value;

        internal float NonAmplifiedValue(float value) => _amplify.HasValue ? value / _amplify.Value : value;

        internal void SetControlValue() => SetValue(ActiveValue);

        internal void SetValue(float value) => _parameter?.Set(_menu.Module, AmplifiedValue(value));

        internal void SetSubValue(int index, float value) => _subParameters[index]?.Set(_menu.Module, AmplifiedValue(value));

        public VRCExpressionsMenu.Control.Label[] GetSubLabels() => _subLabels;

        internal string GetSubParameterName(int index) => _subParameters[index]?.Name;

        protected override float FloatValue() => NonAmplifiedValue(_parameter?.FloatValue() ?? 0f);

        internal float GetSubValue(int index) => NonAmplifiedValue(_subParameters[index]?.FloatValue() ?? 0f);

        public class RadialSettings
        {
            private static RadialSettings _base;
            public static RadialSettings Base => _base ??= new RadialSettings(DisplayType.Percentage, 0f, 1f, null);
            public static RadialSettings Height(float checkpoint) => new(DisplayType.Meters, 0.2f, 5.0f, checkpoint);

            private readonly DisplayType _type;

            private readonly float _min;
            private readonly float _gap;

            public readonly float? Checkpoint;

            private RadialSettings(DisplayType type, float min, float max, float? checkpoint)
            {
                _type = type;
                Checkpoint = checkpoint;
                _gap = max - (_min = min);
            }

            public string Display(float value) => _type switch
            {
                DisplayType.Meters => $"{value:F}m",
                DisplayType.Percentage => $"{RangeFrom(value).Percentage}%",
                _ => null
            };

            private enum DisplayType
            {
                Meters,
                Percentage
            }

            public Range RangeFrom(float value) => new((value - _min) / _gap);

            public float ValueFrom(Range range) => range.Value * _gap + _min;

            public readonly struct Range
            {
                public static Range M1 = new(-1f);
                public static Range Zero = new(0);
                public static Range One = new(1f);

                public readonly float Value;

                public Range(float value) => Value = value;

                public int Percentage => (int)Math.Round(Value * 100, MidpointRounding.ToEven);

                public Quaternion Rotation => Quaternion.Euler(0, 0, Value * 360f);

                public static Range operator -(Range lRange, Range rRange) => new(lRange.Value - rRange.Value);
            }
        }
    }
}
#endif