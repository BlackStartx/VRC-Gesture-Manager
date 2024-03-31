#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Vrc3Debug.Avatar
{
    internal class Vrc3AvatarDebugWindow : EditorWindow
    {
        private ModuleVrc3 _source;
        private Vector2 _scroll;

        private static Color Color => EditorGUIUtility.isProSkin ? Color.magenta : Color.cyan;

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

        public void Update() => Repaint();

        private void OnGUI()
        {
            if (_source == null) Close();
            else DebugGUI();
        }

        private void DebugGUI()
        {
            GUILayout.Label("Gesture Manager - Avatar Debug Window", GestureManagerStyles.Header);
            var isFullScreen = EditorGUIUtility.currentViewWidth > 1279;
            if (!isFullScreen) _source.DebugToolBar = Static.DebugToolbar(_source.DebugToolBar);
            _scroll = GUILayout.BeginScrollView(_scroll);
            _source.DebugContext(rootVisualElement, null, 0, EditorGUIUtility.currentViewWidth - 60, isFullScreen);
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
                        AnimatorsLayout(width, data, module.AnimationHashSet);
                        break;
                }
            }

            private static void DebugLayoutFullScreen(ModuleVrc3 module, float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data)
            {
                width /= 3;
                using (new GUILayout.HorizontalScope())
                {
                    ParametersLayout(module, width);
                    TrackingControlLayout(width, data, module.TrackingControls, module.LocomotionDisabled, module.PoseSpace);
                    AnimatorsLayout(width, data, module.AnimationHashSet);
                }
            }

            internal static void DummyLayout(string mode)
            {
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    GUILayout.Space(100);
                    GUILayout.Label($"Debug mode is disabled in {mode}-Mode!", GestureManagerStyles.TextError);
                    GUILayout.Space(20);
                    GUILayout.Label($"Exit {mode}-Mode to show the debug information of your avatar!", GestureManagerStyles.Centered);
                    GUILayout.Space(100);
                }
            }

            private static void ParametersLayout(ModuleVrc3 module, float width)
            {
                var widthOption = GUILayout.Width(width);
                var innerOption = GUILayout.Width(width / 3);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Parameters", GestureManagerStyles.GuiHandTitle, widthOption);
                    module.ParamFilterSearch();
                    GUILayout.Space(5);
                    using (new GUILayout.HorizontalScope(widthOption))
                    {
                        GUILayout.Label("Parameter", GestureManagerStyles.GuiDebugTitle, innerOption);
                        GUILayout.Label("Type", GestureManagerStyles.GuiDebugTitle, innerOption);
                        GUILayout.Label("Value", GestureManagerStyles.GuiDebugTitle, innerOption);
                    }

                    var color = GUI.backgroundColor;
                    var intTime = Vrc3Param.Time;
                    foreach (var paramPair in module.FilteredParams)
                    {
                        GUILayout.Space(-4);
                        GUI.backgroundColor = Color.Lerp(Color, color, (intTime - paramPair.Value.LastUpdate) / 100f);
                        using (new GUILayout.HorizontalScope(GUI.skin.box, widthOption))
                        {
                            GUILayout.Label(paramPair.Key, innerOption);
                            GUILayout.Label(paramPair.Value.TypeText, innerOption);
                            ParametersLayoutValue(module, paramPair, innerOption);
                        }
                    }

                    GUI.backgroundColor = color;
                }
            }

            private static void ParametersLayoutValue(ModuleVrc3 module, KeyValuePair<string, Vrc3Param> paramPair, GUILayoutOption innerOption)
            {
                if (module.Edit != paramPair.Key)
                {
                    GmgLayoutHelper.GuiLabel(LabelTuple(paramPair.Value), innerOption);
                    var rect = GUILayoutUtility.GetLastRect();
                    rect.x += rect.width - 20;
                    rect.width = 15;
                    if (GUI.Toggle(rect, false, "")) module.Edit = paramPair.Key;
                }
                else FieldTuple(paramPair.Value, module, innerOption);
            }

            private static void TrackingControlLayout(float width, Dictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data, Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> trackingControls, bool locomotionDisabled, bool poseSpace)
            {
                var widthOption = GUILayout.Width(width);
                var innerOption = GUILayout.Width(width / 2);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Tracking Control", GestureManagerStyles.GuiHandTitle, widthOption);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Name", GestureManagerStyles.GuiDebugTitle, innerOption);
                            foreach (var trackingPair in trackingControls) GUILayout.Label(trackingPair.Key, innerOption);
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Value", GestureManagerStyles.GuiDebugTitle, innerOption);
                            foreach (var trackingPair in trackingControls) GmgLayoutHelper.GuiLabel(TrackingTuple(trackingPair.Value), innerOption);
                        }
                    }

                    GUILayout.Label("Animation Controllers", GestureManagerStyles.GuiHandTitle, widthOption);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Name", GestureManagerStyles.GuiDebugTitle, innerOption);
                            foreach (var controllerPair in data) GUILayout.Label(controllerPair.Key.ToString(), innerOption);
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Weight", GestureManagerStyles.GuiDebugTitle, innerOption);
                            foreach (var controllerPair in data) GUILayout.Label(controllerPair.Value.Weight.Weight.ToString("0.00"), innerOption);
                        }
                    }

                    GUILayout.Label("Miscellaneous", GestureManagerStyles.GuiHandTitle, widthOption);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Locomotion", innerOption);
                            GUILayout.Label("Pose Space", innerOption);
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GmgLayoutHelper.GuiLabel(LocomotionTuple(!locomotionDisabled), innerOption);
                            GmgLayoutHelper.GuiLabel(PoseSpaceTuple(poseSpace), innerOption);
                        }
                    }
                }
            }

            private static void AnimatorsLayout(float width, IReadOnlyDictionary<VRCAvatarDescriptor.AnimLayerType, ModuleVrc3.LayerData> data, IReadOnlyDictionary<int, VRCAvatarDescriptor.DebugHash> debugDic)
            {
                var widthOption = GUILayout.Width(width);
                var innerOption = GUILayout.Width(width / 3);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Animator States", GestureManagerStyles.GuiHandTitle, widthOption);
                    foreach (var sortPair in ModuleVrc3Styles.Data.SortValue.Where(sortPair => data.ContainsKey(sortPair.Key)))
                    {
                        GUILayout.Label(sortPair.Key.ToString(), GestureManagerStyles.GuiDebugTitle, widthOption);
                        var animator = data[sortPair.Key].Playable;
                        var layerList = Enumerable.Range(0, animator.GetLayerCount()).ToList();

                        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Name", GestureManagerStyles.Centered, innerOption);
                                foreach (var intLayer in layerList) GUILayout.Label(animator.GetLayerName(intLayer), innerOption);
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Weight", GestureManagerStyles.Centered, innerOption);
                                foreach (var intLayer in layerList) GUILayout.Label(animator.GetLayerWeight(intLayer).ToString("0.00"), innerOption);
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("State", GestureManagerStyles.Centered, innerOption);
                                foreach (var state in layerList.Select(intLayer => animator.GetCurrentAnimatorStateInfo(intLayer)))
                                    GUILayout.Label(debugDic.TryGetValue(state.fullPathHash, out var hash) ? hash.name : "[UNKNOWN]");
                            }
                        }
                    }
                }
            }

            private static (Color? color, string text) TrackingTuple(VRC_AnimatorTrackingControl.TrackingType trackingType) => trackingType switch
            {
                VRC_AnimatorTrackingControl.TrackingType.NoChange => (Color.yellow, "No Change"),
                VRC_AnimatorTrackingControl.TrackingType.Tracking => (Color.green, "Tracking"),
                VRC_AnimatorTrackingControl.TrackingType.Animation => (Color.red, "Animation"),
                _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
            };

            private static (Color? color, string text) LabelTuple(Vrc3Param param) => param.Type switch
            {
                AnimatorControllerParameterType.Float => (null, param.FloatValue().ToString("0.00")),
                AnimatorControllerParameterType.Int => (null, param.IntValue().ToString()),
                AnimatorControllerParameterType.Bool => param.BoolValue() ? (Color.green, "True") : (Color.red, "False"),
                AnimatorControllerParameterType.Trigger => param.BoolValue() ? (Color.green, "True") : (Color.red, "False"),
                _ => throw new ArgumentOutOfRangeException()
            };

            private static void FieldTuple(Vrc3Param param, ModuleVrc3 module, GUILayoutOption innerOption)
            {
                var rect = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.label, innerOption);
                switch (param.Type)
                {
                    case AnimatorControllerParameterType.Float:
                        if (GmgLayoutHelper.UnityFieldEnterListener(param.FloatValue(), module, rect, EditorGUI.FloatField, param.Set, module.Edit)) module.Edit = null;
                        break;
                    case AnimatorControllerParameterType.Int:
                        if (GmgLayoutHelper.UnityFieldEnterListener(param.IntValue(), module, rect, EditorGUI.IntField, param.Set, module.Edit)) module.Edit = null;
                        break;
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                        param.Set(module, !param.BoolValue());
                        module.Edit = null;
                        break;
                    default: throw new ArgumentOutOfRangeException();
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
                public const string Hint = "or you can dock the window back in by using the button below!";
                public const string Button = "Dock Debug Window";
            }

            public static class D
            {
                private const string Select = "Select a ";
                public const string Subtitle = Select + "category from the toolbar below.";
                public const string Hint = "For an easier experience, you can undock the window with the button below!";
                public const string Button = "Undock Debug Window";
            }
        }
    }
}
#endif