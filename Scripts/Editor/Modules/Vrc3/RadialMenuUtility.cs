#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules.Vrc3.Params;
using GestureManager.Scripts.Editor.Modules.Vrc3.RadialButtons;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.ScriptableObjects;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public static class RadialMenuUtility
    {
        public static class Colors
        {
            internal static readonly Color RadialTextBackground = new Color(0.11f, 0.11f, 0.11f, 0.49f);

            internal static readonly Color OuterBorder = new Color(0.1f, 0.35f, 0.38f);
            internal static readonly Color SubIcon = new Color(0.22f, 0.24f, 0.27f);

            internal static readonly Color Middle = new Color(0.14f, 0.18f, 0.2f);
            internal static readonly Color Inner = new Color(0.21f, 0.24f, 0.27f);

            internal static readonly Color CursorBorder = new Color(0.1f, 0.35f, 0.38f);
            internal static Color Cursor => Middle;
        }

        public static class Prefabs
        {
            internal static VisualElement NewBorder(float size)
            {
                return new VisualElement
                {
                    style =
                    {
                        width = size,
                        height = 2,
                        backgroundColor = Colors.OuterBorder,
                        positionType = PositionType.Absolute
                    }
                };
            }

            internal static VisualElement NewBorder(float size, float euler)
            {
                var element = NewBorder(size);
                element.transform.rotation = Quaternion.Euler(0, 0, euler);
                return element;
            }

            internal static VisualElement NewIconText(float x, float y, float size, Texture2D icon, string text) => NewIconText(x, y, size, size / 2, icon, text);

            private static VisualElement NewIconText(float x, float y, float size, float hSize, Texture2D icon, string text)
            {
                return new VisualElement
                {
                    style =
                    {
                        width = size,
                        height = size,
                        positionLeft = x - hSize,
                        positionTop = y - hSize,
                        backgroundImage = icon,
                        positionType = PositionType.Absolute
                    }
                }.With(new TextElement
                {
                    text = text,
                    style =
                    {
                        color = Color.white,
                        fontSize = 8,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        positionTop = size
                    }
                });
            }

            internal static VisualElement NewData()
            {
                return new VisualElement
                {
                    style =
                    {
                        width = 1,
                        height = 1,
                        backgroundColor = Color.clear,
                        positionType = PositionType.Absolute,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center
                    }
                };
            }

            internal static VisualElement NewCircle(float size, Color color, Color border, PositionType positionType = default)
            {
                return SetCircle(new VisualElement(), size, color, border, positionType);
            }

            internal static VisualElement SetCircle(VisualElement element, float size, Color color, Color border, PositionType positionType = default)
            {
                element.style.width = element.style.height = size;

                element.style.backgroundColor = color;

                element.style.borderColor = border;
                element.style.borderRadius = size;
                element.style.borderBottomWidth = element.style.borderLeftWidth = element.style.borderRightWidth = element.style.borderTopWidth = 2;

                element.style.alignItems = Align.Center;
                element.style.justifyContent = Justify.Center;
                element.style.positionType = positionType;
                return element;
            }

            internal static VisualElement NewRadialText(out TextElement text, int positionTop, PositionType positionType = default)
            {
                return SetRadialText(new VisualElement(), out text, positionTop, positionType);
            }

            internal static VisualElement SetRadialText(VisualElement element, out TextElement text, int positionTop, PositionType positionType = default)
            {
                element.style.width = 50;
                element.style.height = 20;
                element.style.backgroundColor = Colors.RadialTextBackground;
                element.style.borderColor = Color.clear;
                element.style.borderRadius = 10;
                element.style.positionType = positionType;
                if (positionTop != 0) element.style.positionTop = positionTop;
                text = element.MyAdd(new TextElement {style = {height = 20, unityTextAlign = TextAnchor.MiddleCenter, color = Color.white, fontSize = 14}});
                return element;
            }

            internal static VisualElement NewSubIcon(Texture2D texture)
            {
                return new VisualElement
                {
                    style =
                    {
                        width = 25,
                        height = 25,
                        borderRadius = 15,
                        positionLeft = 10,
                        positionTop = -2,
                        backgroundColor = Colors.SubIcon,
                        positionType = PositionType.Absolute,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center
                    }
                }.With(new VisualElement
                {
                    style =
                    {
                        width = 20,
                        height = 20,
                        backgroundImage = texture
                    }
                });
            }
        }

        public static class Buttons
        {
            public static RadialMenuControl ToggleFromParam(RadialMenu menu, string name, Vrc3Param param)
            {
                return ParamStateToggle(menu, name, param, 1f);
            }

            public static RadialMenuControl ParamStateToggle(RadialMenu menu, string name, Vrc3Param param, float activeValue)
            {
                return new RadialMenuControl(menu, name, null, ControlType.Toggle, activeValue, param, new Vrc3Param[0], null, null);
            }

            public static RadialMenuControl AxisFromParams(RadialMenu menu, string name, Vrc3Param xParam, Vrc3Param yParam)
            {
                var subLabels = new VRCExpressionsMenu.Control.Label[4];
                return new RadialMenuControl(menu, name, null, ControlType.TwoAxisPuppet, 1f, null, new[] {xParam, yParam}, null, subLabels);
            }

            public static RadialMenuItem RadialFromParam(RadialMenu menu, string name, Vrc3Param param)
            {
                return new RadialMenuControl(menu, name, null, ControlType.RadialPuppet, 1f, null, new[] {param}, null, null);
            }
        }

        public static Texture2D GetSubIcon(ControlType type)
        {
            switch (type)
            {
                case ControlType.Button:
                    return null;
                case ControlType.Toggle:
                    return ModuleVrc3Styles.Toggle;
                case ControlType.SubMenu:
                    return ModuleVrc3Styles.Option;
                case ControlType.TwoAxisPuppet:
                    return ModuleVrc3Styles.TwoAxis;
                case ControlType.FourAxisPuppet:
                    return ModuleVrc3Styles.FourAxis;
                case ControlType.RadialPuppet:
                    return ModuleVrc3Styles.Radial;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static IEnumerable<AnimatorControllerParameter> GetParameters(AnimatorControllerPlayable playable)
        {
            if (playable.IsNull()) yield break;
            for (var i = 0; i < playable.GetParameterCount(); i++)
                yield return playable.GetParameter(i);
        }

        public static Vrc3ParamController CreateParamFromController(Animator animator, AnimatorControllerParameter parameter, AnimatorControllerPlayable controller)
        {
            return new Vrc3ParamController(parameter.type, parameter.name, controller, animator);
        }

        public static Vrc3Param CreateParamFromNothing(VRCExpressionParameters.Parameter parameter)
        {
            return new Vrc3ParamExternal(parameter.name, ModuleVrc3Styles.Data.TypeOf[parameter.valueType]);
        }

        public static int RadialPercentage(float value)
        {
            return RadialPercentage(value, out _);
        }

        public static int RadialPercentage(float value, out float clamp)
        {
            clamp = Mathf.Clamp(value, 0f, 1f);
            return (int) (clamp * 100);
        }

        private static void AppendMenus(VRCExpressionsMenu menu, ICollection<VRCExpressionsMenu> menus)
        {
            if (!menu || menus.Contains(menu)) return;
            menus.Add(menu);
            foreach (var control in menu.controls) AppendMenus(control.subMenu, menus);
        }

        private static IEnumerable<VRCExpressionsMenu> GetMenus(VRCExpressionsMenu menu)
        {
            var menus = new List<VRCExpressionsMenu>();
            AppendMenus(menu, menus);
            return menus;
        }

        private static IEnumerable<string> GetParams(IEnumerable<VRCExpressionsMenu> menus)
        {
            var paramList = new HashSet<string>();
            foreach (var menu in menus)
            foreach (var control in menu.controls)
            {
                paramList.Add(control.name);
                if (control.subParameters == null) continue;
                foreach (var subParameter in control.subParameters)
                    paramList.Add(subParameter.name);
            }

            return paramList;
        }

        public static IEnumerable<string> CheckErrors(VRCExpressionsMenu menu, VRCExpressionParameters param)
        {
            var errors = new List<string>();
            if (!menu && !param) return errors;
            if (!menu || !param) errors.Add(!menu ? "- No menu defined when parameters are defined!" : "- No parameters defined when menu is defined!");
            return errors;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static IEnumerable<string> CheckWarnings(VRCExpressionsMenu menu, VRCExpressionParameters param)
        {
            return from paramName in GetParams(GetMenus(menu))
                let isDefined = string.IsNullOrEmpty(paramName) || param.FindParameter(paramName) != null
                where !isDefined
                select "Menu uses a parameter that is not defined: " + paramName;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static bool Is(float get, float value)
        {
            return get == value;
        }
    }
}
#endif