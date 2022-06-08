using System.IO;
using System.Text;
using System.IO.Compression;
using System.Collections.Generic;

namespace ARCHIVE.COMMON.Utilities
{
    public static class Zipper
    {
        public static byte[] Zip(List<ZipItem> zipItems)
        {
            byte[] bytes = null;
            using (var zipStream = new MemoryStream())
            {

                using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var zipItem in zipItems)
                    {
                        var entry = zip.CreateEntry(zipItem.Name);
                        using (var entryStream = entry.Open())
                        {
                            using (var memoryStream = new MemoryStream(zipItem.Content))
                            {
                                memoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
                zipStream.Position = 0;
                bytes = ReadFully(zipStream);
            }
            return bytes;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }

    public class ZipItem
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public ZipItem(string name, byte[] content)
        {
            this.Name = name;
            this.Content = content;
        }
        public ZipItem(string name, string contentStr, Encoding encoding)
        {
            var byteArray = encoding.GetBytes(contentStr);
            //var memoryStream = new MemoryStream(byteArray);
            this.Name = name;
            this.Content = byteArray;
        }
    }
}
