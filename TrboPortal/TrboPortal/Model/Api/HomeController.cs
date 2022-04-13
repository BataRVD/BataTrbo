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

        public ActionResult Logs()
        {
            ViewBag.Title = "Logs";
            return View();
        }

        public ActionResult Radios()
        {
            ViewBag.Title = "Radios";
            return View();
        }

        public ActionResult MessageQueue()
        {
            ViewBag.Title = "MessageQueue";
            return View();
        }

        public ActionResult Playground()
        {
            ViewBag.Title = "Playground";
            return View();
        }

        public ActionResult Swagger()
        {
            ViewBag.Title = "Swagger";
            return View();
        }

        public ActionResult Settings()
        {
            ViewBag.Title = "Settings";
            return View();
        }
    }
}
