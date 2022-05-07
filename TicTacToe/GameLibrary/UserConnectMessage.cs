using MessageLibrary;

namespace GameLibrary
{
    public class UserConnectMessage :  Message, ICustomMessage
    {
        public string UserName { get; set; }

        public UserConnectMessage(string userName)
        {
            UserName = userName;
            CustomType = "user_connect";
            Type = MessageType.Custom;
        }

        public string CustomType { get; private set; }
    }
}
