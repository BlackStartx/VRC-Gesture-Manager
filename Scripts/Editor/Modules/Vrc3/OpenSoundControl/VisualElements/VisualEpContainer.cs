﻿#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl.VisualElements
{
    public class VisualEpContainer : Vrc3VisualRender
    {
        private readonly Dictionary<EndpointControl, VisualElement> _elements = new Dictionary<EndpointControl, VisualElement>();

        public VisualEpContainer() => style.positionType = PositionType.Absolute;

        protected override bool RenderCondition(ModuleVrc3 module, RadialMenu menu)
        {
            return menu.ToolBar.Selected == 2 && menu.DebugToolBar.Selected == 1 && module.OscModule.Enabled && !module.DebugOscWindow && module.OscModule.ToolBar.Selected == 0;
        }

        public void Render(VisualElement root) => Render(root, new Rect(0, style.positionTop.value, 0, 0));

        public void RenderMessage(Rect rect, EndpointControl endpoint)
        {
            if (!_elements.TryGetValue(endpoint, out var messageElement)) messageElement = _elements[endpoint] = endpoint.New();
            if (messageElement.parent != this) Add(messageElement);
            messageElement.style.height = rect.height;
            messageElement.style.width = rect.width;
            messageElement.style.positionLeft = rect.x;
            messageElement.style.positionTop = rect.y;
        }
    }
}
#endif