using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeOffTracker.Models.ManagerModels
{
    public class RequestsModel
    {
        public int Id { get; set; } // id Request
        public string FullNameEmployee { get; set; }
        public string EmailEmployee { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string VacationType { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public List<RequestChecks> Approvers { get; set; }
    }
}