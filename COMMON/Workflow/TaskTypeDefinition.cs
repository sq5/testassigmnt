// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace COMMON.Workflow
{

    public class TaskTypeDefinition
    {
        public string TaskTypeName { get; set; }
        public string TaskResolution { get; set; }
        public string TaskDocState { get; set; }
        public int TaskTypeNumber { get; set; }
        public string MailSubject { get; set; }

        public TaskTypeDefinition(string taskTypeName, string taskResolution, string taskDocState, int taskTypeNumber, string subject)
        {
            TaskTypeName = taskTypeName;
            TaskResolution = taskResolution;
            TaskTypeNumber = taskTypeNumber;
            TaskDocState = taskDocState;
            MailSubject = subject;
        }
    }
}
