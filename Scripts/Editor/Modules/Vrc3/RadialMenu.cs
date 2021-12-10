#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialMenu : Vrc3VisualRender
    {
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
            "lindesu"
        };

        private Vector2 ExternalPosition()
        {
            var vector2 = Event.current.mousePosition - Rect.center;
            if (_puppet != null) vector2 -= new Vector2(_puppet.style.left.value.value, _puppet.style.top.value.value);
            return vector2;
        }

        private readonly List<RadialPage> _menuPath = new List<RadialPage>();

        private VRCExpressionsMenu _menu;
        internal IEnumerable<RadialMenu> RadialMenus => _module.RadialMenus.Values;

        internal GmgLayoutHelper.GmgToolbarHeader ToolBar;

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
         * Events
         */

        private void OnClickStart()
        {
            if (_puppet != null) return;
            var choice = _cursor.GetChoice(ExternalPosition(), _borderHolder);
            if (choice != -1) _buttons[choice].OnClickStart();
        }

        private void OnClickEnd()
        {
            if (_puppet == null)
            {
                var choice = _cursor.GetChoice(ExternalPosition(), _borderHolder);
                if (choice != -1) _buttons[choice].OnClickEnd();
                PressingButton?.OnClickEnd();
            }
            else ClosePuppet();
        }

        /*
         * Core
         */

        public override void Render(VisualElement root, Rect rect)
        {
            if (Event.current.type == EventType.MouseDown) OnClickStart();
            base.Render(root, rect);
            if (Event.current.type == EventType.MouseUp) OnClickEnd();
            HandleExternalInput();
        }

        protected override bool RenderCondition(int selectedIndex) => selectedIndex != 2;

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
            _cursor.SetData(20F, float.MaxValue, (int)(InnerSize / 2f), (int)(Size / 2f), _puppet);
            _cursor.Update(ExternalPosition());
            _puppet.AfterCursor();
        }

        private void ClosePuppet()
        {
            if (_puppet == null) return;

            _puppetHolder.Remove(_puppet);
            _cursor.SetData(Clamp, ClampReset, (int)(InnerSize / 2f), (int)(Size / 2f), _radial);
            _puppet.OnClose();
            _puppet = null;

            _cursor.Update(Event.current.mousePosition);
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
            _radialDescription = _module.DummyDescription();
        }

        private void OptionMainMenuPrefab()
        {
            OpenCustom(new RadialMenuItem[]
            {
                new RadialMenuButton(OptionExtraMenuPrefab, "Extra", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionTrackingMenuPrefab, "Tracking", ModuleVrc3Styles.Option),
                new RadialMenuButton(_module.EnableEditMode, "Edit-Mode", ModuleVrc3Styles.Default),
                new RadialMenuButton(OptionStatesMenuPrefab, "States", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionLocomotionMenuPrefab, "Locomotion", ModuleVrc3Styles.Option)
            });
        }

        private void OptionLocomotionMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Grounded", GetParam("Grounded")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Falling Speed", GetParam("VelocityY")),
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
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Full Body", param, 6f)
            });
            _radialDescription = new RadialDescription("If you don't know what those are you can check the", "documentation!", "", Application.OpenURL, TrackingDocumentationUrl);
        }

        private void OptionStatesMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "T Pose", _module.PoseT),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "AFK", GetParam("AFK")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Seated", GetParam("Seated")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "IK Pose", _module.PoseIK)
            });
        }

        private void OptionExtraMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "IsLocal", GetParam("IsLocal")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nRight Weight", GetParam("GestureRightWeight")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "MuteSelf", GetParam("MuteSelf")),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nLeft Weight", GetParam("GestureLeftWeight")),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "InStation", GetParam("InStation"))
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
                new RadialMenuButton(null, _supporters[5], ModuleVrc3Styles.SupportLike)
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

        public void ShowRadialDescription() => _radialDescription?.Show();

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
            pickingMode = PickingMode.Ignore;

            _radial = this.MyAdd(RadialMenuUtility.Prefabs.NewCircle(Size, RadialMenuUtility.Colors.RadialCenter, RadialMenuUtility.Colors.RadialMiddle, RadialMenuUtility.Colors.RadialBorder));

            _borderHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = Position.Absolute } });
            _radial.MyAdd(RadialMenuUtility.Prefabs.NewCircle((int)InnerSize, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.OuterBorder, Position.Absolute));

            _dataHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = Position.Absolute } });
            _puppetHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = Position.Absolute } });
            _radial.MyAdd(_cursor);

            _cursor.SetData(Clamp, ClampReset, (int)(InnerSize / 2f), (int)(Size / 2f), _radial);
        }

        private void SetButtons(int count, Func<int, RadialMenuItem> create) => SetButtons((from index in Enumerable.Range(0, count) select create(index)).ToArray());

        private void SetButtons(RadialMenuItem[] buttons)
        {
            _radialDescription = null;
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

        internal void UpdateValue(string pName, float amplified, float value)
        {
            _puppet?.UpdateValue(pName, value);
            if (!UpdateMenus(pName, amplified)) UpdateRunning();
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
    }
}
#endif