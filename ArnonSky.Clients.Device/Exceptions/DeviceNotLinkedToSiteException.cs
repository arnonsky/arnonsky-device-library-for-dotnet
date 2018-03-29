// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device.Exceptions
{
    public class DeviceNotLinkedToSiteException : ConfigurationException
    {
        public DeviceNotLinkedToSiteException(string productKey)
            : base($"The device is not linked to a site. Please register the device {productKey} to your site.")
        {
        }
    }
}
