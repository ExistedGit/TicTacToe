using GameLibrary;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Server
{
    class Program
    {
        static TcpServerWrap server = new TcpServerWrap();
        static string Prompt(string text)
        {
            Console.Write(text);
            return Console.ReadLine();
        }
        static void ErrorMessage(string text)
        {
            Console.WriteLine(text);
            Console.ReadKey();
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            string input = Prompt("Введите порт для прослушивания: ");
            int port = -1;
            if(!int.TryParse(input, out port))
            {
                ErrorMessage("Неверные данные");
                return;
            }
            if(port < 0 || port > ushort.MaxValue)
            {
                ErrorMessage($"Номер порта должен быть от 0 до {ushort.MaxValue}");
                return;
            }

            Console.WriteLine("Запускаем сервер...");
            server.Started += Server_Started;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.MessageReceived += Server_MessageReceived;
            server.Start(port);
            
            Console.ReadKey();
        }

        private static void Server_ClientDisconnected(TcpClientWrap obj) => Console.WriteLine("Disconnected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());

        private static void Server_ClientConnected(TcpClientWrap obj) => Console.WriteLine("Connected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());

        private static void Server_MessageReceived(TcpClientWrap client, Message msg)
        {
            switch (msg.Type) {
                case MessageType.Text:
                    Console.WriteLine((client.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString() + ": " + (msg as TextMessage).ToString());
                    break;
                case MessageType.Custom:
                    if(msg is UserConnectMessage)
                    {
                        UserConnectMessage userConnect = msg as UserConnectMessage;
                        Console.WriteLine("User " + userConnect.UserName + " joined // " + (client.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());
                    }
                    break;
            }
        }

        private static void Server_Started(TcpServerWrap server)
        {
            Console.WriteLine("Сервер запущен");
        }

    }
}
