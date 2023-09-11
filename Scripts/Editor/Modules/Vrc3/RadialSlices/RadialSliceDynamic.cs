#if VRC_SDK_VRCSDK3
using UnityEngine;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public abstract class RadialSliceDynamic : RadialSliceBase
    {
        internal readonly float ActiveValue;

        private VisualRunningElement _runningElement;
        private readonly DynamicType _dynamicType;

        private bool RunCheck => RadialMenuUtility.Is(FloatValue(), ActiveValue);

        internal RadialSliceDynamic(string name, Texture2D icon, Texture2D subIcon, DynamicType type, float activeValue) : base(name, icon, subIcon)
        {
            _dynamicType = type;
            ActiveValue = activeValue;
        }

        protected override void CreateExtra()
        {
            if ((_runningElement = _dynamicType == DynamicType.Running ? new VisualRunningElement(RunCheck) : null) == null) return;
            Texture.Add(_runningElement);
            OnValueChanged(_runningElement.visible);
        }

        protected internal override void CheckRunningUpdate()
        {
            if (_runningElement == null) return;
            if (_runningElement.visible != (_runningElement.visible = RunCheck)) OnValueChanged(_runningElement.visible);
        }

        protected abstract float FloatValue();

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