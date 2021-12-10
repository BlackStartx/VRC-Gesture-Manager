#if VRC_SDK_VRCSDK3
using System;
using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialDescription
    {
        private readonly Action<string> _action;

        private readonly string _text;
        private readonly string _link;
        private readonly string _url;
        private readonly string _tail;

        public RadialDescription(string text, string link, string tail, Action<string> action, string url)
        {
            _text = text;
            _link = link;
            _action = action;
            _url = url;
            _tail = tail;
        }

        public void Show()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GestureManagerStyles.EmoteError);
            GUILayout.FlexibleSpace();
            GUILayout.Label(_text);

            var guiStyle = EditorGUIUtility.isProSkin ? ModuleVrc3Styles.UrlPro : ModuleVrc3Styles.Url;
            if (GUILayout.Button(_link, guiStyle)) _action(_url);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.Label(_tail);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
    }
}
#endif