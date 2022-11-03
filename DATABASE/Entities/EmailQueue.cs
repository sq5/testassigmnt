// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class EmailQueue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Sent { get; set; }
        [Required]
        public string Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<EmailQueueDocFile> EmailQueueDocFiles { get; set; }
    }
}
