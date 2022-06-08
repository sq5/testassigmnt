using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using DATABASE.Context;
using System;
using System.Threading;
using System.Threading.Tasks;
using CloudArchive.Services;
using BackgroudServices.Scheduling;
using COMMON.Common.Services.StorageService;
using COMMON.Models;

namespace CloudArchive.ScheduledTasks
{
    public class RemoveBlockedClientsService : IScheduledTask
    {
        public string ServiceName { get => "RemoveBlockedClientsService"; }
        public IServiceScopeFactory _serviceScopeFactory;

        public RemoveBlockedClientsService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                IAdminService _adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                var fileStorage = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                var clients = await _adminService.GetAsync<Client, ClientDTO>(t => t.BlockDate.HasValue == true && t.Blocked.HasValue == true && t.Blocked.Value == true && t.BlockDate.Value.AddDays(90) <= DateTime.Now);
                if (clients != null && clients.Count > 0)
                {
                    _backgroundServiceLog.AddInfo("TimerJobBlockClient. Найдено клиентов на удаление = " + clients.Count, "TimerJobBlockClient");
                    foreach (var client in clients)
                    {
                        try
                        {
                            var RemoveBlockedClientsWorker = new RemoveBlockedClientsWorker(client, _dbContext, fileStorage);
                            RemoveBlockedClientsWorker.Process();
                            _backgroundServiceLog.AddInfo($"TimerJobBlockClient. Delete BlockClient = {client.Id}", "TimerJobBlockClient", client.Id);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("Error in TimerJobBlockClient: " + ex.Message + "StackTrace: " + ex.StackTrace, "TimerJobBlockClient", client.Id);
                        }
                    }
                }
            }
        }
    }
}
