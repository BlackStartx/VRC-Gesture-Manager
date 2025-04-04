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
        private readonly Dictionary<AnimatorControllerPlayable, PlayableParam> _controllers = new();

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

        public void Subscribe(AnimatorControllerPlayable playable, int index) => _controllers[playable] = new PlayableParam(playable, index, _hashId);

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

        internal float AapValue()
        {
            var (playable, param) = _controllers.FirstOrDefault();
            return playable.IsValid() ? param.GetValue(aapChk: true) : _value;
        }

        public virtual float FloatValue()
        {
            var (playable, param) = _controllers.FirstOrDefault();
            return playable.IsValid() ? param.GetValue() : _value;
        }

        public int IntValue() => (int)FloatValue();

        public bool BoolValue() => FloatValue() != 0f;

        protected internal virtual void InternalSet(float value, object source = null)
        {
            _value = value;
            foreach (var pair in _controllers.Where(pair => ModuleVrc3.IsValid(pair.Key))) pair.Value.SetValue(value, source);
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

        private class PlayableParam
        {
            private AnimatorControllerPlayable _playable;

            private readonly int _hashId;
            private readonly AnimatorControllerParameterType _type;

            private bool _isAap() => _playable.IsParameterControlledByCurve(_hashId);

            public PlayableParam(AnimatorControllerPlayable playable, int index, int hashId)
            {
                _playable = playable;
                _hashId = hashId;

                _type = playable.GetParameter(index).type;
            }

            internal float GetValue(bool aapChk = false) => _type switch
            {
                AnimatorControllerParameterType.Float => aapChk && _isAap() ? 0f : _playable.GetFloat(_hashId),
                AnimatorControllerParameterType.Trigger => _playable.GetBool(_hashId) ? 1f : 0f,
                AnimatorControllerParameterType.Bool => _playable.GetBool(_hashId) ? 1f : 0f,
                AnimatorControllerParameterType.Int => _playable.GetInteger(_hashId),
                _ => 0f
            };

            public void SetValue(float value, object source)
            {
                if (_isAap()) return;
                switch (_type)
                {
                    case AnimatorControllerParameterType.Float:
                        _playable.SetFloat(_hashId, value);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _playable.SetInteger(_hashId, (int)Math.Round(value));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (value != 0f || source is VRC_AvatarParameterDriver) _playable.SetTrigger(_hashId);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        _playable.SetBool(_hashId, value != 0f);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
#endif