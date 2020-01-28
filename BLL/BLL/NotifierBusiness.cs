using Infrastructure.Services;
using Infrastructure.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TimeOffTracker.Models;

namespace TimeOffTracker.BLL
{
    public class NotifierBusiness : INotifierBusiness
    {
        INotifierData _notifierData;
        public NotifierBusiness(INotifierData notifierData)
        {
            _notifierData = notifierData;
        }

        public bool SendTelegramMessage(TelegramBotClient bot, string userId, string message)
        {
            var userNotify = _notifierData.GetUserNotificationSetting(userId);
            if (userNotify != null && userNotify.NotifyByTelegram == true && userNotify.TelegramChatID != null)
            {
                bot.SendTextMessageAsync(userNotify.TelegramChatID, message);
                return true;
            }

            return false;
        }
    }
}
