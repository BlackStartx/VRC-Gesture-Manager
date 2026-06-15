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

        public static Dictionary<string, VRCExpressionParameters.Parameter> Parameters => new()
        {
            { GestureRightWeight, new VRCExpressionParameters.Parameter { name = GestureRightWeight, valueType = VRCExpressionParameters.ValueType.Float } },
            { ScaleFactorInverse, new VRCExpressionParameters.Parameter { name = ScaleFactorInverse, valueType = VRCExpressionParameters.ValueType.Float } },
            { EyeHeightAsPercent, new VRCExpressionParameters.Parameter { name = EyeHeightAsPercent, valueType = VRCExpressionParameters.ValueType.Float } },
            { EyeHeightAsMeters, new VRCExpressionParameters.Parameter { name = EyeHeightAsMeters, valueType = VRCExpressionParameters.ValueType.Float } },
            { GestureLeftWeight, new VRCExpressionParameters.Parameter { name = GestureLeftWeight, valueType = VRCExpressionParameters.ValueType.Float } },
            { VelocityMagnitude, new VRCExpressionParameters.Parameter { name = VelocityMagnitude, valueType = VRCExpressionParameters.ValueType.Float } },
            { IsAnimatorEnabled, new VRCExpressionParameters.Parameter { name = IsAnimatorEnabled, valueType = VRCExpressionParameters.ValueType.Bool } },
            { IsOnFriendsList, new VRCExpressionParameters.Parameter { name = IsOnFriendsList, valueType = VRCExpressionParameters.ValueType.Bool } },
            { VrcFaceBlendH, new VRCExpressionParameters.Parameter { name = VrcFaceBlendH, valueType = VRCExpressionParameters.ValueType.Float } },
            { VrcFaceBlendV, new VRCExpressionParameters.Parameter { name = VrcFaceBlendV, valueType = VRCExpressionParameters.ValueType.Float } },
            { ScaleModified, new VRCExpressionParameters.Parameter { name = ScaleModified, valueType = VRCExpressionParameters.ValueType.Bool } },
            { AvatarVersion, new VRCExpressionParameters.Parameter { name = AvatarVersion, valueType = VRCExpressionParameters.ValueType.Int } },
            { TrackingType, new VRCExpressionParameters.Parameter { name = TrackingType, valueType = VRCExpressionParameters.ValueType.Int } },
            { GestureRight, new VRCExpressionParameters.Parameter { name = GestureRight, valueType = VRCExpressionParameters.ValueType.Int } },
            { ScaleFactor, new VRCExpressionParameters.Parameter { name = ScaleFactor, valueType = VRCExpressionParameters.ValueType.Float } },
            { GestureLeft, new VRCExpressionParameters.Parameter { name = GestureLeft, valueType = VRCExpressionParameters.ValueType.Int } },
            { PreviewMode, new VRCExpressionParameters.Parameter { name = PreviewMode, valueType = VRCExpressionParameters.ValueType.Int } },
            { VelocityX, new VRCExpressionParameters.Parameter { name = VelocityX, valueType = VRCExpressionParameters.ValueType.Float } },
            { VelocityY, new VRCExpressionParameters.Parameter { name = VelocityY, valueType = VRCExpressionParameters.ValueType.Float } },
            { VelocityZ, new VRCExpressionParameters.Parameter { name = VelocityZ, valueType = VRCExpressionParameters.ValueType.Float } },
            { InStation, new VRCExpressionParameters.Parameter { name = InStation, valueType = VRCExpressionParameters.ValueType.Bool } },
            { AngularY, new VRCExpressionParameters.Parameter { name = AngularY, valueType = VRCExpressionParameters.ValueType.Float } },
            { Earmuffs, new VRCExpressionParameters.Parameter { name = Earmuffs, valueType = VRCExpressionParameters.ValueType.Bool } },
            { Grounded, new VRCExpressionParameters.Parameter { name = Grounded, valueType = VRCExpressionParameters.ValueType.Bool } },
            { MuteSelf, new VRCExpressionParameters.Parameter { name = MuteSelf, valueType = VRCExpressionParameters.ValueType.Bool } },
            { VrcEmote, new VRCExpressionParameters.Parameter { name = VrcEmote, valueType = VRCExpressionParameters.ValueType.Int } },
            { Upright, new VRCExpressionParameters.Parameter { name = Upright, valueType = VRCExpressionParameters.ValueType.Float } },
            { IsLocal, new VRCExpressionParameters.Parameter { name = IsLocal, valueType = VRCExpressionParameters.ValueType.Bool } },
            { Seated, new VRCExpressionParameters.Parameter { name = Seated, valueType = VRCExpressionParameters.ValueType.Bool } },
            { VRMode, new VRCExpressionParameters.Parameter { name = VRMode, valueType = VRCExpressionParameters.ValueType.Int } },
            { Voice, new VRCExpressionParameters.Parameter { name = Voice, valueType = VRCExpressionParameters.ValueType.Float } },
            { Vise, new VRCExpressionParameters.Parameter { name = Vise, valueType = VRCExpressionParameters.ValueType.Int } },
            { Afk, new VRCExpressionParameters.Parameter { name = Afk, valueType = VRCExpressionParameters.ValueType.Bool } }
        };
    }
}
#endif