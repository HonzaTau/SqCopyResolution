namespace SqCopyResolution.Services
{
    public interface ILogger
    {
        void LogInformation(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogWarn(string message, params object[] args);
        void LogError(string message, params object[] args);
    }
}
