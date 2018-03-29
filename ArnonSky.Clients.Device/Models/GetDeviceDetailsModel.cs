// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;

namespace ArnonSky.Clients.Device.Models
{
    public class GetDeviceDetailsModel
    {
        public string Type { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public DateTime? SiteLinkedTimestamp { get; set; }

        public long? SiteId { get; set; }

        public List<GetDeviceSourceDetailsModel> Sources { get; set; }

        public string Label { get; set; }

    }
}
