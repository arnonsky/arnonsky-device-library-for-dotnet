// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device.Exceptions
{
    public class SourceNameNotFoundException : ConfigurationException
    {
        public SourceNameNotFoundException(string sourceName)
            : base($"The device is not connected to a source with name {sourceName}. Please make sure that the source exists and is linked to the device.")
        {
        }
    }
}
