#if VRC_SDK_VRCSDK3
using GestureManager.Scripts.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons
{
    public abstract class RadialMenuItem
    {
        internal Color TextColor;

        public VisualElement Border;
        public VisualElement DataHolder;
        protected VisualElement Texture;

        private readonly string _text;
        private readonly Texture2D _texture;
        private readonly Texture2D _subIcon;

        protected RadialMenuItem(string text, Texture2D icon, Texture2D subIcon)
        {
            _text = text;
            _texture = icon ? icon : ModuleVrc3Styles.Default;
            _subIcon = subIcon;
            TextColor = Color.white;
        }

        public void Create(float size)
        {
            Border = RadialMenuUtility.Prefabs.NewBorder(size / 2);
            DataHolder = RadialMenuUtility.Prefabs.NewData(100, 100);
            Texture = DataHolder.MyAdd(new VisualElement {style = {width = 50, height = 50, backgroundImage = _texture}});
            if (_subIcon) DataHolder.Add(RadialMenuUtility.Prefabs.NewSubIcon(_subIcon));
            DataHolder.MyAdd(new TextElement {text = _text, style = {color = TextColor, unityTextAlign = TextAnchor.MiddleCenter}});
            CreateExtra();
        }

        protected abstract void CreateExtra();

        public abstract void OnClickStart();

        public abstract void OnClickEnd();
    }
}
#endif