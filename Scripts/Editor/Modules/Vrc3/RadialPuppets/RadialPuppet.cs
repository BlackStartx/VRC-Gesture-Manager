#if VRC_SDK_VRCSDK3
using System;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets.Base;
using BlackStartX.GestureManager.Library;
using BlackStartX.GestureManager.Library.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;
using Range = BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.RadialSliceControl.RadialSettings.Range;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets
{
    public class RadialPuppet : BasePuppet
    {
        internal override float Clamp => 20F;

        private readonly TextElement _text;
        private readonly VisualElement _arrow;
        private readonly GmgCircleElement _progress;

        private Range _checkpoint = Range.M1;

        private float Get => Control.GetSubValue(0);

        public RadialPuppet(RadialSliceControl control) : base(100, control)
        {
            _progress = this.MyAdd(RadialMenuUtility.Prefabs.NewCircle(96, RadialMenuUtility.Colors.CustomSelected, RadialMenuUtility.Colors.CustomSelected, UIEPosition.Absolute));
            Add(RadialMenuUtility.Prefabs.NewCircle(65, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));
            Add(RadialMenuUtility.Prefabs.NewRadialText(out _text, 0, UIEPosition.Absolute));
            SetupCheckpoint(control.Settings.Checkpoint);
            _arrow = this.MyAdd(ArrowElement());

            ShowValue(Get);
        }

        private void SetupCheckpoint(float? checkpoint, float scale = 0.6f)
        {
            if (!checkpoint.HasValue) return;
            var checkElement = this.MyAdd(ArrowElement(scale));
            _checkpoint = Control.Settings.RangeFrom(checkpoint.Value);
            checkElement.transform.rotation = _checkpoint.Rotation;
        }

        private void ShowValue(float value)
        {
            var range = Control.Settings.RangeFrom(value);
            _progress.Progress = range.Value;
            _text.text = Control.Settings.Display(value);
            _arrow.transform.rotation = range.Rotation;
        }

        public override void UpdateValue(string pName, float value)
        {
            if (Control.GetSubParameterName(0) == pName) ShowValue(Control.NonAmplifiedValue(value));
        }

        public override void Update(RadialCursor cursor)
        {
            if (cursor.GetRadial(Clamp, out var range)) TrySetValue(Control.Settings.RangeFrom(Get), range);
        }

        private void TrySetValue(Range from, Range to)
        {
            var pRange = to - from;
            var mRange = from - to;

            if (mRange.Value > 0.5f) to = Range.One;
            if (pRange.Value > 0.5f) to = Range.Zero;

            if (Math.Abs((to - _checkpoint).Value) < 0.03f) to = _checkpoint;

            Control.SetSubValue(0, Control.Settings.ValueFrom(to));
        }

        public override void AfterCursor() => _text.parent.BringToFront();

        /*
         * Static
         */

        private static VisualElement ArrowElement(float scale = 1f)
        {
            var container = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } };
            var element = container.MyAdd(new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    width = 20 * scale,
                    height = 20 * scale,
                    top = -(50 + 15 * scale),
                    left = -20 * scale / 2,
                    position = UIEPosition.Absolute,
                    backgroundColor = RadialMenuUtility.Colors.CustomSelected
                }
            }).MyBorder(2f * scale, 0f, RadialMenuUtility.Colors.ProgressBorder);
            element.transform.rotation = Quaternion.Euler(0, 0, 45);
            return container;
        }
    }
}
#endif