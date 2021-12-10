#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Params
{
    public abstract class Vrc3Param
    {
        public readonly AnimatorControllerParameterType Type;
        protected readonly int HashId;
        public readonly string Name;

        internal Action<Vrc3Param, float> OnChange;

        private float _amplifier;
        public float NotAmplified => Get() / _amplifier;

        protected Vrc3Param(string name, AnimatorControllerParameterType type, float amplifier = 1f)
        {
            Name = name;
            Type = type;
            HashId = Animator.StringToHash(Name);
            _amplifier = amplifier;
        }

        public void Set(IEnumerable<RadialMenu> menus, float value)
        {
            var amplified = value * _amplifier;
            if (Is(amplified)) return;
            InternalSet(amplified);
            OnChange?.Invoke(this, amplified);
            foreach (var menu in menus) menu.UpdateValue(Name, amplified, value);
        }

        public void Amplify(float amplifier) => _amplifier = amplifier;

        public void SetOnChange(Action<Vrc3Param, float> onChange) => OnChange = onChange;

        private bool Is(float value) => RadialMenuUtility.Is(Get(), value);

        public abstract float Get();

        protected internal abstract void InternalSet(float value);

        public void Add(IEnumerable<RadialMenu> menu, float value) => Set(menu, Get() + value);

        public void Random(IEnumerable<RadialMenu> menu, float min, float max, float chance)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Int:
                    Set(menu, UnityEngine.Random.Range((int)min, (int)max + 1));
                    break;
                case AnimatorControllerParameterType.Float:
                    Set(menu, UnityEngine.Random.Range(min, max));
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    Set(menu, UnityEngine.Random.Range(0.0f, 1.0f) < chance ? 1f : 0f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public (Color? color, string text) LabelTuple()
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    return (null, Get().ToString("0.00"));
                case AnimatorControllerParameterType.Int:
                    return (null, ((int)Get()).ToString());
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return Get() < 0.5f ? (Color.red, "False") : (Color.green, "True");
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif