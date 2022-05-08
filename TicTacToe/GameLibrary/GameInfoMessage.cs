using MessageLibrary;
using System;
namespace GameLibrary
{
    [Serializable]
    public class GameInfoMessage : Message
    {
        public Cell UpdatedCell { get; set; }
        public GameResult Result { get; set; } = GameResult.None;
        public uint Id { get; set; }


        public GameInfoMessage(Cell UpdatedCell, uint roomId)
        {
            this.UpdatedCell = UpdatedCell;
            Type = MessageType.Custom;
            Id = roomId;
        }
        public GameInfoMessage(Cell UpdatedCell, uint roomId, GameResult Result)
        {
            this.UpdatedCell = UpdatedCell;
            this.Result = Result;
            Type = MessageType.Custom;            
            Id = roomId;
        }

    }
}
