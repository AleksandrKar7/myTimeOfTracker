using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeOffTracker.Data;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public class VacationControlBusiness : IVacationControlBusiness
    {
        IVacationControlData _VCData;

        public VacationControlBusiness(IVacationControlData VCData)
        {
            _VCData = VCData;
        }

        public void ControlUserVacationDays(string userEmail)
        {
            _VCData.BindingMissingVacation(userEmail);
            _VCData.UpdateUserVacationDays(userEmail);
        }   
    }
}