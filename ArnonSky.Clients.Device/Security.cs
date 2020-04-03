// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArnonSky.Clients.Device
{
    static class Security
    {
        /// <summary>
        /// Function that may be used to protect data under the current user profile
        /// </summary>
        /// <param name="data">Unprotected data</param>
        /// <returns>Protected data</returns>
        public static byte[] ProtectData(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (PlatformNotSupportedException)
            {
                // on linux use aes
                ReadOrGenerateAesKeys(out var key, out var iv);
                return AesEncrypt(data, key, iv);
            }
        }


        /// <summary>
        /// Function that may be used to unprotect data under the current user profile
        /// </summary>
        /// <param name="data">Protected data</param>
        /// <returns>Unprotected data</returns>
        public static byte[] UnprotectData(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (PlatformNotSupportedException)
            {
                ReadOrGenerateAesKeys(out var key, out var iv);
                // on linux use aes
                return AesDecrypt(data, key, iv);
            }
        }

        private static void ReadOrGenerateAesKeys(out byte[] key, out byte[] iv)
        {
            const string file = ".dataprotector";
            try
            {
                var json = File.ReadAllText(file, Encoding.UTF8);
                var keys = JsonConvert.DeserializeObject<AesKeys>(json);
                key = keys.Key;
                iv = keys.IV;
            }
            catch (Exception)
            {
                using (var aes = Aes.Create())
                {
                    aes.GenerateKey();
                    aes.GenerateIV();
                    var keys = new AesKeys() { Key = aes.Key, IV = aes.IV };
                    File.WriteAllText(file, JsonConvert.SerializeObject(keys), Encoding.UTF8);
                    key = keys.Key;
                    iv = keys.IV;
                }
            }
        }

        private static byte[] AesEncrypt(byte[] data, byte[] Key, byte[] IV)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(Convert.ToBase64String(data));
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private static byte[] AesDecrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return Convert.FromBase64String(srDecrypt.ReadToEnd());
                        }
                    }
                }

            }
        }


        private class AesKeys
        {
            public AesKeys()
            {

            }

            [JsonProperty(PropertyName = "1")]
            public byte[] Key { get; set; }

            [JsonProperty(PropertyName = "2")]
            public byte[] IV { get; set; }
        }

    }
}
