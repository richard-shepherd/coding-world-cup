﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GermanyYogesh
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
            Logger.filePath = String.Format("log/log_{0}.txt", Process.GetCurrentProcess().Id);
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

            var messageToLog = String.Format("{0}: {1}", logLevel.ToString(), message);
            var lines = new List<string>();
            lines.Add(messageToLog);

            File.AppendAllLines(Logger.filePath, lines);
        }

        // The current log level...
        private static LogLevel m_logLevel = LogLevel.INFO;

        // The file name we're logging to...
        private static string filePath = "";
    }
}
