#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Params
{
    public class Vrc3ParamBool : Vrc3Param
    {
        private bool _state;

        public Vrc3ParamBool(Action<bool> onChange = null) : base(null, AnimatorControllerParameterType.Bool)
        {
            if (onChange != null) SetOnChange((param, f) => onChange(f > 0.5f));
        }

        [Obsolete] public override float Get() => FloatValue();

        public override float FloatValue() => _state ? 1f : 0f;

        protected internal override void InternalSet(float value) => _state = value > 0.5f;
    }
}
#endif