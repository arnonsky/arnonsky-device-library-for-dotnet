// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device.Models
{
    public class TagValue
    {
        public TagValue(string tagName, object value)
        {
            TagName = tagName;
            Value = value;
        }

        public string TagName { get; set; }

        public object Value { get; set; }

    }
}
