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

        private DummyMode _dummyMode;
        internal GameObject DummyAvatar;
        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _externalOutput;
        private AnimatorControllerPlayable[] _humanAnimatorPlayables;
        private RadialWeightController[] _weightControllers;
        private bool _hooked;

        internal readonly Vrc3ParamBool Debug = new Vrc3ParamBool();
        internal readonly Vrc3ParamBool Dummy;

        internal readonly Dictionary<string, Vrc3Param> Params = new Dictionary<string, Vrc3Param>();
        internal readonly Dictionary<UnityEditor.Editor, RadialMenu> RadialMenus = new Dictionary<UnityEditor.Editor, RadialMenu>();

        private IEnumerable<AnimationClip> OriginalClips => _avatarClips.Where(clip => !clip.name.StartsWith("proxy_"));
        private VRCExpressionsMenu Menu => _avatarDescriptor.expressionsMenu;
        private VRCExpressionParameters Parameters => _avatarDescriptor.expressionParameters;

        public override bool LateBoneUpdate => false;
        public override bool RequiresConstantRepaint => true;
        public string ExitDummyText => "Exit " + _dummyMode + "-Mode";

        private readonly Dictionary<VRCAvatarDescriptor.AnimLayerType, RadialWeightController> _fromBlend = new Dictionary<VRCAvatarDescriptor.AnimLayerType, RadialWeightController>();
        private readonly HashSet<AnimationClip> _avatarClips = new HashSet<AnimationClip>();

        private float _prevSeated, _prevTPoseCalibration, _prevIKPoseCalibration;

        private AnimationClip _selectingCustomAnim;
        private GmgLayoutHelper.GmgToolbarHeader _toolBar;

        public ModuleVrc3(GestureManager manager, VRCAvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;
            Dummy = new Vrc3ParamBool(OnDummyModeChange);
        }

        public override void Update()
        {
            if (_dummyMode != DummyMode.None && (!DummyAvatar || Avatar.activeSelf)) DisableDummy();
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

            _playableGraph = PlayableGraph.Create("Gesture Manager 3.2");
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

                playableMixer.ConnectInput(iGraph, _humanAnimatorPlayables[i], 0, 1);
                _fromBlend[vrcAnimLayer.type] = _weightControllers[i];

                if (limit) playableMixer.SetInputWeight(iGraph, 0f);
                if (isAdd) playableMixer.SetLayerAdditive((uint)iGraph, true);
                if (mask) playableMixer.SetLayerMaskFromAvatarMask((uint)iGraph, mask);
            }

            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _playableGraph.Play();
            _playableGraph.Evaluate(0f);

            foreach (var radialMenu in RadialMenus.Values) radialMenu.Set(Menu);
            InitParams(AvatarAnimator, Parameters, _humanAnimatorPlayables);

            GetParam("TrackingType")?.InternalSet(1f);
            GetParam("Upright")?.InternalSet(1f);
            GetParam("Grounded")?.InternalSet(1f);
            GetParam("VelocityX")?.Amplify(-7f);
            GetParam("VelocityZ")?.Amplify(7f);
            GetParam("AvatarVersion")?.InternalSet(3f);
            GetParam("Seated")?.OnChange(OnSeatedModeChange);
        }

        public override void Unlink()
        {
            if (Dummy.State) DisableDummy();
            if (AvatarAnimator) ForgetAvatar();
            StopVrcHooks();
        }

        public override void SetValues(bool onCustomAnimation, int left, int right, int emote)
        {
            if (Dummy.State) return;
            AvatarAnimator.SetInteger(_handGestureLeft, left);
            AvatarAnimator.SetInteger(_handGestureRight, right);
        }

        protected override List<string> CheckErrors()
        {
            var errors = base.CheckErrors();
            errors.AddRange(RadialMenuUtility.CheckErrors(_avatarDescriptor.expressionsMenu, _avatarDescriptor.expressionParameters));
            return errors;
        }

        public override void EditorContent(object editor, VisualElement element)
        {
            GmgLayoutHelper.MyToolbar(ref _toolBar, new[]
            {
                new GmgLayoutHelper.GmgToolbarRow("Gestures", () =>
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
                }),
                new GmgLayoutHelper.GmgToolbarRow("Test Animation", () =>
                {
                    GUILayout.Label("Test animation", GestureManagerStyles.GuiHandTitle);
                    GUILayout.Label("Use this window to preview any animation in your project.", GestureManagerStyles.SubHeader);
                    GUILayout.Label("You can preview Emotes or Gestures.", GestureManagerStyles.SubHeader);

                    GUILayout.Space(18);

                    GUILayout.BeginHorizontal();
                    _selectingCustomAnim = GmgLayoutHelper.ObjectField("Animation: ", _selectingCustomAnim, Manager.SetCustomAnimation);
                    var isEditMode = _dummyMode == DummyMode.Edit;

                    GUI.enabled = _selectingCustomAnim && !isEditMode;
                    if (Manager.OnCustomAnimation)
                    {
                        if (GUILayout.Button("Stop", GestureManagerStyles.GuiGreenButton)) Manager.StopCustomAnimation();
                    }
                    else if (GUILayout.Button("Play", GUILayout.Width(100))) Manager.PlayCustomAnimation(_selectingCustomAnim);

                    GUI.enabled = true;

                    GUILayout.EndHorizontal();

                    GUILayout.Space(18);
                    GUILayout.Label(isEditMode ? "Cannot test animation while in Edit-Mode." : "Will reset the simulation.", GestureManagerStyles.TextError);
                    GUILayout.Space(18);
                })
            });

            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);
            GUILayout.Label("Radial Menu", GestureManagerStyles.GuiHandTitle);

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(RadialMenu.Size));
            var menu = GetOrCreateRadial(editor as UnityEditor.Editor);
            var extraSize = menu.Render(element, GmgLayoutHelper.GetLastRect(ref menu.Rect)) - RadialMenu.Size;
            if (extraSize > 0) GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(extraSize));
            menu.ShowRadialDescription();
        }

        public override AnimationClip GetFinalGestureByIndex(GestureHand hand, int gestureIndex)
        {
            return ModuleVrc3Styles.Data.GestureClips[gestureIndex];
        }

        public override AnimationClip GetEmoteByIndex(int emoteIndex) => null;

        public override bool HasGestureBeenOverridden(int gesture) => true;

        public override Animator OnCustomAnimationPlay(AnimationClip clip)
        {
            if (!clip)
            {
                RemoveDummy(true);
                return null;
            }

            if (!DummyAvatar) CreateDummy("[Testing]", DummyMode.Test);
            var animator = DummyAvatar.GetOrAddComponent<Animator>();
            animator.runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(clip);
            return animator;
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

        private RadialMenu GetOrCreateRadial(UnityEditor.Editor editor)
        {
            if (RadialMenus.ContainsKey(editor)) return RadialMenus[editor];

            RadialMenus[editor] = new RadialMenu(this);
            RadialMenus[editor].Set(Menu);
            return RadialMenus[editor];
        }

        private void CreateDummy(string dummyName, DummyMode mode)
        {
            ForgetAvatar();
            Dummy.State = true;
            DummyAvatar = UnityEngine.Object.Instantiate(Avatar);
            DummyAvatar.name = Avatar.name + " " + dummyName;
            _dummyMode = mode;
            Avatar.SetActive(false);
            Avatar.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            EditorApplication.DirtyHierarchyWindowSorting();
            foreach (var radialMenu in RadialMenus.Values) radialMenu.MainMenuPrefab();
        }

        private void RemoveDummy(bool reset)
        {
            if (DummyAvatar) UnityEngine.Object.DestroyImmediate(DummyAvatar);
            Dummy.State = false;
            Avatar.SetActive(true);
            Avatar.hideFlags = HideFlags.None;
            EditorApplication.DirtyHierarchyWindowSorting();
            if (!reset) return;

            AvatarAnimator.Update(1f);
            AvatarAnimator.runtimeAnimatorController = null;
            AvatarAnimator.Update(1f);
            InitForAvatar();
        }

        private void OnDummyModeChange(bool state)
        {
            if (state) return;
            DisableDummy();
        }

        private void DisableDummy()
        {
            if (!Avatar) return;

            switch (_dummyMode)
            {
                case DummyMode.Edit:
                    break;
                case DummyMode.Test:
                    Manager.StopCustomAnimation();
                    break;
                case DummyMode.None:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            RemoveDummy(true);
            Dummy.State = false;
            _dummyMode = DummyMode.None;
        }

        internal void EnableEditMode()
        {
            CreateDummy("[Edit-Mode]", DummyMode.Edit);
            DummyAvatar.GetOrAddComponent<Animator>().runtimeAnimatorController = GmgAnimatorControllerHelper.CreateControllerWith(OriginalClips);
        }

        public void NoExpressionRefresh()
        {
            if (Dummy.State) return;
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
            ForgetParams();
            DestroyGraphs();
        }

        private void DestroyGraphs()
        {
            if (_playableGraph.IsValid()) _playableGraph.Destroy();
            if (AvatarAnimator.playableGraph.IsValid()) AvatarAnimator.playableGraph.Destroy();
        }

        private void OnSeatedModeChange(Vrc3Param param, float seated)
        {
            _fromBlend[VRCAvatarDescriptor.AnimLayerType.Sitting].Set(seated);
        }

        /*
         * Params
         */

        private void InitParams(Animator animator, VRCExpressionParameters parameters, IEnumerable<AnimatorControllerPlayable> animatorControllerPlayables)
        {
            Params.Clear();
            foreach (var controller in animatorControllerPlayables)
            foreach (var parameter in RadialMenuUtility.GetParameters(controller))
                Params[parameter.name] = RadialMenuUtility.CreateParamFromController(animator, parameter, controller);

            if (!parameters) return;
            foreach (var parameter in parameters.parameters)
                if (!Params.ContainsKey(parameter.name))
                    Params[parameter.name] = RadialMenuUtility.CreateParamFromNothing(parameter);

            foreach (var parameter in parameters.parameters)
                Params[parameter.name].InternalSet(parameter.defaultValue);
        }

        public Vrc3Param GetParam(string pName)
        {
            if (pName == null) return null;
            Params.TryGetValue(pName, out var param);
            return param;
        }

        private void ForgetParams() => Params.Clear();

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
                var param = GetParam(parameter.name);
                if (param == null) continue;

                switch (parameter.type)
                {
                    case VRC_AvatarParameterDriver.ChangeType.Set:
                        param.Set(RadialMenus.Values, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Add:
                        param.Add(RadialMenus.Values, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Random:
                        param.Random(RadialMenus.Values, parameter.valueMin, parameter.valueMax, parameter.chance);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        private enum DummyMode
        {
            None,
            Edit,
            Test
        }
    }
}
#endif