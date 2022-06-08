// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CloudArchive.Services;
using ARCHIVE.COMMON.Entities;
using System.Collections.Generic;
using BackgroudServices.ScheduledTasks.OCR.Common;
using Microsoft.EntityFrameworkCore;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Servises;

namespace CloudArchive.ScheduledTasks
{
    public class OCRSenderService : IScheduledTask
    {
        public string ServiceName { get => "OCRSenderService"; }

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly int TakeCount = 10;
        public OCRSenderService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }


        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                IStorageService<StoredFile> _storageService = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                IAdminService _db = scope.ServiceProvider.GetRequiredService<IAdminService>();
                OCRCommon.QuequeImport = _configuration["OCRQuequeImport"];
                OCRCommon.QuequeExport = _configuration["OCRQuequeExport"];
                //                _backgroundServiceLog.AddInfo("Начало работы OCRSenderService", ServiceName);
                var clients = _dbContext.NonFormDocs.AsNoTracking().Where(x => x.OCRState == "Отправка на распознавание").Select(x => x.ClientId.Value).Distinct().ToList();
                using (var connection = OCRCommon.CreateConnection(_configuration, _backgroundServiceLog, ServiceName))
                {
                    using (var channel = OCRCommon.ConnectToChannel(connection, _backgroundServiceLog, ServiceName))
                    {
                        if (channel == null)
                            return;
                        foreach (var clientid in clients)
                        {
                            var docs = FindNextBatch(_dbContext, clientid);
                            string mess = "Отправлено на распознавание документов: ";
                            var client = await _db.SingleAsync<Client, ClientDTO>(c => c.Id.Equals(clientid));
                            foreach (var doc in docs)
                            {
                                if (client.OCRUsed.HasValue && client.OCRQuota.HasValue && client.OCRQuota <= client.OCRUsed)
                                {
                                    doc.OCRState = "Превышен лимит";
                                    mess = "Превышен лимит, к отправке было ";
                                }
                                else
                                {
                                    try
                                    {
                                        OCRCommon.SendTORabbitMq(doc, channel, _dbContext, _storageService, _backgroundServiceLog);
                                    }
                                    catch (Exception e)
                                    {
                                        _backgroundServiceLog.AddError("Ошибка отправки на распознавание NonForm ID: " + doc.Id + " Error: " + e.Message + " StackTrace: " + e.StackTrace, ServiceName, clientid);
                                    }
                                }
                            }
                            if (docs.Count > 0)
                            {
                                _dbContext.SaveChanges();
                                _backgroundServiceLog.AddInfo(mess + docs.Count, ServiceName, clientid);
                            }
                        }
                        channel.Close();
                        connection.Close();
                    }
                }
                //              _backgroundServiceLog.AddInfo("Завершение работы OCRSenderService", ServiceName);
            }
        }


        public List<NonFormDocs> FindNextBatch(SearchServiceDBContext _dbContext, int client)
        {
            List<NonFormDocs> docs = _dbContext.NonFormDocs
                .Where(x => x.ClientId == client && x.OCRState == "Отправка на распознавание" &&
                x.Deleted != true)
                .Take(TakeCount).ToList();
            return docs;
        }
    }
}
