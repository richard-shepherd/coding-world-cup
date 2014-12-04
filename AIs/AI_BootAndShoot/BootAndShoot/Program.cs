using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                // We set up the log level, and play the game...
                Logger.setLogLevel(Logger.LogLevel.DEBUG);
                var game = new BootAndShoot();
            }
            catch (Exception ex)
            {
                Logger.log("Exception in Main: " + ex.Message, Logger.LogLevel.FATAL);
            }
        }
    }
}
