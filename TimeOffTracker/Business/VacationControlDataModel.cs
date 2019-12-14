using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public class VacationControlDataModel : IVacationControlDataModel
    {
        public void BindingMissingVacationByEmail(string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                var listVacationTypes = context.VacationTypes.ToList();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                foreach (var vacType in listVacationTypes)
                {
                    if (DoesUserContainVacationType(vacType, listUserVacation) == false)
                    {
                        context.UserVacationDays.Add(new UserVacationDays()
                        {
                            User = user,
                            LastUpdate = user.EmploymentDate,
                            VacationType = vacType,
                            VacationDays = vacType.MaxDays
                        });
                    }
                }
                context.SaveChanges();
            }
        }

        //Возвращает список отпусков которые были добавлены пользователю
        public List<string> BindingMissingVacationWithMessageByEmail(string email)
        {
            List<string> result = new List<string>();
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                var listVacationTypes = context.VacationTypes.ToList();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                foreach (var vacType in listVacationTypes)
                {
                    if (DoesUserContainVacationType(vacType, listUserVacation) == false)
                    {
                        result.Add(vacType.Name);
                        context.UserVacationDays.Add(new UserVacationDays()
                        {
                            User = user,
                            LastUpdate = user.EmploymentDate,
                            VacationType = vacType,
                            VacationDays = vacType.MaxDays
                        });
                    }
                }
                context.SaveChanges();
            }
            return result;
        }

        private bool DoesUserContainVacationType(VacationTypes vacationType, List<UserVacationDays> userVacations)
        {
            foreach (var item in userVacations)
            {
                if (item.VacationType == vacationType)
                {
                    return true;
                }
            }
            return false;
        }
        private const int daysInYear = 365;

        public void UpdateUserVacationDaysByEmail(string email)
        {
            bool isOk = true;
            DateTime tempLastUpdate = DateTime.Now;
            //Обновление происходит пошагово, для того что бы верно просчитать кол-во дней для старых сотрудников
            do
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                    List<UserVacationDays> userVacations = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();
                    isOk = true;
                    foreach (var userVac in userVacations)
                    {
                        if ((DateTime.Now - userVac.LastUpdate).Days >= daysInYear)
                        {
                            userVac.VacationDays += userVac.VacationType.MaxDays;
                            userVac.LastUpdate = new DateTime(userVac.LastUpdate.AddYears(1).Year, user.EmploymentDate.Month, user.EmploymentDate.Day);
                            tempLastUpdate = userVac.LastUpdate;
                            isOk = false;
                        }
                    }
                    if (isOk == false)
                    {
                        context.SaveChanges();

                        BurnOldYearInUserVacationDays(user.Email, tempLastUpdate);
                    }
                }

            } while (!isOk);
        }


        //Скольк лет нужно отступить перед тем как сжечь 
        private const int countYearsBeforeBurn = 2;
        //Сжигает неиспользованые дни отпуска для каждого типа за один год после отступа countYearsBeforeBurn 
        //основываясь на истории заявок
        private void BurnOldYearInUserVacationDays(string userEmail, DateTime lastUpdate)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == userEmail).First();

                List<UserVacationDays> userVacations = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();
                if ((lastUpdate - user.EmploymentDate).Days >= daysInYear * (countYearsBeforeBurn))
                {
                    RequestStatuses requestStatus = context.RequestStatuses.Where(p => p.Id == 1).First();
                    var userHistory = GetSumUserHistoryVacation(user.Email, requestStatus, true, lastUpdate.AddYears(-countYearsBeforeBurn), lastUpdate.AddYears(-countYearsBeforeBurn + 1));

                    foreach (var userVac in userVacations)
                    {
                        for (int i = 0; i < userHistory.Count; i++)
                        {
                            if (userHistory[i].VacationType.Name == userVac.VacationType.Name)
                            {
                                //Тернарный оператор для защиты от некорректного поведения в случае
                                //если пользователь воспользуется днями начисленными администратором
                                int result = userVac.VacationType.MaxDays - userHistory[i].VacationDays;
                                userVac.VacationDays -= result >= 0 ? result : 0;
                            }
                        }
                    }
                }

                context.SaveChanges();
            }
        }

        //userEmail - email или логин пользователя 
        //targetStatus - сортировка по статусу (если предать null то сортировки по статусу не будет)
        //allChainInStatus - цепочка подтверждений целиком состоит из targetStatus?
        //lowerLimit - нижняя временная граница (включена)
        //upperLimit - верхняя временная граница (включена)
        //Возвращает лист с суммой дней по каждому типу заявки      
        public List<UserVacationDays> GetSumUserHistoryVacation(string userEmail, RequestStatuses targetStatus, bool allChainInStatus, DateTime lowerLimit, DateTime upperLimit)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == userEmail).First();

                List<VacationTypes> vacationTypes = context.VacationTypes.ToList();
                List<Requests> userRequests = context.Requests.Where(m => m.Employee.Id == user.Id).OrderBy(o => o.DateStart).ToList();

                List<UserVacationDays> sumDays = new List<UserVacationDays>();

                //Заполняем лист всеми возможными типами отпуска
                foreach (var item in vacationTypes)
                {
                    sumDays.Add(new UserVacationDays { User = user, VacationDays = 0, VacationType = item });
                }

                foreach (var hVac in userRequests)
                {
                    if (hVac.DateStart.Date >= lowerLimit.Date && hVac.DateStart.Date <= upperLimit.Date)
                    {
                        for (int i = 0; i < sumDays.Count; i++)
                        {
                            if (sumDays[i].VacationType.Name == hVac.VacationTypes.Name)
                            {
                                //Добавляем 1 день для того что бы обе границы были включительны  
                                if (targetStatus == null)
                                {
                                    sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                                }
                                else if (allChainInStatus && DoesAllChainContainSatus(context, targetStatus, hVac))
                                {
                                    sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                                }
                                else if (!allChainInStatus && DoesChainContainStatus(context, targetStatus, hVac))
                                {
                                    sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                                }
                            }
                        }
                    }
                }

                return sumDays;
            }
        }



        //userEmail - email или логин пользователя 
        //targetStatus - сортировка по статусу (если предать null то сортировки по статусу не будет)
        //allChainInStatus - цепочка подтверждений целиком состоит из targetStatus?
        //Возвращает лист с суммой дней по каждому типу заявки      
        public List<UserVacationDays> GetAllSumUserHistoryVacation(string userEmail, RequestStatuses targetStatus, bool allChainInStatus)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == userEmail).First();

                List<VacationTypes> vacationTypes = context.VacationTypes.ToList();
                List<Requests> userRequests = context.Requests.Where(m => m.Employee.Id == user.Id).OrderBy(o => o.DateStart).ToList();

                List<UserVacationDays> sumDays = new List<UserVacationDays>();

                //Заполняем лист всеми возможными типами отпуска
                foreach (var item in vacationTypes)
                {
                    sumDays.Add(new UserVacationDays { User = user, VacationDays = 0, VacationType = item });
                }

                foreach (var hVac in userRequests)
                {
                    for (int i = 0; i < sumDays.Count; i++)
                    {
                        if (sumDays[i].VacationType.Name == hVac.VacationTypes.Name)
                        {
                            //Добавляем 1 день для того что бы обе границы были включительны  
                            if (targetStatus == null)
                            {
                                sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                            }
                            else if (allChainInStatus && DoesAllChainContainSatus(context, targetStatus, hVac))
                            {
                                sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                            }
                            else if (!allChainInStatus && DoesChainContainStatus(context, targetStatus, hVac))
                            {
                                sumDays[i].VacationDays += ((hVac.DateEnd.Date.AddDays(1)) - hVac.DateStart.Date).Days;
                            }
                        }
                    }
                }

                return sumDays;
            }
        }



        private bool DoesChainContainStatus(ApplicationDbContext context, RequestStatuses type, Requests request)
        {
            List<RequestChecks> requestChecks = context.RequestChecks.Where(m => m.Request.Id == request.Id).ToList();
            if (requestChecks.Count == 0)
            {
                return false;
            }
            foreach (var check in requestChecks)
            {
                if (check.Status.Id == type.Id)
                {
                    return true;
                }
            }
            return false;
        }

        private bool DoesAllChainContainSatus(ApplicationDbContext context, RequestStatuses type, Requests request)
        {
            List<RequestChecks> requestChecks = context.RequestChecks.Where(m => m.Request.Id == request.Id).ToList();
            if (requestChecks.Count == 0)
            {
                return false;
            }
            foreach (var check in requestChecks)
            {
                if (check.Status.Id != type.Id)
                {
                    return false;
                }
            }
            return true;
        }
    }
}