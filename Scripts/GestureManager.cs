using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Extra;
using UnityEngine;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        [SerializeField] private int instanceId;

        public static Dictionary<GameObject, ModuleBase> ControlledAvatars = new Dictionary<GameObject, ModuleBase>();

        public int right, left, emote;

        public bool inWebClientRequest;

        private Vector3 _beforeEmoteAvatarScale;
        private Vector3 _beforeEmoteAvatarPosition;
        private Quaternion _beforeEmoteAvatarRotation;

        public List<ModuleBase> LastCheckedActiveDescriptors = new List<ModuleBase>();
        public ModuleBase Module;

        public bool OnCustomAnimation { get; private set; }
        public AnimationClip customAnim;
        private Animator _customAnimator;

        public RuntimeAnimatorController avatarWasUsing;

        // Used to control the rotation of the model bones during the animation playtime. (like outside the play mode)
        private Dictionary<HumanBodyBones, Quaternion> _lastBoneQuaternions;
        public int controlDelay;

        private IEnumerable<HumanBodyBones> _whiteListedControlBones;

        private readonly List<HumanBodyBones> _blackListedControlBones = new List<HumanBodyBones>()
        {
            // Left
            HumanBodyBones.LeftThumbDistal,
            HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.LeftThumbProximal,

            HumanBodyBones.LeftIndexDistal,
            HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.LeftIndexProximal,

            HumanBodyBones.LeftMiddleDistal,
            HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftMiddleProximal,

            HumanBodyBones.LeftRingDistal,
            HumanBodyBones.LeftRingIntermediate,
            HumanBodyBones.LeftRingProximal,

            HumanBodyBones.LeftLittleDistal,
            HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.LeftLittleProximal,

            // Right
            HumanBodyBones.RightThumbDistal,
            HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.RightThumbProximal,

            HumanBodyBones.RightIndexDistal,
            HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.RightIndexProximal,

            HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleProximal,

            HumanBodyBones.RightRingDistal,
            HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.RightRingProximal,

            HumanBodyBones.RightLittleDistal,
            HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleProximal,

            // ???
            HumanBodyBones.LastBone
        };

        private void Awake()
        {
            if (instanceId == GetInstanceID()) return;

            if (instanceId != 0)
            {
                instanceId = GetInstanceID();
                if (instanceId >= 0) return;

                Module = null;
            }
            else instanceId = GetInstanceID();
        }

        private void OnDisable() => UnlinkModule();

        private void Update()
        {
            ControlledAvatars = ControlledAvatars.Where(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

            if (Module != null && Module.IsInvalid()) UnlinkModule();
            if (Module == null) return;

            FetchLastBoneRotation();
            Module.SetValues(OnCustomAnimation, left, right, emote);
            Module.Update();
        }

        private void LateUpdate()
        {
            if (Module == null || !Module.LateBoneUpdate) return;

            if (emote != 0 || OnCustomAnimation) return;

            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = Module.AvatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    var boneRotation = _lastBoneQuaternions[bodyBone];
                    bone.localRotation = new Quaternion(boneRotation.x, boneRotation.y, boneRotation.z, boneRotation.w);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void FetchLastBoneRotation()
        {
            if (!Module.LateBoneUpdate) return;

            if (emote != 0 || OnCustomAnimation)
            {
                controlDelay = 5;
                return;
            }

            if (controlDelay > 0)
            {
                controlDelay--;
                return;
            }

            _lastBoneQuaternions = new Dictionary<HumanBodyBones, Quaternion>();
            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = Module.AvatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    _lastBoneQuaternions[bodyBone] = bone.localRotation;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private IEnumerable<HumanBodyBones> GetWhiteListedControlBones()
        {
            return _whiteListedControlBones ?? (_whiteListedControlBones = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().Where(bones => !_blackListedControlBones.Contains(bones)));
        }

        public void UnlinkModule() => SetModule(null);

        public void SetModule(ModuleBase module)
        {
            Module?.Unlink();
            if (Module != null) ResetCurrentAvatarController();

            Module = module;
            if (module == null || !module.Avatar) return;

            Module?.InitForAvatar();
            ControlledAvatars[module.Avatar] = module;
        }

        public List<ModuleBase> GetLastCheckedActiveDescriptors()
        {
            return LastCheckedActiveDescriptors;
        }

        private void ResetCurrentAvatarController()
        {
            if (Module == null) return;

            var animator = Module.AvatarAnimator;
            if (!animator) return;

            animator.runtimeAnimatorController = avatarWasUsing;
            avatarWasUsing = null;
            ControlledAvatars.Remove(Module.Avatar);
        }

        public string GetEmoteName(int emoteIndex)
        {
            return Module.GetEmoteByIndex(emoteIndex).name;
        }

        public string GetFinalGestureName(GestureHand hand, int gestureIndex)
        {
            return Module.GetFinalGestureByIndex(hand, gestureIndex).name;
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