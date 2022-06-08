// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.Servises;
using DATABASE.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;
using CloudArchive.Services;
using COMMON.Utilities;

namespace CloudArchive.ScheduledTasks
{
    public class ClientNotificationService : IScheduledTask
    {
        public string ServiceName { get => "ClientNotificationService"; }
        public IServiceScopeFactory _serviceScopeFactory;
        private IEmailService _emailSender;

        public ClientNotificationService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                    IAdminService _adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    _emailSender = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    ICommonService _commonService = scope.ServiceProvider.GetRequiredService<ICommonService>();
                    MailConstructor mailer = new MailConstructor(_commonService, _emailSender);
                    mailer.SetTemplate(MailTemplate.BillPaymentNotif);
                    var clientsNotify = (from i in _dbContext.Billings
                                         join c in _dbContext.Clients
                                         on i.ClientId equals c.Id
                                         where i.Paid != true && c.Blocked != true && i.Type == "Счет" &&
                                         (i.Date.AddDays(1).Date == DateTime.Today || i.Date.AddDays(3).Date == DateTime.Today || i.Date.AddDays(6).Date == DateTime.Today
                                         || i.Date.AddDays(9).Date == DateTime.Today)
                                         select new { Id = (int)i.ClientId, Email = c.Email, Name = c.Name, Date = i.Date });
                    foreach (var client in clientsNotify)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(client.Email))
                                continue;
                            mailer.SetValue("%ORG%", client.Name);
                            mailer.SetValue("%DATE%", client.Date.AddDays(10).ToString("dd.MM.yyyy"));
                            await mailer.SendMail("Оплата счета", client.Email);
                            _backgroundServiceLog.AddInfo("ClientNotificationService Send Mail - BillPaymentNotif", "ClientNotificationService", client.Id);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("ClientNotificationService.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "ClientNotificationService", client.Id);
                        }
                    }
                    var clientsNeedBlock = (from i in _dbContext.Billings
                                            join c in _dbContext.Clients
                                            on i.ClientId equals c.Id
                                            where i.Paid != true && c.Blocked != true && (i.Date.AddDays(10) <= DateTime.Today && i.Type == "Счет")
                                            select c);
                    foreach (var client in clientsNeedBlock)
                    {
                        try
                        {
                            client.BlockDate = DateTime.Now;
                            client.Blocked = true;
                            _backgroundServiceLog.AddInfo("ClientNotificationService - Blocked Client", "ClientNotificationService", client.Id);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("ClientNotificationService - Blocked Client.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "ClientNotificationService", client.Id);
                        }
                    }
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
