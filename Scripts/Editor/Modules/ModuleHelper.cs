using GestureManager.Scripts.Extra;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules
{
    public static class ModuleHelper
    {
        public static ModuleBase GetModuleForDescriptor(GestureManager manager, VRC_AvatarDescriptor descriptor)
        {
            switch (descriptor)
            {
#if VRC_SDK_VRCSDK2
                case VRCSDK2.VRC_AvatarDescriptor descriptorV2:
                    return new Vrc2.ModuleVrc2(manager, descriptorV2);
#endif
#if VRC_SDK_VRCSDK3
                case VRC.SDK3.Avatars.Components.VRCAvatarDescriptor descriptorV3:
                    return new Vrc3.ModuleVrc3(manager, descriptorV3);
#endif
                default: return null;
            }
        }
    }
}