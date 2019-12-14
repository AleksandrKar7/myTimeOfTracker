using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models.ManagerModels;

namespace TimeOffTracker.Business
{
    public interface IListActiveRequests
    {
        ListRequestsModel GetListRequestsModel(string id);
        ListRequestsModel GetInfoRequestsModel(string id);
        RequestsModel GetRequestsModel(int? id);
        void Reject(int? id, string reason);
        void Confirm(int? id);
    }
}
