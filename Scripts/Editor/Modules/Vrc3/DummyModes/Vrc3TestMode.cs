﻿#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Lib;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3TestMode : Vrc3DummyMode
    {
        internal override string ModeName => "Test";

        internal Vrc3TestMode(ModuleVrc3 module) : base(module, "[Testing]")
        {
        }

        protected internal override void StopExecution() => Module.Manager.StopCustomAnimation();

        public override RadialDescription DummyDescription() => null;

        public static Animator Disable(Vrc3DummyMode dummyMode)
        {
            dummyMode.Close();
            return null;
        }

        public Animator Test(AnimationClip clip)
        {
            Animator.runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(clip);
            Animator.applyRootMotion = clip;
            return Animator;
        }
    }
}
#endif