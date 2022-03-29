#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base
{
    public abstract class BasePuppet : GmgCircleElement
    {
        internal readonly RadialMenuControl Control;

        protected BasePuppet(float size, RadialMenuControl control)
        {
            Control = control;
            RadialMenuUtility.Prefabs.SetCircle(this, size, RadialMenuUtility.Colors.RadialMiddle, RadialMenuUtility.Colors.OuterBorder);
        }

        public void OnOpen()
        {
            Control.SetControlValue();
        }

        public void OnClose()
        {
            Control.SetValue(0);
        }

        public abstract void UpdateValue(string pName, float value);

        public abstract void Update(Vector2 mouse, RadialCursor cursor);

        public abstract void AfterCursor();
    }
}
#endif