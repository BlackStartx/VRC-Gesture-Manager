using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlackStartX.GestureManager.Editor.Lib;
using BlackStartX.GestureManager.Editor.Modules;
using BlackStartX.GestureManager.Runtime.Extra;
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
        private IEnumerable<ModuleBase> Modules => Descriptors.Select(descriptor => ModuleHelper.GetModuleFor(Manager, descriptor)).Where(module => module != null);
        private static bool IsValidObject(GameObject g) => g.hideFlags != HideFlags.NotEditable && g.hideFlags != HideFlags.HideAndDontSave && g.scene.name != null;
        private static IEnumerable<T> FindSceneObjectsOfTypeAll<T>() where T : Component => Resources.FindObjectsOfTypeAll<T>().Where(t => IsValidObject(t.gameObject));

        private VisualElement _root;

        private const int AntiAliasing = 4;

        private const string Discord = "BlackStartx#6593";
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

            GUILayout.Label("Gesture Manager 3.8", GestureManagerStyles.TitleStyle);

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
                GUILayout.Label(isLoaded ? "I'm a useless script if you aren't in play mode :D" : "Drag & Drop me into the scene to start testing! ♥", GestureManagerStyles.MiddleStyle);
                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = !GestureManager.InWebClientRequest;
                    if (GmgLayoutHelper.DebugButton("Enter Play-Mode")) EditorApplication.EnterPlaymode();

                    GUILayout.Space(20);

                    if (GmgLayoutHelper.DebugButton("My Discord Name")) CheckDiscordName();
                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(5);
            }
        }

        private void CheckGui()
        {
            if (GestureManager.LastCheckedActiveModules.Count != 0)
            {
                var eligibleList = GestureManager.LastCheckedActiveModules.Where(module => module.Avatar && module.IsValidDesc()).ToList();
                var nonEligibleList = GestureManager.LastCheckedActiveModules.Where(module => module.Avatar && !module.IsValidDesc()).ToList();

                GUILayout.Label(eligibleList.Count == 0 ? "No one of your VRC_AvatarDescriptor are eligible." : "Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                foreach (var module in eligibleList)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(module.Avatar.name + ":", GUILayout.Width(Screen.width - 131));
                            if (GUILayout.Button("Set As Avatar", GUILayout.Width(100))) Manager.SetModule(module);
                        }

                        foreach (var warningString in module.GetWarnings()) GUILayout.Label(warningString, GestureManagerStyles.TextWarning);
                    }
                }

                if (eligibleList.Count != 0 && nonEligibleList.Count != 0) GUILayout.Label("Non-Eligible VRC_AvatarDescriptors:", GestureManagerStyles.SubHeader);

                foreach (var module in nonEligibleList)
                {
                    using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError))
                    {
                        GUILayout.Label(module.Avatar.name + ":");
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
                var module = ModuleHelper.GetModuleFor(Manager, obj);
                if (module != null && module.IsValidDesc()) Manager.SetModule(module);
            }
            else Manager.UnlinkModule();
        }

        private ModuleBase GetValidDescriptor() => CheckActiveModules().FirstOrDefault(module => module.IsPerfectDesc());

        private List<ModuleBase> CheckActiveModules() => GestureManager.LastCheckedActiveModules = Modules.ToList();

        private static void DiscordPopup(string discord)
        {
            if (EditorUtility.DisplayDialog("It's me!", discord, "Copy To Clipboard!", "Ok!"))
                EditorGUIUtility.systemCopyBuffer = discord;
        }

        /*
         * Layout Builders
         */

        internal static void OnCheckBoxGuiHand(ModuleBase module, GestureHand hand, int position, Action<int> click)
        {
            for (var i = 1; i < 8; i++)
                using (new GUILayout.HorizontalScope())
                    if (OnCheckBoxGuiHandAnimation(module, i, position, click, out var isOn))
                        module.OnNewHand(hand, isOn ? i : 0);
        }

        private static bool OnCheckBoxGuiHandAnimation(ModuleBase module, int i, int position, Action<int> click, out bool isOn)
        {
            GUILayout.Label(module.GetGestureTextNameByIndex(i));
            GUILayout.FlexibleSpace();
            isOn = position == i;
            var isDifferent = isOn != (isOn = GUILayout.Toggle(isOn, ""));
            if (click == null) return isDifferent;
            if (!module.HasGestureBeenOverridden(i)) OverrideButton(i, click);
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

        private static async Task<string> GetDiscord() => await Get(DiscordURL);
        private static async Task<string> GetSupport() => await Get(SupportURL);
    }
}