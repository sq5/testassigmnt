// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.Entities;
using System;

namespace ARCHIVE.COMMON.DTOModels
{
    public class BinariesDTO
    {
        public Int64 Id { get; set; }
        public string FileBase64 { get; set; }
        public string FileName { get; set; }
        public string BlobUrl { get; set; }
        public int? FileSize { get; set; }
        public long MetaId { get; set; }
        public int ContractId { get; set; }
        public int NonFormDocId { get; set; }
        public string Sender { get; set; }
        public Organization Organization { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string HtmlContent { get; set; }
        public int HtmlWidth { get; set; }
        public bool? HasSignature { get; set; }
    }
}
