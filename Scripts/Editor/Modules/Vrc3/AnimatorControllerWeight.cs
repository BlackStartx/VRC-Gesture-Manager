﻿#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class AnimatorControllerWeight
    {
        private readonly List<SubTimer> subTimers = new List<SubTimer>();
        private readonly AnimationLayerMixerPlayable _playableMixer;
        private readonly int _index;

        private AnimatorControllerPlayable _playable;
        private VRC_PlayableLayerControl _control;

        private float _startWeight;
        private float _startTime;

        private void OnSubTimerCompleted(SubTimer timer) => subTimers.Remove(timer);

        public float Weight => _playableMixer.GetInputWeight(_index);

        public AnimatorControllerWeight(AnimationLayerMixerPlayable playableMixer, AnimatorControllerPlayable playable, int index)
        {
            _playableMixer = playableMixer;
            _playable = playable;
            _index = index;
        }

        public void Update()
        {
            if (_control) 
                Set(UpdateWeight());
            for (int i = 0; i < subTimers.Count; i++)
                subTimers[i].Update(_playable);
        }

        public void Start(VRC_PlayableLayerControl control)
        {
            _control = control;

            _startWeight = Weight;
            _startTime = Time.time;
        }

        public void Start(VRC_AnimatorLayerControl control)
        {
            var count = _playable.GetLayerCount();
            if (!control || control.layer >= count)
                return;
            subTimers.Add(new SubTimer(control, _playable.GetLayerWeight(control.layer), Time.time, OnSubTimerCompleted));
        }

        public void Set(float goalWeight)
        {
            _playableMixer.SetInputWeight(_index, goalWeight);
        }

        private float UpdateWeight()
        {
            var time = Time.time - _startTime;
            return time > _control.blendDuration ? Stop() : Mathf.Lerp(_startWeight, _control.goalWeight, time / _control.blendDuration);
        }

        private float Stop()
        {
            var weight = _control.goalWeight;
            _control = null;
            return weight;
        }

        private class SubTimer
        {
            private readonly VRC_AnimatorLayerControl _control;
            private readonly Action<SubTimer> _onComplete;
            private readonly float _startWeight;
            private readonly float _startTime;
            public SubTimer(VRC_AnimatorLayerControl control, float startWeight, float startTime, Action<SubTimer> onComplete)
            {
                _control = control;
                _startTime = startTime;
                _onComplete = onComplete;
                _startWeight = startWeight;
            }

            public void Update(AnimatorControllerPlayable playable)
            {
                var count = playable.GetLayerCount();
                if (!_control || _control.layer >= count)
                    return;
                if (_control) 
                    playable.SetLayerWeight(_control.layer, SubUpdateWeight(_control, _startWeight, _startTime));
            }

            private float SubUpdateWeight(VRC_AnimatorLayerControl control, float startWeight, float startTime)
            {
                var time = Time.time - startTime;
                return time > control.blendDuration ? SubStop(control) : Mathf.Lerp(startWeight, control.goalWeight, time / control.blendDuration);
            }

            private float SubStop(VRC_AnimatorLayerControl control)
            {
                _onComplete(this);
                return control.goalWeight;
            }
        }
    }
}
#endif