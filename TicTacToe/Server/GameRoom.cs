using GameLibrary;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameRoom
    {
        public static Random rng = new Random();
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Cell[,] Cells { get; private set; }
        public bool Full { get => Player1 != null && Player2 != null; }
        public uint Id { get; private set; } = IdCounter++;
        private static uint IdCounter = 0;
        public GameRoom()
        {
            Cells = new Cell[3, 3];
            for(int i =0; i< 3; i++)
                for(int j =0; j < 3; j++)
                    Cells[i, j] = new Cell(i, j);
        }
        public void StartGame()
        {
            bool turn = Convert.ToBoolean(rng.Next(0, 2));
            Player1.Client.SendAsync(new StartGameMessage(Player2.UserName, Id, turn, CellState.Cross));
            Player2.Client.SendAsync(new StartGameMessage(Player1.UserName, Id, !turn, CellState.Circle));
        }
        public void MessageReceived(TcpClientWrap client, Message msg)
        {
            if(msg is GameInfoMessage)
            {
                GameInfoMessage gameInfo = msg as GameInfoMessage;
                if(gameInfo.Id == Id)
                {
                    Player secondPlayer = Player1.Client.Tcp.Equals(client) ? Player2 : Player1;
                    Cell cell = gameInfo.UpdatedCell;
                    Cells[cell.X, cell.Y] = cell;
                    secondPlayer.Client.SendAsync(gameInfo);
                }
            }
        }

    }

}
