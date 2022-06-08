using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using BackgroudServices.Scheduling;
using CloudArchive.Services;
using DATABASE.Context;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CloudArchive.ScheduledTasks
{
    public class IMAPBackgroundService : IScheduledTask
    {
        public string ServiceName { get => "IMAPBackgroundService"; }

        public IServiceScopeFactory _serviceScopeFactory;
        public IMAPBackgroundService(IServiceScopeFactory serviceScopeFactory)
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
                IEmailService _emailSender = scope.ServiceProvider.GetRequiredService<IEmailService>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                var connectionStrings = _adminService.GetAsync<ExtConnection, ExtConnectionDTO>(t => t.IsActive && (t.Type == "IMAP" || t.Type == "E-mail")).Result;
                if (connectionStrings != null && connectionStrings.Count > 0)
                {
                    List<string> AllUsersEmails = _dbContext.Users.AsNoTracking().Select(x => x.Email.ToLower()).ToList();
                    foreach (var connectionString in connectionStrings)
                    {
                        try
                        {
                            var client = _adminService.SingleAsync<Client, ClientDTO>(t => t.Id == connectionString.ClientId).Result;
                            if (client == null || client.Token == null)
                            {
                                _backgroundServiceLog.AddError("Error in IMAPBackgroundService. Не найден токен", "IMAPBackgroundService", client.Id);
                                continue;
                            }
                            var checkQuota = _commonService.CheckClientIsActive(client);
                            if (!checkQuota.res)
                            {
                                _backgroundServiceLog.AddError(checkQuota.err + ". Conn ID", "IMAPBackgroundService", client.Id);
                                continue;
                            }
                            if (client.TariffId.HasValue)
                            {
                                string tariff = _dbContext.Tariffs.FirstOrDefault(x => x.Id == client.TariffId)?.Name;
                                if (client.LastLogin.HasValue && client.LastLogin.Value.AddDays(5) <= DateTime.Today && tariff == "Старт")
                                {
                                    //_backgroundServiceLog.AddError("Клиент на стартовом тарифе и не заходил в систему более 5 дней", "IMAPBackgroundService", client.Id);
                                    continue;
                                }
                            }
                            var IMAP = new IMAPService(connectionString, _backgroundServiceLog, client, _commonService, _emailSender);
                            await IMAP.Process(AllUsersEmails);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("Error in IMAPBackgroundService. Connection " + connectionString.Id + ". Error " + ex.Message + "StackTrace: " + ex.StackTrace, "IMAPBackgroundService", connectionString.ClientId);
                        }
                    }
                }
            }
        }
    }
}
