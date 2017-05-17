namespace ForumScannerApp
{
    using System;
    using System.Globalization;
    using System.Linq;
    using log4net;
    using MukMafiaTool.Database;
    using MukMafiaTool.ForumScanning;

    public class Program
    {
        private static ILog Log = LogManager.GetLogger("ForumScannerApp");

        internal static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (args.Any() && string.Equals(args.First(), "doScan", StringComparison.OrdinalIgnoreCase))
            {
                using (var repository = new MongoRepository())
                {
                    try
                    {
                        var scanner = new ForumScanner(repository);

                        scanner.DoWholeUpdate();

                        Log.Info("Scan complete!");
                    }
                    catch (Exception exception)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Exception thrown when attempting to update repository from the forum: {0}", exception.Message);

                        Log.Error(message);
                        repository.LogMessage(message);
                    }
                }                
            }
        }
    }
}