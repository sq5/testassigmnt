using CloudArchive.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YandexDisk.Client;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace CloudArchive.ScheduledTasks
{
    public interface ICleanBackupStorageService
    {
        Task CleanAsync();
    }

    public interface IExternalStorageService
    {
        Task<bool> UploadAsync(Stream stream, string path);
        Task<IEnumerable<string>> GetFoldersAsync(string path);
        Task<Tuple<bool, string>> DeleteAsync(string path);
        Task<string> PublishAsync(string path);
        Task<bool> UnpublishAsync(string path);
    }

    public class YandexExternalStorageService : IExternalStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IDiskApi _diskApi;
        public YandexExternalStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            string tokenYandexDisk = _configuration["YandexDiskToken"];
            if (string.IsNullOrEmpty(tokenYandexDisk))
            {
                throw new ArgumentNullException();
            }
            _diskApi = new DiskHttpApi(tokenYandexDisk);

        }
        public async Task<Tuple<bool, string>> DeleteAsync(string path)
        {
            try
            {
                var deleteFolders = await _diskApi.Commands.DeleteAsync(new DeleteFileRequest() { Path = path });
            }
            catch (YandexApiException ex)
            {
                return Tuple.Create<bool, string>(false, ex.ToString());
            }
            catch (Exception ex)
            {
                return Tuple.Create<bool, string>(false, ex.Message + "StackTrace: " + ex.StackTrace);
            }
            return Tuple.Create<bool, string>(true, string.Empty);
        }

        public async Task<IEnumerable<string>> GetFoldersAsync(string path)
        {
            IEnumerable<string> listOfFolders = new List<string>();
            try
            {
                var folders = await _diskApi.MetaInfo.GetInfoAsync(new ResourceRequest() { Path = path });
                if (folders != null && folders.Embedded != null && folders.Embedded.Items != null)
                {
                    listOfFolders = folders.Embedded.Items.Select(t => t.Path);
                }
            }
            catch (YandexApiException)
            {
            }
            catch (Exception)
            {
            }
            return listOfFolders;
        }

        public Task<string> PublishAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnpublishAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UploadAsync(Stream stream, string path)
        {
            throw new NotImplementedException();
        }
    }

    public class YandexCleanBackupStorageService : ICleanBackupStorageService
    {
        private readonly IExternalStorageService _externalStorageService;
        public YandexCleanBackupStorageService(IExternalStorageService externalStorageService)
        {
            _externalStorageService = externalStorageService;
        }
        public async Task CleanAsync()
        {
            try
            {
                var folders = await _externalStorageService.GetFoldersAsync("/");
                foreach (var path in folders)
                {
                    string datepart = path.Substring(6, 10);
                    DateTime? flddate = Helper.ParseDateTime(datepart);
                    if (flddate != null && flddate.Value.AddDays(10) <= DateTime.Now)
                    {
                        var isDeleted = await _externalStorageService.DeleteAsync(path);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
