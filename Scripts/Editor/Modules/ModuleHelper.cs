using GestureManager.Scripts.Extra;
using UnityEngine;
using VRC.SDKBase;

namespace GestureManager.Scripts.Editor.Modules
{
    public static class ModuleHelper
    {
        public static ModuleBase GetModuleFor(GestureManager manager, GameObject gameObject) => GetModuleFor(manager, gameObject.GetComponent<VRC_AvatarDescriptor>());

        public static ModuleBase GetModuleFor(GestureManager manager, VRC_AvatarDescriptor descriptorComponent)
        {
            switch (descriptorComponent)
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