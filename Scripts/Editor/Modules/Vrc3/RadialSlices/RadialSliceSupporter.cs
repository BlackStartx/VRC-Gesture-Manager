#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public class RadialSliceSupporter : RadialSliceBase
    {
        private readonly Func<float, float> _ease = Easing.Linear;

        private const float To = 6f;
        private const float From = -To;
        private const int Duration = 2000;

        private readonly int _offset;

        private readonly bool _early;
        private readonly int _pool;

        private int _index;
        private bool _shouldFade;
        private Vrc3Supporter.Supporter _supporter;

        private RadialSliceSupporter(Vrc3Supporter.Supporter supporter, bool early, int index, int pool, int offset, Texture2D icon) : base(supporter.name, icon)
        {
            _pool = pool;
            _index = index;
            _early = early;
            _offset = offset;
            _supporter = supporter;
            if (early) return;
            Text.Color = supporter.text;
            SelectedCenterColor = supporter.background;
        }

        public RadialSliceSupporter(bool early, int index, int pool, int offset, Texture2D icon) : this(Vrc3Supporter.SupporterData(early, index), early, index, pool, offset, icon)
        {
        }

        protected internal override void CheckRunningUpdate()
        {
        }

        protected override void CreateExtra() => StartOffset();

        public override void OnClickStart()
        {
        }

        public override void OnClickEnd()
        {
        }

        private void FadeOut(VisualElement element, float value)
        {
            if (!_shouldFade) return;
            Fade(element, value);
        }

        private void FadeIn(VisualElement element, float value)
        {
            if (!_shouldFade) return;
            Fade(element, value);
        }

        private void Fade(VisualElement element, float value)
        {
            value = Mathf.Clamp01(Math.Abs(value));
            element.style.opacity = new StyleFloat(value);
            if (!_early && Selected) CenterColor = Color.Lerp(RadialMenuUtility.Colors.CenterSelected, _supporter.background, value);
        }

        private void StartOffset(Action<VisualElement, float> change = null) => DataHolder.experimental.animation.Start(From, To, _offset, change).Ease(_ease).OnCompleted(StartFadeOut);

        private void StartFadeOut()
        {
            _shouldFade = Vrc3Supporter.ShouldFade(_early, _pool);
            DataHolder.experimental.animation.Start(From, 0f, Duration, FadeOut).Ease(_ease).OnCompleted(StartFadeIn);
        }

        private void StartFadeIn()
        {
            SetSupporter(Vrc3Supporter.SupporterData(_early, _index += _pool));
            DataHolder.experimental.animation.Start(0f, To, Duration, FadeIn).Ease(_ease).OnCompleted(StartFadeOut);
        }

        private void SetSupporter(Vrc3Supporter.Supporter supporter)
        {
            _supporter = supporter;
            Text.Text = supporter.name;
            if (_early) return;
            Text.Color = supporter.text;
            SelectedCenterColor = supporter.background;
        }
    }
}
#endif