﻿using MessageLibrary;
using System;
namespace GameLibrary
{
    [Serializable]
    public class GameInfoMessage : Message
    {
        public Cell UpdatedCell { get; set; }
        public bool IsGameOver { get; set; }
        public GameResult Result { get; set; }
        public uint Id { get; set; }


        public GameInfoMessage(Cell UpdatedCell, uint roomId)
        {
            this.UpdatedCell = UpdatedCell;
            Type = MessageType.Custom;
            Id = roomId;
        }

        public GameInfoMessage(Cell UpdatedCell, uint roomId, bool isGameEnded, GameResult Result)
        {
            this.UpdatedCell = UpdatedCell;
            this.IsGameOver = isGameEnded;
            this.Result = Result;
            Id = roomId;
            Type = MessageType.Custom;
            
        }

    }
}
