using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    [Serializable]
    public class UserDisconnectMessage : Message
    {
        public UserDisconnectMessage() {
            Type = MessageType.Custom;
        }
    }
}
