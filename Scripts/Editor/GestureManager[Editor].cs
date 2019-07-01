using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

[CustomEditor(typeof(GestureManager))]
public class GestureManagerEditor : Editor
{
    private GUIStyle titleStyle;
    private GUIStyle middleStyle;
    private GUIStyle bottomStyle;
    private GUIStyle guiGreenButton;
    private GUIStyle guiHandTitle;
    private GUIStyle emoteError;
    private GUIStyle textError;
    private GUIStyle subHeader;

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

            if (!GetManager().avatar.activeInHierarchy)
            {
                GetManager().UnlinkFromAvatar();
            }

            GestureManager.ControllerType usingType = GetManager().GetUsedType();
            GestureManager.ControllerType notUsingType = GetManager().GetNotUsedType();

            MyLayoutHelper.ObjectField("Controlling Avatar: ", GetManager().avatar, newObject =>
            {
                if (newObject != null)
                {
                    VRC_AvatarDescriptor descriptor = newObject.GetComponent<VRC_AvatarDescriptor>();
                    if (GetManager().IsValidDesc(descriptor)) 
                    {
                        GetManager().UnlinkFromAvatar();
                        GetManager().InitForAvatar(newObject.GetComponent<VRC_AvatarDescriptor>());
                    }
                }
                else
                {
                    GetManager().UnlinkFromAvatar();
                }
            });

            if(GetManager().avatar == null)
                return;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Using Override: " + GetManager().GetOverrideController().name + " [" + usingType.ToString() + "]");
            GUI.enabled = GetManager().CanSwitchController();
            if (GUILayout.Button("Switch to " + notUsingType.ToString().ToLower() + "!"))
            {
                GetManager().SwitchType();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            tab = GUILayout.Toolbar(tab, new string[] { "Gestures", "Emotes", "Idles", "Test animation" });
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
                    GUILayout.Label("Left Hand", guiHandTitle);
                    GetManager().left = OnCheckBoxGUIHand(GetManager().left, position => { return 0; });
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Right Hand", guiHandTitle);
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
                    GUILayout.Label("Avatar Idles.");

                    

                    break;
                }
                case 3:
                {
                    GUILayout.Label("Force animation.");

                    GUILayout.BeginHorizontal();
                    AnimationClip lastAnim = selectingCustomAnim;
                    selectingCustomAnim = (AnimationClip)EditorGUILayout.ObjectField("Animation: ", selectingCustomAnim, typeof(AnimationClip), true, null);
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
            if (EditorApplication.isPlaying)
            {
                if (this.GetManager().gameObject.activeInHierarchy)
                {
                    if (GetManager().GetLastCheckedActiveDescriptors().Length == 0)
                    {
                        GUILayout.Label("There are no VRC_AvatarDescriptor on your scene. \nPlease consider adding one to your avatar before entering in PlayMode.", textError);

                        if (GUILayout.Button("Check Again"))
                        {
                            GetManager().CheckActiveDescriptors();
                        }

                    }
                    else
                    {
                        List<VRC_AvatarDescriptor> elegible = new List<VRC_AvatarDescriptor>();
                        List<VRC_AvatarDescriptor> unelegible = new List<VRC_AvatarDescriptor>();

                        foreach (VRC_AvatarDescriptor descriptor in GetManager().GetLastCheckedActiveDescriptors())
                        {
                            if(GetManager().IsValidDesc(descriptor))
                                elegible.Add(descriptor);
                            else
                                unelegible.Add(descriptor);
                        }

                        if(elegible.Count == 0)
                            GUILayout.Label("No one of your VRC_AvatarDescriptor are elegible.", subHeader);
                        else
                            GUILayout.Label("Elegible VRC_AvatarDescriptors:", subHeader); 

                        foreach (VRC_AvatarDescriptor descriptor in elegible)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(descriptor.gameObject.name + ":"); 

                            if (GUILayout.Button("Set As Avatar", GUILayout.Width(200)))
                            {
                                GetManager().InitForAvatar(descriptor);
                            }

                            GUILayout.EndHorizontal();
                        }

                        if (elegible.Count != 0 && unelegible.Count != 0)
                            GUILayout.Label("Non-Elegible VRC_AvatarDescriptors:", subHeader);

                        foreach (VRC_AvatarDescriptor descriptor in unelegible)
                        {
                            if (descriptor != null)
                            {
                                GUILayout.Label(descriptor.gameObject.name + ":");
                                GUILayout.BeginVertical(emoteError);

                                if (!descriptor.gameObject.activeInHierarchy)
                                    GUILayout.Label("The GameObject is disabled!", textError);
                                if (descriptor.CustomSittingAnims == null && descriptor.CustomStandingAnims == null)
                                    GUILayout.Label("The Descriptor doesn't have any kind of controller!", textError);
                                if (descriptor.gameObject.GetComponent<Animator>() == null)
                                    GUILayout.Label("The model doesn't have any animator!", textError);
                                else if (!descriptor.gameObject.GetComponent<Animator>().isHuman)
                                    GUILayout.Label("The avatar is not imported as a humanoid rig!", textError);

                                GUILayout.EndVertical();
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label("I'm disabled!", textError);
                }
            }
            else
            {
                GUILayout.Label("I'm an useless script if you aren't on play mode :D", middleStyle);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check For Updates", GUILayout.Width(130)))
                {
                    GetManager().CheckForUpdates((error) =>
                    {
                        EditorUtility.DisplayDialog("Gesture Manager Updater", "Error :c (" + error.responseCode + ")", "Okay");
                    }, response =>
                    {
                        string[] infos = response.downloadHandler.text.Trim().Split('\n');
                        string lastVersion = infos[0];
                        string download = infos[1];
                        if (GetManager().GetCurrentVersion().Equals(lastVersion))
                        {
                            EditorUtility.DisplayDialog("Gesture Manager Updater", "You have the latest version of the manager. (" + lastVersion + ")", "Okay");
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Gesture Manager Updater", "Newer version aviable! (" + lastVersion + ")", "Download", "Cancel"))
                            {
                                Application.OpenURL(download);
                            }
                        }
                    });
                }
                GUILayout.Space(20);
                if (GUILayout.Button("My Discord Name", GUILayout.Width(130)))
                {
                    string me = "BlackStartx#6593";
                    if (EditorUtility.DisplayDialog("It's me!", me, "Copy To Clipboard!", "Ok!"))
                    {
                        CopyToClipboard(me);
                    }
                }


                /**
                 * Dunno if i will insert a donation button directly on the script... 
                 *
                GUILayout.Space(20);
                if (GUILayout.Button("Donate", GUILayout.Width(130)))
                {
                    Application.OpenURL("https://www.paypal.me/blackstartx");
                }
                */


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

        guiHandTitle = new GUIStyle(GUI.skin.label);
        guiHandTitle.fontSize = 12;
        guiHandTitle.fontStyle = FontStyle.Bold;
        guiHandTitle.alignment = TextAnchor.UpperCenter;
        guiHandTitle.padding = new RectOffset(10, 10, 10, 10);
        
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

        textError = new GUIStyle(GUI.skin.label);
        textError.active.textColor = Color.red;
        textError.normal.textColor = Color.red;
        textError.fontSize = 13;
        textError.alignment = TextAnchor.MiddleCenter;

        guiGreenButton = new GUIStyle(GUI.skin.button);
        guiGreenButton.normal.textColor = Color.green;

        subHeader = new GUIStyle(GUI.skin.label);
        subHeader.alignment = TextAnchor.MiddleCenter;

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

    /**
     * Utils
     */

    public static void CopyToClipboard(string toClipboard)
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = toClipboard;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    static class MyLayoutHelper
    {
        public static T ObjectField<T>(string label, T unityObject) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(label, unityObject, typeof(T), true, null);
        }

        public static T ObjectField<T>(string label, T unityObject, OnObjectSet<T> onObjectSet) where T : UnityEngine.Object
        {
            return ObjectField(label, unityObject, onObjectSet, (oldObject, newObject) => { onObjectSet(newObject); }, oldObject => { onObjectSet(null); });
        }

        public static T ObjectField<T>(string label, T unityObject, OnObjectSet<T> onObjectSet, OnObjectChange<T> onObjectChange, OnObjectRemove<T> onObjectRemove) where T : UnityEngine.Object
        {
            T oldObject = unityObject;

            unityObject = (T)EditorGUILayout.ObjectField(label, unityObject, typeof(T), true, null);
            if (oldObject != unityObject)
            {
                if (oldObject == null)
                    onObjectSet(unityObject);
                else if (unityObject == null)
                    onObjectRemove(oldObject);
                else
                    onObjectChange(oldObject, unityObject);
            }
            return unityObject;
        }

        public delegate void OnObjectSet<T>(T newObject);
        public delegate void OnObjectRemove<T>(T oldObject);
        public delegate void OnObjectChange<T>(T oldObject, T newObject);
    }

}
