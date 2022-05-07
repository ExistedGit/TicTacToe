using MessageLibrary;

namespace GameLibrary
{
    public class ServerMessage : Message
    {
        public string Message { get; set; }

        public ServerMessage(string Message)
        {
            Type = MessageType.Custom;
            this.Message = Message; 
        }

    }
}
