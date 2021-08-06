#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Core.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialCursor : GmgCircleElement
    {
        private float _clamp;
        private float _clampReset;
        private float _min;
        private float _max;

        public RadialCursor(int size)
        {
            RadialMenuUtility.Prefabs.SetCircle(this, size, RadialMenuUtility.Colors.Cursor, RadialMenuUtility.Colors.CursorBorder)
                .MyAdd(RadialMenuUtility.Prefabs.NewCircle((int) (size / 1.5f), RadialMenuUtility.Colors.Cursor, RadialMenuUtility.Colors.CursorBorder))
                .Add(RadialMenuUtility.Prefabs.NewCircle((int) (size / 4f), RadialMenuUtility.Colors.Cursor, RadialMenuUtility.Colors.CursorBorder));
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

        public void Update(Vector2 mouse)
        {
            if (mouse.magnitude < _clampReset) SetCursorPosition(mouse);
            else SetCursorPosition(0, 0);
        }

        private float GetAngle() => GetAngle(new Vector2(style.left.value.value, style.top.value.value));

        private static float GetAngle(Vector2 vector)
        {
            return -Mathf.Atan2(vector.x, vector.y) * 180f / Mathf.PI + 180f;
        }

        /*
         * Listeners
         */

        internal bool GetRadial(Vector2 mouse, out float radial)
        {
            radial = -1;
            if (Event.current.type == EventType.Layout) return false;

            if (mouse.magnitude < _min) return false;

            radial = GetAngle(mouse) / 360f;
            return true;
        }

        internal bool Get2Axis(Vector2 mouse, float range, out Vector2 axis)
        {
            axis = Vector2.zero;
            if (Event.current.type == EventType.Layout) return false;

            axis = Vector2.ClampMagnitude(new Vector2(mouse.x / range, -mouse.y / range), 1);
            return true;
        }

        internal bool Get4Axis(Vector2 mouse, float range, out Vector4 axis)
        {
            axis = Vector4.zero;
            if (!Get2Axis(mouse, range, out var twoAxis)) return false;

            axis.x = Mathf.Max(twoAxis.y, 0);
            axis.y = Mathf.Max(twoAxis.x, 0);
            axis.z = Mathf.Max(-twoAxis.y, 0);
            axis.w = Mathf.Max(-twoAxis.x, 0);

            return true;
        }

        internal int GetChoice(Vector2 mouse, VisualElement borderHolder)
        {
            mouse -= parent.worldBound.center;
            if (mouse.magnitude < _min || mouse.magnitude > _max) return -1;

            var angle = GetAngle();
            var counter = 0;
            foreach (var borderElement in borderHolder.Children())
            {
                var borderAngle = (borderElement.transform.rotation.eulerAngles.z + 90) % 360;
                if (angle < borderAngle) return counter;
                counter++;
            }

            return 0;
        }

        /*
         * Cursor Position
         */

        private void SetCursorPosition(float x, float y) => SetCursorPosition(new Vector2(x, y));

        private void SetCursorPosition(Vector2 position)
        {
            position = Vector2.ClampMagnitude(position, _clamp);

            style.left = position.x;
            style.top = position.y;
        }
    }
}
#endif