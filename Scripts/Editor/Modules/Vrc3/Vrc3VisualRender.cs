﻿#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public abstract class Vrc3VisualRender : VisualElement
    {
        private bool _rendering;
        internal Rect Rect;

        public virtual void Render(VisualElement root, Rect rect)
        {
            if (Event.current.type != EventType.Layout && !root.Contains(this)) root.Add(this);
            _rendering = true;

            style.height = rect.height;
            style.width = rect.width;
            style.left = rect.x;
            style.top = rect.y;
        }

        internal void StopRendering()
        {
            if (!_rendering) return;
            _rendering = false;
            parent?.Remove(this);
        }

        protected abstract bool RenderCondition(ModuleVrc3 module, RadialMenu menu);

        public void CheckCondition(ModuleVrc3 module, RadialMenu menu)
        {
            if (!_rendering) return;
            if (!RenderCondition(module, menu)) StopRendering();
        }
    }
}
#endif