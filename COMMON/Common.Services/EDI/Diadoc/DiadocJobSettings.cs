// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CloudArchive.Services.EDI.Settings;

namespace CloudArchive.Services.EDI.EnsolDiadoc
{
    public class DiadocJobSettings : IEDIJobSettings
    {
        public GeneralJobSettings generalSettings { get; set; }
        public DiadocSettings ediSettings { get; set; }
        public IEDIClientSettings ClientSettings { get; set; }
        public IGeneralJobSettings GeneralSettings { get { return generalSettings; } set { generalSettings = value as GeneralJobSettings; } }
        public IEDISettings EdiSettings { get { return ediSettings; } set { ediSettings = value as DiadocSettings; } }
    }
}
