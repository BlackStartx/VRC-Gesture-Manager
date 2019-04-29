using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRCSDK2;

public class GestureManager : MonoBehaviour
{
    public enum ControllerType
    {
        STANDING,
        SEATED
    };

    private static string version = "1.0.0";
    private static string versionUrl = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.version";

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

    private string[] gestureBaseNames = new string[] { "...", "FIST", "HANDOPEN", "FINGERPOINT", "VICTORY", "ROCKNROLL", "HANDGUN", "THUMBSUP" };
    private string[] emoteBaseNames = new string[] { "EMOTE1", "EMOTE2", "EMOTE3", "EMOTE4", "EMOTE5", "EMOTE6", "EMOTE7", "EMOTE8" };

    private ControllerType usingType;
    private ControllerType notUsedType;

    /**
     * An array that contains the final Emote clips.
     */
    private AnimationClip[] emoteClips = new AnimationClip[8];

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
    private AnimationClip[] gestureClips = new AnimationClip[8];

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
        this.left = 0;
        this.right = 0;
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
    private string[] emoteNames = new string[8];

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
    private string[] gestureNames = new string[8];
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

    [SerializeField] int instanceID = 0;

    void Awake()
    {
        if (instanceID != GetInstanceID())
        {
            if (instanceID == 0)
            {
                instanceID = GetInstanceID();
            }
            else
            {
                instanceID = GetInstanceID();
                if (instanceID < 0)
                {
                    this.avatar = null;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (avatar != null)
        {
            SetValues();
        }
    }

    void OnEnable()
    {
        if (avatar == null)
        {
            VRCSDK2.VRC_AvatarDescriptor validDescriptor = GetValidDescriptor();
            if (validDescriptor != null)
                InitForAvatar(validDescriptor);
        }
    }

    void OnDisable()
    {
        ResetCurrentAvatarController();
        avatar = null;
        avatarDescriptor = null;
    }

    public string GetCurrentVersion()
    {
        return version;
    }

    public void CheckForUpdates(OnNetworkResponseError onNetworkResponseError, OnNetworkResponse onNetworkResponse)
    {
        if (currentlyCheckingForUpdates)
        {
            Debug.Log("Gesture Manager: Already looking for updates...");
            return;
        }
        currentlyCheckingForUpdates = true;
        StartCoroutine(GetRequest(versionUrl, (error) =>
        {
            onNetworkResponseError(error);
            currentlyCheckingForUpdates = false;
        }, (response) =>
        {
            onNetworkResponse(response);
            currentlyCheckingForUpdates = false;
        }));
    }

    VRCSDK2.VRC_AvatarDescriptor GetValidDescriptor()
    {
        foreach (VRC_AvatarDescriptor descriptor in VRC.Tools.FindSceneObjectsOfTypeAll<VRCSDK2.VRC_AvatarDescriptor>())
        {
            if (descriptor.gameObject.activeInHierarchy)
            {
                Animator animator = descriptor.gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    return descriptor;
                }
                else
                {
                    RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
                    if (runtimeAnimatorController == null)
                    {
                        return descriptor;
                    }
                    else
                    {
                        if (!runtimeAnimatorController.name.Equals(GetStandingRuntimeOverrideControllerPreset().name) && !runtimeAnimatorController.name.Equals(GetSeatedRuntimeOverrideControllerPreset().name))
                        {
                            return descriptor;
                        }
                    }
                }
            }
        }

        return null;
    }

    void FetchRuntimeOverrideAnimationNames()
    {
        if (runtimeOverrideController != null)
        {
            for (int i = 0; i < 8; i++)
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

    void ResetCurrentAvatarController()
    {
        if (avatar != null)
        {
            Animator animator = avatar.GetComponent<Animator>();
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

    void InitForAvatar(VRCSDK2.VRC_AvatarDescriptor descriptor)
    {
        avatar = descriptor.gameObject;
        avatarDescriptor = descriptor;

        avatarAnimator = avatar.GetComponent<Animator>();
        if (avatarAnimator == null)
            avatarAnimator = avatar.AddComponent<Animator>();

        SetupOverride(ControllerType.STANDING);
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

    void SetupOverride(ControllerType controllerType)
    {
        string controllerName = null;
        switch (controllerType)
        {
            case ControllerType.STANDING:
            {
                usingType = ControllerType.STANDING;
                notUsedType = ControllerType.SEATED;

                overrideController = avatarDescriptor.CustomStandingAnims;
                runtimeOverrideController = new AnimatorOverrideController(GetStandingRuntimeOverrideControllerPreset());
                controllerName = GetStandingRuntimeOverrideControllerPreset().name;

                break;
            }
            case ControllerType.SEATED:
            {
                usingType = ControllerType.SEATED;
                notUsedType = ControllerType.STANDING;

                overrideController = avatarDescriptor.CustomSittingAnims;
                runtimeOverrideController = new AnimatorOverrideController(GetSeatedRuntimeOverrideControllerPreset());
                controllerName = GetSeatedRuntimeOverrideControllerPreset().name;

                break;
            }
        }

        FetchRuntimeOverrideAnimationNames();

        List<KeyValuePair<AnimationClip, AnimationClip>> finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[customAnimName], customAnim));

        /**
         * Gestures...
         */

        int[] gestureIndex = new[] {0, 39, 42, 40, 44, 41, 45, 43};

        for (int index = 0; index < 8; index++)
        {
            AnimationClip overrideClip = runtimeOverrideController[gestureNames[index]];
            AnimationClip clip = overrideController.animationClips[gestureIndex[index]];
            if (!clip.name.Equals(gestureBaseNames[index]))
                overrideClip = clip;
            finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[gestureNames[index]], overrideClip));
            gestureClips[index] = overrideClip;
        }

        /**
         * Emotes...
         */

        int[] emoteIndex = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        for (int index = 0; index < 8; index++)
        {
            AnimationClip overrideClip = runtimeOverrideController[emoteNames[index]];
            AnimationClip clip = overrideController.animationClips[emoteIndex[index]];
            if (!clip.name.Equals(emoteBaseNames[index]))
                overrideClip = clip;
            finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(runtimeOverrideController[emoteNames[index]], overrideClip));
            emoteClips[index] = overrideClip;
        }

        runtimeOverrideController.ApplyOverrides(finalOverride);

        avatarWasUsing = avatarAnimator.runtimeAnimatorController;

        avatarAnimator.runtimeAnimatorController = runtimeOverrideController;
        avatarAnimator.runtimeAnimatorController.name = controllerName;
    }

    void SetValues()
    {
        int lastEmote = avatarAnimator.GetInteger("Emote");
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
        this.SetupOverride(notUsedType);
    }

    public ControllerType GetUsedType()
    {
        return this.usingType;
    }

    public ControllerType GetNotUsedType()
    {
        return this.notUsedType;
    }

    public void ApplyCustomAnim()
    {
        List<KeyValuePair<AnimationClip, AnimationClip>> finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        finalOverride.Add(new KeyValuePair<AnimationClip, AnimationClip>(currentCustomAnim, customAnim));

        runtimeOverrideController.ApplyOverrides(finalOverride);
        avatarAnimator.runtimeAnimatorController = runtimeOverrideController;
    }

    public void SaveCurrentStartEmotePosition()
    {
        this.beforeEmoteAvatarPosition = avatar.transform.position;
        this.beforeEmoteAvatarRotation = avatar.transform.rotation;
        this.beforeEmoteAvatarScale = avatar.transform.localScale;
    }

    public void RevertoToEmotePosition()
    {
        avatar.transform.position = this.beforeEmoteAvatarPosition;
        avatar.transform.rotation = this.beforeEmoteAvatarRotation;
        avatar.transform.localScale = this.beforeEmoteAvatarScale;
    }

    public void RevertoToOriginPosition()
    {
        avatar.transform.position = new Vector3(0, 0, 0);
        avatar.transform.rotation = new Quaternion(0, 0, 0, 0);
        avatar.transform.localScale = this.beforeEmoteAvatarScale;
    }

    public void SetCustomAnimation(AnimationClip clip)
    {
        this.customAnim = clip;

        SetupOverride(this.GetUsedType());
    }

    /**
     *  LISTENERS
     *  LISTENERS
     *  LISTENERS
     */

    public void OnEmoteStop()
    {
        this.emote = 0;
        this.avatarAnimator.applyRootMotion = false;
        RevertoToEmotePosition();
    }

    public void OnEmoteStart(int emote)
    {
        this.emote = emote;
        this.avatarAnimator.applyRootMotion = true;
        SetCustomAnimation(emoteClips[emote - 1]);
        SaveCurrentStartEmotePosition();
    }

    public void OnCustomEmoteStop()
    {
        this.onCustomAnimation = false;
        this.avatarAnimator.applyRootMotion = false;
        SetCustomAnimation(null);
        RevertoToEmotePosition();
    }

    public void OnCustomEmoteStart()
    {
        this.avatarAnimator.applyRootMotion = true;
        SaveCurrentStartEmotePosition();
        this.onCustomAnimation = true;
    }

    /**
     * Async
     */

    public delegate void OnNetworkResponseError(UnityWebRequest error);

    public delegate void OnNetworkResponse(UnityWebRequest response);

    IEnumerator GetRequest(string uri, OnNetworkResponseError onNetworkResponseError, OnNetworkResponse onNetworkResponse)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
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

    /**
     * Debugging Stuff...
     */

    void IterClip(AnimatorOverrideController controller, string text)
    {
        foreach (AnimationClip clip in controller.animationClips)
        {
            Debug.Log(text + clip.name);
        }
    }

    void IterClip(Dictionary<string, string> controller, string text)
    {
        foreach (KeyValuePair<string, string> keyValuePair in controller)
        {
            Debug.Log(text + keyValuePair.Key + " -> " + keyValuePair.Value);
        }
    }

    void IterClip(string[] list, string text)
    {
        foreach (string clip in list)
        {
            Debug.Log(text + clip);
        }
    }

    void IterClip(AnimationClip[] clips, string text)
    {
        foreach (AnimationClip clip in clips)
        {
            if (clip != null)
                Debug.Log(text + clip);
            else
                Debug.Log(text + "null");
        }
    }
}
