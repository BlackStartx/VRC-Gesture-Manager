#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using VRC.SDKBase;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.AvatarDynamics
{
    internal class AnimParameterAccessAvatarGmg : IAnimParameterAccess
    {
        private readonly ModuleVrc3 _module;
        private readonly string _parameter;

        private Vrc3Param Param => _module.GetParam(_parameter);

        public AnimParameterAccessAvatarGmg(ModuleVrc3 module, string parameter)
        {
            _module = module;
            _parameter = parameter;
        }

        public bool boolVal
        {
            get => Param?.BoolValue() ?? false;
            set => Param?.Set(_module, value);
        }

        public int intVal
        {
            get => Param?.IntValue() ?? 0;
            set => Param?.Set(_module, value);
        }

        public float floatVal
        {
            get => Param?.FloatValue() ?? 0f;
            set => Param?.Set(_module, value);
        }
    }
}
#endif