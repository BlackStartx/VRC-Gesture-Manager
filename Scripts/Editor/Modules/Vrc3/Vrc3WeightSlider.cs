#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class Vrc3WeightSlider : Vrc3VisualRender
    {
        private const float FadeSpeed = 0.02f;

        private readonly Vrc3WeightController _controller;
        private readonly string _target;

        private VisualElement _slider;
        private VisualElement _right;
        private VisualElement _left;
        private VisualElement _dot;

        private VisualElement _textHolder;
        private TextElement _textWeight;

        internal bool Drag;
        internal bool Active;

        private int _afk;
        private bool _out;
        private float _weight = 1f;

        public Vrc3WeightSlider(Vrc3WeightController controller, string target)
        {
            _controller = controller;
            _target = target;

            style.justifyContent = Justify.Center;
            style.position = Position.Absolute;
            pickingMode = pickingMode = PickingMode.Ignore;

            CreateWeightController();
        }

        private void CreateWeightController()
        {
            _slider = this.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { opacity = 0f, position = Position.Absolute, justifyContent = Justify.Center } });
            _right = _slider.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { backgroundColor = Color.gray, height = 10, position = Position.Absolute, right = 0 } }).MyBorder(1, 5, Color.black);
            _left = _slider.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { backgroundColor = RadialMenuUtility.Colors.ProgressRadial, height = 10, width = 10, position = Position.Absolute, left = 0 } }).MyBorder(1, 5, Color.black);
            _dot = _slider.MyAdd(RadialMenuUtility.Prefabs.NewCircle(16, RadialMenuUtility.Colors.RadialCenter, RadialMenuUtility.Colors.RadialMiddle, RadialMenuUtility.Colors.RadialBorder, Position.Absolute));

            _textHolder = this.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { opacity = 1f, position = Position.Absolute, justifyContent = Justify.Center, unityTextAlign = TextAnchor.MiddleCenter, alignItems = Align.Center, flexDirection = FlexDirection.Row } });
            _textHolder.MyAdd(new TextElement { pickingMode = PickingMode.Ignore, text = "Weight: ", style = { fontSize = 12, height = 15 } });
            _textWeight = _textHolder.MyAdd(new TextElement { pickingMode = PickingMode.Ignore, text = "100%", style = { fontSize = 15, color = RadialMenuUtility.Colors.ProgressRadial, height = 15 } });
        }

        public override void Render(VisualElement root, Rect rect)
        {
            base.Render(root, rect);
            if (Event.current.type == EventType.MouseUp) Drag = false;
            style.width = style.width.value.value - 80;
            style.left = rect.xMin + 40;
            style.top = rect.y - 5;

            _slider.style.width = style.width;
            _textHolder.style.width = style.width;

            HandleFade(Event.current.mousePosition);
            if (Drag) Update(Event.current.mousePosition);
            SetWeight(_controller.Module.GetParam(_target)?.Get() ?? 0f);

            rect.center += new Vector2((rect.width - _slider.style.width.value.value) / 2, -5);
            rect.width = _slider.style.width.value.value;
            if (Event.current.type == EventType.MouseDown) CheckDragStart(rect);
        }

        private void CheckDragStart(Rect rect)
        {
            if (!rect.Contains(Event.current.mousePosition)) return;
            _controller.DisableDrag();
            Drag = true;
            UpdatePosition(Event.current.mousePosition, false);
        }

        protected override bool RenderCondition(int selectedIndex) => selectedIndex == 0;

        private void Update(Vector2 mousePosition)
        {
            if (Event.current.type == EventType.MouseDrag) UpdatePosition(mousePosition, false);
            else if (!Xin(mousePosition)) UpdatePosition(mousePosition, true);
            else if (_out && --_afk == 0) Drag = false;
        }

        private void UpdatePosition(Vector2 mousePosition, bool outer)
        {
            _out = outer;
            var position = mousePosition.x - layout.x;
            var weight = Mathf.Clamp(position / layout.width, 0f, 1f);
            _controller.Module.GetParam(_target)?.Set(_controller.Module.RadialMenus.Values, weight);
            _afk = 5;
        }

        private void HandleFade(Vector2 mousePosition)
        {
            if (GUI.enabled)
            {
                Active = _controller.Dragging || Mathf.Abs(mousePosition.y - Rect.y) < 16 && Xin(mousePosition);
                if (_controller.Active) FadeOn();
                else FadeOff();
            }
            else FadeDisable();
        }

        private void FadeDisable()
        {
            _slider.style.opacity = 0f;
            _textHolder.style.opacity = 0.5f;
        }

        private void FadeOff()
        {
            _slider.style.opacity = Mathf.Clamp(_slider.style.opacity.value - FadeSpeed, 0f, 1f);
            _textHolder.style.opacity = Mathf.Clamp(_textHolder.style.opacity.value + FadeSpeed, 0f, 1f);
        }

        private void FadeOn()
        {
            _slider.style.opacity = Mathf.Clamp(_slider.style.opacity.value + FadeSpeed, 0f, 1f);
            _textHolder.style.opacity = Mathf.Clamp(_textHolder.style.opacity.value - FadeSpeed, 0f, 1f);
        }

        private void SetWeight(float weight)
        {
            _weight = weight;
            ShowWeight();
        }

        internal void ShowWeight()
        {
            _left.style.width = _weight * style.width.value.value;
            _right.style.width = style.width.value.value - _left.style.width.value.value;
            _dot.style.left = _weight * style.width.value.value - 8;
            _textWeight.text = (int)(_weight * 100f) + "%";
        }
    }
}
#endif