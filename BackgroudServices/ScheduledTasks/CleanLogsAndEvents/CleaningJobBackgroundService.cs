using Microsoft.Extensions.DependencyInjection;
using DATABASE.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudArchive.Services;
using BackgroudServices.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CloudArchive.ScheduledTasks
{
    public class CleaningJobBackgroundService : IScheduledTask
    {
        public string ServiceName { get => "CleaningJobBackgroundService"; }

        public IServiceScopeFactory _serviceScopeFactory;
        public CleaningJobBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                try
                {
                    SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    var apiLogs = _dbContext.ApiLogs.AsNoTracking().Where(x => x.Date < DateTime.Today.AddDays(-30));
                    var backgroundSevicelogs = _dbContext.BackgroundServiceLogs.AsNoTracking().Where(x => x.Time < DateTime.Today.AddDays(-10));
                    var oldEvents = _dbContext.UsersEvents.AsNoTracking().Where(x => x.EventDate < DateTime.Today.AddDays(-20));
                    var oldclienttasks = _dbContext.ClientsTasks.AsNoTracking().Where(x => x.EndDate < DateTime.Today.AddDays(-11));
                    _dbContext.RemoveRange(oldEvents);
                    _dbContext.RemoveRange(apiLogs);
                    _dbContext.RemoveRange(backgroundSevicelogs);
                    _dbContext.RemoveRange(oldclienttasks);
                    if (DateTime.Today.Day == 1)
                    {
                        var clientsOCR = _dbContext.Clients.AsNoTracking().Where(x => x.OCRQuota.HasValue);
                        foreach (var client in clientsOCR)
                        {
                            client.OCRUsed = 0;
                            _dbContext.Update(client);
                        }
                    }
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("Error in CleaningJobBackgroundService :" + ex.Message + "StackTrace: " + ex.StackTrace, "CleaningJobBackgroundService");
                }
            }

        }
    }
}
