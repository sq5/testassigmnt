﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DATABASE.Context;
using CloudArchive.Services;
using CloudArchive.Services.EDI.EnsolDiadoc;
using CloudArchive.Services.EDI.Settings;
using CloudArchive.Services.EDI;
using ARCHIVE.COMMON.DTOModels.Admin;
using BackgroudServices.Scheduling;
using COMMON.Common.Services.StorageService;
using COMMON.Models;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocExportService : IScheduledTask
    {
        public string ServiceName { get => "DiadocExportService"; }
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;

        public DiadocExportService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _timerJobLogService = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                try
                {
                    IAdminService _db = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    ICommonService _commonService = scope.ServiceProvider.GetRequiredService<ICommonService>();
                    IStorageService<StoredFile> _fileStorage = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                    SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    List<IClientWorker> AllWorkers = null;
                    do
                    {
                        AllWorkers = await GetWorkers(_dbContext, _timerJobLogService, AllWorkers, _db, _commonService, _fileStorage);
                        foreach (IClientWorker worker in AllWorkers)
                        {
                            try
                            {
                                worker.ProcessBatch();
                            }
                            catch (Exception e)
                            {
                                worker.Completed = true;
                                _timerJobLogService.AddError("Произощла ошибка во время экспорта документов по подключению " + worker.Settings.EdiSettings.ConnectionInfo.OrganizationName + ": " + e.Message, ServiceName, worker.Settings.EdiSettings.ConnectionInfo.ClientID);
                            }
                        }
                    } while (AllWorkers != null && AllWorkers.Count > 0);
                }
                catch (Exception ex)
                {
                    _timerJobLogService.AddError(ex.Message + "StackTrace: " + ex.StackTrace, ServiceName);
                }
            }
        }

        private async Task<List<IClientWorker>> GetWorkers(SearchServiceDBContext _dbContext, IBackgroundServiceLog _timerJobLogService, List<IClientWorker> AllWorkers, IAdminService _db, ICommonService _commonService, IStorageService<StoredFile> _fileStorage)
        {
            if (AllWorkers == null || AllWorkers.Count == 0)
            {
                GeneralJobSettings genSettings = new GeneralJobSettings(ServiceName, _timerJobLogService, _configuration, _dbContext, _fileStorage);

                AllWorkers = new List<IClientWorker>();
                List<EDISettings> Settings = _dbContext.EDISettings.Where(x => x.EDIProvider == "Diadoc" || x.EDIProvider == "Диадок").ToList();
                foreach (EDISettings set in Settings)
                {
                    var client = await _db.SingleAsync<Client, ClientDTO>(c => c.Id.Equals(set.ClientID));
                    var checkQuota = _commonService.CheckClientIsActive(client);
                    if (checkQuota.res)
                    {
                        DiadocSettings ediSettings = new DiadocSettings(set, _configuration["DiadocApiClientID"]);
                        EDIClientSettings clientsettings = new EDIClientSettings();
                        DiadocJobSettings settings = new DiadocJobSettings();
                        settings.ClientSettings = clientsettings;
                        settings.EdiSettings = ediSettings;
                        settings.GeneralSettings = genSettings;
                        DiadocExportContractWorker wrk = new DiadocExportContractWorker(settings);
                        AllWorkers.Add(wrk);
                        DiadocExportMetadataWorker wrk2 = new DiadocExportMetadataWorker(settings);
                        AllWorkers.Add(wrk2);
                    }
                    else
                    {
                        _timerJobLogService.AddError(checkQuota.err, ServiceName, client.Id);
                    }
                }
            }
            AllWorkers = AllWorkers.Where(x => !x.Completed).ToList();
            return AllWorkers;
        }
    }
}