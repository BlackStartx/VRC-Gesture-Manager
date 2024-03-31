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
        private static Texture2D _earmuffs;
        private static Texture2D _fallingSpeed;
        private static Texture2D _fullBody;
        private static Texture2D _generic;
        private static Texture2D _gestureLeftWeight;
        private static Texture2D _gestureRightWeight;
        private static Texture2D _grounded;
        private static Texture2D _handsOnly;
        private static Texture2D _headHands;
        private static Texture2D _poseIk;
        private static Texture2D _muteSelf;
        private static Texture2D _seated;
        private static Texture2D _poseT;
        private static Texture2D _upright;
        private static Texture2D _velocity;
        private static Texture2D _visemes;
        private static Texture2D _vRMode;
        private static Texture2D _afk;
        private static Texture2D _fourPoint;
        private static Texture2D _uninitialized;
        private static Texture2D _isLocal;
        private static Texture2D _extras;
        private static Texture2D _isOnFriendsList;

        internal static GUIStyle Url => _url ??= new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.blue } };
        internal static GUIStyle UrlPro => _urlPro ??= new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.cyan } };

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
        internal static Texture2D Earmuffs => _earmuffs ? _earmuffs : _earmuffs = Resources.Load<Texture2D>("Vrc3/BSX_GM_Earmuffs");
        internal static Texture2D FallingSpeed => _fallingSpeed ? _fallingSpeed : _fallingSpeed = Resources.Load<Texture2D>("Vrc3/BSX_GM_FallingSpeed");
        internal static Texture2D FullBody => _fullBody ? _fullBody : _fullBody = Resources.Load<Texture2D>("Vrc3/BSX_GM_FullBody");
        internal static Texture2D Generic => _generic ? _generic : _generic = Resources.Load<Texture2D>("Vrc3/BSX_GM_Generic");
        internal static Texture2D GestureLeftWeight => _gestureLeftWeight ? _gestureLeftWeight : _gestureLeftWeight = Resources.Load<Texture2D>("Vrc3/BSX_GM_GestureLeftWeight");
        internal static Texture2D GestureRightWeight => _gestureRightWeight ? _gestureRightWeight : _gestureRightWeight = Resources.Load<Texture2D>("Vrc3/BSX_GM_GestureRightWeight");
        internal static Texture2D Grounded => _grounded ? _grounded : _grounded = Resources.Load<Texture2D>("Vrc3/BSX_GM_Grounded");
        internal static Texture2D HandsOnly => _handsOnly ? _handsOnly : _handsOnly = Resources.Load<Texture2D>("Vrc3/BSX_GM_HandsOnly");
        internal static Texture2D HeadHands => _headHands ? _headHands : _headHands = Resources.Load<Texture2D>("Vrc3/BSX_GM_HeadHands");
        internal static Texture2D PoseIK => _poseIk ? _poseIk : _poseIk = Resources.Load<Texture2D>("Vrc3/BSX_GM_Pose_IK");
        internal static Texture2D MuteSelf => _muteSelf ? _muteSelf : _muteSelf = Resources.Load<Texture2D>("Vrc3/BSX_GM_MuteSelf");
        internal static Texture2D Seated => _seated ? _seated : _seated = Resources.Load<Texture2D>("Vrc3/BSX_GM_Seated");
        internal static Texture2D PoseT => _poseT ? _poseT : _poseT = Resources.Load<Texture2D>("Vrc3/BSX_GM_Pose_T");
        internal static Texture2D Upright => _upright ? _upright : _upright = Resources.Load<Texture2D>("Vrc3/BSX_GM_Upright");
        internal static Texture2D Velocity => _velocity ? _velocity : _velocity = Resources.Load<Texture2D>("Vrc3/BSX_GM_Velocity");
        internal static Texture2D Visemes => _visemes ? _visemes : _visemes = Resources.Load<Texture2D>("Vrc3/BSX_GM_Visemes");
        internal static Texture2D VRMode => _vRMode ? _vRMode : _vRMode = Resources.Load<Texture2D>("Vrc3/BSX_GM_VRMode");
        internal static Texture2D Afk => _afk ? _afk : _afk = Resources.Load<Texture2D>("Vrc3/BSX_GM_AFK");
        internal static Texture2D FourPoint => _fourPoint ? _fourPoint : _fourPoint = Resources.Load<Texture2D>("Vrc3/BSX_GM_FourPoint");
        internal static Texture2D Uninitialized => _uninitialized ? _uninitialized : _uninitialized = Resources.Load<Texture2D>("Vrc3/BSX_GM_Uninitialized");
        internal static Texture2D IsLocal => _isLocal ? _isLocal : _isLocal = Resources.Load<Texture2D>("Vrc3/BSX_GM_IsLocal");
        internal static Texture2D Extras => _extras ? _extras : _extras = Resources.Load<Texture2D>("Vrc3/BSX_GM_Extras");
        internal static Texture2D IsOnFriendsList => _isOnFriendsList ? _isOnFriendsList : _isOnFriendsList = Resources.Load<Texture2D>("Vrc3/BSX_GM_IsOnFriendsList");

        public static class Data
        {
            private const string VrcSdk3ControllerPath = "Vrc3/Controllers/";
            private const string VrcSdk3RestorePath = "Vrc3/Controllers/Restore/";

            private static AnimatorController ControllerOfPath(string path) => Resources.Load<AnimatorController>(VrcSdk3ControllerPath + path);

            private static TextAsset RestoreOfPath(string path) => Resources.Load<TextAsset>(VrcSdk3RestorePath + path);

            internal static TextAsset RestoreOf(AnimLayerType type) => RestoreOfPath(NameOf[type]);

            internal static AnimatorController ControllerOf(AnimLayerType type) => ControllerOfPath(NameOf[type]);

            internal static int LayerSort(VRCAvatarDescriptor.CustomAnimLayer x, VRCAvatarDescriptor.CustomAnimLayer y) => SortValue[x.type] - SortValue[y.type];

            internal static AvatarMask MaskOf(AnimLayerType type) => type switch
            {
                AnimLayerType.Gesture => Masks.Hands,
                AnimLayerType.IKPose => Masks.Armature,
                AnimLayerType.TPose => Masks.Armature,
                AnimLayerType.FX => Masks.Empty,
                AnimLayerType.Deprecated0 => null,
                AnimLayerType.Additive => null,
                AnimLayerType.Sitting => null,
                AnimLayerType.Action => null,
                AnimLayerType.Base => null,
                _ => null
            };

            private static readonly Dictionary<AnimLayerType, string> NameOf = new()
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

            internal static readonly Dictionary<BlendableAnimatorLayer, AnimLayerType> AnimatorToLayer = new()
            {
                { BlendableAnimatorLayer.FX, AnimLayerType.FX },
                { BlendableAnimatorLayer.Action, AnimLayerType.Action },
                { BlendableAnimatorLayer.Gesture, AnimLayerType.Gesture },
                { BlendableAnimatorLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<BlendablePlayableLayer, AnimLayerType> PlayableToLayer = new()
            {
                { BlendablePlayableLayer.FX, AnimLayerType.FX },
                { BlendablePlayableLayer.Action, AnimLayerType.Action },
                { BlendablePlayableLayer.Gesture, AnimLayerType.Gesture },
                { BlendablePlayableLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<AnimLayerType, int> SortValue = new()
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

            internal static readonly Dictionary<ValueType, AnimatorControllerParameterType> TypeOf = new()
            {
                { ValueType.Int, AnimatorControllerParameterType.Int },
                { ValueType.Bool, AnimatorControllerParameterType.Bool },
                { ValueType.Float, AnimatorControllerParameterType.Float }
            };

            public static Dictionary<string, TrackingType> DefaultTrackingState => new()
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