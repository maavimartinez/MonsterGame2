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
            var received = 0;
            while (received < dataLength)
            {
                received += Socket.Receive(dataReceived, dataLength - received, SocketFlags.None);
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
            var sent = 0;
            while (sent < dataLength)
            {
                sent += Socket.Send(data, sent, dataLength - sent, SocketFlags.None);
            }
        }
    }
}
