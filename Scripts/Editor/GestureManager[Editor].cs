using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GestureManager.Scripts.Core.Editor;
using GestureManager.Scripts.Editor.Modules;
using GestureManager.Scripts.Extra;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor
{
    [CustomEditor(typeof(GestureManager))]
    public class GestureManagerEditor : UnityEditor.Editor
    {
        private GestureManager Manager => target as GestureManager;
        public override bool RequiresConstantRepaint() => Manager.Module?.RequiresConstantRepaint ?? false;

        private VisualElement _root;

        private const int AntiAliasing = 4;
        
        private const string Version = "3.2.0";
        private const string BsxName = "BlackStartx";
        private const string Discord = "BlackStartx#6593";

        private const string VersionURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.v3rsion";
        private const string DiscordURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.discord";

        private void OnEnable()
        {
            if (!Application.isPlaying || !Manager.enabled || !Manager.gameObject.activeInHierarchy) return;

            if (Manager.Module != null) return;
            Manager.SetModule(GetValidDescriptor());
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _root.Add(new IMGUIContainer(ManagerGui));
            foreach (var inspectorWindow in GmgLayoutHelper.GetInspectorWindows()) inspectorWindow.MySetAntiAliasing(AntiAliasing);
            return _root;
        }

        private void ManagerGui()
        {
            GUILayout.Label("Gesture Manager 3.2", GestureManagerStyles.TitleStyle);

            if (Manager.Module != null)
            {
                if (Manager.Module.IsInvalid()) Manager.UnlinkModule();

                GmgLayoutHelper.ObjectField("Controlling Avatar: ", Manager.Module.Avatar, newObject =>
                {
                    if (newObject)
                    {
                        var module = ModuleHelper.GetModuleForDescriptor(Manager, newObject.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>());
                        if (module == null || !module.IsValidDesc()) return;
                        Manager.SetModule(module);
                    }
                    else Manager.UnlinkModule();
                });

                if (Manager.Module == null) return;

                GUILayout.BeginHorizontal();
                Manager.Module.EditorHeader();
                GUILayout.EndHorizontal();

                Manager.Module.EditorContent(this, _root);
            }
            else
            {
                if (EditorApplication.isPlaying)
                {
                    if (Manager.enabled && Manager.gameObject.activeInHierarchy)
                    {
                        if (Manager.GetLastCheckedActiveDescriptors().Count == 0)
                            GUILayout.Label("There are no VRC_AvatarDescriptor on your scene. \nPlease consider adding one to your avatar before entering in PlayMode.",
                                GestureManagerStyles.TextError);
                        else
                        {
                            var eligible = Manager.GetLastCheckedActiveDescriptors().Where(module => module.IsValidDesc()).ToList();
                            var nonEligible = Manager.GetLastCheckedActiveDescriptors().Where(module => !module.IsValidDesc()).ToList();

                            GUILayout.Label(eligible.Count == 0 ? "No one of your VRC_AvatarDescriptor are eligible." : "Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                            foreach (var module in eligible)
                            {
                                GUILayout.BeginVertical(GUI.skin.box);
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Label(module.AvatarDescriptor.gameObject.name + ":", GUILayout.Width(Screen.width - 131));
                                if (GUILayout.Button("Set As Avatar", GUILayout.Width(100))) Manager.SetModule(module);
                                GUILayout.EndHorizontal();
                                foreach (var warning in module.GetWarnings()) GUILayout.Label(warning, GestureManagerStyles.TextWarning);
                                GUILayout.EndVertical();
                            }

                            if (eligible.Count != 0 && nonEligible.Count != 0) GUILayout.Label("Non-Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                            foreach (var module in nonEligible)
                            {
                                GUILayout.BeginVertical(GestureManagerStyles.EmoteError);
                                GUILayout.Label(module.AvatarDescriptor.gameObject.name + ":");
                                foreach (var error in module.GetErrors()) GUILayout.Label(error, GestureManagerStyles.TextError);
                                GUILayout.EndVertical();
                            }
                        }

                        if (GUILayout.Button("Check Again")) CheckActiveDescriptors();
                    }
                    else GUILayout.Label("I'm disabled!", GestureManagerStyles.TextError);
                }
                else
                {
                    GUILayout.Label("I'm an useless script if you aren't on play mode :D", GestureManagerStyles.MiddleStyle);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUI.enabled = !Manager.inWebClientRequest;
                    if (GUILayout.Button("Check For Updates", GUILayout.Width(130)))
                        CheckForUpdates();

                    GUILayout.Space(20);

                    if (GUILayout.Button("My Discord Name", GUILayout.Width(130)))
                        CheckDiscordName();
                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Label($"Script made by {BsxName}", GestureManagerStyles.BottomStyle);
        }

        private async void CheckForUpdates()
        {
            if (Manager.inWebClientRequest) return;

            Manager.inWebClientRequest = true;
            var version = await GetVersion();
            Manager.inWebClientRequest = false;

            const string title = "Gesture Manager Updater";
            if (version != null)
            {
                var infos = version.Trim().Split('\n');
                var lastVersion = infos[0];
                var download = infos[1];
                if (lastVersion.Equals(Version)) EditorUtility.DisplayDialog(title, $"You have the latest version of the manager. ({lastVersion})", "Good!");
                else if (EditorUtility.DisplayDialog(title, $"Newer version available! ({lastVersion})", "Download", "Cancel")) Application.OpenURL(download);
            }
            else EditorUtility.DisplayDialog(title, "Unable to check for updates.", "Okay");
        }

        private async void CheckDiscordName()
        {
            if (Manager.inWebClientRequest) return;

            Manager.inWebClientRequest = true;
            var discord = await GetDiscord();
            Manager.inWebClientRequest = false;

            EditorUtility.ClearProgressBar();
            DiscordPopup(discord ?? Discord);
        }

        private ModuleBase GetValidDescriptor()
        {
            CheckActiveDescriptors();
            return Manager.LastCheckedActiveDescriptors.FirstOrDefault(module => module.IsPerfectDesc());
        }

        private void CheckActiveDescriptors()
        {
            Manager.LastCheckedActiveDescriptors = VRC.Tools.FindSceneObjectsOfTypeAll<VRC.SDKBase.VRC_AvatarDescriptor>()
                .Select(descriptor => ModuleHelper.GetModuleForDescriptor(Manager, descriptor))
                .Where(module => module != null)
                .ToList();
        }

        private static void DiscordPopup(string discord)
        {
            if (EditorUtility.DisplayDialog("It's me!", discord, "Copy To Clipboard!", "Ok!"))
                EditorGUIUtility.systemCopyBuffer = discord;
        }

        private static void RequestGestureDuplication(GestureManager manager, GestureHand hand, int gestureIndex)
        {
            var fullGestureName = manager.GetFinalGestureName(hand, gestureIndex);
            var gestureName = "[" + fullGestureName.Substring(fullGestureName.IndexOf("]", StringComparison.Ordinal) + 2) + "]";
            var newAnimation = GmgAnimationHelper.CloneAnimationAsset(manager.Module.GetFinalGestureByIndex(hand, gestureIndex));
            var path = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + fullGestureName, gestureName + ".anim", "anim", "Hi (?)");

            if (path.Length == 0) return;

            AssetDatabase.CreateAsset(newAnimation, path);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            manager.Module.AddGestureToOverrideController(gestureIndex, newAnimation);
        }

        /*
         * Layout Builders
         */

        internal static int OnCheckBoxGuiHand(GestureManager manager, GestureHand hand, int position, Func<int, int> onNone)
        {
            var gesture = new bool[8];
            var texture = EditorGUIUtility.isProSkin ? GestureManagerStyles.PlusTexturePro : GestureManagerStyles.PlusTexture;

            gesture[position] = true;

            for (var i = 1; i < 8; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(manager.GetFinalGestureName(hand, i));
                GUILayout.FlexibleSpace();
                gesture[i] = GUILayout.Toggle(gesture[i], "");
                if (!manager.Module.HasGestureBeenOverridden(i)) OverrideButton(texture, manager, hand, i);
                else GUILayout.Space(35);
                GUILayout.EndHorizontal();
            }

            for (var i = 0; i < gesture.Length; i++)
            {
                if (!gesture[i] || position == i) continue;

                for (var ix = 0; ix < gesture.Length; ix++)
                    if (ix != i)
                        gesture[ix] = false;
            }

            for (var i = 0; i < gesture.Length; i++)
                if (gesture[i])
                    return i;

            return onNone(position);
        }

        private static void OverrideButton(Texture texture, GestureManager manager, GestureHand hand, int i)
        {
            if (GUILayout.Button(texture, GestureManagerStyles.PlusButton, GUILayout.Width(15), GUILayout.Height(15)))
                RequestGestureDuplication(manager, hand, i);
        }

        internal static void OnEmoteButton(GestureManager manager, int emote, Action<int> play, Action stop)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(manager.GetEmoteName(emote - 1));
            if (manager.emote == emote)
            {
                if (GUILayout.Button("Stop", GestureManagerStyles.GuiGreenButton)) stop();
            }
            else if (GUILayout.Button("Play", GUILayout.Width(100))) play(emote);

            GUILayout.EndHorizontal();
        }

        /*
         * Async
         */

        private static async Task<string> Get(string uri)
        {
            try
            {
                return await new WebClient().DownloadStringTaskAsync(uri);
            }
            catch (WebException)
            {
                return null;
            }
        }

        private static async Task<string> GetVersion() => await Get(VersionURL);
        private static async Task<string> GetDiscord() => await Get(DiscordURL);
    }
}