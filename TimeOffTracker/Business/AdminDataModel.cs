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


namespace TimeOffTracker.Business
{
    public class AdminDataModel : IAdminDataModel
    {
        public ListShowUserViewModel GetAllUsersForShow()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ListShowUserViewModel allUsers = new ListShowUserViewModel();

                var userList = (from user in context.Users
                                orderby user.LockoutEndDateUtc
                                select new
                                {
                                    FullName = user.FullName,
                                    user.Email,
                                    user.EmploymentDate,
                                    user.LockoutEndDateUtc ,
                                    RoleNames = (from userRole in user.Roles
                                                 join role in context.Roles
                                                 on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name).ToList()

                                }).ToList();

                allUsers.MenuItems = userList.Select(p => new ShowUserViewModel
                {
                    FullName = p.FullName ,
                    Email = p.Email,
                    LockoutTime = p.LockoutEndDateUtc ,
                    AllRoles = string.Join(", ", p.RoleNames) ,
                    EmploymentDate = p.EmploymentDate.ToShortDateString()
                }).ToList();

                return allUsers;
            }
        }

        public ShowUserViewModel GetUserForShowByEmail(ApplicationUserManager UserManager, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }
            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                var user = UserManager.FindByEmail(email);
                var userRoles = UserManager.GetRoles(user.Id);

                return new ShowUserViewModel
                {
                    FullName = user.FullName ,
                    Email = user.Email ,
                    LockoutTime = user.LockoutEndDateUtc,
                    AllRoles = string.Join(", ", userRoles),
                    EmploymentDate = user.EmploymentDate.ToShortDateString()
                };
            }
        }



        public IdentityResult CreateUser(ApplicationUserManager UserManager, CreateUserViewModel model)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName ,
                EmploymentDate = model.EmploymentDate
            };

            IdentityResult result = UserManager.Create(user, model.Password);

            if (result.Succeeded)
            {
                if (model.SelectedRoles != null)
                {
                    foreach (string role in model.SelectedRoles)
                    {
                        result = UserManager.AddToRole(user.Id, role);
                    }
                }
            }

            return result;
        }

        public void SwitchLockoutUserByEmail(ApplicationUserManager UserManager, string email)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return;
            }
            var user = UserManager.FindByEmail(email);
            user.LockoutEnabled = true;
            if (user.LockoutEndDateUtc == null || user.LockoutEndDateUtc == DateTimeOffset.MinValue)
            {
                UserManager.SetLockoutEndDate(user.Id, DateTime.Now.AddYears(1000));
            }
            else
            {
                UserManager.SetLockoutEndDate(user.Id, DateTimeOffset.MinValue);
            }
        }



        public EditUserViewModel GetUserForEditByEmail(ApplicationUserManager UserManager, string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return null;
                }
                var user = UserManager.FindByEmail(email);
                var userRoles = UserManager.GetRoles(user.Id);

                return new EditUserViewModel
                {
                    OldFullName = user.FullName,
                    NewFullName = user.FullName,
                    OldEmail = user.Email,
                    NewEmail = user.Email,
                    OldEmploymentDate = user.EmploymentDate.ToShortDateString(),
                    NewEmploymentDate = user.EmploymentDate,
                    OldRoles = string.Join(", ", userRoles),
                    AvailableRoles = GetSelectListItemRoles(userRoles)
                };
            }
        }

        public IdentityResult EditUser(ApplicationUserManager UserManager, EditUserViewModel model)
        {
            IdentityResult result;

            ApplicationUser user = UserManager.FindByEmail(model.OldEmail + "");
            if(user == null)
            {
                return new IdentityResult("User is not exist");
            }

            var rolesUser = UserManager.GetRoles(user.Id);

            if (rolesUser.Count() > 0)
            {
                //Удалаем все старые роли перед обновлением
                foreach (var item in rolesUser.ToList())
                {
                    result = UserManager.RemoveFromRole(user.Id, item);
                }
            }

            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;
            user.FullName = model.NewFullName;
            user.EmploymentDate = model.NewEmploymentDate;

            result = UserManager.Update(user);

            if (result.Succeeded && model.SelectedRoles != null)
            {
                foreach (string role in model.SelectedRoles)
                {
                    if (model.SelectedRoles != null)
                    {
                        result = UserManager.AddToRole(user.Id, role);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(model.IsChangePassword))
            {
                //добавляю "" т.к. ValidateAsync генерирует NullReferenceException при получении null
                result = UserManager.PasswordValidator.ValidateAsync(model.NewPassword + "").Result;
                if (result.Succeeded)
                {
                    string token = UserManager.GeneratePasswordResetToken(user.Id);
                    UserManager.ResetPassword(user.Id, token, model.NewPassword);
                }
            }

            return result;
        }



        public EditUserVacationDaysViewModel GetUserForEditVacationDaysByEmail(ApplicationUserManager UserManager, string email)
        {
            ApplicationUser user = UserManager.FindByEmail(email + "");
            if(user == null)
            {
                return null;
            }
            var userRoles = UserManager.GetRoles(user.Id);
            Dictionary<string, int> vacations = new Dictionary<string, int>();
            vacations = GetVacationDictionaryByEmail(email);

            return new EditUserVacationDaysViewModel()
            {
                FullName = user.FullName,
                Email = user.Email,
                EmploymentDate = user.EmploymentDate.ToShortDateString(),
                AllRoles = string.Join(", ", userRoles),
                Vacations = vacations
            };
        }

        //Возвращает строку с пречнем ошибок
        public string EditUserVacationDays(EditUserVacationDaysViewModel model)
        {
            string result = "";
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Email == model.Email).ToList();
                if (!(model.VacationNames.Count == model.VacationDays.Count))
                {
                    result = "Something went wrong! Please refresh and try again.";
                    return result;
                }
                Dictionary<string, int> tempDic = new Dictionary<string, int>();
                for (int i = 0; i < model.VacationNames.Count; i++)
                {
                    tempDic.Add(model.VacationNames[i], model.VacationDays[i]);
                }
                foreach (var temp in tempDic)
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
                if (string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
                context.SaveChanges();
            }
            return result;
        }

        public Dictionary<string, int> GetVacationDictionaryByEmail(string email)
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
    }
}