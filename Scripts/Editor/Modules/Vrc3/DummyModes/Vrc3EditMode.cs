#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using BlackStartX.GestureManager.Editor.Library;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.DummyModes
{
    public class Vrc3EditMode : Vrc3DummyMode
    {
        internal override string ModeName => "Edit";

        private const string Prefix = "[Edit-Mode]";

        private const string Text = "You're in Edit-Mode, ";
        private const string Link = "select your avatar";
        private const string Tail = " to directly edit your animations!";

        private RadialDescription _description;
        private RadialDescription Description => _description ?? (_description = new RadialDescription(Text, Link, Tail, SelectAvatarAction));

        private static RuntimeAnimatorController Controller(IEnumerable<AnimationClip> clips) => GmgAnimatorControllerHelper.CreateControllerWith(clips);

        internal Vrc3EditMode(ModuleVrc3 module, IEnumerable<AnimationClip> clips) : base(module, Prefix) => Animator.runtimeAnimatorController = Controller(clips);

        public override RadialDescription DummyDescription() => Description;

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