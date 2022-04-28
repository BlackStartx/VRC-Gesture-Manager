#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets
{
    public class FourAxisPuppet : BaseAxisPuppet
    {
        public FourAxisPuppet(RadialMenuControl control) : base(control)
        {
        }

        public override void Update(RadialCursor cursor)
        {
            if (!cursor.Get4Axis(Clamp, out var axis)) return;

            Control.SetSubValue(0, axis.x);
            Control.SetSubValue(1, axis.y);
            Control.SetSubValue(2, axis.z);
            Control.SetSubValue(3, axis.w);
        }
    }
}
#endif