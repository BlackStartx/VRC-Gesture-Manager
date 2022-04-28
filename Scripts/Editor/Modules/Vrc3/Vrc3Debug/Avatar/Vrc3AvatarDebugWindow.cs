#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Vrc3Debug.Avatar
{
    internal class Vrc3AvatarDebugWindow : EditorWindow
    {
        private ModuleVrc3 _source;
        private Vector2 _scroll;

        internal static Vrc3AvatarDebugWindow Create(ModuleVrc3 source)
        {
            var instance = CreateInstance<Vrc3AvatarDebugWindow>();
            instance.titleContent = new GUIContent("[Debug Window] Gesture Manager");
            instance._source = source;
            instance.Show();
            return instance;
        }

        internal static Vrc3AvatarDebugWindow Close(Vrc3AvatarDebugWindow source)
        {
            source.Close();
            return null;
        }

        public void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            if (_source == null) Close();
            else DebugGUI();
        }

        private void DebugGUI()
        {
            GUILayout.Label("Gesture Manager - Avatar Debug Window", GestureManagerStyles.Header);
            var fullScreen = Screen.width > 1279;
            if (!fullScreen) _source.DebugToolBar = Static.DebugToolbar(_source.DebugToolBar);
            _scroll = GUILayout.BeginScrollView(_scroll);
            _source.DebugContext(rootVisualElement, null, 0, Screen.width - 60, fullScreen);
            GUILayout.EndScrollView();
        }

        internal static class Static
        {
            internal static int DebugToolbar(int toolbar) => GUILayout.Toolbar(toolbar, new[] { "Parameters", "Tracking Control", "Animator States" });

            internal static void DebugLayout(ModuleVrc3 module, float width, bool fullscreen, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data)
            {
                if (fullscreen) DebugLayoutFullScreen(module, width, data);
                else DebugLayoutCompact(module, width, data);
            }

            private static void DebugLayoutCompact(ModuleVrc3 module, float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data)
            {
                switch (module.DebugToolBar)
                {
                    case 0:
                        ParametersLayout(module, width);
                        break;
                    case 1:
                        TrackingControlLayout(width, data, module.TrackingControls, module.LocomotionDisabled, module.PoseSpace);
                        break;
                    case 2:
                        AnimatorsLayout(width, data);
                        break;
                }
            }

            private static void DebugLayoutFullScreen(ModuleVrc3 module, float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data)
            {
                width /= 3;
                GUILayout.BeginHorizontal();
                ParametersLayout(module, width);
                TrackingControlLayout(width, data, module.TrackingControls, module.LocomotionDisabled, module.PoseSpace);
                AnimatorsLayout(width, data);
                GUILayout.EndHorizontal();
            }

            internal static void DummyLayout(string mode)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Space(100);
                GUILayout.Label($"Debug mode is disabled in {mode}-Mode!", GestureManagerStyles.TextError);
                GUILayout.Space(20);
                GUILayout.Label($"Exit {mode}-Mode to show the debug information of your avatar!", GestureManagerStyles.SubHeader);
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
            }

            private static void ParametersLayout(ModuleVrc3 module, float width)
            {
                var inWidth = width / 3;
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label("Parameters", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Parameter", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in module.Params) GUILayout.Label(paramPair.Key);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Type", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in module.Params) GUILayout.Label(paramPair.Value.Type.ToString());
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Value", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in module.Params) ParametersLayoutValue(module, paramPair);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            private static void ParametersLayoutValue(ModuleVrc3 module, KeyValuePair<string, Vrc3Param> paramPair)
            {
                if (module.Edit != paramPair.Key)
                {
                    GmgLayoutHelper.GuiLabel(paramPair.Value.LabelTuple());
                    var rect = GUILayoutUtility.GetLastRect();
                    rect.x += rect.width - 15;
                    if (GUI.Toggle(rect, false, "")) module.Edit = paramPair.Key;
                }
                else paramPair.Value.FieldTuple(module);
            }

            private static void TrackingControlLayout(float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data, Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> trackingControls, bool locomotionDisabled, bool poseSpace)
            {
                var inWidth = width / 2;
                GUILayout.BeginVertical(GUILayout.Width(width));

                GUILayout.Label("Tracking Control", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Name", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var trackingPair in trackingControls) GUILayout.Label(trackingPair.Key, GUILayout.Width(inWidth));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Value", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var trackingPair in trackingControls) GmgLayoutHelper.GuiLabel(TrackingTuple(trackingPair.Value), GUILayout.Width(inWidth));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Label("Animation Controllers", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Name", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var controllerPair in data) GUILayout.Label(controllerPair.Key.ToString(), GUILayout.Width(inWidth));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Weight", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var controllerPair in data) GUILayout.Label(controllerPair.Value.Weight.Weight.ToString("0.00"), GUILayout.Width(inWidth));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Label("Miscellaneous", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Locomotion", GUILayout.Width(inWidth));
                GUILayout.Label("Pose Space", GUILayout.Width(inWidth));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GmgLayoutHelper.GuiLabel(LocomotionTuple(!locomotionDisabled), GUILayout.Width(inWidth));
                GmgLayoutHelper.GuiLabel(PoseSpaceTuple(poseSpace), GUILayout.Width(inWidth));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            private static void AnimatorsLayout(float width, IReadOnlyDictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data)
            {
                var inWidth = width / 3;
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label("Animator States", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                foreach (var sortPair in ModuleVrc3Styles.Data.SortValue.Where(sortPair => data.ContainsKey(sortPair.Key)))
                {
                    GUILayout.Label(sortPair.Key.ToString(), GestureManagerStyles.GuiDebugTitle);
                    var animator = data[sortPair.Key].Playable;
                    var layerList = Enumerable.Range(0, animator.GetLayerCount()).ToList();

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.BeginVertical();
                    GUILayout.Label("Name", GestureManagerStyles.SubHeader, GUILayout.Width(inWidth));
                    foreach (var intLayer in layerList) GUILayout.Label(animator.GetLayerName(intLayer), GUILayout.Width(inWidth));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Weight", GestureManagerStyles.SubHeader, GUILayout.Width(inWidth));
                    foreach (var intLayer in layerList) GUILayout.Label(animator.GetLayerWeight(intLayer).ToString("0.00"), GUILayout.Width(inWidth));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label("State", GestureManagerStyles.SubHeader, GUILayout.Width(inWidth));
                    foreach (var infos in layerList.Select(intLayer => animator.GetCurrentAnimatorClipInfo(intLayer)))
                        GUILayout.Label(infos.Length == 0 ? "[UNKNOWN]" : infos[0].clip.name, GUILayout.Width(inWidth));

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            private static (Color? color, string text) TrackingTuple(VRC_AnimatorTrackingControl.TrackingType trackingType)
            {
                switch (trackingType)
                {
                    case VRC_AnimatorTrackingControl.TrackingType.NoChange:
                        return (Color.yellow, "No Change");
                    case VRC_AnimatorTrackingControl.TrackingType.Tracking:
                        return (Color.green, "Tracking");
                    case VRC_AnimatorTrackingControl.TrackingType.Animation:
                        return (Color.red, "Animation");
                    default: throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null);
                }
            }

            private static (Color? color, string text) LocomotionTuple(bool locomotion) => locomotion ? (Color.green, "Enabled") : (Color.red, "Disabled");

            private static (Color? color, string text) PoseSpaceTuple(bool poseSpace) => poseSpace ? (Color.green, "Pose") : (Color.white, "Default");
        }

        internal static class Text
        {
            public static class W
            {
                public const string Subtitle = "The debug view is now floating,";
                public const string Message = "♥   you can switch to other tabs now   ♥";
                public const string Hint = "or you can dock the window back in by using the button bellow!";
                public const string Button = "Dock Debug Window";
            }

            public static class D
            {
                public const string Subtitle = "Select a category from the toolbar bellow.";
                public const string Hint = "For an easier experience you can undock the window with the button bellow!";
                public const string Button = "Undock Debug Window";
            }
        }
    }
}
#endif