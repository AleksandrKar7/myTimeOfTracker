using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public class VacationRequestDataModel : IVacationRequestDataModel
    {
        IVacationControlDataModel _vacationControlDataModel;

        public VacationRequestDataModel(IVacationControlDataModel vacationControlDataModel)
        {
            _vacationControlDataModel = vacationControlDataModel;
        }

        public VacationRequestViewModels GetVacationRequest(ApplicationUserManager UserManager, string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var currentUser = UserManager.FindById(id + "");
                if (currentUser == null)
                {
                    return null;
                }

                var allApprovers = db.Roles.Where(r => r.Name == "Manager").First().Users.Join(db.Users, role => role.UserId, u => u.Id, (role, u) => new ApplicationUser()
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    LockoutEndDateUtc = u.LockoutEndDateUtc
                }).ToList();

                var approvers = allApprovers.Where(x => x.LockoutEndDateUtc == null).ToList();

                VacationRequestViewModels vacationRequestViewModel = new VacationRequestViewModels();
                List<VacationTypes> vacationTypes = db.VacationTypes.ToList();
                vacationRequestViewModel.User = currentUser;
                vacationRequestViewModel.VacationTypes = vacationTypes;
                vacationRequestViewModel.Approvers = approvers;
                return vacationRequestViewModel;
            }
        }


        public string CreateVacationRequest(ApplicationUserManager UserManager, string id, CreateVacationRequestViewModel model)
        {
            var currentUser = UserManager.FindById(id + "");

            if (currentUser == null)
            {
                return "Oh! Something went wrong! Please, try again later";
            }

            model.VacationRequest.EmployeeId = currentUser.Id;

            using (var db = new ApplicationDbContext())
            {
                int days = (int)(model.VacationRequest.DateEnd - model.VacationRequest.DateStart).TotalDays + 1;

                if(model.VacationRequest.DateStart < DateTime.Now.Date)
                {
                    return "Start date can't be more than current date";
                }
                if (days <= 0)
                {
                    return "End date can't be more than date start";
                }

                var userVacation = db.UserVacationDays.Where(v => v.User.Id == currentUser.Id && v.VacationType.Id == model.VacationRequest.VacationTypesId).First();

                var waitStatus = db.RequestStatuses.Where(x => x.Id == 3).First();
                var userHistory = _vacationControlDataModel.GetAllSumUserHistoryVacation(currentUser.Email, waitStatus, false);

                foreach (var item in userHistory)
                {
                    if (item.VacationType.Name == userVacation.VacationType.Name)
                    {
                        if (userVacation.VacationDays < (item.VacationDays + days))
                        {
                            string message = "The number of specified days could exceed the limit."
                                + " Please change the number of days or wait for the results of the verification of previous requests.";
                            if ((userVacation.VacationDays - item.VacationDays) > 0)
                            {
                                message += " Now for " + userVacation.VacationType.Name + " you can get: " + (userVacation.VacationDays - item.VacationDays).ToString() + " days";
                            }
                            return message;
                        }
                    }
                }

                db.Requests.Add(model.VacationRequest);
                db.SaveChanges();

                var status = db.RequestStatuses.Where(s => s.Name == "Waiting").First();
                int priority = 1;
                var approvers = model.ApproversId;
                foreach (var approver in approvers)
                {
                    RequestChecks vacationRequestCheck = new RequestChecks()
                    {
                        RequestId = model.VacationRequest.Id,
                        ApproverId = approver,
                        Priority = priority,
                        StatusId = status.Id
                    };
                    db.RequestChecks.Add(vacationRequestCheck);
                    db.SaveChanges();
                    priority++;
                }
            }
            return null;
        }

        public ListHistoryOfVacationViewModel GetUserHistoryVacation(ApplicationUserManager UserManager, string id)
        {          
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var history = new ListHistoryOfVacationViewModel();
                ApplicationUser user = UserManager.FindById(id + "");

                if(user == null)
                {
                    return null;
                }

                var UserVacationDays = context.UserVacationDays.Where(x => x.User.Id == user.Id).Select(p => p).ToList();
                history.UserVacationDays = UserVacationDays.Select(p => new UserVacationDays
                {
                    Id = p.Id,
                    User = p.User,
                    LastUpdate = p.LastUpdate,
                    VacationDays = p.VacationDays,
                    VacationType = p.VacationType
                }).ToList();


                var request = (from r in context.Requests
                               where r.Employee.Id == user.Id
                               orderby r.DateEnd
                               select new
                               {
                                   r.Id,
                                   r.VacationTypes,
                                   r.Description,
                                   r.DateStart,
                                   r.DateEnd,
                                   Reason = (from appr in context.RequestChecks
                                             where appr.Request == r
                                             select appr.Reason).ToList(),
                                   Approvers = (from appr in context.RequestChecks
                                                where appr.Request == r
                                                select appr).ToList()
                               }).ToList();


                history.AllVacations = request.Select(p => new HistoryOfVacationViewModel
                {
                    Id = p.Id,
                    VacationType = p.VacationTypes.Name,
                    Description = p.Description,
                    DateStart = p.DateStart,
                    DateEnd = p.DateEnd,
                    Days = ((p.DateEnd - p.DateStart).Days + 1).ToString(),
                    Approvers = p.Approvers.Select(x => new RequestChecks
                    {
                        Id = x.Id,
                        Approver = x.Approver,
                        Priority = x.Priority,
                        Status = x.Status,
                        Reason = x.Reason,
                        Request = x.Request
                    }).ToList(),
                    Reson = string.Join("", p.Reason)
                }).ToList();

                return history;
            }
        }
    }
}