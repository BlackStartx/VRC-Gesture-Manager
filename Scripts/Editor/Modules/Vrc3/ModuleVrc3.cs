#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.Vrc3Debug;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Experimental.UIElements;
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

        private ModuleVrc3DebugWindow _debugWindow;

        private DummyMode _dummyMode;
        internal GameObject DummyAvatar;

        private PlayableGraph _playableGraph;
        private AnimatorControllerWeight[] _animatorWeights;
        private AnimatorControllerPlayable[] _animatorPlayables;
        private bool _hooked;
        private bool _ignoreBrokenLayers;

        internal readonly Vrc3ParamBool Dummy;
        internal readonly Vrc3ParamBool PoseT;
        internal readonly Vrc3ParamBool PoseIK;

        internal readonly Dictionary<UnityEditor.Editor, RadialMenu> RadialMenus = new Dictionary<UnityEditor.Editor, RadialMenu>();
        internal readonly Dictionary<string, Vrc3Param> Params = new Dictionary<string, Vrc3Param>();

        internal readonly Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorControllerWeight> FromBlend = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorControllerWeight>();
        private readonly Dictionary<UnityEditor.Editor, Vrc3WeightController> _weightControllers = new Dictionary<UnityEditor.Editor, Vrc3WeightController>();

        internal bool LocomotionDisabled;
        internal bool PoseSpace;

        internal readonly Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> TrackingControls = new Dictionary<string, VRC_AnimatorTrackingControl.TrackingType>
        {
            { "Head", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Left Hand", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Right Hand", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Hip", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Left Foot", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Right Foot", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Left Fingers", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Right Fingers", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Eye & Eyelid", VRC_AnimatorTrackingControl.TrackingType.Tracking },
            { "Mouth & Jaw", VRC_AnimatorTrackingControl.TrackingType.Tracking }
        };

        private readonly List<VRCAvatarDescriptor.AnimLayerType> _brokenLayers = new List<VRCAvatarDescriptor.AnimLayerType>();
        private readonly HashSet<AnimationClip> _avatarClips = new HashSet<AnimationClip>();

        private AnimationClip _selectingCustomAnim;
        internal int DebugToolBar;

        private IEnumerable<AnimationClip> OriginalClips => _avatarClips.Where(clip => !clip.name.StartsWith("proxy_"));
        private VRCExpressionsMenu Menu => _avatarDescriptor.expressionsMenu;
        private VRCExpressionParameters Parameters => _avatarDescriptor.expressionParameters;
        public string ExitDummyText => "Exit " + _dummyMode + "-Mode";
        public override bool LateBoneUpdate => false;
        public override bool RequiresConstantRepaint => true;

        public ModuleVrc3(GestureManager manager, VRCAvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;
            Dummy = new Vrc3ParamBool(OnDummyModeChange);
            PoseT = new Vrc3ParamBool(OnTPoseChange);
            PoseIK = new Vrc3ParamBool(OnIKPoseChange);
        }

        public override void Update()
        {
            if (_dummyMode != DummyMode.None && (!DummyAvatar || Avatar.activeSelf)) Dummy.ShutDown();
            foreach (var weightController in _animatorWeights) weightController.Update();
        }

        public override void InitForAvatar()
        {
            StartVrcHooks();

            AvatarAnimator.applyRootMotion = false;
            AvatarAnimator.runtimeAnimatorController = null;
            AvatarAnimator.updateMode = AnimatorUpdateMode.Normal;
            AvatarAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            DestroyGraphs();

            var layerList = _avatarDescriptor.baseAnimationLayers.ToList();
            layerList.AddRange(_avatarDescriptor.specialAnimationLayers);
            layerList.Sort(ModuleVrc3Styles.Data.LayerSort);

            _playableGraph = PlayableGraph.Create("Gesture Manager 3.3");
            var externalOutput = AnimationPlayableOutput.Create(_playableGraph, "Gesture Manager", AvatarAnimator);
            var playableMixer = AnimationLayerMixerPlayable.Create(_playableGraph, layerList.Count + 1);
            externalOutput.SetSourcePlayable(playableMixer);

            FromBlend.Clear();
            _avatarClips.Clear();
            _brokenLayers.Clear();
            _animatorWeights = new AnimatorControllerWeight[layerList.Count];
            _animatorPlayables = new AnimatorControllerPlayable[layerList.Count];

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

                var controller = vrcAnimLayer.animatorController ? vrcAnimLayer.animatorController : RequestBuiltInController(vrcAnimLayer.type);
                var mask = vrcAnimLayer.isDefault || isFx ? ModuleVrc3Styles.Data.MaskOf[vrcAnimLayer.type] : vrcAnimLayer.mask;

                _animatorPlayables[i] = AnimatorControllerPlayable.Create(_playableGraph, Vrc3ProxyOverride.OverrideController(controller));
                _animatorWeights[i] = new AnimatorControllerWeight(playableMixer, _animatorPlayables[i], iGraph);

                playableMixer.ConnectInput(iGraph, _animatorPlayables[i], 0, 1);
                FromBlend[vrcAnimLayer.type] = _animatorWeights[i];

                if (limit) playableMixer.SetInputWeight(iGraph, 0f);
                if (isAdd) playableMixer.SetLayerAdditive((uint)iGraph, true);
                if (mask) playableMixer.SetLayerMaskFromAvatarMask((uint)iGraph, mask);
            }

            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _playableGraph.Play();
            _playableGraph.Evaluate(0f);

            foreach (var radialMenu in RadialMenus.Values) radialMenu.Set(Menu);
            InitParams(AvatarAnimator, Parameters, _animatorPlayables);

            GetParam("TrackingType")?.InternalSet(1f);
            GetParam("Upright")?.InternalSet(1f);
            GetParam("Grounded")?.InternalSet(1f);
            GetParam("VelocityX")?.Amplify(-7f);
            GetParam("VelocityY")?.Amplify(-22f);
            GetParam("VelocityZ")?.Amplify(7f);
            GetParam("AvatarVersion")?.InternalSet(3f);
            GetParam("GestureLeftWeight")?.InternalSet(1f);
            GetParam("GestureRightWeight")?.InternalSet(1f);
            GetParam("Seated")?.SetOnChange(OnSeatedModeChange);
        }

        public override void Unlink()
        {
            if (_debugWindow) _debugWindow.Close();
            if (Dummy.State) Dummy.ShutDown();
            if (AvatarAnimator) ForgetAvatar();
            StopVisualElements();
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
            var menu = GetOrCreateRadial(editor as UnityEditor.Editor);
            var weightController = GetOrCreateWeightController(editor as UnityEditor.Editor);
            GmgLayoutHelper.MyToolbar(ref menu.ToolBar, new[]
            {
                new GmgLayoutHelper.GmgToolbarRow("Gestures", () => EditorContentGesture(element, weightController)),
                new GmgLayoutHelper.GmgToolbarRow("Test Animation", EditorContentTesting),
                new GmgLayoutHelper.GmgToolbarRow("Debug", EditorContentDebug)
            });

            menu.CheckCondition(menu.ToolBar.GetSelected());
            weightController.CheckCondition(menu.ToolBar.GetSelected());

            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);

            if (menu.ToolBar.GetSelected() != 2) ShowRadialMenu(menu, element);
            else if (!_debugWindow) ShowDebugMenu();
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
            if (_brokenLayers.Count == 0 || _ignoreBrokenLayers) return;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            ShowWarnings();
            GUI.backgroundColor = color;
        }

        public override void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
        }

        /*
         * Editor GUI
         */

        private void EditorContentGesture(VisualElement element, Vrc3WeightController weightController)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = _dummyMode == DummyMode.None;

            GUILayout.BeginVertical();
            GUILayout.Label("Left Hand", GestureManagerStyles.GuiHandTitle);
            weightController.RenderLeft(element);
            Manager.left = GestureManagerEditor.OnCheckBoxGuiHand(Manager, GestureHand.Left, Manager.left, position => 0, false);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Right Hand", GestureManagerStyles.GuiHandTitle);
            weightController.RenderRight(element);
            Manager.right = GestureManagerEditor.OnCheckBoxGuiHand(Manager, GestureHand.Right, Manager.right, position => 0, false);
            GUILayout.EndVertical();

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void EditorContentTesting()
        {
            GUILayout.Label("Test animation", GestureManagerStyles.GuiHandTitle);
            GUILayout.Label("Use this window to preview any animation in your project.", GestureManagerStyles.SubHeader);
            GUILayout.Label("You can preview Emotes or Gestures.", GestureManagerStyles.SubHeader);

            GUILayout.Space(23);

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

            GUILayout.Space(23);
            GUILayout.Label(isEditMode ? "Cannot test animation while in Edit-Mode." : "Will reset the simulation.", GestureManagerStyles.TextError);
            GUILayout.Space(23);
        }

        private void EditorContentDebug()
        {
            GUILayout.Label("Debug", GestureManagerStyles.GuiHandTitle);

            GUILayout.Label(_debugWindow ? ModuleVrc3DebugWindow.Text.W.Subtitle : ModuleVrc3DebugWindow.Text.D.Subtitle, GestureManagerStyles.SubHeader);
            GUILayout.Space(_debugWindow ? 16 : 15);
            if (_debugWindow) GUILayout.Label(ModuleVrc3DebugWindow.Text.W.Message, GestureManagerStyles.SubHeader);
            else DebugToolBar = ModuleVrc3DebugWindow.Static.DebugToolbar(DebugToolBar);
            GUILayout.Space(_debugWindow ? 16 : 15);
            GUILayout.Label(_debugWindow ? ModuleVrc3DebugWindow.Text.W.Hint : ModuleVrc3DebugWindow.Text.D.Hint, GestureManagerStyles.SubHeader);

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_debugWindow ? ModuleVrc3DebugWindow.Text.W.Button : ModuleVrc3DebugWindow.Text.D.Button, GUILayout.Height(30))) SwitchDebugView();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
        }

        /*
         * Functions
         */

        private static string NameOf(UnityEngine.Object o) => Application.dataPath + AssetDatabase.GetAssetPath(o).Substring(6);

        private static bool CheckIntegrity(FileInfo file1, FileInfo file2)
        {
            if (file1.Length != file2.Length) return false;

            using (var stream1 = file1.OpenRead())
            using (var stream2 = file2.OpenRead())
                for (var i = 0; i < file1.Length; i++)
                    if (stream1.ReadByte() != stream2.ReadByte())
                        return false;

            return true;
        }

        private RuntimeAnimatorController RequestBuiltInController(VRCAvatarDescriptor.AnimLayerType type)
        {
            var controller = ModuleVrc3Styles.Data.ControllerOf(type);
            var restoreAsset = ModuleVrc3Styles.Data.RestoreOf(type);

            if (!controller || !CheckIntegrity(new FileInfo(NameOf(controller)), new FileInfo(NameOf(restoreAsset)))) _brokenLayers.Add(type);
            return controller ? controller : new AnimatorController();
        }

        private void StopVisualElements()
        {
            foreach (var weight in _weightControllers.Values) weight.StopRendering();
            foreach (var menu in RadialMenus.Values) menu.StopRendering();
        }

        private void SwitchDebugView()
        {
            if (_debugWindow)
            {
                _debugWindow.Close();
                _debugWindow = null;
            }
            else _debugWindow = ModuleVrc3DebugWindow.Create(this);
        }

        private static void ShowRadialMenu(RadialMenu menu, VisualElement element)
        {
            GUILayout.Label("Radial Menu", GestureManagerStyles.GuiHandTitle);

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(RadialMenu.Size));
            menu.Render(element, GmgLayoutHelper.GetLastRect(ref menu.Rect));
            menu.ShowRadialDescription();
        }

        private void ShowDebugMenu()
        {
            DebugContext(Screen.width - 60, false);
            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);
        }

        internal void DebugContext(float width, bool fullScreen)
        {
            if (!DummyAvatar) ModuleVrc3DebugWindow.Static.DebugLayout(this, width, fullScreen, _animatorPlayables);
            else ModuleVrc3DebugWindow.Static.DummyLayout(_dummyMode.ToString());
        }

        private void WarningTitle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label("WARNING", GestureManagerStyles.TextWarningHeader);
            if (GUILayout.Button("X", GUI.skin.label, GUILayout.Width(15))) _ignoreBrokenLayers = true;
            GUILayout.EndHorizontal();
        }

        private void ShowWarnings()
        {
            GUILayout.BeginVertical(GestureManagerStyles.EmoteError);
            GUILayout.Space(2);
            WarningTitle();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Some default Animator Controllers has changed:", GestureManagerStyles.TextWarning);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fix and Restore Default Controllers")) RestoreDefaultControllers();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void RestoreDefaultControllers()
        {
            var deleted = false;
            
            foreach (var brokenLayer in _brokenLayers)
            {
                var restore = AssetDatabase.GetAssetPath(ModuleVrc3Styles.Data.RestoreOf(brokenLayer));
                var original = AssetDatabase.GetAssetPath(ModuleVrc3Styles.Data.ControllerOf(brokenLayer));
                if (!string.IsNullOrEmpty(restore) && !string.IsNullOrEmpty(original)) AssetDatabase.CopyAsset(restore, original);
                else deleted = true;
            }

            const string message = "It seems some controllers has been moved or has been deleted!\n\nPlease, reimport the Gesture Manager to fix this.";
            if (deleted) EditorUtility.DisplayDialog("Restore Error!", message, "Ok");

            EditorApplication.isPlaying = false;
        }

        private RadialMenu GetOrCreateRadial(UnityEditor.Editor editor)
        {
            if (RadialMenus.ContainsKey(editor)) return RadialMenus[editor];

            RadialMenus[editor] = new RadialMenu(this);
            RadialMenus[editor].Set(Menu);
            return RadialMenus[editor];
        }

        private Vrc3WeightController GetOrCreateWeightController(UnityEditor.Editor editor)
        {
            if (_weightControllers.ContainsKey(editor)) return _weightControllers[editor];

            _weightControllers[editor] = new Vrc3WeightController(this);
            return _weightControllers[editor];
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
            if (state || !Avatar) return;

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

            _dummyMode = DummyMode.None;
            RemoveDummy(true);
        }

        internal RadialDescription DummyDescription()
        {
            return _dummyMode == DummyMode.Edit ? new RadialDescription("You're in Edit-Mode,", "select your avatar", "to directly edit your animations!", SelectAvatarAction, null) : null;
        }

        private void SelectAvatarAction(string obj)
        {
            if (DummyAvatar == null) return;

            Selection.activeGameObject = DummyAvatar;
            // Unity is too shy and hide too much stuff in his internal scope...
            // this is a sad and fragile work around for opening the Animation Window.
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
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
            Params.Clear();
            DestroyGraphs();
        }

        private void DestroyGraphs()
        {
            if (_playableGraph.IsValid()) _playableGraph.Destroy();
            if (AvatarAnimator.playableGraph.IsValid()) AvatarAnimator.playableGraph.Destroy();
        }

        private void OnSeatedModeChange(Vrc3Param param, float seated) => FromBlend[VRCAvatarDescriptor.AnimLayerType.Sitting].Set(seated);

        private void OnTPoseChange(bool state) => FromBlend[VRCAvatarDescriptor.AnimLayerType.TPose].Set(state ? 1f : 0f);

        private void OnIKPoseChange(bool state) => FromBlend[VRCAvatarDescriptor.AnimLayerType.IKPose].Set(state ? 1f : 0f);

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

        /*
         * Vrc Hooks
         */

        private void StartVrcHooks()
        {
            if (_hooked) return;

            VRC_AnimatorLayerControl.Initialize += AnimatorLayerControlInit;
            VRC_PlayableLayerControl.Initialize += PlayableLayerControlInit;
            VRC_AvatarParameterDriver.Initialize += AvatarParameterDriverInit;
            VRC_AnimatorTrackingControl.Initialize += AnimatorTrackingControlInit;
            VRC_AnimatorLocomotionControl.Initialize += AnimatorLocomotionControlInit;
            VRC_AnimatorTemporaryPoseSpace.Initialize += AnimatorTemporaryPoseSpaceInit;
            _hooked = true;
        }

        private void StopVrcHooks()
        {
            if (!_hooked) return;

            VRC_AnimatorLayerControl.Initialize -= AnimatorLayerControlInit;
            VRC_PlayableLayerControl.Initialize -= PlayableLayerControlInit;
            VRC_AvatarParameterDriver.Initialize -= AvatarParameterDriverInit;
            VRC_AnimatorTrackingControl.Initialize -= AnimatorTrackingControlInit;
            VRC_AnimatorLocomotionControl.Initialize -= AnimatorLocomotionControlInit;
            VRC_AnimatorTemporaryPoseSpace.Initialize -= AnimatorTemporaryPoseSpaceInit;
            _hooked = false;
        }

        /*
         * Vrc Hooks (Static)
         */

        private void AnimatorLayerControlInit(VRC_AnimatorLayerControl animatorLayerControl) => animatorLayerControl.ApplySettings += AnimatorLayerControlSettings;

        private void PlayableLayerControlInit(VRC_PlayableLayerControl playableLayerControl) => playableLayerControl.ApplySettings += PlayableLayerControlSettings;

        private void AvatarParameterDriverInit(VRC_AvatarParameterDriver avatarParameterDriver) => avatarParameterDriver.ApplySettings += AvatarParameterDriverSettings;

        private void AnimatorTrackingControlInit(VRC_AnimatorTrackingControl animatorTrackingControl) => animatorTrackingControl.ApplySettings += AnimatorTrackingControlSettings;

        private void AnimatorLocomotionControlInit(VRC_AnimatorLocomotionControl animatorLocomotionControl) => animatorLocomotionControl.ApplySettings += AnimatorLocomotionControlSettings;

        private void AnimatorTemporaryPoseSpaceInit(VRC_AnimatorTemporaryPoseSpace animatorTemporaryPoseSpace) => animatorTemporaryPoseSpace.ApplySettings += AnimatorTemporaryPoseSpaceSettings;

        /*
         * Vrc Hooks (Dynamic)
         */

        private void AnimatorLayerControlSettings(VRC_AnimatorLayerControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            FromBlend[ModuleVrc3Styles.Data.AnimatorToLayer[control.playable]].Start(control);
        }

        private void PlayableLayerControlSettings(VRC_PlayableLayerControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            FromBlend[ModuleVrc3Styles.Data.PlayableToLayer[control.layer]].Start(control);
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

        private void AnimatorTrackingControlSettings(VRC_AnimatorTrackingControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;
            const VRC_AnimatorTrackingControl.TrackingType noChange = VRC_AnimatorTrackingControl.TrackingType.NoChange;

            if (control.trackingHip != noChange) TrackingControls["Hip"] = control.trackingHip;
            if (control.trackingHead != noChange) TrackingControls["Head"] = control.trackingHead;
            if (control.trackingEyes != noChange) TrackingControls["Eye & Eyelid"] = control.trackingEyes;
            if (control.trackingMouth != noChange) TrackingControls["Mouth & Jaw"] = control.trackingMouth;
            if (control.trackingLeftHand != noChange) TrackingControls["Left Hand"] = control.trackingLeftHand;
            if (control.trackingLeftFoot != noChange) TrackingControls["Left Foot"] = control.trackingLeftFoot;
            if (control.trackingRightHand != noChange) TrackingControls["Right Hand"] = control.trackingRightHand;
            if (control.trackingRightFoot != noChange) TrackingControls["Right Foot"] = control.trackingRightFoot;
            if (control.trackingLeftFingers != noChange) TrackingControls["Left Fingers"] = control.trackingLeftFingers;
            if (control.trackingRightFingers != noChange) TrackingControls["Right Fingers"] = control.trackingRightFingers;
        }

        private void AnimatorLocomotionControlSettings(VRC_AnimatorLocomotionControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            LocomotionDisabled = control.disableLocomotion;
        }

        private void AnimatorTemporaryPoseSpaceSettings(VRC_AnimatorTemporaryPoseSpace poseSpace, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            PoseSpace = poseSpace.enterPoseSpace;
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