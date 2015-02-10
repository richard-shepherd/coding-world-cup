using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class Messages
    {
        public enum MessageType
        {
            Msg_ReceiveBall,
            Msg_PassToMe,
            Msg_SupportAttacker,
            Msg_GoHome,
            Msg_Wait
        };

        public string MessageToString(MessageType msg)
        {
            switch (msg)
            {
                case MessageType.Msg_ReceiveBall:

                    return "Msg_ReceiveBall";

                case MessageType.Msg_PassToMe:

                    return "Msg_PassToMe";

                case MessageType.Msg_SupportAttacker:

                    return "Msg_SupportAttacker";

                case MessageType.Msg_GoHome:

                    return "Msg_GoHome";

                case MessageType.Msg_Wait:

                    return "Msg_Wait";

                default:

                    return "INVALID MESSAGE!!";
            }
        }
    }
}
