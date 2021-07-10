#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Params
{
    public class Vrc3ParamController : Vrc3Param
    {
        private AnimatorControllerPlayable _controller;

        private readonly Animator _animator;

        public Vrc3ParamController(AnimatorControllerParameterType type, string name, AnimatorControllerPlayable controller, Animator animator) : base(name, type)
        {
            _animator = animator;
            _controller = controller;
        }

        public override float Get()
        {
            if (!_controller.IsValid()) return 0;
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    return _controller.GetFloat(Name);
                case AnimatorControllerParameterType.Int:
                    return _controller.GetInteger(Name);
                case AnimatorControllerParameterType.Trigger:
                case AnimatorControllerParameterType.Bool:
                    return _controller.GetBool(Name) ? 1f : 0f;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected internal override void InternalSet(float value)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    _animator.SetFloat(HashId, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    _animator.SetInteger(HashId, (int) value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                case AnimatorControllerParameterType.Bool:
                    _animator.SetBool(HashId, value != 0f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif