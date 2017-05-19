namespace ForumScannerApp
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Timers;
    using log4net;
    using MukMafiaTool.Common;
    using MukMafiaTool.Database;
    using MukMafiaTool.ForumScanning;

    public class Program
    {
        private static ILog Log = LogManager.GetLogger("ForumScannerApp");

        internal static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            using (var repository = new MongoRepository())
            {
                if (args.Any() && string.Equals(args.First(), "doScan", StringComparison.OrdinalIgnoreCase))
                {
                    DoScan(repository);
                }
                else
                {
                    var interval = TimeSpan.FromMinutes(5);
                    var intervalInConfig = ConfigurationManager.AppSettings["ScanIntervalInMinutes"];
                    int intervalAsInteger = 0;

                    if (int.TryParse(intervalInConfig, out intervalAsInteger))
                    {
                        interval = TimeSpan.FromMinutes(intervalAsInteger);
                    }

                    var timer = new System.Timers.Timer(interval.TotalMilliseconds);
                    timer.Enabled = true;
                    timer.Elapsed += new ElapsedEventHandler((obj, eventHandler) => DoScan(repository));

                    using (var resetEvent = new ManualResetEventSlim(initialState: false))
                    {
                        Console.CancelKeyPress += (s, e) => resetEvent.Set();
                        timer.Start();
                        resetEvent.Wait();
                    }
                }
            }
        }

        internal static void DoScan(IRepository repository)
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