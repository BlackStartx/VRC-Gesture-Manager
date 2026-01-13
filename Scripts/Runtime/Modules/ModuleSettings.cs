using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.UI.GraphicRaycaster;
#endif
using System;

namespace BlackStartX.GestureManager.Modules
{
    [Serializable]
    public class ModuleSettings
    {
        public GmgAvatarDescriptor favourite;
        public Pose initialPose;
        public int userIndex;

        public bool isOnFriendsList;
        public bool loadStored;
        public bool isRemote;
        public bool vrMode;
    }

    public enum Pose
    {
        None,
        PoseT,
        PoseIK
    }
}