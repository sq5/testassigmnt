// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class EmailQueueDocFile
    {
        // в новых версиях EFCore (5+) промежуточная модель не нужна
        public long EmailQueueId { get; set; }
        public EmailQueue EmailQueue { get; set; }
        public long DocFileId { get; set; }
        public DocFile DocFile { get; set; }
    }
}
