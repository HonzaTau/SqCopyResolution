using SqCopyResolution.Services;
using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace SqCopyResolutionr
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            logger.LogInformation("Welcome!");
            logger.LogInformation(string.Format(CultureInfo.InvariantCulture,
                "{0} v{1}",
                Assembly.GetEntryAssembly().GetName().Name,
                Assembly.GetEntryAssembly().GetName().Version));

            var sonarQubeUrl = ConfigurationManager.AppSettings["SQ_Url"];
            var username = ConfigurationManager.AppSettings["SQ_Username"];
            var password = ConfigurationManager.AppSettings["SQ_Password"];
            var sourceProjectKey = ConfigurationManager.AppSettings["SQ_SourceProjectKey"];
            var destinationProjectKeys = ConfigurationManager.AppSettings["SQ_DestinationProjectKeys"];

            var argsIndex = 0;
            while (argsIndex < args.Length)
            {
                switch (args[argsIndex].ToUpperInvariant())
                {
                    case "-URL":
                        sonarQubeUrl = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    case "-USERNAME":
                        username = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    case "-PASSWORD":
                        password = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    case "-SOURCEPROJECTKEY":
                        sourceProjectKey = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    case "-DESTINATIONPROJECTKEYS":
                        destinationProjectKeys = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    default:
                        WriteHelp(args[argsIndex]);
                        argsIndex += 2;
                        break;
                }
            }

            var sqProxy = new SonarQubeProxy(logger, sonarQubeUrl, username, password);
            var sourceIssues = sqProxy.GetIssuesForProject(sourceProjectKey, true);

            var destinationProjectKeysArray = destinationProjectKeys.Split(',');
            foreach (var destinationProjectKey in destinationProjectKeysArray)
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
                            sqProxy.UpdateIssueResolution(destinationIssue.Key, sourceIssue.Resolution);
                        }
                    }
                }
            }

            Console.ReadLine();
        }

        private static void WriteHelp(string argument)
        {
            Console.WriteLine("Unknown argument: {0}", argument);
        }
    }
}
