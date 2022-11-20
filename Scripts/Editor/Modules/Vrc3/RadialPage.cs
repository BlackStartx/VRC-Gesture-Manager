#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class RadialPage
    {
        private readonly IReadOnlyList<RadialSliceBase> _controls;
        private readonly VRCExpressionsMenu _menu;
        private readonly RadialMenu _radialMenu;
        public readonly Vrc3Param Param;
        public readonly float Value;

        public RadialPage(RadialMenu radialMenu, VRCExpressionsMenu menu, Vrc3Param param, float value)
        {
            _radialMenu = radialMenu;
            _menu = menu;
            Param = param;
            Value = value;
        }

        public RadialPage(RadialMenu radialMenu, IReadOnlyList<RadialSliceBase> controls)
        {
            _radialMenu = radialMenu;
            _controls = controls;
        }

        public void Open()
        {
            if (_menu) _radialMenu.SetMenu(_menu);
            else _radialMenu.SetCustom(_controls);
        }
    }
}
#endif