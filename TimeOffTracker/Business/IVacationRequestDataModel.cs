using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public interface IVacationRequestDataModel
    {
        VacationRequestViewModels GetVacationRequest(ApplicationUserManager UserManager, string id);
        string CreateVacationRequest(ApplicationUserManager UserManager, string id, CreateVacationRequestViewModel model);

        ListHistoryOfVacationViewModel GetUserHistoryVacation(ApplicationUserManager UserManager, string id);
    }
}
