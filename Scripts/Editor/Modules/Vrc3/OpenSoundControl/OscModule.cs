#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Vrc3Debug.Avatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class OscModule
    {
        private readonly ModuleVrc3 _module;
        private const int SquareSize = VisualEpStyles.SquareSize;

        private const string VrcReceiverAddress = "127.0.0.1";
        private const int VrcListenerPort = 9000;
        private const int VrcSenderPort = 9001;

        internal GmgLayoutHelper.Toolbar ToolBar;
        private readonly OscSettings _settings;

        private UdpSender _sender;
        private UdpListener _listener;
        private readonly List<byte[]> _queue = new();

        private bool _customSelection;
        private string _customAddress = VrcReceiverAddress;
        private int _customListener = VrcSenderPort;
        private int _customSender = VrcListenerPort;
        private (string address, int listenerPort, int senderPort)? _forgotData;

        private IEnumerable<OscPacket.Message> Messages => _messages.Select(tuple => new OscPacket.Message(tuple.address, tuple.data));
        private readonly List<(string address, List<object> data)> _messages = new();
        private bool _sendingBundle;
        private int _addIndex = -1;
        private ulong _timeTag = 1;
        private string _dateTag = OscPacket.TimeTagToDateTime(1).ToString("F");

        private static string VrcReceiverPortUsed => $"The VRChat default port {VrcListenerPort} is already in use.";
        internal bool Enabled => _listener != null && _sender != null;

        private readonly Dictionary<string, EndpointControl> _dataDictionary = new();
        private readonly List<EndpointControl> _chronological = new();

        public OscModule(ModuleVrc3 module)
        {
            _module = module;
            _settings = new OscSettings(_module);
        }

        public void Update()
        {
            lock (_queue)
            {
                if (_queue.Count == 0) return;
                foreach (var bytes in _queue) OnListenerBytes(bytes);
                _queue.Clear();
            }
        }

        public void ControlPanel()
        {
            if ((_listener != null || _sender != null) && !Enabled) Stop();

            GUILayout.Label("Osc Debug", GestureManagerStyles.GuiHandTitle);
            if (_module.DummyMode != null)
            {
                GUILayout.Space(28);
                GUILayout.Label($"Osc Debug is not available while you are in {_module.DummyMode.ModeName}-Mode!", GestureManagerStyles.Centered);
                GUILayout.Space(28);
            }
            else if (!Enabled)
            {
                GUILayout.Label("Osc Debug is disabled, start it with the button bellows!", GestureManagerStyles.Centered);

                GUILayout.Space(15);
                var isCustomMode = _customSelection && _settings.Stable;

                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    if (isCustomMode)
                    {
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label("Listening", GestureManagerStyles.GuiDebugTitle);
                            _customListener = EditorGUILayout.IntField(_customListener);
                        }

                        using (new GmgLayoutHelper.FlexibleScope())
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label("Sending", GestureManagerStyles.GuiDebugTitle);
                            using (new GUILayout.HorizontalScope())
                            {
                                _customAddress = EditorGUILayout.TextField(_customAddress);
                                _customSender = EditorGUILayout.IntField(_customSender);
                            }
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            var option = GUILayout.Height(20);
                            if (GUILayout.Button("Back!", option)) _customSelection = false;
                            using (new GmgLayoutHelper.GuiEnabled(_customListener >= 0 && _customSender >= 0))
                                if (GmgLayoutHelper.Button("Start!", Color.green, option))
                                    Start(_customListener, _customAddress, _customSender);
                        }
                    }
                    else
                    {
                        using (new GmgLayoutHelper.GuiEnabled(_settings.Stable))
                        {
                            if (GmgLayoutHelper.DebugButton("Start on VRChat ports")) Start(VrcListenerPort, VrcReceiverAddress, VrcSenderPort);
                            GUILayout.FlexibleSpace();
                            if (GmgLayoutHelper.DebugButton("Start on custom ports")) _customSelection = true;
                        }
                    }
                }

                GUILayout.Space(isCustomMode ? -2 : 11);
            }
            else
            {
                GUILayout.Label("Osc Debug is running, you can stop it with the orange button!", GestureManagerStyles.Centered);

                GUILayout.Space(4);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label("Listening To", GestureManagerStyles.GuiDebugTitle);
                        GUILayout.Label($"{_listener?.Port}", GestureManagerStyles.Centered);
                    }

                    using (new GmgLayoutHelper.FlexibleScope())
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Space(8);
                        if (GmgLayoutHelper.Button("Stop", RadialMenuUtility.Colors.RestartButton, GUILayout.Height(30), GUILayout.Width(60))) Stop();
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label("Sending To", GestureManagerStyles.GuiDebugTitle);
                        GUILayout.Label($"{_sender?.Port}", GestureManagerStyles.Centered);
                    }
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(9);

            using (new GUILayout.HorizontalScope())
            using (new GmgLayoutHelper.FlexibleScope())
                if (GmgLayoutHelper.DebugButton(!_module.DebugOscWindow ? Vrc3AvatarDebugWindow.Text.D.Button : Vrc3AvatarDebugWindow.Text.W.Button))
                    _module.SwitchDebugOscView();

            GUILayout.Space(6);
        }

        private void Start(int listenerPort, string sendAddress, int senderPort)
        {
            if (!_settings.Setup()) return;

            try
            {
                _listener = new UdpListener(listenerPort, AddQueue);
                _sender = new UdpSender(sendAddress, senderPort);
                _customSelection = false;
            }
            catch (Exception)
            {
                var messageString = listenerPort == VrcListenerPort ? VrcReceiverPortUsed : $"The port {listenerPort} is already in use.";
                EditorUtility.DisplayDialog("Gesture Manager", messageString, ":c");
            }
        }

        internal void Stop(bool clear = true)
        {
            _listener?.Close();
            _listener = null;
            _sender?.Close();
            _sender = null;
            if (!clear) return;
            lock (_queue) _queue.Clear();
            ClearElements();
        }

        private void ClearElements()
        {
            foreach (var endpointControl in _chronological) endpointControl.Clear();
            _dataDictionary.Clear();
            _chronological.Clear();
        }

        public void Forget()
        {
            _forgotData = (_sender.Address, _listener.Port, _sender.Port);
            Stop(false);
        }

        public void Resume()
        {
            if (_forgotData == null) return;
            Start(_forgotData.Value.listenerPort, _forgotData.Value.address, _forgotData.Value.senderPort);
            _forgotData = null;
        }

        private void AddQueue(byte[] bytes)
        {
            lock (_queue) _queue.Add(bytes);
        }

        private void OnListenerBytes(byte[] bytes)
        {
            foreach (var message in OscPacket.GetMessages(bytes)) OnMessage(message);
        }

        public void DebugLayout(VisualElement root, VisualEpContainer holder, float width)
        {
            if (_listener == null || _sender == null) SettingsLayout();
            else Layout(root, holder, width);
        }

        private void SettingsLayout()
        {
            if (!_settings.Loaded)
            {
                GUILayout.Label("Settings", GestureManagerStyles.Header);
                GUILayout.Space(5);
                GUILayout.Label("You can start the OSC debug right now and it will use wide settings!", GestureManagerStyles.Centered);
                GUILayout.Space(10);
                GUILayout.Label("Or you can load your OSC settings!", GestureManagerStyles.Centered);
                GUILayout.Space(30);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                    if (GmgLayoutHelper.DebugButton("Load Settings"))
                        _settings.Load();

                GUILayout.Space(20);
            }
            else if (_settings.Layout()) _settings.Clean();
        }

        private void Layout(VisualElement root, VisualEpContainer holder, float width)
        {
            GmgLayoutHelper.MyToolbar(ref ToolBar, new (string, Action)[]
            {
                ("Receive", () => ReceiveLayout(root, holder, width)),
                ("Send", SendLayout)
            });
        }

        private void ReceiveLayout(VisualElement root, VisualEpContainer holder, float width)
        {
            holder.Render(root);
            if (holder.parent != root) root.Add(holder);
            GUILayout.Label("Received Osc Packet Messages", GestureManagerStyles.Header);

            if (_chronological.Count != 0) GmgLayoutHelper.HorizontalGrid(holder, width, SquareSize, SquareSize, 5, _chronological, ShowData);
            else NoDataLayout();
        }

        private static void ShowData(VisualEpContainer holder, Rect rect, EndpointControl element)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Used) return;

            holder.RenderMessage(rect, element);
        }

        private void SendLayout()
        {
            if (GmgLayoutHelper.TitleButton($"Send Custom {(_sendingBundle ? "Bundle" : "Message")}", _sendingBundle ? "Message" : "Bundle")) _sendingBundle = !_sendingBundle;

            if (_messages.Count == 0) AddMessage();
            if (_sendingBundle) BundleLayout();
            else SingleMessageLayout();

            GUILayout.Space(15);
            using (new GUILayout.HorizontalScope())
            using (new GmgLayoutHelper.FlexibleScope())
                if (GmgLayoutHelper.DebugButton("Send"))
                    _sender.Send(_sendingBundle ? new OscPacket(_timeTag, Messages.ToList()).GetBytes() : Messages.First().GetBytes());
        }

        private void AddMessage() => _messages.Add(("/avatar/parameters/", new List<object>()));

        private void SingleMessageLayout()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) MessageLayout(0);
        }

        private void BundleLayout()
        {
            if (_timeTag != (_timeTag = GmgLayoutHelper.ULongField("TimeTag: ", _timeTag))) OnNewTimeTag();
            GUILayout.Label(_dateTag, EditorStyles.helpBox);
            for (var i = 0; i < _messages.Count; i++) MessageBundleLayout(ref i);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                if (GUILayout.Button("New Osc Message"))
                    AddMessage();
        }

        private void OnNewTimeTag() => _dateTag = OscPacket.TimeTagToDateTime(_timeTag).ToString("F");

        private void MessageBundleLayout(ref int i)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GmgLayoutHelper.TitleButton($"Osc Message [{i}]", "-", 20))
                {
                    _messages.RemoveAt(i);
                    i--;
                }
                else MessageLayout(i);
            }
        }

        private void MessageLayout(int mId)
        {
            var (address, data) = _messages[mId];

            address = EditorGUILayout.TextField("Address: ", address);
            for (var i = 0; i < data.Count; i++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    switch (data[i])
                    {
                        case bool boolObject:
                            data[i] = EditorGUILayout.Toggle("Bool: ", boolObject);
                            break;
                        case float floatObject:
                            data[i] = EditorGUILayout.FloatField("Float: ", floatObject);
                            break;
                        case char charObject:
                            data[i] = (char)EditorGUILayout.IntField("Char: ", charObject);
                            GUILayout.Label(charObject.ToString(), GUILayout.Width(50));
                            break;
                        case string stringObject:
                            data[i] = EditorGUILayout.TextField("String: ", stringObject);
                            break;
                        case int intObject:
                            data[i] = EditorGUILayout.IntField("Int: ", intObject);
                            break;
                        default:
                            data.RemoveAt(i);
                            i--;
                            continue;
                    }

                    if (!GUILayout.Button("-", GUILayout.Width(20))) continue;
                    data.RemoveAt(i);
                    i--;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                if (_addIndex != mId)
                {
                    EditorGUILayout.LabelField("", "");
                    if (GUILayout.Button("+", GUILayout.Width(20))) _addIndex = mId;
                }
                else
                {
                    if (GUILayout.Button("Add Boolean")) Add(data, false);
                    if (GUILayout.Button("Add Integer")) Add(data, new int());
                    if (GUILayout.Button("Add Float")) Add(data, new float());
                    if (GUILayout.Button("Add String")) Add(data, "");
                    if (GUILayout.Button("Add Char")) Add(data, 'a');
                    if (GUILayout.Button("x", GUILayout.Width(20))) _addIndex = -1;
                }
            }

            _messages[mId] = (address, data);
        }

        private void Add(ICollection<object> data, object value)
        {
            data.Add(value);
            _addIndex = -1;
        }

        private void NoDataLayout() => ErrorLayout("No data received yet!", $"Is your application sending on port {_listener.Port}?");

        private static void ErrorLayout(string red, string white)
        {
            using (new GmgLayoutHelper.FlexibleScope())
            {
                GUILayout.Space(100);
                GUILayout.Label(red, GestureManagerStyles.TextError);
                GUILayout.Space(20);
                GUILayout.Label(white, GestureManagerStyles.Centered);
                GUILayout.Space(100);
            }
        }

        private void OnMessage(OscPacket.Message message)
        {
            _settings.OnMessage(message);
            if (!_dataDictionary.TryGetValue(message.Address, out var control)) _dataDictionary[message.Address] = control = new EndpointControl(message.Address, _chronological);
            control.OnMessage(message);
        }

        public void OnParameterChange(Vrc3Param param, float value)
        {
            if (_sender == null) return;
            var message = _settings.OnParameterChange(param, value);
            if (message == null) return;
            _sender.Send(message.GetBytes());
        }
    }
}
#endif