using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageLibrary
{
    [Serializable]
    public abstract partial class Message
    {
        private static BinaryFormatter bf = new BinaryFormatter();
        public MessageType Type { get; protected set; } = MessageType.Undefined;
        public DateTime Time { get; protected set; }
        public byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            ms.Position = 0;
            return ms.ToArray();
        }

        public void SendToAsync(Socket socket, AsyncCallback cb) {
            byte[] array = ToByteArray();
            socket.BeginSend(array, 0, array.Length, SocketFlags.None, cb, socket);
        }
        public void StreamTo(Stream stream) => bf.Serialize(stream, this);

        public static void ReceiveFromSocket(Socket socket, AsyncCallback cb)
        {
            byte[] buffer = new byte[8192];
            socket.BeginReceive(buffer, 0, 8192, SocketFlags.None, cb, new ValueTuple<Socket, byte[]>(socket, buffer));
        }

        public static Message FromNetworkStream(NetworkStream stream) => bf.Deserialize(stream) as Message;

        public static T FromNetworkStream<T>(NetworkStream stream) where T: Message => bf.Deserialize(stream) as T;
        
        public static Message FromByteArray(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            return bf.Deserialize(ms) as Message;
        }
        public static T FromByteArray<T> (byte[] buffer) where T : Message
        {
            MemoryStream ms = new MemoryStream(buffer);
            return bf.Deserialize(ms) as T;
        }
    }
}
