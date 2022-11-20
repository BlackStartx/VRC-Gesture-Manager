#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Runtime.VisualElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public abstract class RadialSliceBase : GmgCircleElement
    {
        internal bool Selected;

        internal Color TextColor;

        private Color _idleBorderColor = RadialMenuUtility.Colors.CustomMain;

        [PublicAPI]
        public Color IdleBorderColor
        {
            get => _idleBorderColor;
            set => _idleBorderColor = Selected ? value : VertexColor = value;
        }

        private Color _idleCenterColor = RadialMenuUtility.Colors.CenterIdle;

        [PublicAPI]
        public Color IdleCenterColor
        {
            get => _idleCenterColor;
            set => _idleCenterColor = Selected ? value : CenterColor = value;
        }

        private Color _selectedBorderColor = RadialMenuUtility.Colors.CustomSelected;

        [PublicAPI]
        public Color SelectedBorderColor
        {
            get => _selectedBorderColor;
            set => _selectedBorderColor = Selected ? VertexColor = value : value;
        }

        private Color _selectedCenterColor = RadialMenuUtility.Colors.CenterSelected;

        [PublicAPI]
        public Color SelectedCenterColor
        {
            get => _selectedCenterColor;
            set => _selectedCenterColor = Selected ? CenterColor = value : value;
        }

        public VisualElement DataHolder;
        protected VisualElement Texture;
        protected GmgTmpRichTextElement Text;

        private readonly string _text;
        private readonly Texture2D _texture;
        private readonly Texture2D _subIcon;

        protected RadialSliceBase(string text, Texture2D icon, Texture2D subIcon)
        {
            _text = text;
            _texture = icon ? icon : ModuleVrc3Styles.Default;
            _subIcon = subIcon;
            TextColor = Color.white;
            RadialMenuUtility.Prefabs.SetSlice(this, RadialMenu.Size, IdleCenterColor, IdleBorderColor, RadialMenuUtility.Colors.CustomBorder);
        }

        public RadialSliceBase Create()
        {
            DataHolder = RadialMenuUtility.Prefabs.NewData(100, 100);
            DataHolder.Add(Texture = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 50, height = 50, backgroundImage = _texture } });
            if (_subIcon) DataHolder.Add(RadialMenuUtility.Prefabs.NewSubIcon(_subIcon));
            DataHolder.Add(Text = new GmgTmpRichTextElement { pickingMode = PickingMode.Ignore, Text = _text, style = { color = TextColor, unityTextAlign = TextAnchor.MiddleCenter } });
            CreateExtra();
            return this;
        }

        protected abstract void CreateExtra();

        public abstract void OnClickStart();

        public abstract void OnClickEnd();
    }
}
#endif