//-----------------------------------------------------------------------
// <copyright file="AudioDevice.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCapture
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;

    /// <summary>
    /// A class representing an audio device.
    /// </summary>
    public static class AudioDevice
    {
        /// <summary>
        /// Get an audio  devices from an identifier.
        /// </summary>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <returns>An AudioDevice.</returns>
        public static Task<DeviceInformation> GetDeviceAsync(string deviceIdentifier)
        {
            if (deviceIdentifier == null)
            {
                return Task.FromResult<DeviceInformation>(null);
            }

            return DeviceInformation.CreateFromIdAsync(deviceIdentifier).AsTask();
        }

        /// <summary>
        /// Get a collection of audio capture devices.
        /// </summary>
        /// <returns>A collection of AudioDevice for all audio input devices.</returns>
        public static Task<IEnumerable<DeviceInformation>> FindInputDevicesAsync()
        {
            return FindDevicesAsync(DeviceClass.AudioCapture);
        }

        /// <summary>
        /// Get a collection of audio render devices.
        /// </summary>
        /// <returns>A collection of AudioDevice for all audio output devices.</returns>
        public static Task<IEnumerable<DeviceInformation>> FindOutputDevicesAsync()
        {
            return FindDevicesAsync(DeviceClass.AudioRender);
        }

        /// <summary>
        /// Get a collection of devices.
        /// </summary>
        /// <param name="deviceClass">the class of device to get.</param>
        /// <returns>A collection of device AudioDevice for the device class.</returns>
        private static async Task<IEnumerable<DeviceInformation>> FindDevicesAsync(DeviceClass deviceClass)
        {
            List<DeviceInformation> devices = new List<DeviceInformation>();

            DeviceInformationCollection deviceCollection = await DeviceInformation.FindAllAsync(deviceClass);
            for (int i = 0; i < deviceCollection.Count; i++)
            {
                DeviceInformation device = deviceCollection[i];
                devices.Add(device);

                // Debug info.
                Debug.WriteLine("Device: " + device.Name);
                foreach (var prop in device.Properties)
                {
                    var key = prop.Key;
                    var value = prop.Value;
                    string propVal = key + "=" + ((value != null) ? value.ToString() : string.Empty);
                    Debug.WriteLine("  " + propVal);
                }

                Debug.WriteLine(string.Empty);
            }

            return devices;
        }
    }
}
