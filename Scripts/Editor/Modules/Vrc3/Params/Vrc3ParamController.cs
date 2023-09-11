#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Params
{
    public class Vrc3ParamController : Vrc3Param
    {
        private AnimatorControllerPlayable _controller;

        public Vrc3ParamController(AnimatorControllerParameterType type, string name, AnimatorControllerPlayable controller) : base(name, type)
        {
            _controller = controller;
            Subscribe(_controller);
        }

        [Obsolete] public override float Get() => FloatValue();

        public override float FloatValue()
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
                    foreach (var controller in Playables) controller.SetFloat(HashId, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    foreach (var controller in Playables) controller.SetInteger(HashId, (int)value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                case AnimatorControllerParameterType.Bool:
                    foreach (var controller in Playables) controller.SetBool(HashId, value > 0.5f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif