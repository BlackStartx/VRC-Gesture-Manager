using System.Collections.Generic;
using GestureManager.Scripts.Extra;
using UnityEngine;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, ModuleBase> ControlledAvatars = new Dictionary<GameObject, ModuleBase>();
        public static bool InWebClientRequest;

        private TransformData _managerTransform;
        private Animator _customAnimator;
        private Transform _beforeEmote;
        private bool _drag;

        public List<ModuleBase> LastCheckedActiveModules = new List<ModuleBase>();
        public ModuleBase Module;

        public bool OnCustomAnimation { get; private set; }
        public AnimationClip customAnim;

        private void OnDisable() => UnlinkModule();

        private void Update()
        {
            if (Module == null) return;
            if (Module.IsInvalid()) UnlinkModule();
            else ModuleUpdate();
        }

        private void ModuleUpdate()
        {
            if (_drag) _managerTransform.Difference(transform).AddTo(Module.Avatar.transform);
            _managerTransform = new TransformData(transform);
            Module.Update();
        }

        private void LateUpdate()
        {
            _managerTransform = new TransformData(transform);
            Module?.LateUpdate();
        }

        public void SetDrag(bool drag) => _drag = drag;

        public void UnlinkModule() => SetModule(null);

        public void SetModule(ModuleBase module)
        {
            Module?.Unlink();
            if (Module != null) ControlledAvatars.Remove(Module.Avatar);

            Module = module;
            Module?.Avatar.transform.ApplyTo(transform);
            if (Module == null) return;

            Module.InitForAvatar();
            ControlledAvatars[module.Avatar] = module;
        }

        private void SaveCurrentStartEmotePosition() => _beforeEmote = _customAnimator.gameObject.transform;

        private void RevertToEmotePosition() => _beforeEmote.ApplyTo(_customAnimator.gameObject.transform);

        public void SetCustomAnimation(AnimationClip clip)
        {
            if (!OnCustomAnimation) customAnim = clip;
            else if (!clip) StopCustomAnimation();
            else PlayCustomAnimation(clip);
        }

        /*
         *  Events
         */

        public void PlayCustomAnimation(AnimationClip clip)
        {
            if (OnCustomAnimation) RevertToEmotePosition();
            customAnim = clip;
            OnCustomAnimation = true;
            _customAnimator = Module.OnCustomAnimationPlay(customAnim);
            SaveCurrentStartEmotePosition();
            _customAnimator.applyRootMotion = true;
        }

        public void StopCustomAnimation()
        {
            OnCustomAnimation = false;
            SetCustomAnimation(null);
            _customAnimator = Module.OnCustomAnimationPlay(null);
            if (!_customAnimator) return;
            RevertToEmotePosition();
            _customAnimator.applyRootMotion = false;
        }
    }
}