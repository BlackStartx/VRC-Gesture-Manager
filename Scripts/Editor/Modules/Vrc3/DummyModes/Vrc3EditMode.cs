#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using GestureManager.Scripts.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3EditMode : Vrc3DummyMode
    {
        public static void Enable(ModuleVrc3 module, IEnumerable<AnimationClip> originalClips)
        {
            module.DummyMode = new Vrc3EditMode(module, originalClips);
            foreach (var radialMenu in module.Radials) radialMenu.MainMenuPrefab();
        }

        internal override string ModeName => "Edit";

        private Vrc3EditMode(ModuleVrc3 module, IEnumerable<AnimationClip> clips) : base(module, "[Edit-Mode]")
        {
            Avatar.GetOrAddComponent<Animator>().runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(clips);
        }

        public override RadialDescription DummyDescription() => new RadialDescription("You're in Edit-Mode,", "select your avatar", "to directly edit your animations!", SelectAvatarAction, null);

        private void SelectAvatarAction(string obj)
        {
            if (Avatar == null) return;

            Selection.activeGameObject = Avatar;
            // Unity is too shy and hide too much stuff in his internal scope...
            // this is a sad and fragile work around for opening the Animation Window.
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
        }
    }
}
#endif