﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace MessageLibrary
{
    public class TcpClientWrap
    {

        private IPEndPoint endPoint;
        private TcpClient client;
        public TcpClient Tcp => client;

        public event Action<TcpClientWrap> Connected;
        public event Action<TcpClientWrap> Disconnected;

        public event MessageHandler MessageReceived;
        public event Action<Message> MessageSent;

        public TcpClientWrap(IPAddress ip, int port)
        {
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            client = null;
        }

        public void SendAsync(object gameInfoMessage)
        {
            throw new NotImplementedException();
        }

        public TcpClientWrap(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentException("Подключение не может быть пустым");

            client = tcpClient;

        }

        public bool Connect()
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
                Connected?.Invoke(this);
            
            return client.Connected;
        }
        /// <summary>
        /// Асинхронно подключает клиент к адресу
        /// </summary>
        /// <returns>Была ли начата операция подключения</returns>
        public bool ConnectAsync()
        {
            if (client != null)
                return false;
            try
            {
                client = new TcpClient();
                client.BeginConnect(endPoint.Address, endPoint.Port, ConnectCB, client);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public event Action<TcpClientWrap> ConnectFailed;
        private void ConnectCB(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception)
            {
                ConnectFailed?.Invoke(this);
            }
            if(client.Connected)
                Connected?.Invoke(this);
        }

        public void Disconnect()
        {
            Disconnected?.Invoke(this);
            client?.Close();
            client = null;
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
        /// <summary>
        /// Асинхронно отправляет сообщение клиента
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Была ли начата операция отправки</returns>
        public bool SendAsync(Message message) {
            if (client != null & client.Connected)
            {
                message.SendToAsync(Tcp.Client, SendCB);
                return true;
            }
            return false;
        }
        private void SendCB(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            state.Socket.EndSend(ar);
            MessageSent?.Invoke(Message.FromByteArray(state.Buffer));
        }

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
        public bool ReceiveAsync()
        {
            if (client != null & client.Connected)
            {
                Message.ReceiveFromSocket(Tcp.Client, ReceiveCB);
                return true;
            }
            return false;
        }
        private void ReceiveCB(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.Socket;
            try
            {
                int bytesRead = Tcp.Client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    Message msg = Message.FromByteArray(state.Buffer);
                    MessageReceived?.Invoke(this, msg);
                    socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCB, state);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("TcpClientWrap.Receive: SOCKET EXCEPTION");
                return;
            }
            
            //array = new byte[8192];
        }
    }
}
