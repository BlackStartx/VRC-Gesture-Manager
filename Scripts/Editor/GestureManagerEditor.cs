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
using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.UI.GraphicRaycaster;
#endif
using UnityEngine.UIElements;

namespace GestureManager.Scripts.Editor
{
    [CustomEditor(typeof(GestureManager))]
    public class GestureManagerEditor : UnityEditor.Editor
    {
        private GestureManager Manager => target as GestureManager;
        private static IEnumerable<GmgAvatarDescriptor> Descriptors => FindSceneObjectsOfTypeAll<GmgAvatarDescriptor>();
        private IEnumerable<ModuleBase> Modules => Descriptors.Select(descriptor => ModuleHelper.GetModuleFor(Manager, descriptor)).Where(module => module != null);
        private static bool IsValidObject(GameObject g) => g.hideFlags != HideFlags.NotEditable && g.hideFlags != HideFlags.HideAndDontSave && g.scene.name != null;
        private static IEnumerable<T> FindSceneObjectsOfTypeAll<T>() where T : Component => Resources.FindObjectsOfTypeAll<T>().Where(t => IsValidObject(t.gameObject));

        private readonly PrefUpdater _updater = new PrefUpdater();
        private VisualElement _root;

        private const int AntiAliasing = 4;

        private const string Version = "3.6.0";
        private const string Discord = "BlackStartx#6593";

        private const string VersionURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.v3rsion";
        private const string DiscordURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.discord";

        [MenuItem("Tools/Gesture Manager Emulator", false, -42)]
        public static void AddNewEmulator()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GestureManager/GestureManager.prefab");
            if (!asset) asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.FindAssets("t:prefab GestureManager").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault());
            AddNewEmulator(asset);
        }

        private static void AddNewEmulator(UnityEngine.Object gObject)
        {
            var prefab = PrefabUtility.InstantiatePrefab(gObject) as GameObject;
            CreateAndPing(prefab ? prefab.GetComponent<GestureManager>() : null);
        }

        public static void CreateAndPing(GestureManager manager = null)
        {
            if (!manager) manager = new GameObject("GestureManager").AddComponent<GestureManager>();
            Selection.activeObject = manager;
            EditorGUIUtility.PingObject(manager);
        }

        private void Awake()
        {
            if (!Manager.gameObject.GetComponent<GmgAvatarDescriptor>()) return;
            DestroyImmediate(Manager);
            CreateAndPing();
        }

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
            if (!Manager) return;
            Manager.SetDrag(!Event.current.alt);

            GUILayout.Label("Gesture Manager 3.6", GestureManagerStyles.TitleStyle);
            _updater.Gui();

            if (Manager.Module != null)
            {
                GmgLayoutHelper.ObjectField("Controlling Avatar: ", Manager.Module.Avatar, OnAvatarSwitch);
                Manager.Module?.EditorHeader();
                Manager.Module?.EditorContent(this, _root);
            }
            else SetupGui();

            GestureManagerStyles.Sign();
        }

        private void SetupGui()
        {
            var isLoaded = Manager.gameObject.scene.isLoaded;
            if (EditorApplication.isPlaying && isLoaded)
            {
                if (!Manager || !Manager.enabled || !Manager.gameObject.activeInHierarchy) GUILayout.Label("I'm disabled!", GestureManagerStyles.TextError);
                else CheckGui();
            }
            else
            {
                GUILayout.Label(isLoaded ? "I'm an useless script if you aren't on play mode :D" : "Drag & Drop me into the scene to start testing! ♥", GestureManagerStyles.MiddleStyle);
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
            var fullGestureString = module.GetFinalGestureByIndex(gestureIndex).name;
            var nameString = "[" + fullGestureString.Substring(fullGestureString.IndexOf("]", StringComparison.Ordinal) + 2) + "]";
            var newAnimation = GmgAnimationHelper.CloneAnimationAsset(module.GetFinalGestureByIndex(gestureIndex));
            var pathString = EditorUtility.SaveFilePanelInProject("Creating Gesture: " + fullGestureString, nameString + ".anim", "anim", "Hi (?)");

            if (pathString.Length == 0) return;

            AssetDatabase.CreateAsset(newAnimation, pathString);
            newAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathString);
            module.AddGestureToOverrideController(gestureIndex, newAnimation);
        }

        /*
         * Layout Builders
         */

        internal static void OnCheckBoxGuiHand(ModuleBase module, GestureHand hand, int position, bool overrider)
        {
            for (var i = 1; i < 8; i++)
                using (new GUILayout.HorizontalScope())
                    if (OnCheckBoxGuiHandAnimation(module, i, position, overrider, out var isOn))
                        module.OnNewHand(hand, isOn ? i : 0);
        }

        private static bool OnCheckBoxGuiHandAnimation(ModuleBase module, int i, int position, bool overrider, out bool isOn)
        {
            GUILayout.Label(module.GetFinalGestureByIndex(i).name);
            GUILayout.FlexibleSpace();
            isOn = position == i;
            var isDifferent = isOn != (isOn = GUILayout.Toggle(isOn, ""));
            if (!overrider) return isDifferent;
            if (!module.HasGestureBeenOverridden(i)) OverrideButton(module, i);
            else GUILayout.Space(35);
            return isDifferent;
        }

        private static void OverrideButton(ModuleBase module, int i)
        {
            if (GUILayout.Button(GestureManagerStyles.PlusTexture, GestureManagerStyles.PlusButton, GUILayout.Width(15), GUILayout.Height(15)))
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
            private static readonly Color Color = new Color(0.07f, 0.55f, 0.58f);

            private const string DayKey = "GM3 Update Day";
            private const string VerKey = "GM3 Update Ver";

            private int? _day;
            private int? _today;
            private string _version;
            private bool _checked;
            private bool _higher;

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

            private bool Higher() => string.CompareOrdinal(Version, Ver) < 0;

            public void Gui()
            {
                if (!_checked) Check();
                else if (_higher) Draw(GUILayoutUtility.GetLastRect());
            }

            private void Check()
            {
                _checked = true;
                _higher = Higher();
                if (Day != Today) RunCheck();
            }

            private async void RunCheck()
            {
                var versionString = await GetVersion();
                if (versionString == null) return;
                var (lastVersionString, _) = GetVersionInfo(versionString);
                Ver = lastVersionString;
                Day = Today;
                _higher = Higher();
            }

            private void Draw(Rect rect)
            {
                var cEvent = Event.current;
                rect.x += rect.width - 100;
                rect.width = 100;
                rect.height -= 16;
                rect.y += 8;
                using (new GmgLayoutHelper.GuiBackground(Color)) GUI.Label(rect, $"{Ver} is out!", GestureManagerStyles.UpdateStyle);
                if (cEvent.type == EventType.MouseDown && cEvent.button == 0 && rect.Contains(cEvent.mousePosition)) CheckForUpdates();
            }
        }
    }
}