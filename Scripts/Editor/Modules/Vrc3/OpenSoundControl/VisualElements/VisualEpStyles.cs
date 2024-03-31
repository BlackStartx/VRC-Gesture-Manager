#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEngine.UIElements;
using BlackStartX.GestureManager.Library;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements
{
    public static class VisualEpStyles
    {
        private const int TextHeight = 20;
        private const int TextMargin = 5;

        internal const int Radius = 5;
        internal const int SquareSize = 200;
        internal const int InnerSize = SquareSize - TextHeight - TextHeight - TextMargin - TextMargin;

        public static StyleColor BgColor = new Color(0.21f, 0.52f, 0.57f);

        internal static VisualElement Center => new()
        {
            pickingMode = PickingMode.Ignore,
            style = { height = InnerSize, width = InnerSize, left = TextHeight + TextMargin }
        };

        public static VisualElement Holder => new VisualElement
        {
            style =
            {
                width = InnerSize,
                height = InnerSize,
                backgroundColor = Color.black
            }
        }.MyBorder(0, Radius, Color.clear);

        public static VisualElement Outline => new VisualElement
        {
            style =
            {
                position = UIEPosition.Absolute,
                width = InnerSize,
                height = InnerSize
            }
        }.MyBorder(1, Radius, Color.gray);

        internal static TextElement HorizontalText => new TextElement
        {
            pickingMode = PickingMode.Ignore,
            style =
            {
                color = Color.white,
                unityTextAlign = TextAnchor.MiddleLeft,
                right = -TextHeight - TextMargin,
                top = -TextHeight + TextMargin,
                backgroundColor = Color.black,
                overflow = Overflow.Hidden,
                height = TextHeight,
                width = InnerSize,
                fontSize = 10
            }
        }.MyBorder(1, 5, Color.black);

        internal static TextElement VerticalText
        {
            get
            {
                var element = HorizontalText;
                element.transform.rotation = Quaternion.Euler(0, 0, 90);
                element.style.right = -TextHeight;
                element.style.top = -InnerSize;
                element.visible = false;
                return element;
            }
        }
    }
}
#endif