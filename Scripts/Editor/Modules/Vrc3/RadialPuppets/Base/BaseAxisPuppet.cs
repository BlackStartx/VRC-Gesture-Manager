#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;
using UIEPosition = UnityEngine.UIElements.Position;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base
{
    public abstract class BaseAxisPuppet : BasePuppet
    {
        internal override float Clamp => 60F;

        private readonly VRCExpressionsMenu.Control.Label[] _labels;

        protected BaseAxisPuppet(RadialMenuControl control) : base(140, control)
        {
            var holder = this.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 45));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 135));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 225));
            holder.Add(RadialMenuUtility.Prefabs.NewBorder(70, 315));
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));
            _labels = control.GetSubLabels();
        }

        public override void AfterCursor()
        {
            const int v = 50;
            var holder = this.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(0, -v, 24, LabelIcon(0, ModuleVrc3Styles.AxisUp), LabelText(0)));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(v, 0, 24, LabelIcon(1, ModuleVrc3Styles.AxisRight), LabelText(1)));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(0, v, 24, LabelIcon(2, ModuleVrc3Styles.AxisDown), LabelText(2)));
            holder.Add(RadialMenuUtility.Prefabs.NewIconText(-v, 0, 24, LabelIcon(3, ModuleVrc3Styles.AxisLeft), LabelText(3)));
        }

        private Texture2D LabelIcon(int index, Texture2D def) => _labels == null || _labels.Length <= index || !_labels[index].icon ? def : _labels[index].icon;

        private string LabelText(int index) => _labels == null || _labels.Length <= index ? null : _labels[index].name;

        public override void UpdateValue(string pName, float value)
        {
        }
    }
}
#endif