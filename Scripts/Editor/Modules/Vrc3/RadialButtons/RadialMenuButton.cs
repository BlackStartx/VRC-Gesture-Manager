#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons
{
    public class RadialMenuButton : RadialMenuItem
    {
        private readonly Action _onClick;

        public RadialMenuButton(Action onClick, string text, Texture2D icon, Color? color = null) : base(text, icon, null)
        {
            _onClick = onClick ?? (() => { });
            TextColor = color ?? Color.white;
        }

        protected override void CreateExtra()
        {
        }

        public override void OnClickStart()
        {
        }

        public override void OnClickEnd()
        {
            _onClick();
        }
    }
}
#endif