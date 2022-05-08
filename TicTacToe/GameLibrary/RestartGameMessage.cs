using MessageLibrary;
using System;

namespace GameLibrary
{
    [Serializable]
    public class RestartGameMessage : Message
    {
        public uint Id { get; set; }
        public bool NewEnemy { get; set; }
        public bool NewGame { get; set; }

        public RestartGameMessage(bool NewGame, bool NewEnemy = false)
        {
            this.NewEnemy = NewEnemy;
            this.NewGame = NewGame;
            Type = MessageType.Custom;
        }
    }
}
