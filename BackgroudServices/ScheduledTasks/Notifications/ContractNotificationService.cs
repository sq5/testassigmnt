// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;
using COMMON.Utilities;
using COMMON.Common.Services.ContextService;
using Microsoft.EntityFrameworkCore;
using ARCHIVE.COMMON.Entities;
using DATABASE.Services;
using CloudArchive.Services;

namespace CloudArchive.ScheduledTasks
{
    public class ContractNotificationService : IScheduledTask
    {
        public string ServiceName { get => "ContractNotificationService"; }
        public IServiceScopeFactory _serviceScopeFactory;
        private IContextService _context;
        private IUserService _userService;
        private IBackgroundServiceLog _backgroundServiceLog;

        public ContractNotificationService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            using (var scope = _serviceScopeFactory.CreateScope())
            {

                try
                {
                    _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                    _context = scope.ServiceProvider.GetRequiredService<IContextService>();
                    _userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    MailConstructor mailer = new MailConstructor(_context.CommonService, _context.EmailSender);
                    var ctr = _context.DbContext.Contracts.AsNoTracking().Where(x => x.ValidityPeriod.HasValue && x.ValidityPeriod.Value.Date > DateTime.Today.Date && x.ValidityPeriod.Value.Date < DateTime.Today.Date.AddDays(30));
                    var ctrNotify = (from x in _context.DbContext.Contracts.AsNoTracking()
                                     where x.ValidityPeriod.HasValue && x.ValidityPeriod.Value.Date > DateTime.Today.Date && x.ValidityPeriod.Value.Date < DateTime.Today.Date.AddDays(30)
                                     select new ContractDTO
                                     {
                                         Id = x.Id,
                                         CreatedBy = x.CreatedBy,
                                         ContractorName = x.Contractor.Name,
                                         OrganizationName = x.Organization.Name,
                                         ClientId = x.ClientId.Value,
                                         DocKind = x.DocKind == null ? null : x.DocKind,
                                         ValidityPeriod = x.ValidityPeriod,
                                         DocNumber = x.DocNumber,
                                         DocDate = x.DocDate

                                     }).ToList();
                    var ctrNotifyGouped = ctrNotify.OrderBy(x => x.ValidityPeriod).GroupBy(x => x.ClientId);
                    string rowTempExpired = "<tr><td><medium>{0}</medium></td><td align=\"center\"><medium>{1}</medium></td><td align=\"center\"><medium>{2}</medium></td><td align=\"center\"><medium>{3}</medium></td><td align=\"center\"><medium>{4}</medium></td></tr>";
                    string expireDate = "";
                    string docName = "";
                    string doclink = "";
                    string resp = "";
                    foreach (var clientCtr in ctrNotifyGouped)
                    {
                        string clEmail = _context.DbContext.Clients.AsNoTracking().Where(x => x.Id == clientCtr.Key).FirstOrDefault().Email;
                        var clAdm = await _userService.GetUserByEmailAsync(clEmail);
                        var allClientRows = "";
                        if (!string.IsNullOrEmpty(clEmail) && clAdm != null)
                        {
                            try
                            {
                                foreach (var ct in clientCtr)
                                {
                                    try
                                    {
                                        string ctDate = ct.DocDate == null ? "" : " от " + ct.DocDate.Value.ToString("dd.MM.yyyy");
                                        string ctNum = ct.DocNumber == null ? "" : " N " + ct.DocNumber;
                                        string ctType = ct.DocKind == null ? "Договор" : ct.DocKind?.Name;
                                        expireDate = ct.ValidityPeriod.Value.ToString("dd.MM.yyyy");
                                        docName = ctType + ctNum + ctDate;
                                        doclink = "<a href='" + _context.Configuration["HttpClient_Address"] + "/NewStyle/Document/view?ItemId=" + ct.Id + "&SettName=Contracts&SettFormName=Договор" + "'>" + docName + "</a>";
                                        resp = string.IsNullOrEmpty(ct.CreatedBy) || ct.CreatedBy == "ExternalSystem" ? "" : ct.CreatedBy;
                                        if (!string.IsNullOrEmpty(resp))
                                        {
                                            var respUrs = await _userService.GetUserByEmailAsync(resp);
                                            resp = respUrs?.DisplayName;
                                        }
                                        var ctRow = string.Format(rowTempExpired, doclink, ct.OrganizationName, ct.ContractorName, expireDate, resp);
                                        allClientRows += ctRow;
                                    }
                                    catch (Exception ex)
                                    {
                                        _backgroundServiceLog.AddError("Error with contract:" + ct.Id + " Error: " + ex.Message + "StackTrace: " + ex.StackTrace, ServiceName, clientCtr.Key);
                                    }
                                }
                                mailer.SetTemplate(MailTemplate.ContractExpireNotify);
                                mailer.SetValue("%TABLE%", allClientRows);
                                mailer.SetValue("%USERNAME%", clAdm.DisplayName);
                                await mailer.SendMail("Договоры с истекающим сроком действия", clEmail);
                                _backgroundServiceLog.AddInfo("Successfully Send Mail", ServiceName, clientCtr.Key);
                            }
                            catch (Exception ex)
                            {
                                _backgroundServiceLog.AddError("Error with client:" + ex.Message + "StackTrace: " + ex.StackTrace, ServiceName, clientCtr.Key);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("Error:" + ex.Message + "StackTrace: " + ex.StackTrace, ServiceName);

                }
            }

        }
    }
}
