using MessageLibrary;

namespace GameLibrary
{
    public class UserConnectMessage : Message
    {
        public string UserName { get; set; }

        public UserConnectMessage(string userName)
        {
            UserName = userName;
            Type = MessageType.Custom;
        }
    }
}
