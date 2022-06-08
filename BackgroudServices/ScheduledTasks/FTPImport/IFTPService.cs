using System.Collections.Generic;
using FluentFTP;

namespace CloudArchive.ScheduledTasks
{
    interface IFTPService
    {
        void Process();
        byte[] GetFile(FtpClient client, FTPFile ftpFile);
        List<FTPFile> GetFiles(FtpClient client);
        void MoveFile(FtpClient client, string source, string dest, string name);
    }
}
