#if VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes
{
    public abstract class Vrc3DummyMode
    {
        protected readonly ModuleVrc3 Module;

        internal abstract string ModeName { get; }

        internal readonly GameObject Avatar;

        public string ExitDummyText => "Exit " + ModeName + "-Mode";

        protected Vrc3DummyMode(ModuleVrc3 module, string prefix)
        {
            Module = module;
            Module.ForgetAvatar();
            Module.Dummy.State = true;
            Avatar = Object.Instantiate(Module.Avatar);
            Avatar.name = Module.Avatar.name + " " + prefix;
            Module.Avatar.SetActive(false);
            Module.Avatar.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        internal void Close()
        {
            if (Avatar) Object.DestroyImmediate(Avatar);
            Module.Dummy.State = false;
            Module.DummyMode = null;
            Module.Avatar.SetActive(true);
            Module.Avatar.hideFlags = HideFlags.None;
            EditorApplication.DirtyHierarchyWindowSorting();
            Module.AvatarAnimator.Update(1f);
            Module.AvatarAnimator.runtimeAnimatorController = null;
            Module.AvatarAnimator.Update(1f);
            Module.InitForAvatar();
        }

        public void OnExecutionChange(bool state)
        {
            if (state || !Module.Avatar) return;
            OnExecutionOff();
        }

        protected virtual void OnExecutionOff() => Close();

        public abstract RadialDescription DummyDescription();
    }
}
#endif