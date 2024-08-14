using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Data
{
    public static class GestureManagerSettings
    {
        public const string LocalFolder = "LocalAvatarData";

        private static string UserPath(string folder, string user) => user == null ? null : Path.Combine(VrcDirectory, folder, user);
        private static string UserID(int index, IReadOnlyList<string> users) => index < users.Count ? users[index] : null;
        private static IEnumerable<string> Users => Directory.GetDirectories(DataDirectory).Select(Path.GetFileName);
        public static string VrcDirectory => Path.Combine(ModuleHelper.LocalLowPath, "VRChat", "VRChat");
        public static string UserPath(int index, string folder) => UserPath(folder, UserID(index));
        public static string UserID(int index) => index < 1 ? null : UserID(index, Choose);
        private static string DataDirectory => Path.Combine(VrcDirectory, LocalFolder);
        private static string[] Choose => new[] { "[Unset]" }.Concat(Users).ToArray();

        private static bool _generalSettings;
        private static bool _simulationSettings;

        public static void SettingGui(GestureManager manager)
        {
            GUILayout.Label("Here you can customize the tool settings!", GestureManagerStyles.MiddleStyle);
            GUILayout.Space(10);

            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError)) GeneralSettings(manager);
            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError)) SimulationSettings(manager);
        }

        private static void GeneralSettings(GestureManager manager)
        {
            if (!GmgLayoutHelper.FoldoutSection("General Settings", ref _generalSettings)) return;
            GUILayout.Label("What avatar should be picked on play?", GestureManagerStyles.SettingsText);
            manager.settings.favourite = GmgLayoutHelper.ComponentField("Favourite Avatar: ", manager.settings.favourite, manager);
            GUILayout.Label("User Data", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("The folder will be used to fetch local avatar information!", GestureManagerStyles.SettingsText);
            manager.settings.userIndex = GmgLayoutHelper.Popup("User ID: ", manager.settings.userIndex, Choose, manager);
            GUILayout.Label("Editor Settings", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("Easy toggles for Unity Editor settings!", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) BlendShapeSettings();
        }

        private static void SimulationSettings(GestureManager manager)
        {
            if (!GmgLayoutHelper.FoldoutSection("Simulation Settings", ref _simulationSettings)) return;
            GUILayout.Label("Do you wish to load the local stored parameters values!", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) LoadParametersSettings(manager);
            GUILayout.Label("Initial Pose", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("Set the initial pose of your avatar!", GestureManagerStyles.SettingsText);
            manager.settings.initialPose = GmgLayoutHelper.EnumPopup("Initial Pose: ", manager.settings.initialPose, manager);
            GUILayout.Label("Default Parameters", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("Set the initial states of the default parameters!", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) DefaultParametersSettings(manager);
        }

        private static void BlendShapeSettings(string label = "Blend-Shapes Clamping: ")
        {
            PlayerSettings.legacyClampBlendShapeWeights = EditorGUILayout.Toggle(label, PlayerSettings.legacyClampBlendShapeWeights);
            if (PlayerSettings.legacyClampBlendShapeWeights) return;
            GUILayout.FlexibleSpace();
            using (new GmgLayoutHelper.GuiContent(Color.yellow)) GUILayout.Label("[This is recommended to be on]");
        }

        private static void LoadParametersSettings(GestureManager manager)
        {
            using (new GmgLayoutHelper.GuiEnabled(manager.settings.userIndex != 0))
            {
                var buttonString = GUI.enabled ? "Open cache folder" : "No user selected!";

                if (!GUI.enabled) GmgLayoutHelper.Toggle("Load stored parameters: ", GUI.enabled, manager);
                else manager.settings.loadStored = GmgLayoutHelper.Toggle("Load stored parameters: ", manager.settings.loadStored, manager);

                if (GUILayout.Button(buttonString)) EditorUtility.RevealInFinder(UserPath(manager.settings.userIndex, LocalFolder));
            }
        }

        private static void DefaultParametersSettings(GestureManager manager)
        {
            manager.settings.isRemote = !GmgLayoutHelper.Toggle("IsLocal: ", !manager.settings.isRemote, manager);
            GUILayout.FlexibleSpace();
            manager.settings.vrMode = GmgLayoutHelper.Toggle("VRMode: ", manager.settings.vrMode, manager);
        }
    }
}