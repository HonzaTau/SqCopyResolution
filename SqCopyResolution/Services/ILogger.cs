using SqCopyResolution.Model;

namespace SqCopyResolution.Services
{
    public interface ILogger
    {
        LogLevel LogLevel { get; set; }
        void LogInfo(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogWarn(string message, params object[] args);
        void LogError(string message, params object[] args);
    }
}
