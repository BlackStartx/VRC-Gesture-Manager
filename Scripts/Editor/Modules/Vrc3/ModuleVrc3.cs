#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.AvatarDynamics;
using GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes;
using GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl;
using GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.Vrc3Debug.Avatar;
using GestureManager.Scripts.Editor.Modules.Vrc3.Vrc3Debug.Osc;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class ModuleVrc3 : ModuleBase
    {
        private readonly VRCAvatarDescriptor _avatarDescriptor;

        internal readonly OscModule OscModule;

        private Vrc3AvatarDebugWindow _debugAvatarWindow;
        private readonly AvatarTools _avatarTools;
        internal Vrc3OscDebugWindow DebugOscWindow;

        internal Vrc3DummyMode DummyMode;

        private PlayableGraph _playableGraph;
        private bool _hooked;

        private bool _broken;
        private bool _ignoreWarnings;

        private int _debug;

        internal readonly Vrc3ParamBool Dummy;
        internal readonly Vrc3ParamBool PoseT;
        internal readonly Vrc3ParamBool PoseIK;

        private readonly Dictionary<UnityEditor.Editor, Vrc3WeightController> _weightControllers = new Dictionary<UnityEditor.Editor, Vrc3WeightController>();
        private readonly Dictionary<UnityEditor.Editor, VisualEpContainer> _oscContainers = new Dictionary<UnityEditor.Editor, VisualEpContainer>();
        private readonly Dictionary<UnityEditor.Editor, RadialMenu> _radialMenus = new Dictionary<UnityEditor.Editor, RadialMenu>();

        private readonly Dictionary<VRCAvatarDescriptor.AnimLayerType, LayerData> _layers = new Dictionary<VRCAvatarDescriptor.AnimLayerType, LayerData>();
        private readonly List<VRCAvatarDescriptor.AnimLayerType> _brokenLayers = new List<VRCAvatarDescriptor.AnimLayerType>();
        internal readonly Dictionary<string, Vrc3Param> Params = new Dictionary<string, Vrc3Param>();

        internal bool LocomotionDisabled;
        internal bool PoseSpace;

        internal readonly Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> TrackingControls = ModuleVrc3Styles.Data.DefaultTrackingState;
        internal readonly HashSet<ContactReceiver> Receivers = new HashSet<ContactReceiver>();
        private readonly HashSet<AnimationClip> _avatarClips = new HashSet<AnimationClip>();
        private readonly HashSet<ContactSender> _senders = new HashSet<ContactSender>();

        internal int DebugToolBar;
        internal string Edit;

        private IEnumerable<AnimationClip> OriginalClips => _avatarClips.Where(clip => !clip.name.StartsWith("proxy_"));
        private VRCExpressionParameters Parameters => _avatarDescriptor.expressionParameters;
        private VRCExpressionsMenu Menu => _avatarDescriptor.expressionsMenu;
        internal IEnumerable<RadialMenu> Radials => _radialMenus.Values;
        internal float ViseAmount => _avatarDescriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape ? 14 : 100;

        public ModuleVrc3(GestureManager manager, VRCAvatarDescriptor avatarDescriptor) : base(manager, avatarDescriptor)
        {
            _avatarTools = new AvatarTools();
            _avatarDescriptor = avatarDescriptor;
            OscModule = new OscModule(this);

            Dummy = new Vrc3ParamBool(OnDummyModeChange);
            PoseIK = new Vrc3ParamBool(OnIKPoseChange);
            PoseT = new Vrc3ParamBool(OnTPoseChange);
        }

        public override void Update()
        {
            if (_broken) return;
            OscModule.Update();
            _avatarTools.OnUpdate(this);
            if (DummyMode == null && _layers.Any(IsBroken)) OnBrokenSimulation();
            if (DummyMode != null && (!DummyMode.Avatar || Avatar.activeSelf)) Dummy.ShutDown();
            foreach (var pair in _layers) pair.Value.Weight.Update();
        }

        public override void LateUpdate() => _avatarTools.OnLateUpdate(this);

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

            _playableGraph = PlayableGraph.Create("Gesture Manager 3.6");
            var externalOutput = AnimationPlayableOutput.Create(_playableGraph, "Gesture Manager", AvatarAnimator);
            var playableMixer = AnimationLayerMixerPlayable.Create(_playableGraph, layerList.Count + 1);
            externalOutput.SetSourcePlayable(playableMixer);

            _layers.Clear();
            _avatarClips.Clear();
            _brokenLayers.Clear();

            _senders.Clear();
            Receivers.Clear();

            for (var i = 0; i < layerList.Count; i++)
            {
                var vrcAnimLayer = layerList[i];
                var intGraph = i + 1;

                var isFx = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.FX;
                var isAdd = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Additive;
                var isPose = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.IKPose || vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.TPose;
                var isAction = vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Sitting || vrcAnimLayer.type == VRCAvatarDescriptor.AnimLayerType.Action;
                var isLim = isPose || isAction;

                if (vrcAnimLayer.animatorController)
                    foreach (var clip in vrcAnimLayer.animatorController.animationClips)
                        _avatarClips.Add(clip);

                var controller = Vrc3ProxyOverride.OverrideController(vrcAnimLayer.isDefault ? RequestBuiltInController(vrcAnimLayer.type) : vrcAnimLayer.animatorController);
                var mask = vrcAnimLayer.isDefault || isFx ? ModuleVrc3Styles.Data.MaskOf[vrcAnimLayer.type] : vrcAnimLayer.mask;

                var playable = AnimatorControllerPlayable.Create(_playableGraph, controller);
                var weight = new AnimatorControllerWeight(playableMixer, playable, intGraph);
                _layers[vrcAnimLayer.type] = new LayerData { Playable = playable, Weight = weight, Empty = playable.GetInput(0).IsNull() };

                playableMixer.ConnectInput(intGraph, playable, 0, 1);

                if (isLim) playableMixer.SetInputWeight(intGraph, 0f);
                if (isAdd) playableMixer.SetLayerAdditive((uint)intGraph, true);
                if (mask) playableMixer.SetLayerMaskFromAvatarMask((uint)intGraph, mask);
            }

            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _playableGraph.Play();
            _playableGraph.Evaluate(0f);

            foreach (var menu in Radials) menu.Set(Menu);
            InitParams(AvatarAnimator, Parameters);

            GetParam(Vrc3DefaultParams.Upright)?.InternalSet(1f);
            GetParam(Vrc3DefaultParams.Grounded)?.InternalSet(1f);
            GetParam(Vrc3DefaultParams.TrackingType)?.InternalSet(1f);
            GetParam(Vrc3DefaultParams.AvatarVersion)?.InternalSet(3f);
            GetParam(Vrc3DefaultParams.GestureLeftWeight)?.InternalSet(1f);
            GetParam(Vrc3DefaultParams.GestureRightWeight)?.InternalSet(1f);

            Left = (int)(GetParam(Vrc3DefaultParams.GestureLeft)?.Get() ?? 0);
            Right = (int)(GetParam(Vrc3DefaultParams.GestureRight)?.Get() ?? 0);

            GetParam(Vrc3DefaultParams.Vise)?.SetOnChange(OnViseChange);
            GetParam(Vrc3DefaultParams.Seated)?.SetOnChange(OnSeatedModeChange);
            GetParam(Vrc3DefaultParams.GestureLeft)?.SetOnChange(OnGestureLeftChange);
            GetParam(Vrc3DefaultParams.GestureRight)?.SetOnChange(OnGestureRightChange);

            foreach (var physBone in AvatarComponents<VRCPhysBoneBase>()) PhysBoneBaseSetup(physBone);
            foreach (var receiver in AvatarComponents<ContactReceiver>()) ReceiverBaseSetup(receiver);
            foreach (var sender in AvatarComponents<ContactSender>()) SenderBaseSetup(sender);

            OscModule.Resume();
        }

        public override void Unlink()
        {
            CloseDebugWindows();
            if (OscModule.Enabled) OscModule.Stop();
            if (Dummy.State) Dummy.ShutDown();
            if (AvatarAnimator) ForgetAvatar();
            StopVisualElements();
            StopVrcHooks();
        }

        public override void EditorHeader()
        {
            if (_brokenLayers.Count == 0 || _ignoreWarnings || _broken) return;
            using (new GmgLayoutHelper.GuiBackground(Color.yellow)) ShowWarnings();
        }

        public override void EditorContent(object editor, VisualElement element)
        {
            var menu = GetOrCreateRadial(editor as UnityEditor.Editor);
            var oscContainer = GetOrCreateOscContainer(editor as UnityEditor.Editor);
            var weightController = GetOrCreateWeightController(editor as UnityEditor.Editor);

            GUI.enabled = !_broken;
            GmgLayoutHelper.MyToolbar(ref menu.ToolBar, new (string, Action)[]
            {
                ("Gestures", () => EditorContentGesture(element, weightController)),
                ("Tools", () => _avatarTools.Gui(this)),
                ("Debug", () => EditorContentDebug(menu))
            });
            GUI.enabled = true;

            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);

            if (_broken) ShowError(menu);
            else
                switch (menu.ToolBar.Selected)
                {
                    case 0:
                        ShowRadialMenu(menu, element);
                        break;
                    case 2:
                        ShowDebugMenu(element, oscContainer, menu);
                        break;
                }

            menu.CheckCondition(this, menu);
            oscContainer.CheckCondition(this, menu);
            weightController.CheckCondition(this, menu);
        }

        protected override void OnNewLeft(int left) => Params[Vrc3DefaultParams.GestureLeft].Set(this, left);

        protected override void OnNewRight(int right) => Params[Vrc3DefaultParams.GestureRight].Set(this, right);

        public override AnimationClip GetFinalGestureByIndex(int gestureIndex) => ModuleVrc3Styles.Data.GestureClips[gestureIndex];

        public override Animator OnCustomAnimationPlay(AnimationClip clip)
        {
            if (!clip) return Vrc3TestMode.Disable(DummyMode);
            if (!(DummyMode is Vrc3TestMode testMode)) testMode = Vrc3TestMode.Enable(this);
            return testMode.Test(clip);
        }

        public override bool HasGestureBeenOverridden(int gesture) => true;

        public override void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation)
        {
        }

        protected override List<string> CheckErrors()
        {
            var errorList = base.CheckErrors();
            errorList.AddRange(RadialMenuUtility.CheckErrors(_avatarDescriptor.expressionsMenu, _avatarDescriptor.expressionParameters));
            return errorList;
        }

        /*
         * Editor GUI
         */

        private void EditorContentGesture(VisualElement element, Vrc3WeightController weightController)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = DummyMode == null && !_broken;

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Left Hand", GestureManagerStyles.GuiHandTitle);
                    weightController.RenderLeft(element);
                    using (new GUILayout.VerticalScope()) GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Left, Left, false);
                    var rect = GUILayoutUtility.GetLastRect();
                    var isContained = rect.Contains(Event.current.mousePosition);
                    GestureDrag = (Event.current.type == EventType.MouseDown && isContained) || Event.current.type != EventType.MouseUp && GestureDrag;
                    if (isContained && GestureDrag && Event.current.type == EventType.MouseDrag) OnNewLeft((int)((Event.current.mousePosition.y - GUILayoutUtility.GetLastRect().y) / 19) + 1);
                }

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Right Hand", GestureManagerStyles.GuiHandTitle);
                    weightController.RenderRight(element);
                    using (new GUILayout.VerticalScope()) GestureManagerEditor.OnCheckBoxGuiHand(this, GestureHand.Right, Right, false);
                    var rect = GUILayoutUtility.GetLastRect();
                    var isContained = rect.Contains(Event.current.mousePosition);
                    GestureDrag = (Event.current.type == EventType.MouseDown && isContained) || Event.current.type != EventType.MouseUp && GestureDrag;
                    if (isContained && GestureDrag && Event.current.type == EventType.MouseDrag) OnNewRight((int)((Event.current.mousePosition.y - GUILayoutUtility.GetLastRect().y) / 19) + 1);
                }

                GUI.enabled = true;
            }
        }

        private void EditorContentDebug(RadialMenu menu)
        {
            GmgLayoutHelper.MyToolbar(ref menu.DebugToolBar, new (string, Action)[]
            {
                ("Avatar", EditorContentDebugAvatar),
                ("Open Sound Control", OscModule.ControlPanel)
            });
        }

        private void EditorContentDebugAvatar()
        {
            GUILayout.Label("Avatar Debug", GestureManagerStyles.GuiHandTitle);
            GUILayout.Label(_debugAvatarWindow ? Vrc3AvatarDebugWindow.Text.W.Subtitle : Vrc3AvatarDebugWindow.Text.D.Subtitle, GestureManagerStyles.SubHeader);

            GUILayout.Space(_debugAvatarWindow ? 11 : 10);
            if (_debugAvatarWindow) GUILayout.Label(Vrc3AvatarDebugWindow.Text.W.Message, GestureManagerStyles.SubHeader);
            else DebugToolBar = Vrc3AvatarDebugWindow.Static.DebugToolbar(DebugToolBar);
            GUILayout.Space(_debugAvatarWindow ? 11 : 10);

            GUILayout.Label(_debugAvatarWindow ? Vrc3AvatarDebugWindow.Text.W.Hint : Vrc3AvatarDebugWindow.Text.D.Hint, GestureManagerStyles.SubHeader);
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GmgLayoutHelper.DebugButton(_debugAvatarWindow ? Vrc3AvatarDebugWindow.Text.W.Button : Vrc3AvatarDebugWindow.Text.D.Button)) SwitchDebugAvatarView();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        /*
         * Functions
         */

        private static bool IsBroken(KeyValuePair<VRCAvatarDescriptor.AnimLayerType, LayerData> pair) => !pair.Value.Empty && pair.Value.Playable.GetInput(0).IsNull();

        private IEnumerable<T> AvatarComponents<T>(bool includeInactive = true) where T : Component => Avatar.GetComponentsInChildren<T>(includeInactive);

        private void RemoveVise() => OnViseChange(null, 0f);

        private void OnBrokenSimulation()
        {
            _broken = true;
            foreach (var menu in Radials) menu.ToolBar.Selected = 0;
            if (OscModule.Enabled) OscModule.Stop();
            CloseDebugWindows();
        }

        private void CloseDebugWindows()
        {
            if (_debugAvatarWindow) _debugAvatarWindow.Close();
            if (DebugOscWindow) DebugOscWindow.Close();
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
            foreach (var container in _oscContainers.Values) container.StopRendering();
            foreach (var weight in _weightControllers.Values) weight.StopRendering();
            foreach (var menu in Radials) menu.StopRendering();
        }

        internal void ReloadRadials()
        {
            foreach (var weight in _weightControllers.Values) weight.StopRendering();
            foreach (var menu in Radials) menu.StopRendering();
            _weightControllers.Clear();
            _radialMenus.Clear();
        }

        private void SwitchDebugAvatarView() => _debugAvatarWindow = _debugAvatarWindow ? Vrc3AvatarDebugWindow.Close(_debugAvatarWindow) : Vrc3AvatarDebugWindow.Create(this);

        internal void SwitchDebugOscView() => DebugOscWindow = DebugOscWindow ? Vrc3OscDebugWindow.Close(DebugOscWindow) : Vrc3OscDebugWindow.Create(this);

        private void ShowDebugMenu(VisualElement root, VisualEpContainer holder, RadialMenu menu)
        {
            switch (menu.DebugToolBar.Selected)
            {
                case 0 when _debugAvatarWindow:
                case 1 when DebugOscWindow:
                    return;
            }

            DebugContext(root, holder, menu.DebugToolBar.Selected, Screen.width - 60, false);
            GUILayout.Space(4);
            GmgLayoutHelper.Divisor(1);
        }

        internal void DebugContext(VisualElement root, VisualEpContainer holder, int selected, float width, bool fullScreen)
        {
            if (DummyMode == null)
            {
                switch (selected)
                {
                    case 0:
                        Vrc3AvatarDebugWindow.Static.DebugLayout(this, width, fullScreen, _layers);
                        break;
                    case 1:
                        OscModule.DebugLayout(root, holder, width);
                        break;
                }
            }
            else Vrc3AvatarDebugWindow.Static.DummyLayout(DummyMode.ModeName);
        }

        private void ShowWarnings()
        {
            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError))
            {
                GUILayout.Space(2);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(15);
                    GUILayout.Label("WARNING", GestureManagerStyles.TextWarningHeader);
                    if (GUILayout.Button("X", GUI.skin.label, GUILayout.Width(15))) _ignoreWarnings = true;
                }

                GUILayout.Space(5);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Some default Animator Controllers has changed!", GestureManagerStyles.TextWarning);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Restore Controllers")) RestoreDefaultControllers();
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void ShowError(Vrc3VisualRender menu)
        {
            menu.StopRendering();
            GUILayout.Label("Radial Menu", GestureManagerStyles.GuiHandTitle);
            using (new GmgLayoutHelper.GuiBackground(Color.red))
            using (new GUILayout.VerticalScope(GUILayout.Height(RadialMenu.Size)))
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(3);
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("An Animator Controller changed during the simulation!", GestureManagerStyles.TextError);
                        GUILayout.FlexibleSpace();
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            GUI.backgroundColor = RadialMenuUtility.Colors.RestartButton;
                            if (GmgLayoutHelper.DebugButton("Click to reload scene")) ReloadScene();
                            GUILayout.FlexibleSpace();
                        }

                        GUILayout.FlexibleSpace();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void ReloadScene()
        {
            if (Manager.Module == null) return;
            var activeScene = SceneManager.GetActiveScene();
            LoadScene(activeScene.buildIndex, LoadSceneMode.Single, (scene, mode) =>
            {
                var manager = VRC.Tools.FindSceneObjectsOfTypeAll<GestureManager>().FirstOrDefault();
                GestureManagerEditor.CreateAndPing(manager);
            });
        }

        private void RestoreDefaultControllers()
        {
            var deleted = false;

            foreach (var brokenLayer in _brokenLayers)
            {
                var pString = AssetDatabase.GetAssetPath(ModuleVrc3Styles.Data.RestoreOf(brokenLayer));
                var newString = AssetDatabase.GetAssetPath(ModuleVrc3Styles.Data.ControllerOf(brokenLayer));
                if (!string.IsNullOrEmpty(pString) && !string.IsNullOrEmpty(newString)) AssetDatabase.CopyAsset(pString, newString);
                else deleted = true;
            }

            const string message = "It seems some controllers has been moved or has been deleted!\n\nPlease, reimport the Gesture Manager to fix this.";
            if (deleted) EditorUtility.DisplayDialog("Restore Error!", message, "Ok");

            ReloadScene();
        }

        private RadialMenu GetOrCreateRadial(UnityEditor.Editor editor)
        {
            if (_radialMenus.ContainsKey(editor)) return _radialMenus[editor];

            _radialMenus[editor] = new RadialMenu(this, true);
            _radialMenus[editor].Set(Menu);
            return _radialMenus[editor];
        }

        private Vrc3WeightController GetOrCreateWeightController(UnityEditor.Editor editor)
        {
            if (_weightControllers.ContainsKey(editor)) return _weightControllers[editor];

            _weightControllers[editor] = new Vrc3WeightController(this);
            return _weightControllers[editor];
        }

        private VisualEpContainer GetOrCreateOscContainer(UnityEditor.Editor editor)
        {
            if (_oscContainers.ContainsKey(editor)) return _oscContainers[editor];

            _oscContainers[editor] = new VisualEpContainer();
            return _oscContainers[editor];
        }

        private void OnDummyModeChange(bool state) => DummyMode?.OnExecutionChange(state);

        internal void EnableEditMode() => Vrc3EditMode.Enable(this, OriginalClips);

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

        internal void ForgetAvatar()
        {
            RemoveVise();
            if (OscModule.Enabled) OscModule.Forget();
            AvatarAnimator.Rebind();
            Params.Clear();
            DestroyGraphs();
        }

        private void DestroyGraphs()
        {
            if (_playableGraph.IsValid()) _playableGraph.Destroy();
            if (AvatarAnimator.playableGraph.IsValid()) AvatarAnimator.playableGraph.Destroy();
        }

        private void OnViseChange(Vrc3Param param, float fVise)
        {
            var vise = (int)fVise;
            var skinnedMesh = _avatarDescriptor.VisemeSkinnedMesh;

            switch (_avatarDescriptor.lipSync)
            {
                case VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone:
                    if (!_avatarDescriptor.lipSyncJawBone) return;
                    _avatarDescriptor.lipSyncJawBone.transform.rotation = vise == 0 ? _avatarDescriptor.lipSyncJawClosed : _avatarDescriptor.lipSyncJawOpen;
                    return;
                case VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape:
                    if (!skinnedMesh) return;
                    SetBlendShapeWeight(skinnedMesh, _avatarDescriptor.MouthOpenBlendShapeName, vise);
                    return;
                case VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape:
                    if (_avatarDescriptor.VisemeBlendShapes == null || !skinnedMesh) return;
                    for (var i = 0; i < _avatarDescriptor.VisemeBlendShapes.Length; i++) SetBlendShapeWeight(skinnedMesh, _avatarDescriptor.VisemeBlendShapes[i], i == vise ? 100.0f : 0.0f);
                    return;
                case VRC_AvatarDescriptor.LipSyncStyle.Default:
                case VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly:
                    return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSeatedModeChange(Vrc3Param param, float seated) => _layers[VRCAvatarDescriptor.AnimLayerType.Sitting].Weight.Set(seated);

        private void OnTPoseChange(bool state) => _layers[VRCAvatarDescriptor.AnimLayerType.TPose].Weight.Set(state ? 1f : 0f);

        private void OnIKPoseChange(bool state) => _layers[VRCAvatarDescriptor.AnimLayerType.IKPose].Weight.Set(state ? 1f : 0f);

        private void OnGestureLeftChange(Vrc3Param param, float left) => Left = (int)left;

        private void OnGestureRightChange(Vrc3Param param, float right) => Right = (int)right;

        /*
         * Params
         */

        private void InitParams(Animator animator, VRCExpressionParameters parameters)
        {
            Params.Clear();
            foreach (var pair in _layers)
            foreach (var parameter in RadialMenuUtility.GetParameters(pair.Value.Playable))
                Params[parameter.name] = RadialMenuUtility.CreateParamFromController(animator, parameter, pair.Value.Playable);

            if (parameters)
                foreach (var parameter in parameters.parameters)
                    if (!Params.ContainsKey(parameter.name))
                        Params[parameter.name] = RadialMenuUtility.CreateParamFromNothing(parameter);

            if (parameters)
                foreach (var parameter in parameters.parameters)
                    Params[parameter.name].InternalSet(parameter.defaultValue);

            foreach (var (nameString, type) in Vrc3DefaultParams.Parameters)
                if (!Params.ContainsKey(nameString))
                    Params[nameString] = RadialMenuUtility.CreateParamFromNothing(nameString, type);
        }

        public Vrc3Param GetParam(string pName)
        {
            if (pName == null) return null;
            Params.TryGetValue(pName, out var param);
            return param;
        }

        private static string NameOf(UnityEngine.Object o) => Application.dataPath + AssetDatabase.GetAssetPath(o).Substring(6);

        private static void LoadScene(int sceneBuildIndex, LoadSceneMode mode, Action<Scene, LoadSceneMode> onLoad)
        {
            void OnSceneManagerLoaded(Scene scene, LoadSceneMode m)
            {
                SceneManager.sceneLoaded -= OnSceneManagerLoaded;
                onLoad(scene, m);
            }

            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            SceneManager.LoadScene(sceneBuildIndex, mode);
        }

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

        private static void ShowRadialMenu(RadialMenu menu, VisualElement element)
        {
            GUILayout.Label("Radial Menu", GestureManagerStyles.GuiHandTitle);
            GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(RadialMenu.Size));
            menu.Render(element, GmgLayoutHelper.GetLastRect(ref menu.Rect));
            menu.ShowRadialDescription();
        }

        private static void SetBlendShapeWeight(SkinnedMeshRenderer skinnedMesh, string name, float weight)
        {
            var intIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(name);
            if (intIndex != -1) skinnedMesh.SetBlendShapeWeight(intIndex, weight);
        }

        /*
         * Vrc Hooks
         */

        private void StartVrcHooks()
        {
            if (_hooked) return;
            AvatarDynamicReset.CheckSceneCollisions();

            ContactBase.OnInitialize += ContactBaseInit;
            VRCPhysBoneBase.OnInitialize += PhysBoneBaseInit;
            VRC_AnimatorLayerControl.Initialize += AnimatorLayerControlInit;
            VRC_PlayableLayerControl.Initialize += PlayableLayerControlInit;
            VRC_AnimatorTrackingControl.Initialize += AnimatorTrackingControlInit;
            VRC_AnimatorLocomotionControl.Initialize += AnimatorLocomotionControlInit;
            VRC_AnimatorTemporaryPoseSpace.Initialize += AnimatorTemporaryPoseSpaceInit;

            VRC_AvatarParameterDriver.OnApplySettings += AvatarParameterDriverSettings;

            _hooked = true;
        }

        private void StopVrcHooks()
        {
            if (!_hooked) return;

            ContactBase.OnInitialize -= ContactBaseInit;
            VRCPhysBoneBase.OnInitialize -= PhysBoneBaseInit;
            VRC_AnimatorLayerControl.Initialize -= AnimatorLayerControlInit;
            VRC_PlayableLayerControl.Initialize -= PlayableLayerControlInit;
            VRC_AnimatorTrackingControl.Initialize -= AnimatorTrackingControlInit;
            VRC_AnimatorLocomotionControl.Initialize -= AnimatorLocomotionControlInit;
            VRC_AnimatorTemporaryPoseSpace.Initialize -= AnimatorTemporaryPoseSpaceInit;

            VRC_AvatarParameterDriver.OnApplySettings -= AvatarParameterDriverSettings;

            _hooked = false;
        }

        /*
         * Vrc Hooks (Static)
         */

        private void AnimatorLayerControlInit(VRC_AnimatorLayerControl animatorLayerControl) => animatorLayerControl.ApplySettings += AnimatorLayerControlSettings;

        private void PlayableLayerControlInit(VRC_PlayableLayerControl playableLayerControl) => playableLayerControl.ApplySettings += PlayableLayerControlSettings;

        private void AnimatorTrackingControlInit(VRC_AnimatorTrackingControl animatorTrackingControl) => animatorTrackingControl.ApplySettings += AnimatorTrackingControlSettings;

        private void AnimatorLocomotionControlInit(VRC_AnimatorLocomotionControl animatorLocomotionControl) => animatorLocomotionControl.ApplySettings += AnimatorLocomotionControlSettings;

        private void AnimatorTemporaryPoseSpaceInit(VRC_AnimatorTemporaryPoseSpace animatorTemporaryPoseSpace) => animatorTemporaryPoseSpace.ApplySettings += AnimatorTemporaryPoseSpaceSettings;

        /*
         * Vrc Hooks (Dynamic)
         */

        private void AnimatorLayerControlSettings(VRC_AnimatorLayerControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            _layers[ModuleVrc3Styles.Data.AnimatorToLayer[control.playable]].Weight.Start(control);
        }

        private void PlayableLayerControlSettings(VRC_PlayableLayerControl control, Animator animator)
        {
            if (!_hooked || animator != AvatarAnimator) return;

            _layers[ModuleVrc3Styles.Data.PlayableToLayer[control.layer]].Weight.Start(control);
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
                        param.Set(this, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Add:
                        param.Add(this, parameter.value);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Copy:
                        param.Copy(this, GetParam(parameter.source)?.Get() ?? 0f, parameter.convertRange, parameter.sourceMin, parameter.sourceMax, parameter.destMin, parameter.destMax);
                        break;
                    case VRC_AvatarParameterDriver.ChangeType.Random:
                        param.Random(this, parameter.valueMin, parameter.valueMax, parameter.chance);
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

        /*
         * Vrc Hooks (Dynamics)
         */

        private bool ContactBaseInit(ContactBase contactBase)
        {
            var animator = contactBase.GetComponentInParent<VRCAvatarDescriptor>()?.GetComponent<Animator>();
            if (!_hooked || !animator || animator != AvatarAnimator) return true;

            switch (contactBase)
            {
                case ContactReceiver receiver:
                    ReceiverBaseSetup(receiver);
                    return true;
                case ContactSender sender:
                    SenderBaseSetup(sender);
                    return true;
                default: return true;
            }
        }

        private void ReceiverBaseSetup(ContactReceiver receiver)
        {
            if (string.IsNullOrWhiteSpace(receiver.parameter)) return;
            Receivers.Add(receiver);
            receiver.paramAccess = new AnimParameterAccessAvatarGmg(this, receiver.parameter);
            if (receiver.shape != null) receiver.shape.OnEnter += shape => ContactShapeCheck(receiver, shape.component as ContactSender);
        }

        private void SenderBaseSetup(ContactSender sender) => _senders.Add(sender);

        private void ContactShapeCheck(ContactReceiver receiver, ContactSender sender)
        {
            if (!sender || !receiver) return;
            if (!receiver.allowSelf && _senders.Contains(sender)) receiver.shape.OnExit.Invoke(sender.shape);
            if (!receiver.allowOthers && !_senders.Contains(sender)) receiver.shape.OnExit.Invoke(sender.shape);
        }

        private void PhysBoneBaseInit(VRCPhysBoneBase vrcPhysBoneBase)
        {
            if (string.IsNullOrEmpty(vrcPhysBoneBase.parameter)) return;

            var animator = vrcPhysBoneBase.GetComponentInParent<VRCAvatarDescriptor>()?.GetComponent<Animator>();
            if (!_hooked || !animator || animator != AvatarAnimator) return;

            PhysBoneBaseSetup(vrcPhysBoneBase);
        }

        private void PhysBoneBaseSetup(VRCPhysBoneBase vrcPhysBoneBase)
        {
            vrcPhysBoneBase.param_IsGrabbed = new AnimParameterAccessAvatarGmg(this, vrcPhysBoneBase.parameter + VRCPhysBoneBase.PARAM_ISGRABBED);
            vrcPhysBoneBase.param_Angle = new AnimParameterAccessAvatarGmg(this, vrcPhysBoneBase.parameter + VRCPhysBoneBase.PARAM_ANGLE);
            vrcPhysBoneBase.param_Stretch = new AnimParameterAccessAvatarGmg(this, vrcPhysBoneBase.parameter + VRCPhysBoneBase.PARAM_STRETCH);
        }

        internal struct LayerData
        {
            internal AnimatorControllerPlayable Playable;
            internal AnimatorControllerWeight Weight;
            internal bool Empty;
        }
    }
}
#endif