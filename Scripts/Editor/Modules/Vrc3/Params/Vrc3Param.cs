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

        private Action<Vrc3Param, float> _onChange;
        private float _amplifier;

        protected Vrc3Param(string name, AnimatorControllerParameterType type, float amplifier = 1f)
        {
            Name = name;
            Type = type;
            HashId = Animator.StringToHash(Name);
            _amplifier = amplifier;
        }

        public void Set(IEnumerable<RadialMenu> menus, float value)
        {
            value *= _amplifier;
            if (Is(value)) return;
            InternalSet(value);
            _onChange?.Invoke(this, value);
            foreach (var menu in menus) menu.UpdateValue(Name, value);
        }

        public void Amplify(float amplifier) => _amplifier = amplifier;

        public void OnChange(Action<Vrc3Param, float> onChange) => _onChange = onChange;

        private bool Is(float value) => RadialMenuUtility.Is(Get(), value);

        public abstract float Get();

        protected internal abstract void InternalSet(float value);

        public void Add(IEnumerable<RadialMenu> menu, float value) => Set(menu, Get() + value);

        public void Random(IEnumerable<RadialMenu> menu, float min, float max, float chance)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Int:
                    Set(menu, UnityEngine.Random.Range((int) min, (int) max + 1));
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
    }
}
#endif