using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Infrastructure.Services
{
    public interface INotifierBusiness
    {
        bool SendTelegramMessage(TelegramBotClient bot, string userId, string message);
    }
}
