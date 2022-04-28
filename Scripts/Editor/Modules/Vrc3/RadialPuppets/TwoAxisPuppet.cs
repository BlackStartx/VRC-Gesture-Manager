#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets
{
    public class TwoAxisPuppet : BaseAxisPuppet
    {
        public TwoAxisPuppet(RadialMenuControl control) : base(control)
        {
        }

        public override void Update(RadialCursor cursor)
        {
            if (!cursor.Get2Axis(Clamp, out var axis)) return;

            Control.SetSubValue(0, axis.x);
            Control.SetSubValue(1, axis.y);
        }
    }
}
#endif