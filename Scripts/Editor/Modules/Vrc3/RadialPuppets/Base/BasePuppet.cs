#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base
{
    public abstract class BasePuppet : GmgCircleElement
    {
        internal readonly RadialMenuControl Control;
        internal abstract float Clamp { get; }

        protected BasePuppet(float size, RadialMenuControl control)
        {
            Control = control;
            RadialMenuUtility.Prefabs.SetCircle(this, size, RadialMenuUtility.Colors.CustomMain, RadialMenuUtility.Colors.CustomBorder);
        }

        public void OnOpen() => Control.SetControlValue();

        public void OnClose() => Control.SetValue(0);

        public abstract void UpdateValue(string pName, float value);

        public abstract void Update(RadialCursor cursor);

        public abstract void AfterCursor();
    }
}
#endif