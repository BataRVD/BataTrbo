using System.Web.Mvc;

namespace TrboPortal.Model.Api
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
