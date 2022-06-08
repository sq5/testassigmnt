using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(List<string> email, string subject, string message, List<DocFile> files = null);
    }
}
