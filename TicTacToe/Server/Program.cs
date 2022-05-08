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
        static List<GameRoom> rooms = new List<GameRoom>();
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
            if (!int.TryParse(input, out port))
            {
                ErrorMessage("Неверные данные");
                return;
            }
            if (port < 0 || port > ushort.MaxValue)
            {
                ErrorMessage($"Номер порта должен быть от 0 до {ushort.MaxValue}");
                return;
            }

            Console.WriteLine("Запускаем сервер...");
            server.Started += Server_Started;
            server.ClientConnected += ClientConnected;
            server.ClientDisconnected += ClientDisconnected;
            server.MessageReceived += Server_MessageReceived;
            server.Start(port);

            Console.ReadKey();
        }

        private static void ClientDisconnected(TcpClientWrap obj) => Console.WriteLine("Disconnected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());

        private static void ClientConnected(TcpClientWrap obj)
        {
            Console.WriteLine("Connected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());

        }


        private static void Server_MessageReceived(TcpClientWrap client, Message msg)
        {
            GameInfoMessage m = msg as GameInfoMessage;
            if (msg is UserConnectMessage)
            {
                UserConnectMessage userConnect = msg as UserConnectMessage;
                Console.WriteLine("User " + userConnect.UserName + " joined // " + (client.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());
                Player player = new Player(userConnect.UserName, client);
                if (rooms.All(r => r.Full))
                {
                    GameRoom newRoom = new GameRoom();
                    newRoom.Player1 = player;
                    rooms.Add(newRoom);
                }
                else
                {
                    GameRoom room = rooms.First(r => !r.Full);
                    room.Player2 = player;
                    server.MessageReceived += room.MessageReceived;
                    room.PlayerLeft -= Room_PlayerLeft;
                    room.PlayerLeft += Room_PlayerLeft;
                    room.StartGame();
                }
            }

        }

        private static void Room_PlayerLeft(GameRoom room)
        {
            if (room.Player1 == null && room.Player2 == null)
            {
                rooms.Remove(room);
            } else if(room.Player1 == null ^ room.Player2==null) {
                server.MessageReceived -= room.MessageReceived;
            }
        }

        private static void Server_Started(TcpServerWrap server)
        {
            Console.WriteLine("Сервер запущен");
        }

    }
}
