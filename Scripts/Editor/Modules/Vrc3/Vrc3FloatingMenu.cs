#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Library;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class Vrc3FloatingMenu : EditorWindow
    {
        private ModuleVrc3 _module;

        private RadialMenu _menu;
        private RadialMenu Menu => _menu ??= _module.GetOrCreateRadial(this, true);

        private readonly GUILayoutOption[] _options = { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };
        private static Vector2 HeaderSize => new(0, 21);

        private TextElement _focusElement;
        private TextElement FocusElement => _focusElement ??= rootVisualElement.MyAdd(TextElement);

        private static TextElement TextElement => new()
        {
            text = "The window is not focused!",
            style =
            {
                fontSize = 25, unityTextAlign = TextAnchor.MiddleCenter, position = UIEPosition.Absolute, visibility = Visibility.Hidden,
                unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 0.5f), backgroundColor = Color.black
            }
        };

        private bool _focused = true;

        private bool Focused
        {
            set
            {
                if (_focused == value) return;
                _focused = value;
                if (!value) StopRendering();
                FocusElement.SetVisibility(!value);
                if (value) Repaint();
            }
            get => _focused;
        }

        private void StopRendering()
        {
            if (_menu == null) return;
            FocusElement.style.backgroundImage = new StyleBackground(TextureV());
            FocusElement.style.height = new StyleLength(_menu.Rect.height);
            FocusElement.style.width = new StyleLength(_menu.Rect.width);
            FocusElement.BringToFront();
            _menu.StopRendering();
        }

        private Texture2D TextureV()
        {
            if (_menu == null || !InternalEditorUtility.isApplicationActive) return null;
            var width = (int)_menu.Rect.width;
            var height = (int)_menu.Rect.height;
            var texture = new Texture2D(width, height);
            texture.SetPixels(InternalEditorUtility.ReadScreenPixel(position.position + HeaderSize, width, height));
            texture.Apply();
            return texture;
        }

        private void Awake() => this.MySetAntiAliasing(4);

        public void Update() => Repaint();

        private void OnFocus() => Focused = true;

        private void OnLostFocus() => Focused = false;

        private void OnGUI()
        {
            if (_module is not { Active: true } || _module.Broken) Close();
            else if (Focused) Gui();
        }

        private void Gui()
        {
            GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, _options);
            Menu.Render(rootVisualElement, GmgLayoutHelper.GetLastRect(ref Menu.Rect));
        }

        public static void Create(ModuleVrc3 module, string text = "Radial Menu")
        {
            var instance = CreateInstance<Vrc3FloatingMenu>();
            instance.titleContent = new GUIContent(text);
            instance._module = module;
            instance.Show();
        }
    }
}
#endif