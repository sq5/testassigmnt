// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.Entities;

namespace COMMON.Models
{
    public abstract class StoredFile
    {
        public DocFile DocFile { get; private set; }
        public StoredFile(DocFile docFile)
        {
            DocFile = docFile;
        }

        public virtual byte[] FileBin { get; set; }
    }
}
