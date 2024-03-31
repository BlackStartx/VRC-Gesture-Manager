#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Params
{
    public class Vrc3Param
    {
        private readonly Dictionary<AnimatorControllerPlayable, int> _controllers = new();

        public readonly AnimatorControllerParameterType Type;
        private readonly int _hashId;
        public readonly string Name;
        public int LastUpdate;

        private float _value;
        private Action<Vrc3Param, float> _onChange;

        public Vrc3Param(string name, AnimatorControllerParameterType type, Action<Vrc3Param, float> onChange = null)
        {
            Name = name;
            Type = type;
            LastUpdate = Time;
            _onChange = onChange;
            _hashId = Animator.StringToHash(Name);
        }

        public void Set(ModuleVrc3 module, float value)
        {
            if (RadialMenuUtility.Is(FloatValue(), value)) return;
            module.OscModule.OnParameterChange(this, value);
            LastUpdate = Time;
            InternalSet(value);
            _onChange?.Invoke(this, value);
            foreach (var menu in module.Radials) menu.UpdateValue(Name, value);
        }

        public string TypeText => Type.ToString();

        public static int Time => (int)(DateTime.Now.Ticks / 100000L);

        public void Subscribe(AnimatorControllerPlayable playable, int index) => _controllers[playable] = index;

        public void Set(ModuleVrc3 module, bool value) => Set(module, value ? 1f : 0f);

        public void Set(ModuleVrc3 module, int value) => Set(module, (float)value);

        public void Set(ModuleVrc3 module, float? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void Set(ModuleVrc3 module, bool? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void SetOnChange(Action<Vrc3Param, float> onChange) => _onChange = onChange;

        [Obsolete("This method be removed on 3.9, override FloatValue for now on. Kept for compilation compatibility.")]
        public virtual float Get() => FloatValue();

        public virtual float FloatValue() => _value;

        public int IntValue() => (int)FloatValue();

        public bool BoolValue() => FloatValue() != 0f;

        protected internal virtual void InternalSet(float value)
        {
            _value = value;
            foreach (var pair in _controllers.Where(pair => ModuleVrc3.IsValid(pair.Key)))
            {
                switch (pair.Key.GetParameter(pair.Value).type)
                {
                    case AnimatorControllerParameterType.Float:
                        pair.Key.SetFloat(_hashId, value);
                        break;
                    case AnimatorControllerParameterType.Int:
                        pair.Key.SetInteger(_hashId, (int)Math.Round(value));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                    case AnimatorControllerParameterType.Bool:
                        pair.Key.SetBool(_hashId, value != 0f);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Add(ModuleVrc3 module, float value) => Set(module, FloatValue() + value);

        public void Copy(ModuleVrc3 module, float floatValue, bool range, float sourceMin, float sourceMax, float destMin, float destMax)
        {
            if (range) floatValue = RangeOf(floatValue, sourceMin, sourceMax, destMin, destMax);
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    Set(module, floatValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    Set(module, (int)floatValue);
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    Set(module, floatValue != 0f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static float RangeOf(float value, float sourceMin, float sourceMax, float destMin, float destMax) => RangeOf(value - sourceMin, sourceMax - sourceMin, destMin, destMax - destMin);

        private static float RangeOf(float offset, float sourceLen, float destMin, float destLen) => destMin + destLen * (sourceLen != 0 ? Mathf.Clamp01(offset / sourceLen) : 0.0f);

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
    }
}
#endif