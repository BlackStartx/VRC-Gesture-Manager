#if VRC_SDK_VRCSDK3
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class UdpSender
    {
        public string Address { get; }
        public int Port { get; }

        private readonly IPEndPoint _remoteIpEndPoint;
        private readonly Socket _socket;

        public UdpSender(string address, int port) : this(Dns.GetHostAddresses(address).FirstOrDefault(), port)
        {
            Address = address;
        }

        private UdpSender(IPAddress addressDns, int port)
        {
            Port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _remoteIpEndPoint = new IPEndPoint(addressDns, port);
        }

        public void Send(byte[] bytes) => _socket.SendTo(bytes, _remoteIpEndPoint);

        public void Close() => _socket.Close();
    }
}
#endif