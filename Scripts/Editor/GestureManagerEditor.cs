using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlackStartX.GestureManager.Data;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules;
using UnityEditor;
using UnityEngine;
using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.UI.GraphicRaycaster;
#endif
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor
{
    [CustomEditor(typeof(GestureManager))]
    public class GestureManagerEditor : UnityEditor.Editor
    {
        private GestureManager Manager => target as GestureManager;
        private static IEnumerable<GmgAvatarDescriptor> Descriptors => FindSceneObjectsOfTypeAll<GmgAvatarDescriptor>();
        private static IEnumerable<ModuleBase> Modules => Descriptors.Select(ModuleHelper.GetModuleFor).Where(module => module != null);
        private static bool IsValidObject(GameObject g) => g.hideFlags != HideFlags.NotEditable && g.hideFlags != HideFlags.HideAndDontSave && g.scene.name != null;
        private static IEnumerable<T> FindSceneObjectsOfTypeAll<T>() where T : Component => Resources.FindObjectsOfTypeAll<T>().Where(t => IsValidObject(t.gameObject));

        private VisualElement _root;
        private bool _setting;

        private const int AntiAliasing = 4;

        private const string Discord = "blackstartx";
        private const string StringPath = "Packages/vrchat.blackstartx.gesture-manager/GestureManager.prefab";

        private const string DiscordURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.discord";
        private const string SupportURL = "https://raw.githubusercontent.com/BlackStartx/VRC-Gesture-Manager/master/.support";

        [MenuItem("Tools/Gesture Manager Emulator", false, -42)]
        public static void AddNewEmulator()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(StringPath);
            if (!asset) asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.FindAssets("t:prefab GestureManager").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault());
            AddNewEmulator(asset);
        }

        private static void AddNewEmulator(UnityEngine.Object gObject)
        {
            var prefab = PrefabUtility.InstantiatePrefab(gObject) as GameObject;
            CreateAndPing(!prefab ? null : prefab.GetComponent<GestureManager>());
        }

        private static void TogglePlaymode(bool isPlaying)
        {
            if (isPlaying) EditorApplication.ExitPlaymode();
            else EditorApplication.EnterPlaymode();
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

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _root.Add(new IMGUIContainer(ManagerGui));
            if (Application.isPlaying) TryInitialize();
            foreach (var inspectorWindow in GmgLayoutHelper.GetInspectorWindows()) inspectorWindow.MySetAntiAliasing(AntiAliasing);
            return _root;
        }

        private void TryInitialize()
        {
            if (!Manager.enabled || !Manager.gameObject.activeInHierarchy || Manager.Module != null) return;
            Manager.StartCoroutine(TryInitializeRoutine(ModuleHelper.GetModuleFor(Manager.settings.favourite)));
        }

        private IEnumerator TryInitializeRoutine(ModuleBase module)
        {
            yield return null;
            module ??= GetValidDescriptor();
            if (module != null) Manager.SetModule(module);
        }

        private void ManagerGui()
        {
            if (!Manager) return;
            var gObject = Manager.gameObject;
            var isPrefab = !gObject.scene.isLoaded;
            var isActive = isPrefab ? gObject.activeSelf : gObject.activeInHierarchy;
            var isPlaying = EditorApplication.isPlaying;

            Manager.SetDrag(!Event.current.alt);

            if (isPrefab || isPlaying) GUILayout.Label(GestureManager.Version, GestureManagerStyles.TitleStyle);
            else if (GmgLayoutHelper.SettingsGearLabel(GestureManager.Version, _setting)) _setting = !_setting;

            if (Manager.Module != null) ModuleGui();
            else if (_setting) GestureManagerSettings.SettingGui(Manager);
            else SetupGui(isActive, Manager.enabled, isPlaying, isPrefab);

            GestureManagerStyles.Sign();
        }

        private void ModuleGui()
        {
            GmgLayoutHelper.ObjectField("Controlling Avatar: ", Manager.Module.Avatar, OnAvatarSwitch);
            if (Manager.Module == null) return;
            Manager.Module.EditorHeader();
            Manager.Module.EditorContent(this, _root);
        }

        private void SetupGui(bool isActive, bool isEnabled, bool isPlaying, bool isPrefab)
        {
            if (!isActive || !isEnabled || !isPlaying || isPrefab)
            {
                if (!isEnabled || !isActive) GUILayout.Label("I'm disabled!", GestureManagerStyles.MiddleError);
                else GUILayout.Label(isPrefab ? "Drag & Drop me into the scene to start testing! ♥" : "I'm a useless script if you aren't in play mode :D", GestureManagerStyles.MiddleStyle);

                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                using (new GmgLayoutHelper.GuiEnabled(!GestureManager.InWebClientRequest))
                {
                    if (GmgLayoutHelper.DebugButton(isPlaying ? "Exit Play-Mode" : "Enter Play-Mode")) TogglePlaymode(isPlaying);

                    GUILayout.Space(20);

                    if (GmgLayoutHelper.DebugButton("My Discord Name")) CheckDiscordName();
                }

                GUILayout.Space(5);
            }
            else CheckGui();
        }

        private void CheckGui()
        {
            if (GestureManager.LastCheckedActiveModules.Count != 0)
            {
                var eligibleList = GestureManager.LastCheckedActiveModules.Where(module => module.IsValidDesc()).ToList();
                var nonEligibleList = GestureManager.LastCheckedActiveModules.Where(module => !module.IsValidDesc()).ToList();

                GUILayout.Label(eligibleList.Count == 0 ? "No one of your VRC_AvatarDescriptor are eligible." : "Eligible VRC_AvatarDescriptors:", GestureManagerStyles.Centered);

                foreach (var module in eligibleList)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"{module.Name}:", GUILayout.Width(EditorGUIUtility.currentViewWidth - 133));
                            if (GUILayout.Button("Set As Avatar", GUILayout.Width(100))) Manager.SetModule(module);
                        }

                        foreach (var warningString in module.GetWarnings()) GUILayout.Label(warningString, GestureManagerStyles.TextWarning);
                    }
                }

                if (eligibleList.Count != 0 && nonEligibleList.Count != 0) GUILayout.Label("Non-Eligible VRC_AvatarDescriptors:", GestureManagerStyles.Centered);

                foreach (var module in nonEligibleList)
                {
                    using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError))
                    {
                        GUILayout.Label($"{module.Name}:");
                        foreach (var errorString in module.GetErrors()) GUILayout.Label(errorString, GestureManagerStyles.TextError);
                    }
                }
            }
            else GUILayout.Label("There are no VRC_AvatarDescriptors in your scene. \nPlease consider adding one to your avatar before entering PlayMode.", GestureManagerStyles.TextError);

            if (GUILayout.Button("Check Again")) CheckActiveModules();
        }

        private void OnAvatarSwitch(GameObject obj)
        {
            if (obj)
            {
                var module = ModuleHelper.GetModuleFor(obj);
                if (module != null) Manager.SetModule(module);
            }
            else Manager.UnlinkModule();
        }

        private static ModuleBase GetValidDescriptor() => CheckActiveModules().FirstOrDefault(module => module.IsPerfectDesc());

        private static List<ModuleBase> CheckActiveModules() => GestureManager.LastCheckedActiveModules = Modules.ToList();

        private static void DiscordPopup(string discord)
        {
            if (EditorUtility.DisplayDialog("It's me!", discord, "Copy To Clipboard!", "Ok!"))
                EditorGUIUtility.systemCopyBuffer = discord;
        }

        /*
         * Layout Builders
         */

        internal static void OnCheckBoxGuiHand(ModuleBase module, GestureHand hand, int position, Action<int> click, Func<int, bool> overridden = null)
        {
            for (var i = 1; i < 8; i++)
                using (new GUILayout.HorizontalScope())
                    if (OnCheckBoxGuiHandAnimation(module, i, position, click, out var isOn, overridden))
                        module.OnNewHand(hand, isOn ? i : 0);
        }

        private static bool OnCheckBoxGuiHandAnimation(ModuleBase module, int i, int position, Action<int> click, out bool isOn, Func<int, bool> overridden)
        {
            GUILayout.Label(module.GetGestureTextNameByIndex(i));
            GUILayout.FlexibleSpace();
            isOn = position == i;
            var isDifferent = isOn != (isOn = GUILayout.Toggle(isOn, ""));
            if (click == null) return isDifferent;
            if (overridden?.Invoke(i) ?? false) OverrideButton(i, click);
            else GUILayout.Space(35);
            return isDifferent;
        }

        private static void OverrideButton(int i, Action<int> click)
        {
            if (GUILayout.Button(GestureManagerStyles.PlusTexture, GestureManagerStyles.PlusButton, GUILayout.Width(15), GUILayout.Height(15)))
                click(i);
        }

        /*
         * Async Calls
         */

        private static async void CheckDiscordName()
        {
            if (GestureManager.InWebClientRequest) return;

            GestureManager.InWebClientRequest = true;
            var discordString = await GetDiscord();
            GestureManager.InWebClientRequest = false;

            DiscordPopup(discordString ?? Discord);
        }

        internal static async void CheckSupporters(Action<string> supporters)
        {
            if (GestureManager.InWebClientRequest) return;

            GestureManager.InWebClientRequest = true;
            var supportString = await GetSupport();
            GestureManager.InWebClientRequest = false;
            if (!string.IsNullOrEmpty(supportString)) supporters(supportString);
        }

        /*
         * Async
         */

        private static async Task<string> Get(string url)
        {
            try
            {
                return await new WebClient().DownloadStringTaskAsync(url);
            }
            catch (WebException)
            {
                return null;
            }
        }

        private static Task<string> GetDiscord() => Get(DiscordURL);
        private static Task<string> GetSupport() => Get(SupportURL);
    }
}