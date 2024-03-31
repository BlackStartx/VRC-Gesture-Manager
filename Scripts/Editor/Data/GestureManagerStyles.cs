using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Data
{
    public static class GestureManagerStyles
    {
        private const string BsxName = "BlackStartx";

        private static GUIStyle _bottomStyle;
        private static GUIStyle _emoteError;
        private static GUIStyle _guiHandTitle;
        private static GUIStyle _guiDebugTitle;
        private static GUIStyle _settingsText;
        private static GUIStyle _middleStyle;
        private static GUIStyle _middleError;
        private static GUIStyle _plusButton;
        private static GUIStyle _header;
        private static GUIStyle _toolHeader;
        private static GUIStyle _toolSubHeader;
        private static GUIStyle _headerButton;
        private static GUIStyle _textError;
        private static GUIStyle _textWarningHeader;
        private static GUIStyle _textWarning;
        private static GUIStyle _titleStyle;

        private static GUIStyle _centered;

        private static Texture _gearTexture;
        private static Texture _backTexture;
        private static Texture _closeTexture;
        private static Texture _plusTextureLgt;
        private static Texture _plusTexturePro;

        internal static GUIStyle TitleStyle => _titleStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10)
        };

        internal static GUIStyle GuiHandTitle => _guiHandTitle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10)
        };

        internal static GUIStyle GuiDebugTitle => _guiDebugTitle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        internal static GUIStyle MiddleStyle => _middleStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(5, 5, 5, 5)
        };

        internal static GUIStyle SettingsText => _settingsText ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(5, 5, 5, 12)
        };

        internal static GUIStyle MiddleError => _middleError ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            active = { textColor = Color.red },
            normal = { textColor = Color.red },
            padding = new RectOffset(5, 5, 5, 5)
        };

        internal static GUIStyle EmoteError => _emoteError ??= new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(5, 5, 5, 5)
        };

        internal static GUIStyle TextError => _textError ??= new GUIStyle(GUI.skin.label)
        {
            active = { textColor = Color.red },
            normal = { textColor = Color.red },
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        };

        internal static GUIStyle TextWarningHeader => _textWarningHeader ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };

        internal static GUIStyle TextWarning => _textWarning ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        internal static GUIStyle Header => _header ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10)
        };

        internal static GUIStyle ToolHeader => _toolHeader ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 5, 5)
        };

        internal static GUIStyle ToolSubHeader => _toolSubHeader ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 5)
        };

        public static GUIStyle HeaderButton => _headerButton ??= new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 10, 12, 0)
        };

        private static GUIStyle BottomStyle => _bottomStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(5, 5, 5, 5)
        };

        internal static GUIStyle Centered => _centered ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        internal static GUIStyle PlusButton => _plusButton ??= new GUIStyle { margin = new RectOffset(0, 20, 3, 3) };

        internal static Texture GearTexture => !_gearTexture ? _gearTexture = EditorGUIUtility.IconContent("d_Settings").image : _gearTexture;
        internal static Texture BackTexture => !_backTexture ? _backTexture = EditorGUIUtility.IconContent("d_tab_prev").image : _backTexture;
        internal static Texture CloseTexture => !_closeTexture ? _closeTexture = EditorGUIUtility.IconContent("d_winBtn_win_close").image : _closeTexture;
        internal static Texture PlusTexture => EditorGUIUtility.isProSkin ? PlusTexturePro : PlusTextureLgt;
        private static Texture PlusTextureLgt => !_plusTextureLgt ? _plusTextureLgt = Resources.Load<Texture>("Gm/BSX_GM_PlusSign") : _plusTextureLgt;
        private static Texture PlusTexturePro => !_plusTexturePro ? _plusTexturePro = Resources.Load<Texture>("Gm/BSX_GM_PlusSign[Pro]") : _plusTexturePro;

        public static class Data
        {
            public static readonly string[] GestureNames =
            {
                "[GESTURE] Idle",
                "[GESTURE] Fist",
                "[GESTURE] Open",
                "[GESTURE] FingerPoint",
                "[GESTURE] Victory",
                "[GESTURE] Rock&Roll",
                "[GESTURE] Gun",
                "[GESTURE] ThumbsUp"
            };

            public static readonly string[] EmoteStandingName =
            {
                "[EMOTE 1] Wave",
                "[EMOTE 2] Clap",
                "[EMOTE 3] Point",
                "[EMOTE 4] Cheer",
                "[EMOTE 5] Dance",
                "[EMOTE 6] BackFlip",
                "[EMOTE 7] Die",
                "[EMOTE 8] Sad"
            };

            public static readonly string[] EmoteSeatedName =
            {
                "[EMOTE 1] Laugh",
                "[EMOTE 2] Point",
                "[EMOTE 3] Raise Hand",
                "[EMOTE 4] Drum",
                "[EMOTE 5] Clap",
                "[EMOTE 6] Angry Fist",
                "[EMOTE 7] Disbelief",
                "[EMOTE 8] Disapprove"
            };
        }

        public static class Animations
        {
            public static class Gesture
            {
                private const string Path = "Gm/Animations/Gesture/";

                private static AnimationClip _fist;
                private static AnimationClip _open;
                private static AnimationClip _point;
                private static AnimationClip _peace;
                private static AnimationClip _rock;
                private static AnimationClip _run;
                private static AnimationClip _thumbsUp;

                public static AnimationClip Fist => _fist ? _fist : _fist = Resources.Load<AnimationClip>(Path + Data.GestureNames[1]);
                public static AnimationClip Open => _open ? _open : _open = Resources.Load<AnimationClip>(Path + Data.GestureNames[2]);
                public static AnimationClip Point => _point ? _point : _point = Resources.Load<AnimationClip>(Path + Data.GestureNames[3]);
                public static AnimationClip Peace => _peace ? _peace : _peace = Resources.Load<AnimationClip>(Path + Data.GestureNames[4]);
                public static AnimationClip Rock => _rock ? _rock : _rock = Resources.Load<AnimationClip>(Path + Data.GestureNames[5]);
                public static AnimationClip Gun => _run ? _run : _run = Resources.Load<AnimationClip>(Path + Data.GestureNames[6]);
                public static AnimationClip ThumbsUp => _thumbsUp ? _thumbsUp : _thumbsUp = Resources.Load<AnimationClip>(Path + Data.GestureNames[7]);
            }

            public static class Emote
            {
                private const string Path = "Gm/Animations/Emote/";

                public static class Standing
                {
                    private static AnimationClip _wave;
                    private static AnimationClip _clap;
                    private static AnimationClip _point;
                    private static AnimationClip _cheer;
                    private static AnimationClip _dance;
                    private static AnimationClip _backFlip;
                    private static AnimationClip _die;
                    private static AnimationClip _sadKick;

                    public static AnimationClip Wave => _wave ? _wave : _wave = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[0]);
                    public static AnimationClip Clap => _clap ? _clap : _clap = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[1]);
                    public static AnimationClip Point => _point ? _point : _point = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[2]);
                    public static AnimationClip Cheer => _cheer ? _cheer : _cheer = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[3]);
                    public static AnimationClip Dance => _dance ? _dance : _dance = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[4]);
                    public static AnimationClip BackFlip => _backFlip ? _backFlip : _backFlip = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[5]);
                    public static AnimationClip Die => _die ? _die : _die = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[6]);
                    public static AnimationClip SadKick => _sadKick ? _sadKick : _sadKick = Resources.Load<AnimationClip>(Path + Data.EmoteStandingName[7]);
                }

                public static class Seated
                {
                    private static AnimationClip _laugh;
                    private static AnimationClip _point;
                    private static AnimationClip _raiseHand;
                    private static AnimationClip _drum;
                    private static AnimationClip _clap;
                    private static AnimationClip _shakeFist;
                    private static AnimationClip _disbelief;
                    private static AnimationClip _disapprove;

                    public static AnimationClip Laugh => _laugh ? _laugh : _laugh = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[0]);
                    public static AnimationClip Point => _point ? _point : _point = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[1]);
                    public static AnimationClip RaiseHand => _raiseHand ? _raiseHand : _raiseHand = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[2]);
                    public static AnimationClip Drum => _drum ? _drum : _drum = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[3]);
                    public static AnimationClip Clap => _clap ? _clap : _clap = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[4]);
                    public static AnimationClip ShakeFist => _shakeFist ? _shakeFist : _shakeFist = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[5]);
                    public static AnimationClip Disbelief => _disbelief ? _disbelief : _disbelief = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[6]);
                    public static AnimationClip Disapprove => _disapprove ? _disapprove : _disapprove = Resources.Load<AnimationClip>(Path + Data.EmoteSeatedName[7]);
                }
            }
        }

        public static void Sign(string category = "Script") => GUILayout.Label($"{category} made by {BsxName}", BottomStyle);
    }
}