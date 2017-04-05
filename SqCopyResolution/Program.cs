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
            var resolutions = ConfigurationManager.AppSettings["SQ_Resolutions"];

            var argsIndex = 0;
            while (argsIndex <= args.Length)
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
                    case "-RESOLUTIONS":
                        resolutions = args[argsIndex + 1].Trim();
                        argsIndex += 2;
                        break;
                    default:
                        WriteHelp(args[argsIndex]);
                        argsIndex += 2;
                        break;
                }
            }

            var sqProxy = new SonarQubeProxy(logger, sonarQubeUrl, username, password);
            var sourceIssues = sqProxy.GetIssues(sourceProjectKey, resolutions);
            Console.ReadLine();
        }

        private static void WriteHelp(string argument)
        {
            Console.WriteLine("Unknown argument: {0}", argument);
        }
    }
}
