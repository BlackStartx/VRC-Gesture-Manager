using System;
using System.Collections.Generic;
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
        private static IEnumerable<VRC.SDKBase.VRC_AvatarDescriptor> Descriptors => VRC.Tools.FindSceneObjectsOfTypeAll<VRC.SDKBase.VRC_AvatarDescriptor>();
        private IEnumerable<ModuleBase> Modules => Descriptors.Select(descriptor => ModuleHelper.GetModuleFor(Manager, descriptor)).Where(module => module != null);

        private readonly PrefUpdater _updater = new PrefUpdater();
        private VisualElement _root;

        private const int AntiAliasing = 4;

        private const string Version = "3.4.0";
        private const string BsxName = "BlackStartx";
        private const string Discord = "BlackStartx#6593";

        private const string VersionURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.v3rsion";
        private const string DiscordURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.discord";

        private void OnEnable()
        {
            if (!Application.isPlaying || !Manager.enabled || !Manager.gameObject.activeInHierarchy || Manager.Module != null) return;
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
            GUILayout.Label("Gesture Manager 3.4", GestureManagerStyles.TitleStyle);
            if (!_updater.Checked) _updater.Check();
            else if (_updater.Different) _updater.Draw(GUILayoutUtility.GetLastRect());

            if (Manager.Module != null)
            {
                GmgLayoutHelper.ObjectField("Controlling Avatar: ", Manager.Module.Avatar, OnAvatarSwitch);
                Manager.Module?.EditorHeader();
                Manager.Module?.EditorContent(this, _root);
            }
            else SetupGui();

            GUILayout.Label($"Script made by {BsxName}", GestureManagerStyles.BottomStyle);
        }

        private void SetupGui()
        {
            if (EditorApplication.isPlaying)
            {
                if (!Manager || !Manager.enabled || !Manager.gameObject.activeInHierarchy) GUILayout.Label("I'm disabled!", GestureManagerStyles.TextError);
                else CheckGui();
            }
            else
            {
                GUILayout.Label("I'm an useless script if you aren't on play mode :D", GestureManagerStyles.MiddleStyle);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = !GestureManager.InWebClientRequest;
                    if (GUILayout.Button("Check For Updates", GUILayout.Width(130))) CheckForUpdates();

                    GUILayout.Space(20);

                    if (GUILayout.Button("My Discord Name", GUILayout.Width(130))) CheckDiscordName();
                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void CheckGui()
        {
            if (Manager.LastCheckedActiveModules.Count != 0)
            {
                var eligibleList = Manager.LastCheckedActiveModules.Where(module => module.Avatar && module.IsValidDesc()).ToList();
                var nonEligibleList = Manager.LastCheckedActiveModules.Where(module => module.Avatar && !module.IsValidDesc()).ToList();

                GUILayout.Label(eligibleList.Count == 0 ? "No one of your VRC_AvatarDescriptor are eligible." : "Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                foreach (var module in eligibleList)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(module.Avatar.name + ":", GUILayout.Width(Screen.width - 131));
                    if (GUILayout.Button("Set As Avatar", GUILayout.Width(100))) Manager.SetModule(module);
                    GUILayout.EndHorizontal();
                    foreach (var warning in module.GetWarnings()) GUILayout.Label(warning, GestureManagerStyles.TextWarning);
                    GUILayout.EndVertical();
                }

                if (eligibleList.Count != 0 && nonEligibleList.Count != 0) GUILayout.Label("Non-Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                foreach (var module in nonEligibleList)
                {
                    GUILayout.BeginVertical(GestureManagerStyles.EmoteError);
                    GUILayout.Label(module.Avatar.name + ":");
                    foreach (var error in module.GetErrors()) GUILayout.Label(error, GestureManagerStyles.TextError);
                    GUILayout.EndVertical();
                }
            }
            else GUILayout.Label("There are no VRC_AvatarDescriptor on your scene. \nPlease consider adding one to your avatar before entering in PlayMode.", GestureManagerStyles.TextError);

            if (GUILayout.Button("Check Again")) CheckActiveModules();
        }

        private void OnAvatarSwitch(GameObject obj)
        {
            if (obj)
            {
                var module = ModuleHelper.GetModuleFor(Manager, obj);
                if (module != null && module.IsValidDesc()) Manager.SetModule(module);
            }
            else Manager.UnlinkModule();
        }

        private ModuleBase GetValidDescriptor() => CheckActiveModules().FirstOrDefault(module => module.IsPerfectDesc());

        private List<ModuleBase> CheckActiveModules() => Manager.LastCheckedActiveModules = Modules.ToList();

        private static void DiscordPopup(string discord)
        {
            if (EditorUtility.DisplayDialog("It's me!", discord, "Copy To Clipboard!", "Ok!"))
                EditorGUIUtility.systemCopyBuffer = discord;
        }

        private static void RequestGestureDuplication(ModuleBase module, int gestureIndex)
        {
            var fullGestureName = module.GetFinalGestureByIndex(gestureIndex).name;
            var gestureName = "[" + fullGestureName.Substring(fullGestureName.IndexOf("]", StringComparison.Ordinal) + 2) + "]";
            var newAnimation = GmgAnimationHelper.CloneAnimationAsset(module.GetFinalGestureByIndex(gestureIndex));
            var path = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + fullGestureName, gestureName + ".anim", "anim", "Hi (?)");

            if (path.Length == 0) return;

            AssetDatabase.CreateAsset(newAnimation, path);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            module.AddGestureToOverrideController(gestureIndex, newAnimation);
        }

        /*
         * Layout Builders
         */

        internal static void OnCheckBoxGuiHand(ModuleBase module, GestureHand hand, int position, bool overrider)
        {
            for (var i = 1; i < 8; i++)
                using (new GUILayout.HorizontalScope())
                    if (OnCheckBoxGuiHandAnimation(module, i, position, GestureManagerStyles.PlusTexture, overrider, out var isOn))
                        module.OnNewHand(hand, isOn ? i : 0);
        }

        private static bool OnCheckBoxGuiHandAnimation(ModuleBase module, int i, int position, Texture texture, bool overrider, out bool isOn)
        {
            GUILayout.Label(module.GetFinalGestureByIndex(i).name);
            GUILayout.FlexibleSpace();
            isOn = position == i;
            var isDifferent = isOn != (isOn = GUILayout.Toggle(isOn, ""));
            if (!overrider) return isDifferent;
            if (!module.HasGestureBeenOverridden(i)) OverrideButton(texture, module, i);
            else GUILayout.Space(35);
            return isDifferent;
        }

        private static void OverrideButton(Texture texture, ModuleBase module, int i)
        {
            if (GUILayout.Button(texture, GestureManagerStyles.PlusButton, GUILayout.Width(15), GUILayout.Height(15)))
                RequestGestureDuplication(module, i);
        }

        /*
         * Async Calls
         */

        private static (string lastVersion, string download) GetVersionInfo(string info) => GetVersionInfo(info.Trim().Split('\n'));

        private static (string lastVersion, string download) GetVersionInfo(IReadOnlyList<string> split) => (split[0], split[1]);

        private static async void CheckForUpdates()
        {
            if (GestureManager.InWebClientRequest) return;

            GestureManager.InWebClientRequest = true;
            var versionString = await GetVersion();
            GestureManager.InWebClientRequest = false;

            const string title = "Gesture Manager Updater";
            if (versionString != null)
            {
                var (lastVersionString, downloadString) = GetVersionInfo(versionString);
                var message = $"Newer version available! ({lastVersionString})\n\nIt's recommended to delete the GestureManager folder before importing the new package.";
                if (lastVersionString.Equals(Version)) EditorUtility.DisplayDialog(title, $"You have the latest version of the manager. ({lastVersionString})", "Good!");
                else if (EditorUtility.DisplayDialog(title, message, "Download", "Cancel")) Application.OpenURL(downloadString);
            }
            else EditorUtility.DisplayDialog(title, "Unable to check for updates.", "Okay");
        }

        private static async void CheckDiscordName()
        {
            if (GestureManager.InWebClientRequest) return;

            GestureManager.InWebClientRequest = true;
            var discordString = await GetDiscord();
            GestureManager.InWebClientRequest = false;

            EditorUtility.ClearProgressBar();
            DiscordPopup(discordString ?? Discord);
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

        private class PrefUpdater
        {
            public bool Different => Ver != Version;
            private const string DayKey = "GM3 Update Day";
            private const string VerKey = "GM3 Update Ver";

            private int? _day;
            private int? _today;
            private string _version;
            internal bool Checked;

            private int Day
            {
                get => _day ?? (_day = EditorPrefs.GetInt(DayKey)).Value;
                set => EditorPrefs.SetInt(DayKey, (_day = value).Value);
            }

            private string Ver
            {
                get => _version ?? (_version = EditorPrefs.GetString(VerKey, Version));
                set => EditorPrefs.SetString(VerKey, _version = value);
            }

            private int Today => _today ?? (_today = DateTime.Now.Day).Value;

            public void Check()
            {
                Checked = true;
                if (Day != Today) RunCheck();
            }

            private async void RunCheck()
            {
                var versionString = await GetVersion();
                if (versionString == null) return;
                var (lastVersionString, _) = GetVersionInfo(versionString);
                Ver = lastVersionString;
                Day = Today;
            }

            internal void Draw(Rect rect)
            {
                rect.x += rect.width - 100;
                rect.width = 100;
                rect.height -= 16;
                rect.y += 8;
                GUI.Label(rect, $"{Ver} is out!", GestureManagerStyles.UpdateStyle);
            }
        }
    }
}