using GameLibrary;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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

            handler = new ConsoleEventHandler(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            server.Start(port);

            Console.ReadKey();
            OnExit();
            Console.WriteLine("waiting for messages...");
            DisconnectWaiter.WaitOne();
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                OnExit();
                DisconnectWaiter.WaitOne();
            }
            return false;
        }
        static ConsoleEventHandler handler;
        private static int playersLeft = 0;

        private delegate bool ConsoleEventHandler(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventHandler callback, bool add);
        private static ManualResetEvent DisconnectWaiter = new ManualResetEvent(false);
        private static int PlayersLeft { 
            get => playersLeft;
            set
            {
                playersLeft = value;
                if (playersLeft == rooms.Sum(
                    r =>
                    Convert.ToInt32(r.Player1 != null) +
                    Convert.ToInt32(r.Player2 != null)))
                    DisconnectWaiter.Set();
            }
        }
        private static void OnExit()
        {
            if(rooms.Count == 0)
            {
                DisconnectWaiter.Set();
                return;
            }

            foreach (var room in rooms)
            {
                if (room.Player1 != null)
                    room.Player1.Client.MessageSent += (msg) =>
                    {
                        if (msg is UserDisconnectMessage)
                            PlayersLeft++;
                    };
                if (room.Player2 != null)
                    room.Player2.Client.MessageSent += (msg) =>
                    {
                        if (msg is UserDisconnectMessage)
                            PlayersLeft++;
                    };
                room.Player1?.Client.SendAsync(new UserDisconnectMessage());
                room.Player2?.Client.SendAsync(new UserDisconnectMessage());
            }

        }

        private static void ClientDisconnected(TcpClientWrap obj)
        {
            Console.WriteLine("Disconnected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());
            GameRoom room = rooms.First(r => 
            r.Player1.Client.Tcp.Client.RemoteEndPoint.ToString() == 
            obj.Tcp.Client.RemoteEndPoint.ToString());
            if(room == null)
                room = rooms.First(r =>
             r.Player2.Client.Tcp.Client.RemoteEndPoint.ToString() ==
             obj.Tcp.Client.RemoteEndPoint.ToString());
            else
            {
                room.Player1 = null;
                rooms.Remove(room);
            }


            if(room != null)
            {
                room.Player2 = null;
            }
        }
        private static void ClientConnected(TcpClientWrap obj)
        {
            Console.WriteLine("Connected: " + (obj.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());
        }


        private static void Server_MessageReceived(TcpClientWrap client, Message msg)
        {
            if (msg is UserConnectMessage)
            {
                UserConnectMessage userConnect = msg as UserConnectMessage;
                Console.WriteLine("USER " + userConnect.UserName + " connected with IP: " + (client.Tcp.Client.RemoteEndPoint as IPEndPoint).ToString());
                Player player = new Player(userConnect.UserName, client);
                if (rooms.All(r => r.Full))
                {
                    GameRoom newRoom = new GameRoom();
                    newRoom.Player1 = player;
                    rooms.Add(newRoom);
                    Console.WriteLine($"[ROOM {newRoom.Id}]: USER {player.UserName} placed in a new room");
                }
                else
                {
                    GameRoom room = rooms.First(r => !r.Full);
                    room.Player2 = player;
                    Console.WriteLine($"[ROOM {room.Id}]: USER {player.UserName} joined existing room");
                    server.MessageReceived -= room.MessageReceived;
                    server.MessageReceived += room.MessageReceived;
                    room.FindNewRoom -= Room_FindNewRoom;
                    room.FindNewRoom += Room_FindNewRoom;
                    room.PlayerLeft -= Room_PlayerLeft;
                    room.PlayerLeft += Room_PlayerLeft;
                    room.StartGame();
                }

            }

        }

        private static void Room_FindNewRoom(GameRoom sender, Player player)
        {
            if (rooms.Count(r => r.Id != sender.Id) == 0 || rooms.All(r => r.Id != sender.Id && r.Full))
            {

                GameRoom newRoom = new GameRoom();
                newRoom.Player1 = player;
                rooms.Add(newRoom);

                Console.WriteLine($"[ROOM {sender.Id}]: USER {player.UserName} left to a new room {newRoom.Id}");
            }
            else
            {
                GameRoom room = rooms.First(r => r.Id != sender.Id && !r.Full);
                room.Player2 = player;
                Console.WriteLine($"[ROOM {sender.Id}]: USER {player.UserName} left to an existing room {room.Id}");
                server.MessageReceived -= room.MessageReceived;
                server.MessageReceived += room.MessageReceived;
                room.FindNewRoom -= Room_FindNewRoom;
                room.FindNewRoom += Room_FindNewRoom;
                room.PlayerLeft -= Room_PlayerLeft;
                room.PlayerLeft += Room_PlayerLeft;
                room.StartGame();
            }
        }



        private static void Room_PlayerLeft(GameRoom room)
        {
            Console.WriteLine($"[ROOM {room.Id}]: {(room.Player1 == null ? "last " : "")}user left");
            if (room.Player1 == null)
            {
                rooms.Remove(room);
                Console.WriteLine($"[ROOM {room.Id}]: empty room removed from room pool");
            }
            else
                server.MessageReceived -= room.MessageReceived;

        }

        private static void Server_Started(TcpServerWrap server)
        {
            Console.WriteLine("Сервер запущен");
        }

    }
}
