#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3TestMode : Vrc3DummyMode
    {
        public static Vrc3TestMode Enable(ModuleVrc3 module)
        {
            module.DummyMode = new Vrc3TestMode(module);
            foreach (var radialMenu in module.Radials) radialMenu.MainMenuPrefab();
            return module.DummyMode as Vrc3TestMode;
        }

        public static Animator Disable(Vrc3DummyMode dummyMode)
        {
            dummyMode.Close();
            return null;
        }

        internal override string ModeName => "Test";

        private Vrc3TestMode(ModuleVrc3 module) : base(module, "[Testing]")
        {
        }

        protected override void OnExecutionOff() => Module.Manager.StopCustomAnimation();

        public override RadialDescription DummyDescription() => null;

        public Animator Test(AnimationClip clip)
        {
            var animator = Avatar.GetOrAddComponent<Animator>();
            animator.runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(clip);
            return animator;
        }
    }
}
#endif