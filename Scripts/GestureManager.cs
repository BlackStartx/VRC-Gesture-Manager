using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VRCSDK2;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        private const string VersionUrl = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.version";
        
        public GameObject avatar;
        public int right, left, emote;
        public bool onCustomAnimation;

        public bool currentlyCheckingForUpdates;

        public AnimationClip customAnim;

        private Vector3 beforeEmoteAvatarScale;
        private Vector3 beforeEmoteAvatarPosition;
        private Quaternion beforeEmoteAvatarRotation;

        private RuntimeAnimatorController standingRuntimeOverrideControllerPreset;
        private RuntimeAnimatorController seatedRuntimeOverrideControllerPreset;

        private VRC_AvatarDescriptor avatarDescriptor;

        private readonly string[] gestureBaseNames = {"...", "FIST", "HANDOPEN", "FINGERPOINT", "VICTORY", "ROCKNROLL", "HANDGUN", "THUMBSUP"};
        private readonly string[] emoteBaseNames = {"EMOTE1", "EMOTE2", "EMOTE3", "EMOTE4", "EMOTE5", "EMOTE6", "EMOTE7", "EMOTE8"};

        private ControllerType usingType;
        private ControllerType notUsedType;

        private VRC_AvatarDescriptor[] lastCheckedActiveDescriptors;

        private readonly AnimationClip[] emoteClips = new AnimationClip[8];
        private readonly AnimationClip[] gestureClips = new AnimationClip[8];

        private readonly string[] emoteNames = new string[8];
        private readonly string[] gestureNames = new string[8];
        private string customAnimName;

        private Animator avatarAnimator;
        private AnimatorOverrideController overrideController;
        private AnimatorOverrideController runtimeOverrideController;

        private RuntimeAnimatorController avatarWasUsing;

        [SerializeField] private int instanceId;
        private static readonly int HandGestureLeft = Animator.StringToHash("HandGestureLeft");
        private static readonly int HandGestureRight = Animator.StringToHash("HandGestureRight");
        private static readonly int Emote = Animator.StringToHash("Emote");

        private void Awake()
        {
            if (instanceId != GetInstanceID())
            {
                if (instanceId == 0)
                    instanceId = GetInstanceID();
                else
                {
                    instanceId = GetInstanceID();
                    if (instanceId < 0) 
                        avatar = null;
                }
            }
        }

        private void Update()
        {
            if (avatar != null) SetValues();
        }

        private void OnEnable()
        {
            if (avatar == null)
            {
                var validDescriptor = GetValidDescriptor();
                if (validDescriptor != null)
                    InitForAvatar(validDescriptor);
            }
        }

        private void OnDisable()
        {
            UnlinkFromAvatar();
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
            avatar = null;
            avatarDescriptor = null;
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
                onNetworkResponseError(error);
                currentlyCheckingForUpdates = false;
            }, (response) =>
            {
                onNetworkResponse(response);
                currentlyCheckingForUpdates = false;
            }));
        }

        public bool CanSwitchController()
        {
            if (notUsedType == ControllerType.Seated)
                return avatarDescriptor.CustomSittingAnims != null;
            return avatarDescriptor.CustomStandingAnims != null;
        }

        private VRC_AvatarDescriptor GetValidDescriptor()
        {
            CheckActiveDescriptors();
            return lastCheckedActiveDescriptors.FirstOrDefault(IsValidDesc);
        }

        public void CheckActiveDescriptors()
        {
            lastCheckedActiveDescriptors = VRC.Tools.FindSceneObjectsOfTypeAll<VRC_AvatarDescriptor>();
        }

        public VRC_AvatarDescriptor[] GetLastCheckedActiveDescriptors()
        {
            return lastCheckedActiveDescriptors;
        }

        public bool IsValidDesc(VRC_AvatarDescriptor descriptor)
        {
            if (descriptor != null)
            {
                if (descriptor.gameObject.activeInHierarchy)
                {
                    var animator = descriptor.gameObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        if (animator.isHuman)
                        {
                            if (descriptor.CustomSittingAnims != null || descriptor.CustomStandingAnims != null)
                            {
                                var runtimeAnimatorController = animator.runtimeAnimatorController;
                                if (runtimeAnimatorController == null)
                                    return true;
                                if (!runtimeAnimatorController.name.Equals(GetStandingRuntimeOverrideControllerPreset().name) &&
                                    !runtimeAnimatorController.name.Equals(GetSeatedRuntimeOverrideControllerPreset().name))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void FetchRuntimeOverrideAnimationNames()
        {
            if (runtimeOverrideController != null)
            {
                for (var i = 0; i < 8; i++)
                    emoteNames[i] = runtimeOverrideController.animationClips[i].name;

                customAnimName = runtimeOverrideController.animationClips[25].name;

                gestureNames[0] = runtimeOverrideController.animationClips[24].name; // Idle        V
                gestureNames[1] = runtimeOverrideController.animationClips[17].name; // Close       V
                gestureNames[2] = runtimeOverrideController.animationClips[20].name; // Open        V
                gestureNames[3] = runtimeOverrideController.animationClips[18].name; // Finger      V
                gestureNames[4] = runtimeOverrideController.animationClips[22].name; // Victory     V
                gestureNames[5] = runtimeOverrideController.animationClips[19].name; // Rock&Roll   V
                gestureNames[6] = runtimeOverrideController.animationClips[23].name; // HandGun     V
                gestureNames[7] = runtimeOverrideController.animationClips[21].name; // ThumbsUp    V
            }
        }

        public AnimatorOverrideController GetOverrideController()
        {
            return overrideController;
        }

        private void ResetCurrentAvatarController()
        {
            if (avatar != null)
            {
                var animator = avatar.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = avatarWasUsing;
                    avatarWasUsing = null;
                }
            }
        }

        public string GetEmoteName(int emoteIndex)
        {
            return emoteClips[emoteIndex].name;
        }

        public string GetGestureName(int gestureIndex)
        {
            return gestureClips[gestureIndex].name;
        }

        public void InitForAvatar(VRC_AvatarDescriptor descriptor)
        {
            avatar = descriptor.gameObject;
            avatarDescriptor = descriptor;

            avatarAnimator = avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
                avatarAnimator = avatar.AddComponent<Animator>();

            if (avatarDescriptor.CustomStandingAnims != null)
                SetupOverride(ControllerType.Standing, true);
            else if (avatarDescriptor.CustomSittingAnims != null)
                SetupOverride(ControllerType.Seated, true);
            else
            {
                avatar = null;
                avatarDescriptor = null;
                avatarAnimator = null;
            }
        }

        private RuntimeAnimatorController GetStandingRuntimeOverrideControllerPreset()
        {
            if (standingRuntimeOverrideControllerPreset == null)
                standingRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("StandingEmoteTestingTemplate");
            return standingRuntimeOverrideControllerPreset;
        }

        private RuntimeAnimatorController GetSeatedRuntimeOverrideControllerPreset()
        {
            if (seatedRuntimeOverrideControllerPreset == null)
                seatedRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("SeatedEmoteTestingTemplate");
            return seatedRuntimeOverrideControllerPreset;
        }

        private void SetupOverride(ControllerType controllerType, bool saveController)
        {
            string controllerName;
            switch (controllerType)
            {
                case ControllerType.Standing:
                {
                    usingType = ControllerType.Standing;
                    notUsedType = ControllerType.Seated;

                    overrideController = avatarDescriptor.CustomStandingAnims;
                    runtimeOverrideController = new AnimatorOverrideController(GetStandingRuntimeOverrideControllerPreset());
                    controllerName = GetStandingRuntimeOverrideControllerPreset().name;

                    break;
                }
                case ControllerType.Seated:
                {
                    usingType = ControllerType.Seated;
                    notUsedType = ControllerType.Standing;

                    overrideController = avatarDescriptor.CustomSittingAnims;
                    runtimeOverrideController = new AnimatorOverrideController(GetSeatedRuntimeOverrideControllerPreset());
                    controllerName = GetSeatedRuntimeOverrideControllerPreset().name;

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("controllerType", controllerType, null);
            }

            FetchRuntimeOverrideAnimationNames();

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[customAnimName], customAnim)
            };


            /**
             * Gestures...
             */

            var gestureIndex = new[] {0, 39, 42, 40, 44, 41, 45, 43};

            for (var index = 0; index < 8; index++)
            {
                var overrideClip = runtimeOverrideController[gestureNames[index]];
                var clip = overrideController.animationClips[gestureIndex[index]];
                if (!clip.name.Equals(gestureBaseNames[index]))
                    overrideClip = clip;
                finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[gestureNames[index]], overrideClip));
                gestureClips[index] = overrideClip;
            }

            /**
             * Emotes...
             */

            var emoteIndex = new[] {2, 3, 4, 5, 6, 7, 8, 9, 10};

            for (var index = 0; index < 8; index++)
            {
                var overrideClip = runtimeOverrideController[emoteNames[index]];
                var clip = overrideController.animationClips[emoteIndex[index]];
                if (!clip.name.Equals(emoteBaseNames[index]))
                    overrideClip = clip;
                finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[emoteNames[index]], overrideClip));
                emoteClips[index] = overrideClip;
            }

            runtimeOverrideController.ApplyOverrides(finalOverride);

            if (saveController)
                avatarWasUsing = avatarAnimator.runtimeAnimatorController;

            avatarAnimator.runtimeAnimatorController = runtimeOverrideController;
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
            SetupOverride(notUsedType, false);
        }

        public ControllerType GetUsedType()
        {
            return usingType;
        }

        public ControllerType GetNotUsedType()
        {
            return notUsedType;
        }

        private void SaveCurrentStartEmotePosition()
        {
            beforeEmoteAvatarPosition = avatar.transform.position;
            beforeEmoteAvatarRotation = avatar.transform.rotation;
            beforeEmoteAvatarScale = avatar.transform.localScale;
        }

        private void RevertToEmotePosition()
        {
            avatar.transform.position = beforeEmoteAvatarPosition;
            avatar.transform.rotation = beforeEmoteAvatarRotation;
            avatar.transform.localScale = beforeEmoteAvatarScale;
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
            SetCustomAnimation(emoteClips[emoteIndex - 1]);
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
                {
                    onNetworkResponseError(webRequest);
                }
                else
                {
                    onNetworkResponse(webRequest);
                }
            }
        }
    }

    public enum ControllerType
    {
        Standing,
        Seated
    };
}