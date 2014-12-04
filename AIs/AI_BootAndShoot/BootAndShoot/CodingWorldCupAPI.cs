using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    /// <summary>
    /// Helps manage the coding-world-cup API.
    /// 
    /// You can derive from this class and implement its abstract 
    /// functions in your API.
    /// </summary>
    class CodingWorldCupAPI
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CodingWorldCupAPI()
        {
            // We run the game loop...
            // The game loop...
            Logger.log("Starting game loop", Logger.LogLevel.INFO);
            for (; ; )
            {
                // We "listen" to game messages on stdin, and process them...
                var message = Console.ReadLine();
                processMessage(message);
            }
        }

            /// <summary>
        /// Called when we have received a new message from the game-engine.
        /// </summary>
        private void processMessage(string message)
        {
            Logger.log("Received message: " + message, Logger.LogLevel.DEBUG);
            
            // We decode the JSON message, and process it depending
            // on its message type...
            var jsonMessage = Json.Decode(message);
            string messageType = jsonMessage.messageType; 
            switch(messageType)
            {
                default:
                    throw new Exception("Unexpected messageType");
            }
        }
    }
}
}
