using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TimeOffTracker.Business;

namespace TimeOffTracker.Controllers
{
    public class VacationRequestController : Controller
    {
        IVacationControlDataModel _vacationControlDataModel;
        IVacationRequestDataModel _vacationRequestDataModel;

        private ApplicationUserManager _userManager;

        public VacationRequestController(IVacationControlDataModel vacationControlDataModel, IVacationRequestDataModel vacationRequestDataModel)
        {
            _vacationControlDataModel = vacationControlDataModel;
            _vacationRequestDataModel = vacationRequestDataModel;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Authorize(Roles = "Employee")]
        public ActionResult VacationRequest()
        {
            _vacationControlDataModel.BindingMissingVacationByEmail(User.Identity.GetUserName());
            _vacationControlDataModel.UpdateUserVacationDaysByEmail(User.Identity.GetUserName());

            var model = _vacationRequestDataModel.GetVacationRequest(UserManager, User.Identity.GetUserId());
            if (model == null)
            {
                return View("Error");
            }
            return View(model);   
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        public ActionResult CreateVacationRequest(CreateVacationRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = _vacationRequestDataModel.CreateVacationRequest(UserManager, User.Identity.GetUserId(), model);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    Response.StatusCode = 500;

                    return new JsonResult
                    {
                        Data = new { message = message },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);          
        }



        [Authorize(Roles = "Employee")]
        public ActionResult UserHistoryPanel()
        {
            _vacationControlDataModel.BindingMissingVacationByEmail(User.Identity.GetUserName());
            _vacationControlDataModel.UpdateUserVacationDaysByEmail(User.Identity.GetUserName());

            var model = _vacationRequestDataModel.GetUserHistoryVacation(UserManager, User.Identity.GetUserId());
            if (model == null)
            {
                return View("Error");
            }

            return View(model);
        }
    }
}