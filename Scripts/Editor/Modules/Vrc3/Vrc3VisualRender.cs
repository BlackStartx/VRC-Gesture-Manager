#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public abstract class Vrc3VisualRender : VisualElement
    {
        private bool _rendering;
        internal Rect Rect;

        private int _lifeTick;

        internal bool Xin(Vector2 p) => Rect.xMin < p.x && Rect.xMax > p.x;

        public virtual void Render(VisualElement root, Rect rect)
        {
            if (Event.current.type != EventType.Layout && !root.Contains(this)) root.Add(this);
            _rendering = true;
            _lifeTick = 2;

            style.height = rect.height;
            style.width = rect.width;
            style.positionLeft = rect.x;
            style.positionTop = rect.y;
        }

        protected override void DoRepaint(IStylePainter painter)
        {
            if (_lifeTick > 0) _lifeTick--;
            if (_lifeTick == 0) parent?.Clear();
        }

        internal void StopRendering()
        {
            if (!_rendering) return;
            _rendering = false;
            parent?.Remove(this);
        }

        protected abstract bool RenderCondition(int selectedIndex);

        public void CheckCondition(int selectedIndex)
        {
            if (!_rendering) return;
            if (!RenderCondition(selectedIndex)) StopRendering();
        }
    }
}
#endif