using SqCopyResolution.Model;
using System;

namespace SqCopyResolution.Services
{
    public class Operation
    {
        private ILogger Logger { get; set; }
        private ISonarQubeProxy SonarQubeProxy { get; set; }
        private ConfigurationParameters ConfigParameters { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public Operation(ILogger logger, ConfigurationParameters configParameters) : 
            this(logger, configParameters, new SonarQubeProxy(logger, configParameters.SonarQubeUrl, configParameters.UserName, configParameters.Password))
        {}


        public Operation(ILogger logger, ConfigurationParameters configParameters, ISonarQubeProxy sonarQubeProxy)
        {
            Logger = logger;
            ConfigParameters = configParameters;
            SonarQubeProxy = sonarQubeProxy;
        }

        public void Run()
        {
            switch (ConfigParameters.OperationType)
            {
                case OperationType.CopyFalsePositivesAndWontFixes:
                    CopyResolutions();
                    break;
                default:
                    throw new InvalidOperationException("Unknown operation type: " + ConfigParameters.OperationType);
            }
        }


        private void CopyResolutions()
        {
            Logger.LogInfo("Getting list of issues for project {0}", ConfigParameters.SourceProjectKey);
            var sourceIssues = SonarQubeProxy.GetIssuesForProject(ConfigParameters.SourceProjectKey, true);

            if (sourceIssues.Count > 0)
            {
                foreach (var destinationProjectKey in ConfigParameters.DestinationProjectKeys)
                {
                    Logger.LogInfo("Copying resolutions to project {0}", destinationProjectKey);

                    var destinationIssues = SonarQubeProxy.GetIssuesForProject(destinationProjectKey, false);
                    foreach (var sourceIssue in sourceIssues)
                    {
                        if ((string.CompareOrdinal(sourceIssue.Resolution, "FALSE-POSITIVE") != 0) || (string.CompareOrdinal(sourceIssue.Resolution, "WONTFIX") != 0))
                        {
                            Logger.LogInfo("Issue {0}", sourceIssue);

                            var destinationIssue = destinationIssues.Find((i) =>
                                i.Message == sourceIssue.Message
                                && i.Rule == sourceIssue.Rule
                                && i.ComponentPath == sourceIssue.ComponentPath
                                && i.StartLine == sourceIssue.StartLine
                                && i.StartOffset == sourceIssue.StartOffset);

                            if (destinationIssue == null)
                            {
                                Logger.LogWarn("\tNot found in the destination project");
                            }
                            else if (!string.IsNullOrEmpty(destinationIssue.Resolution))
                            {
                                Logger.LogInfo("\tIssue is already marked as {0} in the destination project.",
                                    destinationIssue.Resolution);
                            }
                            else
                            {
                                Logger.LogInfo("\tUpdating issue resolution to {0}",
                                    sourceIssue.Resolution);
                                SonarQubeProxy.UpdateIssueResolution(destinationIssue.Key, sourceIssue.Resolution, sourceIssue.Comments);
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogWarn("There are no issues to copy!");
            }
        }

    }
}
