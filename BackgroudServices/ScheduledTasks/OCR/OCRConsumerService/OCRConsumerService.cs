// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CloudArchive.Services;
using BackgroudServices.ScheduledTasks.OCR.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using COMMON.Common.Services.ContextService;
using ARCHIVE.COMMON.Servises;
using System.Collections.Generic;

namespace CloudArchive.ScheduledTasks
{
    public class OCRConsumerService : IScheduledTask
    {
        public string ServiceName { get => "OCRConsumerService"; }

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        public OCRConsumerService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IAdminService _adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                IStorageService<StoredFile> _storageService = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                OCRCommon.QuequeImport = _configuration["OCRQuequeImport"];
                OCRCommon.QuequeExport = _configuration["OCRQuequeExport"];
                //               _backgroundServiceLog.AddInfo("Начало работы OCRConsumerService", ServiceName);
                var hasMessages = true;
                var AnyDocsToRecieve = _dbContext.NonFormDocs.AsNoTracking().Where(x => x.OCRState == "На распознавании").Select(x => x.Id).Any();
                if (AnyDocsToRecieve)
                {
                    using (var connection = OCRCommon.CreateConnection(_configuration, _backgroundServiceLog, ServiceName))
                    {
                        using (var channel = OCRCommon.ConnectToChannel(connection, _backgroundServiceLog, ServiceName))
                        {
                            if (channel == null)
                                return;
                            var docsByClients = new Dictionary<int, int>();
                            try
                            {
                                while (hasMessages)
                                {
                                    try
                                    {
                                        var message = OCRCommon.ReadFromRabbitMq(channel, _dbContext, _storageService, _backgroundServiceLog);
                                        if (message != null)
                                        {
                                            var doc = OCRCommon.ParseRabbitMqMessage(message, _dbContext, _storageService, _backgroundServiceLog, _adminService);
                                            if (doc != null && doc.OCRXML != null)
                                            {
                                                OCRCommon.ParseXMLFile(doc, _backgroundServiceLog, _dbContext, ServiceName);
                                                _backgroundServiceLog.AddInfo("Получены результаты распознавания по NonFormID: " + doc.Id, ServiceName, doc.ClientId.Value);
                                            }
                                            bool exist = docsByClients.ContainsKey(doc.ClientId.Value);
                                            if (!exist)
                                                docsByClients.Add(doc.ClientId.Value, 1);
                                            else
                                                docsByClients[doc.ClientId.Value] += 1;
                                        }
                                        else
                                            hasMessages = false;
                                    }
                                    catch (Exception e)
                                    {
                                        _backgroundServiceLog.AddError("Произошла ошибка во время работы получения результатов распознавания " + e.Message + " StackTrace: " + e.StackTrace, ServiceName);
                                        hasMessages = false;
                                    }
                                    //_dbContext.SaveChanges();
                                }
                                foreach (int clientId in docsByClients.Keys)
                                {
                                    var client = _dbContext.Clients.AsNoTracking().Where(x => x.Id == clientId).FirstOrDefault();
                                    int ocrUsed = client.OCRUsed.HasValue ? client.OCRUsed.Value : 0;
                                    client.OCRUsed = ocrUsed + docsByClients[clientId];
                                    _dbContext.Update(client);
                                }
                                _dbContext.SaveChanges();
                                channel.Close();
                                connection.Close();
                            }
                            catch (Exception e)
                            {
                                _backgroundServiceLog.AddError("Произошла ошибка во время работы TJ " + e.Message + " StackTrace: " + e.StackTrace, ServiceName);
                            }
                        }
                    }
                }
                //                _backgroundServiceLog.AddInfo("Завершение работы OCRConsumerService", ServiceName);
            }
        }
    }
}
