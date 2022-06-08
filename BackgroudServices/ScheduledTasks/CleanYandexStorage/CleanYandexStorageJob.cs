using BackgroudServices.Scheduling;
using CloudArchive.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudArchive.ScheduledTasks
{
    public class CleanYandexStorageJob : IScheduledTask
    {
        public string ServiceName { get => "CleanYandexStorageJob"; }

        public IServiceScopeFactory _serviceScopeFactory;
        public CleanYandexStorageJob(IServiceScopeFactory serviceScopeFactory)
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
                    ICleanBackupStorageService cleanBackupStorageService = scope.ServiceProvider.GetService<ICleanBackupStorageService>();
                    await cleanBackupStorageService.CleanAsync();
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("Error in CleanYandexStorageJob." + ex.Message + "StackTrace: " + ex.StackTrace, "CleanYandexStorageJob");
                }
            }
        }
    }
}
