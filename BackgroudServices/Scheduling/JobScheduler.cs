using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;

namespace BackgroudServices.Scheduling
{

    public static class JobScheduler
    {
        private static Dictionary<string, string> JobSchedules
        {
            get
            {
                Dictionary<string, string> _Schedules = new Dictionary<string, string>();
                _Schedules.Add("CleaningJobBackgroundService", "1 0 1/1 * *");
                _Schedules.Add("BackUpClientBackground", "39 0 1/1 * *");
                _Schedules.Add("CleanYandexStorageJob", "20 8 1/1 * *");
                _Schedules.Add("DeleteRecycledDocumentsService", "0 2 1/1 * *");
                _Schedules.Add("DiadocExportService", "0/10 * * * *");
                _Schedules.Add("DiadocLoadService", "0/10 * * * *");
                _Schedules.Add("DiadocSyncContractService", "0/10 * * * *");
                _Schedules.Add("DiadocSyncMetadataService", "0/10 * * * *");
                _Schedules.Add("FTPBackgroundService", "0 */2 * * *");
                _Schedules.Add("IMAPBackgroundService", "0/5 * * * *");
                _Schedules.Add("MaintanceBackgroundService", "0 0/1 * * *");
                _Schedules.Add("ClientNotificationService", "0 3 1/1 * *");
                _Schedules.Add("WFUserNotificationService", "30 1 * * 1-5");
                _Schedules.Add("RemoveBlockedClientsService", "0 4 1/1 * *");
                _Schedules.Add("OCRSenderService", "0/2 * * * *");
                _Schedules.Add("OCRConsumerService", "0/2 * * * *");
                _Schedules.Add("ContractNotificationService", "0 4 * * 5");
                //_Schedules.Add("MigrationService", "0/5 * * * *");
                return _Schedules;
            }
        }

        public static TimeSpan GetWaitDelay(string JobName)
        {
            DateTime? NextTime = GetNextOccurenceTime(JobName);
            var delay = NextTime.Value - DateTime.UtcNow;
            return delay;
        }

        public static double GetWaitTimer(string JobName)
        {
            var delay = GetWaitDelay(JobName);
            return delay.TotalMilliseconds > 0 ? delay.TotalMilliseconds : 1;
        }

        public static DateTime? GetNextOccurenceTime(string JobName)
        {
            DateTime? NextTime = DateTime.Now;

            if (!JobSchedules.ContainsKey(JobName))
                throw new Exception("Для сервиса" + JobName + " не задано расписание");
            string Schedule = JobSchedules[JobName];
            var ShedArr = Schedule.Split(' ');
            if (ShedArr.Length != 5)
                throw new Exception("Для сервиса" + JobName + " не задан неверный формат расписания");
            var ce = CronExpression.Parse(Schedule);

            NextTime = ce.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local);

            return NextTime;
        }
    }
}
