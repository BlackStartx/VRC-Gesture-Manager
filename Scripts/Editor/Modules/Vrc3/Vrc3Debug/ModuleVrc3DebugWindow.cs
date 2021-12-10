#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.Vrc3Debug
{
    public class ModuleVrc3DebugWindow : EditorWindow
    {
        private ModuleVrc3 _source;
        private Vector2 _scroll;

        internal static ModuleVrc3DebugWindow Create(ModuleVrc3 source)
        {
            var instance = CreateInstance<ModuleVrc3DebugWindow>();
            instance.titleContent = new GUIContent("[Debug Window] Gesture Manager");
            instance._source = source;
            instance.Show();
            return instance;
        }

        public void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            if (_source == null) Close();
            else DebugGUI();
        }

        private void DebugGUI()
        {
            GUILayout.Label("Gesture Manager - Debug Window", GestureManagerStyles.Header);
            var fullScreen = Screen.width > 1279;
            if (!fullScreen) _source.DebugToolBar = Static.DebugToolbar(_source.DebugToolBar);
            _scroll = GUILayout.BeginScrollView(_scroll);
            _source.DebugContext(Screen.width - 60, fullScreen);
            GUILayout.EndScrollView();
        }

        public static class Static
        {
            public static int DebugToolbar(int toolbar) => GUILayout.Toolbar(toolbar, new[] { "Parameters", "Tracking Control", "Animator States" });

            public static void DebugLayout(ModuleVrc3 module, float width, bool fullscreen, AnimatorControllerPlayable[] animatorPlayables)
            {
                if (fullscreen) DebugLayoutFullScreen(module, width, animatorPlayables);
                else DebugLayoutCompact(module, width, animatorPlayables);
            }

            private static void DebugLayoutCompact(ModuleVrc3 module, float width, IReadOnlyList<AnimatorControllerPlayable> animatorPlayables)
            {
                switch (module.DebugToolBar)
                {
                    case 0:
                        ParametersLayout(width, module.Params);
                        break;
                    case 1:
                        TrackingControlLayout(width, module.FromBlend, module.TrackingControls, module.LocomotionDisabled, module.PoseSpace);
                        break;
                    case 2:
                        AnimatorsLayout(width, animatorPlayables);
                        break;
                }
            }

            private static void DebugLayoutFullScreen(ModuleVrc3 module, float width, IReadOnlyList<AnimatorControllerPlayable> animatorPlayables)
            {
                width /= 3;
                GUILayout.BeginHorizontal();
                ParametersLayout(width, module.Params);
                TrackingControlLayout(width, module.FromBlend, module.TrackingControls, module.LocomotionDisabled, module.PoseSpace);
                AnimatorsLayout(width, animatorPlayables);
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

            private static void ParametersLayout(float width, Dictionary<string, Vrc3Param> moduleParams)
            {
                var inWidth = width / 3;
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label("Parameters", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Parameter", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in moduleParams) GUILayout.Label(paramPair.Key);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Type", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in moduleParams) GUILayout.Label(paramPair.Value.Type.ToString());
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Value", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var paramPair in moduleParams) GmgLayoutHelper.GuiLabel(paramPair.Value.LabelTuple());
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            private static void TrackingControlLayout(float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorControllerWeight> controllers, Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> trackingControls, bool locomotionDisabled, bool poseSpace)
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
                foreach (var controllerPair in controllers) GUILayout.Label(controllerPair.Key.ToString(), GUILayout.Width(inWidth));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Weight", GestureManagerStyles.GuiDebugTitle, GUILayout.Width(inWidth));
                foreach (var controllerPair in controllers) GUILayout.Label(controllerPair.Value.Weight.ToString("0.00"), GUILayout.Width(inWidth));
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

            private static void AnimatorsLayout(float width, IReadOnlyList<AnimatorControllerPlayable> animatorPlayables)
            {
                var inWidth = width / 3;
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label("Animator States", GestureManagerStyles.GuiHandTitle, GUILayout.Width(width));
                foreach (var sortPair in ModuleVrc3Styles.Data.SortValue)
                {
                    GUILayout.Label(sortPair.Key.ToString(), GestureManagerStyles.GuiDebugTitle);
                    var animator = animatorPlayables[sortPair.Value];
                    var layerList = Enumerable.Range(0, animator.GetLayerCount()).ToList();

                    GUILayout.BeginHorizontal();
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

        public static class Text
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
                public const string Subtitle = "Select a category" + " from the toolbar bellow.";
                public const string Hint = "For an easier experience you can undock the window with the button bellow!";
                public const string Button = "Undock Debug Window";
            }
        }
    }
}
#endif