using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
    public class TcpServerWrap
    {

        public event Action<TcpClient> Connected;
        private TcpListener listener;
        public void Start(int port)
        {
            if (listener != null)
                return;

            listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start(10);
            Task.Run(() => {
                do
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Connected?.Invoke(client);
                    Task.Run(() => Receive(client));
                } while (true);
                });
        }
        public event Action<TcpClientWrap, Message> MessageReceived;
        private void Receive(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            do
            {
                Message message = user.Receive();
                MessageReceived?.Invoke(user, message);
            } while (user.Tcp.Client.Available > 0);
            user.Disconnect();
        }
        public void Shutdown()
        {
            listener?.Stop();
        }
    }
}
