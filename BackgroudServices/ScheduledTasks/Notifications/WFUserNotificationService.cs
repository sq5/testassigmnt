// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using DATABASE.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DATABASE.Services;
using ARCHIVE.COMMON.DTOModels.UI;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using BackgroudServices.Scheduling;
using CloudArchive.Services;
using COMMON.Utilities;
using Microsoft.AspNetCore.Http;

namespace CloudArchive.ScheduledTasks
{
    public class WFUserNotificationService : IScheduledTask
    {
        public string ServiceName { get => "WFUserNotificationService"; }
        public IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _cfg;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WFUserNotificationService(IServiceScopeFactory serviceScopeFactory, 
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _cfg = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                IAdminService _adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                ICommonService _commonService = scope.ServiceProvider.GetRequiredService<ICommonService>();
                IUserService _userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                IEmailService _emailSender = scope.ServiceProvider.GetRequiredService<IEmailService>();
                List<UsersTasks> tasks;
                List<ARCHIVE.COMMON.DTOModels.UserDTO> users;
                int docssent = 0;
                try
                {
                    var clients = _dbContext.Clients.AsNoTracking().Where(x => x.Blocked != true && x.LastLogin.HasValue && !(x.Tariff.Name == "Старт" && x.LastLogin.Value.AddDays(10) <= DateTime.Today));
                    foreach (var client in clients)
                    {
                        try
                        {
                            Dictionary<string, string> activetasks = new Dictionary<string, string>();
                            Dictionary<string, string> expiredocs = new Dictionary<string, string>();
                            users = _userService.GetUsersByClient(client.Id);
                            List<string> emails = users.Select(x => x.Email).ToList();
                            tasks = _dbContext.UsersTasks.AsNoTracking().Where(x => x.Active && emails.Contains(x.Users)).ToList();
                            ProcessTasks(tasks, users, activetasks, expiredocs, _dbContext);
                            await SendMails(activetasks, expiredocs, client.Id, _backgroundServiceLog, _commonService, _emailSender);
                            docssent = activetasks.Count + expiredocs.Count;
                            if (docssent > 0)
                                _backgroundServiceLog.AddInfo("WFUserNotificationService. Отправлено писем по клиенту" + docssent, "WFUserNotificationService", client.Id);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("WFUserNotificationService general client.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "WFUserNotificationService", client.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("WFUserNotificationService general.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "WFUserNotificationService");
                }
            }
        }

        private void ProcessTasks(List<UsersTasks> tasks, List<ARCHIVE.COMMON.DTOModels.UserDTO> users, Dictionary<string, string> activetasks, Dictionary<string, string> expiredocs, SearchServiceDBContext _dbContext)
        {
            MetadataDTO MDocument;
            ContractDTO CDocument;
            CultureInfo culture = new CultureInfo("ru-RU");
            string Deadline;
            string Resp;
            string DocName = "";
            string Doclink;
            string DocID = "";
            string Settname = "";
            string RowTempExpired = "<tr><td><medium>{0}</medium></td><td align=\"center\"><medium>{1}</medium></td><td align=\"center\"><medium>{2}</medium></td></tr>";
            string RowTempMyTsk = "<tr><td><medium>{0}</medium></td><td align=\"center\"></medium>{1}</medium></td></tr>";
            foreach (var task in tasks)
            {
                //получаем название документа, ссылку, срок, ответственного
                Resp = users.Where(x => x.Email == task.Users).FirstOrDefault().DisplayName;
                Deadline = task.DeadLine == null ? "" : task.DeadLine.Value.ToString("dd.MM.yyyy", culture);
                if (task.MetadataId > 0)
                {
                    MDocument = Ensol.CommonUtils.Common.GetMetadataByID(task.MetadataId.Value, _dbContext);
                    string Contractor = MDocument.Contractor == null ? "" : " контрагент " + MDocument.Contractor.Name;
                    DocName = MDocument.DocType + " " + MDocument.DocNumber + Contractor;
                    DocID = MDocument.Id.ToString();
                    Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == MDocument.DocTypeId).FirstOrDefault().Reestr;
                }
                else if (task.ContractId > 0)
                {
                    CDocument = Ensol.CommonUtils.Common.GetContractByID(task.ContractId.Value, _dbContext);
                    string Contractor = CDocument.Contractor == null ? "" : " контрагент " + CDocument.Contractor.Name;
                    DocName = "Договор " + CDocument.DocNumber + Contractor;
                    DocID = CDocument.Id.ToString();
                    Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == CDocument.DocTypeId).FirstOrDefault().Reestr;
                }

                Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + DocID + "&SettName=" + Settname + "'>" + DocName + "</a>";
                //формируем словарь для просточенных документов, по которым я запустил процесс
                if (task.DeadLine != null && task.DeadLine < DateTime.Today)
                {
                    var starttaskS = _dbContext.UsersTasks.AsNoTracking().Where(x => x.Stage == task.Stage && x.MetadataId == task.MetadataId && x.ContractId == task.ContractId && x.Order == 0);
                    if (starttaskS.Any())
                    {
                        var RowExpired = string.Format(RowTempExpired, Doclink, Resp, Deadline);
                        if (expiredocs.ContainsKey(starttaskS.FirstOrDefault().Users))
                        {
                            RowExpired += expiredocs.GetValueOrDefault(starttaskS.FirstOrDefault().Users);
                            expiredocs[starttaskS.FirstOrDefault().Users] = RowExpired;
                        }
                        else
                            expiredocs.Add(starttaskS.FirstOrDefault().Users, RowExpired);
                    }
                }
                //формируем словарь по текущим задачам пользователя
                var RowMyTsk = string.Format(RowTempMyTsk, Doclink, Deadline);
                if (activetasks.ContainsKey(task.Users))
                {
                    RowMyTsk += activetasks.GetValueOrDefault(task.Users);
                    activetasks[task.Users] = RowMyTsk;
                }
                else
                    activetasks.Add(task.Users, RowMyTsk);
            }
        }

        private async Task SendMails(Dictionary<string, string> activetasks, Dictionary<string, string> expiredocs, int clientId, IBackgroundServiceLog _backgroundServiceLog, ICommonService _commonService, IEmailService _emailSender)
        {
            MailConstructor mailer = new MailConstructor(_commonService, _emailSender);
            // отправляем письма по просроченным
            foreach (var exp in expiredocs)
            {
                try
                {
                    mailer.SetTemplate(MailTemplate.WFMyExpiredDocs);
                    mailer.SetValue("%TABLE%", exp.Value);
                    await mailer.SendMail("Просроченные задачи по Вашим документам", exp.Key);
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("WFUserNotificationService send mail exp.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "WFUserNotificationService", clientId);
                }
            }
            //отправляем письма по моим задачам
            foreach (var mytsk in activetasks)
            {
                try
                {
                    mailer.SetTemplate(MailTemplate.WFMyTasks);
                    mailer.SetValue("%TABLE%", mytsk.Value);
                    await mailer.SendMail("Ваши активные задачи", mytsk.Key);
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("WFUserNotificationService send mail task.Error:" + ex.Message + "StackTrace: " + ex.StackTrace, "WFUserNotificationService", clientId);
                }
            }
        }
    }
}
