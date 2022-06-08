namespace ARCHIVE.COMMON.DTOModels
{
    public class ClientsTemplatesWithFileDTO
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public int DocTypeId { get; set; }
        public byte[] FileBin { get; set; }
        public string Comment { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
    }
}
