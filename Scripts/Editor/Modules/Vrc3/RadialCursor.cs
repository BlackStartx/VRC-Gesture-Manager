#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Library;
using BlackStartX.GestureManager.Library.VisualElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Range = BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.RadialSliceControl.RadialSettings.Range;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class RadialCursor : GmgCircleElement
    {
        private const float CursorSize = 50;

        private const float MiddleSize = CursorSize / 1.5f;
        private const float SmallSize = CursorSize / 4f;

        private float _clampReset;
        private float _clamp;
        private float _min;
        private float _max;

        private Vector2 _position;
        internal int Selection = -1;

        public RadialCursor()
        {
            RadialMenuUtility.Prefabs.SetCircle(this, CursorSize, RadialMenuUtility.Colors.Cursor, RadialMenuUtility.Colors.CursorBorder)
                .MyAdd(RadialMenuUtility.Prefabs.NewCircleBorder(MiddleSize, RadialMenuUtility.Colors.CursorBorder))
                .Add(RadialMenuUtility.Prefabs.NewCircleBorder(SmallSize, RadialMenuUtility.Colors.CursorBorder));
        }

        /*
         * Data
         */

        private void SetParent(VisualElement referenceLayout)
        {
            parent.Remove(this);
            referenceLayout.Add(this);
        }

        public void SetData(float clamp, float clampReset, float min, float max, VisualElement referenceLayout)
        {
            _min = min;
            _max = max;
            _clamp = clamp;
            _clampReset = clampReset;
            if (referenceLayout != null && referenceLayout != parent) SetParent(referenceLayout);
        }

        public void Update(Vector2 mouse, IList<RadialSliceBase> selectionTuple, bool puppet)
        {
            if (mouse.magnitude < _clampReset) SetCursorPosition(mouse);
            else SetCursorPosition(0, 0);
            if (selectionTuple == null) return;
            var intSelection = GetChoice(selectionTuple.Count, puppet);
            if (Selection != intSelection) UpdateSelection(selectionTuple, intSelection);
        }

        internal void UpdateSelection(IList<RadialSliceBase> selectionTuple, int selection)
        {
            if (Selection != -1) Des(selectionTuple[Selection]);
            if (selection != -1) Sel(selectionTuple[selection]);
            Selection = selection;
        }

        private static void Des(RadialSliceBase oldElement)
        {
            oldElement.Selected = false;
            oldElement.DataHolder.experimental.animation.Scale(1f, 100);
            oldElement.CenterColor = oldElement.IdleCenterColor;
            oldElement.VertexColor = oldElement.IdleBorderColor;
        }

        internal static void Sel(RadialSliceBase newElement, float scale = 0.10f)
        {
            newElement.Selected = true;
            newElement.DataHolder.experimental.animation.Scale(1f + scale, 100);
            newElement.CenterColor = newElement.SelectedCenterColor;
            newElement.VertexColor = newElement.SelectedBorderColor;
        }

        /*
         * Static
         */

        private static float GetAngle(Vector2 mouse) => -Mathf.Atan2(mouse.x, mouse.y) * 180f / Mathf.PI + 180f;

        private static bool Get2Axis(Vector2 mouse, float range, out Vector2 axis)
        {
            axis = Vector2.zero;
            if (Event.current.type == EventType.Layout) return false;

            axis = Vector2.ClampMagnitude(new Vector2(mouse.x / range, -mouse.y / range), 1);
            return true;
        }

        private static bool Get4Axis(Vector2 mouse, float range, out Vector4 axis)
        {
            axis = Vector4.zero;
            if (!Get2Axis(mouse, range, out var aVectorAxis)) return false;

            axis.x = Mathf.Max(aVectorAxis.y, 0);
            axis.y = Mathf.Max(aVectorAxis.x, 0);
            axis.z = Mathf.Max(-aVectorAxis.y, 0);
            axis.w = Mathf.Max(-aVectorAxis.x, 0);

            return true;
        }

        private static bool GetRadial(Vector2 mouse, float min, out Range range)
        {
            range = Range.M1;
            if (Event.current.type == EventType.Layout || mouse.magnitude < min) return false;

            range = new Range(GetAngle(mouse) / 360f);
            return true;
        }

        private static int GetChoice(Vector2 mouse, float min, float max, int elements)
        {
            if (mouse.magnitude < min || mouse.magnitude > max) return -1;
            return (int)((GetAngle(mouse) + 180f / elements) % 360 / (360f / elements));
        }

        /*
         * Listeners
         */

        [PublicAPI] public int GetChoice(int elements, bool puppet) => puppet ? -1 : GetChoice(_position, _min, _max, elements);

        internal bool GetRadial(float min, out Range range) => GetRadial(_position, min, out range);

        internal bool Get2Axis(float range, out Vector2 axis) => Get2Axis(_position, range, out axis);

        internal bool Get4Axis(float range, out Vector4 axis) => Get4Axis(_position, range, out axis);

        /*
         * Cursor Position
         */

        private void SetCursorPosition(float x, float y) => SetCursorPosition(new Vector2(x, y));

        private void SetCursorPosition(Vector2 position)
        {
            position = Vector2.ClampMagnitude(position, _clamp);
            style.left = position.x;
            style.top = position.y;
            _position = new Vector2(style.left.value.value, style.top.value.value);
        }
    }
}
#endif