using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core;
using UnityEngine;
using UnityEngine.Networking;
using VRC.SDKBase;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, GestureManager> ControlledAvatars = new Dictionary<GameObject, GestureManager>();
        
        private const string VersionUrl = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.version";

        private GameObject _avatar;

        public GameObject Avatar
        {
            get => _avatar;
            private set
            {
                _isControllingAnAvatar = value != null;
                _avatar = value;
            }
        }

        public int right, left, emote;
        public bool onCustomAnimation;

        public bool currentlyCheckingForUpdates;

        public AnimationClip customAnim;

        public Animator avatarAnimator;

        private Vector3 _beforeEmoteAvatarScale;
        private Vector3 _beforeEmoteAvatarPosition;
        private Quaternion _beforeEmoteAvatarRotation;

        private RuntimeAnimatorController _standingRuntimeOverrideControllerPreset;
        private RuntimeAnimatorController _seatedRuntimeOverrideControllerPreset;

        private VRC_AvatarDescriptor _avatarDescriptor;

        private ControllerType _usingType;
        private ControllerType _notUsedType;

        private VRC_AvatarDescriptor[] _lastCheckedActiveDescriptors;

        private AnimatorOverrideController _originalUsingOverrideController;
        private AnimatorOverrideController _myRuntimeOverrideController;

        private RuntimeAnimatorController _avatarWasUsing;

        // Used to control the rotation of the model bones during the animation playtime. (like outside the play mode)
        private Dictionary<HumanBodyBones, Quaternion> _lastBoneQuaternions;
        private int _controlDelay;

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

        // I'm using the ReShaper editor... and those type are really annoying! >.<
        // ReSharper disable StringLiteralTypo
        private readonly AnimationBind[] _gestureBinds =
        {
            new AnimationBind(null, "[GESTURE] Idle"),
            new AnimationBind("FIST", "[GESTURE] Fist"),
            new AnimationBind("HANDOPEN", "[GESTURE] Open"),
            new AnimationBind("FINGERPOINT", "[GESTURE] FingerPoint"),
            new AnimationBind("VICTORY", "[GESTURE] Victory"),
            new AnimationBind("ROCKNROLL", "[GESTURE] Rock&Roll"),
            new AnimationBind("HANDGUN", "[GESTURE] Gun"),
            new AnimationBind("THUMBSUP", "[GESTURE] ThumbsUp"),
        };
        // ReSharper restore StringLiteralTypo

        private readonly AnimationBind[] _emoteBinds =
        {
            new AnimationBind("EMOTE1", "[EMOTE 1] Wave", "[EMOTE 1] Laugh"),
            new AnimationBind("EMOTE2", "[EMOTE 2] Clap", "[EMOTE 2] Point"),
            new AnimationBind("EMOTE3", "[EMOTE 3] Point", "[EMOTE 3] Raise Hand"),
            new AnimationBind("EMOTE4", "[EMOTE 4] Cheer", "[EMOTE 4] Drum"),
            new AnimationBind("EMOTE5", "[EMOTE 5] Dance", "[EMOTE 5] Clap"),
            new AnimationBind("EMOTE6", "[EMOTE 6] BackFlip", "[EMOTE 6] Angry Fist"),
            new AnimationBind("EMOTE7", "[EMOTE 7] Die", "[EMOTE 7] Disbelief"),
            new AnimationBind("EMOTE8", "[EMOTE 8] Sad", "[EMOTE 8] Disapprove"),
        };

        /*
         * Animation Info...
         */

        private Dictionary<string, bool> _hasBeenOverridden;

        [SerializeField] private int instanceId;

        private static readonly int HandGestureLeft = Animator.StringToHash("HandGestureLeft");
        private static readonly int HandGestureRight = Animator.StringToHash("HandGestureRight");
        private static readonly int Emote = Animator.StringToHash("Emote");

        private bool _isControllingAnAvatar;

        private void Awake()
        {
            if (instanceId == GetInstanceID()) return;

            if (instanceId == 0)
                instanceId = GetInstanceID();
            else
            {
                instanceId = GetInstanceID();
                if (instanceId >= 0) return;

                Avatar = null;
            }
        }

        private void OnEnable()
        {
            if (Avatar != null) return;

            var validDescriptor = GetValidDescriptor();
            if (validDescriptor != null)
                InitForAvatar(validDescriptor);
        }

        private void OnDisable()
        {
            UnlinkFromAvatar();
        }

        private void Update()
        {
            if (!_isControllingAnAvatar) return;

            FetchLastBoneRotation();
            SetValues();
        }

        private void LateUpdate()
        {
            if (!_isControllingAnAvatar) return;

            if (emote != 0 || onCustomAnimation) return;

            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = avatarAnimator.GetBoneTransform(bodyBone);
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
            if (emote != 0 || onCustomAnimation)
            {
                _controlDelay = 5;
                return;
            }

            if (_controlDelay > 0)
            {
                _controlDelay--;
                return;
            }

            _lastBoneQuaternions = new Dictionary<HumanBodyBones, Quaternion>();
            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = avatarAnimator.GetBoneTransform(bodyBone);
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

        public void StopCurrentEmote()
        {
            if (emote != 0)
                OnEmoteStop();

            if (onCustomAnimation)
                OnCustomEmoteStop();
        }

        public void UnlinkFromAvatar()
        {
            ResetCurrentAvatarController();
            Avatar = null;
            _avatarDescriptor = null;
        }

        public void CheckForUpdates(Action<UnityWebRequest> onNetworkResponseError, Action<UnityWebRequest> onNetworkResponse)
        {
            if (currentlyCheckingForUpdates)
            {
                Debug.Log("Gesture Manager: Already looking for updates...");
                return;
            }

            currentlyCheckingForUpdates = true;
            StartCoroutine(GetRequest(VersionUrl, (error) =>
            {
                currentlyCheckingForUpdates = false;
                onNetworkResponseError(error);
            }, (response) =>
            {
                currentlyCheckingForUpdates = false;
                onNetworkResponse(response);
            }));
        }

        public bool CanSwitchController()
        {
            if (_notUsedType == ControllerType.Seated)
                return _avatarDescriptor.CustomSittingAnims != null;
            return _avatarDescriptor.CustomStandingAnims != null;
        }

        private VRC_AvatarDescriptor GetValidDescriptor()
        {
            CheckActiveDescriptors();
            return _lastCheckedActiveDescriptors.FirstOrDefault(IsValidDesc);
        }

        public void CheckActiveDescriptors()
        {
            _lastCheckedActiveDescriptors = VRC.Tools.FindSceneObjectsOfTypeAll<VRC_AvatarDescriptor>();
        }

        public VRC_AvatarDescriptor[] GetLastCheckedActiveDescriptors()
        {
            return _lastCheckedActiveDescriptors;
        }

        public bool IsValidDesc(VRC_AvatarDescriptor descriptor)
        {
            if (descriptor == null) return false;
            if (!descriptor.gameObject.activeInHierarchy) return false;

            var animator = descriptor.gameObject.GetComponent<Animator>();

            if (animator == null) return false;
            if (!animator.isHuman) return false;
            if (descriptor.CustomSittingAnims == null && descriptor.CustomStandingAnims == null) return false;

            return !ControlledAvatars.ContainsKey(descriptor.gameObject);
        }

        public AnimatorOverrideController GetOverrideController()
        {
            return _originalUsingOverrideController;
        }

        private void ResetCurrentAvatarController()
        {
            if (Avatar == null) return;

            var animator = Avatar.GetComponent<Animator>();
            if (animator == null) return;

            animator.runtimeAnimatorController = _avatarWasUsing;
            _avatarWasUsing = null;
            ControlledAvatars.Remove(Avatar);
        }

        public string GetEmoteName(int emoteIndex)
        {
            return GetEmoteByIndex(emoteIndex).name;
        }

        public string GetFinalGestureName(int gestureIndex)
        {
            return GetFinalGestureByIndex(gestureIndex).name;
        }

        public bool HasGestureBeenOverridden(int gestureIndex)
        {
            return _hasBeenOverridden.ContainsKey(_gestureBinds[gestureIndex].GetMyName(_usingType));
        }

        public string GetMyGestureNameByIndex(int gestureIndex)
        {
            return _gestureBinds[gestureIndex].GetMyName(_usingType);
        }

        public void InitForAvatar(VRC_AvatarDescriptor descriptor)
        {
            Avatar = descriptor.gameObject;
            _avatarDescriptor = descriptor;

            avatarAnimator = Avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
                avatarAnimator = Avatar.AddComponent<Animator>();

            if (_avatarDescriptor.CustomStandingAnims != null)
                SetupOverride(ControllerType.Standing, true);
            else if (_avatarDescriptor.CustomSittingAnims != null)
                SetupOverride(ControllerType.Seated, true);
            else
            {
                Avatar = null;
                _avatarDescriptor = null;
                avatarAnimator = null;
            }
        }

        private RuntimeAnimatorController GetStandingRuntimeOverrideControllerPreset()
        {
            if (_standingRuntimeOverrideControllerPreset == null)
                _standingRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("StandingEmoteTestingTemplate");
            return _standingRuntimeOverrideControllerPreset;
        }

        private RuntimeAnimatorController GetSeatedRuntimeOverrideControllerPreset()
        {
            if (_seatedRuntimeOverrideControllerPreset == null)
                _seatedRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("SeatedEmoteTestingTemplate");
            return _seatedRuntimeOverrideControllerPreset;
        }

        private void SetupOverride(ControllerType controllerType, bool saveController)
        {
            ControlledAvatars[Avatar] = this;
            _controlDelay = 4;

            string controllerName;
            if (controllerType == ControllerType.Standing)
            {
                _usingType = ControllerType.Standing;
                _notUsedType = ControllerType.Seated;

                _originalUsingOverrideController = _avatarDescriptor.CustomStandingAnims;
                _myRuntimeOverrideController = new AnimatorOverrideController(GetStandingRuntimeOverrideControllerPreset());
                controllerName = GetStandingRuntimeOverrideControllerPreset().name;
            }
            else
            {
                _usingType = ControllerType.Seated;
                _notUsedType = ControllerType.Standing;

                _originalUsingOverrideController = _avatarDescriptor.CustomSittingAnims;
                _myRuntimeOverrideController = new AnimatorOverrideController(GetSeatedRuntimeOverrideControllerPreset());
                controllerName = GetSeatedRuntimeOverrideControllerPreset().name;
            }

            GenerateDictionary();

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(_myRuntimeOverrideController["[EXTRA] CustomAnimation"], customAnim)
            };


            var validOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (var pair in GmgMyAnimatorControllerHelper.GetOverrides(_originalUsingOverrideController).Where(keyValuePair => keyValuePair.Value != null))
            {
                try
                {
                    validOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(_myRuntimeOverrideController[_myTranslateDictionary[pair.Key.name]], pair.Value));
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            finalOverride.AddRange(validOverrides);

            _hasBeenOverridden = new Dictionary<string, bool>();
            foreach (var valuePair in finalOverride) _hasBeenOverridden[valuePair.Key.name] = true;

            _myRuntimeOverrideController.ApplyOverrides(finalOverride);

            if (saveController)
                _avatarWasUsing = avatarAnimator.runtimeAnimatorController;

            avatarAnimator.runtimeAnimatorController = _myRuntimeOverrideController;
            avatarAnimator.runtimeAnimatorController.name = controllerName;
        }

        private void SetValues()
        {
            if (onCustomAnimation)
            {
                avatarAnimator.SetInteger(HandGestureLeft, 8);
                avatarAnimator.SetInteger(HandGestureRight, 8);
                avatarAnimator.SetInteger(Emote, 9);
            }
            else if (emote != 0)
            {
                avatarAnimator.SetInteger(HandGestureLeft, 8);
                avatarAnimator.SetInteger(HandGestureRight, 8);
                avatarAnimator.SetInteger(Emote, emote);
            }
            else
            {
                avatarAnimator.SetInteger(HandGestureLeft, left);
                avatarAnimator.SetInteger(HandGestureRight, right);
                avatarAnimator.SetInteger(Emote, emote);
            }
        }

        public void SwitchType()
        {
            SetupOverride(_notUsedType, false);
        }

        public ControllerType GetUsedType()
        {
            return _usingType;
        }

        public ControllerType GetNotUsedType()
        {
            return _notUsedType;
        }

        private void SaveCurrentStartEmotePosition()
        {
            _beforeEmoteAvatarPosition = Avatar.transform.position;
            _beforeEmoteAvatarRotation = Avatar.transform.rotation;
            _beforeEmoteAvatarScale = Avatar.transform.localScale;
        }

        private void RevertToEmotePosition()
        {
            Avatar.transform.position = _beforeEmoteAvatarPosition;
            Avatar.transform.rotation = _beforeEmoteAvatarRotation;
            Avatar.transform.localScale = _beforeEmoteAvatarScale;
        }

        public void SetCustomAnimation(AnimationClip clip)
        {
            customAnim = clip;

            SetupOverride(GetUsedType(), false);
        }

        /**
         *  LISTENERS
         *  LISTENERS
         *  LISTENERS
         */

        public void OnEmoteStop()
        {
            emote = 0;
            avatarAnimator.applyRootMotion = false;
            RevertToEmotePosition();
            SetCustomAnimation(null);
        }

        public void OnEmoteStart(int emoteIndex)
        {
            emote = emoteIndex;
            avatarAnimator.applyRootMotion = true;
            SetCustomAnimation(GetEmoteByIndex(emoteIndex - 1));
            SaveCurrentStartEmotePosition();
        }

        public void OnCustomEmoteStop()
        {
            onCustomAnimation = false;
            avatarAnimator.applyRootMotion = false;
            SetCustomAnimation(null);
            RevertToEmotePosition();
        }

        public void OnCustomEmoteStart()
        {
            avatarAnimator.applyRootMotion = true;
            SaveCurrentStartEmotePosition();
            onCustomAnimation = true;
        }

        /**
         * Async
         */

        private static IEnumerator GetRequest(string uri, Action<UnityWebRequest> onNetworkResponseError, Action<UnityWebRequest> onNetworkResponse)
        {
            using (var webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                    onNetworkResponseError(webRequest);
                else
                    onNetworkResponse(webRequest);
            }
        }

        /**
         *     This dictionary is needed just because I hate the original animation names.
         *     It will just translate the name of the original animation to the my version of the name. 
         */
        private Dictionary<string, string> _myTranslateDictionary;

        private void GenerateDictionary()
        {
            _myTranslateDictionary = new Dictionary<string, string>()
            {
                {_gestureBinds[1].GetOriginalName(), _gestureBinds[1].GetMyName(_usingType)},
                {_gestureBinds[2].GetOriginalName(), _gestureBinds[2].GetMyName(_usingType)},
                {_gestureBinds[3].GetOriginalName(), _gestureBinds[3].GetMyName(_usingType)},
                {_gestureBinds[4].GetOriginalName(), _gestureBinds[4].GetMyName(_usingType)},
                {_gestureBinds[5].GetOriginalName(), _gestureBinds[5].GetMyName(_usingType)},
                {_gestureBinds[6].GetOriginalName(), _gestureBinds[6].GetMyName(_usingType)},
                {_gestureBinds[7].GetOriginalName(), _gestureBinds[7].GetMyName(_usingType)},

                {_emoteBinds[0].GetOriginalName(), _emoteBinds[0].GetMyName(_usingType)},
                {_emoteBinds[1].GetOriginalName(), _emoteBinds[1].GetMyName(_usingType)},
                {_emoteBinds[2].GetOriginalName(), _emoteBinds[2].GetMyName(_usingType)},
                {_emoteBinds[3].GetOriginalName(), _emoteBinds[3].GetMyName(_usingType)},
                {_emoteBinds[4].GetOriginalName(), _emoteBinds[4].GetMyName(_usingType)},
                {_emoteBinds[5].GetOriginalName(), _emoteBinds[5].GetMyName(_usingType)},
                {_emoteBinds[6].GetOriginalName(), _emoteBinds[6].GetMyName(_usingType)},
                {_emoteBinds[7].GetOriginalName(), _emoteBinds[7].GetMyName(_usingType)},
            };
        }

        private AnimationClip GetEmoteByIndex(int emoteIndex)
        {
            return _myRuntimeOverrideController[_emoteBinds[emoteIndex].GetMyName(_usingType)];
        }

        public AnimationClip GetFinalGestureByIndex(int gestureIndex)
        {
            return _myRuntimeOverrideController[_gestureBinds[gestureIndex].GetMyName(_usingType)];
        }

        public void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
            _originalUsingOverrideController[_gestureBinds[gestureIndex].GetOriginalName()] = newAnimation;
            SetupOverride(_usingType, false);
        }
    }

    public enum ControllerType
    {
        Standing,
        Seated
    };

    public class AnimationBind
    {
        private readonly string _originalName;
        private readonly string _myStandingName;
        private readonly string _mySeatedName;

        public AnimationBind(string originalName, string myStandingName, string mySeatedName)
        {
            _originalName = originalName;
            _myStandingName = myStandingName;
            _mySeatedName = mySeatedName;
        }

        public AnimationBind(string originalName, string myName) : this(originalName, myName, myName)
        {
        }

        public string GetMyName(ControllerType controller)
        {
            return controller == ControllerType.Standing ? _myStandingName : _mySeatedName;
        }

        public string GetOriginalName()
        {
            return _originalName;
        }
    }
}