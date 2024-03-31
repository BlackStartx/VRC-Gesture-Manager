#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using BlackStartX.GestureManager.Library;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
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

        private bool Xin(Vector2 p) => Rect.xMin < p.x && Rect.xMax > p.x;

        public Vrc3WeightSlider(Vrc3WeightController controller, string target)
        {
            _controller = controller;
            _target = target;

            style.justifyContent = Justify.Center;
            style.position = UIEPosition.Absolute;
            pickingMode = PickingMode.Ignore;

            CreateWeightController();
        }

        private void CreateWeightController()
        {
            Add(_slider = new VisualElement { pickingMode = PickingMode.Ignore, style = { opacity = 0f, position = UIEPosition.Absolute, justifyContent = Justify.Center } });
            _slider.Add(_right = new VisualElement { pickingMode = PickingMode.Ignore, style = { backgroundColor = Color.gray, height = 10, position = UIEPosition.Absolute, right = 0 } }.MyBorder(1, 5, Color.black));
            _slider.Add(_left = new VisualElement { pickingMode = PickingMode.Ignore, style = { backgroundColor = RadialMenuUtility.Colors.CustomSelected, height = 10, width = 10, position = UIEPosition.Absolute, left = 0 } }.MyBorder(1, 5, Color.black));
            _slider.Add(_dot = RadialMenuUtility.Prefabs.NewCircle(16, RadialMenuUtility.Colors.CenterIdle, RadialMenuUtility.Colors.CustomMain, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));

            Add(_textHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { opacity = 1f, position = UIEPosition.Absolute, justifyContent = Justify.Center, unityTextAlign = TextAnchor.MiddleCenter, alignItems = Align.Center, flexDirection = FlexDirection.Row } });
            _textHolder.Add(new TextElement { pickingMode = PickingMode.Ignore, text = "Weight: ", style = { fontSize = 12, height = 15 } });
            _textHolder.Add(_textWeight = new TextElement { pickingMode = PickingMode.Ignore, text = "100%", style = { fontSize = 15, color = RadialMenuUtility.Colors.CustomSelected, height = 15 } });
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
            if (!GUI.enabled) return;
            if (Drag) Update(Event.current.mousePosition);
            SetWeight(_controller.Module.GetParam(_target)?.FloatValue() ?? 0f);

            rect.center += new Vector2((rect.width - _slider.style.width.value.value) / 2 - 10, -10);
            rect.width = _slider.style.width.value.value + 20f;
            rect.height += 10f;
            if (Event.current.type == EventType.MouseDown) CheckDragStart(rect);
        }

        private void CheckDragStart(Rect rect)
        {
            if (!rect.Contains(Event.current.mousePosition)) return;
            _controller.DisableDrag();
            Drag = true;
            UpdatePosition(Event.current.mousePosition, false);
        }

        protected override bool RenderCondition(ModuleVrc3 module, RadialMenu menu) => menu.ToolBar.Selected == 0;

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
            _controller.Module.GetParam(_target)?.Set(_controller.Module, weight);
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
            _textWeight.text = $"{(int)(_weight * 100f)}%";
        }
    }
}
#endif