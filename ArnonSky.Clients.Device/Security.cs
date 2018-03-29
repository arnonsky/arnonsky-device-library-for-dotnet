// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device
{
    public static class Security
    {
        /// <summary>
        /// Function that may be used to protect data under the current user profile
        /// </summary>
        /// <param name="data">Unprotected data</param>
        /// <returns>Protected data</returns>
        public static byte[] ProtectData(byte[] data)
            =>  ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        

        /// <summary>
        /// Function that may be used to unprotect data under the current user profile
        /// </summary>
        /// <param name="data">Protected data</param>
        /// <returns>Unprotected data</returns>
        public static byte[] UnprotectData(byte[] data)
            => ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);

    }
}
