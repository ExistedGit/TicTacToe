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
            Task.Run(() =>
            {
                do
                {
                    TcpClient client = listener.AcceptTcpClient();
                    #region old method
                    /*
                    Console.WriteLine($"Connection " + client.Client.RemoteEndPoint);
                    BinaryFormatter bf = new BinaryFormatter();
                    using (NetworkStream ns = client.GetStream())
                    {
                        // read-write
                        #region v1 method
                        // v1 send string
                        // byte[] buf = Encoding.UTF8.GetBytes("Hello User!");
                        // ns.Write(buf, 0, buf.Length);

                        // v1 read message
                        //StringBuilder sb = new StringBuilder();
                        //byte[] buf = new byte[1024];
                        //int len;
                        //do
                        //{
                        //    len = ns.Read(buf, 0, buf.Length);
                        //    sb.Append(Encoding.UTF8.GetString(buf, 0, len));
                        //} while (ns.DataAvailable);
                        //Console.WriteLine(sb.ToString());
                        #endregion

                        // v2 send object
                        LanMessage message = new LanMessage("Hello User!");
                        bf.Serialize(ns, message);

                        // v2 read message
                        message = (LanMessage)bf.Deserialize(ns);
                        Console.WriteLine(message);
                    }
                    client.Close();
                    */
                    #endregion

                    Connected?.Invoke(client);
                    Task.Run(() => UserMessaging(client));
                } while (true);
            });
        }
        public event Action<TcpClientWrap, Message> MessageReceived;
        private void UserMessaging(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            do
            {
                TextMessage message = user.Receive() as TextMessage;
                if (message != null)
                {
                    if (message.Text.ToLower().Equals("/exit"))
                        break;
                }
                else
                    break;

                //Console.Write("enter answer>>>");
                //user.SendMessage(new LanMessage(Console.ReadLine()));
                MessageReceived?.Invoke(user, message);
            } while (true);

            user.CloseConnection();
        }
        public void Shutdown()
        {
            listener?.Stop();
        }
    }
}
