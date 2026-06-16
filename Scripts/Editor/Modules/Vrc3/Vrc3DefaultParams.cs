#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class Vrc3DefaultParams
    {
        internal const string GestureRightWeight = "GestureRightWeight";
        internal const string ScaleFactorInverse = "ScaleFactorInverse";
        internal const string EyeHeightAsPercent = "EyeHeightAsPercent";
        internal const string EyeHeightAsMeters = "EyeHeightAsMeters";
        internal const string GestureLeftWeight = "GestureLeftWeight";
        internal const string VelocityMagnitude = "VelocityMagnitude";
        internal const string IsAnimatorEnabled = "IsAnimatorEnabled";
        internal const string IsOnFriendsList = "IsOnFriendsList";
        internal const string ScaleModified = "ScaleModified";
        internal const string AvatarVersion = "AvatarVersion";
        internal const string TrackingType = "TrackingType";
        internal const string GestureRight = "GestureRight";
        internal const string ScaleFactor = "ScaleFactor";
        internal const string GestureLeft = "GestureLeft";
        internal const string PreviewMode = "PreviewMode";
        internal const string VelocityX = "VelocityX";
        internal const string VelocityY = "VelocityY";
        internal const string VelocityZ = "VelocityZ";
        internal const string InStation = "InStation";
        internal const string Grounded = "Grounded";
        internal const string MuteSelf = "MuteSelf";
        internal const string Earmuffs = "Earmuffs";
        internal const string Upright = "Upright";
        internal const string IsLocal = "IsLocal";
        internal const string Seated = "Seated";
        internal const string VRMode = "VRMode";
        internal const string Voice = "Voice";
        internal const string Vise = "Viseme";
        internal const string Afk = "AFK";

        private const string VrcFaceBlendH = "VRCFaceBlendH";
        private const string VrcFaceBlendV = "VRCFaceBlendV";
        private const string VrcEmote = "VRCEmote";
        private const string AngularY = "AngularY";

        public static Dictionary<string, DefaultParameter> Parameters => new()
        {
            { GestureRightWeight, new DefaultParameter { name = GestureRightWeight, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { ScaleFactorInverse, new DefaultParameter { name = ScaleFactorInverse, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { EyeHeightAsPercent, new DefaultParameter { name = EyeHeightAsPercent, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { EyeHeightAsMeters, new DefaultParameter { name = EyeHeightAsMeters, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { GestureLeftWeight, new DefaultParameter { name = GestureLeftWeight, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { VelocityMagnitude, new DefaultParameter { name = VelocityMagnitude, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { IsAnimatorEnabled, new DefaultParameter { name = IsAnimatorEnabled, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { IsOnFriendsList, new DefaultParameter { name = IsOnFriendsList, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.No } },
            { VrcFaceBlendH, new DefaultParameter { name = VrcFaceBlendH, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { VrcFaceBlendV, new DefaultParameter { name = VrcFaceBlendV, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { ScaleModified, new DefaultParameter { name = ScaleModified, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { AvatarVersion, new DefaultParameter { name = AvatarVersion, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { TrackingType, new DefaultParameter { name = TrackingType, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { GestureRight, new DefaultParameter { name = GestureRight, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { ScaleFactor, new DefaultParameter { name = ScaleFactor, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { GestureLeft, new DefaultParameter { name = GestureLeft, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { PreviewMode, new DefaultParameter { name = PreviewMode, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { VelocityX, new DefaultParameter { name = VelocityX, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { VelocityY, new DefaultParameter { name = VelocityY, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { VelocityZ, new DefaultParameter { name = VelocityZ, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { InStation, new DefaultParameter { name = InStation, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { AngularY, new DefaultParameter { name = AngularY, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { Earmuffs, new DefaultParameter { name = Earmuffs, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { Grounded, new DefaultParameter { name = Grounded, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { MuteSelf, new DefaultParameter { name = MuteSelf, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { VrcEmote, new DefaultParameter { name = VrcEmote, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { Upright, new DefaultParameter { name = Upright, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { IsLocal, new DefaultParameter { name = IsLocal, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.No } },
            { Seated, new DefaultParameter { name = Seated, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } },
            { VRMode, new DefaultParameter { name = VRMode, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { Voice, new DefaultParameter { name = Voice, valueType = VRCExpressionParameters.ValueType.Float, SyncType = SyncType.Ik } },
            { Vise, new DefaultParameter { name = Vise, valueType = VRCExpressionParameters.ValueType.Int, SyncType = SyncType.Ik } },
            { Afk, new DefaultParameter { name = Afk, valueType = VRCExpressionParameters.ValueType.Bool, SyncType = SyncType.Ik } }
        };

        public class DefaultParameter : VRCExpressionParameters.Parameter
        {
            public SyncType SyncType;
        }

        public enum SyncType
        {
            No,
            Ik,
            Playable
        }
    }
}
#endif