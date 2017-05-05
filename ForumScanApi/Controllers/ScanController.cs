namespace ForumScanApi.Controllers
{
    using System;
    using System.Web.Http;
    using MukMafiaTool.Database;

    [Route("scan")]
    public class ScanController : ApiController
    {
        [HttpGet]

        public IHttpActionResult DoScan()
        {
            var message = string.Empty;

            try
            {
                using (var repository = new MongoRepository())
                {
                    var lastUpdatedTime = repository.FindLastUpdatedDateTime();

                    var timeSinceLastUpdate = DateTime.UtcNow.Subtract(lastUpdatedTime);

                    if (timeSinceLastUpdate > TimeSpan.FromMinutes(3))
                    {
                        ForumScanner scanner = new ForumScanner(repository);
                        scanner.DoWholeUpdate();

                        message = "Scan was successful.";
                    }
                    else
                    {
                        message = $"Did not scan because the previous scan was only {timeSinceLastUpdate.TotalSeconds} seconds ago.";
                    }
                }
            }
            catch (Exception e)
            {
                return this.InternalServerError(e);
            }

            return this.Ok(message);
        }
    }
}