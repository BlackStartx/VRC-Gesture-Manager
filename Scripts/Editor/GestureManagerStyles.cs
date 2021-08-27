using UnityEngine;

namespace GestureManager.Scripts.Editor
{
    public static class GestureManagerStyles
    {
        private static GUIStyle _bottomStyle;
        private static GUIStyle _emoteError;
        private static GUIStyle _guiGreenButton;
        private static GUIStyle _guiHandTitle;
        private static GUIStyle _middleStyle;
        private static GUIStyle _plusButton;
        private static GUIStyle _subHeader;
        private static GUIStyle _textError;
        private static GUIStyle _textWarning;
        private static GUIStyle _titleStyle;

        private static Texture _plusTexture;
        private static Texture _plusTexturePro;

        internal static GUIStyle TitleStyle => _titleStyle ?? (_titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            padding = new RectOffset(10, 10, 10, 10)
        });

        internal static GUIStyle GuiHandTitle => _guiHandTitle ?? (_guiHandTitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            padding = new RectOffset(10, 10, 10, 10)
        });

        internal static GUIStyle BottomStyle => _bottomStyle ?? (_bottomStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            padding = new RectOffset(5, 5, 5, 5)
        });

        internal static GUIStyle MiddleStyle => _middleStyle ?? (_middleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            padding = new RectOffset(5, 5, 5, 5)
        });

        internal static GUIStyle EmoteError => _emoteError ?? (_emoteError = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(5, 5, 5, 5)
        });

        internal static GUIStyle TextWarning => _textWarning ?? (_textWarning = new GUIStyle(GUI.skin.label)
        {
            active = {textColor = Color.yellow},
            normal = {textColor = Color.yellow},
            alignment = TextAnchor.MiddleCenter
        });

        internal static GUIStyle TextError => _textError ?? (_textError = new GUIStyle(GUI.skin.label)
        {
            active = {textColor = Color.red},
            normal = {textColor = Color.red},
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        });

        internal static GUIStyle GuiGreenButton => _guiGreenButton ?? (_guiGreenButton = new GUIStyle(GUI.skin.button)
        {
            active = {textColor = Color.green},
            normal = {textColor = Color.green},
            hover = {textColor = Color.green},
            fixedWidth = 100
        });

        internal static GUIStyle SubHeader => _subHeader ?? (_subHeader = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});
        internal static GUIStyle PlusButton => _plusButton ?? (_plusButton = new GUIStyle {margin = new RectOffset(0, 20, 3, 3)});

        internal static Texture PlusTexture => _plusTexture ? _plusTexture : _plusTexture = Resources.Load<Texture>("Gm/BSX_GM_PlusSign");
        internal static Texture PlusTexturePro => _plusTexturePro ? _plusTexturePro : _plusTexturePro = Resources.Load<Texture>("Gm/BSX_GM_PlusSign[Pro]");

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
    }
}