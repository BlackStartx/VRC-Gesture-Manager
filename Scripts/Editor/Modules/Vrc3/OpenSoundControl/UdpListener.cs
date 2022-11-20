#if VRC_SDK_VRCSDK3
using System;
using System.Net;
using System.Net.Sockets;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class UdpListener
    {
        internal int Port { get; }

        private readonly UdpClient _listener;
        private IPEndPoint _remoteIpEndPoint;

        private bool _closing;

        private readonly Action<byte[]> _onData;

        public UdpListener(int port, Action<byte[]> callback)
        {
            Port = port;
            _onData = callback;
            _listener = new UdpClient(port);
            _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
            _listener?.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                OnData(_listener.EndReceive(result, ref _remoteIpEndPoint));
            }
            catch (ObjectDisposedException)
            {
            }

            if (!_closing) _listener.BeginReceive(ReceiveCallback, null);
        }

        private void OnData(byte[] endReceive)
        {
            try
            {
                _onData(endReceive);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Close()
        {
            _closing = true;
            _listener.Close();
        }
    }
}
#endif