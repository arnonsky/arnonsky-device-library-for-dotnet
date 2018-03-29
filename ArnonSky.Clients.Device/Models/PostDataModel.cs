// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System.Collections.Generic;

namespace ArnonSky.Clients.Device.Models
{
    public class PostDataModel
    {
        public PostDataModel(string timestamp, IEnumerable<object> list)
        {
            Timestamp = timestamp;
            Values = list;
        }

        public string Timestamp { get; set; }

        public IEnumerable<object> Values { get; set; }

    }
}
