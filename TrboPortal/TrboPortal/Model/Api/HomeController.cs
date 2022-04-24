using System.Web.Mvc;
using TrboPortal.Model.Db;

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
            ViewBag.GoogleMapsApiKey = Repository.GetLatestSystemSettingsAsync().Result?.GoogleMapsApiKey ?? "DEFINE_API_KEY_IN_SETTINGS_MENU";
            return View();
        }

        public ActionResult Map()
        {
            ViewBag.Title = "Radios";
            ViewBag.GoogleMapsApiKey = Repository.GetLatestSystemSettingsAsync().Result?.GoogleMapsApiKey ?? "DEFINE_API_KEY_IN_SETTINGS_MENU";
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
