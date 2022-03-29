#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialWeightController
    {
        private readonly AnimationLayerMixerPlayable _playableMixer;
        private readonly int _index;

        private VRC_PlayableLayerControl _control;
        private float _startWeight;
        private float _startTime;

        public RadialWeightController(AnimationLayerMixerPlayable playableMixer, int index)
        {
            _playableMixer = playableMixer;
            _index = index;
        }

        public void Update()
        {
            if (!_control) return;
            Set(UpdateWeight());
        }

        public void Start(VRC_PlayableLayerControl control)
        {
            _control = control;

            _startWeight = _playableMixer.GetInputWeight(_index);
            _startTime = Time.time;
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
    }
}
#endif