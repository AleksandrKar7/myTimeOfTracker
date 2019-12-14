using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TimeOffTracker.Models;
using TimeOffTracker.Models.ManagerModels;

namespace TimeOffTracker.Business
{
    public class ListActiveRequests : IListActiveRequests
    {

        public ListRequestsModel GetListRequestsModel(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                var r = context.RequestChecks
                    .Where(s => s.Status.Name == "Waiting")   // Тестовый статус заменить на прод
                    .AsEnumerable()     // Нужно юзать enumerable, потому что если его не юзать, то будет ругаться на TakeWhile
                    .GroupBy(gr => gr.RequestId)
                    .Select(group => group.OrderBy(o => o.Priority).TakeWhile(w => w.Approver.Id == id))
                    .SelectMany(grop => grop)
                    .Select(rCheck => new
                    {
                        rCheck.Id,
                        rCheck.Request.Employee.FullName,
                        rCheck.Request.Employee.Email,
                        rCheck.Request.DateStart,
                        rCheck.Request.DateEnd,
                        rCheck.Request.VacationTypes.Name,
                        rCheck.Request.Description
                    }).ToList();

                var model = new ListRequestsModel
                {
                    Items = r.Select(s => new RequestsModel
                    {
                        Id = s.Id,
                        FullNameEmployee = s.FullName,
                        EmailEmployee = s.Email,
                        DateStart = s.DateStart,
                        DateEnd = s.DateEnd,
                        VacationType = s.Name,
                        Description = s.Description
                    }).ToList()
                };
                return model;
            }
        }

        public void Confirm(int? id)
        {
            using (var context = new ApplicationDbContext())
            {
                RequestChecks requestChecks = context.RequestChecks.Find(id);
                if (requestChecks != null)
                {
                    context.Entry(requestChecks).State = EntityState.Modified;
                    RequestStatuses status = context.RequestStatuses.Where(s => s.Name == "Passed").Single();
                    requestChecks.Status = status;
                }

                Requests request = requestChecks.Request;
                int difference = ((int)(request.DateEnd - request.DateStart).TotalDays + 1);

                var employeeId = request.EmployeeId;
                var vacationTypeId = request.VacationTypesId;

                UserVacationDays uvd = context.UserVacationDays
                    .Where(u => u.User.Id == employeeId)
                    .Where(v => v.VacationType.Id == vacationTypeId)
                    .Single();

                if (uvd != null)
                {
                    context.Entry(uvd).State = EntityState.Modified;
                    uvd.VacationDays -= difference;
                }
                context.SaveChanges();
            }
        }

        public void Reject(int? id, string reason)
        {
            using (var context = new ApplicationDbContext())
            {
                RequestChecks request = context.RequestChecks.Find(id);
                if (request != null)
                {
                    var rej = context.RequestChecks
                        .Where(w => w.Request.Id == request.Request.Id)
                        .Where(e => e.Priority >= request.Priority)
                        .OrderBy(o => o.Priority)
                        .ToList();

                    RequestStatuses status = context.RequestStatuses.Where(s => s.Name == "Rejected").Single();
                    for (int i = 0; i < rej.Count; i++)
                    {
                        context.Entry(rej[i]).State = EntityState.Modified;
                        rej[i].Status = status;
                        if (i == 0) { rej[i].Reason = reason; }
                    }
                    context.SaveChanges();
                }
            }
        }


        /* Убрать лишние private методы */

        private ApplicationUser GetUser(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Users.Find(id);
            }
        }

        private VacationTypes GetVacationType(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.VacationTypes.Find(id);
            }
        }
        private Requests GetRequest(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Requests.Find(id);
            }
        }

        private RequestStatuses GetStatus(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.RequestStatuses.Find(id);
            }
        }

        public RequestsModel GetRequestsModel(int? id)
        {
            RequestsModel requestsModel;
            using (var context = new ApplicationDbContext())
            {
                var activeRequest = context.RequestChecks.Find(id).Request;

                var list = context.RequestChecks
                    .Where(w => w.Request.Id == activeRequest.Id)
                    .AsEnumerable()
                    .Select(s => new RequestChecks
                    {
                        Approver = GetUser(s.ApproverId),
                        Id = s.Id,
                        Priority = s.Priority,
                        Status = GetStatus(s.StatusId),
                        Reason = s.Reason,
                        Request = GetRequest(s.RequestId)
                    }).ToList();

                requestsModel = new RequestsModel
                {
                    Id = id.Value,
                    DateStart = activeRequest.DateStart,
                    DateEnd = activeRequest.DateEnd,
                    FullNameEmployee = activeRequest.Employee.FullName,
                    VacationType = activeRequest.VacationTypes.Name,
                    Approvers = list,
                    EmailEmployee = activeRequest.Employee.Email,
                    Description = activeRequest.Description
                };
            }
            return requestsModel;
        }

        public ListRequestsModel GetInfoRequestsModel(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                // Лист ID реквестов там где есть текущий аппр
                var request = context.RequestChecks
                    .OrderBy(o => o.Request.Employee.FullName)
                    .Where(w => w.ApproverId == id)
                    .AsEnumerable()
                    .Select(d => d.Id).ToList();

                var item = new List<RequestsModel>();
                foreach (var i in request)
                {
                    item.Add(GetRequestsModel(i));
                }

                return new ListRequestsModel() { Items = item };
            }
        }
    }
}