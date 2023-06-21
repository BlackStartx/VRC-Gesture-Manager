﻿using BlackStartX.GestureManager.Data;
using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.Component;
#endif
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules
{
    public static class ModuleHelper
    {
        public static ModuleBase GetModuleFor(GameObject gameObject) => GetModuleFor(gameObject.GetComponent<GmgAvatarDescriptor>());

        public static ModuleBase GetModuleFor(GmgAvatarDescriptor descriptorComponent)
        {
            switch (descriptorComponent)
            {
#if VRC_SDK_VRCSDK2
                case VRCSDK2.VRC_AvatarDescriptor descriptorV2:
                    return new Vrc2.ModuleVrc2(descriptorV2);
#endif
#if VRC_SDK_VRCSDK3
                case VRC.SDK3.Avatars.Components.VRCAvatarDescriptor descriptorV3:
                    return new Vrc3.ModuleVrc3(descriptorV3);
#endif
                default: return null;
            }
        }
    }
}