#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.DummyModes;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.Utility;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class AvatarTools
    {
        private UpdateSceneCamera _sceneCamera;
        private UpdateSceneCamera SceneCamera => _sceneCamera ?? (_sceneCamera = new UpdateSceneCamera());

        private ClickableContacts _clickableContacts;
        private ClickableContacts ContactsClickable => _clickableContacts ?? (_clickableContacts = new ClickableContacts());

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
            AnimationTest.Display(module);
            GUILayout.Label("Customization", GestureManagerStyles.Header);
            CustomizationTool.Display(module);
        }

        internal void OnUpdate(ModuleVrc3 module)
        {
            SceneCamera.OnUpdate(module);
            AnimationTest.OnUpdate(module);
            ContactsClickable.OnUpdate(module);
        }

        internal void OnLateUpdate(ModuleVrc3 module)
        {
            SceneCamera.OnLateUpdate(module);
            AnimationTest.OnLateUpdate(module);
            ContactsClickable.OnLateUpdate(module);
        }

        private class UpdateSceneCamera : GmgDynamicFunction
        {
            private static Camera _camera;

            protected override string Name => "Scene Camera";
            protected override string Description => "This will match your game view with your scene view!\nClick the button to setup the main camera automatically~";
            protected override bool Active => _camera;

            protected override void Update(ModuleVrc3 module)
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (!sceneView) return;
                var camera = sceneView.camera;
                if (!camera) return;
                var sceneTransform = camera.transform;
                var transform = _camera.transform;
                var positionVector = sceneTransform.position;
                transform.rotation = sceneTransform.rotation;
                transform.position = new Vector3(positionVector.x, positionVector.y + 0.001f, positionVector.z);
            }

            protected override void Gui(ModuleVrc3 module)
            {
                if (GmgLayoutHelper.ButtonObjectField("Scene Camera: ", _camera, _camera ? 'X' : 'A', camera => _camera = camera)) _camera = _camera ? null : Camera.main;
            }
        }

        private class ClickableContacts : GmgDynamicFunction
        {
            private bool _active;
            private readonly Camera _camera = Camera.main;
            private readonly HashSet<ContactReceiver> _activeContact = new HashSet<ContactReceiver>();

            protected override string Name => "Clickable Contacts";
            protected override string Description => "Click and trigger Avatar Contacts with your mouse!\nLike you can do with PhysBones~";
            protected override bool Active => _active;

            protected override void Update(ModuleVrc3 module) => LateUpdate(module);

            protected override void LateUpdate(ModuleVrc3 module)
            {
                if (Input.GetMouseButton(0)) OnClick(module);
                if (Input.GetMouseButtonUp(0)) Disable();
            }

            private void OnClick(ModuleVrc3 module)
            {
                if (!_camera) return;
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                CheckRay(module, ray.origin, ray.origin + ray.direction * 1000f);
            }

            private void CheckRay(ModuleVrc3 module, Vector3 s, Vector3 e)
            {
                var manager = ContactManager.Inst;
                if (!manager) return;

                foreach (var receiver in module.Contacts)
                {
                    var distance = ValueFor(manager, receiver, s, e);
                    receiver.SetParameter(distance);
                    if (distance == 0f) _activeContact.Remove(receiver);
                    else _activeContact.Add(receiver);
                }
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
                Vector3 result0;
                Vector3 result1;
                if (receiver.shapeType == ContactBase.ShapeType.Sphere) PhysicsUtil.ClosestPointsBetweenLineSegments(s, e, shape.outPos0, shape.outPos0, out result0, out result1);
                else PhysicsUtil.ClosestPointsBetweenLineSegments(s, e, shape.outPos0, shape.outPos1, out result0, out result1);
                return (result0 - result1).magnitude - radius;
            }

            private void Disable()
            {
                foreach (var receiver in _activeContact) receiver.SetParameter(0f);
            }

            protected override void Gui(ModuleVrc3 module) => _active = GmgLayoutHelper.ToggleRight("Activate clickable contacts: ", _active);
        }

        private class TestAnimation : GmgDynamicFunction
        {
            private bool _testMode;
            private AnimationClip _selectingCustomAnim;

            protected override string Name => "Test Animation";
            protected override string Description => "Use this tool to preview any animation in your project.\nYou can preview Emotes or Gestures.";
            protected override bool Active => _testMode;

            protected override void Gui(ModuleVrc3 module)
            {
                _testMode = module.Manager.OnCustomAnimation;

                var isEditMode = module.DummyMode is Vrc3EditMode;
                using (new GUILayout.HorizontalScope())
                {
                    _selectingCustomAnim = GmgLayoutHelper.ObjectField("Animation: ", _selectingCustomAnim, module.Manager.SetCustomAnimation);

                    GUI.enabled = _selectingCustomAnim && !isEditMode;
                    switch (_testMode)
                    {
                        case true when GUILayout.Button("Stop", GestureManagerStyles.GuiGreenButton):
                            module.Manager.StopCustomAnimation();
                            break;
                        case false when GUILayout.Button("Play", GUILayout.Width(100)):
                            module.Manager.PlayCustomAnimation(_selectingCustomAnim);
                            break;
                    }

                    GUI.enabled = true;
                }
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

            protected abstract void Gui(ModuleVrc3 module);

            protected virtual void Update(ModuleVrc3 module)
            {
            }

            protected virtual void LateUpdate(ModuleVrc3 module)
            {
            }
        }
    }
}
#endif