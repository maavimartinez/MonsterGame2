using System;
using System.Net.Sockets;
using System.Text;

namespace Protocol
{
    public class Connection
    {
        private const int LengthByteSize = 4;
        private Socket Socket { get; set; }

        public Connection(Socket socket)
        {
            Socket = socket;
        }

        public void SendMessage(object[] message)
        {
            string serializedMessage = Serializer.Serialize(message);
            var data = Encoding.ASCII.GetBytes(serializedMessage);
            SendDataLength(data);
            SendData(data, data.Length);
        }

        public string[][][] ReadMessage()
        {
            var dataLength = ReadDataLength();
            var dataReceived = ReadData(dataLength);
            var message = Encoding.UTF8.GetString(dataReceived);
            return Serializer.DeSerialize(message);
        }

        public void Close()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        private int ReadDataLength()
        {
            var dataLengthAsBytes = ReadData(LengthByteSize);
            return BitConverter.ToInt32(dataLengthAsBytes, 0);
        }

        private byte[] ReadData(int dataLength)
        {
            var dataReceived = new byte[dataLength];
            var totalReceived = 0;
            while (totalReceived < dataLength)
            {
                var received = Socket.Receive(dataReceived, dataLength - totalReceived, SocketFlags.None);

                if(received == 0)
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }
            return dataReceived;
        }

        private void SendDataLength(byte[] data)
        {
            var length = data.Length;
            var dataLength = BitConverter.GetBytes(length);

            SendData(dataLength, LengthByteSize);
        }

        private void SendData(byte[] data, int dataLength)
        {
            var totalSent = 0;
            while (totalSent < dataLength)
            { 
                var sent = Socket.Send(data, totalSent, dataLength - totalSent, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                totalSent += sent;
            }
        }
        
    }
}