#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Core.VisualElements;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialPuppets.Base;
using VRC.SDK3.Avatars.ScriptableObjects;
using UIEPosition = UnityEngine.UIElements.Position;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialMenu : Vrc3VisualRender
    {
        private readonly RadialCursor _cursor;
        internal readonly ModuleVrc3 Module;

        public const float Size = 300;

        private const float InnerSize = Size / 3;
        private const float Clamp = Size / 3;
        private const float ClampReset = Size / 1.7f;

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
        private readonly List<GmgButton> _selectionTuple = new List<GmgButton>();

        private VRCExpressionsMenu _menu;

        internal GmgLayoutHelper.Toolbar ToolBar;
        internal GmgLayoutHelper.Toolbar DebugToolBar;

        private bool Puppet => _puppet != null;
        private BasePuppet _puppet;
        private RadialDescription _radialDescription;


        private VisualElement _borderHolder;
        private VisualElement _sliceHolder;
        private VisualElement _dataHolder;

        private VisualElement _puppetHolder;
        private VisualElement _radial;

        internal RadialMenuItem PressingButton;
        private RadialMenuItem[] _buttons;

        private readonly bool _official;

        public RadialMenu(ModuleVrc3 module, bool official = false)
        {
            Module = module;
            _official = official;
            _cursor = new RadialCursor();
            CreateRadial();
        }

        /*
         * Events
         */

        private void OnClickStart()
        {
            if (_puppet != null) return;
            if (_cursor.Selection != -1) _buttons[_cursor.Selection].OnClickStart();
        }

        private void OnClickEnd()
        {
            if (_puppet == null)
            {
                if (_cursor.Selection != -1) _buttons[_cursor.Selection].OnClickEnd();
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
            if (!_official) GestureManagerStyles.Sign("UI");
        }

        protected override bool RenderCondition(ModuleVrc3 module, RadialMenu menu) => menu.ToolBar.Selected == 0;

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
            _cursor.SetData(puppet.Clamp, float.MaxValue, (int)(InnerSize / 2f), (int)(Size / 2f), _puppet);
            _cursor.Update(ExternalPosition(), _selectionTuple, Puppet);
            _puppet.AfterCursor();
        }

        private void ClosePuppet()
        {
            if (_puppet == null) return;

            _puppetHolder.Remove(_puppet);
            _cursor.SetData(Clamp, ClampReset, (int)(InnerSize / 2f), (int)(Size / 2f), _radial);
            _puppet.OnClose();
            _puppet = null;

            _cursor.Update(Event.current.mousePosition, _selectionTuple, Puppet);
        }

        /*
         * Menu Prefabs
         */

        internal void MainMenuPrefab()
        {
            _menuPath.Clear();
            var buttons = new RadialMenuItem[3];
            if (Module.DummyMode == null) buttons[0] = new RadialMenuButton(OptionMainMenuPrefab, "Options", ModuleVrc3Styles.Option);
            else buttons[0] = RadialMenuUtility.Buttons.ToggleFromParam(this, Module.DummyMode.ExitDummyText, Module.Dummy);
            if (Module.DummyMode != null || !_menu) buttons[1] = new RadialMenuButton(Module.NoExpressionRefresh, "Expressions", ModuleVrc3Styles.NoExpressions, Color.gray);
            else buttons[1] = new RadialMenuButton(ExpressionsMenu, "Expressions", ModuleVrc3Styles.Expressions);
            buttons[2] = new RadialMenuButton(SupporterMenuPrefab, "Thanks to...", ModuleVrc3Styles.Emojis);
            SetButtons(buttons);
            _radialDescription = Module.DummyMode?.DummyDescription();
        }

        private void OptionMainMenuPrefab()
        {
            OpenCustom(new RadialMenuItem[]
            {
                new RadialMenuButton(OptionExtraMenuPrefab, "Extra", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionTrackingMenuPrefab, "Tracking", ModuleVrc3Styles.Option),
                new RadialMenuButton(Module.EnableEditMode, "Edit-Mode", ModuleVrc3Styles.Default),
                new RadialMenuButton(OptionStatesMenuPrefab, "States", ModuleVrc3Styles.Option),
                new RadialMenuButton(OptionLocomotionMenuPrefab, "Locomotion", ModuleVrc3Styles.Option)
            });
        }

        private void OptionLocomotionMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Grounded", GetParam(Vrc3DefaultParams.Grounded)),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Falling Speed", GetParam(Vrc3DefaultParams.VelocityY), -22f),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Upright", GetParam(Vrc3DefaultParams.Upright)),
                RadialMenuUtility.Buttons.AxisFromParams(this, "Velocity", GetParam(Vrc3DefaultParams.VelocityX), GetParam(Vrc3DefaultParams.VelocityZ), 7f)
            });
        }

        private void OptionTrackingMenuPrefab()
        {
            var param = GetParam(Vrc3DefaultParams.TrackingType);
            OpenCustom(new RadialMenuItem[]
            {
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Uninitialized", param, 0f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Generic", param, 1f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Hands-only", param, 2f),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "VRMode", GetParam(Vrc3DefaultParams.VRMode)),
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
                RadialMenuUtility.Buttons.ToggleFromParam(this, "T Pose", Module.PoseT),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "AFK", GetParam(Vrc3DefaultParams.Afk)),
                RadialMenuUtility.Buttons.RadialFromParam(this, Vrc3DefaultParams.Vise, GetParam(Vrc3DefaultParams.Vise), Module.ViseAmount),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "Seated", GetParam(Vrc3DefaultParams.Seated)),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "IK Pose", Module.PoseIK)
            });
        }

        private void OptionExtraMenuPrefab()
        {
            OpenCustom(new[]
            {
                RadialMenuUtility.Buttons.ToggleFromParam(this, "IsLocal", GetParam(Vrc3DefaultParams.IsLocal)),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nRight Weight", GetParam(Vrc3DefaultParams.GestureRightWeight)),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "MuteSelf", GetParam(Vrc3DefaultParams.MuteSelf)),
                RadialMenuUtility.Buttons.RadialFromParam(this, "Gesture\nLeft Weight", GetParam(Vrc3DefaultParams.GestureLeftWeight)),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "InStation", GetParam(Vrc3DefaultParams.InStation))
            });
        }

        private void SupporterMenuPrefab()
        {
            OpenCustom(new[]
            {
                new RadialMenuButton(null, _supporters[0], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[1], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[2], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, "You!", ModuleVrc3Styles.SupportHeart) { SelectedCenterColor = Color.red },
                new RadialMenuButton(null, _supporters[3], ModuleVrc3Styles.SupportLike),
                new RadialMenuButton(null, _supporters[4], ModuleVrc3Styles.SupportLike) { SelectedCenterColor = Color.blue },
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
            var defaultButtonsInt = isMain ? 2 : 1;
            var intCount = menu.controls.Count + defaultButtonsInt;
            var intMax = defaultButtonsInt + 8;

            SetButtons(intCount, intMax, intCurrent =>
            {
                switch (intCurrent)
                {
                    case 0: return new RadialMenuButton(GoBack, "Back", isMain ? ModuleVrc3Styles.BackHome : ModuleVrc3Styles.Back);
                    case 1 when isMain: return new RadialMenuButton(Module.ResetAvatar, "Reset Avatar", ModuleVrc3Styles.Reset);
                    default: return new RadialMenuControl(this, menu.controls[intCurrent - defaultButtonsInt]);
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
            param?.Set(Module, 0);
        }

        /*
         * Radial Stuff
         */

        public Vrc3Param GetParam(string pName) => Module.GetParam(pName);

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
            style.position = UIEPosition.Absolute;
            pickingMode = PickingMode.Ignore;

            _radial = this.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { justifyContent = Justify.Center, position = UIEPosition.Absolute, alignItems = Align.Center } });

            _sliceHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _borderHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _radial.MyAdd(RadialMenuUtility.Prefabs.NewCircle((int)InnerSize, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));

            _dataHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _puppetHolder = _radial.MyAdd(new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _radial.MyAdd(_cursor);

            _cursor.SetData(Clamp, ClampReset, (int)(InnerSize / 2f), (int)(Size / 2f), _radial);
        }

        private void SetButtons(int count, Func<int, RadialMenuItem> create) => SetButtons(count, 10, create);

        private void SetButtons(int count, int max, Func<int, RadialMenuItem> create) => SetButtons((from iInt in Enumerable.Range(0, Math.Min(count, max)) select create(iInt)).ToArray());

        private void SetButtons(RadialMenuItem[] buttons)
        {
            _radialDescription = null;
            _buttons = buttons;

            _selectionTuple.Clear();
            _borderHolder.Clear();
            _sliceHolder.Clear();
            _dataHolder.Clear();

            var step = 360f / _buttons.Length;
            var current = -step / 2;
            var progress = 1f / _buttons.Length;

            var rStep = Mathf.PI * 2 / _buttons.Length;
            var rCurrent = Mathf.PI;

            foreach (var item in _buttons)
            {
                item.Create();

                var circleHolder = new VisualElement();
                var circle = circleHolder.MyAdd(RadialMenuUtility.Prefabs.NewSlice(Size, RadialMenuUtility.Colors.RadialCenter, RadialMenuUtility.Colors.CustomMain, RadialMenuUtility.Colors.CustomBorder));
                circle.Progress = progress;

                _sliceHolder.MyAdd(circleHolder).transform.rotation = Quaternion.Euler(0, 0, current);
                _borderHolder.MyAdd(RadialMenuUtility.Prefabs.NewBorder(Size / 2)).transform.rotation = Quaternion.Euler(0, 0, current - 90);

                item.DataHolder.transform.position = new Vector3(Mathf.Sin(rCurrent) * Size / 3, Mathf.Cos(rCurrent) * Size / 3, 0);

                _dataHolder.MyAdd(item.DataHolder);
                _selectionTuple.Add(new GmgButton { Button = item, Data = item.DataHolder, CircleElement = circle });

                current += step;
                rCurrent -= rStep;
            }

            _cursor.Selection = _cursor.GetChoice(buttons.Length, Puppet);
            if (_cursor.Selection != -1) RadialCursor.Sel(_selectionTuple[_cursor.Selection], true);
        }

        private void HandleExternalInput()
        {
            if (Event.current.type == EventType.Layout) return;

            var mouseVector = ExternalPosition();
            _cursor.Update(mouseVector, _selectionTuple, Puppet);
            _puppet?.Update(_cursor);
        }

        internal void UpdateValue(string pName, float value)
        {
            _puppet?.UpdateValue(pName, value);
            if (!UpdateMenus(pName, value)) UpdateRunning();
        }

        private bool UpdateMenus(string pName, float value)
        {
            if (string.IsNullOrEmpty(pName)) return false;

            var list = _menuPath.TakeWhile(page => page.Param?.Name != pName || RadialMenuUtility.Is(page.Value, value)).ToList();
            if (list.Count == _menuPath.Count) return false;

            for (var i = list.Count; i < _menuPath.Count; i++) RemoveMenu(list.Count);
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

    public class GmgButton
    {
        internal GmgCircleElement CircleElement;
        internal RadialMenuItem Button;
        internal VisualElement Data;
    }
}
#endif