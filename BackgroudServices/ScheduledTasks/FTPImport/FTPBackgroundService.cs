using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Threading;
using System.Threading.Tasks;
using CloudArchive.Services;
using BackgroudServices.Scheduling;
using DATABASE.Context;
using System.Linq;

namespace CloudArchive.ScheduledTasks
{
    public class FTPBackgroundService : IScheduledTask
    {
        public string ServiceName { get => "FTPBackgroundService"; }
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FTPBackgroundService(IServiceScopeFactory serviceScopeFactory)
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
                ICommonService _commonService = scope.ServiceProvider.GetRequiredService<ICommonService>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                var connectionStrings = _adminService.GetAsync<ExtConnection, ExtConnectionDTO>(t => t.IsActive && t.Type == "FTP").Result;
                if (connectionStrings != null && connectionStrings.Count > 0)
                {
                    foreach (var connectionString in connectionStrings)
                    {
                        try
                        {
                            var client = _adminService.SingleAsync<Client, ClientDTO>(t => t.Id == connectionString.ClientId).Result;
                            if (client == null || client.Token == null)
                            {
                                _backgroundServiceLog.AddError("Error in FTPBackgroundService. Не найден токен" + connectionString.Id, "FTPBackgroundService", client.Id);
                                continue;
                            }
                            var checkQuota = _commonService.CheckClientIsActive(client);
                            if (!checkQuota.res)
                            {
                                continue;
                            }
                            if (client.TariffId.HasValue)
                            {
                                string tariff = _dbContext.Tariffs.FirstOrDefault(x => x.Id == client.TariffId)?.Name;
                                if (client.LastLogin.HasValue && client.LastLogin.Value.AddDays(5) <= DateTime.Today && tariff == "Старт")
                                {
                                    //_backgroundServiceLog.AddError("Клиент на стартовом тарифе и не заходил в систему более 5 дней", "FTPBackgroundService", client.Id);
                                    continue;
                                }
                            }
                            FTPService FTP = new FTPService(connectionString, _backgroundServiceLog, _commonService);
                            FTP.Process();
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("Error in FTPBackgroundService" + connectionString.Id + ex.Message + " StackTrace: " + ex.StackTrace, "FTPBackgroundService", connectionString.ClientId);
                        }
                    }
                }
            }
        }
    }
}
