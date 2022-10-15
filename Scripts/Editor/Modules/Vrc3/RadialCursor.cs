#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Core.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
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

        [Obsolete]
        public void Update(Vector2 mouse, IEnumerable<GmgButton> selectionTuple, bool puppet) => Update(mouse, selectionTuple.Select(Element).ToList(), puppet);

        public void Update(Vector2 mouse, IList<RadialMenuItem> selectionTuple, bool puppet)
        {
            if (mouse.magnitude < _clampReset) SetCursorPosition(mouse);
            else SetCursorPosition(0, 0);
            if (selectionTuple == null) return;
            var intSelection = GetChoice(selectionTuple.Count, puppet);
            if (Selection != intSelection) UpdateSelection(selectionTuple, intSelection);
        }

        [Obsolete]
        private void UpdateSelection(IEnumerable<GmgButton> selectionTuple, int selection) => UpdateSelection(selectionTuple.Select(Element).ToList(), selection);

        private void UpdateSelection(IList<RadialMenuItem> selectionTuple, int selection)
        {
            if (Selection != -1) Des(selectionTuple[Selection]);
            if (selection != -1) Sel(selectionTuple[selection]);
            Selection = selection;
        }

        [Obsolete]
        private static void Des(GmgButton oldElement) => Des(Element(oldElement));

        private static void Des(RadialMenuItem oldElement)
        {
            oldElement.Selected = false;
            oldElement.DataHolder.experimental.animation.Scale(1f, 100);
            oldElement.DataHolder.experimental.animation.TopLeft(RadialMenuUtility.DataVector, 100);
            oldElement.CircleElement.CenterColor = RadialMenuUtility.Colors.RadialCenter;
            oldElement.CircleElement.VertexColor = RadialMenuUtility.Colors.CustomMain;
        }

        [Obsolete]
        internal static void Sel(GmgButton newElement, bool instant = false, float scale = 0.10f) => Sel(Element(newElement), instant, scale);

        internal static void Sel(RadialMenuItem newElement, bool instant = false, float scale = 0.10f)
        {
            newElement.Selected = true;
            var topLeftVector = RadialMenuUtility.DataVector + RadialMenuUtility.DataVector * scale;
            newElement.DataHolder.experimental.animation.Scale(1f + scale, 100);
            newElement.DataHolder.experimental.animation.TopLeft(topLeftVector, instant ? 0 : 100);
            newElement.CircleElement.CenterColor = newElement.SelectedCenterColor;
            newElement.CircleElement.VertexColor = newElement.SelectedBorderColor;
        }

        [Obsolete]
        private static RadialMenuItem Element(GmgButton element)
        {
            element.Button.CircleElement = element.CircleElement;
            return element.Button;
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

        private static bool GetRadial(Vector2 mouse, float min, out float radial)
        {
            radial = -1;
            if (Event.current.type == EventType.Layout || mouse.magnitude < min) return false;

            radial = GetAngle(mouse) / 360f;
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

        internal int GetChoice(int elements, bool puppet) => puppet ? -1 : GetChoice(_position, _min, _max, elements);

        internal bool GetRadial(float min, out float radial) => GetRadial(_position, min, out radial);

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