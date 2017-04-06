using SqCopyResolution.Model;
using SqCopyResolution.Services;
using System.Globalization;
using System.Reflection;

namespace SqCopyResolutionr
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            logger.LogInformation(string.Format(CultureInfo.InvariantCulture,
                "{0} v{1}",
                Assembly.GetEntryAssembly().GetName().Name,
                Assembly.GetEntryAssembly().GetName().Version));

            var configParams = new ConfigurationParameters(args);

            CopyResolutions(logger, configParams);
        }

        private static void CopyResolutions(ConsoleLogger logger, ConfigurationParameters configParams)
        {
            var sqProxy = new SonarQubeProxy(logger, configParams.SonarQubeUrl, configParams.UserName, configParams.Password);
            var sourceIssues = sqProxy.GetIssuesForProject(configParams.SourceProjectKey, true);

            foreach (var destinationProjectKey in configParams.DestinationProjectKeys)
            {
                var destinationIssues = sqProxy.GetIssuesForProject(destinationProjectKey, false);
                foreach (var sourceIssue in sourceIssues)
                {
                    if ((string.CompareOrdinal(sourceIssue.Resolution, "FALSE-POSITIVE") != 0) || (string.CompareOrdinal(sourceIssue.Resolution, "WONTFIX") != 0))
                    {
                        var destinationIssue = destinationIssues.Find((i) =>
                            i.Message == sourceIssue.Message
                            && i.Rule == sourceIssue.Rule
                            && i.ComponentPath == sourceIssue.ComponentPath
                            && i.StartLine == sourceIssue.StartLine
                            && i.StartOffset == sourceIssue.StartOffset);

                        if (destinationIssue == null)
                        {
                            logger.LogInformation("Issue {0} was not found in destination project.",
                                sourceIssue);
                        }
                        else if (!string.IsNullOrEmpty(destinationIssue.Resolution))
                        {
                            logger.LogInformation("Issue {0} is already marked as {1} in the destination project.",
                                sourceIssue,
                                destinationIssue.Resolution);
                        }
                        else
                        {
                            logger.LogInformation("Issue {0} is not resolved in destination project - updating issue resolution to {1}",
                                sourceIssue,
                                sourceIssue.Resolution);
                            sqProxy.UpdateIssueResolution(destinationIssue.Key, sourceIssue.Resolution, sourceIssue.Comments);
                        }
                    }
                }
            }
        }
    }
}
