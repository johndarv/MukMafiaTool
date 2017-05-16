namespace MukMafiaTool.Controllers
{
    using System.Web.Mvc;
    using MukMafiaTool.Database;

    public class ScanController : Controller
    {
        // GET: Scan
        public ActionResult Index()
        {
            using (var repository = new MongoRepository())
            {
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}