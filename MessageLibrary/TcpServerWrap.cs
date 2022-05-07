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
        public event Action<TcpServerWrap> Started;
        public event Action<TcpClientWrap> ClientConnected;
        public event Action<TcpClientWrap> ClientDisconnected;
        private TcpListener listener;

        public void Start(int port)
        {
            if (listener != null)
                return;

            listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start(10);
            Started?.Invoke(this);
            ClientDisconnected += (client) => {
                
            };
            Task.Run(() => {
                do
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ReceiveAsync(client);
                } while (true);
            });
        }
        public event Action<TcpClientWrap, Message> MessageReceived;
        private void Receive(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            ClientConnected?.Invoke(user);
            user.Disconnected += ClientDisconnected;
            user.MessageReceived += MessageReceived;
            do
            {
                user.Receive();
            } while (user.Tcp.Client.Available > 0);
            user.Disconnect();
        }
        private void ReceiveAsync(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            ClientConnected?.Invoke(user);
            user.Disconnected += ClientDisconnected;
            user.MessageReceived += MessageReceived;

            user.ReceiveAsync();
        }

        

        public void Shutdown()
        {
            listener?.Stop();
        }
    }
}
