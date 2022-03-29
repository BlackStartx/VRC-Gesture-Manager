#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Core.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets
{
    public class RadialPuppet : BasePuppet
    {
        private readonly TextElement _text;
        private readonly VisualElement _point;
        private readonly GmgCircleElement _progress;

        private float Get => Control.GetSubValue(0);

        public RadialPuppet(RadialMenuControl control) : base(100, control)
        {
            _progress = this.MyAdd(RadialMenuUtility.Prefabs.NewCircle(96, RadialMenuUtility.Colors.ProgressRadial, RadialMenuUtility.Colors.ProgressRadial, PositionType.Absolute));
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.OuterBorder, PositionType.Absolute));
            Add(RadialMenuUtility.Prefabs.NewRadialText(out _text, 0, PositionType.Absolute));
            _point = this.MyAdd(GeneratePoint());

            ShowValue(Get);
        }

        private void ShowValue(float value)
        {
            var intValue = RadialMenuUtility.RadialPercentage(value, out var cValue);
            _progress.Progress = value;
            _text.text = intValue + "%";
            _point.transform.rotation = Quaternion.Euler(0, 0, cValue * 360f);
        }

        public override void UpdateValue(string pName, float value)
        {
            if (Control.GetSubParameterName(0) == pName) ShowValue(value);
        }

        public override void Update(Vector2 mouse, RadialCursor cursor)
        {
            if (cursor.GetRadial(mouse, out var radial)) TrySetValue(Get, radial);
        }

        private void TrySetValue(float from, float to)
        {
            var pDistance = to - from;
            var mDistance = from - to;

            if (mDistance > 0.5f) to = 1f;
            if (pDistance > 0.5f) to = 0f;

            Control.SetSubValue(0, to);
        }

        public override void AfterCursor()
        {
            _text.parent.BringToFront();
        }

        /*
         * Static
         */

        private static VisualElement GeneratePoint()
        {
            var container = new VisualElement {style = {positionType = PositionType.Absolute}};
            var element = container.MyAdd(GenerateArrow());

            element.style.borderColor = RadialMenuUtility.Colors.ProgressBorder;
            
            element.style.borderBottomWidth = 2f;
            element.style.borderRightWidth = 2f;
            element.style.borderLeftWidth = 2f;
            element.style.borderTopWidth = 2f;

            element.transform.rotation = Quaternion.Euler(0, 0, 45);
            return container;
        }

        private static VisualElement GenerateArrow()
        {
            return new VisualElement {style = {width = 20, height = 20, backgroundColor = RadialMenuUtility.Colors.ProgressRadial, positionTop = -65, positionType = PositionType.Absolute}};
        }
    }
}
#endif