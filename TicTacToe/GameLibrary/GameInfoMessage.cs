using MessageLibrary;
using System;
namespace GameLibrary
{
    [Serializable]
    public class GameInfoMessage : Message
    {
        public Cell UpdatedCell { get; set; }
        public bool isGameEnded { get; set; }
        public bool isWinner { get; set; }


        public GameInfoMessage(Cell UpdatedCell)
        {
            this.UpdatedCell = UpdatedCell;
        }

        public GameInfoMessage(Cell UpdatedCell, bool isGameEnded, bool isWinner)
        {
            this.UpdatedCell = UpdatedCell;
            this.isGameEnded = isGameEnded;
            this.isWinner = isWinner;
        }

    }
}
