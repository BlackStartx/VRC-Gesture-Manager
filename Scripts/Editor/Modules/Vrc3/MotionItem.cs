#if VRC_SDK_VRCSDK3
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class MotionItem
    {
        public readonly Motion Motion;
        private readonly VRCAvatarDescriptor.AnimLayerType _layer;

        public readonly bool Default;

        public readonly string FullName;
        private string LayerName => $"{_layer.ToString()}/";

        public MotionItem(Motion motion, VRCAvatarDescriptor.AnimLayerType layer)
        {
            _layer = layer;
            Motion = motion;
            Default = Motion.name.StartsWith("proxy_");
            FullName = Motion.name.StartsWith(LayerName) ? Motion.name : $"{LayerName}{Motion.name}";
        }
    }
}
#endif