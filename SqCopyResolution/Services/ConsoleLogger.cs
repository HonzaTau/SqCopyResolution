using System;

namespace SqCopyResolution.Services
{
    public class ConsoleLogger : ILogger
    {
        public void LogDebug(string message, params object[] args)
        {
            ConsoleWriteLine(ConsoleColor.DarkGray, message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            ConsoleWriteLine(ConsoleColor.Gray, message, args);
        }

        public void LogWarn(string message, params object[] args)
        {
            ConsoleWriteLine(ConsoleColor.Yellow, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            ConsoleWriteLine(ConsoleColor.Red, message, args);
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
