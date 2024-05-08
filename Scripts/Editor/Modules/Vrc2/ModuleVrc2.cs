#if VRC_SDK_VRCSDK2
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BlackStartX.GestureManager.Data;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRCSDK2;
using GmData = BlackStartX.GestureManager.Editor.Data.GestureManagerStyles.Data;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc2
{
    public class ModuleVrc2 : ModuleBase
    {
        private readonly VRC_AvatarDescriptor _avatarDescriptor;

        private readonly GUILayoutOption _options = GUILayout.Width(100);
        private readonly int _emoteHash = Animator.StringToHash("Emote");
        private readonly int _handGestureLeft = Animator.StringToHash("HandGestureLeft");
        private readonly int _handGestureRight = Animator.StringToHash("HandGestureRight");

        private int _emote;

        private RuntimeAnimatorController _avatarWasUsing;

        private ControllerType _usingType;
        private ControllerType _notUsedType;

        private AnimatorOverrideController _originalUsingOverrideController;
        private AnimatorOverrideController _myRuntimeOverrideController;

        private RuntimeAnimatorController _standingControllerPreset;
        private RuntimeAnimatorController _seatedControllerPreset;

        private Dictionary<string, bool> _hasBeenOverridden;

        private AnimationClip _selectingCustomAnim;
        private GmgLayoutHelper.Toolbar _toolBar;

        private static GUIStyle _guiGreenButton;
        private static GUIStyle GuiGreenButton => _guiGreenButton ??= Ggb(new GUIStyleState { textColor = Color.green });
        private static GUIStyle Ggb(GUIStyleState state) => new(GUI.skin.button) { active = state, normal = state, hover = state, fixedWidth = 100 };
        private RuntimeAnimatorController StandingControllerPreset => !_standingControllerPreset ? _standingControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/StandingEmoteTestingTemplate") : _standingControllerPreset;
        private RuntimeAnimatorController SeatedControllerPreset => !_seatedControllerPreset ? _seatedControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/SeatedEmoteTestingTemplate") : _seatedControllerPreset;
        private bool AnimationField => !(_selectingCustomAnim = GmgLayoutHelper.ObjectField("Animation: ", _selectingCustomAnim, SetCustomAnimation));

        private int _controlDelay;

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private readonly AnimationBind[] _gestureBinds =
        {
            new(null, GmData.GestureNames[0]),
            new("FIST", GmData.GestureNames[1]),
            new("HANDOPEN", GmData.GestureNames[2]),
            new("FINGERPOINT", GmData.GestureNames[3]),
            new("VICTORY", GmData.GestureNames[4]),
            new("ROCKNROLL", GmData.GestureNames[5]),
            new("HANDGUN", GmData.GestureNames[6]),
            new("THUMBSUP", GmData.GestureNames[7])
        };

        private readonly AnimationBind[] _emoteBinds =
        {
            new("EMOTE1", GmData.EmoteStandingName[0], GmData.EmoteSeatedName[0]),
            new("EMOTE2", GmData.EmoteStandingName[1], GmData.EmoteSeatedName[1]),
            new("EMOTE3", GmData.EmoteStandingName[2], GmData.EmoteSeatedName[2]),
            new("EMOTE4", GmData.EmoteStandingName[3], GmData.EmoteSeatedName[3]),
            new("EMOTE5", GmData.EmoteStandingName[4], GmData.EmoteSeatedName[4]),
            new("EMOTE6", GmData.EmoteStandingName[5], GmData.EmoteSeatedName[5]),
            new("EMOTE7", GmData.EmoteStandingName[6], GmData.EmoteSeatedName[6]),
            new("EMOTE8", GmData.EmoteStandingName[7], GmData.EmoteSeatedName[7])
        };

        /**
         *     This dictionary is needed just because I hate the original animation names.
         *     It will just translate the name of the original animation to the my version of the name.
         */
        private Dictionary<string, string> _myTranslateDictionary;

        private Dictionary<string, string> TranslateDictionary => _myTranslateDictionary ??= new Dictionary<string, string>
        {
            { _gestureBinds[1].GetOriginalName(), _gestureBinds[1].GetMyName(_usingType) },
            { _gestureBinds[2].GetOriginalName(), _gestureBinds[2].GetMyName(_usingType) },
            { _gestureBinds[3].GetOriginalName(), _gestureBinds[3].GetMyName(_usingType) },
            { _gestureBinds[4].GetOriginalName(), _gestureBinds[4].GetMyName(_usingType) },
            { _gestureBinds[5].GetOriginalName(), _gestureBinds[5].GetMyName(_usingType) },
            { _gestureBinds[6].GetOriginalName(), _gestureBinds[6].GetMyName(_usingType) },
            { _gestureBinds[7].GetOriginalName(), _gestureBinds[7].GetMyName(_usingType) },

            { _emoteBinds[0].GetOriginalName(), _emoteBinds[0].GetMyName(_usingType) },
            { _emoteBinds[1].GetOriginalName(), _emoteBinds[1].GetMyName(_usingType) },
            { _emoteBinds[2].GetOriginalName(), _emoteBinds[2].GetMyName(_usingType) },
            { _emoteBinds[3].GetOriginalName(), _emoteBinds[3].GetMyName(_usingType) },
            { _emoteBinds[4].GetOriginalName(), _emoteBinds[4].GetMyName(_usingType) },
            { _emoteBinds[5].GetOriginalName(), _emoteBinds[5].GetMyName(_usingType) },
            { _emoteBinds[6].GetOriginalName(), _emoteBinds[6].GetMyName(_usingType) },
            { _emoteBinds[7].GetOriginalName(), _emoteBinds[7].GetMyName(_usingType) }
        };

        protected override List<HumanBodyBones> PoseBones => new()
        {
            HumanBodyBones.Hips,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
            HumanBodyBones.LeftEye,
            HumanBodyBones.RightEye,
            HumanBodyBones.Jaw,
            HumanBodyBones.UpperChest
        };

        public ModuleVrc2(VRC_AvatarDescriptor avatarDescriptor) : base(avatarDescriptor) => _avatarDescriptor = avatarDescriptor;

        public override void Update()
        {
            if (_emote != 0 || PlayingCustomAnimation) _controlDelay = 5;
            else if (_controlDelay <= 0) SavePose(AvatarAnimator);
            else _controlDelay--;

            AvatarAnimator.SetInteger(_handGestureLeft, PlayingCustomAnimation || _emote != 0 ? 8 : Left);
            AvatarAnimator.SetInteger(_handGestureRight, PlayingCustomAnimation || _emote != 0 ? 8 : Right);
            AvatarAnimator.SetInteger(_emoteHash, PlayingCustomAnimation ? 9 : _emote);
        }

        public override void LateUpdate()
        {
            if (_emote != 0 || PlayingCustomAnimation) return;
            SetPose(AvatarAnimator);
        }

        public override void OnDrawGizmos()
        {
        }

        public override void InitForAvatar()
        {
            if (_avatarDescriptor.CustomStandingAnims) SetupOverride(ControllerType.Standing, true);
            else if (_avatarDescriptor.CustomSittingAnims) SetupOverride(ControllerType.Seated, true);
        }

        protected override void Unlink()
        {
            if (!AvatarAnimator) return;
            AvatarAnimator.runtimeAnimatorController = _avatarWasUsing;
            _avatarWasUsing = null;
        }

        public override void EditorHeader()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Using Override: {GetOverrideController().name} [{_usingType}]");
                using (new GmgLayoutHelper.GuiEnabled(CanSwitchController()))
                    if (GUILayout.Button($"Switch to {_notUsedType.ToString().ToLower()}!"))
                        SwitchType();
            }
        }

        public override void EditorContent(object editor, VisualElement element)
        {
            GUILayout.Space(15);

            GmgLayoutHelper.MyToolbar(ref _toolBar, new (string, Action)[]
            {
                ("Gestures", () =>
                {
                    if (_emote != 0 || PlayingCustomAnimation)
                    {
                        using (new GUILayout.HorizontalScope(GestureManagerStyles.EmoteError))
                        {
                            GUILayout.Label("Gesture doesn't work while you're playing an emote!");
                            if (GUILayout.Button("Stop!", GuiGreenButton)) StopCurrentEmote();
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Left Hand", GestureManagerStyles.GuiHandTitle);
                            GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Left, Left, RequestGestureDuplication, HasGestureBeenOverridden);
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Right Hand", GestureManagerStyles.GuiHandTitle);
                            GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Right, Right, RequestGestureDuplication, HasGestureBeenOverridden);
                        }
                    }
                }),
                ("Emotes", () =>
                {
                    GUILayout.Label("Emotes", GestureManagerStyles.GuiHandTitle);

                    OnEmoteButton(1, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(2, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(3, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(4, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(5, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(6, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(7, OnEmoteStart, OnEmoteStop);
                    OnEmoteButton(8, OnEmoteStart, OnEmoteStop);
                }),
                ("Test Animation", () =>
                {
                    GUILayout.Label("Force animation.", GestureManagerStyles.GuiHandTitle);
                    using (new GUILayout.HorizontalScope())
                    using (new GmgLayoutHelper.GuiEnabled(!AnimationField))
                    {
                        if (!PlayingCustomAnimation || _emote != 0)
                        {
                            if (!GUILayout.Button("Play", _options)) return;
                            _emote = 0;
                            PlayCustomAnimation(_selectingCustomAnim);
                        }
                        else if (GUILayout.Button("Stop", GuiGreenButton)) StopCustomAnimation();
                    }
                })
            });
        }

        protected override void OnNewLeft(int left) => Left = left;

        protected override void OnNewRight(int right) => Right = right;

        public override string GetGestureTextNameByIndex(int gestureIndex) => GetFinalGestureByIndex(gestureIndex).name;

        protected override Animator OnCustomAnimationPlay(AnimationClip clip)
        {
            SetupOverride(_usingType, false);
            AvatarAnimator.applyRootMotion = clip;
            return AvatarAnimator;
        }

        public override bool IsInvalid() => base.IsInvalid() || !Avatar.activeInHierarchy;

        protected override List<string> CheckWarnings()
        {
            var warningList = base.CheckWarnings();
            if (!AvatarAnimator) return warningList;
            if (!AvatarAnimator.isHuman) warningList.Add("- The avatar has no humanoid rig!\n(Simulation could not match in-game)");
            return warningList;
        }

        protected override List<string> CheckErrors()
        {
            var errorList = base.CheckErrors();
            if (!_avatarDescriptor.CustomSittingAnims && !_avatarDescriptor.CustomStandingAnims) errorList.Add("- The Descriptor doesn't have any kind of controller!");
            return errorList;
        }

        private AnimationClip GetFinalGestureByIndex(int gestureIndex) => _myRuntimeOverrideController[_gestureBinds[gestureIndex].GetMyName(_usingType)];

        private AnimationClip GetEmoteByIndex(int emoteIndex) => _myRuntimeOverrideController[_emoteBinds[emoteIndex].GetMyName(_usingType)];

        private bool HasGestureBeenOverridden(int gestureIndex) => _hasBeenOverridden.ContainsKey(_gestureBinds[gestureIndex].GetMyName(_usingType));

        private void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
            _originalUsingOverrideController[_gestureBinds[gestureIndex].GetOriginalName()] = newAnimation;
            SetupOverride(_usingType, false);
        }

        private void OnEmoteButton(int current, Action<int> play, Action stop)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(GetEmoteByIndex(current - 1).name);
                if (_emote == current && GUILayout.Button("Stop", GuiGreenButton)) stop();
                if (_emote != current && GUILayout.Button("Play", _options)) play(current);
            }
        }

        private void OnEmoteStart(int emoteIndex)
        {
            _emote = emoteIndex;
            PlayCustomAnimation(GetEmoteByIndex(emoteIndex - 1));
        }

        private void OnEmoteStop()
        {
            _emote = 0;
            StopCustomAnimation();
        }

        private void StopCurrentEmote()
        {
            if (_emote != 0) OnEmoteStop();
            if (PlayingCustomAnimation) StopCustomAnimation();
        }

        private void SetupOverride(ControllerType controllerType, bool saveController)
        {
            _controlDelay = 4;

            var controllerPreset = controllerType == ControllerType.Standing ? StandingControllerPreset : SeatedControllerPreset;
            _usingType = controllerType == ControllerType.Standing ? ControllerType.Standing : ControllerType.Seated;
            _notUsedType = controllerType == ControllerType.Standing ? ControllerType.Seated : ControllerType.Standing;
            _originalUsingOverrideController = controllerType == ControllerType.Standing ? _avatarDescriptor.CustomStandingAnims : _avatarDescriptor.CustomSittingAnims;
            _myRuntimeOverrideController = new AnimatorOverrideController(controllerPreset);

            _myTranslateDictionary = null;

            var finalOverride = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new(_myRuntimeOverrideController["[EXTRA] CustomAnimation"], CustomAnim)
            };

            var validOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (var pair in GmgAnimatorControllerHelper.GetOverrides(_originalUsingOverrideController).Where(keyValuePair => keyValuePair.Value))
            {
                try
                {
                    validOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(_myRuntimeOverrideController[TranslateDictionary[pair.Key.name]], pair.Value));
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

            if (saveController) _avatarWasUsing = AvatarAnimator.runtimeAnimatorController;

            AvatarAnimator.Rebind();
            AvatarAnimator.runtimeAnimatorController = _myRuntimeOverrideController;
            AvatarAnimator.runtimeAnimatorController.name = controllerPreset.name;
        }

        private void RequestGestureDuplication(int gestureIndex)
        {
            var fullGestureString = GetGestureTextNameByIndex(gestureIndex);
            var nameString = $"[{fullGestureString[(fullGestureString.IndexOf("]", StringComparison.Ordinal) + 2)..]}]";
            var newAnimation = GmgAnimationHelper.CloneAnimationAsset(GetFinalGestureByIndex(gestureIndex));
            var pathString = EditorUtility.SaveFilePanelInProject($"Creating Gesture: {fullGestureString}", $"{nameString}.anim", "anim", "Hi (?)");

            if (pathString.Length == 0) return;

            AssetDatabase.CreateAsset(newAnimation, pathString);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathString);
            AddGestureToOverrideController(gestureIndex, newAnimation);
        }

        private AnimatorOverrideController GetOverrideController() => _originalUsingOverrideController;

        private void SwitchType() => SetupOverride(_notUsedType, false);

        private bool CanSwitchController() => _notUsedType == ControllerType.Seated ? _avatarDescriptor.CustomSittingAnims : _avatarDescriptor.CustomStandingAnims;
    }

    public static class GmgAnimatorControllerHelper
    {
        public static IEnumerable<KeyValuePair<AnimationClip, AnimationClip>> GetOverrides(AnimatorOverrideController overrideController)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            return overrides;
        }
    }

    public static class GmgAnimationHelper
    {
        public static AnimationClip CloneAnimationAsset(AnimationClip toClone)
        {
            var toRet = new AnimationClip { name = toClone.name, frameRate = toClone.frameRate, legacy = toClone.legacy, localBounds = toClone.localBounds, wrapMode = toClone.wrapMode };

            foreach (var binding in AnimationUtility.GetCurveBindings(toClone)) AnimationUtility.SetEditorCurve(toRet, binding, AnimationUtility.GetEditorCurve(toClone, binding));
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(toClone)) AnimationUtility.SetObjectReferenceCurve(toRet, binding, Keyframes(toClone, binding));

            return toRet;
        }

        private static ObjectReferenceKeyframe[] Keyframes(AnimationClip toClone, EditorCurveBinding binding) => AnimationUtility.GetObjectReferenceCurve(toClone, binding);
    }

    public enum ControllerType
    {
        Standing,
        Seated
    }

    public class AnimationBind
    {
        private readonly string _originalName;
        private readonly string _standingName;
        private readonly string _seatedName;

        public AnimationBind(string originalName, string standingName, string seatedName)
        {
            _originalName = originalName;
            _standingName = standingName;
            _seatedName = seatedName;
        }

        public AnimationBind(string originalName, string gestureName) : this(originalName, gestureName, gestureName)
        {
        }

        public string GetMyName(ControllerType controller) => controller == ControllerType.Standing ? _standingName : _seatedName;

        public string GetOriginalName() => _originalName;
    }
}
#endif