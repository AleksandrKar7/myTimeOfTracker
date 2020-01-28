using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace Infrastructure.Services.Data
{
    public interface INotifierData
    {
        Notification GetUserNotificationSetting(string userId);
    }
}
