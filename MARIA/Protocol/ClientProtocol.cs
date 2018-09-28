using System;
using System.Net;
using System.Net.Sockets;

namespace Protocol
{
    public class ClientProtocol
    {
        private IPEndPoint ClientIpEndPoint { get; set; }
        private IPEndPoint ServerIpEndPoint { get; set; }
        private string securityToken;

        public ClientProtocol(string serverIp, int serverPort, string clientIp = "127.0.0.1", int clientPort = 0)
        {
            ClientIpEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), clientPort);
            ServerIpEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }

        public Connection ConnectToServer()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ClientIpEndPoint);
            socket.Connect(ServerIpEndPoint);

            return new Connection(socket);
        }
    }
}