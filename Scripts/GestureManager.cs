using System.Collections.Generic;
using GestureManager.Scripts.Extra;
using UnityEngine;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, ModuleBase> ControlledAvatars = new Dictionary<GameObject, ModuleBase>();
        public static bool InWebClientRequest;

        private Vector3 _beforeEmoteAvatarScale;
        private Vector3 _beforeEmoteAvatarPosition;
        private Quaternion _beforeEmoteAvatarRotation;
        private Animator _customAnimator;

        public List<ModuleBase> LastCheckedActiveModules = new List<ModuleBase>();
        public ModuleBase Module;

        public bool OnCustomAnimation { get; private set; }
        public AnimationClip customAnim;

        private void OnDisable() => UnlinkModule();

        private void Update()
        {
            if (Module != null && Module.IsInvalid()) UnlinkModule();
            Module?.Update();
        }

        private void LateUpdate() => Module?.LateUpdate();

        public void UnlinkModule() => SetModule(null);

        public void SetModule(ModuleBase module)
        {
            Module?.Unlink();
            if (Module != null) ControlledAvatars.Remove(Module.Avatar);

            Module = module;
            if (Module == null) return;

            Module.InitForAvatar();
            ControlledAvatars[module.Avatar] = module;
        }

        private void SaveCurrentStartEmotePosition()
        {
            var animatorGameObject = _customAnimator.gameObject;
            _beforeEmoteAvatarPosition = animatorGameObject.transform.position;
            _beforeEmoteAvatarRotation = animatorGameObject.transform.rotation;
            _beforeEmoteAvatarScale = animatorGameObject.transform.localScale;
        }

        private void RevertToEmotePosition()
        {
            var animatorGameObject = _customAnimator.gameObject;
            animatorGameObject.transform.position = _beforeEmoteAvatarPosition;
            animatorGameObject.transform.rotation = _beforeEmoteAvatarRotation;
            animatorGameObject.transform.localScale = _beforeEmoteAvatarScale;
        }

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