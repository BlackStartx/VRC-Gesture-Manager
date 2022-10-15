#if VRC_SDK_VRCSDK2
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRCSDK2;
using GmData = GestureManager.Scripts.Editor.GestureManagerStyles.Data;

namespace GestureManager.Scripts.Editor.Modules.Vrc2
{
    public class ModuleVrc2 : ModuleBase
    {
        private readonly VRC_AvatarDescriptor _avatarDescriptor;

        private readonly GUILayoutOption _options = GUILayout.Width(100);
        private readonly int _emoteHash = Animator.StringToHash("Emote");
        private readonly int _handGestureLeft = Animator.StringToHash("HandGestureLeft");
        private readonly int _handGestureRight = Animator.StringToHash("HandGestureRight");

        private int emote;

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
        private static GUIStyle GuiGreenButton => _guiGreenButton ?? (_guiGreenButton = Ggb(new GUIStyleState { textColor = Color.green }));
        private static GUIStyle Ggb(GUIStyleState state) => new GUIStyle(GUI.skin.button) { active = state, normal = state, hover = state, fixedWidth = 100 };
        private RuntimeAnimatorController StandingControllerPreset => _standingControllerPreset ? _standingControllerPreset : _standingControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/StandingEmoteTestingTemplate");
        private RuntimeAnimatorController SeatedControllerPreset => _seatedControllerPreset ? _seatedControllerPreset : _seatedControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/SeatedEmoteTestingTemplate");

        private Dictionary<HumanBodyBones, Quaternion> _lastBoneQuaternions;

        private int _controlDelay;
        private static IEnumerable<HumanBodyBones> Bones => Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private readonly AnimationBind[] _gestureBinds =
        {
            new AnimationBind(null, GmData.GestureNames[0]),
            new AnimationBind("FIST", GmData.GestureNames[1]),
            new AnimationBind("HANDOPEN", GmData.GestureNames[2]),
            new AnimationBind("FINGERPOINT", GmData.GestureNames[3]),
            new AnimationBind("VICTORY", GmData.GestureNames[4]),
            new AnimationBind("ROCKNROLL", GmData.GestureNames[5]),
            new AnimationBind("HANDGUN", GmData.GestureNames[6]),
            new AnimationBind("THUMBSUP", GmData.GestureNames[7])
        };

        private readonly AnimationBind[] _emoteBinds =
        {
            new AnimationBind("EMOTE1", GmData.EmoteStandingName[0], GmData.EmoteSeatedName[0]),
            new AnimationBind("EMOTE2", GmData.EmoteStandingName[1], GmData.EmoteSeatedName[1]),
            new AnimationBind("EMOTE3", GmData.EmoteStandingName[2], GmData.EmoteSeatedName[2]),
            new AnimationBind("EMOTE4", GmData.EmoteStandingName[3], GmData.EmoteSeatedName[3]),
            new AnimationBind("EMOTE5", GmData.EmoteStandingName[4], GmData.EmoteSeatedName[4]),
            new AnimationBind("EMOTE6", GmData.EmoteStandingName[5], GmData.EmoteSeatedName[5]),
            new AnimationBind("EMOTE7", GmData.EmoteStandingName[6], GmData.EmoteSeatedName[6]),
            new AnimationBind("EMOTE8", GmData.EmoteStandingName[7], GmData.EmoteSeatedName[7])
        };

        /**
         *     This dictionary is needed just because I hate the original animation names.
         *     It will just translate the name of the original animation to the my version of the name. 
         */
        private Dictionary<string, string> _myTranslateDictionary;

        private Dictionary<string, string> TranslateDictionary => _myTranslateDictionary ?? (_myTranslateDictionary = new Dictionary<string, string>
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
        });

        private IEnumerable<HumanBodyBones> _whiteListedControlBones;
        private IEnumerable<HumanBodyBones> WhiteListedControlBones => _whiteListedControlBones ?? (_whiteListedControlBones = Bones.Where(bones => !_blackListedControlBones.Contains(bones)));

        private readonly List<HumanBodyBones> _blackListedControlBones = new List<HumanBodyBones>
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

        public ModuleVrc2(GestureManager manager, VRC_AvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor) => _avatarDescriptor = avatarDescriptor;

        public override void Update()
        {
            FetchLastBoneRotation();
            AvatarAnimator.SetInteger(_handGestureLeft, Manager.PlayingCustomAnimation || emote != 0 ? 8 : Left);
            AvatarAnimator.SetInteger(_handGestureRight, Manager.PlayingCustomAnimation || emote != 0 ? 8 : Right);
            AvatarAnimator.SetInteger(_emoteHash, Manager.PlayingCustomAnimation ? 9 : emote);
        }

        public override void LateUpdate()
        {
            if (emote != 0 || Manager.PlayingCustomAnimation) return;

            foreach (var bodyBone in WhiteListedControlBones)
            {
                var boneTransform = AvatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    var boneQuaternion = _lastBoneQuaternions[bodyBone];
                    boneTransform.localRotation = new Quaternion(boneQuaternion.x, boneQuaternion.y, boneQuaternion.z, boneQuaternion.w);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public override void OnDrawGizmos()
        {
        }

        public override void InitForAvatar()
        {
            if (_avatarDescriptor.CustomStandingAnims) SetupOverride(ControllerType.Standing, true);
            else if (_avatarDescriptor.CustomSittingAnims) SetupOverride(ControllerType.Seated, true);
        }

        public override void Unlink()
        {
            if (!AvatarAnimator) return;
            AvatarAnimator.runtimeAnimatorController = _avatarWasUsing;
            _avatarWasUsing = null;
        }

        public override void EditorHeader()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Using Override: " + GetOverrideController().name + " [" + _usingType + "]");
                GUI.enabled = CanSwitchController();
                if (GUILayout.Button("Switch to " + _notUsedType.ToString().ToLower() + "!")) SwitchType();
                GUI.enabled = true;
            }
        }

        public override void EditorContent(object editor, VisualElement element)
        {
            GUILayout.Space(15);

            GmgLayoutHelper.MyToolbar(ref _toolBar, new (string, Action)[]
            {
                ("Gestures", () =>
                {
                    if (emote != 0 || Manager.PlayingCustomAnimation)
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
                            GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Left, Left, RequestGestureDuplication);
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Right Hand", GestureManagerStyles.GuiHandTitle);
                            GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Right, Right, RequestGestureDuplication);
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
                    {
                        if (!(_selectingCustomAnim = GmgLayoutHelper.ObjectField("Animation: ", _selectingCustomAnim, Manager.SetCustomAnimation))) GUI.enabled = false;
                        if (!Manager.PlayingCustomAnimation || emote != 0)
                        {
                            if (GUILayout.Button("Play", _options))
                            {
                                emote = 0;
                                Manager.PlayCustomAnimation(_selectingCustomAnim);
                            }
                        }
                        else if (GUILayout.Button("Stop", GuiGreenButton)) Manager.StopCustomAnimation();

                        GUI.enabled = true;
                    }
                })
            });
        }

        protected override void OnNewLeft(int left) => Left = left;

        protected override void OnNewRight(int right) => Right = right;

        public override string GetGestureTextNameByIndex(int gestureIndex) => GetFinalGestureByIndex(gestureIndex).name;

        public override Animator OnCustomAnimationPlay(AnimationClip animationClip)
        {
            SetupOverride(_usingType, false);
            AvatarAnimator.applyRootMotion = animationClip != null;
            return AvatarAnimator;
        }

        public override bool HasGestureBeenOverridden(int gestureIndex) => _hasBeenOverridden.ContainsKey(_gestureBinds[gestureIndex].GetMyName(_usingType));

        public override bool IsInvalid() => base.IsInvalid() || !Avatar.activeInHierarchy;

        protected override List<string> CheckWarnings()
        {
            var warningList = base.CheckWarnings();
            if (AvatarAnimator != null && !AvatarAnimator.isHuman) warningList.Add("- The avatar has no humanoid rig!\n(Simulation could not match in-app)");
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
                if (emote == current && GUILayout.Button("Stop", GuiGreenButton)) stop();
                if (emote != current && GUILayout.Button("Play", _options)) play(current);
            }
        }

        private void OnEmoteStart(int emoteIndex)
        {
            emote = emoteIndex;
            Manager.PlayCustomAnimation(GetEmoteByIndex(emoteIndex - 1));
        }

        private void OnEmoteStop()
        {
            emote = 0;
            Manager.StopCustomAnimation();
        }

        private void StopCurrentEmote()
        {
            if (emote != 0) OnEmoteStop();
            if (Manager.PlayingCustomAnimation) Manager.StopCustomAnimation();
        }

        private void FetchLastBoneRotation()
        {
            if (emote != 0 || Manager.PlayingCustomAnimation)
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
            foreach (var bodyBone in WhiteListedControlBones)
            {
                var boneTransform = AvatarAnimator.GetBoneTransform(bodyBone);
                try
                {
                    _lastBoneQuaternions[bodyBone] = boneTransform.localRotation;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
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
                new KeyValuePair<AnimationClip, AnimationClip>(_myRuntimeOverrideController["[EXTRA] CustomAnimation"], Manager.customAnim)
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
            var nameString = "[" + fullGestureString.Substring(fullGestureString.IndexOf("]", StringComparison.Ordinal) + 2) + "]";
            var newAnimation = GmgAnimationHelper.CloneAnimationAsset(GetFinalGestureByIndex(gestureIndex));
            var pathString = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + fullGestureString, nameString + ".anim", "anim", "Hi (?)");

            if (pathString.Length == 0) return;

            AssetDatabase.CreateAsset(newAnimation, pathString);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathString);
            AddGestureToOverrideController(gestureIndex, newAnimation);
        }

        private AnimatorOverrideController GetOverrideController() => _originalUsingOverrideController;

        private void SwitchType() => SetupOverride(_notUsedType, false);

        private bool CanSwitchController() => _notUsedType == ControllerType.Seated ? _avatarDescriptor.CustomSittingAnims : _avatarDescriptor.CustomStandingAnims;
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