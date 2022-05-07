using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MessageLibrary
{
    public class TcpClientWrap
    {
        private IPEndPoint endPoint;
        private TcpClient client;

        public TcpClient Client => client;

        public event Action Connected;
        public event Action Disconnected;

        public event Action<TcpClientWrap, Message> MessageReceived;
        public event Action<Message> MessageSent;

        public TcpClientWrap(IPAddress ip, int port)
        {
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            client = null;
        }
        public TcpClientWrap(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentException("Подключение не может быть пустым");

            client = tcpClient;
        }


        public bool StartConnection()
        {
            if (client != null)
                return client.Connected;
            try
            {
                client = new TcpClient();
                client.Connect(endPoint);
            }
            catch (Exception)
            {
                return false;
            }
            if (client.Connected)
                Connected?.Invoke();
            
            return client.Connected;
            
        }


        public void CloseConnection()
        {
            client?.Close();
            client = null;
            Disconnected?.Invoke();
        }

        public bool Send(Message message)
        {
            if (client != null & client.Connected)
            {
                message.StreamTo(client.GetStream());
                MessageSent?.Invoke(message);
                return true;
            }
            return false;
        }

        public Task<bool> SendAsync(Message message) => Task.Run(() => Send(message));
        public Message Receive()
        {
            if (client != null & client.Connected)
            {
                var message = Message.FromNetworkStream(client.GetStream());
                MessageReceived?.Invoke(this, message);
                return message ;
            }
            return null;
        }
        public Task<Message> ReceiveAsync() => Task.Run(Receive);
    }
}
