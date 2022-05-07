using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            server.Start(port);
            server.ClientConnected += Server_ClientConnected;
            server.MessageReceived += Server_MessageReceived;
            Console.ReadKey();
        }

        private static void Server_ClientConnected(System.Net.Sockets.TcpClient obj)
        {
            Console.WriteLine("Connected: " + (obj.Client.RemoteEndPoint as IPEndPoint).ToString());
            
        }

        private static void Server_MessageReceived(TcpClientWrap client, Message msg)
        {
            if(msg.Type== MessageType.Text)
                Console.WriteLine(msg as TextMessage);
        }

        private static void Server_Started(TcpServerWrap server)
        {
            Console.WriteLine("Сервер запущен");
        }
    }
}
