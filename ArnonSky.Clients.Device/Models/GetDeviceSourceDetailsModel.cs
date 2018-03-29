// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;

namespace ArnonSky.Clients.Device.Models
{
    public class GetDeviceSourceDetailsModel
    {
        public long? ParentId { get; set; }

        public long SourceId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Dictionary<string, object>[] Items { get; set; }

        public DateTime ItemsEditedTimestamp { get; set; }

    }
}
