using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace TimeOffTracker.BLL
{
    public interface IVacationControlBusiness
    {
        void ControlUserVacationDays(string userEmail);
    }
}
