using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrillTown
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
                var game = new BrillTown();
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception in Main: {0} \nStack:\n{1}", ex.Message, ex.StackTrace);
                Logger.log(message, Logger.LogLevel.FATAL);
            }
        }
    }
}
