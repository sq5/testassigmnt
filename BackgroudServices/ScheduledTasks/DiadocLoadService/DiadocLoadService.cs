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
using CloudArchive.Services.EDI.Settings;
using CloudArchive.Services.EDI.EnsolDiadoc;
using ARCHIVE.COMMON.DTOModels.Admin;
using BackgroudServices.Scheduling;
using COMMON.Common.Services.StorageService;
using COMMON.Models;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocLoadService : IScheduledTask
    {
        public string ServiceName { get => "DiadocLoadService"; }
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;

        public DiadocLoadService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
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
                    List<DiadocLoadClientWorker> AllWorkers = null;
                    do
                    {
                        AllWorkers = await GetWorkers(_dbContext, _timerJobLogService, AllWorkers, _db, _commonService, _fileStorage);
                        foreach (DiadocLoadClientWorker worker in AllWorkers)
                        {
                            try
                            {
                                worker.ProcessBatch();
                            }
                            catch (Exception e)
                            {
                                worker.Completed = true;
                                _timerJobLogService.AddError("Произошла ошибка во время синхронизации документов по подключению " + worker.Settings.EdiSettings.ConnectionInfo.OrganizationName + ": " + e.Message + "StackTrace: " + e.StackTrace, ServiceName, worker.Settings.EdiSettings.ConnectionInfo.ClientID);
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

        private async Task<List<DiadocLoadClientWorker>> GetWorkers(SearchServiceDBContext _dbContext, IBackgroundServiceLog _timerJobLogService, List<DiadocLoadClientWorker> AllWorkers, IAdminService _db, ICommonService _commonService, IStorageService<StoredFile> _fileStorage)
        {
            GeneralJobSettings genSettings = new GeneralJobSettings(ServiceName, _timerJobLogService, _configuration, _dbContext, _fileStorage);
            if (AllWorkers == null || AllWorkers.Count == 0)
            {
                AllWorkers = new List<DiadocLoadClientWorker>();
                List<EDISettings> Settings = _dbContext.EDISettings.Where(x => (x.EDIProvider == "Diadoc" || x.EDIProvider == "Диадок") && (x.LoadContracts || x.LoadBuhDocs)).ToList();
                foreach (EDISettings set in Settings)
                {
                    var client = await _db.SingleAsync<Client, ClientDTO>(c => c.Id.Equals(set.ClientID));
                    var checkQuota = _commonService.CheckClientIsActive(client);
                    if (checkQuota.res)
                    {
                        DiadocSettings ediSettings = new DiadocSettings(set, _configuration["DiadocApiClientID"]);
                        EDIClientSettings clientsettings = new EDIClientSettings();
                        clientsettings.TakeCount = 100;
                        DiadocJobSettings settings = new DiadocJobSettings();
                        settings.ClientSettings = clientsettings;
                        settings.EdiSettings = ediSettings;
                        settings.GeneralSettings = genSettings;
                        DiadocLoadClientWorker wrk = new DiadocLoadClientWorker(settings);
                        AllWorkers.Add(wrk);
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
