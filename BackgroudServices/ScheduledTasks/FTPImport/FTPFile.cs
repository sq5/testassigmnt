using System;
using System.IO;

namespace CloudArchive.ScheduledTasks
{
    public class FTPFile
    {
        public Uri BaseUri;
        public DateTime DateCreated;
        public string Name;
        public string Folder;
        public string NameWithoutExtension { get { return Path.GetFileNameWithoutExtension(Name); } }
        public string Extension { get { return Path.GetExtension(Name); } }
        public string AbsolutePath { get { return string.Format("{0}/{1}/{2}", BaseUri, Folder, Name); } }
        public string RelativePath { get { return string.Format("/{0}/{1}", Folder, Name); } }
        public bool IsDirectory;
    }
}