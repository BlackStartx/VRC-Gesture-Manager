#if VRC_SDK_VRCSDK3
using System;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.Dynamics;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceButton : RadialSliceBase
    {
        private readonly bool _running;
        private readonly Action _onClick;
        private readonly Vrc3Param _enParam;

        private VisualElement Extra => new VisualRunningElement(_running);

        public RadialSliceButton(Action onClick, string text, Texture2D icon = null, bool enabled = true, bool running = false) : base(text, icon, enabled: enabled)
        {
            _running = running;
            _onClick = onClick ?? (() => { });
        }

        public RadialSliceButton(Action onClick, string text, Vrc3Param enParam, Texture2D icon = null, bool running = false) : this(onClick, text, icon, enParam?.BoolValue() ?? true, running) => _enParam = enParam;

        protected internal override void CheckRunningUpdate() => Enabled = _enParam?.BoolValue() ?? Enabled;

        protected override void CreateExtra()
        {
            if (_running) Texture.Add(Extra);
        }

        public override void OnClickStart()
        {
        }

        public override void OnClickEnd() => _onClick();
    }
}
#endif