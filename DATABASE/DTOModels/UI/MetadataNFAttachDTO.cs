// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATABASE.DTOModels.UI
{
    public class MetadataNFAttachDTO
    {
        public Int64 Id { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public string Comment { get; set; }
        public DocType DocType { get; set; }
        public Organization Organization { get; set; }
        public Contractor Contractor { get; set; }
    }
}
