using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeOffTracker.Data
{
    public interface IVacationControlData
    {
        void BindingMissingVacation(string email);
        void UpdateUserVacationDays(string email);
    }
}
