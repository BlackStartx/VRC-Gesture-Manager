#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class ModuleVrc3 : ModuleBase
    {
        private readonly VRCAvatarDescriptor _avatarDescriptor;

        private readonly int _handGestureLeft = Animator.StringToHash("GestureLeft");
        private readonly int _handGestureRight = Animator.StringToHash("GestureRight");

        private GameObject _editAvatar;
        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _externalOutput;
        private AnimatorControllerPlayable[] _humanAnimatorPlayables;
        private RadialWeightController[] _weightControllers;
        private Rect _rect;
        private bool _hooked;

        internal readonly Vrc3ParamBool Debug = new Vrc3ParamBool();
        internal readonly Vrc3ParamBool Editing;

        private RadialMenu _radialMenu;
        private RadialMenu RadialMenu => _radialMenu ?? (_radialMenu = new RadialMenu(this));
        private IEnumerable<AnimationClip> OriginalClips => _avatarClips.Where(clip => !clip.name.StartsWith("proxy_"));
        private VRCExpressionsMenu Menu => _avatarDescriptor.expressionsMenu;
        private VRCExpressionParameters Parameters => _avatarDescriptor.expressionParameters;

        public override bool LateBoneUpdate => false;
        public override bool RequiresConstantRepaint => true;

        private readonly Dictionary<VRCAvatarDescriptor.AnimLayerType, RadialWeightController> _fromBlend = new Dictionary<VRCAvatarDescriptor.AnimLayerType, RadialWeightController>();
        private readonly HashSet<AnimationClip> _avatarClips = new HashSet<AnimationClip>();
        private RadialDescription _radialDescription;

        private float _prevSeated, _prevTPoseCalibration, _prevIKPoseCalibration;

        public ModuleVrc3(GestureManager manager, VRCAvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;
            Editing = new Vrc3ParamBool(OnEditModeChange);
        }

        public override void Update()
        {
            if (Editing.State && (!_editAvatar || Avatar.activeSelf)) DisableEditMode();
            foreach (var weightController in _weightControllers) weightController.Update();
        }

        public override void InitForAvatar()
        {
            StartVrcHooks();

            AvatarAnimator.applyRootMotion = false;
            AvatarAnimator.runtimeAnimatorController = null;
            AvatarAnimator.updateMode = AnimatorUpdateMode.Normal;
            AvatarAnimator.cullingMode = AnimatorCullingMode.CullCompletely;
            DestroyGraphs();

            var layerList = _avatarDescriptor.baseAnimationLayers.ToList();
            layerList.AddRange(_avatarDescriptor.specialAnimationLayers);
            layerList.Sort(ModuleVrc3Styles.Data.LayerSort);

            _playableGraph = PlayableGraph.Create("Gesture Manager 3.1");
            var externalOutput = AnimationPlayableOutput.Create(_playableGraph, "Gesture Manager", AvatarAnimator);
            var playableMixer = AnimationLayerMixerPlayable.Create(_playableGraph, layerList.Count + 1);
            externalOutput.SetSourcePlayable(playableMixer);

            _fromBlend.Clear();
            _avatarClips.Clear();
            _weightControllers = new RadialWeightController[layerList.Count];
            _humanAnimatorPlayables = new AnimatorControllerPlayable[layerList.Count];

            for (var i = 0; i < layerList.Count; i++)
            {
                var vrcAnimLayer = layerList[i];
                var iGraph = i + 1;

                if (vrcAnimLayer.animatorController)
                    foreach (var clip in vrcAnimLayer.animatorController.animationClips)
                        _avatarClips.Add(clip);

                var isFx = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.FX;
                var isAdd = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Additive;
                var isPose = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.IKPose || vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.TPose;
                var isAction = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Sitting || vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Action;
                var limit = isPose || isAction;

                var controller = vrcAnimLayer.animatorController ? vrcAnimLayer.animatorController : ModuleVrc3Styles.Data.ControllerOf[vrcAnimLayer.type];
                var mask = vrcAnimLayer.isDefault || isFx ? ModuleVrc3Styles.Data.MaskOf[vrcAnimLayer.type] : vrcAnimLayer.mask;

                if (!controller) continue;

                _humanAnimatorPlayables[i] = AnimatorControllerPlayable.Create(_playableGraph, Vrc3ProxyOverride.OverrideController(controller));
                _weightControllers[i] = new RadialWeightController(playableMixer, iGraph);
                for (var j = 0; j < _humanAnimatorPlayables[i].GetLayerCount(); j++) _humanAnimatorPlayables[i].SetLayerWeight(j, 1f);

                playableMixer.ConnectInput(iGraph, _humanAnimatorPlayables[i], 0, 1);
                _fromBlend[vrcAnimLayer.type] = _weightControllers[i];

                if (limit) playableMixer.SetInputWeight(iGraph, 0f);
                if (isAdd) playableMixer.SetLayerAdditive((uint) iGraph, true);
                if (mask) playableMixer.SetLayerMaskFromAvatarMask((uint) iGraph, mask);
            }

            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _playableGraph.Play();
            _playableGraph.Evaluate(0f);

            RadialMenu.Set(AvatarAnimator, Menu, Parameters, _humanAnimatorPlayables);
            RadialMenu.GetParam("TrackingType")?.InternalSet(1f);
            RadialMenu.GetParam("Upright")?.InternalSet(1f);
            RadialMenu.GetParam("Grounded")?.InternalSet(1f);
            RadialMenu.GetParam("VelocityX")?.Amplify(-7f);
            RadialMenu.GetParam("VelocityZ")?.Amplify(7f);
            RadialMenu.GetParam("AvatarVersion")?.InternalSet(3f);
            RadialMenu.GetParam("Seated")?.OnChange(OnSeatedModeChange);
        }

        public override void Unlink()
        {
            if (Editing.State) DisableEditMode();
            if (AvatarAnimator) ForgetAvatar();
            StopVrcHooks();
        }

        public override void SetValues(bool onCustomAnimation, int left, int right, int emote)
        {
            if (Editing.State) return;
            AvatarAnimator.SetInteger(_handGestureLeft, left);
            AvatarAnimator.SetInteger(_handGestureRight, right);
        }

        protected override List<string> CheckErrors()
        {
            var errors = base.CheckErrors();
            errors.AddRange(RadialMenuUtility.CheckErrors(_avatarDescriptor.expressionsMenu, _avatarDescriptor.expressionParameters));
            return errors;
        }

        public override void EditorContent(VisualElement element)
        {
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

            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);
            GUILayout.Label("Radial Menu", GestureManagerStyles.GuiHandTitle);

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(RadialMenu.Size));
            var extraSize = RadialMenu.Render(element, GmgLayoutHelper.GetLastRect(ref _rect)) - RadialMenu.Size;
            if (extraSize > 0) GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(extraSize));

            if (_radialDescription != null) ShowRadialDescription();
        }

        public override AnimationClip GetFinalGestureByIndex(GestureHand hand, int gestureIndex)
        {
            return ModuleVrc3Styles.Data.GestureClips[gestureIndex];
        }

        public override AnimationClip GetEmoteByIndex(int emoteIndex) => null;

        public override bool HasGestureBeenOverridden(int gesture) => true;

        public override void OnCustomAnimationChange()
        {
        }

        public override void EditorHeader()
        {
        }

        public override void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
        }

        /*
         *  Functions
         */

        private void DisableEditMode() => Editing.Set(RadialMenu, 0f);

        public void RemoveRadialDescription() => _radialDescription = null;

        public void SetRadialDescription(string text, string link, string url) => _radialDescription = new RadialDescription(text, link, url);

        private void ShowRadialDescription()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GestureManagerStyles.EmoteError);
            GUILayout.FlexibleSpace();
            GUILayout.Label(_radialDescription.Text);

            var style = EditorGUIUtility.isProSkin ? ModuleVrc3Styles.UrlPro : ModuleVrc3Styles.Url;
            if (GUILayout.Button(_radialDescription.Link, style)) Application.OpenURL(_radialDescription.Url);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        public void NoExpressionRefresh()
        {
            if (Editing.State) return;
            if (Menu) ResetAvatar();
        }

        public void ResetAvatar()
        {
            AvatarAnimator.Rebind();
            InitForAvatar();
        }

        private void ForgetAvatar()
        {
            AvatarAnimator.Rebind();
            RadialMenu.ForgetParams();
            DestroyGraphs();
        }

        private void DestroyGraphs()
        {
            if (_playableGraph.IsValid()) _playableGraph.Destroy();
            if (AvatarAnimator.playableGraph.IsValid()) AvatarAnimator.playableGraph.Destroy();
        }

        private void OnEditModeChange(bool editMode)
        {
            if (editMode)
            {
                ForgetAvatar();
                _editAvatar = UnityEngine.Object.Instantiate(Avatar);
                _editAvatar.name = Avatar.name + " (Edit-Mode)";

                Avatar.SetActive(false);
                Avatar.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                _editAvatar.GetOrAddComponent<Animator>().runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(OriginalClips);
                _radialMenu.MainMenuPrefab();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            else
            {
                if (_editAvatar) UnityEngine.Object.DestroyImmediate(_editAvatar);

                Avatar.SetActive(true);
                Avatar.hideFlags = HideFlags.None;

                AvatarAnimator.Update(1f);
                AvatarAnimator.runtimeAnimatorController = null;
                AvatarAnimator.Update(1f);
                InitForAvatar();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
        }

        private void OnSeatedModeChange(Vrc3Param param, float seated)
        {
            _fromBlend[VRCAvatarDescriptor.AnimLayerType.Sitting].Set(seated);
        }

        /*
         * Vrc Hooks
         */

        private void StartVrcHooks()
        {
            if (_hooked) return;

            VRC_AvatarParameterDriver.Initialize += AvatarParameterDriverInit;
            VRC_PlayableLayerControl.Initialize += PlayableLayerControlInit;
            _hooked = true;
        }

        private void StopVrcHooks()
        {
            if (!_hooked) return;

            VRC_AvatarParameterDriver.Initialize -= AvatarParameterDriverInit;
            VRC_PlayableLayerControl.Initialize -= PlayableLayerControlInit;
            _hooked = false;
        }

        private void PlayableLayerControlInit(VRC_PlayableLayerControl playableLayerControl) => playableLayerControl.ApplySettings += PlayableLayerControlSettings;

        private void AvatarParameterDriverInit(VRC_AvatarParameterDriver avatarParameterDriver) => avatarParameterDriver.ApplySettings += AvatarParameterDriverSettings;

        private void PlayableLayerControlSettings(VRC_PlayableLayerControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            _fromBlend[ModuleVrc3Styles.Data.ToLayer[control.layer]].Start(control);
        }

        private void AvatarParameterDriverSettings(VRC_AvatarParameterDriver driver, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            foreach (var parameter in driver.parameters)
            {
                var param = RadialMenu.GetParam(parameter.name);
                if (param == null) continue;

                switch (parameter.type)
                {
                    case VRC_AvatarParameterDriver.ChangeType.Set:
                        param.Set(RadialMenu, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Add:
                        param.Add(RadialMenu, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Random:
                        param.Random(RadialMenu, parameter.valueMin, parameter.valueMax, parameter.chance);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
#endif