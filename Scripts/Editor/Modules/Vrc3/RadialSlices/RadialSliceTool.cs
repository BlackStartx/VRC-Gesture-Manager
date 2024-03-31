#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Tools;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceTool : RadialSliceDynamic
    {
        private readonly Color _activeColor = new(0.5f, 1f, 0.5f);
        private readonly GmgDynamicFunction _tool;
        private readonly ModuleVrc3 _module;

        public RadialSliceTool(ModuleVrc3 module, GmgDynamicFunction tool, Texture2D icon) : base(tool.Name, icon, null, DynamicType.Running, 1f)
        {
            _tool = tool;
            _module = module;
        }

        protected override float FloatValue() => _tool.Active ? 1f : 0f;

        public override void OnClickStart()
        {
        }

        public override void OnClickEnd()
        {
            _tool.Toggle(_module);
            CheckRunningUpdate();
        }

        protected override void OnValueChanged(bool active)
        {
            IdleCenterColor = active ? Color.green : RadialMenuUtility.Colors.CenterIdle;
            IdleBorderColor = active ? RadialMenuUtility.Colors.CustomMain * _activeColor : RadialMenuUtility.Colors.CustomMain;
            SelectedCenterColor = active ? Color.green : RadialMenuUtility.Colors.CenterSelected;
            SelectedBorderColor = active ? RadialMenuUtility.Colors.CustomSelected * _activeColor : RadialMenuUtility.Colors.CustomSelected;
        }
    }
}
#endif