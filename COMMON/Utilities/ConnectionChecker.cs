using System;
using System.Collections.Generic;
using System.Linq;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using System.IO;
using System.Net;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using ARCHIVE.COMMON.DTOModels;
using System.Security;
using System.Text;

namespace ARCHIVE.COMMON.Utilities
{
    public class ConnectionChecker
    {
        public string ErrorText = "";
        public bool CheckConnection(ExtConnectionDTO model)
        {
            if (model.Type == "E-mail")
            {
                return CheckConnectionSMTP(model);
            }
            else if (model.Type == "FTP")
            {
                return CheckConnectionFTP(model);
            }
            else
            {
                ErrorText = "Некорректный тип подключения!";
                return false;
            }
        }


        public bool CheckConnectionFTP(ExtConnectionDTO model)
        {
            bool result = true;
            try
            {

                this.ErrorText += "Подключение к удаленному серверу...";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(model.Server);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.EnableSsl = model.TLS;
                if (model.TLS)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                request.Credentials = new NetworkCredential(model.User, model.Password);
                string[] list = null;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                reader.Close();
                response.Close();
                this.ErrorText += "<br/><font color='#38761d'>Соединение успешно установлено!</font>";

                bool FoundInFolder = false;
                bool FoundOutFolder = false;
                bool FoundErrorFolder = false;
                foreach (string line in list)
                {
                    var arr = line.Split(' ');
                    string name = arr[arr.Length - 1];
                    if (name == model.FolderIn)
                    {
                        FoundInFolder = true;
                        this.ErrorText += "<br/>Поиск папки входящих.....<font color='#38761d'> Папка найдена</font>";
                    }
                    if (name == model.FolderProcessed)
                    {
                        FoundOutFolder = true;
                        this.ErrorText += "<br/>Поиск папки обработанных.....<font color='#38761d'> Папка найдена</font>";
                    }
                    if (name == model.FolderError)
                    {
                        FoundErrorFolder = true;
                        this.ErrorText += "<br/>Поиск папки ошибок.....<font color='#38761d'> Папка найдена</font>";
                    }
                }
                if (!FoundInFolder)
                {
                    result = false;
                    this.ErrorText += "<br/>Поиск папки входящих.....<font color='red'> Папка не найдена</font>";
                }
                if (!FoundOutFolder)
                {
                    result = false;
                    this.ErrorText += "<br/>Поиск папки обработанных.....<font color='red'> Папка не найдена</font>";
                }
                if (!FoundErrorFolder)
                {
                    result = false;
                    this.ErrorText += "<br/>Поиск папки ошибок.....<font color='red'> Папка не найдена</font>";
                }
                if (model.FolderIn == model.FolderProcessed || model.FolderError == model.FolderIn)
                {
                    result = false;
                    this.ErrorText += "<br/>Ошибка..... <font color='red'>Папка Входящих не может совпадать с папкой Обработанных/Ошибочных</font>";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "The remote name could not be resolved")
                {
                    this.ErrorText += "<br/><font color='red'>Не удалось подключиться к серверу</font>";
                }
                else if (ex.Message == "Invalid URI: The format of the URI could not be determined.")
                {
                    this.ErrorText += "<br/><font color='red'>Некорректный формат URL адреса</font>";
                }
                else if (ex.Message == "The remote server returned an error: (530) Not logged in.")
                {
                    this.ErrorText += "<br/><font color='red'>Неправильный логин или пароль</font>";
                }
                else
                {
                    this.ErrorText += "<br/><font color='red'>" + ex.Message + "</font>";
                }
                result = false;
            }
            return result;
        }

        public bool CheckConnectionSMTP(ExtConnectionDTO model)
        {
            bool result = true;
            try
            {
                using (var client = new ImapClient())
                {
                    this.ErrorText += "Подключение к удаленному серверу...";
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(model.Server, model.Port, SecureSocketOptions.SslOnConnect);
                    this.ErrorText += "<br/><font color='#38761d'>Сервер найден</font>, проводится аутентификация";
                    client.Authenticate(model.User, model.Password);
                    this.ErrorText += "<br/><font color='#38761d'>Соединение успешно установлено!</font>";

                    try
                    {
                        var inbox = client.GetFolder(model.FolderIn);
                        this.ErrorText += "<br/>Поиск папки входящих..... <font color='#38761d'>Папка найдена</font>";
                    }
                    catch
                    {
                        result = false;
                        this.ErrorText += "<br/>Поиск папки входящих..... <font color='red'>Папка не найдена</font>";
                    }

                    try
                    {
                        var folderProcessing = client.GetFolder(model.FolderProcessed);
                        this.ErrorText += "<br/>Поиск папки обработанных..... <font color='#38761d'>Папка найдена</font>";
                    }
                    catch
                    {
                        result = false;
                        this.ErrorText += "<br/>Поиск папки обработанных..... <font color='red'>Папка не найдена</font>";
                    }
                    try
                    {
                        var FolderError = client.GetFolder(model.FolderError);
                        this.ErrorText += "<br/>Поиск папки ошибок..... <font color='#38761d'>Папка найдена</font>";
                    }
                    catch
                    {
                        result = false;
                        this.ErrorText += "<br/>Поиск папки ошибок..... <font color='red'>Папка не найдена</font>";
                    }

                    if (model.FolderIn == model.FolderProcessed || model.FolderError == model.FolderIn)
                    {
                        result = false;
                        this.ErrorText += "<br/>Ошибка..... <font color='red'>Папка Входящих не может совпадать с папкой Обработанных/Ошибочных</font>";
                    }


                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "nodename nor servname provided, or not known")
                {
                    this.ErrorText += "<br/><font color='red'>Не удалось подключиться к серверу</font>";
                }
                else if (ex.Message == "The operation has timed out.")
                {
                    this.ErrorText += "<br/><font color='red'>Превышено время ожидания подключения</font>";
                }
                else if (ex.Message == "Invalid credentials (Failure)")
                {
                    this.ErrorText += "<br/><font color='red'>Неправильный логин или пароль</font>";
                }
                else
                {
                    this.ErrorText += "<br/><font color='red'>" + ex.Message + "</font>";
                }
                result = false;
            }
            return result;
        }
        public bool PreCheckConnection(ExtConnectionDTO model)
        {
            if (model.Type == "E-mail")
            {
                return PreCheckConnectionSMTP(model);
            }
            else if (model.Type == "FTP")
            {
                return PreCheckConnectionFTP(model);
            }
            else
            {
                ErrorText = "Некорректный тип подключения!";
                return false;
            }
        }
        public bool PreCheckConnectionFTP(ExtConnectionDTO model)
        {
            bool result = true;

            try
            {

                this.ErrorText += "Подключение к удаленному серверу...";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(model.Server);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.EnableSsl = model.TLS;
                if (model.TLS)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                var secureString = new SecureString();
                foreach (var b in Encoding.Default.GetBytes(model.Password))
                    secureString.AppendChar((char)b);
                request.Credentials = new NetworkCredential(model.User, secureString);
                string[] list = null;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                reader.Close();
                response.Close();
                this.ErrorText += "<br/><font color='#38761d'>Соединение успешно установлено!</font>";


            }
            catch (Exception ex)
            {
                if (ex.Message == "The remote name could not be resolved")
                {
                    this.ErrorText += "<br/><font color='red'>Не удалось подключиться к серверу</font>";
                }
                else if (ex.Message == "Invalid URI: The format of the URI could not be determined.")
                {
                    this.ErrorText += "<br/><font color='red'>Некорректный формат URL адреса</font>";
                }
                else if (ex.Message == "The remote server returned an error: (530) Not logged in.")
                {
                    this.ErrorText += "<br/><font color='red'>Неправильный логин или пароль. Или неверные настройки TLS </font>";
                }
                else
                {
                    this.ErrorText += "<br/><font color='red'>" + ex.Message + "</font>";
                }
                result = false;
            }
            return result;
        }

        public bool PreCheckConnectionSMTP(ExtConnectionDTO model)
        {
            bool result = true;
            try
            {
                using (var client = new ImapClient())
                {
                    this.ErrorText += "Подключение к удаленному серверу...";
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(model.Server, model.Port, SecureSocketOptions.SslOnConnect);
                    this.ErrorText += "<br/><font color='#38761d'>Сервер найден</font>, проводится аутентификация";
                    client.Authenticate(model.User, model.Password);
                    this.ErrorText += "<br/><font color='#38761d'>Соединение успешно установлено!</font>";

                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "nodename nor servname provided, or not known")
                {
                    this.ErrorText += "<br/><font color='red'>Не удалось подключиться к серверу</font>";
                }
                else if (ex.Message == "The operation has timed out.")
                {
                    this.ErrorText += "<br/><font color='red'>Превышено время ожидания подключения</font>";
                }
                else if (ex.Message == "Invalid credentials (Failure)")
                {
                    this.ErrorText += "<br/><font color='red'>Неправильный логин или пароль</font>";
                }
                else
                {
                    this.ErrorText += "<br/><font color='red'>" + ex.Message + "</font>";
                }
                result = false;
            }
            return result;
        }
    }
}
