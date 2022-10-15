#if VRC_SDK_VRCSDK3
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3PoseMode : Vrc3DummyMode
    {
        internal override string ModeName => "Pose";

        internal Vrc3PoseMode(ModuleVrc3 module) : base(module, "[Posing]", keepPose: true) => Object.Destroy(Animator);

        public override RadialDescription DummyDescription() => null;
    }
}
#endif