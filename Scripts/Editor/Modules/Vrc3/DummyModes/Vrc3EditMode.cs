#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Editor.Library;
using UnityEditor;
using UnityEditor.Animations;
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

        private readonly Dictionary<Motion, Motion> _convert = new();

        private AnimationWindow _animation;
        private AnimationWindow Animation => _animation ? _animation : _animation = EditorWindow.GetWindow<AnimationWindow>();

        private RadialDescription _description;
        private RadialDescription Description => _description ??= new RadialDescription(Text, Link, Tail, SelectAvatarAction);

        private readonly AnimatorState _state;

        internal Vrc3EditMode(ModuleVrc3 module, IEnumerable<MotionItem> motions) : base(module, Prefix)
        {
            var controller = GmgAnimatorControllerHelper.CreateController();
            _state = GmgAnimatorControllerHelper.AddMotionWith(controller, motions.Select(Create));
            Animator.runtimeAnimatorController = controller;
        }

        private Motion Create(MotionItem motion)
        {
            var clip = new AnimationClip { name = motion.FullName };
            _convert[clip] = motion.Motion;
            return clip;
        }

        public override RadialDescription DummyDescription() => Description;

        protected override void OnUpdate()
        {
            var clip = Animation?.animationClip;
            if (!clip || !_convert.TryGetValue(clip, out var motion)) return;
            if (clip != motion) Animation.animationClip = (AnimationClip)(_state.motion = motion);
        }

        private void SelectAvatarAction(string obj)
        {
            if (Avatar == null) return;

            Selection.activeGameObject = Avatar;
            EditorWindow.GetWindow<AnimationWindow>().Focus();
        }
    }
}
#endif