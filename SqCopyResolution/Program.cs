using SqCopyResolution.Model;
using SqCopyResolution.Services;
using System.Globalization;
using System.Reflection;
using System.Linq;

namespace SqCopyResolutionr
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            LogApplicationHeader(logger);

            var configParams = new ConfigurationParameters(logger, args.ToList());

            if (configParams.Validate())
            {
                var operation = new Operation(logger, configParams);
                operation.Run();
            }
        }

        private static void LogApplicationHeader(ConsoleLogger logger)
        {
            logger.LogInfo(string.Format(CultureInfo.InvariantCulture,
                "{0} v{1}",
                Assembly.GetEntryAssembly().GetName().Name,
                Assembly.GetEntryAssembly().GetName().Version));
            logger.LogInfo(string.Empty);
        }
    }
}
