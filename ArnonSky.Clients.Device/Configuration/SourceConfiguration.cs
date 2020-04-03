using System;
using System.Collections.Generic;

namespace ArnonSky.Clients.Device.Configuration
{
    public class SourceConfiguration
    {
        public long? ParentId { get; set; }

        public long SourceId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Dictionary<string, object>[] Items { get; set; }

        public DateTime ItemsEditedTimestamp { get; set; }
    }
}
