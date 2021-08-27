#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GestureManager.Scripts.Core;
using UnityEngine;
using UnityEngine.UIElements;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialMenu : VisualElement
    {
        private EditorWindow _inspectorWindow;

        private readonly RadialCursor _cursor;
        private readonly ModuleVrc3 _module;

        public const float Size = 300;

        private const float InnerSize = Size / 3;
        private const float Clamp = Size / 3;
        private const float ClampReset = Size / 1.7f;

        private const int CursorSize = 50;

        private const string TrackingDocumentationUrl = "https://docs.vrchat.com/docs/animator-parameters#trackingtype-parameter";

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private readonly string[] _supporters =
        {
            "Stack_",
            "GaNyan",
            "Nayu",
            "Ahri~",
            "Hiro N.",
            "lindesu",
        };

        private Vector2 ExternalPosition()
        {
            const float hSize = Size / 2;
            var vector2 = Event.current.mousePosition - new Vector2(style.left.value.value + hSize, style.top.value.value + hSize);
            vector2 -= new Vector2(_cursor.parent.style.left.value.value, _cursor.parent.style.top.value.value);
            return vector2;
        }

        private readonly List<RadialPage> _menuPath = new List<RadialPage>();

        private VRCExpressionsMenu _menu;
        internal IEnumerable<RadialMenu> RadialMenus => _module.RadialMenus.Values;
        
        internal Rect Rect;

        private BasePuppet _puppet;
        private RadialDescription _radialDescription;

        private VisualElement _borderHolder;
        private VisualElement _dataHolder;
        private VisualElement _puppetHolder;
        private VisualElement _radial;

        internal RadialMenuItem PressingButton;
        private RadialMenuItem[] _buttons;

        public RadialMenu(ModuleVrc3 module)
        {
            _module = module;
            _cursor = new RadialCursor(CursorSize);
            CreateRadial();
        }

        /*
         * Visual Element Events
         */

        private void OnRadialClickStart(MouseDownEvent mouseDownEvent)
        {
            if (_puppet != null) return;
            var choice = _cursor.GetChoice(Event.current.mousePosition, _borderHolder);
            if (choice != -1) _buttons[choice].OnClickStart();
        }

        private void OnRadialClickEnd(MouseUpEvent evt)
        {
            if (_puppet == null)
            {
                var choice = _cursor.GetChoice(Event.current.mousePosition, _borderHolder);
                if (choice != -1) _buttons[choice].OnClickEnd();
                PressingButton?.OnClickEnd();
            }
            else ClosePuppet();
        }

        /*
         * Core
         */

        public int Render(VisualElement root, Rect rect)
        {
            if (Event.current.type != EventType.Layout && !root.Contains(this)) root.Add(this);
            if (Event.current.type == EventType.MouseUp) OnRadialClickEnd(null);

            style.left = (rect.width - Size) / 2;
            style.top = rect.y;

            HandleExternalInput();

            return _module.Debug.State ? DebugRender(rect) : 0;
        }

        /*
         * Puppets
         */

        internal void OpenPuppet(BasePuppet puppet)
        {
            if (_puppet != null) return;

            _puppet = puppet;
            _puppet.OnOpen();

            _puppetHolder.Add(_puppet);
            _puppet.style.left = _cursor.style.left;
            _puppet.style.top = _cursor.style.top;
            _cursor.SetData(20F, float.MaxValue, (int) (InnerSize / 2f), (int) (Size / 2f), _puppet);
            _cursor.Update(ExternalPosition());
            _puppet.AfterCursor();
        }

        private void ClosePuppet()
        {
            if (_puppet == null) return;

            _puppetHolder.Remove(_puppet);
            _cursor.SetData(Clamp, ClampReset, (int) (InnerSize / 2f), (int) (Size / 2f), _radial);
            _puppet.OnClose();
            _puppet = null;

            _cursor.Update(Event.current.mousePosition);
        }

        /*
         * Radial Descriptions
         */

        private void RemoveRadialDescription() => _radialDescription = null;

        private void SetRadialDescription(string text, string link, string url) => _radialDescription = new RadialDescription(text, link, url);

        public void ShowRadialDescription()
        {
            if (_radialDescription == null) return;
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GestureManagerStyles.EmoteError);
            GUILayout.FlexibleSpace();
            GUILayout.Label(_radialDescription.Text);

            var guiStyle = EditorGUIUtility.isProSkin ? ModuleVrc3Styles.UrlPro : ModuleVrc3Styles.Url;
            if (GUILayout.Button(_radialDescription.Link, guiStyle)) Application.OpenURL(_radialDescription.Url);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        /*
         * Menu Prefabs
         */

        internal void MainMenuPrefab()
        {
            _menuPath.Clear();
            var buttons = new RadialMenuItem[3];

            if (!_module.DummyAvatar) buttons[0] = new RadialMenuButton(OptionMainMenuPrefab, "Options", ModuleVrc3Styles.Option);
            else buttons[0] = RadialMenuUtility.Buttons.ToggleFromParam(this, _module.ExitDummyText, _module.Dummy);

            if (_module.DummyAvatar || !_menu) buttons[1] = new RadialMenuButton(_module.NoExpressionRefresh, "Expressions", ModuleVrc3Styles.NoExpressions, Color.gray);
            else buttons[1] = new RadialMenuButton(ExpressionsMenu, "Expressions", ModuleVrc3Styles.Expressions);

            buttons[2] = new RadialMenuButton(SupporterMenuPrefab, "Thanks to...", ModuleVrc3Styles.Emojis);

            SetButtons(buttons);
        }

        private void OptionMainMenuPrefab()
        {
            OpenCustom(new RadialMenuItem[]
            {
                new RadialMenuButton(OptionExtraMenuPrefab, "Extra", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionTrackingMenuPrefab, "Tracking", ModuleVrc3Styles.Option),
                new RadialMenuButton(_module.EnableEditMode, "Edit-Mode", ModuleVrc3Styles.Default),
                new RadialMenuButton(OptionStatesMenuPrefab, "States", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionLocomotionMenuPrefab, "Locomotion", ModuleVrc3Styles.Option),
            });
        }

        private void OptionLocomotionMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Grounded", GetParam("Grounded")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Upright", GetParam("Upright")),
                RadialMenuUtility.Buttons.AxisFromParams(this, "Velocity", GetParam("VelocityX"), GetParam("VelocityZ"))
            });
        }

        private void OptionTrackingMenuPrefab()
        {
            var param = GetParam("TrackingType");
            OpenCustom(new RadialMenuItem[]
            {
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Uninitialized", param, 0f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Generic", param, 1f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Hands-only", param, 2f),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "VRMode", GetParam("VRMode")),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Head And Hands", param, 3f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "4-Point VR", param, 4f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Full Body", param, 6f),
            });
            SetRadialDescription("If you don't know what those are you can check the", "documentation!", TrackingDocumentationUrl);
        }

        private void OptionStatesMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "AFK", GetParam("AFK")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Seated", GetParam("Seated")),
            });
        }

        private void OptionExtraMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "IsLocal", GetParam("IsLocal")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nLeft Weight", GetParam("GestureLeftWeight")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "MuteSelf", GetParam("MuteSelf")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nRight Weight", GetParam("GestureRightWeight")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "InStation", GetParam("InStation")),
            });
        }

        private void SupporterMenuPrefab()
        {
            OpenCustom(new[]
            {
                new RadialMenuButton(null, _supporters[0], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[1], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[2], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, "You!", ModuleVrc3Styles.SupportHeart),
                new RadialMenuButton(null, _supporters[3], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[4], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[5], ModuleVrc3Styles.SupportLike),
            });
        }

        private void ExpressionsMenu() => OpenMenu(_menu, null, 0f);

        /*
         * Menu
         */

        internal void OpenMenu(VRCExpressionsMenu menu, Vrc3Param param, float value)
        {
            _menuPath.Add(new RadialPage(this, menu, param, value));
            SetMenu(menu);
        }

        internal void SetMenu(VRCExpressionsMenu menu)
        {
            var isMain = menu == _menu;
            var defaultButtonsCount = isMain ? 2 : 1;

            SetButtons(menu.controls.Count + defaultButtonsCount, i =>
            {
                switch (i)
                {
                    case 0: return new RadialMenuButton(GoBack, "Back", isMain ? ModuleVrc3Styles.BackHome : ModuleVrc3Styles.Back);
                    case 1 when isMain: return new RadialMenuButton(_module.ResetAvatar, "Reset Avatar", ModuleVrc3Styles.Reset);
                    default: return new RadialMenuControl(this, menu.controls[i - defaultButtonsCount]);
                }
            });
        }

        /*
         * Custom Menu
         */

        private void OpenCustom(IReadOnlyList<RadialMenuItem> controls)
        {
            _menuPath.Add(new RadialPage(this, controls));
            SetCustom(controls);
        }

        internal void SetCustom(IReadOnlyList<RadialMenuItem> controls)
        {
            var isMain = _menuPath.Count == 1;
            SetButtons(controls.Count + 1, i =>
            {
                switch (i)
                {
                    case 0: return new RadialMenuButton(GoBack, "Back", isMain ? ModuleVrc3Styles.BackHome : ModuleVrc3Styles.Back);
                    default: return controls[i - 1];
                }
            });
        }

        /*
         * Menu Actions
         */

        private void GoBack()
        {
            RemoveMenu(_menuPath.Count - 1);
            if (_menuPath.Count == 0) MainMenuPrefab();
            else _menuPath[_menuPath.Count - 1].Open();
        }

        private void RemoveMenu(int index)
        {
            var param = _menuPath[index].Param;
            _menuPath.RemoveAt(index);
            param?.Set(RadialMenus, 0);
        }

        /*
         * Radial Stuff
         */

        public Vrc3Param GetParam(string pName) => _module.GetParam(pName);

        public void Set(VRCExpressionsMenu menu)
        {
            _menu = menu;
            MainMenuPrefab();
        }

        private void CreateRadial()
        {
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.position = Position.Absolute;
            this.OnClickUpEvent(OnRadialClickEnd);

            _radial = this.MyAdd(RadialMenuUtility.Prefabs.NewCircle(Size, RadialMenuUtility.Colors.RadialCenter, RadialMenuUtility.Colors.RadialMiddle, RadialMenuUtility.Colors.RadialBorder));
            _radial.OnClickDownEvent(OnRadialClickStart);

            _borderHolder = _radial.MyAdd(new VisualElement {style = {position = Position.Absolute}});
            _radial.MyAdd(RadialMenuUtility.Prefabs.NewCircle((int) InnerSize, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.OuterBorder, Position.Absolute));

            _dataHolder = _radial.MyAdd(new VisualElement {style = {position = Position.Absolute}});
            _puppetHolder = _radial.MyAdd(new VisualElement {style = {position = Position.Absolute}});
            _radial.MyAdd(_cursor);

            _cursor.SetData(Clamp, ClampReset, (int) (InnerSize / 2f), (int) (Size / 2f), _radial);
        }

        private void SetButtons(int count, Func<int, RadialMenuItem> create) => SetButtons((from index in Enumerable.Range(0, count) select create(index)).ToArray());

        private void SetButtons(RadialMenuItem[] buttons)
        {
            RemoveRadialDescription();
            _buttons = buttons;

            _borderHolder.Clear();
            _dataHolder.Clear();

            var step = 360f / _buttons.Length;
            var current = step / 2 - 90;

            var rStep = Mathf.PI * 2 / _buttons.Length;
            var rCurrent = Mathf.PI;

            foreach (var item in _buttons)
            {
                item.Create(Size);
                _borderHolder.MyAdd(item.Border).transform.rotation = Quaternion.Euler(0, 0, current);

                item.DataHolder.transform.position = new Vector3(Mathf.Sin(rCurrent) * Size / 3, Mathf.Cos(rCurrent) * Size / 3, 0);

                _dataHolder.MyAdd(item.DataHolder);
                current += step;
                rCurrent -= rStep;
            }
        }

        private void HandleExternalInput()
        {
            if (Event.current.type == EventType.Layout) return;

            var mouseVector = ExternalPosition();
            _cursor.Update(mouseVector);
            _puppet?.Update(mouseVector, _cursor);
        }

        internal void UpdateValue(string pName, float value)
        {
            _puppet?.UpdateValue(pName, value);
            if (!UpdateMenus(pName, value)) UpdateRunning();
        }

        private bool UpdateMenus(string pName, float value)
        {
            if (string.IsNullOrEmpty(pName)) return false;

            var list = _menuPath.TakeWhile(menu => menu.Param?.Name != pName || RadialMenuUtility.Is(menu.Value, value)).ToList();
            if (list.Count == _menuPath.Count) return false;

            var count = _menuPath.Count;
            for (var i = list.Count; i < count; i++) RemoveMenu(list.Count);
            _menuPath[_menuPath.Count - 1].Open();
            return true;
        }

        private void UpdateRunning()
        {
            foreach (var item in _buttons)
                if (item is RadialMenuDynamic dynamic)
                    dynamic.CheckRunningUpdate();
        }

        private int DebugRender(Rect rect)
        {
            const int size = 22;
            var list = _module.Params.BatchTwo().ToList();
            var intHeight = size * (list.Count + 1);

            GUILayout.BeginArea(new Rect(rect.x, rect.y, rect.width, intHeight));
            DebugPair($"[DEBUG ({_module.Params.Count})]", $"[({_module.Params.Count}) DEBUG]", ModuleVrc3Styles.DebugHeader);
            foreach (var (lPair, rPair, isRight) in list) DebugPair(lPair.Value, isRight ? rPair.Value : null);
            GUILayout.EndArea();
            return intHeight;
        }

        private static void DebugPair(Vrc3Param lPair, Vrc3Param rPair)
        {
            var left = $"[{lPair.Type.ToString()[0]}] {lPair.Name} = {lPair.Get()}";
            var right = rPair == null ? null : $"{rPair.Get()} = {rPair.Name} [{rPair.Type.ToString()[0]}]";
            DebugPair(left, right, GUI.skin.label);
        }

        private static void DebugPair(string left, string right, GUIStyle guiStyle)
        {
            GUILayout.BeginHorizontal();
            DebugElement(left, guiStyle);
            GUILayout.FlexibleSpace();
            if (right != null) DebugElement(right, guiStyle);
            GUILayout.EndHorizontal();
        }

        private static void DebugElement(string text, GUIStyle guiStyle)
        {
            GUILayout.BeginHorizontal(GUI.skin.textField);
            GUILayout.Label(text, guiStyle);
            GUILayout.EndHorizontal();
        }
    }
}
#endif