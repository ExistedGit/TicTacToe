using MessageLibrary;
using System;
namespace GameLibrary
{
    [Serializable]
    public class GameInfoMessage : Message
    {
        public Cell UpdatedCell { get; set; }

        public GameInfoMessage(Cell UpdatedCell)
        {
            this.UpdatedCell = UpdatedCell;
        }

    }
}
