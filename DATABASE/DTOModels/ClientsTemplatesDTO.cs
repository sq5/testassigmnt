namespace ARCHIVE.COMMON.DTOModels
{
    public class ClientsTemplatesDTO
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string FileName { get; set; }
        public bool FileExists { get; set; }
        public int DocTypeID { get; set; }
        public string Comment { get; set; }
        public int ClientId { get; set; }
    }
}
