// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using ArnonSky.Clients.Device.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArnonSky.Clients.Device.Models
{
    class GetDeviceDetailsModel
    {
        public string Type { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public DateTime? SiteLinkedTimestamp { get; set; }

        public long? SiteId { get; set; }

        public List<GetDeviceSourceDetailsModel> Sources { get; set; }

        public string Label { get; set; }

        internal DeviceConfiguration ToDeviceConfiguration()
            => new DeviceConfiguration() { CreatedTimestamp = CreatedTimestamp, Label = Label, SiteId = SiteId, SiteLinkedTimestamp = SiteLinkedTimestamp, Sources = Sources.Select(f => f.ToSourceConfiguration()).ToList(), Type = Type };
    }
}
