using log4net;
using System.Globalization;

namespace SqCopyResolution.Services
{
    public class Log4NetWrapper : ILogger
    {
        private static readonly ILog log = LogManager.GetLogger("SqCopyResolution");

        public void LogDebug(string message, params object[] args)
        {
            log.DebugFormat(CultureInfo.InvariantCulture, message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, message, args);
        }

        public void LogWarn(string message, params object[] args)
        {
            log.WarnFormat(CultureInfo.InvariantCulture, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            log.ErrorFormat(CultureInfo.InvariantCulture, message, args);
        }
    }
}
