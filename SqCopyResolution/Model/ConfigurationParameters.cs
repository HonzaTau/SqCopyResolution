using SqCopyResolution.Services;
using System;
using System.Configuration;

namespace SqCopyResolution.Model
{
    public class ConfigurationParameters
    {
        private ILogger Logger { get; set; }
        public string SonarQubeUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SourceProjectKey { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is just for passing list of destination project keys to the applicaton.")]
        public string[] DestinationProjectKeys { get; set; }
        public LogLevel LogLevel { get; set; }

        public ConfigurationParameters(ILogger logger, string[] commandLineArguments)
        {
            Logger = logger;

            SonarQubeUrl = ConfigurationManager.AppSettings["SQ_Url"];
            UserName = ConfigurationManager.AppSettings["SQ_UserName"];
            Password = ConfigurationManager.AppSettings["SQ_Password"];
            SourceProjectKey = ConfigurationManager.AppSettings["SQ_SourceProjectKey"];
            DestinationProjectKeys = ConfigurationManager.AppSettings["SQ_DestinationProjectKeys"].Split(',');
            LogLevel logLevel;
            if (Enum.TryParse<LogLevel>(ConfigurationManager.AppSettings["SQ_LogLevel"], true, out logLevel))
            {
                LogLevel = logLevel;
            }
            else
            {
                LogLevel = LogLevel.Info;
            }

            if (commandLineArguments != null)
            {
                var argsIndex = 0;
                while (argsIndex < commandLineArguments.Length)
                {
                    switch (commandLineArguments[argsIndex].ToUpperInvariant())
                    {
                        case "-URL":
                            SonarQubeUrl = commandLineArguments[argsIndex + 1].Trim();
                            argsIndex += 2;
                            break;
                        case "-USERNAME":
                            UserName = commandLineArguments[argsIndex + 1].Trim();
                            argsIndex += 2;
                            break;
                        case "-PASSWORD":
                            Password = commandLineArguments[argsIndex + 1].Trim();
                            argsIndex += 2;
                            break;
                        case "-SOURCEPROJECTKEY":
                            SourceProjectKey = commandLineArguments[argsIndex + 1].Trim();
                            argsIndex += 2;
                            break;
                        case "-DESTINATIONPROJECTKEYS":
                            DestinationProjectKeys = commandLineArguments[argsIndex + 1].Trim().Split(',');
                            argsIndex += 2;
                            break;
                        case "-LOGLEVEL":
                            if (Enum.TryParse<LogLevel>(commandLineArguments[argsIndex + 1], true, out logLevel))
                            {
                                LogLevel = logLevel;
                            }
                            else
                            {
                                Console.WriteLine("Unknown log level: {0}", commandLineArguments[argsIndex + 1]);
                            }
                            argsIndex += 2;
                            break;
                        default:
                            Console.WriteLine("Unknown argument: {0}", commandLineArguments[argsIndex]);
                            argsIndex += 2;
                            break;
                    }
                }
            }
        }

        internal bool Validate()
        {
            var result = true;

            if (string.IsNullOrEmpty(SonarQubeUrl))
            {
                Logger.LogError("SonarQube url is empty.");
                result = false;
            }
            if (string.IsNullOrEmpty(UserName))
            {
                Logger.LogError("SonarQube user name is empty.");
                result = false;
            }
            if (string.IsNullOrEmpty(Password))
            {
                Logger.LogError("SonarQube password is empty.");
                result = false;
            }
            if (string.IsNullOrEmpty(SourceProjectKey))
            {
                Logger.LogError("Source project key is empty.");
                result = false;
            }
            if (DestinationProjectKeys.Length < 1)
            {
                Logger.LogError("List of destination project keys is empty.");
                result = false;
            }

            return result;
        }
    }
}
