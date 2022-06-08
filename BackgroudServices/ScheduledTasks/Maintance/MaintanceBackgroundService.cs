using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using DATABASE.Context;
using CloudArchive.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;

namespace CloudArchive.ScheduledTasks
{
    public class MaintanceBackgroundService : IScheduledTask
    {
        public string ServiceName { get => "MaintanceBackgroundService"; }
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MaintanceBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            try
            {
                float sMeta = 0;
                float sCtr = 0;
                float sNonForm = 0;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                    SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    var clients = _dbContext.Clients.AsNoTracking().Select(x => x.Id).ToList();
                    foreach (int client in clients)
                    {
                        try
                        {
                            var meta = _dbContext.Metadatas.AsNoTracking().Where(m => m.ClientId.Equals(client)).Select(x => new MetadataDTO { Id = x.Id });
                            var ctr = _dbContext.Contracts.AsNoTracking().Where(m => m.ClientId.Equals(client)).Select(x => new ContractDTO { Id = x.Id });
                            var nonform = _dbContext.NonFormDocs.AsNoTracking().Where(m => m.ClientId.Equals(client)).Select(x => new NonFormDocsDTO { Id = x.Id });
                            var files = _dbContext.Files.AsNoTracking().Select(x => new DocFile { MetaId = x.MetaId, NonFormDocId = x.NonFormDocId, ContractId = x.ContractId, FileSize = x.FileSize });
                            var sumMeta = (from d in meta
                                           join f in files
                                          on d.Id equals f.MetaId
                                          into FileTable
                                           from x in FileTable.DefaultIfEmpty()
                                           select x.FileSize).Where(t => t.HasValue).Select(t => (long)t.Value).Sum();
                            var sumCtr = (from d in ctr
                                          join f in files
                                         on d.Id equals f.ContractId
                                         into FileTable
                                          from x in FileTable.DefaultIfEmpty()
                                          select x.FileSize).Where(t => t.HasValue).Select(t => (long)t.Value).Sum();
                            var sumNonForm = (from d in nonform
                                              join f in files
                                             on d.Id equals f.NonFormDocId
                                             into FileTable
                                              from x in FileTable.DefaultIfEmpty()
                                              select x.FileSize).Where(t => t.HasValue).Select(t => (long)t.Value).Sum();
                            sMeta = sumMeta > 0 ? (float)Math.Round(sumMeta / 1024.0F / 1024.0F, 2) : 0;
                            sCtr = sumCtr > 0 ? (float)Math.Round(sumCtr / 1024.0F / 1024.0F, 2) : 0;
                            sNonForm = sumNonForm > 0 ? (float)Math.Round(sumNonForm / 1024.0F / 1024.0F, 2) : 0;
                            var cl = _dbContext.Clients.FirstOrDefault(c => c.Id.Equals(client));
                            cl.StorageUsed = (sMeta + sCtr + sNonForm);
                            _dbContext.Update(cl);
                            _dbContext.SaveChanges();
                            //if (cl.StorageUsed.HasValue && cl.StorageUsed > 0)
                            //  _backgroundServiceLog.AddInfo("MaintanceService. Counted storage" + cl.StorageUsed.ToString(), "MaintanceService", client);
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("Error in MaintanceService" + ex.Message + "StackTrace: " + ex.StackTrace, "MaintanceService", client);
                        }
                    }
                }
                await Task.Delay(JobScheduler.GetWaitDelay("MaintanceBackgroundService"), stoppingToken);
            }
            catch (Exception)
            {
            }
        }
    }
}
