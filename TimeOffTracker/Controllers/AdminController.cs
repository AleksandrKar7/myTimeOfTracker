﻿using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Collections.Generic;
using TimeOffTracker.BLL;

namespace TimeOffTracker.Controllers
{
    public class AdminController : Controller
    {
        IAdminBusiness _adminBusiness;
        IVacationControlBusiness _VCBusiness;
        //public AdminController() { }
        //public AdminController() : this(new IAdminBusiness(), new IVacationControlBusiness()) { }
        public AdminController(IAdminBusiness adminDataModel, IVacationControlBusiness vacationControlDataModel)
        {
            _adminBusiness = adminDataModel;
            _VCBusiness = vacationControlDataModel;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminUsersPanel()
        {
            return View(_adminBusiness.GetAllUsersForShow());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetPartOfUsers()
        {
            //var temp = _adminBusiness.GetAllUsersForShow();
            //var onlyOne = new ListShowUserViewModel();
            ////onlyOne.MenuItems.Add(temp.MenuItems[0]);
            //onlyOne.MenuItems = new List<ShowUserViewModel>();
            //onlyOne.MenuItems.Add(temp.MenuItems[0]);


            //return PartialView(onlyOne);
            return PartialView(_adminBusiness.GetAllUsersForShow());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            CreateUserViewModel model = new CreateUserViewModel
            {
                AvailableRoles = _adminBusiness.GetSelectListItemRoles()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            model.AvailableRoles = _adminBusiness.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.EmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Employment date can't be longer than the current date");
                    return View(model);
                }

                IdentityResult result = _adminBusiness.CreateUser(UserManager, model);

                if (result.Succeeded)
                {
                    _VCBusiness.ControlUserVacationDays(model.Email);

                    return RedirectToAction("AdminUsersPanel");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmSwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                return View(_adminBusiness.GetUserForShowByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                _adminBusiness.SwitchLockoutUserByEmail(UserManager, email);
            }
            else
            {
                return RedirectToAction("AdminUsersPanel");
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                return View(_adminBusiness.GetUserForEditByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(EditUserViewModel model)
        {
            model.AvailableRoles = _adminBusiness.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.NewEmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Employment date can't be longer than the current date");
                    return View(model);
                }
                IdentityResult result = _adminBusiness.EditUser(UserManager, model);

                if (result.Succeeded)
                {
                    return RedirectToAction("AdminUsersPanel");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult EditUserVacationDays(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                _VCBusiness.ControlUserVacationDays(email);

                return View(_adminBusiness.GetUserByEmailForEditVacationDays(UserManager, email));
            }

            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserVacationDays(EditUserVacationDaysViewModel model)
        {
            model.Vacations = _adminBusiness.GetUserVacationDictionary(model.Email);
            if (ModelState.IsValid)
            {
                string result = _adminBusiness.EditUserVacationDays(UserManager, model);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ModelState.AddModelError("", result);
                    return View(model);
                }
                return RedirectToAction("AdminUsersPanel");
            }

            return View(model);
        }



        [Authorize(Roles = "Admin")]
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

    }
}