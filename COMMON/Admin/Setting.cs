using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.Admin
{
    public class Setting
    {
        public string DataField { get; set; }
        public string FieldEn { get; set; }
        public string FieldRu { get; set; }
        public string DataType { get; set; }
        public string Width { get; set; }
        public string MinWidth { get; set; }
        public string Values { get; set; }
        public int Sequence { get; set; }
        public bool Visible { get; set; }
        public bool Required { get; set; }
        public string DefaultVal { get; set; }
        public bool AllowFiltering { get; set; }
        public string FieldsGroup { get; set; }
        public string ColGroup { get; set; }
    }
}
