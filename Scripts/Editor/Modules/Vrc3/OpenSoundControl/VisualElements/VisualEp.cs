#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl.VisualElements
{
    public class VisualEp : VisualElement
    {
        private readonly VisualElement _dataHolder;
        private readonly VisualElement _dataBorder;

        internal VisualEp(EndpointControl endpoint)
        {
            VisualElement center;
            TextElement horizontal;

            style.positionType = PositionType.Absolute;
            pickingMode = PickingMode.Ignore;

            Add(center = VisualEpStyles.Center);
            center.Add(_dataHolder = VisualEpStyles.Holder);
            center.Add(_dataBorder = VisualEpStyles.Outline);

            Add(VisualEpStyles.VerticalText);
            Add(horizontal = VisualEpStyles.HorizontalText);
            horizontal.text = endpoint.HorizontalEndpoint;
            OnMessage(endpoint);
        }

        public void OnMessage(EndpointControl control)
        {
            // _dataBorder.experimental.animation.Start(1f, 0.5f, 500, Animation);
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
            e.style.borderColor = color;
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

            private static TextElement Create(TextAnchor anchor) => new TextElement
            {
                style = { color = Color.white, positionType = PositionType.Absolute, fontSize = 10, width = VisualEpStyles.InnerSize - 8, marginLeft = 3, unityTextAlign = anchor }
            };

            public void SetHeight(float childHeight)
            {
                style.height = childHeight;
                _min.style.height = childHeight;
                _max.style.height = childHeight;
                _bar.style.height = childHeight;
                _value.style.height = childHeight;
            }

            public void SetValue((float? min, float? value, float? max, object data) value) => SetValue(value.min, value.value, value.max, value.data);

            private void SetValue(float? min, float? value, float? max, object data)
            {
                _min.visible = value.HasValue;
                _max.visible = value.HasValue;
                _bar.visible = value.HasValue;
                if (min.HasValue) _min.text = min.Value.ToString("F");
                if (max.HasValue) _max.text = max.Value.ToString("F");
                _value.text = value.HasValue ? value.Value.ToString("F") : EndpointControl.StringValue(data);
                if (!value.HasValue) SetValue(false);
                else if (min.HasValue && max.HasValue && !RadialMenuUtility.Is(min.Value, max.Value)) SetValue(min.Value, value.Value, max.Value);
                else SetValue(value.Value > 0.5);
            }

            private void SetValue(float min, float value, float max)
            {
                var dis = max - min;
                var width = Mathf.Abs(value) / dis * VisualEpStyles.InnerSize;
                _bar.style.width = width;
                _bar.style.positionRight = min / dis * VisualEpStyles.InnerSize;
                if (value < 0f) _bar.style.positionRight = _bar.style.positionRight.value + width;
            }

            private void SetValue(bool value)
            {
                _bar.style.width = value ? VisualEpStyles.InnerSize : 0f;
            }
        }
    }
}
#endif