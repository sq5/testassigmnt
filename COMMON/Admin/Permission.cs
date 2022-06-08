// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace COMMON.Admin
{
    public class Permission
    {
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool FileWrite { get; set; }
        public string Error { get; set; }
    }
}
