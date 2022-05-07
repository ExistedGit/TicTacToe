using MessageLibrary;
using System;

namespace GameLibrary
{
    [Serializable]
    public class StartGameMessage : Message
    {
        public string EnemyUserName { get; set; }
        public bool IsYourTurn { get; set; }
        public CellState Cell { get; set; }
        public StartGameMessage(string EnemyUserName, bool IsYourTurn, CellState cell)
        {
            this.EnemyUserName = EnemyUserName;
            this.IsYourTurn = IsYourTurn;
            if (cell == CellState.Empty)
                throw new ArgumentException("Клетку игрока можно установить только как крестик или нолик");
            Cell = cell;
        }


    }
}
