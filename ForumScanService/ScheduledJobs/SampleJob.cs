using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using MukMafiaTool.Database;
using MukMafiaTool.ForumScanService;

namespace ForumScanService.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/sample".

    public class SampleJob : ScheduledJob
    {
        public override Task ExecuteAsync()
        {
            Services.Log.Info(string.Format("Starting job at {0}.", DateTime.UtcNow));

            try
            {
                ForumScanner scanner = new ForumScanner(new MongoRepository());
                scanner.DoWholeUpdate();
            }
            catch (Exception e)
            {
                Services.Log.Error(string.Format("Could not update: {0}", e.Message));
            }

            Services.Log.Info(string.Format("Job completed at {0}.", DateTime.UtcNow));

            return Task.FromResult(true);
        }
    }
}