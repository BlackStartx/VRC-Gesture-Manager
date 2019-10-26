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
        public enum ControllerType
        {
            Standing,
            Seated
        };

        private const string Version = "1.0.0";
        private const string VersionUrl = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.version";

        public GameObject avatar;
        public int right, left, emote;
        public bool onCustomAnimation;

        public bool currentlyCheckingForUpdates = false;

        public AnimationClip customAnim;
        public AnimationClip currentCustomAnim;

        private Vector3 beforeEmoteAvatarScale;
        private Vector3 beforeEmoteAvatarPosition;
        private Quaternion beforeEmoteAvatarRotation;

        private RuntimeAnimatorController standingRuntimeOverrideControllerPreset;
        private RuntimeAnimatorController seatedRuntimeOverrideControllerPreset;

        private VRC_AvatarDescriptor avatarDescriptor;

        private readonly string[] gestureBaseNames = { "...", "FIST", "HANDOPEN", "FINGERPOINT", "VICTORY", "ROCKNROLL", "HANDGUN", "THUMBSUP" };
        private readonly string[] emoteBaseNames = { "EMOTE1", "EMOTE2", "EMOTE3", "EMOTE4", "EMOTE5", "EMOTE6", "EMOTE7", "EMOTE8" };

        private ControllerType usingType;
        private ControllerType notUsedType;

        private VRC_AvatarDescriptor[] lastCheckedActiveDescriptors;

        /**
         * An array that contains the final Emote clips.
         */
        private readonly AnimationClip[] emoteClips = new AnimationClip[8];

        /**
         * An array that contains the final Gesture clips.
         *
         *  0 = Idle
         *  1 = Fist
         *  2 = Open
         *  3 = Finger
         *  4 = Victory
         *  5 = Rock&Roll
         *  6 = HandGun
         *  7 = ThumbsUp
         */
        private readonly AnimationClip[] gestureClips = new AnimationClip[8];

        public void StopCurrentEmote()
        {
            if (emote != 0)
            {
                OnEmoteStop();
            }

            if (onCustomAnimation)
            {
                OnCustomEmoteStop();
            }
        }

        public void StopCurrentGesture()
        {
            left = 0;
            right = 0;
        }

        /**
         *  Only Original Names.
         *
         *  0 = [EMOTE 1] (... / ---)
         *  1 = [EMOTE 2] (... / ---)
         *  2 = [EMOTE 3] (... / ---)
         *  3 = [EMOTE 4] (... / ---)
         *  4 = [EMOTE 5] (... / ---)
         *  5 = [EMOTE 6] (... / ---)
         *  6 = [EMOTE 7] (... / ---)
         *  7 = [EMOTE 8] (... / ---)
         */
        private readonly string[] emoteNames = new string[8];

        /**
         *  0 = Idle
         *  1 = Fist
         *  2 = Open
         *  3 = Finger
         *  4 = Victory
         *  5 = Rock&Roll
         *  6 = HandGun
         *  7 = ThumbsUp
         */
        private readonly string[] gestureNames = new string[8];
        private string customAnimName;

        private Animator avatarAnimator;

        /**
         *  The Clip Of the Animation or Override.
         *
         *  IDLE            -> ?
         *  PRONEIDLE       -> ?
         *  EMOTE1          -> ?
         *  EMOTE2          -> ?
         *  EMOTE3          -> ?
         *  EMOTE4          -> ?
         *  EMOTE5          -> ?
         *  EMOTE6          -> ?
         *  EMOTE7          -> ?
         *  EMOTE8          -> ?
         *  FALL            -> ?
         *  PRONEFWD        -> ?
         *  CROUCHIDLE      -> ?
         *  CROUCHWALKFWD   -> ?
         *  CROUCHWALKRT    -> ?
         *  SPRINTFWD       -> ?
         *  RUNFWD          -> ?
         *  WALFFWD         -> ?
         *  WALFBACK        -> ?
         *  RUNBACK         -> ?
         *  STRAFERT        -> ?
         *  ...             -> ?
         *  FIST            -> ?
         *  FINGERPOINT     -> ?
         *  ROCKNROLL       -> ?
         *  HANDOPEN        -> ?
         *  THUMBSUP        -> ?
         *  VICTORY         -> ?
         *  HANDGUN         -> ?
         */
        private AnimatorOverrideController overrideController;

        /**
         *  [EMOTE 1] ?
         *  [EMOTE 2] ?
         *  [EMOTE 3] ?
         *  [EMOTE 4] ?
         *  [EMOTE 5] ?
         *  [EMOTE 6] ?
         *  [EMOTE 7] ?
         *  [EMOTE 8] ?
         *  [EXTRA] CustomAnimaiton
         *  [GESTURE] Fist
         *  [GESTURE] Fingerpoint
         *  [GESTURE] Rock&Roll
         *  [GESTURE] Open
         *  [GESTURE] ThumbsUp
         *  [GESTURE] Victory
         *  [GESTURE] Gun
         *  [GESTURE] Idle
         */
        private AnimatorOverrideController runtimeOverrideController;

        private RuntimeAnimatorController avatarWasUsing;

        [SerializeField] private int instanceId;

        private void Awake()
        {
            if (instanceId != GetInstanceID())
            {
                if (instanceId == 0)
                {
                    instanceId = GetInstanceID();
                }
                else
                {
                    instanceId = GetInstanceID();
                    if (instanceId < 0)
                    {
                        avatar = null;
                    }
                }
            }
        }

        private void Update()
        {
            if (avatar != null)
            {
                SetValues();
            }
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

        public void UnlinkFromAvatar()
        {
            ResetCurrentAvatarController();
            avatar = null;
            avatarDescriptor = null;
        }

        public string GetCurrentVersion()
        {
            return Version;
        }

        public void CheckForUpdates(OnNetworkResponseError onNetworkResponseError, OnNetworkResponse onNetworkResponse)
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

        public VRC_AvatarDescriptor[] GetLastCheckedActiveDescriptors()
        {
            return lastCheckedActiveDescriptors;
        }

        public void CheckActiveDescriptors()
        {
            lastCheckedActiveDescriptors = VRC.Tools.FindSceneObjectsOfTypeAll<VRC_AvatarDescriptor>();
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
                                if (!runtimeAnimatorController.name.Equals(GetStandingRuntimeOverrideControllerPreset()
                                        .name) && !runtimeAnimatorController.name.Equals(
                                        GetSeatedRuntimeOverrideControllerPreset().name))
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

                customAnimName = runtimeOverrideController.animationClips[8].name;

                gestureNames[0] = runtimeOverrideController.animationClips[16].name; // Idle        V
                gestureNames[1] = runtimeOverrideController.animationClips[9].name;  // Close       V
                gestureNames[2] = runtimeOverrideController.animationClips[12].name; // Open        V
                gestureNames[3] = runtimeOverrideController.animationClips[10].name; // Finger      V
                gestureNames[4] = runtimeOverrideController.animationClips[14].name; // Victory     V
                gestureNames[5] = runtimeOverrideController.animationClips[11].name; // Rock&Roll   V
                gestureNames[6] = runtimeOverrideController.animationClips[15].name; // HandGun     V
                gestureNames[7] = runtimeOverrideController.animationClips[13].name; // ThumbsUp    V
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

        RuntimeAnimatorController GetStandingRuntimeOverrideControllerPreset()
        {
            if (standingRuntimeOverrideControllerPreset == null)
                standingRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("StandingEmoteTestingTemplate");
            return standingRuntimeOverrideControllerPreset;
        }

        RuntimeAnimatorController GetSeatedRuntimeOverrideControllerPreset()
        {
            if (seatedRuntimeOverrideControllerPreset == null)
                seatedRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("SeatedEmoteTestingTemplate");
            return seatedRuntimeOverrideControllerPreset;
        }

        void SetupOverride(ControllerType controllerType, bool saveController)
        {
            string controllerName = null;
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
            }

            FetchRuntimeOverrideAnimationNames();

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[customAnimName], customAnim));

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

            var emoteIndex = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 };

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

            if(saveController)
                avatarWasUsing = avatarAnimator.runtimeAnimatorController;

            avatarAnimator.runtimeAnimatorController = runtimeOverrideController;
            avatarAnimator.runtimeAnimatorController.name = controllerName;
        }

        private void SetValues()
        {
            if (onCustomAnimation)
            {
                avatarAnimator.SetInteger("HandGestureLeft", 8);
                avatarAnimator.SetInteger("HandGestureRight", 8);
                avatarAnimator.SetInteger("Emote", 9);
            }
            else if(emote != 0)
            {
                avatarAnimator.SetInteger("HandGestureLeft", 8);
                avatarAnimator.SetInteger("HandGestureRight", 8);
                avatarAnimator.SetInteger("Emote", emote);
            }
            else
            {
                avatarAnimator.SetInteger("HandGestureLeft", left);
                avatarAnimator.SetInteger("HandGestureRight", right);
                avatarAnimator.SetInteger("Emote", emote);
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

        public void RevertToOriginPosition()
        {
            avatar.transform.position = new Vector3(0, 0, 0);
            avatar.transform.rotation = new Quaternion(0, 0, 0, 0);
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

        public void OnEmoteStart(int emote)
        {
            this.emote = emote;
            avatarAnimator.applyRootMotion = true;
            SetCustomAnimation(emoteClips[emote - 1]);
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

        public delegate void OnNetworkResponseError(UnityWebRequest error);

        public delegate void OnNetworkResponse(UnityWebRequest response);

        IEnumerator GetRequest(string uri, OnNetworkResponseError onNetworkResponseError, OnNetworkResponse onNetworkResponse)
        {
            using (var webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
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
}
