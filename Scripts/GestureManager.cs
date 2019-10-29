using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using VRCSDK2;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        private const string VersionUrl = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.version";

        private GameObject avatar;

        public GameObject Avatar
        {
            get { return avatar; }
            private set
            {
                isControllingAnAvatar = value != null;
                avatar = value;
            }
        }

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

        private ControllerType usingType;
        private ControllerType notUsedType;

        private VRC_AvatarDescriptor[] lastCheckedActiveDescriptors;

        private Animator avatarAnimator;

        private AnimatorOverrideController originalUsingOverrideController;
        private AnimatorOverrideController myRuntimeOverrideController;

        private RuntimeAnimatorController avatarWasUsing;

        /*
         * Animation Info...
         */

        private Dictionary<string, bool> hasBeenOverridden;

        [SerializeField] private int instanceId;

        private static readonly int HandGestureLeft = Animator.StringToHash("HandGestureLeft");
        private static readonly int HandGestureRight = Animator.StringToHash("HandGestureRight");
        private static readonly int Emote = Animator.StringToHash("Emote");
        private bool isControllingAnAvatar;

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

        private void Update()
        {
            if (isControllingAnAvatar)
                SetValues();
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
            if (descriptor == null) return false;
            if (!descriptor.gameObject.activeInHierarchy) return false;

            var animator = descriptor.gameObject.GetComponent<Animator>();

            if (animator == null) return false;
            if (!animator.isHuman) return false;
            if (descriptor.CustomSittingAnims == null && descriptor.CustomStandingAnims == null) return false;

            var runtimeAnimatorController = animator.runtimeAnimatorController;

            return runtimeAnimatorController == null ||
                   !runtimeAnimatorController.name.Equals(GetStandingRuntimeOverrideControllerPreset().name) &&
                   !runtimeAnimatorController.name.Equals(GetSeatedRuntimeOverrideControllerPreset().name);
        }

        public AnimatorOverrideController GetOverrideController()
        {
            return originalUsingOverrideController;
        }

        private void ResetCurrentAvatarController()
        {
            if (Avatar == null) return;

            var animator = Avatar.GetComponent<Animator>();
            if (animator == null) return;

            animator.runtimeAnimatorController = avatarWasUsing;
            avatarWasUsing = null;
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
            return hasBeenOverridden.ContainsKey(MyTranslate("F" + (gestureIndex + 1)));
        }

        public void RequestGestureDuplication(int gestureIndex)
        {
            var gestureName = MyTranslate("F" + (gestureIndex + 1));
            var newAnimation = MyAnimationHelper.CloneAnimationAsset(myRuntimeOverrideController[gestureName]);
            var path = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + gestureName, gestureName + ".anim", "anim", "Hi (?)");
            
            if (path.Length == 0)
                return;

            AssetDatabase.CreateAsset(newAnimation, path);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            originalUsingOverrideController[MyTranslate(gestureName)] = newAnimation;

            SetupOverride(usingType, false);
        }

        public void InitForAvatar(VRC_AvatarDescriptor descriptor)
        {
            Avatar = descriptor.gameObject;
            avatarDescriptor = descriptor;

            avatarAnimator = Avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
                avatarAnimator = Avatar.AddComponent<Animator>();

            if (avatarDescriptor.CustomStandingAnims != null)
                SetupOverride(ControllerType.Standing, true);
            else if (avatarDescriptor.CustomSittingAnims != null)
                SetupOverride(ControllerType.Seated, true);
            else
            {
                Avatar = null;
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

                    originalUsingOverrideController = avatarDescriptor.CustomStandingAnims;
                    myRuntimeOverrideController = new AnimatorOverrideController(GetStandingRuntimeOverrideControllerPreset());
                    controllerName = GetStandingRuntimeOverrideControllerPreset().name;

                    break;
                }
                case ControllerType.Seated:
                {
                    usingType = ControllerType.Seated;
                    notUsedType = ControllerType.Standing;

                    originalUsingOverrideController = avatarDescriptor.CustomSittingAnims;
                    myRuntimeOverrideController = new AnimatorOverrideController(GetSeatedRuntimeOverrideControllerPreset());
                    controllerName = GetSeatedRuntimeOverrideControllerPreset().name;

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("controllerType", controllerType, null);
            }

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(myRuntimeOverrideController["[EXTRA] CustomAnimation"], customAnim)
            };

            finalOverride.AddRange(
                MyAnimatorControllerHelper.GetOverrides(
                    originalUsingOverrideController).Where(keyValuePair => keyValuePair.Value != null).Select(
                    controllerOverride => new KeyValuePair<AnimationClip, AnimationClip>(myRuntimeOverrideController[MyTranslate(controllerOverride.Key.name)], controllerOverride.Value
                    )
                )
            );

            hasBeenOverridden = new Dictionary<string, bool>();
            foreach (var valuePair in finalOverride)
            {
                hasBeenOverridden[valuePair.Key.name] = true;
            }

            myRuntimeOverrideController.ApplyOverrides(finalOverride);

            if (saveController)
                avatarWasUsing = avatarAnimator.runtimeAnimatorController;

            avatarAnimator.runtimeAnimatorController = myRuntimeOverrideController;
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
            beforeEmoteAvatarPosition = Avatar.transform.position;
            beforeEmoteAvatarRotation = Avatar.transform.rotation;
            beforeEmoteAvatarScale = Avatar.transform.localScale;
        }

        private void RevertToEmotePosition()
        {
            Avatar.transform.position = beforeEmoteAvatarPosition;
            Avatar.transform.rotation = beforeEmoteAvatarRotation;
            Avatar.transform.localScale = beforeEmoteAvatarScale;
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
                {
                    onNetworkResponseError(webRequest);
                }
                else
                {
                    onNetworkResponse(webRequest);
                }
            }
        }

        /**
         *     STUPID DICTIONARY!!! >.<
         */

        private string MyTranslate(string keyName)
        {
            switch (keyName)
            {
                case "F2":
                case "FIST":
                {
                    return "[GESTURE] Fist";
                }
                case "[GESTURE] Fist":
                {
                    return "FIST";
                }
                case "F3":
                case "HAND" + "OPEN":
                {
                    return "[GESTURE] Open";
                }
                case "[GESTURE] Open":
                {
                    return "HAND" + "OPEN";
                }
                case "F4":
                case "FINGER" + "POINT":
                {
                    return "[GESTURE] FingerPoint";
                }
                case "[GESTURE] FingerPoint":
                {
                    return "FINGER" + "POINT";
                }
                case "F5":
                case "VICTORY":
                {
                    return "[GESTURE] Victory";
                }
                case "[GESTURE] Victory":
                {
                    return "VICTORY";
                }
                case "F6":
                case "ROCK" + "N" + "ROLL":
                {
                    return "[GESTURE] Rock&Roll";
                }
                case "[GESTURE] Rock&Roll":
                {
                    return "ROCK" + "N" + "ROLL";
                }
                case "F7":
                case "HANDGUN":
                {
                    return "[GESTURE] Gun";
                }
                case "[GESTURE] Gun":
                {
                    return "HANDGUN";
                }
                case "F8":
                case "THUMBS" + "UP":
                {
                    return "[GESTURE] ThumbsUp";
                }
                case "[GESTURE] ThumbsUp":
                {
                    return "THUMBS" + "UP";
                }
                case "EMOTE1":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 1] Wave" : "[S - EMOTE 1] Laugh";
                }
                case "EMOTE2":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 2] Clap" : "[S - EMOTE 2] Point";
                }
                case "EMOTE3":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 3] Point" : "[S - EMOTE 3] Raise Hand";
                }
                case "EMOTE4":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 4] Cheer" : "[S - EMOTE 4] Drum";
                }
                case "EMOTE5":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 5] Dance" : "[S - EMOTE 5] Clap";
                }
                case "EMOTE6":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 6] BackFlip" : "[S - EMOTE 6] Angry Fist";
                }
                case "EMOTE7":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 7] Die" : "[S - EMOTE 7] Disbelief";
                }
                case "EMOTE8":
                {
                    return usingType == ControllerType.Standing ? "[P - EMOTE 8] Sad" : "[S - EMOTE 8] Disapprove";
                }
                default:
                {
                    return null;
                }
            }
        }

        private AnimationClip GetEmoteByIndex(int emoteIndex)
        {
            return myRuntimeOverrideController[MyTranslate("EMOTE" + (emoteIndex + 1))];
        }

        private AnimationClip GetFinalGestureByIndex(int gestureIndex)
        {
            return myRuntimeOverrideController[MyTranslate("F" + (gestureIndex + 1))];
        }
    }

    public enum ControllerType
    {
        Standing,
        Seated
    };
}