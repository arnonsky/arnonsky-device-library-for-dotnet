using System;
using System.Collections.Generic;

namespace ArnonSky.Clients.Device.Configuration
{
    public class DeviceConfiguration
    {
        public string Type { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public DateTime? SiteLinkedTimestamp { get; set; }

        public long? SiteId { get; set; }

        public List<SourceConfiguration> Sources { get; set; }

        public string Label { get; set; }
    }
}
