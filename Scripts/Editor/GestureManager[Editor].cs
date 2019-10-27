using System.Collections.Generic;
using System.Linq;
using GestureManager.Scripts.Core;
using UnityEditor;
using UnityEngine;
using VRCSDK2;

namespace GestureManager.Scripts.Editor
{
    [CustomEditor(typeof(GestureManager))]
    public class GestureManagerEditor : UnityEditor.Editor
    {
        private const string Version = "1.0.0";

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

        private delegate int OnNoneSelected(int lastPosition);

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

                var usingType = GetManager().GetUsedType();
                var notUsingType = GetManager().GetNotUsedType();

                GetManager().avatar = MyLayoutHelper.ObjectField("Controlling Avatar: ", GetManager().avatar, newObject =>
                {
                    if (newObject != null)
                    {
                        var descriptor = newObject.GetComponent<VRC_AvatarDescriptor>();
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

                if (GetManager().avatar == null)
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

                MyLayoutHelper.MyToolbar("GestureManager_Main_Toolbar", new[]
                {
                    new MyLayoutHelper.MyToolbarRow("Gestures", () =>
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
                        GetManager().left = OnCheckBoxGuiHand(GetManager().left, position => 0);
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        GUILayout.Label("Right Hand", guiHandTitle);
                        GetManager().right = OnCheckBoxGuiHand(GetManager().right, position => 0);
                        GUILayout.EndVertical();

                        GUILayout.EndHorizontal();
                    }),
                    new MyLayoutHelper.MyToolbarRow("Emotes", () =>
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
                    }),
                    new MyLayoutHelper.MyToolbarRow("Idles", () => { GUILayout.Label("Avatar Idles."); }),
                    new MyLayoutHelper.MyToolbarRow("Test Animation", () =>
                    {
                        GUILayout.Label("Force animation.");

                        GUILayout.BeginHorizontal();
                        var lastAnim = selectingCustomAnim;
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
                    })
                });
            }
            else
            {
                if (EditorApplication.isPlaying)
                {
                    if (GetManager().gameObject.activeInHierarchy)
                    {
                        if (GetManager().GetLastCheckedActiveDescriptors().Length == 0)
                        {
                            GUILayout.Label(
                                "There are no VRC_AvatarDescriptor on your scene. \nPlease consider adding one to your avatar before entering in PlayMode."
                                , textError);
                        }
                        else
                        {
                            var eligible = new List<VRC_AvatarDescriptor>();
                            var nonEligible = new List<VRC_AvatarDescriptor>();

                            foreach (var descriptor in GetManager().GetLastCheckedActiveDescriptors())
                            {
                                if (GetManager().IsValidDesc(descriptor))
                                    eligible.Add(descriptor);
                                else
                                    nonEligible.Add(descriptor);
                            }

                            GUILayout.Label(eligible.Count == 0 ? "No one of your VRC_AvatarDescriptor are eligible." : "Eligible VRC_AvatarDescriptors:", subHeader);

                            foreach (var descriptor in eligible)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(descriptor.gameObject.name + ":");

                                if (GUILayout.Button("Set As Avatar", GUILayout.Width(200)))
                                {
                                    GetManager().InitForAvatar(descriptor);
                                }

                                GUILayout.EndHorizontal();
                            }

                            if (eligible.Count != 0 && nonEligible.Count != 0)
                                GUILayout.Label("Non-Eligible VRC_AvatarDescriptors:", subHeader);

                            foreach (var descriptor in nonEligible.Where(descriptor => descriptor != null))
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

                        if (GUILayout.Button("Check Again"))
                        {
                            GetManager().CheckActiveDescriptors();
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
                        GetManager().CheckForUpdates((error) => { EditorUtility.DisplayDialog("Gesture Manager Updater", "Error :c (" + error.responseCode + ")", "Okay"); }, response =>
                        {
                            var infos = response.downloadHandler.text.Trim().Split('\n');
                            var lastVersion = infos[0];
                            var download = infos[1];
                            if (lastVersion.Equals(Version))
                            {
                                EditorUtility.DisplayDialog("Gesture Manager Updater", "You have the latest version of the manager. (" + lastVersion + ")", "Good!");
                            }
                            else
                            {
                                if (EditorUtility.DisplayDialog("Gesture Manager Updater", "Newer version available! (" + lastVersion + ")", "Download", "Cancel"))
                                {
                                    Application.OpenURL(download);
                                }
                            }
                        });
                    }

                    GUILayout.Space(20);
                    if (GUILayout.Button("My Discord Name", GUILayout.Width(130)))
                    {
                        const string CONTENT_ME = "BlackStartx#6593";
                        if (EditorUtility.DisplayDialog("It's me!", CONTENT_ME, "Copy To Clipboard!", "Ok!"))
                        {
                            ClipBoard = CONTENT_ME;
                        }
                    }


                    /**
                     * Dunno if i will insert a donation button directly on the script... 
                     */
//                    GUILayout.Space(20);
//                    if (GUILayout.Button("Donate", GUILayout.Width(130)))
//                    {
//                        Application.OpenURL("https://www.paypal.me/blackstartx");
//                    }


                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Label("Script made by BlackStartx", bottomStyle);
        }

        private void Init()
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(10, 10, 10, 10)
            };

            guiHandTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(10, 10, 10, 10)
            };

            bottomStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                padding = new RectOffset(5, 5, 5, 5)
            };

            middleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(5, 5, 5, 5)
            };

            emoteError = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(5, 5, 5, 5)
            };

            textError = new GUIStyle(GUI.skin.label)
            {
                active = {textColor = Color.red},
                normal = {textColor = Color.red},
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };

            guiGreenButton = new GUIStyle(GUI.skin.button)
            {
                normal = {textColor = Color.green}
            };

            subHeader = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            guiGreenButton.fixedWidth = 100;
        }

        private int OnCheckBoxGuiHand(int position, OnNoneSelected onNone)
        {
            var gesture = new[] {false, false, false, false, false, false, false, false};

            gesture[position] = true;

            gesture[1] = EditorGUILayout.Toggle(manager.GetFinalGestureName(1), gesture[1]);
            gesture[2] = EditorGUILayout.Toggle(manager.GetFinalGestureName(2), gesture[2]);
            gesture[3] = EditorGUILayout.Toggle(manager.GetFinalGestureName(3), gesture[3]);
            gesture[4] = EditorGUILayout.Toggle(manager.GetFinalGestureName(4), gesture[4]);
            gesture[5] = EditorGUILayout.Toggle(manager.GetFinalGestureName(5), gesture[5]);
            gesture[6] = EditorGUILayout.Toggle(manager.GetFinalGestureName(6), gesture[6]);
            gesture[7] = EditorGUILayout.Toggle(manager.GetFinalGestureName(7), gesture[7]);

            for (var i = 0; i < gesture.Length; i++)
            {
                if (gesture[i] && position != i)
                {
                    for (var ix = 0; ix < gesture.Length; ix++)
                    {
                        if (ix == i)
                            continue;
                        gesture[ix] = false;
                    }
                }
            }

            for (var i = 0; i < gesture.Length; i++)
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

        private static string ClipBoard
        {
            set { EditorGUIUtility.systemCopyBuffer = value; }
        }
    }
}