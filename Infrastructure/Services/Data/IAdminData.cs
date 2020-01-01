using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeOffTracker.Models;

namespace TimeOffTracker.Data
{
    public interface IAdminData
    {  
        ListShowUserViewModel GetAllUsers();
        ApplicationUser GetUserByEmail(UserManager<ApplicationUser> UserManager, string email);
        IdentityResult CreateUser(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, IList<string> roles);
        void SwitchLockoutUserByEmail(UserManager<ApplicationUser> userManager, string email);
        IList<SelectListItem> GetSelectListItemRoles(IList<string> roles);
        IList<IdentityRole> GetAllRoles();
        IList<string> GetUserRoles(UserManager<ApplicationUser> userManager, string email);
        IList<string> GetUserRoles(UserManager<ApplicationUser> userManager, ApplicationUser user);
        IdentityResult EditUser(UserManager<ApplicationUser> userManager, ApplicationUser newUser, IList<string> newRoles, string newPassword);
        Dictionary<string, int> GetUserVacationDictionary(ApplicationUser user);
        Dictionary<string, int> GetUserVacationDictionary(string email);
        string EditUserVacationDays(UserManager<ApplicationUser> userManager, ApplicationUser user, IList<string> vacNames, IList<int> vacDays);
    }
}