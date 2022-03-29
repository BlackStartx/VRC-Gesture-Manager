#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Params
{
    public abstract class Vrc3Param
    {
        public readonly AnimatorControllerParameterType Type;
        protected readonly int HashId;
        public readonly string Name;

        internal Action<Vrc3Param, float> OnChange;

        protected Vrc3Param(string name, AnimatorControllerParameterType type)
        {
            Name = name;
            Type = type;
            HashId = Animator.StringToHash(Name);
        }

        public void Set(ModuleVrc3 module, float value)
        {
            if (Is(value)) return;
            module.OscModule.OnParameterChange(this, value);
            InternalSet(value);
            OnChange?.Invoke(this, value);
            foreach (var menu in module.RadialMenus.Values) menu.UpdateValue(Name, value);
        }

        private void Set(ModuleVrc3 module, bool value) => Set(module, value ? 1f : 0f);

        public void Set(ModuleVrc3 module, bool? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void Set(ModuleVrc3 module, float? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void SetOnChange(Action<Vrc3Param, float> onChange) => OnChange = onChange;

        private bool Is(float value) => RadialMenuUtility.Is(Get(), value);

        public abstract float Get();

        protected internal abstract void InternalSet(float value);

        public void Add(ModuleVrc3 module, float value) => Set(module, Get() + value);

        public void Random(ModuleVrc3 module, float min, float max, float chance)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    Set(module, UnityEngine.Random.Range(min, max));
                    break;
                case AnimatorControllerParameterType.Int:
                    Set(module, UnityEngine.Random.Range((int)min, (int)max + 1));
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    Set(module, UnityEngine.Random.Range(0.0f, 1.0f) < chance);
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