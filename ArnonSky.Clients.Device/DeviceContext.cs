// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using ArnonSky.Clients.Device.Configuration;
using ArnonSky.Clients.Device.Exceptions;
using ArnonSky.Clients.Device.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device
{
    /// <summary>
    /// A device that may communicate with Arnon Sky API
    /// </summary>
    public class DeviceContext
    {
        private const string DefaultSaveFile = "device.json";
        private const string DefaultApiBaseAddress = "https://api-v1.arnonsky.com";

        private readonly byte[] _protectedAccessKey;
        private readonly string _apiBaseAddress;
        private readonly ApiClient _apiClient;
        private DeviceConfigurationContainer _configuration;

        public string ProductKey { get; }

        /// <summary>
        /// Initializes DeviceContext
        /// </summary>
        /// <param name="productKey">Product key of the device</param>
        /// <param name="accessKey">Access key of the device</param>
        /// <param name="apiBaseAddress">Base address of the Arnon Sky API</param>
        public DeviceContext(string productKey, byte[] accessKey, string apiBaseAddress = DefaultApiBaseAddress)
        {
            ValidateProductKey();
            if (accessKey == null || accessKey.Length < 32)
            {
                throw new ArgumentException($"Invalid {nameof(accessKey)}");
            }

            ProductKey = productKey;

            _protectedAccessKey = Security.ProtectData(accessKey);
            _apiBaseAddress = apiBaseAddress;
            _apiClient = new ApiClient(productKey, accessKey, apiBaseAddress);
            _configuration = new DeviceConfigurationContainer();

            void ValidateProductKey()
            {
                if (productKey == null || productKey.Length < 16)
                {
                    throw new ArgumentException($"{nameof(productKey)} is too short.");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(productKey, @"^[A-Z0-9-]+$"))
                {
                    throw new ArgumentException($"{nameof(productKey)} has invalid characters, only letters A-Z, numerals 0-9 and character '-' is allowed.");
                }
            }
        }

        /// <summary>
        /// Loads a device from file.
        /// </summary>
        /// <exception cref="System.IO.InvalidDataException">Thrown when the file doesn't contain valid device state.</exception>
        public static DeviceContext LoadFromFile(string file = DefaultSaveFile)
        {
            var json = File.ReadAllText(file, Encoding.UTF8);
            try
            {
                var deviceSaveModel = JsonConvert.DeserializeObject<DeviceSaveModel>(json);
                return new DeviceContext(deviceSaveModel.ProductKey, Security.UnprotectData(deviceSaveModel.ProtectedAccessKey), deviceSaveModel.ApiBaseAddress);
            }
            catch (Exception)
            {
                throw new InvalidDataException();
            }
        }

        /// <summary>
        /// Saves the device to file.
        /// </summary>
        public void SaveToFile(string file = DefaultSaveFile)
        {
            var deviceSaveModel = new DeviceSaveModel()
            {
                ProductKey = ProductKey,
                ProtectedAccessKey = _protectedAccessKey,
                ApiBaseAddress = _apiBaseAddress
            };

            File.WriteAllText(file, JsonConvert.SerializeObject(deviceSaveModel), Encoding.UTF8);
        }

        /// <summary>
        /// Provisions a device to the Arnon Sky. Each device must have unique ProductKey.
        /// </summary>
        /// <returns>Device context that the device may use to communicate with Arnon Sky</returns>
        public static async Task<DeviceContext> ProvisionDeviceAsync(string provisioningKey, string deviceType = "Sample", string productKey = null, string apiBaseAddress = DefaultApiBaseAddress)
        {
            if (productKey == null)
            {
                productKey = Guid.NewGuid().ToString().ToUpper();
            }

            if (string.IsNullOrEmpty(provisioningKey))
            {
                throw new ArgumentException($"Invalid {nameof(provisioningKey)}");
            }

            if (string.IsNullOrEmpty(deviceType))
            {
                throw new ArgumentException($"Invalid {nameof(deviceType)}");
            }

            if (string.IsNullOrEmpty(productKey))
            {
                throw new ArgumentException($"Invalid {nameof(productKey)}");
            }

            var apiClient = new ApiClient(productKey, null, apiBaseAddress);
            var model = new PostDeviceModel(provisioningKey, productKey, deviceType);

            var accessKeyBase64 = await apiClient.SendRequestWithoutAuthorizationAsync<string>(HttpMethod.Post, "/devices", model).ConfigureAwait(false);
            var accessKey = Convert.FromBase64String(accessKeyBase64);

            return new DeviceContext(productKey, accessKey, apiBaseAddress);
        }


        /// <summary>
        /// Sends data to named source. Data object's properties are automatically matched to source's items by tagname. Arnon Sky API accepts only sourceId and itemIndices so this function automatically maps sourceName to sourceId and item names to item indices. Mapping is done using the device configuration that is read automatically by this function from Arnon Sky API.
        /// </summary>
        /// <param name="sourceName">Non-case sensitive name of the source in device's configuration to send data to.</param>
        /// <param name="timestamp">Timestamp of the data.</param>
        /// <param name="data">Data values of items</param>
        /// <returns></returns>
        public Task SimpleSendDataAsync(string sourceName, DateTime timestamp, object data)
        {
            var values = new List<TagValue>();
            foreach (var prop in data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                values.Add(new TagValue(prop.Name, prop.GetValue(data, null)));
            }

            return SimpleSendDataAsync(sourceName, timestamp, values);
        }

        /// <summary>
        /// Sends data to named source. Arnon Sky API accepts only sourceId and itemIndices so this function automatically maps sourceName to sourceId and item names to item indices. Mapping is done using the device configuration that is read automatically by this function from Arnon Sky API.
        /// </summary>
        /// <param name="sourceName">Non-case sensitive name of the source in device's configuration to send data to.</param>
        /// <param name="timestamp">Timestamp of the data.</param>
        /// <param name="itemValues">Data values of items</param>
        /// <returns></returns>
        public async Task SimpleSendDataAsync(string sourceName, DateTime timestamp, IEnumerable<TagValue> itemValues)
        {
            await RefreshConfigurationAsync(TimeSpan.FromSeconds(300)).ConfigureAwait(false);

            var siteId = GetSiteId();
            var sourceId = GetSourceId(sourceName);

            // convert itemValues to array of values that is required by the API
            var itemIndices = new List<int?>();
            var maxIndex = -1;
            int i = -1;
            foreach (var value in itemValues)
            {
                i += 1;
                var itemIndex = TryGetItemIndexFromConfiguration(sourceId, value.TagName);
                if (itemIndex.HasValue)
                {
                    itemIndices[i] = itemIndex;
                    maxIndex = Math.Max(maxIndex, itemIndex.Value);
                }
            }

            var itemSendValues = new object[maxIndex + 1];

            i = -1;
            foreach (var value in itemValues)
            {
                i += 1;
                if (itemIndices[i].HasValue)
                {
                    itemSendValues[itemIndices[i].Value] = value.Value;
                }
            }

            await PostDataAsync(siteId, sourceId, timestamp, itemSendValues).ConfigureAwait(false);
        }

        public Task RefreshConfigurationAsync()
        {
            return GetConfigurationAsync();
        }

        /// <summary>
        /// Retrieves the configuration of the device. Configuration is cached for 15 seconds to protect API from too many configuration requests.
        /// </summary>
        /// <returns>The device configuration model</returns>
        public async Task<DeviceConfiguration> GetConfigurationAsync()
        {
            if (DateTime.UtcNow.Subtract(_configuration.ReadTimestamp) > TimeSpan.FromSeconds(15))
            {
                var newConfiguration = await _apiClient.SendRequestAsync<GetDeviceDetailsModel>(HttpMethod.Get, $"devices/{ProductKey}").ConfigureAwait(false);
                UpdateConfiguration(newConfiguration);
            }
            return _configuration.Current;
        }

        /// <summary>
        /// Posts data to the Arnon Sky API.
        /// </summary>
        /// <param name="siteId">Id of the site to post</param>
        /// <param name="sourceId">Id of the source to post</param>
        /// <param name="timestamp">Timestamp of the data</param>
        /// <param name="itemValues">Data values of items</param>
        /// <returns></returns>
        public async Task PostDataAsync(long siteId, long sourceId, DateTime timestamp, IEnumerable<object> itemValues)
        {
            var model = new PostDataModel(timestamp.ToString("o"), itemValues);
            await _apiClient.SendRequestAsync(HttpMethod.Post, $"sites/{siteId}/sources/{sourceId}/data", model).ConfigureAwait(false);
        }

        /// <summary>
        /// Posts multiple rows of data to the Arnon Sky API.
        /// </summary>
        /// <param name="siteId">Id of the site to post</param>
        /// <param name="sourceId">Id of the source to post</param>
        /// <param name="multipleDataRowContainer">Object that contains the data rows</param>
        /// <returns></returns>
        public async Task PostMultipleDataRowsAsync(long siteId, long sourceId, MultipleDataRowContainer multipleDataRowContainer)
        {
            var model = multipleDataRowContainer.ToDataPostModel();
            await _apiClient.SendRequestAsync(HttpMethod.Post, $"sites/{siteId}/sources/{sourceId}/data", model).ConfigureAwait(false);
        }

        private void UpdateConfiguration(GetDeviceDetailsModel newConfiguration)
        {
            var replaceConf = new DeviceConfigurationContainer() { ReadTimestamp = DateTime.UtcNow, Current = newConfiguration.ToDeviceConfiguration() };
            Interlocked.Exchange(ref _configuration, replaceConf);
        }


        private async Task RefreshConfigurationAsync(TimeSpan configurationRefreshInterval)
        {
            // limit rate of configuration requests to one per minInterval
            if (DateTime.UtcNow.Subtract(_configuration.ReadTimestamp) > configurationRefreshInterval)
            {
                await GetConfigurationAsync().ConfigureAwait(false);
            }
        }

        private long GetSiteId()
        {
            if (!_configuration.Current.SiteId.HasValue)
            {
                throw new DeviceNotLinkedToSiteException(ProductKey);
            }

            return _configuration.Current.SiteId.Value;
        }

        private long GetSourceId(string sourceName)
        {
            foreach (var source in _configuration.Current.Sources)
            {
                if (source.Name.ToLowerInvariant() == sourceName.ToLowerInvariant())
                {
                    return source.SourceId;
                }
            }

            throw new SourceNameNotFoundException(sourceName);
        }

        private int? TryGetItemIndexFromConfiguration(long sourceId, string tagname)
        {
            SourceConfiguration source = null;
            for (var i = 0; i < _configuration.Current.Sources.Count; i++)
            {
                if (_configuration.Current.Sources[i].SourceId == sourceId)
                {
                    source = _configuration.Current.Sources[i];
                }
            }

            if (source != null)
            {
                for (var i = 0; i < source.Items.Length; i++)
                {
                    var item = source.Items[i];
                    if (item.TryGetValue("tagname", out var itemTagname))
                    {
                        if (itemTagname is string t && t.ToLowerInvariant() == tagname.ToLowerInvariant())
                        {
                            return i;
                        }
                    }
                }
            }

            return null;
        }


    }
}

