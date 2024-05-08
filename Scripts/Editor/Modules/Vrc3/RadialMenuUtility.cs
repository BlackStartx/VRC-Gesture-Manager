#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices;
using BlackStartX.GestureManager.Library;
using BlackStartX.GestureManager.Library.VisualElements;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.ScriptableObjects;
using UIEPosition = UnityEngine.UIElements.Position;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
using DynamicType = BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.RadialSliceDynamic.DynamicType;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class RadialMenuUtility
    {
        public static class Colors
        {
            public static class Default
            {
                public static readonly Color Main = new(0.14f, 0.18f, 0.2f);
                public static readonly Color Border = new(0.1f, 0.35f, 0.38f);
                public static readonly Color Selected = new(0.07f, 0.55f, 0.58f);
            }

            public static readonly Color CenterSelected = new(0.06f, 0.2f, 0.22f);
            public static readonly Color RadialInner = new(0.21f, 0.24f, 0.27f);
            public static readonly Color CenterIdle = new(0.06f, 0.27f, 0.29f);

            private const string MainKeyName = "GM3 Main Color";
            private const string BorderKeyName = "GM3 Border Color";
            private const string SelectedKeyName = "GM3 Selected Color";

            private static Color PrefColor(string name, Color defaultColor) => ColorUtility.TryParseHtmlString(EditorPrefs.GetString(name), out var color) ? color : defaultColor;

            internal static readonly Color RadialTextBackground = new(0.11f, 0.11f, 0.11f, 0.49f);
            internal static readonly Color RestartButton = new(1f, 0.72f, 0.41f);
            internal static readonly Color SubIcon = new(0.22f, 0.24f, 0.27f);

            internal static Color Cursor => new(CustomMain.r, CustomMain.g, CustomMain.b, CustomMain.a - 0.3f);
            internal static Color CursorBorder => new(CustomBorder.r, CustomBorder.g, CustomBorder.b, CustomBorder.a - 0.5f);
            internal static Color ProgressBorder => CustomSelected * 1.5f;

            public static Color CustomMain = PrefColor(MainKeyName, Default.Main);
            public static Color CustomBorder = PrefColor(BorderKeyName, Default.Border);
            public static Color CustomSelected = PrefColor(SelectedKeyName, Default.Selected);

            public static void SaveColors(Color main, Color border, Color selected)
            {
                CustomMain = main;
                CustomBorder = border;
                CustomSelected = selected;
                EditorPrefs.SetString(MainKeyName, $"#{ColorUtility.ToHtmlStringRGBA(main)}");
                EditorPrefs.SetString(BorderKeyName, $"#{ColorUtility.ToHtmlStringRGBA(border)}");
                EditorPrefs.SetString(SelectedKeyName, $"#{ColorUtility.ToHtmlStringRGBA(selected)}");
            }
        }

        public static class Prefabs
        {
            [PublicAPI]
            public static VisualElement NewBorder(float size)
            {
                return new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = size - 2,
                        height = 2,
                        transformOrigin = new StyleTransformOrigin(),
                        backgroundColor = Colors.CustomBorder,
                        position = UIEPosition.Absolute
                    }
                };
            }

            internal static VisualElement NewBorder(float size, float euler)
            {
                var element = NewBorder(size);
                element.transform.rotation = Quaternion.Euler(0, 0, euler);
                return element;
            }

            internal static VisualElement NewCircleBorder(float size, Color border, UIEPosition position = default) => SetCircleBorder(new VisualElement(), size, border, position);

            private static VisualElement SetCircleBorder(VisualElement element, float size, Color border, UIEPosition position = default)
            {
                element.MyBorder(2, size, border);
                element.pickingMode = PickingMode.Ignore;
                element.style.position = position;
                element.style.alignItems = Align.Center;
                element.style.width = element.style.height = size;
                element.style.justifyContent = Justify.Center;
                return element;
            }

            internal static VisualElement NewIconText(float x, float y, float size, Texture2D icon, string text) => NewIconText(x, y, size, size / 2, icon, text);

            private static VisualElement NewIconText(float x, float y, float size, float hSize, Texture2D icon, string text)
            {
                return new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = size,
                        height = size,
                        left = x - hSize,
                        top = y - hSize,
                        backgroundImage = icon,
                        position = UIEPosition.Absolute
                    }
                }.With(new TextElement
                {
                    text = text,
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        color = Color.white,
                        fontSize = 8,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        top = size
                    }
                });
            }

            internal static VisualElement NewData(float width, float height)
            {
                return new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = width,
                        height = height,
                        left = -width / 2,
                        top = -height / 2,
                        backgroundColor = Color.clear,
                        position = UIEPosition.Absolute,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center
                    }
                };
            }

            internal static void SetSlice(GmgCircleElement element, float size, Color centerColor, Color color, Color border, UIEPosition position = UIEPosition.Absolute)
            {
                element.BorderWidth = 2;
                element.VertexColor = color;
                element.BorderColor = border;
                element.CenterColor = centerColor;
                element.pickingMode = PickingMode.Ignore;
                element.style.position = position;
                element.style.alignItems = Align.Center;
                element.style.width = element.style.height = size;
                element.style.justifyContent = Justify.Center;
                element.style.left = element.style.top = -size / 2;
            }

            [PublicAPI]
            public static GmgCircleElement NewCircle(float size, Color color, Color border, UIEPosition position = default) => SetCircle(new GmgCircleElement(), size, color, color, border, position);

            [PublicAPI]
            public static GmgCircleElement NewCircle(float size, Color centerColor, Color color, Color border, UIEPosition position = default) => SetCircle(new GmgCircleElement(), size, centerColor, color, border, position);

            internal static GmgCircleElement SetCircle(GmgCircleElement element, float size, Color color, Color border, UIEPosition position = default) => SetCircle(element, size, color, color, border, position);

            private static GmgCircleElement SetCircle(GmgCircleElement element, float size, Color centerColor, Color color, Color border, UIEPosition position = default)
            {
                element.BorderWidth = 2;
                element.VertexColor = color;
                element.BorderColor = border;
                element.CenterColor = centerColor;
                element.pickingMode = PickingMode.Ignore;
                element.style.position = position;
                element.style.alignItems = Align.Center;
                element.style.width = element.style.height = size;
                element.style.justifyContent = Justify.Center;
                return element;
            }

            internal static VisualElement NewRadialText(out TextElement text, int top, UIEPosition position = default) => SetRadialText(new VisualElement(), out text, top, position);

            internal static VisualElement SetRadialText(VisualElement element, out TextElement text, int top, UIEPosition position = default)
            {
                element.style.width = 50;
                element.style.height = 20;
                element.pickingMode = PickingMode.Ignore;
                element.style.backgroundColor = Colors.RadialTextBackground;
                element.style.borderTopLeftRadius = 10;
                element.style.borderTopRightRadius = 10;
                element.style.borderBottomLeftRadius = 10;
                element.style.borderBottomRightRadius = 10;
                element.style.position = position;
                if (top != 0) element.style.top = top;
                element.Add(text = new TextElement { pickingMode = PickingMode.Ignore, style = { height = 20, unityTextAlign = TextAnchor.MiddleCenter, color = Color.white, fontSize = 14 } });
                return element;
            }

            internal static VisualElement NewSubIcon(Texture2D texture)
            {
                return new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        top = 43,
                        left = 60,
                        width = 25,
                        height = 25,
                        borderTopLeftRadius = 15,
                        borderTopRightRadius = 15,
                        borderBottomLeftRadius = 15,
                        borderBottomRightRadius = 15,
                        backgroundColor = Colors.SubIcon,
                        position = UIEPosition.Absolute,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center
                    }
                }.With(new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
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
            public static RadialSliceControl ToggleFromParam(RadialMenu menu, string name, Vrc3Param param, Texture2D icon = null, float activeValue = 1f)
            {
                return new RadialSliceControl(menu, name, icon, ControlType.Toggle, activeValue, param, Array.Empty<Vrc3Param>(), null, null);
            }

            public static RadialSliceBase RadialFromParam(RadialMenu menu, string name, Vrc3Param param, Texture2D icon = null, float amplify = 1f, RadialSliceControl.RadialSettings settings = null)
            {
                return new RadialSliceControl(menu, name, icon, ControlType.RadialPuppet, 1f, null, new[] { param }, null, null, amplify, settings);
            }

            public static RadialSliceControl AxisFromParams(RadialMenu menu, string name, Vrc3Param xParam, Vrc3Param yParam, Texture2D icon = null, float amplify = 1f)
            {
                var subLabels = new VRCExpressionsMenu.Control.Label[4];
                return new RadialSliceControl(menu, name, icon, ControlType.TwoAxisPuppet, 1f, null, new[] { xParam, yParam }, null, subLabels, amplify);
            }
        }

        public static Texture2D GetSubIcon(ControlType type) => type switch
        {
            ControlType.Button => null,
            ControlType.Toggle => ModuleVrc3Styles.Toggle,
            ControlType.SubMenu => ModuleVrc3Styles.Option,
            ControlType.TwoAxisPuppet => ModuleVrc3Styles.TwoAxis,
            ControlType.FourAxisPuppet => ModuleVrc3Styles.FourAxis,
            ControlType.RadialPuppet => ModuleVrc3Styles.Radial,
            _ => null
        };

        public static DynamicType GetDynamicType(ControlType type) => type switch
        {
            ControlType.Button => DynamicType.Running,
            ControlType.Toggle => DynamicType.Running,
            ControlType.RadialPuppet => DynamicType.Radial,
            ControlType.SubMenu => DynamicType.None,
            ControlType.TwoAxisPuppet => DynamicType.None,
            ControlType.FourAxisPuppet => DynamicType.None,
            _ => DynamicType.None
        };

        public static IEnumerable<(int, AnimatorControllerParameter)> GetParameters(AnimatorControllerPlayable playable)
        {
            if (playable.IsNull()) yield break;
            for (var i = 0; i < playable.GetParameterCount(); i++)
                yield return (i, playable.GetParameter(i));
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
            return from paramString in GetParams(GetMenus(menu))
                let isDefined = string.IsNullOrEmpty(paramString) || param.FindParameter(paramString) != null
                where !isDefined
                select $"Menu uses a parameter that is not defined: {paramString}";
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static bool Is(float floatValue, float value) => floatValue == value;
    }
}
#endif