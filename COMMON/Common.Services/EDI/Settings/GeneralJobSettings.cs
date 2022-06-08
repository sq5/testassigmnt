// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.Extensions.Configuration;

namespace CloudArchive.Services.EDI.Settings
{
    public class GeneralJobSettings : IGeneralJobSettings
    {
        public string ServiceName { get; }
        public IBackgroundServiceLog LogService { get; set; }
        public IConfiguration Configuration { get; set; }
        public IStorageService<StoredFile> FileStorage { get; set; }
        public SearchServiceDBContext DbContext { get; set; }

        public GeneralJobSettings(string serviceName, IBackgroundServiceLog logService, IConfiguration configuration, SearchServiceDBContext dbContext, IStorageService<StoredFile> _fileStorage)
        {
            ServiceName = serviceName;
            LogService = logService;
            Configuration = configuration;
            DbContext = dbContext;
            FileStorage = _fileStorage;
        }
    }
}
