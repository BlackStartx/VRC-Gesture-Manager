#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements
{
    public class VisualEpContainer : Vrc3VisualRender
    {
        private readonly Dictionary<EndpointControl, VisualElement> _elements = new();

        public VisualEpContainer() => style.position = UIEPosition.Absolute;

        protected override bool RenderCondition(ModuleVrc3 module, RadialMenu menu)
        {
            return menu.ToolBar.Selected == 2 && menu.DebugToolBar.Selected == 1 && module.OscModule.Enabled && !module.DebugOscWindow && module.OscModule.ToolBar.Selected == 0;
        }

        public void Render(VisualElement root) => Render(root, new Rect(0, style.top.value.value, 0, 0));

        public void RenderMessage(Rect rect, EndpointControl endpoint)
        {
            if (!_elements.TryGetValue(endpoint, out var messageElement)) messageElement = _elements[endpoint] = endpoint.New();
            if (messageElement.parent != this) Add(messageElement);
            messageElement.style.height = rect.height;
            messageElement.style.width = rect.width;
            messageElement.style.left = rect.x;
            messageElement.style.top = rect.y;
        }
    }
}
#endif