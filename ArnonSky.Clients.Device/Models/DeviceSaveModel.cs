// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

namespace ArnonSky.Clients.Device.Models
{
    class DeviceSaveModel
    {
        public string ProductKey { get; set; }

        public byte[] ProtectedAccessKey { get; set; }

        public string ApiBaseAddress { get; set; }
    }
}
