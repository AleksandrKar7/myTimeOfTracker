using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TimeOffTracker.Models;

namespace TimeOffTracker.BLL
{
    public interface IAdminBusiness
    {
        IList<ShowUserViewModel> GetAllUsersForShow();
        IList<ShowUserViewModel> GetPageOfUsers(int page, int count);

        ShowUserViewModel GetUserForShowByEmail(UserManager<ApplicationUser> UserManager, string email);
        IdentityResult CreateUser(UserManager<ApplicationUser> UserManager, CreateUserViewModel model);

        void SwitchLockoutUserByEmail(UserManager<ApplicationUser> UserManager, string email);

        EditUserViewModel GetUserForEditByEmail(UserManager<ApplicationUser> UserManager, string email);
        IdentityResult EditUser(UserManager<ApplicationUser> UserManager, EditUserViewModel model);

        EditUserVacationDaysViewModel GetUserByEmailForEditVacationDays(UserManager<ApplicationUser> UserManager, string email);
        string EditUserVacationDays(UserManager<ApplicationUser> userManager, EditUserVacationDaysViewModel model);

        Dictionary<string, int> GetUserVacationDictionary(string email);
        IList<SelectListItem> GetSelectListItemRoles();
        IList<SelectListItem> GetSelectListItemRoles(IList<string> roles);
    }
}
