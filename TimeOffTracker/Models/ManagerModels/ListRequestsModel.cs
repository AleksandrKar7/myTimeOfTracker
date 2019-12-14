using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeOffTracker.Models.ManagerModels
{
    public class ListRequestsModel
    {
        public IList<RequestsModel> Items { get; set; }
    }
}