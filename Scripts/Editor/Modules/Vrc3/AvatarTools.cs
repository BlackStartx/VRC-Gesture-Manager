#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class AvatarTools
    {
        private static Camera ToolCamera => Camera.allCameras.FirstOrDefault(CameraRule);
        private static bool CameraRule(Camera camera) => camera.enabled && camera.gameObject.activeInHierarchy;

        private static GUIStyle _smallText;
        private static GUIStyle SmallText => _smallText ?? (_smallText = new GUIStyle(GUI.skin.toggle) { fontSize = 10 });

        private UpdateSceneCamera _sceneCamera;
        private UpdateSceneCamera SceneCamera => _sceneCamera ?? (_sceneCamera = new UpdateSceneCamera());

        private ClickableContacts _clickableContacts;
        private ClickableContacts ContactsClickable => _clickableContacts ?? (_clickableContacts = new ClickableContacts());

        private AvatarBackground _avatarBackground;
        private AvatarBackground BackgroundAvatar => _avatarBackground ?? (_avatarBackground = new AvatarBackground());

        private AvatarPose _avatarPose;
        private AvatarPose PoseAvatar => _avatarPose ?? (_avatarPose = new AvatarPose());

        private TestAnimation _testAnimation;
        private TestAnimation AnimationTest => _testAnimation ?? (_testAnimation = new TestAnimation());

        private Customization _customization;
        private Customization CustomizationTool => _customization ?? (_customization = new Customization());

        internal void Gui(ModuleVrc3 module)
        {
            GUILayout.Label("Gesture Manager Tools", GestureManagerStyles.Header);
            GUILayout.Label("A collection of some small utility functions~", GestureManagerStyles.SubHeader);
            GUILayout.Space(10);
            SceneCamera.Display(module);
            ContactsClickable.Display(module);
            GUILayout.Label("Extra Tools", GestureManagerStyles.Header);
            BackgroundAvatar.Display(module);
            PoseAvatar.Display(module);
            AnimationTest.Display(module);
            GUILayout.Label("Customization", GestureManagerStyles.Header);
            CustomizationTool.Display(module);
        }

        internal void OnUpdate(ModuleVrc3 module)
        {
            SceneCamera.OnUpdate(module);
            BackgroundAvatar.OnUpdate(module);
            ContactsClickable.OnUpdate(module);
        }

        internal void OnLateUpdate(ModuleVrc3 module)
        {
            ContactsClickable.OnLateUpdate(module);
        }

        internal void OnDrawGizmos()
        {
            ContactsClickable.OnDrawGizmos();
        }

        private class UpdateSceneCamera : GmgDynamicFunction
        {
            private static Camera _camera;
            private readonly BoolProperty _isActive = new BoolProperty("GM3 SceneCamera");

            protected override string Name => "Scene Camera";
            protected override string Description => "This will match your game view with your scene view!\nClick the button to setup the main camera automatically~";
            protected override bool Active => _isActive.Property;

            public UpdateSceneCamera() => Toggle(_isActive.Property ? ToolCamera : null);

            protected override void Gui(ModuleVrc3 module)
            {
                if (GmgLayoutHelper.ButtonObjectField("Scene Camera: ", _camera, _camera ? 'X' : 'A', Toggle)) AutoToggle();
                _isActive.Property = _camera != null;
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

            private static void AutoToggle() => Toggle(_camera ? null : ToolCamera);

            private static void Toggle(Camera camera) => _camera = camera;
        }

        private class ClickableContacts : GmgDynamicFunction
        {
            private class Flat
            {
                public float Float;
            }

            private readonly BoolProperty _isActive = new BoolProperty("GM3 ClickableContacts");
            private readonly StringProperty _tag = new StringProperty("GM3 ClickableContacts Tag");
            private readonly Dictionary<ContactReceiver, Flat> _activeContact = new Dictionary<ContactReceiver, Flat>();

            protected override string Name => "Clickable Contacts";
            protected override string Description => "Click and trigger Avatar Contacts with your mouse!\nLike you can do with PhysBones~";
            protected override bool Active => _isActive.Property;

            private static Mesh _sphereMesh;
            private static Mesh SphereMesh => _sphereMesh ? _sphereMesh : _sphereMesh = FetchSpherePrimitive();

            private static Mesh _capsuleMesh;
            private static Mesh CapsuleMesh => _capsuleMesh ? _capsuleMesh : _capsuleMesh = FetchCapsulePrimitive();

            private Camera _camera = ToolCamera;
            private Camera Camera => _camera ? _camera : _camera = ToolCamera;

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

            private void CheckRay(ModuleVrc3 module, Vector3 s, Vector3 e)
            {
                var manager = ContactManager.Inst;
                if (!manager) return;
                foreach (var receiver in module.Receivers.Where(receiver => string.IsNullOrEmpty(_tag.Property) || receiver.collisionTags.Contains(_tag.Property))) OnContactValue(receiver, ValueFor(manager, receiver, s, e));
            }

            private static float ValueFor(ContactManager manager, ContactReceiver receiver, Vector3 s, Vector3 e)
            {
                var isProximity = receiver.receiverType == ContactReceiver.ReceiverType.Proximity;
                var distance = DistanceFrom(manager, receiver, s, e, out var radius);
                if (isProximity) distance -= radius;
                if (isProximity) return Mathf.Clamp(-distance / radius, 0f, 1f);
                return distance < 0 ? 1f : 0f;
            }

            private static float DistanceFrom(ContactManager manager, ContactBase receiver, Vector3 s, Vector3 e, out float radius)
            {
                receiver.InitShape();
                manager.collision.UpdateShapeData(receiver.shape);
                var shape = manager.collision.GetShapeData(receiver.shape);
                var scaleVector = receiver.transform.lossyScale;
                radius = receiver.radius * Mathf.Max(scaleVector.x, scaleVector.y, scaleVector.z);
                var vectorLine = receiver.shapeType == ContactBase.ShapeType.Sphere ? shape.outPos0 : shape.outPos1;
                ClosestPointsBetweenLineSegments(s, e, shape.outPos0, vectorLine, out var vector0, out var vector1);
                return (vector0 - vector1).magnitude - radius;
            }

            /*
             * Saved from the Plugins\VRC.Utility.dll before being obliterate in future versions of the library~ :c
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

        private class TestAnimation : GmgDynamicFunction
        {
            private readonly GUILayoutOption[] _options = { GUILayout.Width(100) };
            private bool _testMode;

            private const string LabelAnimation = "Animation: ";

            protected override string Name => "Test Animation";
            protected override string Description => "Use this tool to preview any animation in your project.\nYou can preview Emotes or Gestures.";
            protected override bool Active => _testMode;

            protected override void Gui(ModuleVrc3 module)
            {
                _testMode = module.Manager.PlayingCustomAnimation;
                using (new GUILayout.HorizontalScope())
                {
                    if (!GmgLayoutHelper.ObjectField(LabelAnimation, module.Manager.customAnim, module.Manager.SetCustomAnimation)) GUI.enabled = false;
                    if (GUILayout.Button(_testMode ? "Stop" : "Play", _options)) ToggleTesting(module);
                    GUI.enabled = true;
                }
            }

            private void ToggleTesting(ModuleBase module)
            {
                if (_testMode) module.Manager.StopCustomAnimation();
                else module.Manager.PlayCustomAnimation(module.Manager.customAnim);
            }
        }

        private class AvatarBackground : GmgDynamicFunction
        {
            private readonly GUILayoutOption[] _options = { GUILayout.Height(5) };

            private static Renderer _cover;
            private static Texture _texture;
            private static Camera _cameraOb;
            private static float _distance;
            private static bool _realTime;

            private const string CmLabel = "Vrc Camera: ";
            private const string DstLabel = "Distance";
            private const string ThumbnailText = "Use this tool to set a background for your avatar!";

            protected override string Description => null;
            protected override string Name => "Avatar Background";
            protected override bool Active => _cover != null;
            private static void ToggleOff() => UnityEngine.Object.DestroyImmediate(_cover.gameObject);
            private static bool VrcCameraRule(Camera camera) => VrcCameraRule(camera, camera.gameObject);
            private static bool SetUpCamera() => (_cameraOb = Camera.allCameras.FirstOrDefault(VrcCameraRule)) != null;
            private static bool VrcCameraRule(Behaviour camera, GameObject cObj) => camera.enabled && cObj.activeInHierarchy && cObj.name == "VRCCam";

            protected override void Gui(ModuleVrc3 module) => Gui(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.label, _options));

            protected override void Update(ModuleVrc3 module)
            {
                if (!_realTime || !_cover || !_cameraOb) return;
                FillCamera(_cameraOb, _cover.transform, _distance);
            }

            private void Gui(Rect rect, bool allowSceneObjects = true, string objLabel = "", float leftValue = 0.1f, float rightValue = 20f)
            {
                rect.y -= 51;
                rect.height = 64;
                GUI.Label(rect, ThumbnailText, GestureManagerStyles.SubHeader);
                rect.y -= 9;
                if (_texture != (_texture = EditorGUI.ObjectField(rect, objLabel, _texture, typeof(Texture), allowSceneObjects) as Texture)) SetUp();
                if (_cameraOb != (_cameraOb = EditorGUILayout.ObjectField(CmLabel, _cameraOb, typeof(Camera), allowSceneObjects) as Camera)) SetUp();
                if (Math.Abs(_distance - (_distance = EditorGUILayout.Slider(DstLabel, _distance, leftValue, rightValue))) > 0.000001f) SetUp();
                if (GuiToggle(rect, Active)) Toggle();
            }

            private static bool GuiToggle(Rect rect, bool active, string text = " Enabled", string time = " Constant")
            {
                rect.height = 15;
                rect.width = 80;
                rect.y -= 3;
                rect.x -= 2;
                var isToggle = GUI.Toggle(rect, active, text, SmallText) != active;
                rect.y += 15;
                _realTime = active && GUI.Toggle(rect, _realTime, time, SmallText);
                return isToggle;
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

            private static void Toggle()
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

        private class AvatarPose : GmgDynamicFunction
        {
            private bool _poseMode;
            protected override string Name => "Pose Avatar";
            protected override string Description => "Use this feature to start posing your avatar!";
            protected override bool Active => _poseMode;

            protected override void Gui(ModuleVrc3 module)
            {
                _poseMode = module.DummyMode is Vrc3PoseMode;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GmgLayoutHelper.DebugButton(_poseMode ? "Stop Posing" : "Start Posing")) TogglePosing(module);
                    GUILayout.FlexibleSpace();
                }
            }

            private void TogglePosing(ModuleVrc3 module)
            {
                if (_poseMode) module.DummyMode.StopExecution();
                else module.DummyMode = new Vrc3PoseMode(module);
            }
        }

        private class Customization : GmgDynamicFunction
        {
            protected override string Name => "Radial Menu";
            protected override string Description => "Customize the colors of the radial menu!";
            protected override bool Active => false;

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
        }

        private abstract class GmgDynamicFunction
        {
            protected abstract string Name { get; }
            protected abstract string Description { get; }
            protected abstract bool Active { get; }

            internal void Display(ModuleVrc3 module)
            {
                using (new GmgLayoutHelper.GuiBackground(Active ? Color.green : GUI.backgroundColor))
                using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError))
                {
                    GUILayout.Label(Name, GestureManagerStyles.ToolHeader);
                    GUILayout.Label(Description, GestureManagerStyles.SubHeader);
                    GUILayout.Space(10);
                    Gui(module);
                }
            }

            internal void OnUpdate(ModuleVrc3 module)
            {
                if (Active) Update(module);
            }

            internal void OnLateUpdate(ModuleVrc3 module)
            {
                if (Active) LateUpdate(module);
            }

            internal void OnDrawGizmos()
            {
                if (Active) DrawGizmos();
            }

            protected abstract void Gui(ModuleVrc3 module);

            protected virtual void Update(ModuleVrc3 module)
            {
            }

            protected virtual void LateUpdate(ModuleVrc3 module)
            {
            }

            protected virtual void DrawGizmos()
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