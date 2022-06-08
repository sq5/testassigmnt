using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels
{
    public class UsersTasksDTO
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public string Users { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DeadLine { get; set; }
        public DateTime Created { get; set; }
        public long? MetadataId { get; set; }
        public int? ContractId { get; set; }
        public string Comment { get; set; }
        public string Resolution { get; set; }
        public string TaskType { get; set; }
        public int Stage { get; set; }
        public int Order { get; set; }
        public int? TaskdeadLine { get; set; }
        public string TaskText { get; set; }
        public string ApprovementType { get; set; }
        public string Position { get; set; }
        public List<byte[]> Pictures { get; set; }
        public string SubstituteFor { get; set; }
        
        public ApprovementTypeEnum AppType
        {
            get
            {
                switch (ApprovementType)
                {
                    case "Параллельный":
                        return ApprovementTypeEnum.Parallel;
                    default:
                        return ApprovementTypeEnum.Consecutive;
                }
            }
            set
            {
                switch (value)
                {
                    case ApprovementTypeEnum.Parallel:
                        ApprovementType = "Параллельный";
                        break;
                    default:
                        ApprovementType = "Последовательный";
                        break;
                }
            }
        }
    }

    public enum ApprovementTypeEnum
    {
        Consecutive = 0,
        Parallel = 1
    }
}
