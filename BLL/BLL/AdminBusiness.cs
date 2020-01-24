using System.Web;
using System.Web.Mvc;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Collections.Generic;
using TimeOffTracker.Data;
using TimeOffTracker.BLL;

namespace TimeOffTracker.BLL
{
    public class AdminBusiness : IAdminBusiness
    {
        IAdminData _adminData;

        public AdminBusiness(IAdminData adminData)
        {
            _adminData = adminData;
        }

        public TableContent GetAllUsersForShow()
        {
            return new TableContent { Users = _adminData.GetAllUsers() };
        }

        public TableContent GetPageOfUsers(int page, int count, SortInfo sort)
        {
            return new TableContent {Users = _adminData.GetPageOfUsers(page, count, sort) };
        }

        public int GetTotalPages(int countInPage)
        {
            return _adminData.GetTotalPages(countInPage);
        }

        public ShowUserViewModel GetUserForShowByEmail(UserManager<ApplicationUser> userManager, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var user = _adminData.GetUserByEmail(userManager, email);
            var userRoles = _adminData.GetUserRoles(userManager, user);

            return new ShowUserViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                LockoutTime = user.LockoutEndDateUtc,
                AllRoles = string.Join(", ", userRoles),
                EmploymentDate = user.EmploymentDate
            };
        }

        public IdentityResult CreateUser(UserManager<ApplicationUser> userManager, CreateUserViewModel model)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmploymentDate = model.EmploymentDate
            };

            return _adminData.CreateUser(userManager, user, model.Password, model.SelectedRoles);
        }

        public void SwitchLockoutUserByEmail(UserManager<ApplicationUser> userManager, string email)
        {
            _adminData.SwitchLockoutUserByEmail(userManager, email);
        }

        public EditUserViewModel GetUserForEditByEmail(UserManager<ApplicationUser> userManager, string email)
        {
            var user = _adminData.GetUserByEmail(userManager, email);
            var roles = _adminData.GetUserRoles(userManager, user);
            return new EditUserViewModel
            {
                OldFullName = user.FullName,
                NewFullName = user.FullName,
                OldEmail = user.Email,
                NewEmail = user.Email,
                OldEmploymentDate = user.EmploymentDate.ToShortDateString(),
                NewEmploymentDate = user.EmploymentDate,
                OldRoles = string.Join(", ", roles),
                AvailableRoles = _adminData.GetSelectListItemRoles(roles)
            };
        }

        public IdentityResult EditUser(UserManager<ApplicationUser> userManager, EditUserViewModel model)
        {
            IdentityResult result;

            var user = _adminData.GetUserByEmail(userManager, model.OldEmail);

            if (user == null)
            {
                return new IdentityResult("User is not exist");
            }
            if(!String.IsNullOrWhiteSpace(model.IsChangePassword) && String.IsNullOrWhiteSpace(model.NewPassword))
            {
                return new IdentityResult("Password not entered");
            }

            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;
            user.FullName = model.NewFullName;
            user.EmploymentDate = model.NewEmploymentDate;      
            return _adminData.EditUser(userManager, user
                , model.SelectedRoles
                , (String.IsNullOrWhiteSpace(model.IsChangePassword) ? model.NewPassword:null));
        }

        public EditUserVacationDaysViewModel GetUserByEmailForEditVacationDays(UserManager<ApplicationUser> userManager, string email)
        {
            ApplicationUser user = _adminData.GetUserByEmail(userManager, email);
            if (user == null)
            {
                return null;
            }
            var roles = _adminData.GetUserRoles(userManager, user);
            Dictionary<string, int> vacations = new Dictionary<string, int>();
            vacations = _adminData.GetUserVacationDictionary(user);

            return new EditUserVacationDaysViewModel()
            {
                FullName = user.FullName,
                Email = user.Email,
                EmploymentDate = user.EmploymentDate.ToShortDateString(),
                AllRoles = string.Join(", ", roles),
                Vacations = vacations
            };
        }

        //Возвращает строку с пречнем ошибок
        public string EditUserVacationDays(UserManager<ApplicationUser> userManager, EditUserVacationDaysViewModel model)
        {
            string result = "";
            if (!(model.VacationNames.Count == model.VacationDays.Count))
            {
                result = "Something went wrong! Please refresh and try again.";
                return result;
            }
            ApplicationUser user = _adminData.GetUserByEmail(userManager, model.Email);
            return _adminData.EditUserVacationDays(userManager, user, model.VacationNames, model.VacationDays);
        }

        public Dictionary<string, int> GetUserVacationDictionary(string email)
        {
            return _adminData.GetUserVacationDictionary(email);
        }

        public IList<SelectListItem> GetSelectListItemRoles()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (IdentityRole role in _adminData.GetAllRoles())
            {
                result.Add(new SelectListItem { Text = role.Name, Value = role.Name });
            }
            return result;
        }

        public IList<SelectListItem> GetSelectListItemRoles(IList<string> roles)
        {
            IList<SelectListItem> result = GetSelectListItemRoles();
            if (roles != null)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    foreach (string str in roles)
                    {
                        if (result[i].Value == str)
                        {
                            result[i].Selected = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}