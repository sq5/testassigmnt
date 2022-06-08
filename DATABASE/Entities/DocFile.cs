// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class DocFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public long MetaId { get; set; }
        public int ContractId { get; set; }
        public int? NonFormDocId { get; set; }
        [Required, MaxLength(255)]
        public string FileName { get; set; }
        public int? FileSize { get; set; }

        public byte[] FileBin { get; set; }
        public string BlobUrl { get; set; }

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
    }
}
