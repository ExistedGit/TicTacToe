using MessageLibrary;

namespace GameLibrary
{
    public class UserConnectMessage :  Message
    {
        public string Name;

        public UserConnectMessage(string Name)
        {
            this.Name = Name;
        }

    }
}
