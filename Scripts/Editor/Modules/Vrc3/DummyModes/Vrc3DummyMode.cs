#if VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.DummyModes
{
    public abstract class Vrc3DummyMode
    {
        protected readonly ModuleVrc3 Module;

        internal abstract string ModeName { get; }

        internal readonly Animator Animator;
        internal readonly GameObject Avatar;

        public string ExitDummyText => $"Exit {ModeName}-Mode";

        protected Vrc3DummyMode(ModuleVrc3 module, string prefix, bool keepPose = false)
        {
            if (keepPose && module.DummyMode != null) Avatar = Object.Instantiate(module.DummyMode.Avatar);
            module.DummyMode?.StopExecution();
            module.DummyMode = this;
            Module = module;
            Module.AvatarTools.PoseAvatar.Disable(Module);
            if (!keepPose) Module.ForgetAvatar();
            if (!Avatar) Avatar = Object.Instantiate(Module.Avatar);
            if (keepPose) Module.ForgetAvatar();
            Animator = VRC.Core.ExtensionMethods.GetOrAddComponent<Animator>(Avatar);
            Avatar.name = $"{Module.Avatar.name} {prefix}";
            Module.Avatar.SetActive(false);
            Module.Avatar.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            EditorApplication.DirtyHierarchyWindowSorting();
            foreach (var radialMenu in module.Radials) radialMenu.MainMenuPrefab();
        }

        internal void Update(GameObject avatar)
        {
            if (!Avatar || avatar.activeSelf) StopExecution();
            else OnUpdate();
        }

        protected virtual void OnUpdate()
        {
        }

        internal void Close()
        {
            if (Avatar) Object.DestroyImmediate(Avatar);
            Module.DummyMode = null;
            if (!Module.Avatar) return;
            Module.Avatar.SetActive(true);
            Module.Avatar.hideFlags = HideFlags.None;
            EditorApplication.DirtyHierarchyWindowSorting();
            Module.AvatarAnimator.Update(1f);
            Module.AvatarAnimator.runtimeAnimatorController = null;
            Module.AvatarAnimator.Update(1f);
            Module.InitForAvatar();
        }

        protected internal virtual void StopExecution() => Close();

        public abstract RadialDescription DummyDescription();
    }
}
#endif