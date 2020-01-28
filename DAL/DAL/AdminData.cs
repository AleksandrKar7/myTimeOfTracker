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
    public class AdminData : IAdminData
    {
        //Возвращает список всех пользователей
        public IList<ShowUserViewModel> GetAllUsers()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                IList<ShowUserViewModel> allUsers = new List<ShowUserViewModel>();
                
                var userList = (from user in context.Users                           
                                orderby user.LockoutEndDateUtc
                                select new
                                {
                                    user.FullName,
                                    user.Email,
                                    user.EmploymentDate,
                                    user.LockoutEndDateUtc,
                                    RoleNames = (from userRole in user.Roles
                                                 join role in context.Roles
                                                 on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name).ToList()
                                }).ToList();

                allUsers = userList.Select(p => new ShowUserViewModel
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    LockoutTime = p.LockoutEndDateUtc,
                    AllRoles = string.Join(", ", p.RoleNames),
                    EmploymentDate = p.EmploymentDate
                }).ToList();

                return allUsers;
            }
        }

        //Возвращает отсортированный набор ApplicationUser
        public IQueryable<ApplicationUser> GetSortedUsers(ApplicationDbContext context, int page, int count, SortInfo sort)
        {
            if(sort.FullNameAscending != null)
            {
                return (bool)sort.FullNameAscending
                    ? context.Users.OrderBy((p => p.FullName)).Skip((page - 1) * count).Take(count)
                    : context.Users.OrderByDescending(p => p.FullName).Skip((page - 1) * count).Take(count);
            }
            if(sort.EmailAscending != null)
            {
                return (bool)sort.EmailAscending 
                    ? context.Users.OrderBy(p => p.Email).Skip((page - 1) * count).Take(count)
                    : context.Users.OrderByDescending(p => p.Email).Skip((page - 1) * count).Take(count);
            }
            if(sort.EmploymentAscending != null)
            {
                return (bool)sort.EmploymentAscending
                   ? context.Users.OrderBy(p => p.EmploymentDate).Skip((page - 1) * count).Take(count)
                   : context.Users.OrderByDescending(p => p.EmploymentDate).Skip((page - 1) * count).Take(count);
            }
            if (sort.RolesAscending != null)
            {
                return (bool)sort.RolesAscending
                   ? context.Users.OrderBy(p => p.Roles.Count).Skip((page - 1) * count).Take(count)
                   : context.Users.OrderByDescending(p => p.Roles.Count).Skip((page - 1) * count).Take(count);
            }
            return context.Users.OrderBy(p => p.LockoutEndDateUtc).ThenBy(p => p.FullName)
            .Skip((page - 1) * count).Take(count);
        }

        public IList<ShowUserViewModel> GetPageOfUsers(int page, int count, SortInfo sort)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                if(page <= 0)
                {
                    return null;
                }
                IQueryable<ApplicationUser> users = GetSortedUsers(context, page, count, sort);

                IList<ShowUserViewModel> result = new List<ShowUserViewModel>();

                var userRoles = (from user in users
                                select new
                                {
                                    RoleNames = (from userRole in user.Roles
                                                 join role in context.Roles
                                                 on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name).ToList()
                                }).ToList();

                result = users.Select(p => new ShowUserViewModel
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    LockoutTime = p.LockoutEndDateUtc,
                    EmploymentDate = p.EmploymentDate
                }).ToList();

                for(int j = 0; j < userRoles.Count(); j++)
                {
                    result[j].AllRoles = string.Join(", ", userRoles[j].RoleNames);
                }

                return result;
            }
        }


        public int GetTotalPages(int countInPage)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                int result;
                double totalUsers = context.Users.Count();
                result = (int)Math.Ceiling(totalUsers / (double)countInPage);

                return result;
            }
        }

        public ApplicationUser GetUserByEmail(UserManager<ApplicationUser> userManager,string email)
        {
            return userManager.FindByEmail(email);
        }

        public IList<IdentityRole> GetAllRoles()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                return context.Roles.ToList();
            }
        }

        public IList<string> GetUserRoles(UserManager<ApplicationUser> userManager, string email)
        {
            return userManager.GetRoles(GetUserByEmail(userManager, email).Id);
        }

        public IList<string> GetUserRoles(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            return userManager.GetRoles(user.Id);
        }

        public IList<SelectListItem> GetSelectListItemRoles()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                List<SelectListItem> result = new List<SelectListItem>();
                foreach (IdentityRole role in context.Roles)
                {
                    result.Add(new SelectListItem { Text = role.Name, Value = role.Name });
                }
                return result;
            }
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

        public IdentityResult CreateUser(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, IList<string> roles)
        {
            IdentityResult result = userManager.Create(user, password);

            if (result.Succeeded)
            {
                if (roles != null)
                {
                    foreach (string role in roles)
                    {
                        result = userManager.AddToRole(user.Id, role);
                    }
                }
            }

            return result;
        }

        public bool SwitchLockoutUserByEmail(UserManager<ApplicationUser> userManager, string email)
        {
            var user = userManager.FindByEmail(email);
            user.LockoutEnabled = true;
            if (user.LockoutEndDateUtc == null || user.LockoutEndDateUtc == DateTimeOffset.MinValue)
            {
                userManager.SetLockoutEndDate(user.Id, DateTime.Now.AddYears(1000));
                return true;
            }
            else
            {
                userManager.SetLockoutEndDate(user.Id, DateTimeOffset.MinValue);
                return false;
            }
        }

        public IdentityResult EditUser(UserManager<ApplicationUser> userManager, ApplicationUser newUser, IList<string> newRoles, string newPassword)
        {
            IdentityResult result;
            var rolesUser = GetUserRoles(userManager, newUser);

            if (rolesUser.Count() > 0)
            {
                //Удалаем все старые роли перед обновлением
                foreach (var item in rolesUser.ToList())
                {
                    result = userManager.RemoveFromRole(newUser.Id, item);
                }
            }

            result = userManager.Update(newUser);

            if (result.Succeeded && newRoles != null)
            {
                foreach (string role in newRoles)
                {
                    result = userManager.AddToRole(newUser.Id, role);
                }
            }

            if (newPassword != null)
            {
                //добавляю "" т.к. ValidateAsync генерирует NullReferenceException при получении null
                result = userManager.PasswordValidator.ValidateAsync(newPassword + "").Result;
                if (result.Succeeded)
                {
                    string token = userManager.GeneratePasswordResetToken(newUser.Id);
                    userManager.ResetPassword(newUser.Id, token, newPassword);
                }
            }

            return result;
        }

        public string EditUserVacationDays(UserManager<ApplicationUser> userManager, ApplicationUser user
            ,IList<string> vacNames, IList<int> vacDays)
        {
            string result = "";
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                Dictionary<string, int> tempDict = new Dictionary<string, int>();
                for (int i = 0; i < vacNames.Count; i++)
                {
                    tempDict.Add(vacNames[i], vacDays[i]);
                }
                foreach (var temp in tempDict)
                {
                    foreach (var item in listUserVacation)
                    {
                        if (item.VacationType.Name == temp.Key)
                        {
                            if (temp.Value < 0)
                            {
                                result += temp.Key + " can't be less than zero" + "\n";
                            }
                            else
                            {
                                item.VacationDays = temp.Value;
                                break;
                            }
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
                context.SaveChanges();
            }
            return result;
        }

        public Dictionary<string, int> GetUserVacationDictionary(ApplicationUser user)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();
                foreach (var item in listUserVacation)
                {
                    result.Add(item.VacationType.Name, item.VacationDays);
                }
                return result;
            }
        }

        public Dictionary<string, int> GetUserVacationDictionary(string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Email == email).ToList();
                foreach (var item in listUserVacation)
                {
                    result.Add(item.VacationType.Name, item.VacationDays);
                }
                return result;
            }
        }
    }
}