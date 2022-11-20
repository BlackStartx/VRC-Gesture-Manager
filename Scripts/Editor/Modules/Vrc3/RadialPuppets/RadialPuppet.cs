#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Lib;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets.Base;
using BlackStartX.GestureManager.Runtime.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets
{
    public class RadialPuppet : BasePuppet
    {
        internal override float Clamp => 20F;

        private readonly TextElement _text;
        private readonly VisualElement _arrow;
        private readonly GmgCircleElement _progress;

        private float Get => Control.GetSubValue(0);

        public RadialPuppet(RadialSliceControl control) : base(100, control)
        {
            _progress = this.MyAdd(RadialMenuUtility.Prefabs.NewCircle(96, RadialMenuUtility.Colors.CustomSelected, RadialMenuUtility.Colors.CustomSelected, UIEPosition.Absolute));
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));
            Add(RadialMenuUtility.Prefabs.NewRadialText(out _text, 0, UIEPosition.Absolute));
            _arrow = this.MyAdd(ArrowElement());

            ShowValue(Get);
        }

        private void ShowValue(float value)
        {
            var intValue = RadialMenuUtility.RadialPercentage(value, out var cValue);
            _progress.Progress = value;
            _text.text = intValue + "%";
            _arrow.transform.rotation = Quaternion.Euler(0, 0, cValue * 360f);
        }

        public override void UpdateValue(string pName, float value)
        {
            if (Control.GetSubParameterName(0) == pName) ShowValue(Control.NonAmplifiedValue(value));
        }

        public override void Update(RadialCursor cursor)
        {
            if (cursor.GetRadial(Clamp, out var radial)) TrySetValue(Get, radial);
        }

        private void TrySetValue(float from, float to)
        {
            var pDistance = to - from;
            var mDistance = from - to;

            if (mDistance > 0.5f) to = 1f;
            if (pDistance > 0.5f) to = 0f;

            Control.SetSubValue(0, to);
        }

        public override void AfterCursor() => _text.parent.BringToFront();

        /*
         * Static
         */

        private static VisualElement ArrowElement()
        {
            var container = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } };
            var element = container.MyAdd(new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    width = 20,
                    height = 20,
                    backgroundColor = RadialMenuUtility.Colors.CustomSelected,
                    top = -65,
                    position = UIEPosition.Absolute
                }
            }).MyBorder(2f, 0f, RadialMenuUtility.Colors.ProgressBorder);
            element.transform.rotation = Quaternion.Euler(0, 0, 45);
            return container;
        }
    }
}
#endif