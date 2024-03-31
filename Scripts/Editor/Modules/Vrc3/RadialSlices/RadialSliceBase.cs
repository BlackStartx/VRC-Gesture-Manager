#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Library.VisualElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices
{
    public abstract class RadialSliceBase : GmgCircleElement
    {
        internal bool Selected;

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

        private bool _enabled = true;

        [PublicAPI]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                Text.Color = _enabled ? Color.white : Color.gray;
                Texture.style.unityBackgroundImageTintColor = new StyleColor(_enabled ? Color.white : Color.gray);
            }
        }

        private readonly VisualElement _sub;
        public readonly VisualElement DataHolder;
        protected readonly VisualElement Texture;
        protected readonly GmgTmpRichTextElement Text;

        protected RadialSliceBase(string text, Texture2D icon, Texture2D subIcon = null, bool enabled = true)
        {
            DataHolder = RadialMenuUtility.Prefabs.NewData(100, 100);
            Texture = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 50, height = 50, backgroundImage = !icon ? ModuleVrc3Styles.Default : icon } };
            Text = new GmgTmpRichTextElement { pickingMode = PickingMode.Ignore, Text = text, style = { unityTextAlign = TextAnchor.MiddleCenter } };
            RadialMenuUtility.Prefabs.SetSlice(this, RadialMenu.Size, IdleCenterColor, IdleBorderColor, RadialMenuUtility.Colors.CustomBorder);
            if (subIcon) _sub = RadialMenuUtility.Prefabs.NewSubIcon(subIcon);
            Enabled = enabled;
        }

        public RadialSliceBase Create()
        {
            DataHolder.Add(Texture);
            DataHolder.Add(_sub);
            DataHolder.Add(Text);
            CreateExtra();
            return this;
        }

        protected internal abstract void CheckRunningUpdate();

        protected abstract void CreateExtra();

        public abstract void OnClickStart();

        public abstract void OnClickEnd();
    }
}
#endif