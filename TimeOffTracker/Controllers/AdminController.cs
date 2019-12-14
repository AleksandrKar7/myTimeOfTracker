using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Collections.Generic;
using TimeOffTracker.Business;

namespace TimeOffTracker.Controllers
{
    public class AdminController : Controller
    {
        IAdminDataModel _adminDataModel;
        IVacationControlDataModel _vacationControlDataModel;

        public AdminController(IAdminDataModel adminDataModel, IVacationControlDataModel vacationControlDataModel)
        {
            _adminDataModel = adminDataModel;
            _vacationControlDataModel = vacationControlDataModel;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminUsersPanel()
        {
            return View(_adminDataModel.GetAllUsersForShow());
        }



        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            CreateUserViewModel model = new CreateUserViewModel
            {
                AvailableRoles = _adminDataModel.GetSelectListItemRoles()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            model.AvailableRoles = _adminDataModel.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.EmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Employment date can't be longer than the current date");
                    return View(model);
                }

                IdentityResult result = _adminDataModel.CreateUser(UserManager, model);

                if (result.Succeeded)
                {
                    _vacationControlDataModel.BindingMissingVacationByEmail(model.Email);
                    _vacationControlDataModel.UpdateUserVacationDaysByEmail(model.Email);

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
                return View(_adminDataModel.GetUserForShowByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                _adminDataModel.SwitchLockoutUserByEmail(UserManager, email);
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
                return View(_adminDataModel.GetUserForEditByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(EditUserViewModel model)
        {
            model.AvailableRoles = _adminDataModel.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.NewEmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Employment date can't be longer than the current date");
                    return View(model);
                }
                IdentityResult result = _adminDataModel.EditUser(UserManager, model);

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
                _vacationControlDataModel.BindingMissingVacationByEmail(email);
                _vacationControlDataModel.UpdateUserVacationDaysByEmail(email);

                return View(_adminDataModel.GetUserForEditVacationDaysByEmail(UserManager, email));
            }

            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserVacationDays(EditUserVacationDaysViewModel model)
        {
            model.Vacations = _adminDataModel.GetVacationDictionaryByEmail(model.Email);
            if (ModelState.IsValid)
            {
                string result = _adminDataModel.EditUserVacationDays(model);
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