#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Cache;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class OscSettings
    {
        private readonly ModuleVrc3 _module;

        private static string VrcDirectory => GestureManagerSettings.VrcDirectory;
        private static string OscDirectory => Path.Combine(VrcDirectory, "OSC");

        public bool Stable => !Loaded || _fileProblem == null && _userProblem == null && _fileExist;

        public bool Loaded;
        private InputOutputData _data;

        private string _filePathString;
        private string _fileNameString;

        private string[] _users;
        private int _selectedUser;

        private string _userProblem;
        private string _fileProblem;

        private bool _fileExist;

        public OscSettings(ModuleVrc3 module)
        {
            _module = module;
        }

        public void Load()
        {
            Loaded = true;
            _userProblem = TryLoadUsers();
            if (_userProblem == null) LoadUser();
        }

        private void LoadUser() => _fileProblem = TryLoadFile();

        public void Clean() => Loaded = false;

        private string TryLoadUsers()
        {
            if (string.IsNullOrEmpty(_module.Pipeline.blueprintId)) return "The current avatar doesn't have a blueprint ID.";
            if (!Directory.Exists(VrcDirectory)) return "VRChat's directory cannot be found.";
            if (!Directory.Exists(OscDirectory)) return "VRChat's OSC directory cannot be found.";
            _users = Directory.GetDirectories(OscDirectory).Select(Path.GetFileName).ToArray();
            var vString = GestureManagerSettings.UserID(_module.Settings.userIndex);
            _selectedUser = Array.IndexOf(_users, vString);
            return _users.Length == 0 ? "No users found in VRChat's OSC Directory." : null;
        }

        private string TryLoadFile()
        {
            if (_selectedUser == -1) return "No default user found.";
            _fileNameString = $"{_module.Pipeline.blueprintId}.json";
            _filePathString = Path.Combine(OscDirectory, _users[_selectedUser], "Avatars", _fileNameString);
            return !File.Exists(_filePathString) ? "OSC settings for the avatar cannot be found in this user folder." : null;
        }

        public bool Layout()
        {
            if (GmgLayoutHelper.TitleButton("File Settings", "X", 20)) return true;
            if (_userProblem != null) UserProblemLayout();
            else UserLayout();
            return false;
        }

        private void UserLayout()
        {
            if (_selectedUser != (_selectedUser = EditorGUILayout.Popup("User: ", _selectedUser, _users))) LoadUser();
            if (_fileProblem != null) FileProblemLayout();
            else IntegrityLayout();
        }

        private void IntegrityLayout()
        {
            _fileExist = File.Exists(_filePathString);
            if (_fileExist) FileLayout();
            else UserProblemLayout(30, "The file has been deleted...", 10);
        }

        private void FileLayout()
        {
            GUILayout.Label("Loaded File", GestureManagerStyles.Header);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(10);
                GUILayout.Label($"{_fileNameString}", GestureManagerStyles.Centered);
                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    if (GmgLayoutHelper.DebugButton("Open in text editor!")) EditorUtility.OpenWithDefaultApp(_filePathString);
                    GUILayout.FlexibleSpace();
                    if (GmgLayoutHelper.DebugButton("Show in Explorer")) EditorUtility.RevealInFinder(_filePathString);
                }

                GUILayout.Space(15);
            }
        }

        private void UserProblemLayout() => UserProblemLayout(20, _userProblem, 20);

        private void FileProblemLayout() => UserProblemLayout(30, _fileProblem, 10);

        private static void UserProblemLayout(int pixelLeft, string text, int pixelRight)
        {
            GUILayout.Space(pixelLeft);
            GUILayout.Label(text, GestureManagerStyles.TextError);
            GUILayout.Space(pixelRight);
        }

        /*
         * Input & Outputs
         */

        public void OnMessage(OscPacket.Message message)
        {
            if (_data.Input.TryGetValue(message.Address, out var tuple)) tuple.execute(message.Arguments);
        }

        public OscPacket.Message OnParameterChange(Vrc3Param param, float value)
        {
            return !_data.Output.TryGetValue(param.Name, out var tuple) ? null : Message(tuple.address, tuple.type, value);
        }

        /*
         * Configuration
         */

        public bool Setup()
        {
            _data = new InputOutputData();
            return Loaded ? CustomSetup() : DefaultSetup();
        }

        private bool DefaultSetup()
        {
            foreach (var pair in _module.Params) _data.Input[$"/avatar/parameters/{pair.Key}"] = (pair.Value.Type, dataList => Execute(pair.Value, pair.Value.Type, dataList[0]));
            foreach (var pair in _module.Params) _data.Output[pair.Key] = ($"/avatar/parameters/{pair.Key}", pair.Value.Type);
            return true;
        }

        private bool CustomSetup()
        {
            try
            {
                return CustomSetup(JsonUtility.FromJson<OscFile>(File.ReadAllText(_filePathString)));
            }
            catch
            {
                const string message = "Failed to parse Json data.\n\nCheck if your file contains invalid Json data.";
                EditorUtility.DisplayDialog("Json Problem", message, "Ok");
                return false;
            }
        }

        private bool CustomSetup(OscFile file)
        {
            foreach (var parameter in file.InputParameters) _data.Input[parameter.input.address] = (parameter.input.Type, dataList => Execute(_module.GetParam(parameter.name), parameter.input.Type, dataList[0]));
            foreach (var parameter in file.OutputParameters) _data.Output[parameter.name] = (parameter.output.address, parameter.output.Type);
            return true;
        }

        private void Execute(Vrc3Param param, AnimatorControllerParameterType type, object value)
        {
            if (param == null) return;

            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    param.Set(_module, EndpointControl.FloatValue(value));
                    break;
                case AnimatorControllerParameterType.Int:
                    param.Set(_module, EndpointControl.IntValue(value));
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    param.Set(_module, EndpointControl.BoolValue(value));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static OscPacket.Message Message(string address, AnimatorControllerParameterType type, float value) => type switch
        {
            AnimatorControllerParameterType.Float => new OscPacket.Message(address, value),
            AnimatorControllerParameterType.Int => new OscPacket.Message(address, (int)value),
            AnimatorControllerParameterType.Bool => new OscPacket.Message(address, value > 0.5),
            AnimatorControllerParameterType.Trigger => new OscPacket.Message(address, value > 0.5),
            _ => throw new ArgumentOutOfRangeException()
        };

        private class InputOutputData
        {
            internal readonly Dictionary<string, (AnimatorControllerParameterType type, Action<IList<object>> execute)> Input = new();
            internal readonly Dictionary<string, (string address, AnimatorControllerParameterType type)> Output = new();
        }
    }
}
#endif