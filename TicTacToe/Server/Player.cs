using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Player
    {
        public string UserName { get; set; }
        public TcpClientWrap Client { get; set; }

        public Player(string name, TcpClientWrap client)
        {
            UserName = name;
            Client = client;
        }
    }
}
