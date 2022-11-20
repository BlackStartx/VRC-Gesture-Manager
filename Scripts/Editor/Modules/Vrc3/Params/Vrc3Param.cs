﻿#if VRC_SDK_VRCSDK3
using System;
using BlackStartX.GestureManager.Editor.Lib;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Params
{
    public abstract class Vrc3Param
    {
        public readonly AnimatorControllerParameterType Type;
        private readonly Func<float, float> _converted;
        protected readonly int HashId;
        public readonly string Name;
        public int LastUpdate;

        private Action<Vrc3Param, float> _onChange;

        protected Vrc3Param(string name, AnimatorControllerParameterType type)
        {
            Name = name;
            Type = type;
            LastUpdate = Time;
            HashId = Animator.StringToHash(Name);
            _converted = GenerateConverter();
        }

        public void Set(ModuleVrc3 module, float value)
        {
            if (Is(value = _converted(value))) return;
            module.OscModule.OnParameterChange(this, value);
            LastUpdate = Time;
            InternalSet(value);
            _onChange?.Invoke(this, value);
            foreach (var menu in module.Radials) menu.UpdateValue(Name, value);
        }

        public string TypeText => Type.ToString();

        public static int Time => (int)(DateTime.Now.Ticks / 100000L);

        private void Set(ModuleVrc3 module, bool value) => Set(module, value ? 1f : 0f);

        private void Set(ModuleVrc3 module, int value) => Set(module, (float)value);

        public void Set(ModuleVrc3 module, float? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void Set(ModuleVrc3 module, bool? value)
        {
            if (value.HasValue) Set(module, value.Value);
        }

        public void SetOnChange(Action<Vrc3Param, float> onChange) => _onChange = onChange;

        private bool Is(float value) => RadialMenuUtility.Is(Get(), value);

        public abstract float Get();

        protected internal abstract void InternalSet(float value);

        public void Add(ModuleVrc3 module, float value) => Set(module, Get() + value);

        public void Copy(ModuleVrc3 module, float get, bool range, float sourceMin, float sourceMax, float destMin, float destMax)
        {
            if (range) get = RangeOf(get, sourceMin, sourceMax, destMin, destMax);
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    Set(module, get);
                    break;
                case AnimatorControllerParameterType.Int:
                    Set(module, (int)get);
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    Set(module, !RadialMenuUtility.Is(get, 0));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static float RangeOf(float get, float sourceMin, float sourceMax, float destMin, float destMax) => RangeOf(get - sourceMin, sourceMax - sourceMin, destMin, destMax - destMin);

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
                    return Get() > 0.5f ? (Color.green, "True") : (Color.red, "False");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void FieldTuple(ModuleVrc3 module, GUILayoutOption innerOption)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.label, innerOption);
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    if (GmgLayoutHelper.UnityFieldEnterListener(Get(), module, rect, EditorGUI.FloatField, Set, module.Edit)) module.Edit = null;
                    break;
                case AnimatorControllerParameterType.Int:
                    if (GmgLayoutHelper.UnityFieldEnterListener((int)Get(), module, rect, EditorGUI.IntField, Set, module.Edit)) module.Edit = null;
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    Set(module, Get() < 0.5f);
                    module.Edit = null;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private Func<float, float> GenerateConverter()
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    return value => value;
                case AnimatorControllerParameterType.Int:
                    return value => (int)value;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return value => value > 0.5f ? 1f : 0f;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif