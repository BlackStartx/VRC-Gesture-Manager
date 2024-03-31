#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Library;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets.Base
{
    public abstract class BaseAxisPuppet : BasePuppet
    {
        internal override float Clamp => 60F;

        private readonly VRCExpressionsMenu.Control.Label[] _labels;

        protected BaseAxisPuppet(RadialSliceControl control) : base(140, control)
        {
            const int size = 70;
            Add(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } }
                .With(RadialMenuUtility.Prefabs.NewBorder(size, 45))
                .With(RadialMenuUtility.Prefabs.NewBorder(size, 135))
                .With(RadialMenuUtility.Prefabs.NewBorder(size, 225))
                .With(RadialMenuUtility.Prefabs.NewBorder(size, 315))
            );
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));
            _labels = control.GetSubLabels();
        }

        public override void AfterCursor()
        {
            const int v = 50;
            const int size = 24;
            Add(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } }
                .With(RadialMenuUtility.Prefabs.NewIconText(0, -v, size, LabelIcon(0, ModuleVrc3Styles.AxisUp), LabelText(0)))
                .With(RadialMenuUtility.Prefabs.NewIconText(v, 0, size, LabelIcon(1, ModuleVrc3Styles.AxisRight), LabelText(1)))
                .With(RadialMenuUtility.Prefabs.NewIconText(0, v, size, LabelIcon(2, ModuleVrc3Styles.AxisDown), LabelText(2)))
                .With(RadialMenuUtility.Prefabs.NewIconText(-v, 0, size, LabelIcon(3, ModuleVrc3Styles.AxisLeft), LabelText(3)))
            );
        }

        private Texture2D LabelIcon(int index, Texture2D def) => _labels == null || _labels.Length <= index || !_labels[index].icon ? def : _labels[index].icon;

        private string LabelText(int index) => _labels == null || _labels.Length <= index ? null : _labels[index].name;

        public override void UpdateValue(string pName, float value)
        {
        }
    }
}
#endif