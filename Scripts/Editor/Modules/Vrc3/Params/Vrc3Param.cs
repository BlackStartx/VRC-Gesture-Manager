#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDKBase;

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

        public Vrc3Param(string name, AnimatorControllerParameterType type, AnimatorControllerPlayable playable, int index) : this(name, type) => Subscribe(playable, index);

        public void Set(ModuleVrc3 module, float value, object source = null)
        {
            var isSame = RadialMenuUtility.Is(FloatValue(), value);
            LastUpdate = Time;
            InternalSet(value, source);
            if (isSame) return;
            _onChange?.Invoke(this, value);
            module.OscModule.OnParameterChange(this, value);
            foreach (var menu in module.Radials) menu.UpdateValue(Name, value);
        }

        public string TypeText => Type.ToString();

        public static int Time => (int)(DateTime.Now.Ticks / 100000L);

        public void Subscribe(AnimatorControllerPlayable playable, int index) => _controllers[playable] = index;

        public void Set(ModuleVrc3 module, bool value, object source = null) => Set(module, value ? 1f : 0f, source);

        public void Set(ModuleVrc3 module, int value, object source = null) => Set(module, (float)value, source);

        public void Set(ModuleVrc3 module, float? value, object source = null)
        {
            if (value.HasValue) Set(module, value.Value, source);
        }

        public void Set(ModuleVrc3 module, int? value, object source = null)
        {
            if (value.HasValue) Set(module, value.Value, source);
        }

        public void Set(ModuleVrc3 module, bool? value, object source = null)
        {
            if (value.HasValue) Set(module, value.Value, source);
        }

        public void SetOnChange(Action<Vrc3Param, float> onChange) => _onChange = onChange;

        public virtual float FloatValue()
        {
            var (playable, intIndex) = _controllers.FirstOrDefault();
            return playable.IsValid() ? PlayableValue(playable, intIndex) : _value;
        }

        public int IntValue() => (int)FloatValue();

        public bool BoolValue() => FloatValue() != 0f;

        private float PlayableValue(AnimatorControllerPlayable playable, int intIndex) => playable.GetParameter(intIndex).type switch
        {
            AnimatorControllerParameterType.Float => playable.GetFloat(_hashId),
            AnimatorControllerParameterType.Int => playable.GetInteger(_hashId),
            AnimatorControllerParameterType.Trigger => playable.GetBool(_hashId) ? 1f : 0f,
            AnimatorControllerParameterType.Bool => playable.GetBool(_hashId) ? 1f : 0f,
            _ => _value
        };

        [Obsolete]
        protected internal virtual void InternalSet(float value) => InternalSet(value, source: null);

        protected internal virtual void InternalSet(float value, object source)
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
                        if (value != 0f || source is VRC_AvatarParameterDriver) pair.Key.SetTrigger(_hashId);
                        break;
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