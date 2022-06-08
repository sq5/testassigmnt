using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentFTP;
using System.Security.Authentication;
using CloudArchive.Services;

namespace CloudArchive.ScheduledTasks
{
    public class FTPService : IFTPService
    {
        private readonly IBackgroundServiceLog _backgroundServiceLog;
        private readonly ExtConnectionDTO _extConnectionDTO;
        private readonly ICommonService _commonService;

        public FTPService(ExtConnectionDTO extConnectionDTO, IBackgroundServiceLog backgroundServiceLog, ICommonService commonService)
        {
            _extConnectionDTO = extConnectionDTO;
            _backgroundServiceLog = backgroundServiceLog;
            _commonService = commonService;
        }

        private static void Client_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public byte[] GetFile(FtpClient client, FTPFile ftpFile)
        {
            byte[] retByte = null;
            bool fileis = client.FileExists(ftpFile.RelativePath);
            var isFileDownloaded = client.Download(out retByte, ftpFile.RelativePath);
            return retByte;
        }

        public List<FTPFile> GetFiles(FtpClient client)
        {
            List<FTPFile> ftpFiles = new List<FTPFile>();
            try
            {

                var ftpListItems = client.GetListing(_extConnectionDTO.FolderIn);
                int limitLength = (ftpListItems.Length > 10) ? 10 : ftpListItems.Length;
                _backgroundServiceLog.AddInfo($"Found {ftpListItems.Length} in Server Name= {_extConnectionDTO.Server};Box Name= {_extConnectionDTO.User};Folder= {_extConnectionDTO.FolderIn}", "FTPBackgroundService", _extConnectionDTO.ClientId);
                for (int i = 0; i < limitLength; i++)
                {
                    if (ftpListItems[i].Type != FtpFileSystemObjectType.Directory)
                    {
                        FtpListItem item = ftpListItems[i];
                        FTPFile ftpFile = new FTPFile();
                        ftpFile.Name = item.Name;
                        ftpFile.Folder = _extConnectionDTO.FolderIn;
                        ftpFile.BaseUri = new Uri(_extConnectionDTO.Server);
                        ftpFile.DateCreated = item.Created;
                        ftpFiles.Add(ftpFile);
                    }
                }

            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("FTPBackgroundService. Ошибка получения файлов " + "Error:" + ex.Message + ". StackTrace: " + ex.StackTrace + ". Conn ID " + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
            }
            return ftpFiles;
        }


        public void MoveFile(FtpClient client, string source, string dest, string name)
        {
            try
            {
                int limit = 100;
                if (source == dest)
                    return;
                string uriSource = string.Format("/{0}/{1}", source, name);
                string newName = string.Format("{0}-{1}", DateTime.Now.ToString("yyyyMMdd HH-mm-ss"), name);
                if (newName.Length >= limit)
                {
                    var newNameExt = Path.GetExtension(newName);
                    newName = Path.GetFileNameWithoutExtension(newName).Substring(0, limit - newNameExt.Length) + newNameExt;
                }
                string uriDestination = string.Format("/{0}/{1}", dest, newName);
                if (client.FileExists(uriDestination.ToString()))
                {
                    throw (new ApplicationException(string.Format("Target '{0}' already exists!", uriDestination)));
                }
                client.Rename(uriSource, uriDestination);
            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("FTPBackgroundService. Ошибка перемещения файла " + source + name + " Error:" + ex.Message + ". StackTrace: " + ex.StackTrace + ". Conn ID " + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);

            }
        }

        public void Process()
        {
            try
            {
                // Get the object used to communicate with the server.
                FtpClient client = new FtpClient(_extConnectionDTO.Server);
                // This example assumes the FTP site uses anonymous logon.
                client.Credentials = new NetworkCredential(_extConnectionDTO.User, _extConnectionDTO.Password);
                if (_extConnectionDTO.TLS == true)
                {
                    client.EncryptionMode = FtpEncryptionMode.Explicit;
                    client.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    client.ValidateCertificate += Client_ValidateCertificate;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                }
                client.Connect();
                List<FTPFile> ftpFiles = this.GetFiles(client);
                foreach (FTPFile ftpFile in ftpFiles)
                {
                    try
                    {
                        if (!client.FileExists(ftpFile.RelativePath))
                        {
                            _backgroundServiceLog.AddError("FTPBackgroundService. Файл " + ftpFile.Name + " не найден. Conn ID: " + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
                            continue;
                        }
                        if (ftpFile.Extension.ToLowerInvariant() != ".pdf")
                        {
                            _backgroundServiceLog.AddError("FTPBackgroundService. Файл " + ftpFile.Name + " не PDF. Conn ID: " + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
                            this.MoveFile(client, _extConnectionDTO.FolderIn, _extConnectionDTO.FolderError, ftpFile.Name);
                            continue;
                        }

                        List<NonFormDocsDTO> nonFormDocsArr = new List<NonFormDocsDTO>();
                        byte[] PDF = this.GetFile(client, ftpFile);
                        var fileSizeInMb = (float)Math.Round((PDF.Length / 1024.0F) / 1024.0F, 2);
                        if (fileSizeInMb > 20)
                        {
                            _backgroundServiceLog.AddError("FTPBackgroundService. Файл " + ftpFile.Name + " превышает 20Мб. Conn ID: " + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
                            this.MoveFile(client, _extConnectionDTO.FolderIn, _extConnectionDTO.FolderError, ftpFile.Name);
                            continue;
                        }
                        NonFormDocsDTO nonFormDocsDTO = new NonFormDocsDTO();
                        nonFormDocsDTO.ClientId = _extConnectionDTO.ClientId;
                        nonFormDocsDTO.Created = DateTime.Now;
                        nonFormDocsDTO.Modified = DateTime.Now;
                        nonFormDocsDTO.OrganizationId = _extConnectionDTO.OrganizationId;
                        nonFormDocsDTO.OCRSplit = _extConnectionDTO.OCRSplit;
                        nonFormDocsDTO.Binaries = new List<BinariesDTO>();
                        nonFormDocsDTO.Binaries.Add(new BinariesDTO()
                        {
                            FileName = ftpFile.Name,
                            FileBase64 = Convert.ToBase64String(PDF),
                            FileSize = PDF.Length

                        });
                        if (_extConnectionDTO.OCR.HasValue && _extConnectionDTO.OCR.Value)
                        {
                            nonFormDocsDTO.OCRState = "Отправка на распознавание";
                        }
                        nonFormDocsDTO.RequestID = Guid.NewGuid().ToString("d");
                        nonFormDocsArr.Add(nonFormDocsDTO);
                        var success = _commonService.CreateNonFormDocs(nonFormDocsArr);
                        if (success.Result)
                        {
                            this.MoveFile(client, _extConnectionDTO.FolderIn, _extConnectionDTO.FolderProcessed, ftpFile.Name);
                        }
                        else
                        {
                            this.MoveFile(client, _extConnectionDTO.FolderIn, _extConnectionDTO.FolderError, ftpFile.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _backgroundServiceLog.AddError("Общая ошибка обработки " + ex.Message + ". StackTrace: " + ex.StackTrace + ". Conn ID" + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
                    }
                }

                client.Dispose();
            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("Ошибка подключения к FTP " + ex.Message + ". StackTrace: " + ex.StackTrace + ". Conn ID" + _extConnectionDTO.Id, "FTPBackgroundService", _extConnectionDTO.ClientId);
            }

        }
    }
}
