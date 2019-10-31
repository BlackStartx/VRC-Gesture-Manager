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

        // Used to control the rotation of the model bones during the animation playtime. (like outside the play mode)
        private Dictionary<HumanBodyBones, Quaternion> lastBoneQuaternions;
        private int controlDelay;

        private IEnumerable<HumanBodyBones> whiteListedControlBones;
        private readonly List<HumanBodyBones> blackListedControlBones = new List<HumanBodyBones>()
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
        };

        // I'm using the ReShaper editor... and those type are really annoying! >.<
        // ReSharper disable StringLiteralTypo
        private readonly AnimationBind[] gestureBinds =
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

        private readonly AnimationBind[] emoteBinds =
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
            FetchLastBoneRotation();

            if (isControllingAnAvatar)
                SetValues();
        }

        private void LateUpdate()
        {
            if (emote != 0 || onCustomAnimation) return;

            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = avatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    var boneRotation = lastBoneQuaternions[bodyBone];
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
                controlDelay = 5;
                return;
            }

            if (controlDelay > 0)
            {
                controlDelay--;
                return;
            }

            lastBoneQuaternions = new Dictionary<HumanBodyBones, Quaternion>();
            foreach (var bodyBone in GetWhiteListedControlBones())
            {
                var bone = avatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    lastBoneQuaternions[bodyBone] = bone.localRotation;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private IEnumerable<HumanBodyBones> GetWhiteListedControlBones()
        {
            return whiteListedControlBones ?? (whiteListedControlBones = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().Where(bones => !blackListedControlBones.Contains(bones)));
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
            return hasBeenOverridden.ContainsKey(gestureBinds[gestureIndex].GetMyName(usingType));
        }

        public void RequestGestureDuplication(int gestureIndex)
        {
            var fullGestureName = gestureBinds[gestureIndex].GetMyName(usingType);
            var gestureName = "[" + fullGestureName.Substring(fullGestureName.IndexOf("]", StringComparison.Ordinal) + 2) + "]";
            var newAnimation = MyAnimationHelper.CloneAnimationAsset(myRuntimeOverrideController[fullGestureName]);
            var path = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + fullGestureName, gestureName + ".anim", "anim", "Hi (?)");

            if (path.Length == 0)
                return;

            AssetDatabase.CreateAsset(newAnimation, path);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            originalUsingOverrideController[gestureBinds[gestureIndex].GetOriginalName()] = newAnimation;

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
            controlDelay = 4;
            
            string controllerName;
            if (controllerType == ControllerType.Standing)
            {
                usingType = ControllerType.Standing;
                notUsedType = ControllerType.Seated;

                originalUsingOverrideController = avatarDescriptor.CustomStandingAnims;
                myRuntimeOverrideController = new AnimatorOverrideController(GetStandingRuntimeOverrideControllerPreset());
                controllerName = GetStandingRuntimeOverrideControllerPreset().name;
            }
            else
            {
                usingType = ControllerType.Seated;
                notUsedType = ControllerType.Standing;

                originalUsingOverrideController = avatarDescriptor.CustomSittingAnims;
                myRuntimeOverrideController = new AnimatorOverrideController(GetSeatedRuntimeOverrideControllerPreset());
                controllerName = GetSeatedRuntimeOverrideControllerPreset().name;
            }

            GenerateDictionary();

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(myRuntimeOverrideController["[EXTRA] CustomAnimation"], customAnim)
            };

            finalOverride.AddRange(
                MyAnimatorControllerHelper.GetOverrides(
                    originalUsingOverrideController).Where(keyValuePair => keyValuePair.Value != null).Select(
                    controllerOverride => new KeyValuePair<AnimationClip, AnimationClip>(myRuntimeOverrideController[myTranslateDictionary[controllerOverride.Key.name]], controllerOverride.Value
                    )
                )
            );

            hasBeenOverridden = new Dictionary<string, bool>();
            foreach (var valuePair in finalOverride) hasBeenOverridden[valuePair.Key.name] = true;

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
                    onNetworkResponseError(webRequest);
                else
                    onNetworkResponse(webRequest);
            }
        }

        /**
         *     This dictionary is needed just because I hate the original animation names.
         *     It will just translate the name of the original animation to the my version of the name. 
         */
        private Dictionary<string, string> myTranslateDictionary;

        private void GenerateDictionary()
        {
            myTranslateDictionary = new Dictionary<string, string>()
            {
                {gestureBinds[1].GetOriginalName(), gestureBinds[1].GetMyName(usingType)},
                {gestureBinds[2].GetOriginalName(), gestureBinds[2].GetMyName(usingType)},
                {gestureBinds[3].GetOriginalName(), gestureBinds[3].GetMyName(usingType)},
                {gestureBinds[4].GetOriginalName(), gestureBinds[4].GetMyName(usingType)},
                {gestureBinds[5].GetOriginalName(), gestureBinds[5].GetMyName(usingType)},
                {gestureBinds[6].GetOriginalName(), gestureBinds[6].GetMyName(usingType)},
                {gestureBinds[7].GetOriginalName(), gestureBinds[7].GetMyName(usingType)},

                {emoteBinds[0].GetOriginalName(), emoteBinds[0].GetMyName(usingType)},
                {emoteBinds[1].GetOriginalName(), emoteBinds[1].GetMyName(usingType)},
                {emoteBinds[2].GetOriginalName(), emoteBinds[2].GetMyName(usingType)},
                {emoteBinds[3].GetOriginalName(), emoteBinds[3].GetMyName(usingType)},
                {emoteBinds[4].GetOriginalName(), emoteBinds[4].GetMyName(usingType)},
                {emoteBinds[5].GetOriginalName(), emoteBinds[5].GetMyName(usingType)},
                {emoteBinds[6].GetOriginalName(), emoteBinds[6].GetMyName(usingType)},
                {emoteBinds[7].GetOriginalName(), emoteBinds[7].GetMyName(usingType)},
            };
        }

        private AnimationClip GetEmoteByIndex(int emoteIndex)
        {
            return myRuntimeOverrideController[emoteBinds[emoteIndex].GetMyName(usingType)];
        }

        private AnimationClip GetFinalGestureByIndex(int gestureIndex)
        {
            return myRuntimeOverrideController[gestureBinds[gestureIndex].GetMyName(usingType)];
        }
    }

    public enum ControllerType
    {
        Standing,
        Seated
    };

    public class AnimationBind
    {
        private readonly string originalName;
        private readonly string myStandingName;
        private readonly string mySeatedName;

        public AnimationBind(string originalName, string myStandingName, string mySeatedName)
        {
            this.originalName = originalName;
            this.myStandingName = myStandingName;
            this.mySeatedName = mySeatedName;
        }

        public AnimationBind(string originalName, string myName) : this(originalName, myName, myName)
        {
        }

        public string GetMyName(ControllerType controller)
        {
            return controller == ControllerType.Standing ? myStandingName : mySeatedName;
        }

        public string GetOriginalName()
        {
            return originalName;
        }
    }
}