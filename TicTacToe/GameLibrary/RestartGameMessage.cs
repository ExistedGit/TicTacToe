using MessageLibrary;
using System;

namespace GameLibrary
{
    [Serializable]
    public class RestartGameMessage : Message
    {
        public bool NewEnemy;
        public bool NewGame;

        public RestartGameMessage(bool NewGame, bool NewEnemy = false)
        {
            this.NewEnemy = NewEnemy;
            this.NewGame = NewGame;
            Type = MessageType.Custom;
        }


    }
}
