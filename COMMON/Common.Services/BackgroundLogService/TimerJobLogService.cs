using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services
{
    public class TimerJobLogService : IBackgroundServiceLog
    {
        private readonly IAdminService _adminService;
        public TimerJobLogService(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public void AddError(string message, string service, int clientId = 0)
        {
            BackgroundServiceLogDTO backgroundServiceLogDTO = new BackgroundServiceLogDTO();
            backgroundServiceLogDTO.Type = "Error";
            backgroundServiceLogDTO.Message = message;
            backgroundServiceLogDTO.Time = DateTime.Now;
            backgroundServiceLogDTO.Service = service;
            backgroundServiceLogDTO.Client = clientId;
            var x = _adminService.CreateAsyncInt32<BackgroundServiceLogDTO, BackgroundServiceLog>(backgroundServiceLogDTO).Result;
        }
       
        public void AddInfoProcFol( int clientId, string message, string service)
        {
            BackgroundServiceLogDTO backgroundServiceLogDTO = new BackgroundServiceLogDTO();
            backgroundServiceLogDTO.Type = "Info";
            backgroundServiceLogDTO.Client = clientId;
            backgroundServiceLogDTO.Message = message ;
            backgroundServiceLogDTO.Time = DateTime.Now;
            backgroundServiceLogDTO.Service = service;
            var x = _adminService.CreateAsyncInt32<BackgroundServiceLogDTO, BackgroundServiceLog>(backgroundServiceLogDTO).Result;
        }

        public void AddInfo(string message, string service, int client = 0)
        {
            BackgroundServiceLogDTO backgroundServiceLogDTO = new BackgroundServiceLogDTO();
            backgroundServiceLogDTO.Type = "Info";
            backgroundServiceLogDTO.Message = message;
            backgroundServiceLogDTO.Time = DateTime.Now;
            backgroundServiceLogDTO.Service = service;
            backgroundServiceLogDTO.Client = client;
            var x = _adminService.CreateAsyncInt32<BackgroundServiceLogDTO, BackgroundServiceLog>(backgroundServiceLogDTO).Result;
        }

    }
}
