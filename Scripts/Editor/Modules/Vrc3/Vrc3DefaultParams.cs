#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using UnityEngine;

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
        internal const string IsOnFriendsList = "IsOnFriendsList";
        internal const string ScaleModified = "ScaleModified";
        internal const string AvatarVersion = "AvatarVersion";
        internal const string TrackingType = "TrackingType";
        internal const string GestureRight = "GestureRight";
        internal const string ScaleFactor = "ScaleFactor";
        internal const string GestureLeft = "GestureLeft";
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
        internal const string Vise = "Viseme";
        internal const string Afk = "AFK";

        private const string VrcFaceBlendH = "VRCFaceBlendH";
        private const string VrcFaceBlendV = "VRCFaceBlendV";
        private const string VrcEmote = "VRCEmote";
        private const string AngularY = "AngularY";
        private const string Voice = "Voice";

        public static Dictionary<string, AnimatorControllerParameterType> Parameters => new()
        {
            { GestureRightWeight, AnimatorControllerParameterType.Float },
            { ScaleFactorInverse, AnimatorControllerParameterType.Float },
            { EyeHeightAsPercent, AnimatorControllerParameterType.Float },
            { EyeHeightAsMeters, AnimatorControllerParameterType.Float },
            { GestureLeftWeight, AnimatorControllerParameterType.Float },
            { VelocityMagnitude, AnimatorControllerParameterType.Float },
            { IsOnFriendsList, AnimatorControllerParameterType.Bool },
            { VrcFaceBlendH, AnimatorControllerParameterType.Float },
            { VrcFaceBlendV, AnimatorControllerParameterType.Float },
            { ScaleModified, AnimatorControllerParameterType.Bool },
            { AvatarVersion, AnimatorControllerParameterType.Int },
            { TrackingType, AnimatorControllerParameterType.Int },
            { GestureRight, AnimatorControllerParameterType.Int },
            { ScaleFactor, AnimatorControllerParameterType.Float },
            { GestureLeft, AnimatorControllerParameterType.Int },
            { VelocityX, AnimatorControllerParameterType.Float },
            { VelocityY, AnimatorControllerParameterType.Float },
            { VelocityZ, AnimatorControllerParameterType.Float },
            { InStation, AnimatorControllerParameterType.Bool },
            { AngularY, AnimatorControllerParameterType.Float },
            { Earmuffs, AnimatorControllerParameterType.Bool },
            { Grounded, AnimatorControllerParameterType.Bool },
            { MuteSelf, AnimatorControllerParameterType.Bool },
            { VrcEmote, AnimatorControllerParameterType.Int },
            { Upright, AnimatorControllerParameterType.Float },
            { IsLocal, AnimatorControllerParameterType.Bool },
            { Seated, AnimatorControllerParameterType.Bool },
            { VRMode, AnimatorControllerParameterType.Int },
            { Voice, AnimatorControllerParameterType.Float },
            { Vise, AnimatorControllerParameterType.Int },
            { Afk, AnimatorControllerParameterType.Bool }
        };
    }
}
#endif