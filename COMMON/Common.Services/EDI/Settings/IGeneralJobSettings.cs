// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.Extensions.Configuration;

namespace CloudArchive.Services.EDI
{
    public interface IGeneralJobSettings
    {
        string ServiceName { get; }
        IBackgroundServiceLog LogService { get; set; }
        IConfiguration Configuration { get; set; }
        SearchServiceDBContext DbContext { get; set; }
        IStorageService<StoredFile> FileStorage { get; set; }
    }
}
