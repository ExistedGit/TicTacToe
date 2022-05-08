using GameLibrary;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                    Cells[i, j] = new Cell(i + 1, j + 1);
        }
        const bool DEBUG = true;
        private Player WhoseTurn;
        public void StartGame()
        {
            bool turn = Convert.ToBoolean(rng.Next(0, 2));
            if (DEBUG) turn = DEBUG;
            WhoseTurn = turn ? Player1 : Player2;
            
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
                    
                    Cell cell = gameInfo.UpdatedCell;
                    Console.WriteLine(WhoseTurn.UserName+ ": " + gameInfo.UpdatedCell.X + " " + gameInfo.UpdatedCell.Y);
                    //Cells[cell.X - 1, cell.Y - 1] = cell;
                    WhoseTurn = WhoseTurn == Player1 ? Player2 : Player1;
                    WhoseTurn.Client.SendAsync(gameInfo);
                }
            }
        }

    }

}
