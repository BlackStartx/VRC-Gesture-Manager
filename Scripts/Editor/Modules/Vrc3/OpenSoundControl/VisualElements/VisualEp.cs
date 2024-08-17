#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using BlackStartX.GestureManager.Library;
using UnityEngine.UIElements.Experimental;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements
{
    public class VisualEp : VisualElement
    {
        private const int AnimDuration = 750;
        private readonly VisualElement _dataHolder;
        private readonly ValueAnimation<float> _animation;

        internal VisualEp(EndpointControl endpoint)
        {
            VisualElement dataBorder;
            VisualElement center;
            TextElement horizontal;

            style.position = UIEPosition.Absolute;
            pickingMode = PickingMode.Ignore;

            Add(center = VisualEpStyles.Center);
            center.Add(_dataHolder = VisualEpStyles.Holder);
            center.Add(dataBorder = VisualEpStyles.Outline);

            _animation = dataBorder.experimental.animation.Start(1f, 0.5f, AnimDuration, Animation).KeepAlive();

            Add(VisualEpStyles.VerticalText);
            Add(horizontal = VisualEpStyles.HorizontalText);
            horizontal.text = endpoint.HorizontalEndpoint;
            OnMessage(endpoint);
        }

        public void OnMessage(EndpointControl control)
        {
            _animation.Start();
            if (control.Values.Count > _dataHolder.childCount) SetValueCount(control.Values.Count);
            for (var i = 0; i < control.Values.Count; i++) (_dataHolder[i] as Data)?.SetValue(control.Values[i]);
        }

        private void SetValueCount(int count)
        {
            var childHeight = (float)VisualEpStyles.InnerSize / count;
            for (var intLeft = _dataHolder.childCount; intLeft < count; intLeft++) _dataHolder.Add(new Data());
            foreach (var element in _dataHolder.Children()) (element as Data)?.SetHeight(childHeight);
        }

        private static void Animation(VisualElement e, float val)
        {
            var color = new Color(val, val, val, 1f);
            var width = 2 * val;
            e.style.borderBottomColor = e.style.borderLeftColor = e.style.borderRightColor = e.style.borderTopColor = color;
            e.style.borderBottomWidth = e.style.borderLeftWidth = e.style.borderRightWidth = e.style.borderTopWidth = width;
        }

        private class Data : VisualElement
        {
            private readonly VisualElement _bar;

            private readonly TextElement _min;
            private readonly TextElement _value;
            private readonly TextElement _max;

            internal Data()
            {
                style.width = VisualEpStyles.InnerSize;

                Add(_bar = new VisualElement { style = { backgroundColor = VisualEpStyles.BgColor } }.MyBorder(1, VisualEpStyles.Radius, Color.clear));
                Add(_min = Create(TextAnchor.MiddleLeft));
                Add(_value = Create(TextAnchor.MiddleCenter));
                Add(_max = Create(TextAnchor.MiddleRight));
            }

            private static TextElement Create(TextAnchor anchor) => new()
            {
                style = { color = Color.white, position = UIEPosition.Absolute, fontSize = 10, width = VisualEpStyles.InnerSize - 8, marginLeft = 3, unityTextAlign = anchor }
            };

            public void SetHeight(float childHeight)
            {
                style.height = childHeight;
                _min.style.height = childHeight;
                _max.style.height = childHeight;
                _bar.style.height = childHeight;
                _value.style.height = childHeight;
            }

            public void SetValue((float? min, object value, float? max) value) => SetValue(value.min, value.value, value.max, EndpointControl.FloatValue(value.value));

            private void SetValue(float? min, object value, float? max, float? vFloat)
            {
                _min.visible = min.HasValue;
                _max.visible = max.HasValue;
                _bar.visible = vFloat.HasValue;
                if (min.HasValue) _min.text = min.Value.ToString("F");
                if (max.HasValue) _max.text = max.Value.ToString("F");
                _value.text = $"{value}";
                if (!vFloat.HasValue) return;
                if (min.HasValue && max.HasValue && !RadialMenuUtility.Is(min.Value, max.Value)) SetValue(min.Value, vFloat.Value, max.Value);
                else SetValue(vFloat.Value > 0.5);
            }

            private void SetValue(float min, float value, float max)
            {
                var dis = max - min;
                var width = Mathf.Abs(value) / dis * VisualEpStyles.InnerSize;
                _bar.style.width = width;
                _bar.style.right = min / dis * VisualEpStyles.InnerSize;
                if (value < 0f) _bar.style.right = _bar.style.right.value.value + width;
            }

            private void SetValue(bool value)
            {
                _bar.style.width = value ? VisualEpStyles.InnerSize : 0f;
            }
        }
    }
}
#endif