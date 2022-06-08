// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace DATABASE.Services
{
    public interface IUserServiceMobile
    {
        public UserEntities AuthUser(string username);

        public class UserEntities
        {
            public string Token { set; get; }
        }
    }
}
