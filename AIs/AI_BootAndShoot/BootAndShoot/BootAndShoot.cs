using System;
using System.Collections.Generic;
using System.Text;

namespace BootAndShoot
{
    /// <summary>
    /// An coding-world-cup API which:
    /// - Keeps two players in its own half as defenders.
    /// - Tries to keep the players in "zones"
    /// - Kicks the ball forward into another player's zone if further than 30 from the goal.
    /// - Shoots at goal if within 30m.
    /// </summary>
    class BootAndShoot
    {
        /// <summary>
        /// Main
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                // We set up the log level...
                Logger.setLogLevel(Logger.LogLevel.DEBUG);

                // And play the game...
                var game = new BootAndShoot();
                game.play();
            }
            catch(Exception ex)
            {
                Logger.log("Exception in Main: " + ex.Message, Logger.LogLevel.FATAL);
            }
        }

        /// <summary>
        /// Plays the game loop.
        /// </summary>
        private void play()
        {
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
            
        }
    }
}
