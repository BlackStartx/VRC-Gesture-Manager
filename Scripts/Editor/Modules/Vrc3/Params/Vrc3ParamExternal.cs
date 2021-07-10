#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Params
{
    public class Vrc3ParamExternal : Vrc3Param
    {
        private float _value;

        public Vrc3ParamExternal(string name, AnimatorControllerParameterType type) : base(name, type)
        {
        }

        public override float Get()
        {
            return _value;
        }

        protected internal override void InternalSet(float value)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    _value = value;
                    break;
                case AnimatorControllerParameterType.Int:
                    _value = (int) value;
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    _value = value > 0.5f ? 1f : 0f;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif