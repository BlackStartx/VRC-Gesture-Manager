#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets.Base;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets
{
    public class FourAxisPuppet : BaseAxisPuppet
    {
        public FourAxisPuppet(RadialSliceControl control) : base(control)
        {
        }

        public override void Update(RadialCursor cursor)
        {
            if (!cursor.Get4Axis(Clamp, out var vector4)) return;

            Control.SetSubValue(0, vector4.x);
            Control.SetSubValue(1, vector4.y);
            Control.SetSubValue(2, vector4.z);
            Control.SetSubValue(3, vector4.w);
        }
    }
}
#endif