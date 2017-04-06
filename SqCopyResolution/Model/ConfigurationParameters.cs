using System;
using System.Configuration;

namespace SqCopyResolution.Model
{
    public class ConfigurationParameters
    {
        public string SonarQubeUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SourceProjectKey { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is just for passing list of destination project keys to the applicaton.")]
        public string[] DestinationProjectKeys { get; set; }

        public ConfigurationParameters(string[] commandLineArguments)
        {
            SonarQubeUrl = ConfigurationManager.AppSettings["SQ_Url"];
            UserName = ConfigurationManager.AppSettings["SQ_UserName"];
            Password = ConfigurationManager.AppSettings["SQ_Password"];
            SourceProjectKey = ConfigurationManager.AppSettings["SQ_SourceProjectKey"];
            DestinationProjectKeys = ConfigurationManager.AppSettings["SQ_DestinationProjectKeys"].Split(',');

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
                        default:
                            WriteHelp(commandLineArguments[argsIndex]);
                            argsIndex += 2;
                            break;
                    }
                }
            }
        }

        private static void WriteHelp(string argument)
        {
            Console.WriteLine("Unknown argument: {0}", argument);
        }

    }
}
