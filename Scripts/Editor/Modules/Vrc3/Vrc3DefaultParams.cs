#if VRC_SDK_VRCSDK3
using System.Collections.Generic;

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
        internal const string Afk = "AFK";

        private const string Vise = "Viseme";

        public static IEnumerable<string> Parameters => new[]
        {
            GestureRightWeight,
            GestureLeftWeight,
            AvatarVersion,
            TrackingType,
            GestureRight,
            GestureLeft,
            VelocityX,
            VelocityY,
            VelocityZ,
            InStation,
            Grounded,
            MuteSelf,
            Upright,
            IsLocal,
            Seated,
            VRMode,
            Vise,
            Afk
        };
    }
}
#endif