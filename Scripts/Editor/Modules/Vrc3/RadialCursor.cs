#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Core.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialCursor : GmgCircleElement
    {
        private const int CursorSize = 50;

        private float _clampReset;
        private float _clamp;
        private float _min;
        private float _max;

        private Vector2 _position;
        internal int Selection = -1;

        public RadialCursor()
        {
            RadialMenuUtility.Prefabs.SetCircle(this, CursorSize, RadialMenuUtility.Colors.Cursor, RadialMenuUtility.Colors.CursorBorder)
                .MyAdd(RadialMenuUtility.Prefabs.NewCircleBorder((int)(CursorSize / 1.5f), RadialMenuUtility.Colors.CursorBorder))
                .Add(RadialMenuUtility.Prefabs.NewCircleBorder((int)(CursorSize / 4f), RadialMenuUtility.Colors.CursorBorder));
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

        public void Update(Vector2 mouse, IList<GmgButton> selectionTuple, bool puppet)
        {
            if (mouse.magnitude < _clampReset) SetCursorPosition(mouse);
            else SetCursorPosition(0, 0);
            if (selectionTuple == null) return;
            var intSelection = GetChoice(selectionTuple.Count, puppet);
            if (Selection != intSelection) UpdateSelection(selectionTuple, intSelection);
        }

        private void UpdateSelection(IList<GmgButton> selectionTuple, int selection)
        {
            if (Selection != -1) Des(selectionTuple[Selection]);
            if (selection != -1) Sel(selectionTuple[selection]);
            Selection = selection;
        }

        private static void Des(GmgButton oldElement)
        {
            oldElement.Data.experimental.animation.Scale(1f, 100);
            oldElement.Data.experimental.animation.TopLeft(RadialMenuUtility.DataVector, 100);
            oldElement.CircleElement.CenterColor = RadialMenuUtility.Colors.RadialCenter;
            oldElement.CircleElement.VertexColor = RadialMenuUtility.Colors.CustomMain;
        }

        internal static void Sel(GmgButton newElement, bool instant = false, float scale = 0.10f)
        {
            var topLeftVector = RadialMenuUtility.DataVector + RadialMenuUtility.DataVector * scale;

            newElement.Data.experimental.animation.Scale(1f + scale, 100);
            newElement.Data.experimental.animation.TopLeft(topLeftVector, instant ? 0 : 100);
            newElement.CircleElement.CenterColor = newElement.Button.SelectedCenterColor;
            newElement.CircleElement.VertexColor = newElement.Button.SelectedBorderColor;
        }

        /*
         * Static
         */

        private static float GetAngle(Vector2 mouse)
        {
            return -Mathf.Atan2(mouse.x, mouse.y) * 180f / Mathf.PI + 180f;
        }

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
            if (!Get2Axis(mouse, range, out var twoAxis)) return false;

            axis.x = Mathf.Max(twoAxis.y, 0);
            axis.y = Mathf.Max(twoAxis.x, 0);
            axis.z = Mathf.Max(-twoAxis.y, 0);
            axis.w = Mathf.Max(-twoAxis.x, 0);

            return true;
        }

        private static bool GetRadial(Vector2 mouse, float min, out float radial)
        {
            radial = -1;
            if (Event.current.type == EventType.Layout) return false;

            if (mouse.magnitude < min) return false;

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