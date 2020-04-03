// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device
{
    /// <summary>
    /// Class that DeviceContext uses for communication with Arnon Sky API using the HTTP protocol.
    /// </summary>
    class ApiClient
    {
        private const string AuthScheme = "Device";

        private readonly HttpClient _httpClient;

        private byte[] _protectedAccessKey = null;

        public string ProductKey { get; }        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="apiBaseAddress"></param>
        /// <param name="tryOptimizeServicePointManager"></param>
        public ApiClient(string productKey, byte[] accessKey, string apiBaseAddress, bool tryOptimizeServicePointManager = true)
        {
            ProductKey = productKey;

            // protect the accessKey so it's not stored as raw value in memory
            if (accessKey != null)
            {
                _protectedAccessKey = Security.ProtectData(accessKey);
            }

#if !NETSTANDARD
            if (tryOptimizeServicePointManager)
            {
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.UseNagleAlgorithm = false;
            }
#endif

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseAddress)
            };
        }

        public void SetAccessKey(byte[] accessKey)
        {
            if (accessKey == null)
            {
                throw new ArgumentNullException(nameof(accessKey));
            }
            _protectedAccessKey = Security.ProtectData(accessKey);
        }

        private async Task<HttpResponseMessage> DoSendRequestAsync(HttpMethod method, string uri, object model, bool addDeviceAuthorization = true)
        {
            var request = PrepareRequestAsync(method, uri, model);

            if (addDeviceAuthorization)
            {
                var timestamp = DateTime.UtcNow.ToString("o");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthScheme, $"{ProductKey}:{CalculateSignature(method.ToString(), $"/{uri}", timestamp)}");
                request.Headers.Add("Device-Date", timestamp);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public Task SendRequestAsync(HttpMethod method, string uri, object model = null)
        {
            return DoSendRequestAsync(method, uri, model);
        }

        public async Task<T> SendRequestAsync<T>(HttpMethod method, string uri, object model = null)
        {
            var response = await DoSendRequestAsync(method, uri, model).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        public Task SendRequestWithoutAuthorizationAsync(HttpMethod method, string uri, object model = null)
        {
            return DoSendRequestAsync(method, uri, model, false);
        }

        public async Task<T> SendRequestWithoutAuthorizationAsync<T>(HttpMethod method, string uri, object model = null)
        {
            var response = await DoSendRequestAsync(method, uri, model, false).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        public HttpRequestMessage PrepareRequestAsync(HttpMethod method, string uri, object model)
        {
            var request = new HttpRequestMessage(method, $"{uri}");

            if (model != null)
            {
                var modelJson = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(modelJson, Encoding.UTF8, "application/json");
            }

            return request;
        }

        /// <summary>
        /// Uses the access key to calculate signature for the API request
        /// </summary>
        /// <param name="accessKeyBytes">Base64 decoded access key of the device</param>
        /// <param name="verb">Request verb</param>
        /// <param name="path">Request path</param>
        /// <param name="timestamp">Request timestamp</param>
        /// <returns>Signature for the request</returns>
        private string CalculateSignature(string verb, string path, string timestamp)
        {
            var stringToSign = string.Join("\n", timestamp, verb, path);

            var unprotectedAccessKeyBytes = Security.UnprotectData(_protectedAccessKey);
            using (var hmacSha256 = new HMACSHA256(unprotectedAccessKeyBytes))
            {
                var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
                var signatureBytes = hmacSha256.ComputeHash(bytesToSign);
                return Convert.ToBase64String(signatureBytes);
            }
        }




    }
}
