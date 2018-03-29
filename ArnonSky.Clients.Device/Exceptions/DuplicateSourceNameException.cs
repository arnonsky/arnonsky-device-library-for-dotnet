// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device.Exceptions
{
    class DuplicateSourceNameException : Exception
    {
        public DuplicateSourceNameException(string sourceName)
            : base($"The device is connected to multiple sources with the name {sourceName}.")
        {
        }
    }
}
