#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Library;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3TestMode : Vrc3DummyMode
    {
        internal override string ModeName => "Test";

        internal Vrc3TestMode(ModuleVrc3 module) : base(module, "[Testing]")
        {
        }

        protected internal override void StopExecution() => Module.StopCustomAnimation();

        public override RadialDescription DummyDescription() => null;

        public static Animator Disable(Vrc3DummyMode dummyMode)
        {
            dummyMode.Close();
            return null;
        }

        public Animator Test(Motion motion)
        {
            Animator.runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(motion);
            Animator.applyRootMotion = motion;
            return Animator;
        }
    }
}
#endif