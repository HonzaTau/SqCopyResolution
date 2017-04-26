using SqCopyResolution.Services;
using System;
using System.Configuration;
using System.Collections.Generic;

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
        public OperationType OperationType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public ConfigurationParameters(ILogger logger, List<string> commandLineArguments)
        {
            Logger = logger ?? throw new ArgumentNullException("logger");

            LogLevel = GetLogLevel(commandLineArguments);
            logger.LogLevel = LogLevel;

            SqCopyResolutionSection appConfig = (SqCopyResolutionSection)ConfigurationManager.GetSection("sqCopyResolutionGroup/sqCopyResolution");
            string profileName = GetConfigValue(commandLineArguments, "profileName", "Default");
            ProfileElement profile = appConfig.Profiles[profileName];
            SonarQubeUrl = GetConfigValue(commandLineArguments, "url", profile.SonarQube.Url);
            UserName = GetConfigValue(commandLineArguments, "username", profile.SonarQube.UserName);
            Password = GetConfigValue(commandLineArguments, "password", profile.SonarQube.Password);
            SourceProjectKey = GetConfigValue(commandLineArguments, "sourceProjectKey", profile.Source.ProjectKey);
            DestinationProjectKeys = GetConfigValue(commandLineArguments, "destinationProjectKeys", profile.Destination.ProjectKeys).Split(',');
            OperationType = GetOperationType(commandLineArguments, profile);
        }

        internal static LogLevel GetLogLevel(List<string> commandLineArguments)
        {
            var logLevelName = GetConfigValue(commandLineArguments, "logLevel", "Info");
            LogLevel logLevel;
            if (Enum.TryParse<LogLevel>(logLevelName, true, out logLevel))
            {
                return logLevel;
            }
            else
            {
                return LogLevel.Info;
            }
        }

        internal static OperationType GetOperationType(List<string> commandLineArguments, ProfileElement profile)
        {
            var operationTypeName = GetConfigValue(commandLineArguments, "profile", profile.Operation.OperationType);
            OperationType operationType;
            if (Enum.TryParse<OperationType>(operationTypeName, true, out operationType))
            {
                return operationType;
            }
            else
            {
                return OperationType.CopyFalsePositivesAndWontFixes;
            }
        }

        internal static string GetConfigValue(List<string> commandLineArguments, string configValueName, string defaultValue)
        {
            var argumentIndex = commandLineArguments.FindIndex(a => a.ToUpperInvariant() == ("-" + configValueName.ToUpperInvariant()));
            if (argumentIndex < 0)
            {
                return defaultValue;
            }
            else
            {
                return commandLineArguments[argumentIndex + 1].Trim();
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
