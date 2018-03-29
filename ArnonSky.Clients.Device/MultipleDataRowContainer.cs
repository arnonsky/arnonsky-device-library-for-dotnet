// Copyright (c) ARNON Solutions Oy. All rights reserved.
// Licensed under the MIT License. See LICENSE in the root for license information.

using ArnonSky.Clients.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnonSky.Clients.Device
{
    /// <summary>
    /// Data collection that may contain item data with multiple timestamps
    /// </summary>
    public class MultipleDataRowContainer
    {
        private Dictionary<DateTime, object[]> _data = new Dictionary<DateTime, object[]>();

        public MultipleDataRowContainer()
        {

        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear() => _data.Clear();

        /// <summary>
        /// Adds or replaces the data for the timestamp
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="itemValues"></param>
        public void SetData(DateTime timestamp, object[] itemValues) 
            => _data[timestamp] = itemValues;

        /// <summary>
        /// Adds or replaces the data of specific item for the timestamp.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="itemIndex"></param>
        /// <param name="itemValue"></param>
        public void SetData(DateTime timestamp, int itemIndex, object itemValue)
        {
            if (_data.TryGetValue(timestamp, out var valuesArray))
            {
                if (valuesArray.Length <= itemIndex)
                {
                    Array.Resize(ref valuesArray, itemIndex + 1);
                }

                valuesArray[itemIndex] = itemValue;
            }
            else
            {
                valuesArray = new object[itemIndex + 1];
                valuesArray[itemIndex] = itemValue;
            }

            _data[timestamp] = valuesArray;
        }

        public IEnumerable<PostDataModel> ToDataPostModel() 
            => _data.Select(entry => new PostDataModel(entry.Key.ToString("o"), entry.Value));

    }
}
