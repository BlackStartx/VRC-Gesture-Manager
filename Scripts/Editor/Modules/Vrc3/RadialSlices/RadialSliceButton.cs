#if VRC_SDK_VRCSDK3
using System;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceButton : RadialSliceBase
    {
        private readonly bool _active;
        private readonly Action _onClick;

        private VisualElement Extra => new VisualRunningElement(_active);

        public RadialSliceButton(Action onClick, string text, Texture2D icon, Color? color = null, bool active = false) : base(text, icon, null)
        {
            _active = active;
            _onClick = onClick ?? (() => { });
            TextColor = color ?? Color.white;
        }

        protected override void CreateExtra()
        {
            if (_active) Texture.Add(Extra);
        }

        public override void OnClickStart()
        {
        }

        public override void OnClickEnd() => _onClick();
    }
}
#endif