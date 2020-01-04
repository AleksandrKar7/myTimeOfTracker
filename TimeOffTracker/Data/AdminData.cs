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
        public ListShowUserViewModel GetAllUsers()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ListShowUserViewModel allUsers = new ListShowUserViewModel();

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

                allUsers.MenuItems = userList.Select(p => new ShowUserViewModel
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

        public ApplicationUser GetUserByEmail(ApplicationUserManager userManager,string email)
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

        public IList<string> GetUserRoles(ApplicationUserManager userManager, string email)
        {
            //var user = userManager.FindByEmail(email);
            return userManager.GetRoles(GetUserByEmail(userManager, email).Id);
        }

        public IList<string> GetUserRoles(ApplicationUserManager userManager, ApplicationUser user)
        {
            //var user = userManager.FindByEmail(email);
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

        //public ShowUserViewModel GetUserByEmail(ApplicationUserManager userManager, string email)
        //{
        //    using (ApplicationDbContext context = new ApplicationDbContext())
        //    {
        //        var user = userManager.FindByEmail(email);
        //        var userRoles = userManager.GetRoles(user.Id);

        //        return new ShowUserViewModel
        //        {
        //            FullName = user.FullName,
        //            Email = user.Email,
        //            LockoutTime = user.LockoutEndDateUtc,
        //            AllRoles = string.Join(", ", userRoles),
        //            EmploymentDate = user.EmploymentDate
        //        };
        //    }
        //}

        public IdentityResult CreateUser(ApplicationUserManager userManager, ApplicationUser user, string password, IList<string> roles)
        {
            //ApplicationUser user = new ApplicationUser
            //{
            //    UserName = model.Email,
            //    Email = model.Email,
            //    FullName = model.FullName,
            //    EmploymentDate = model.EmploymentDate
            //};

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

        public void SwitchLockoutUserByEmail(ApplicationUserManager userManager, string email)
        {
            //var user = userManager.FindByEmail(email);
            //user.LockoutEnabled = true;

            //userManager.SetLockoutEndDate(user.Id, time);

            var user = userManager.FindByEmail(email);
            user.LockoutEnabled = true;
            if (user.LockoutEndDateUtc == null || user.LockoutEndDateUtc == DateTimeOffset.MinValue)
            {
                userManager.SetLockoutEndDate(user.Id, DateTime.Now.AddYears(1000));
            }
            else
            {
                userManager.SetLockoutEndDate(user.Id, DateTimeOffset.MinValue);
            }
        }

        public IdentityResult EditUser(ApplicationUserManager userManager, ApplicationUser newUser, IList<string> newRoles, string newPassword)
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

            //IdentityResult result;

            //ApplicationUser user = userManager.FindByEmail(model.OldEmail + "");
            //if (user == null)
            //{
            //    return new IdentityResult("User is not exist");
            //}

            //var rolesUser = userManager.GetRoles(user.Id);

            //if (rolesUser.Count() > 0)
            //{
            //    //Удалаем все старые роли перед обновлением
            //    foreach (var item in rolesUser.ToList())
            //    {
            //        result = userManager.RemoveFromRole(user.Id, item);
            //    }
            //}

            //user.Email = model.NewEmail;
            //user.UserName = model.NewEmail;
            //user.FullName = model.NewFullName;
            //user.EmploymentDate = model.NewEmploymentDate;

            //result = userManager.Update(user);

            //if (result.Succeeded && model.SelectedRoles != null)
            //{
            //    foreach (string role in model.SelectedRoles)
            //    {
            //        if (model.SelectedRoles != null)
            //        {
            //            result = userManager.AddToRole(user.Id, role);
            //        }
            //    }
            //}

            //if (!string.IsNullOrWhiteSpace(model.IsChangePassword))
            //{
            //    //добавляю "" т.к. ValidateAsync генерирует NullReferenceException при получении null
            //    result = userManager.PasswordValidator.ValidateAsync(model.NewPassword + "").Result;
            //    if (result.Succeeded)
            //    {
            //        string token = userManager.GeneratePasswordResetToken(user.Id);
            //        userManager.ResetPassword(user.Id, token, model.NewPassword);
            //    }
            //}

            //return result;

        }

        //public Dictionary<string, int> GetVacationDictionaryByEmail(string email)
        //{
        //    using (ApplicationDbContext context = new ApplicationDbContext())
        //    {
        //        Dictionary<string, int> result = new Dictionary<string, int>();
        //        var listUserVacation = context.UserVacationDays.Where(m => m.User.Email == email).ToList();
        //        foreach (var item in listUserVacation)
        //        {
        //            result.Add(item.VacationType.Name, item.VacationDays);
        //        }
        //        return result;
        //    }
        //}

        public string EditUserVacationDays(ApplicationUserManager userManager, ApplicationUser user
            ,IList<string> vacNames, IList<int> vacDays)
        {
            string result = "";
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();
                //Если кол-во видов вакансий не совпадает с кол-вом значений
                //if (!(model.VacationNames.Count == model.VacationDays.Count))
                //{
                //    result = "Something went wrong! Please refresh and try again.";
                //    return result;
                //}
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

            //string result = "";
            //using (ApplicationDbContext context = new ApplicationDbContext())
            //{
            //    var listUserVacation = context.UserVacationDays.Where(m => m.User.Email == model.Email).ToList();
            //    //Если кол-во видов вакансий не совпадает с кол-вом значений
            //    if (!(model.VacationNames.Count == model.VacationDays.Count))
            //    {
            //        result = "Something went wrong! Please refresh and try again.";
            //        return result;
            //    }
            //    Dictionary<string, int> tempDic = new Dictionary<string, int>();
            //    for (int i = 0; i < model.VacationNames.Count; i++)
            //    {
            //        tempDic.Add(model.VacationNames[i], model.VacationDays[i]);
            //    }
            //    foreach (var temp in tempDic)
            //    {
            //        foreach (var item in listUserVacation)
            //        {
            //            if (item.VacationType.Name == temp.Key)
            //            {
            //                if (temp.Value < 0)
            //                {
            //                    result += temp.Key + " can't be less than zero" + "\n";
            //                }
            //                else
            //                {
            //                    item.VacationDays = temp.Value;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    if (string.IsNullOrWhiteSpace(result))
            //    {
            //        return result;
            //    }
            //    context.SaveChanges();
            //}
            //return result;
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