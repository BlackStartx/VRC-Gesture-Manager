#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base
{
    public abstract class BaseAxisPuppet : BasePuppet
    {
        private readonly VRCExpressionsMenu.Control.Label[] _labels;

        protected BaseAxisPuppet(RadialMenuControl control) : base(140, control)
        {
            var holder = this.MyAdd(new VisualElement {style = {position = Position.Absolute}});
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 45));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 135));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 225));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 315));
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.OuterBorder, Position.Absolute));
            _labels = control.GetSubLabels();
        }

        public override void AfterCursor()
        {
            const int v = 50;
            var holder = this.MyAdd(new VisualElement {style = {position = Position.Absolute}});
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(0, -v, 24, _labels[0].icon ? _labels[0].icon : ModuleVrc3Styles.AxisUp, _labels[0].name));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(v, 0, 24, _labels[1].icon ? _labels[1].icon : ModuleVrc3Styles.AxisRight, _labels[1].name));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(0, v, 24, _labels[2].icon ? _labels[2].icon : ModuleVrc3Styles.AxisDown, _labels[2].name));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(-v, 0, 24, _labels[3].icon ? _labels[3].icon : ModuleVrc3Styles.AxisLeft, _labels[3].name));
        }

        public override void UpdateValue(string pName, float value)
        {
        }
    }
}
#endif