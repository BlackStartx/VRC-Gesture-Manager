#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public static class Vrc3DefaultParams
    {
        internal const string GestureRightWeight = "GestureRightWeight";
        internal const string GestureLeftWeight = "GestureLeftWeight";
        internal const string AvatarVersion = "AvatarVersion";
        internal const string TrackingType = "TrackingType";
        internal const string GestureRight = "GestureRight";
        internal const string GestureLeft = "GestureLeft";
        internal const string VelocityX = "VelocityX";
        internal const string VelocityY = "VelocityY";
        internal const string VelocityZ = "VelocityZ";
        internal const string InStation = "InStation";
        internal const string Grounded = "Grounded";
        internal const string MuteSelf = "MuteSelf";
        internal const string Upright = "Upright";
        internal const string IsLocal = "IsLocal";
        internal const string Seated = "Seated";
        internal const string VRMode = "VRMode";
        internal const string Vise = "Viseme";
        internal const string Afk = "AFK";
        
        private const string VrcFaceBlendH = "VrcFaceBlendH";
        private const string VrcFaceBlendV = "VrcFaceBlendV";
        private const string VrcEmote = "VRCEmote";
        private const string AngularY = "AngularY";
        private const string Voice = "Voice";

        public static IEnumerable<(string name, AnimatorControllerParameterType type)> Parameters => new[]
        {
            (GestureRightWeight, AnimatorControllerParameterType.Float),
            (GestureLeftWeight, AnimatorControllerParameterType.Float),
            (VrcFaceBlendH, AnimatorControllerParameterType.Float),
            (VrcFaceBlendV, AnimatorControllerParameterType.Float),
            (AvatarVersion, AnimatorControllerParameterType.Int),
            (TrackingType, AnimatorControllerParameterType.Int),
            (GestureRight, AnimatorControllerParameterType.Int),
            (GestureLeft, AnimatorControllerParameterType.Int),
            (VelocityX, AnimatorControllerParameterType.Float),
            (VelocityY, AnimatorControllerParameterType.Float),
            (VelocityZ, AnimatorControllerParameterType.Float),
            (InStation, AnimatorControllerParameterType.Bool),
            (AngularY, AnimatorControllerParameterType.Float),
            (Grounded, AnimatorControllerParameterType.Bool),
            (MuteSelf, AnimatorControllerParameterType.Bool),
            (VrcEmote, AnimatorControllerParameterType.Int),
            (Upright, AnimatorControllerParameterType.Float),
            (IsLocal, AnimatorControllerParameterType.Bool),
            (Seated, AnimatorControllerParameterType.Bool),
            (VRMode, AnimatorControllerParameterType.Int),
            (Voice, AnimatorControllerParameterType.Float),
            (Vise, AnimatorControllerParameterType.Int),
            (Afk, AnimatorControllerParameterType.Bool)
        };
    }
}
#endif