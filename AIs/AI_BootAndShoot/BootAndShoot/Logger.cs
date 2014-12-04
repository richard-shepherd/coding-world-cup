using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    /// <summary>
    /// Logs messages to a log file.
    /// </summary>
    static class Logger
    {
        // An enum for log levels...
        public enum LogLevel
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR,
            FATAL
        }

        /// <summary>
        /// Constructor
        /// </summary>
        static Logger()
        {
            Directory.CreateDirectory("log");
        }

        /// <summary>
        /// Sets the log level
        /// </summary>
        public static void setLogLevel(LogLevel logLevel)
        {
            m_logLevel = logLevel;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        public static void log(string message, LogLevel logLevel)
        {
            if(logLevel < m_logLevel)
            {
                return;
            }

            var messageToLog = String.Format("{0},{1}: {2}", DateTime.Now.ToString(), logLevel.ToString(), message);
            var lines = new List<string>();
            lines.Add(messageToLog);

            //File.WriteAllLines("log/BootAndShoot.log", lines);
            File.AppendAllLines("log/BootAndShoot.log", lines);
        }

        // The current log level...
        private static LogLevel m_logLevel = LogLevel.INFO;
    }
}
