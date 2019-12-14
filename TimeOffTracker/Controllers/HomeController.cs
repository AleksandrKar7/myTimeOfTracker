using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeOffTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return View("NotLoggedIn");
            }
            if (User.IsInRole("Employee"))
            {
                return RedirectToAction("UserHistoryPanel", "VacationRequest");
            }
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("ManagerPanel", "Manager");
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminUsersPanel", "Admin");
            }
            return View();
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}