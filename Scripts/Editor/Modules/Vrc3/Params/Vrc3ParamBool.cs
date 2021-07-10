#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Params
{
    public class Vrc3ParamBool : Vrc3Param
    {
        public bool State;

        public Vrc3ParamBool(Action<bool> onChange = null) : base(null, AnimatorControllerParameterType.Bool)
        {
            if (onChange != null) OnChange((param, f) => onChange(f > 0.5f));
        }

        public override float Get()
        {
            return State ? 1f : 0f;
        }

        protected internal override void InternalSet(float value)
        {
            State = value > 0.5f;
        }
    }
}
#endif