#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Data;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.Dynamics;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Tools
{
    public class AvatarTools
    {
        private static Camera ToolCamera => Camera.allCameras.FirstOrDefault(CameraRule);
        private static bool CameraRule(Camera camera) => camera.enabled && camera.gameObject.activeInHierarchy;

        private UpdateSceneCamera _sceneCamera;
        public UpdateSceneCamera SceneCamera => _sceneCamera ??= new UpdateSceneCamera();

        private ClickableContacts _clickableContacts;
        public ClickableContacts ContactsClickable => _clickableContacts ??= new ClickableContacts();

        private AvatarPose _avatarPose;
        public AvatarPose PoseAvatar => _avatarPose ??= new AvatarPose();

        private AvatarBackground _avatarBackground;
        private AvatarBackground BackgroundAvatar => _avatarBackground ??= new AvatarBackground();

        private TestAnimation _testAnimation;
        private TestAnimation AnimationTest => _testAnimation ??= new TestAnimation();

        private AnimatorPerformance _animatorPerformance;
        public AnimatorPerformance PerformanceAnimator => _animatorPerformance ??= new AnimatorPerformance();

        private Customization _customization;
        private Customization CustomizationTool => _customization ??= new Customization();

        // This is silly but otherwise Rider will prompt casting hints on screen... I hate those more~
        private static bool Exist(UnityEngine.Object uObject) => !NExist(uObject);
        private static bool NExist(UnityEngine.Object uObject) => !uObject;

        internal void Gui(ModuleVrc3 module)
        {
            GUILayout.Label("Gesture Manager Tools", GestureManagerStyles.Header);
            GUILayout.Label("A collection of some small utility functions~", GestureManagerStyles.Centered);
            GUILayout.Space(10);
            SceneCamera.Display(module);
            ContactsClickable.Display(module);
            PoseAvatar.Display(module);
            GUILayout.Label("Extra Tools", GestureManagerStyles.Header);
            BackgroundAvatar.Display(module);
            AnimationTest.Display(module);
            PerformanceAnimator.Display(module);
            GUILayout.Label("Customization", GestureManagerStyles.Header);
            CustomizationTool.Display(module);
        }

        internal void OnUpdate(ModuleVrc3 module)
        {
            SceneCamera.OnUpdate(module);
            BackgroundAvatar.OnUpdate(module);
            ContactsClickable.OnUpdate(module);
            PerformanceAnimator.OnUpdate(module);
        }

        internal void OnLateUpdate(ModuleVrc3 module)
        {
            ContactsClickable.OnLateUpdate(module);
        }

        internal void OnDrawGizmos()
        {
            ContactsClickable.OnDrawGizmos();
        }

        public void Unlink(ModuleVrc3 module) => PerformanceAnimator.Disable(module);

        public class UpdateSceneCamera : GmgDynamicFunction
        {
            private static Camera _camera;
            private static readonly BoolProperty IsActive = new("GM3 SceneCamera");

            protected internal override string Name => "Scene Camera";
            protected override string Description => "This will match your game view with your scene view!\n\nClick the button to setup the main camera automatically~";
            protected internal override bool Active => IsActive.Property;

            public UpdateSceneCamera() => Set(IsActive.Property ? ToolCamera : null);

            protected internal override void Toggle(ModuleVrc3 module) => AutoToggle();

            protected override void Gui(ModuleVrc3 module)
            {
                if (GmgLayoutHelper.ButtonObjectField("Scene Camera: ", _camera, _camera ? 'X' : 'A', Set)) AutoToggle();
            }

            protected override void Update(ModuleVrc3 module)
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (!sceneView) return;
                var camera = sceneView.camera;
                if (!camera || !_camera) return;
                var sceneTransform = camera.transform;
                var transform = _camera.transform;
                var positionVector = sceneTransform.position;
                transform.rotation = sceneTransform.rotation;
                _camera.orthographicSize = camera.orthographicSize;
                _camera.nearClipPlane = camera.nearClipPlane;
                _camera.farClipPlane = camera.farClipPlane;
                _camera.orthographic = camera.orthographic;
                _camera.fieldOfView = camera.fieldOfView;
                transform.position = new Vector3(positionVector.x, positionVector.y + 0.001f, positionVector.z);
            }

            private static void AutoToggle() => Set(_camera ? null : ToolCamera);

            private static void Set(Camera camera)
            {
                _camera = camera;
                IsActive.Property = Exist(_camera);
            }
        }

        public class ClickableContacts : GmgDynamicFunction
        {
            private class Flat
            {
                public float Float;
            }

            private readonly BoolProperty _isActive = new("GM3 ClickableContacts");
            private readonly StringProperty _tag = new("GM3 ClickableContacts Tag");
            private readonly Dictionary<ContactReceiver, Flat> _activeContact = new();

            protected internal override string Name => "Clickable Contacts";
            protected override string Description => "Click and trigger Avatar Contacts with your mouse!\n\nLike you can do with PhysBones~";
            protected internal override bool Active => _isActive.Property;

            private static Mesh _sphereMesh;
            private static Mesh SphereMesh => _sphereMesh ? _sphereMesh : _sphereMesh = FetchSpherePrimitive();

            private static Mesh _capsuleMesh;
            private static Mesh CapsuleMesh => _capsuleMesh ? _capsuleMesh : _capsuleMesh = FetchCapsulePrimitive();

            private Camera _camera = ToolCamera;
            private Camera Camera => _camera ? _camera : _camera = ToolCamera;

            protected internal override void Toggle(ModuleVrc3 module) => _isActive.Property = !_isActive.Property;

            protected override void Gui(ModuleVrc3 module)
            {
                _isActive.Property = GmgLayoutHelper.ToggleRight("Enabled: ", _isActive.Property);
                _tag.Property = GmgLayoutHelper.PlaceHolderTextField("Tag: ", _tag.Property, " <leave blank to ignore tags> ");
            }

            protected override void Update(ModuleVrc3 module) => LateUpdate(module);

            protected override void LateUpdate(ModuleVrc3 module)
            {
                if (Input.GetMouseButton(0)) OnClick(module);
                if (Input.GetMouseButtonUp(0)) Disable();
            }

            protected override void DrawGizmos()
            {
                foreach (var pair in _activeContact) DrawGizmos(pair.Key, pair.Value, pair.Key.radius, pair.Key.height);
            }

            private static void DrawGizmos(ContactReceiver key, Flat value, float xRadiusZ, float yHeight)
            {
                var isSphere = key.shapeType == ContactBase.ShapeType.Sphere;
                var mesh = isSphere ? SphereMesh : CapsuleMesh;
                xRadiusZ *= 2;
                yHeight = isSphere ? xRadiusZ : yHeight / 2f;
                var scaleSize = new Vector3(xRadiusZ, yHeight, xRadiusZ);
                Gizmos.color = new Color(0.0f, 1f, 1f, value.Float * 0.85f);
                Gizmos.matrix = key.shape.transform0.localToWorldMatrix;
                if (mesh) Gizmos.DrawMesh(mesh, key.position, key.rotation, scaleSize);
                else Gizmos.DrawCube(key.shape.center, scaleSize);
                if (key.receiverType == ContactReceiver.ReceiverType.OnEnter && value.Float > 0) value.Float -= 0.05f;
            }

            private void OnClick(ModuleVrc3 module)
            {
                if (!Camera) return;
                var ray = Camera.ScreenPointToRay(Input.mousePosition);
                CheckRay(module, ray.origin, ray.origin + ray.direction * 1000f);
            }

            private void OnContactValue(ContactReceiver receiver, float value)
            {
                if (value == 0f) Disable(receiver);
                else Enable(receiver, value);
            }

            private void Enable(ContactReceiver receiver, float value)
            {
                if (_activeContact.ContainsKey(receiver) && receiver.receiverType == ContactReceiver.ReceiverType.OnEnter) return;
                _activeContact[receiver] = new Flat { Float = value };
                receiver.SetParameter(value);
            }

            private void Disable(ContactReceiver receiver)
            {
                if (!_activeContact.ContainsKey(receiver)) return;
                _activeContact.Remove(receiver);
                receiver.SetParameter(0f);
            }

            private void Disable()
            {
                foreach (var pair in _activeContact) pair.Key.SetParameter(0f);
                _activeContact.Clear();
            }

            private static Mesh FetchSpherePrimitive() => (Mesh)Resources.First(uObject => uObject.name == "Sphere");

            private static Mesh FetchCapsulePrimitive() => (Mesh)Resources.First(uObject => uObject.name == "Capsule");

            private static IEnumerable<UnityEngine.Object> Resources => AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources");

            /*
             * RayCast Calculation~
             */

            private void CheckRay(ModuleVrc3 module, Vector3 s, Vector3 b)
            {
                var manager = ContactManager.Inst;
                if (!manager) return;
                foreach (var receiver in module.Receivers.Where(receiver => string.IsNullOrEmpty(_tag.Property) || receiver.collisionTags.Contains(_tag.Property))) OnContactValue(receiver, ValueFor(manager, receiver, s, b));
            }

            private static float ValueFor(ContactManager manager, ContactReceiver receiver, Vector3 s, Vector3 b)
            {
                var isProximity = receiver.receiverType == ContactReceiver.ReceiverType.Proximity;
                var distance = DistanceFrom(manager, receiver, s, b, out var radius);
                if (isProximity) distance -= radius;
                if (isProximity) return Mathf.Clamp(-distance / radius, 0f, 1f);
                return distance < 0 ? 1f : 0f;
            }

            private static float DistanceFrom(ContactManager manager, ContactBase receiver, Vector3 s, Vector3 b, out float radius)
            {
                receiver.InitShape();
                manager.collision.UpdateShapeData(receiver.shape);
                var aPointData = manager.collision.GetShapeData(receiver.shape);
                var scaleVector = receiver.transform.lossyScale;
                radius = receiver.radius * Mathf.Max(scaleVector.x, scaleVector.y, scaleVector.z);
                var bPointVector = receiver.shapeType == ContactBase.ShapeType.Sphere ? aPointData.outPos0 : aPointData.outPos1;
                ClosestPointsBetweenLineSegments(s, b, aPointData.outPos0, bPointVector, out var vector0, out var vector1);
                return (vector0 - vector1).magnitude - radius;
            }

            /*
             * Saved from the Plugins\VRC.Utility.dll before being obliterated in future versions of the library~ :c
             *
             * This poor function will not be forgotten~
             */
            private static void ClosestPointsBetweenLineSegments(Vector3 lineA, Vector3 lineB, Vector3 aPoint, Vector3 bPoint, out Vector3 vector0, out Vector3 vector1)
            {
                var pointVector1 = ClosestPointOnLineSegment(lineA, lineB, aPoint);
                var pointVector2 = ClosestPointOnLineSegment(lineA, lineB, bPoint);
                vector1 = ClosestPointOnLineSegment(aPoint, bPoint, vector0 = (pointVector1 - aPoint).sqrMagnitude < (pointVector2 - bPoint).sqrMagnitude ? pointVector1 : pointVector2);
            }

            private static Vector3 ClosestPointOnLineSegment(Vector3 lineA, Vector3 lineB, Vector3 point)
            {
                var rhsVector = lineB - lineA;
                var lhsVector = point - lineA;
                return ClosestPointOnLineSegment(lineA, lineB, rhsVector, Vector3.Dot(lhsVector, rhsVector));
            }

            private static Vector3 ClosestPointOnLineSegment(Vector3 lineA, Vector3 lineB, Vector3 lhsRhs, float dot1)
            {
                return dot1 <= 0.0 ? lineA : ClosestPointOnLineSegment(lineA, lineB, lhsRhs, dot1, Vector3.Dot(lhsRhs, lhsRhs));
            }

            private static Vector3 ClosestPointOnLineSegment(Vector3 lineA, Vector3 lineB, Vector3 lhsRhs, float dot1, float dot) => dot <= dot1 ? lineB : lineA + lhsRhs * (dot1 / dot);
        }

        public class AvatarPose : GmgDynamicFunction
        {
            private bool _poseMode;
            protected internal override string Name => "Pose Avatar";
            protected override string Description => "This feature will mask the humanoid animations!\n\nIt allows you to control the transform of your bones!";
            protected internal override bool Active => _poseMode;

            protected override void Gui(ModuleVrc3 module)
            {
                _poseMode = module.PoseMode;
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                    if (GmgLayoutHelper.DebugButton(_poseMode ? "Stop Posing" : "Start Posing"))
                        Toggle(module);
            }

            protected internal override void Toggle(ModuleVrc3 module)
            {
                _poseMode = module.PoseMode = !module.PoseMode;
                if (_poseMode) module.TryAddWarning(_poseWarning);
                else module.TryRemoveWarning(_poseWarning);
                module.AvatarAnimator.applyRootMotion = _poseMode;
                if (!_poseMode || module.DummyMode == null) return;
                module.SavePose(module.DummyMode.Animator);
                var data = new TransformData(module.Avatar.transform).Difference(module.DummyMode.Avatar.transform);
                var transform = module.AvatarAnimator.GetBoneTransform(HumanBodyBones.Hips);
                module.DummyMode.StopExecution();
                module.SetPose(module.AvatarAnimator);
                data.AddTo(transform);
            }

            private readonly Vrc3Warning _poseWarning = new("You are in Pose-Mode!", "You can pose your avatar but the animations of your bones are disabled!", false);
        }

        private class AvatarBackground : GmgDynamicFunction
        {
            private static Renderer _cover;
            private static Texture _texture;
            private static Camera _cameraOb;
            private static float _distance;
            private static bool _realTime;

            private const string CmLabel = "Vrc Camera: ";
            private const string DstLabel = "Distance";

            protected override string Description => "Use this tool to set a background for your avatar!";
            protected internal override string Name => "Avatar Background";
            protected internal override bool Active => _cover != null;
            private static void ToggleOff() => UnityEngine.Object.DestroyImmediate(_cover.gameObject);
            private static bool VrcCameraRule(Camera camera) => VrcCameraRule(camera, camera.gameObject);
            private static bool SetUpCamera() => Exist(_cameraOb = Camera.allCameras.FirstOrDefault(VrcCameraRule));
            private static bool VrcCameraRule(Behaviour camera, GameObject cObj) => camera.enabled && cObj.activeInHierarchy && cObj.name == "VRCCam";

            protected override void Gui(ModuleVrc3 module) => Gui();

            protected override void Update(ModuleVrc3 module)
            {
                if (!_realTime || !_cover || !_cameraOb) return;
                FillCamera(_cameraOb, _cover.transform, _distance);
            }

            private void Gui(bool allowSceneObjects = true, string lObj = "", float leftValue = 0.1f, float rightValue = 20f)
            {
                GUILayout.Space(10);
                var option = GUILayout.Width(64);
                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        if (_cameraOb != (_cameraOb = EditorGUILayout.ObjectField(CmLabel, _cameraOb, typeof(Camera), allowSceneObjects) as Camera)) SetUp();
                        if (Math.Abs(_distance - (_distance = EditorGUILayout.Slider(DstLabel, _distance, leftValue, rightValue))) > 0.000001f) SetUp();
                        if (GuiToggle(Active)) Toggle(null);
                    }

                    if (_texture != (_texture = EditorGUILayout.ObjectField(lObj, _texture, typeof(Texture), allowSceneObjects, option) as Texture)) SetUp();
                }
            }

            private static bool GuiToggle(bool active, string text = "Enabled", string time = "Constant")
            {
                using (new GUILayout.HorizontalScope())
                {
                    var isToggle = EditorGUILayout.Toggle(text, active) != active;
                    GUILayout.FlexibleSpace();
                    _realTime = active && EditorGUILayout.Toggle(time, _realTime);
                    return isToggle;
                }
            }

            private static void SetUp()
            {
                if (!_cameraOb && !SetUpCamera()) return;
                if (!_cover) SetUpCover();
                _cover.material.mainTexture = _texture;
                if (_cameraOb) FillCamera(_cameraOb, _cover.transform, _distance);
            }

            private static void SetUpCover()
            {
                _cover = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Renderer>();
                _cover.gameObject.hideFlags = HideFlags.HideInHierarchy;
                _cover.material.shader = Shader.Find("Unlit/Texture");
            }

            protected internal override void Toggle(ModuleVrc3 module)
            {
                if (_cover) ToggleOff();
                else SetUp();
            }

            private static void FillCamera(Camera cam, Transform quad, float yDistanceZ) => FillCamera(cam, quad, yDistanceZ, cam.fieldOfView);

            private static void FillCamera(Camera cam, Transform quad, float yDistanceZ, float xFieldOfView)
            {
                var cameraTransform = cam.transform;
                yDistanceZ = cam.nearClipPlane + yDistanceZ;
                quad.rotation = cameraTransform.rotation;
                quad.position = cameraTransform.position + cameraTransform.forward * yDistanceZ;
                xFieldOfView = xFieldOfView * Mathf.Deg2Rad * 0.5f;
                yDistanceZ = Mathf.Tan(xFieldOfView) * yDistanceZ * 2f;
                xFieldOfView = yDistanceZ * cam.aspect;
                quad.localScale = new Vector3(xFieldOfView, yDistanceZ, yDistanceZ);
            }
        }

        private class TestAnimation : GmgDynamicFunction
        {
            private readonly GUILayoutOption[] _options = { GUILayout.Width(100) };
            private bool _testMode;

            private const string Label = "Animation: ";
            protected internal override string Name => "Test Animation";
            protected override string Description => "Use this tool to preview any animation in your project.\n\nYou can preview Emotes or Gestures.";
            protected internal override bool Active => _testMode;
            private static bool AnimationField(ModuleBase module) => !GmgLayoutHelper.ObjectField(Label, module.CustomAnim, module.SetCustomAnimation);

            protected override void Gui(ModuleVrc3 module)
            {
                _testMode = module.PlayingCustomAnimation;
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.GuiEnabled(!AnimationField(module)))
                    if (GUILayout.Button(_testMode ? "Stop" : "Play", _options))
                        Toggle(module);
            }

            protected internal override void Toggle(ModuleVrc3 module)
            {
                if (_testMode) module.StopCustomAnimation();
                else module.PlayCustomAnimation(module.CustomAnim);
            }
        }

        public class AnimatorPerformance : GmgDynamicFunction
        {
            private readonly List<int> _cachedIds = new();
            private readonly List<(int id, string name)> _idsNames = new();
            private static readonly Dictionary<int, string> MarkerIds = new();
            private static int _playerLoopMarkerId;
            private const string PlayerLoopName = "PlayerLoop";

            private Dictionary<string, Benchmark> _benchmark = MarkerNames.ToDictionary(nString => nString, _ => new Benchmark());

            private static readonly string[] MarkerNames =
            {
                "Update.DirectorUpdate",
                "PreLateUpdate.DirectorUpdateAnimationBegin",
                "PreLateUpdate.DirectorUpdateAnimationEnd"
            };

            private static readonly int[] Markers = new int[MarkerNames.Length];

            protected internal override string Name => "Animator Performance";
            protected override string Description => "A simple benchmark using the Unity Profiler!\n\nAimed to show animator update calls usages!";
            protected internal override bool Active => Profiler.enabled;
            public bool Watching => Active && Foldout;

            private const int Thread = 0;
            private const int Column = HierarchyFrameDataView.columnName;
            private const HierarchyFrameDataView.ViewModes View = HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName;
            private const bool SortAscending = true;

            protected override void Gui(ModuleVrc3 module)
            {
                using (new GmgLayoutHelper.GuiBackground(Color.cyan))
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                using (new GmgLayoutHelper.GuiEnabled(Active))
                {
                    foreach (var benchmarkPair in _benchmark)
                    {
                        GUILayout.Label(benchmarkPair.Key, GestureManagerStyles.MiddleStyle);
                        benchmarkPair.Value.Render();
                        GUILayout.Space(10);
                    }
                    GUILayout.Label($"\n[Result calculated on {_benchmark.FirstOrDefault().Value?.Frame ?? 0} frames]");
                }

                GUILayout.Space(10);

                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                    if (GmgLayoutHelper.DebugButton(Active ? "Stop Profiler" : "Start Profiler"))
                        Toggle(module);
            }

            protected override void Update(ModuleVrc3 module)
            {
                CacheMarkerIds();
                using var frameData = ProfilerDriver.GetHierarchyFrameDataView(ProfilerDriver.lastFrameIndex, Thread, View, Column, SortAscending);
                GetIds(frameData);
                foreach (var (intId, name) in _idsNames)
                {
                    if (!_benchmark.TryGetValue(name, out var benchmark)) benchmark = _benchmark[name] = new Benchmark();
                    benchmark.Record(frameData.GetItemColumnDataAsFloat(intId, HierarchyFrameDataView.columnTotalTime));
                }
            }

            private static void CacheMarkerIds()
            {
                if (MarkerIds.Count != 0) return;
                using var frameData = ProfilerDriver.GetRawFrameDataView(ProfilerDriver.lastFrameIndex, Thread);
                _playerLoopMarkerId = frameData.GetMarkerId(PlayerLoopName);
                for (var i = 0; i < MarkerNames.Length; i++)
                {
                    var name = MarkerNames[i];
                    var intValue = frameData.GetMarkerId(name);
                    MarkerIds.Add(intValue, name);
                    Markers[i] = intValue;
                }
            }

            private void GetIds(HierarchyFrameDataView frameData)
            {
                frameData.GetItemChildren(frameData.GetRootItemID(), _cachedIds);
                var playerIntId = _cachedIds.FirstOrDefault(intId => frameData.GetItemMarkerID(intId) == _playerLoopMarkerId);

                _idsNames.Clear();
                var found = 0;
                frameData.GetItemChildren(playerIntId, _cachedIds);
                for (var jInt = _cachedIds.Count - 1; jInt >= 0; jInt--)
                {
                    var markerInt = frameData.GetItemMarkerID(_cachedIds[jInt]);
                    if (MarkerIds.TryGetValue(markerInt, out var name))
                    {
                        var intId = _cachedIds[jInt];
                        _idsNames.Add((intId, name));
                        found++;
                    }

                    if (found == MarkerNames.Length) return;
                }
            }

            protected internal override void Toggle(ModuleVrc3 module)
            {
                Profiler.enabled = ProfilerDriver.enabled = !Active;
                if (Active) _benchmark = new Dictionary<string, Benchmark>();
            }

            private class Benchmark
            {
                public int Frame { get; private set; }

                private float _last;
                private float _count;
                private float _maximum;
                private float _average;

                public void Record(float value)
                {
                    if (value > _maximum) _maximum = value;
                    Frame++;
                    _last = value;
                    _count += value;
                    _average = _count / Frame;
                }

                public void Render()
                {
                    var option = GUILayout.Width(Math.Min((EditorGUIUtility.currentViewWidth - 64) / 3f, 115));
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"Last: {_last:F5}", option);
                        using (new GmgLayoutHelper.FlexibleScope()) GUILayout.Label($"Average: {_average:F5}", option);
                        GUILayout.Label($"Maximum: {_maximum:F5}", option);
                    }
                }
            }
        }

        private class Customization : GmgDynamicFunction
        {
            protected internal override string Name => "Radial Menu";
            protected override string Description => "Customize the colors of the radial menu!";
            protected internal override bool Active => false;

            private Color _customMain = RadialMenuUtility.Colors.CustomMain;
            private Color _customBorder = RadialMenuUtility.Colors.CustomBorder;
            private Color _customSelected = RadialMenuUtility.Colors.CustomSelected;

            protected override void Gui(ModuleVrc3 module)
            {
                _customMain = GmgLayoutHelper.ResetColorField("Main Color: ", _customMain, RadialMenuUtility.Colors.Default.Main);
                _customBorder = GmgLayoutHelper.ResetColorField("Border Color: ", _customBorder, RadialMenuUtility.Colors.Default.Border);
                _customSelected = GmgLayoutHelper.ResetColorField("Selected Color:", _customSelected, RadialMenuUtility.Colors.Default.Selected);

                GUILayout.Space(10);

                if (GUILayout.Button("Save")) Save(module);
            }

            private void Save(ModuleVrc3 module)
            {
                RadialMenuUtility.Colors.SaveColors(_customMain, _customBorder, _customSelected);
                module.ReloadRadials();
            }

            protected internal override void Toggle(ModuleVrc3 module)
            {
            }
        }

        private class BoolProperty
        {
            private readonly string _key;

            public BoolProperty(string key) => _key = key;

            internal bool Property
            {
                get => EditorPrefs.GetBool(_key);
                set => EditorPrefs.SetBool(_key, value);
            }
        }

        private class StringProperty
        {
            private readonly string _key;

            public StringProperty(string key) => _key = key;

            internal string Property
            {
                get => EditorPrefs.GetString(_key);
                set => EditorPrefs.SetString(_key, value);
            }
        }
    }
}
#endif