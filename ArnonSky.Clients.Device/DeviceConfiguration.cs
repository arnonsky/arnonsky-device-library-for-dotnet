using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnonSky.Clients.Device.Models;

namespace ArnonSky.Clients.Device
{
    class DeviceConfiguration
    {
        public DateTime ReadTimestamp { get; set; }

        public GetDeviceDetailsModel Current { get; set; }
    }
}
