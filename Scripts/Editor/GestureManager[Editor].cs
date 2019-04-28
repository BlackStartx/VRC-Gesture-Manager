using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GestureManager))]
public class GestureManagerEditor : Editor
{
    private GUIStyle titleStyle;
    private GUIStyle middleStyle;
    private GUIStyle bottomStyle;
    private GUIStyle guiGreenButton;
    private GUIStyle emoteError;

    private AnimationClip selectingCustomAnim;

    private GestureManager manager;
    private static int tab = 0;

    public delegate int OnNoneSelected(int lastPosition);

    public override void OnInspectorGUI()
    {
        Init();

        GUILayout.Label("Gesture Manager", titleStyle);

        if (GetManager().avatar != null)
        {

            GestureManager.ControllerType usingType = GetManager().GetUsedType();
            GestureManager.ControllerType notUsingType = GetManager().GetNotUsedType();

            GUILayout.Label("Current Avatar: " + GetManager().avatar.name);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Using Override: " + GetManager().GetOverrideController().name + " [" + usingType.ToString() + "]");
            if (GUILayout.Button("Switch to " + notUsingType.ToString().ToLower() + "!"))
            {
                GetManager().SwitchType();
            }

            GUILayout.EndHorizontal();

            tab = GUILayout.Toolbar(tab, new string[] { "Gestures", "Emotes", "Test animation" });
            switch (tab)
            {
                case 0:
                {
                    if (manager.emote != 0 || manager.onCustomAnimation)
                    {
                        GUILayout.BeginHorizontal(emoteError);
                        GUILayout.Label("Gesture doesn't work while you're playing an emote!");
                        if (GUILayout.Button("Stop!", guiGreenButton))
                        {
                            GetManager().StopCurrentEmote();
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    
                    GUILayout.BeginVertical();
                    GUILayout.Label("Left Hand");
                    GetManager().left = OnCheckBoxGUIHand(GetManager().left, position => { return 0; });
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Right Hand");
                    GetManager().right = OnCheckBoxGUIHand(GetManager().right, position => { return 0; });
                    GUILayout.EndVertical();
                    
                    GUILayout.EndHorizontal();

                    break;
                }
                case 1:
                {
                    GUILayout.Label("Emotes");

                    OnEmoteButton(1);
                    OnEmoteButton(2);
                    OnEmoteButton(3);
                    OnEmoteButton(4);
                    OnEmoteButton(5);
                    OnEmoteButton(6);
                    OnEmoteButton(7);
                    OnEmoteButton(8);

                    break;
                }
                case 2:
                {
                    GUILayout.Label("Force animation.");

                    GUILayout.BeginHorizontal();
                    AnimationClip lastAnim = selectingCustomAnim;
                    selectingCustomAnim = (AnimationClip) EditorGUILayout.ObjectField("Animation: ", selectingCustomAnim, typeof(AnimationClip), true, null);
                    if (selectingCustomAnim != lastAnim)
                    { 
                        GetManager().SetCustomAnimation(selectingCustomAnim);
                    }
                    if (manager.onCustomAnimation)
                    {
                        if (GUILayout.Button("Stop", guiGreenButton))
                        {
                            GetManager().OnCustomEmoteStop();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Play", GUILayout.Width(100)))
                        {
                            GetManager().StopCurrentEmote();
                            GetManager().SetCustomAnimation(selectingCustomAnim);
                            GetManager().OnCustomEmoteStart();
                        }
                    }
                    GUILayout.EndHorizontal();

                        break;
                }
                default:
                {
                    break;
                }
            }
        }
        else
        {
            if(EditorApplication.isPlaying)
                GUILayout.Label("No avatars.");
            else
            {
                GUILayout.Label("I'm an useless script if you aren't on play mode :D", middleStyle);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check For Updates", GUILayout.Width(130)))
                {
                    GetManager().CheckForUpdates();
                }
                GUILayout.Space(20);
                if (GUILayout.Button("Donate", GUILayout.Width(130)))
                {
                    Application.OpenURL("https://www.paypal.me/blackstartx");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.Label("Script made by BlackStartx", bottomStyle);
    }

    private void Init()
    {
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.UpperCenter;
        titleStyle.padding = new RectOffset(10, 10, 10, 10);
        
        bottomStyle = new GUIStyle(GUI.skin.label);
        bottomStyle.fontSize = 11;
        bottomStyle.fontStyle = FontStyle.Bold;
        bottomStyle.alignment = TextAnchor.UpperRight;
        bottomStyle.padding = new RectOffset(5, 5, 5, 5);

        middleStyle = new GUIStyle(GUI.skin.label);
        middleStyle.fontSize = 12;
        middleStyle.fontStyle = FontStyle.Bold;
        middleStyle.alignment = TextAnchor.UpperCenter;
        middleStyle.padding = new RectOffset(5, 5, 5, 5);

        emoteError = new GUIStyle(GUI.skin.box);
        emoteError.padding = new RectOffset(5, 5, 5, 5);
        emoteError.margin = new RectOffset(5, 5, 5, 5);

        guiGreenButton = new GUIStyle(GUI.skin.button);
        guiGreenButton.normal.textColor = Color.green;

        guiGreenButton.fixedWidth = 100;
    }

    private int OnCheckBoxGUIHand(int position, OnNoneSelected onNone)
    {

        bool[] gesture = new bool[] { false, false, false, false, false, false, false, false };

        gesture[position] = true;
        
        gesture[1] = EditorGUILayout.Toggle(manager.GetGestureName(1), gesture[1]);
        gesture[2] = EditorGUILayout.Toggle(manager.GetGestureName(2), gesture[2]);
        gesture[3] = EditorGUILayout.Toggle(manager.GetGestureName(3), gesture[3]);
        gesture[4] = EditorGUILayout.Toggle(manager.GetGestureName(4), gesture[4]);
        gesture[5] = EditorGUILayout.Toggle(manager.GetGestureName(5), gesture[5]);
        gesture[6] = EditorGUILayout.Toggle(manager.GetGestureName(6), gesture[6]);
        gesture[7] = EditorGUILayout.Toggle(manager.GetGestureName(7), gesture[7]);

        for (int i = 0; i < gesture.Length; i++)
        {
            if (gesture[i] && position != i)
            {
                for (int ix = 0; ix < gesture.Length; ix++)
                {
                    if (ix == i)
                        continue;
                    gesture[ix] = false;
                }
            }
        }

        for (int i = 0; i < gesture.Length; i++) 
        {
            if (gesture[i])
                return i;
        }

        return onNone(position);
    }

    private void OnEmoteButton(int emote)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(manager.GetEmoteName(emote - 1));
        if (manager.emote == emote)
        {
            if (GUILayout.Button("Stop", guiGreenButton))
            {
                GetManager().OnEmoteStop();
            }
        }
        else
        {
            if (GUILayout.Button("Play", GUILayout.Width(100)))
            {
                GetManager().StopCurrentEmote();
                GetManager().OnEmoteStart(emote);
            }
        }
        GUILayout.EndHorizontal();
    }

    private GestureManager GetManager()
    {
        if (manager == null)
            manager = (GestureManager) target;
        return manager;
    }
}
