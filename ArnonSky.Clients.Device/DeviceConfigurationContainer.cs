using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnonSky.Clients.Device.Configuration;

namespace ArnonSky.Clients.Device
{
    class DeviceConfigurationContainer
    {
        public DateTime ReadTimestamp { get; set; }

        public DeviceConfiguration Current { get; set; }
    }
}
