// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace COMMON.Workflow
{
    public class TaskParams
    {
        public int Id { get; set; }
        public bool Agreed { get; set; }
        public string Comment { get; set; }
        public string Sett { get; set; }
        public int TaskType { get; set; }
        public string DelegatedTo { get; set; }
    }

    public class UserTask
    {
        public int Id { get; set; }
        public int TaskType { get; set; }
    }
}
