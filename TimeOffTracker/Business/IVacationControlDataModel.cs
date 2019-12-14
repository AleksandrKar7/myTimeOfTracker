using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public interface IVacationControlDataModel
    {
        void BindingMissingVacationByEmail(string email);
        List<string> BindingMissingVacationWithMessageByEmail(string email);

        void UpdateUserVacationDaysByEmail(string email);

        List<UserVacationDays> GetSumUserHistoryVacation(string userEmail, RequestStatuses targetStatus, bool allChainInStatus, DateTime lowerLimit, DateTime upperLimit);
        List<UserVacationDays> GetAllSumUserHistoryVacation(string userEmail, RequestStatuses targetStatus, bool allChainInStatus);
    }
}
