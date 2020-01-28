using Infrastructure.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace DAL.DAL
{
    public class NotifierData : INotifierData
    {
        public Notification GetUserNotificationSetting(string userId)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                return context.Notifications.Where(x => x.Id == userId).FirstOrDefault();
            }
        }
    }
}
