#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using BlackStartX.GestureManager.Library;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using ValueType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType;
using AnimLayerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType;
using BlendablePlayableLayer = VRC.SDKBase.VRC_PlayableLayerControl.BlendableLayer;
using BlendableAnimatorLayer = VRC.SDKBase.VRC_AnimatorLayerControl.BlendableLayer;
using TrackingType = VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class ModuleVrc3Styles
    {
        private static GUIStyle _url;
        private static GUIStyle _urlPro;

        private static Texture2D _emojis;
        private static Texture2D _option;
        private static Texture2D _expressions;
        private static Texture2D _tools;
        private static Texture2D _back;
        private static Texture2D _backHome;
        private static Texture2D _default;
        private static Texture2D _gear;
        private static Texture2D _reset;
        private static Texture2D _twoAxis;
        private static Texture2D _fourAxis;
        private static Texture2D _radial;
        private static Texture2D _toggle;
        private static Texture2D _resetHeight;
        private static Texture2D _avatarHeight;
        private static Texture2D _releasePoses;
        private static Texture2D _runningParam;
        private static Texture2D _axisUp;
        private static Texture2D _axisRight;
        private static Texture2D _axisDown;
        private static Texture2D _axisLeft;
        private static Texture2D _supportLike;
        private static Texture2D _supportGold;
        private static Texture2D _supportHeart;
        private static Texture2D _toolCamera;
        private static Texture2D _toolClick;
        private static Texture2D _toolPose;

        internal static GUIStyle Url => _url ?? (_url = new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.blue } });
        internal static GUIStyle UrlPro => _urlPro ?? (_urlPro = new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.cyan } });

        internal static Texture2D Emojis => _emojis ? _emojis : _emojis = Resources.Load<Texture2D>("Vrc3/BSX_GM_Emojis");
        internal static Texture2D Option => _option ? _option : _option = Resources.Load<Texture2D>("Vrc3/BSX_GM_Option");
        internal static Texture2D Expressions => _expressions ? _expressions : _expressions = Resources.Load<Texture2D>("Vrc3/BSX_GM_Expressions");
        internal static Texture2D Tools => _tools ? _tools : _tools = Resources.Load<Texture2D>("Vrc3/BSX_GM_Tools");
        internal static Texture2D Back => _back ? _back : _back = Resources.Load<Texture2D>("Vrc3/BSX_GM_Back");
        internal static Texture2D BackHome => _backHome ? _backHome : _backHome = Resources.Load<Texture2D>("Vrc3/BSX_GM_BackHome");
        internal static Texture2D Default => _default ? _default : _default = Resources.Load<Texture2D>("Vrc3/BSX_GM_Default");
        internal static Texture2D Gear => _gear ? _gear : _gear = Resources.Load<Texture2D>("Vrc3/BSX_GM_Gear");
        internal static Texture2D Reset => _reset ? _reset : _reset = Resources.Load<Texture2D>("Vrc3/BSX_GM_Reset");
        internal static Texture2D TwoAxis => _twoAxis ? _twoAxis : _twoAxis = Resources.Load<Texture2D>("Vrc3/BSX_GM_2_Axis");
        internal static Texture2D FourAxis => _fourAxis ? _fourAxis : _fourAxis = Resources.Load<Texture2D>("Vrc3/BSX_GM_4_Axis");
        internal static Texture2D Radial => _radial ? _radial : _radial = Resources.Load<Texture2D>("Vrc3/BSX_GM_Radial");
        internal static Texture2D Toggle => _toggle ? _toggle : _toggle = Resources.Load<Texture2D>("Vrc3/BSX_GM_Toggle");
        internal static Texture2D ResetHeight => _resetHeight ? _resetHeight : _resetHeight = Resources.Load<Texture2D>("Vrc3/BSX_GM_Reset_Height");
        internal static Texture2D AvatarHeight => _avatarHeight ? _avatarHeight : _avatarHeight = Resources.Load<Texture2D>("Vrc3/BSX_GM_Avatar_Height");
        internal static Texture2D ReleasePoses => _releasePoses ? _releasePoses : _releasePoses = Resources.Load<Texture2D>("Vrc3/BSX_GM_Release_Poses");
        internal static Texture2D RunningParam => _runningParam ? _runningParam : _runningParam = Resources.Load<Texture2D>("Vrc3/BSX_GM_Running_Param");
        internal static Texture2D AxisUp => _axisUp ? _axisUp : _axisUp = Resources.Load<Texture2D>("Vrc3/BSX_GM_Axis_Up");
        internal static Texture2D AxisRight => _axisRight ? _axisRight : _axisRight = Resources.Load<Texture2D>("Vrc3/BSX_GM_Axis_Right");
        internal static Texture2D AxisDown => _axisDown ? _axisDown : _axisDown = Resources.Load<Texture2D>("Vrc3/BSX_GM_Axis_Down");
        internal static Texture2D AxisLeft => _axisLeft ? _axisLeft : _axisLeft = Resources.Load<Texture2D>("Vrc3/BSX_GM_Axis_Left");
        internal static Texture2D SupportLike => _supportLike ? _supportLike : _supportLike = Resources.Load<Texture2D>("Vrc3/BSX_GM_Support_Like");
        internal static Texture2D SupportGold => _supportGold ? _supportGold : _supportGold = Resources.Load<Texture2D>("Vrc3/BSX_GM_Support_Gold");
        internal static Texture2D SupportHeart => _supportHeart ? _supportHeart : _supportHeart = Resources.Load<Texture2D>("Vrc3/BSX_GM_Support_Heart");
        internal static Texture2D ToolCamera => _toolCamera ? _toolCamera : _toolCamera = Resources.Load<Texture2D>("Vrc3/BSX_GM_Tool_Camera");
        internal static Texture2D ToolClick => _toolClick ? _toolClick : _toolClick = Resources.Load<Texture2D>("Vrc3/BSX_GM_Tool_Click");
        internal static Texture2D ToolPose => _toolPose ? _toolPose : _toolPose = Resources.Load<Texture2D>("Vrc3/BSX_GM_Tool_Pose");

        public static class Data
        {
            private const string VrcSdk3ControllerPath = "Vrc3/Controllers/";
            private const string VrcSdk3RestorePath = "Vrc3/Controllers/Restore/";

            private static AnimatorController ControllerOfPath(string path) => Resources.Load<AnimatorController>(VrcSdk3ControllerPath + path);

            private static TextAsset RestoreOfPath(string path) => Resources.Load<TextAsset>(VrcSdk3RestorePath + path);

            internal static TextAsset RestoreOf(AnimLayerType type) => RestoreOfPath(NameOf[type]);

            internal static AnimatorController ControllerOf(AnimLayerType type) => ControllerOfPath(NameOf[type]);

            internal static int LayerSort(VRCAvatarDescriptor.CustomAnimLayer x, VRCAvatarDescriptor.CustomAnimLayer y) => SortValue[x.type] - SortValue[y.type];

            internal static AvatarMask MaskOf(AnimLayerType type)
            {
                switch (type)
                {
                    case AnimLayerType.Gesture: return Masks.Hands;
                    case AnimLayerType.IKPose: return Masks.Armature;
                    case AnimLayerType.TPose: return Masks.Armature;
                    case AnimLayerType.FX: return Masks.Empty;
                    case AnimLayerType.Deprecated0:
                    case AnimLayerType.Additive:
                    case AnimLayerType.Sitting:
                    case AnimLayerType.Action:
                    case AnimLayerType.Base:
                    default: return null;
                }
            }

            private static readonly Dictionary<AnimLayerType, string> NameOf = new Dictionary<AnimLayerType, string>
            {
                { AnimLayerType.FX, "GmgFxLayer" },
                { AnimLayerType.Base, "GmgBaseLayer" },
                { AnimLayerType.TPose, "GmgUtilityTPose" },
                { AnimLayerType.Action, "GmgActionLayer" },
                { AnimLayerType.IKPose, "GmgUtilityIKPose" },
                { AnimLayerType.Gesture, "GmgGestureLayer" },
                { AnimLayerType.Sitting, "GmgSittingLayer" },
                { AnimLayerType.Additive, "GmgAdditiveLayer" }
            };

            internal static readonly Dictionary<BlendableAnimatorLayer, AnimLayerType> AnimatorToLayer = new Dictionary<BlendableAnimatorLayer, AnimLayerType>
            {
                { BlendableAnimatorLayer.FX, AnimLayerType.FX },
                { BlendableAnimatorLayer.Action, AnimLayerType.Action },
                { BlendableAnimatorLayer.Gesture, AnimLayerType.Gesture },
                { BlendableAnimatorLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<BlendablePlayableLayer, AnimLayerType> PlayableToLayer = new Dictionary<BlendablePlayableLayer, AnimLayerType>
            {
                { BlendablePlayableLayer.FX, AnimLayerType.FX },
                { BlendablePlayableLayer.Action, AnimLayerType.Action },
                { BlendablePlayableLayer.Gesture, AnimLayerType.Gesture },
                { BlendablePlayableLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<AnimLayerType, int> SortValue = new Dictionary<AnimLayerType, int>
            {
                { AnimLayerType.Base, 0 },
                { AnimLayerType.Additive, 1 },
                { AnimLayerType.Sitting, 2 },
                { AnimLayerType.TPose, 3 },
                { AnimLayerType.IKPose, 4 },
                { AnimLayerType.Gesture, 5 },
                { AnimLayerType.Action, 6 },
                { AnimLayerType.FX, 7 }
            };

            internal static readonly Dictionary<ValueType, AnimatorControllerParameterType> TypeOf = new Dictionary<ValueType, AnimatorControllerParameterType>
            {
                { ValueType.Int, AnimatorControllerParameterType.Int },
                { ValueType.Bool, AnimatorControllerParameterType.Bool },
                { ValueType.Float, AnimatorControllerParameterType.Float }
            };

            public static Dictionary<string, TrackingType> DefaultTrackingState => new Dictionary<string, TrackingType>
            {
                { "Head", TrackingType.Tracking },
                { "Left Hand", TrackingType.Tracking },
                { "Right Hand", TrackingType.Tracking },
                { "Hip", TrackingType.Tracking },
                { "Left Foot", TrackingType.Tracking },
                { "Right Foot", TrackingType.Tracking },
                { "Left Fingers", TrackingType.Tracking },
                { "Right Fingers", TrackingType.Tracking },
                { "Eye & Eyelid", TrackingType.Tracking },
                { "Mouth & Jaw", TrackingType.Tracking }
            };
        }

        private static class Masks
        {
            private static AvatarMask _empty;
            private static AvatarMask _hands;
            private static AvatarMask _armature;

            internal static AvatarMask Empty => _empty ? _empty : _empty = CreateEmptyMask();

            internal static AvatarMask Hands => _hands ? _hands : _hands = CreateHandsMask();

            internal static AvatarMask Armature => _armature ? _armature : _armature = CreateArmatureMask();

            private static AvatarMask CreateEmptyMask() => GmgAvatarMaskHelper.CreateEmptyMask("Empty");

            private static AvatarMask CreateHandsMask() => GmgAvatarMaskHelper.CreateMaskWith("Hands", new[]
            {
                AvatarMaskBodyPart.LeftFingers,
                AvatarMaskBodyPart.RightFingers
            });

            private static AvatarMask CreateArmatureMask() => GmgAvatarMaskHelper.CreateMaskWithout("Armature", new[]
            {
                AvatarMaskBodyPart.LeftFootIK,
                AvatarMaskBodyPart.LeftHandIK,
                AvatarMaskBodyPart.RightFootIK,
                AvatarMaskBodyPart.RightHandIK
            });
        }
    }
}
#endif