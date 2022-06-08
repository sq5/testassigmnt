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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CloudArchive.Services;
using Org.BouncyCastle.Crypto.Tls;

namespace CloudArchive.ScheduledTasks
{
    public class MigrationService : IScheduledTask
    {
        public string ServiceName { get => "MigrationService"; }

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;

        public MigrationService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            if (_configuration["AzureStorageStartMigration"] == "1")
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IBackgroundServiceLog _timerJobLogService = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                    SearchServiceDBContext _searchServiceDBContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    _timerJobLogService.AddInfo("Начало работы службы миграций", ServiceName);
                    IStorageService<AzureStoredFile> _storageService = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                    var idtomove = _searchServiceDBContext.Files.Where(t => t.BlobUrl == null).Select(t => new { ID = t.Id }).Take(100);
                    foreach (var ids in idtomove)
                    {
                        long fileid = ids.ID;
                        _timerJobLogService.AddInfo("Переносим файл ИД: " + fileid, ServiceName);
                        var file = _searchServiceDBContext.Files.Where(t => t.Id == fileid).FirstOrDefault();
                        try
                        {
                            if (await _storageService.UploadFileAsync(file.Id, file.FileBin))
                            {
                                file.BlobUrl = _storageService.GetUrl(file);
                            }
                        }
                        catch (Exception e)
                        {
                            _timerJobLogService.AddError("Произошла ошибка во время работы службы миграций File: " + fileid + " Error: " + e.Message + " StackTrace: " + e.StackTrace, ServiceName);
                            file.BlobUrl = "error: " + e.Message;
                        }
                        _searchServiceDBContext.SaveChanges();
                    }
                    _timerJobLogService.AddInfo("Завершение работы службы миграций", ServiceName);
                }
            }
        }
    }
}
