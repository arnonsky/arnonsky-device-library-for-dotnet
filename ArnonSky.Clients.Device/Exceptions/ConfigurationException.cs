// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device.Exceptions
{
    public abstract class ConfigurationException : Exception
    {
        public ConfigurationException(string message)
            : base(message)
        {

        }
    }
}
