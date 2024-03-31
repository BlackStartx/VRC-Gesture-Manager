#if VRC_SDK_VRCSDK3
using System;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class RadialDescription
    {
        private readonly Action<string> _action;

        private readonly string _text;
        private readonly string _link;
        private readonly string _tail;
        private readonly string _url;

        public RadialDescription(string text, string link, string tail, Action<string> action, string url = null)
        {
            _action = action;
            _text = text;
            _link = link;
            _tail = tail;
            _url = url;
        }

        public void Show()
        {
            using (new GUILayout.HorizontalScope(GestureManagerStyles.EmoteError))
            using (new GmgLayoutHelper.FlexibleScope())
            {
                GUILayout.Label(_text);

                var guiStyle = EditorGUIUtility.isProSkin ? ModuleVrc3Styles.UrlPro : ModuleVrc3Styles.Url;
                if (GUILayout.Button(_link, guiStyle)) _action(_url);
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                GUILayout.Label(_tail);
            }
        }
    }
}
#endif