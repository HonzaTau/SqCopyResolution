using log4net;
using SqCopyResolution.Model;

namespace SqCopyResolution.Services
{
    public class ConsoleLogger : ILogger
    {
        private readonly ILog log = LogManager.GetLogger("SqCopyResolution");

        public ConsoleLogger()
        {
            LogLevel = LogLevel.Info;
            log4net.Config.XmlConfigurator.Configure();
        }

        public LogLevel LogLevel { get; set; }

        public void LogDebug(string message, params object[] args)
        {
            log.DebugFormat(message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            log.InfoFormat(message, args);
        }

        public void LogWarn(string message, params object[] args)
        {
            log.WarnFormat(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            log.ErrorFormat(message, args);
        }
    }
}
