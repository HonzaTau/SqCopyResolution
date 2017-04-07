using System;
using SqCopyResolution.Model;

namespace SqCopyResolution.Services
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
            LogLevel = LogLevel.Info;
        }

        public LogLevel LogLevel { get; set; }

        public void LogDebug(string message, params object[] args)
        {
            if (LogLevel <= LogLevel.Debug)
            {
                ConsoleWriteLine(ConsoleColor.DarkGray, message, args);
            }
        }

        public void LogInfo(string message, params object[] args)
        {
            if (LogLevel <= LogLevel.Info)
            {
                ConsoleWriteLine(ConsoleColor.Gray, message, args);
            }
        }

        public void LogWarn(string message, params object[] args)
        {
            if (LogLevel <= LogLevel.Warning)
            {
                ConsoleWriteLine(ConsoleColor.Yellow, message, args);
            }
        }

        public void LogError(string message, params object[] args)
        {
            if (LogLevel <= LogLevel.Error)
            {
                ConsoleWriteLine(ConsoleColor.Red, message, args);
            }
        }

        private static void ConsoleWriteLine(ConsoleColor foregroundColor, string message, params object[] args)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message, args);
            Console.ForegroundColor = previousColor;
        }
    }
}
