#if VRC_SDK_VRCSDK2
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEngine;
using VRCSDK2;
using GmData = GestureManager.Scripts.Editor.GestureManagerStyles.Data;

namespace GestureManager.Scripts.Editor.Modules.Vrc2
{
    public class ModuleVrc2 : ModuleBase
    {
        private readonly VRC_AvatarDescriptor _avatarDescriptor;

        private readonly int _handGestureLeft = Animator.StringToHash("HandGestureLeft");
        private readonly int _handGestureRight = Animator.StringToHash("HandGestureRight");
        private readonly int _emoteHash = Animator.StringToHash("Emote");

        private ControllerType _usingType;
        private ControllerType _notUsedType;

        private AnimatorOverrideController _originalUsingOverrideController;
        private AnimatorOverrideController _myRuntimeOverrideController;

        private RuntimeAnimatorController _standingRuntimeOverrideControllerPreset;
        private RuntimeAnimatorController _seatedRuntimeOverrideControllerPreset;

        private Dictionary<string, bool> _hasBeenOverridden;

        private AnimationClip _selectingCustomAnim;
        private GmgLayoutHelper.GmgToolbarHeader _toolBar;

        public override bool LateBoneUpdate => true;
        public override bool RequiresConstantRepaint => false;

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
            new AnimationBind("EMOTE8", GmData.EmoteStandingName[7], GmData.EmoteSeatedName[7]),
        };

        public ModuleVrc2(GestureManager manager, VRC_AvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;
        }

        protected override List<string> CheckErrors()
        {
            var errors = base.CheckErrors();
            if (AvatarAnimator != null && !AvatarAnimator.isHuman) errors.Add("- The avatar is not imported as a humanoid rig!");
            if (!_avatarDescriptor.CustomSittingAnims && !_avatarDescriptor.CustomStandingAnims) errors.Add("- The Descriptor doesn't have any kind of controller!");
            return errors;
        }

        public override void InitForAvatar()
        {
            if (_avatarDescriptor.CustomStandingAnims)
                SetupOverride(ControllerType.Standing, true);
            else if (_avatarDescriptor.CustomSittingAnims)
                SetupOverride(ControllerType.Seated, true);
        }

        public override void Unlink()
        {
        }

        public override void SetValues(bool onCustomAnimation, int left, int right, int emote)
        {
            if (onCustomAnimation)
            {
                AvatarAnimator.SetInteger(_handGestureLeft, 8);
                AvatarAnimator.SetInteger(_handGestureRight, 8);
                AvatarAnimator.SetInteger(_emoteHash, 9);
            }
            else if (emote != 0)
            {
                AvatarAnimator.SetInteger(_handGestureLeft, 8);
                AvatarAnimator.SetInteger(_handGestureRight, 8);
                AvatarAnimator.SetInteger(_emoteHash, emote);
            }
            else
            {
                AvatarAnimator.SetInteger(_handGestureLeft, left);
                AvatarAnimator.SetInteger(_handGestureRight, right);
                AvatarAnimator.SetInteger(_emoteHash, emote);
            }
        }

        public override void Update()
        {
        }

        public override AnimationClip GetEmoteByIndex(int emoteIndex)
        {
            return _myRuntimeOverrideController[_emoteBinds[emoteIndex].GetMyName(_usingType)];
        }

        public override AnimationClip GetFinalGestureByIndex(GestureHand hand, int gestureIndex)
        {
            return _myRuntimeOverrideController[_gestureBinds[gestureIndex].GetMyName(_usingType)];
        }

        public override void OnCustomAnimationChange()
        {
            SetupOverride(_usingType, false);
        }

        public override bool HasGestureBeenOverridden(int gestureIndex)
        {
            return _hasBeenOverridden.ContainsKey(_gestureBinds[gestureIndex].GetMyName(_usingType));
        }

        public override void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
            _originalUsingOverrideController[_gestureBinds[gestureIndex].GetOriginalName()] = newAnimation;
            SetupOverride(_usingType, false);
        }

        public override void EditorHeader()
        {
            GUILayout.Label("Using Override: " + GetOverrideController().name + " [" + _usingType + "]");
            GUI.enabled = CanSwitchController();
            if (GUILayout.Button("Switch to " + _notUsedType.ToString().ToLower() + "!")) SwitchType();
            GUI.enabled = true;
        }

        public override void EditorContent()
        {
            GUILayout.Space(15);

            GmgLayoutHelper.MyToolbar(ref _toolBar, new[]
            {
                new GmgLayoutHelper.GmgToolbarRow("Gestures", () =>
                {
                    if (Manager.emote != 0 || Manager.onCustomAnimation)
                    {
                        GUILayout.BeginHorizontal(GestureManagerStyles.EmoteError);
                        GUILayout.Label("Gesture doesn't work while you're playing an emote!");
                        if (GUILayout.Button("Stop!", GestureManagerStyles.GuiGreenButton)) Manager.StopCurrentEmote();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Left Hand", GestureManagerStyles.GuiHandTitle);
                    Manager.left = GestureManagerEditor.OnCheckBoxGuiHand(Manager, GestureHand.Left, Manager.left, position => 0);
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Right Hand", GestureManagerStyles.GuiHandTitle);
                    Manager.right = GestureManagerEditor.OnCheckBoxGuiHand(Manager, GestureHand.Right, Manager.right, position => 0);
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();
                }),
                new GmgLayoutHelper.GmgToolbarRow("Emotes", () =>
                {
                    GUILayout.Label("Emotes", GestureManagerStyles.GuiHandTitle);

                    GestureManagerEditor.OnEmoteButton(Manager, 1);
                    GestureManagerEditor.OnEmoteButton(Manager, 2);
                    GestureManagerEditor.OnEmoteButton(Manager, 3);
                    GestureManagerEditor.OnEmoteButton(Manager, 4);
                    GestureManagerEditor.OnEmoteButton(Manager, 5);
                    GestureManagerEditor.OnEmoteButton(Manager, 6);
                    GestureManagerEditor.OnEmoteButton(Manager, 7);
                    GestureManagerEditor.OnEmoteButton(Manager, 8);
                }),
                new GmgLayoutHelper.GmgToolbarRow("Test Animation", () =>
                {
                    GUILayout.Label("Force animation.", GestureManagerStyles.GuiHandTitle);

                    GUILayout.BeginHorizontal();
                    var lastAnim = _selectingCustomAnim;
                    _selectingCustomAnim = (AnimationClip) EditorGUILayout.ObjectField("Animation: ", _selectingCustomAnim, typeof(AnimationClip), true, null);
                    if (_selectingCustomAnim != lastAnim) Manager.SetCustomAnimation(_selectingCustomAnim);

                    if (Manager.onCustomAnimation)
                    {
                        if (GUILayout.Button("Stop", GestureManagerStyles.GuiGreenButton)) Manager.OnCustomEmoteStop();
                    }
                    else
                    {
                        if (GUILayout.Button("Play", GUILayout.Width(100)))
                        {
                            Manager.StopCurrentEmote();
                            Manager.SetCustomAnimation(_selectingCustomAnim);
                            Manager.OnCustomEmoteStart();
                        }
                    }

                    GUILayout.EndHorizontal();
                })
            });
        }

        private void SetupOverride(ControllerType controllerType, bool saveController)
        {
            Manager.controlDelay = 4;

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
                new KeyValuePair<AnimationClip, AnimationClip>(_myRuntimeOverrideController["[EXTRA] CustomAnimation"], Manager.customAnim)
            };


            var validOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (var pair in GmgAnimatorControllerHelper.GetOverrides(_originalUsingOverrideController).Where(keyValuePair => keyValuePair.Value))
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

            if (saveController) Manager.avatarWasUsing = AvatarAnimator.runtimeAnimatorController;

            AvatarAnimator.runtimeAnimatorController = _myRuntimeOverrideController;
            AvatarAnimator.runtimeAnimatorController.name = controllerName;
        }

        private AnimatorOverrideController GetOverrideController()
        {
            return _originalUsingOverrideController;
        }

        private void SwitchType()
        {
            SetupOverride(_notUsedType, false);
        }

        private bool CanSwitchController()
        {
            return _notUsedType == ControllerType.Seated ? _avatarDescriptor.CustomSittingAnims : _avatarDescriptor.CustomStandingAnims;
        }

        private RuntimeAnimatorController GetStandingRuntimeOverrideControllerPreset()
        {
            if (!_standingRuntimeOverrideControllerPreset)
                _standingRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/StandingEmoteTestingTemplate");
            return _standingRuntimeOverrideControllerPreset;
        }

        private RuntimeAnimatorController GetSeatedRuntimeOverrideControllerPreset()
        {
            if (!_seatedRuntimeOverrideControllerPreset)
                _seatedRuntimeOverrideControllerPreset = Resources.Load<RuntimeAnimatorController>("Vrc2/SeatedEmoteTestingTemplate");
            return _seatedRuntimeOverrideControllerPreset;
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

        public override bool IsInvalid()
        {
            return base.IsInvalid() || !Avatar.activeInHierarchy;
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

        public string GetMyName(ControllerType controller)
        {
            return controller == ControllerType.Standing ? _standingName : _seatedName;
        }

        public string GetOriginalName()
        {
            return _originalName;
        }
    }
}
#endif