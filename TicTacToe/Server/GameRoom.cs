using GameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameRoom
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Cell[,] Cells { get; private set; }
        public bool Full { get => Player1 != null && Player2 != null; }
        public GameRoom()
        {
            Cells = new Cell[3, 3];
            for(int i =0; i< 3; i++)
                for(int j =0; j < 3; j++)
                    Cells[i, j] = new Cell(i, j);
        }
        public void StartGame()
        {
            
        }

    }

}
