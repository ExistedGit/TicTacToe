using MessageLibrary;

namespace GameLibrary
{
    public class InfoStartGameMessage : Message
    {
        public string EnemyUserName { get; set; }
        public bool IsYourTurn { get; set; }

        public InfoStartGameMessage(string EnemyUserName, bool IsYourTurn)
        {
            this.EnemyUserName = EnemyUserName;
            this.IsYourTurn = IsYourTurn;
        }


    }
}
