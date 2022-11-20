﻿#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Editor.Lib;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialPuppets.Base;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;
using UIEPosition = UnityEngine.UIElements.Position;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class RadialMenu : Vrc3VisualRender
    {
        private readonly RadialCursor _cursor;
        internal readonly ModuleVrc3 Module;

        public const float Size = 300;

        private const float MaxSize = Size / 2;

        private const float InnerSize = Size / 3;
        private const float MinSize = InnerSize / 2f;

        private const float Clamp = Size / 3;
        private const float ClampReset = Size / 1.7f;

        private const string TrackingDocumentationUrl = "https://docs.vrchat.com/docs/animator-parameters#trackingtype-parameter";

        private Vector2 MousePosition()
        {
            var vector2 = Event.current.mousePosition - Rect.center;
            if (_puppet != null) vector2 -= new Vector2(_puppet.style.left.value.value, _puppet.style.top.value.value);
            return vector2;
        }

        private readonly List<RadialPage> _menuPath = new List<RadialPage>();

        private VRCExpressionsMenu _menu;

        internal GmgLayoutHelper.Toolbar ToolBar;
        internal GmgLayoutHelper.Toolbar DebugToolBar;

        private bool Puppet => _puppet != null;
        private BasePuppet _puppet;
        private RadialDescription _radialDescription;

        private VisualElement _puppetHolder;
        private VisualElement _sliceHolder;
        private VisualElement _textHolder;
        private VisualElement _radial;

        internal RadialSliceBase PressingButton;
        private RadialSliceBase[] _buttons;

        private readonly bool _official;

        public RadialMenu(ModuleVrc3 module, bool official)
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
            _cursor.SetData(puppet.Clamp, float.MaxValue, MinSize, MaxSize, _puppet);
            _cursor.Update(MousePosition(), _buttons, Puppet);
            _puppet.AfterCursor();
        }

        private void ClosePuppet()
        {
            if (_puppet == null) return;

            _puppetHolder.Remove(_puppet);
            _cursor.SetData(Clamp, ClampReset, MinSize, MaxSize, _radial);
            _puppet.OnClose();
            _puppet = null;

            _cursor.Update(Event.current.mousePosition, _buttons, Puppet);
        }

        /*
         * Menu Prefabs
         */

        internal void MainMenuPrefab()
        {
            _menuPath.Clear();
            var buttons = new RadialSliceBase[4];
            if (Module.DummyMode == null) buttons[0] = new RadialSliceButton(OptionMainMenuPrefab, "Options", ModuleVrc3Styles.Option);
            else buttons[0] = new RadialSliceButton(Module.DummyMode.StopExecution, Module.DummyMode.ExitDummyText, null, active: true);
            if (Module.DummyMode != null || !_menu) buttons[1] = new RadialSliceButton(Module.NoExpressionRefresh, "Expressions", ModuleVrc3Styles.NoExpressions, Color.gray);
            else buttons[1] = new RadialSliceButton(ExpressionsMenu, "Expressions", ModuleVrc3Styles.Expressions);
            buttons[2] = new RadialSliceButton(SupporterMenuPrefab, "Thanks to...", ModuleVrc3Styles.Emojis);
            buttons[3] = new RadialSliceButton(ToolMenuPrefab, "Tools", ModuleVrc3Styles.Tools);
            SetButtons(buttons);
            _radialDescription = Module.DummyMode?.DummyDescription();
        }

        private void OptionMainMenuPrefab()
        {
            OpenCustom(new RadialSliceBase[]
            {
                new RadialSliceButton(OptionExtraMenuPrefab, "Extra", ModuleVrc3Styles.Option),
                new RadialSliceButton(OptionTrackingMenuPrefab, "Tracking", ModuleVrc3Styles.Option),
                new RadialSliceButton(Module.EnableEditMode, "Edit-Mode", ModuleVrc3Styles.Default),
                new RadialSliceButton(OptionStatesMenuPrefab, "States", ModuleVrc3Styles.Option),
                new RadialSliceButton(OptionLocomotionMenuPrefab, "Locomotion", ModuleVrc3Styles.Option)
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
            OpenCustom(new RadialSliceBase[]
            {
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Uninitialized", param, 0f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Generic", param, 1f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Hands-only", param, 2f),
                RadialMenuUtility.Buttons.ToggleFromParam(this, "VRMode", GetParam(Vrc3DefaultParams.VRMode)),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Head And Hands", param, 3f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "4-Point VR", param, 4f),
                RadialMenuUtility.Buttons.ParamStateToggle(this, "Full Body", param, 6f)
            });
            _radialDescription = new RadialDescription("If you don't know what those are you can check the ", "documentation!", "", Application.OpenURL, TrackingDocumentationUrl);
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
            Vrc3Supporter.Check();
            OpenCustom(new RadialSliceBase[]
            {
                new RadialSliceSupporter(true, 0, 3, 400, ModuleVrc3Styles.SupportLike),
                new RadialSliceSupporter(true, 1, 3, 800, ModuleVrc3Styles.SupportLike),
                new RadialSliceSupporter(true, 2, 3, 1200, ModuleVrc3Styles.SupportLike),
                new RadialSliceButton(null, "You!", ModuleVrc3Styles.SupportHeart) { SelectedCenterColor = Color.red },
                new RadialSliceSupporter(false, 0, 3, 1600, ModuleVrc3Styles.SupportGold),
                new RadialSliceSupporter(false, 1, 3, 2000, ModuleVrc3Styles.SupportGold),
                new RadialSliceSupporter(false, 2, 3, 2400, ModuleVrc3Styles.SupportGold)
            });
        }

        private void ToolMenuPrefab()
        {
            OpenCustom(new RadialSliceBase[]
            {
                new RadialSliceTool(Module, Module.AvatarTools.SceneCamera, ModuleVrc3Styles.ToolCamera),
                new RadialSliceTool(Module, Module.AvatarTools.ContactsClickable, ModuleVrc3Styles.ToolClick),
                new RadialSliceTool(Module, Module.AvatarTools.PoseAvatar, ModuleVrc3Styles.ToolPose)
            });
            _radialDescription = new RadialDescription("You edit each tool settings in the \"", "Tools", "\" tab!", OpenToolPage);
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
                    case 0: return new RadialSliceButton(GoBack, "Back", isMain ? ModuleVrc3Styles.BackHome : ModuleVrc3Styles.Back);
                    case 1 when isMain: return new RadialSliceButton(Module.ResetAvatar, "Reset Avatar", ModuleVrc3Styles.Reset);
                    default: return new RadialSliceControl(this, menu.controls[intCurrent - defaultButtonsInt]);
                }
            });
        }

        /*
         * Custom Menu
         */

        private void OpenCustom(IReadOnlyList<RadialSliceBase> controls)
        {
            _menuPath.Add(new RadialPage(this, controls));
            SetCustom(controls);
        }

        internal void SetCustom(IReadOnlyList<RadialSliceBase> controls)
        {
            var isMain = _menuPath.Count == 1;
            SetButtons(controls.Count + 1, 10, i =>
            {
                switch (i)
                {
                    case 0: return new RadialSliceButton(GoBack, "Back", isMain ? ModuleVrc3Styles.BackHome : ModuleVrc3Styles.Back);
                    default: return controls[i - 1];
                }
            });
        }

        /*
         * Menu Actions
         */

        private void OpenToolPage(string s)
        {
            ToolBar.Selected = 1;
            MainMenuPrefab();
        }

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
         * Radial Button Creation Types
         */

        private void ButtonCreationDefault(int index, float progress, float dCurrent, float pCurrent)
        {
            var slice = _buttons[index].Create();
            slice.Progress = progress;

            _sliceHolder.MyAdd(new VisualElement().With(slice)).transform.rotation = Quaternion.Euler(0, 0, dCurrent);
            _sliceHolder.MyAdd(RadialMenuUtility.Prefabs.NewBorder(MaxSize)).transform.rotation = Quaternion.Euler(0, 0, dCurrent - 90);
            _textHolder.MyAdd(slice.DataHolder).transform.position = new Vector3(Mathf.Sin(pCurrent) * Size / 3, Mathf.Cos(pCurrent) * Size / 3, 0);
        }

        /*
         * Radial Stuff
         */

        public Vrc3Param GetParam(string paramName) => Module.GetParam(paramName);

        public void ShowRadialFooter()
        {
            GUILayout.Space(10);
            _radialDescription?.Show();
            if (!Module.PoseMode) return;
            using (new GmgLayoutHelper.GuiBackground(Color.yellow))
            using (new GUILayout.HorizontalScope(GestureManagerStyles.EmoteError))
                GUILayout.Label("You are in Pose-Mode!\n\nYou can pose your avatar but the animations of your bones are disabled!", GestureManagerStyles.SubHeader);
        }

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

            Add(_radial = new VisualElement { pickingMode = PickingMode.Ignore, style = { justifyContent = Justify.Center, position = UIEPosition.Absolute, alignItems = Align.Center } });
            _radial.Add(_sliceHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _radial.Add(RadialMenuUtility.Prefabs.NewCircle(InnerSize, RadialMenuUtility.Colors.RadialInner, RadialMenuUtility.Colors.CustomBorder, UIEPosition.Absolute));
            _radial.Add(_textHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _radial.Add(_puppetHolder = new VisualElement { pickingMode = PickingMode.Ignore, style = { position = UIEPosition.Absolute } });
            _radial.Add(_cursor);

            _cursor.SetData(Clamp, ClampReset, MinSize, MaxSize, _radial);
        }

        private void SetButtons(int count, int max, Func<int, RadialSliceBase> create) => SetButtons((from iInt in Enumerable.Range(0, Math.Min(count, max)) select create(iInt)).ToArray());

        private void SetButtons(RadialSliceBase[] buttons, int deSel = -1)
        {
            _cursor.UpdateSelection(_buttons, deSel);
            _buttons = buttons;
            SetButtons(buttons.Length, ButtonCreationDefault);
            _cursor.Selection = _cursor.GetChoice(buttons.Length, Puppet);
            if (_cursor.Selection != deSel) RadialCursor.Sel(_buttons[_cursor.Selection], true);
        }

        private void SetButtons(int len, Action<int, float, float, float> create) => SetButtons(len, Mathf.Rad2Deg, Mathf.PI, create);

        private void SetButtons(int len, float radCurrent, float piCurrent, Action<int, float, float, float> create)
        {
            _radialDescription = null;
            _sliceHolder.Clear();
            _textHolder.Clear();
            AddButtons(len, radCurrent, piCurrent, create);
        }

        private static void AddButtons(int len, float radCurrent, float piCurrent, Action<int, float, float, float> create)
        {
            radCurrent = radCurrent * -piCurrent / len;
            for (var i = 0; i < len; i++)
            {
                create(i, 1f / len, radCurrent, piCurrent);
                radCurrent += 360f / len;
                piCurrent -= Mathf.PI * 2 / len;
            }
        }

        private void HandleExternalInput()
        {
            if (Event.current.type == EventType.Layout) return;
            _cursor.Update(MousePosition(), _buttons, Puppet);
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

            for (var iInt = list.Count; iInt < _menuPath.Count; iInt++) RemoveMenu(list.Count);
            _menuPath[_menuPath.Count - 1].Open();
            return true;
        }

        internal void UpdateRunning()
        {
            foreach (var slice in _buttons)
                if (slice is RadialSliceDynamic dynamic)
                    dynamic.CheckRunningUpdate();
        }
    }
}
#endif