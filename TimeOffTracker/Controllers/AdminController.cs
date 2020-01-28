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
using TimeOffTracker.BLL;
using System.Threading;
using Telegram.Bot;
using TelegramBotApp.Models;
using Infrastructure.Services;

namespace TimeOffTracker.Controllers
{
    public class AdminController : Controller
    {
        IAdminBusiness _adminBusiness;
        IVacationControlBusiness _VCBusiness;
        INotifierBusiness _notifier;

        public AdminController(IAdminBusiness adminDataModel
            , IVacationControlBusiness vacationControlDataModel
            , INotifierBusiness notifier)
        {
            _adminBusiness = adminDataModel;
            _VCBusiness = vacationControlDataModel;
            _notifier = notifier;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminUsersPanel()
        {
            SortInfo sortInfo = GetSortInfo();
            return View(sortInfo);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult GetPartOfUsers(int? page, int? count, SortInfo sort)
        {
            PagesInfo pi = GetPagesInfo(page, count);
            SortInfo sortInfo = GetSortInfo();

            return PartialView(_adminBusiness.GetPageOfUsers(pi.CurrentPage, pi.CurrentCountUsersInPage, sortInfo));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Pagination(int? page, int? count)
        {
            return PartialView(GetPagesInfo(page, count));
        }
        public PagesInfo GetPagesInfo(int? page, int? count)
        {
            if (page == null)
            {
                if (HttpContext.Session["page"] != null)
                {
                    page = (int)HttpContext.Session["page"];
                }
                else
                {
                    page = 1;
                }
            }
            if (count == null)
            {
                if (HttpContext.Session["count"] != null)
                {
                    count = (int)HttpContext.Session["count"];
                }
                else
                {
                    count = new PagesInfo().CountUsers[0];
                }
            }

            int totalPages = _adminBusiness.GetTotalPages((int)count);
            if (totalPages < page)
            {
                page = totalPages;
            }

            HttpContext.Session.Add("page", page);
            HttpContext.Session.Add("count", count);

            return new PagesInfo { CurrentPage = (int)page, CurrentCountUsersInPage = (int)count, TotalPages = totalPages };
        }

        public void UpdateSortInfo(bool? fullName, bool? email, bool? eDate, bool? roles)
        {
            HttpContext.Session.Add("FullNameAscending", fullName);
            HttpContext.Session.Add("EmailAscending", email);
            HttpContext.Session.Add("EmploymentAscending", eDate);
            HttpContext.Session.Add("RolesAscending", roles);
        }

        public SortInfo GetSortInfo()
        {
            return new SortInfo
            {
                FullNameAscending = (bool?)HttpContext.Session["FullNameAscending"],
                EmailAscending = (bool?)HttpContext.Session["EmailAscending"],
                EmploymentAscending = (bool?)HttpContext.Session["EmploymentAscending"],
                RolesAscending = (bool?)HttpContext.Session["RolesAscending"]
            };
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
                    //Уведомление администратора
                    string message = "Вы добавили нового пользователя:" + "\n"
                        + "ФИО: " + model.FullName + "\n"
                        + "Email: " + model.Email + "\n"
                        + "Employment date: " + model.EmploymentDate.ToShortDateString();
                    _notifier.SendTelegramMessage(Bot.Get().Result, User.Identity.GetUserId(), message);

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
                string message;
                message = _adminBusiness.SwitchLockoutUserByEmail(UserManager, email)
                    ? "Вы заблокировали пользователя " + email
                    : "Вы сняли блокировку с пользователя " + email;
                //Уведомление администратора
                _notifier.SendTelegramMessage(Bot.Get().Result, User.Identity.GetUserId(), message);
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
                    string message = "Вы отредактировали информацию о пользователе:" + "\n"
                        + (model.OldFullName != model.NewFullName ? (model.OldFullName + " => " + model.NewFullName + ";\n") : null)
                        + (model.OldEmail != model.NewEmail ? (model.OldEmail + " => " + model.NewEmail + ";\n") : null)
                        + (model.OldEmploymentDate != model.NewEmploymentDate.ToShortDateString() ? (model.OldEmploymentDate + " => " + model.NewEmploymentDate.ToShortDateString() + ";\n") : null)
                        + (model.OldRoles != string.Join(", ", model.SelectedRoles) ? (model.OldRoles + " => " + string.Join(", ", model.SelectedRoles) + ";\n") : null);
                    //Уведомление администратора
                    _notifier.SendTelegramMessage(Bot.Get().Result, User.Identity.GetUserId(), message);

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
                string message = "Вы изменили кол-во дней отпуска для " + model.FullName + ":\n";

                int value;
                for (int i = 0; i < model.Vacations.Count; i++)
                {
                    value = model.Vacations.Where(x => x.Key == model.VacationNames[i]).First().Value;
                    if (value != model.VacationDays[i])
                    {
                        message += model.VacationNames[i] + ": " + value + " => " + model.VacationDays[i] + ";\n";
                    }
                }
                //Уведомление администратора
                _notifier.SendTelegramMessage(Bot.Get().Result, User.Identity.GetUserId(), message);

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