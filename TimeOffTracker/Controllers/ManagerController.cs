using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeOffTracker.Models;
using TimeOffTracker.Models.ManagerModels;
using Microsoft.AspNet.Identity;
using TimeOffTracker.Business;
using System.Data.Entity;

namespace TimeOffTracker.Controllers.ManagerControllers
{
    public class ManagerController : Controller
    {
        // Rename this!
        IListActiveRequests listActiveRequests;

        public ManagerController(IListActiveRequests listActiveRequests)
        {
            this.listActiveRequests = listActiveRequests;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ManagerPanel()
        {
            string id = User.Identity.GetUserId();      //Узнать ID активного пользователя
            return View(listActiveRequests.GetListRequestsModel(id));
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Confirm(int? id)
        {
            string idUser = User.Identity.GetUserId();
            if (id == null) { return HttpNotFound(); }

            listActiveRequests.Confirm(id);
            return View("ManagerPanel", listActiveRequests.GetListRequestsModel(idUser));
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Reject(int? id, string reason)
        {
            string idUser = User.Identity.GetUserId();
            if (id == null) { return HttpNotFound(); }
            listActiveRequests.Reject(id, reason);
            return View("ManagerPanel", listActiveRequests.GetListRequestsModel(idUser));
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Details(int? id)
        {
            //id RequestCheck
            if (id == null) {
                return HttpNotFound();
            }
            return PartialView(listActiveRequests.GetRequestsModel(id));
        }

        [Authorize(Roles = "Manager")]
        public ActionResult History()
        {
            string id = User.Identity.GetUserId();
            return View(listActiveRequests.GetInfoRequestsModel(id));
            //return PartialView(model);
        }
    }
}