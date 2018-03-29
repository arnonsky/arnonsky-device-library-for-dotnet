// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

namespace ArnonSky.Clients.Device.Models
{
    public class PostDeviceModel
    {
        public PostDeviceModel(string provisioningKey, string productKey, string deviceType)
        {
            ProvisioningKey = provisioningKey;
            ProductKey = productKey;
            Type = deviceType;
        }

        public string ProvisioningKey { get; set; }

        public string ProductKey { get; }

        public string Type { get; }

    }
}
