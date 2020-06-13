//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCaptureUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioCapture;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.ApplicationModel.Core;
    using Windows.Devices.Enumeration;
    using Windows.UI.Core;

    /// <summary>
    /// Unit test utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Get all input devices.
        /// </summary>
        /// <returns>An input device.</returns>
        public static IEnumerable<DeviceInformation> GetInputDevices()
        {
            // Find an input device
            Task<IEnumerable<DeviceInformation>> deviceTask = AudioDevice.FindInputDevicesAsync();
            deviceTask.Wait();

            return deviceTask.Result;
        }

        /// <summary>
        /// Get an input device.
        /// </summary>
        /// <returns>An input device.</returns>
        public static DeviceInformation GetInputDevice()
        {
            DeviceInformation defaultInputDevice = Utilities.GetInputDevices().FirstOrDefault();
            if (defaultInputDevice == null)
            {
                Assert.Inconclusive("Need an input device for this test.");
            }

            return defaultInputDevice;
        }

        /// <summary>
        /// Get all output devices.
        /// </summary>
        /// <returns>An output device.</returns>
        public static IEnumerable<DeviceInformation> GetOutputDevices()
        {
            // Find an output device
            Task<IEnumerable<DeviceInformation>> deviceTask = AudioDevice.FindOutputDevicesAsync();
            deviceTask.Wait();

            return deviceTask.Result;
        }

        /// <summary>
        /// Get an output device.
        /// </summary>
        /// <returns>An output device.</returns>
        public static DeviceInformation GetOutputDevice()
        {
            DeviceInformation defaultOutputDevice = Utilities.GetOutputDevices().FirstOrDefault();
            if (defaultOutputDevice == null)
            {
                Assert.Inconclusive("Need an output device for this test.");
            }

            return defaultOutputDevice;
        }

        /// <summary>
        /// Get an input device.
        /// </summary>
        /// <returns>An input device.</returns>
        public static DeviceInformation GetAnyDevice()
        {
            // Find an input device
            Task<IEnumerable<DeviceInformation>> deviceTask = AudioDevice.FindInputDevicesAsync();
            deviceTask.Wait();

            DeviceInformation defaultDevice = deviceTask.Result.FirstOrDefault();
            if (defaultDevice == null)
            {
                deviceTask = AudioDevice.FindOutputDevicesAsync();
                deviceTask.Wait();

                defaultDevice = deviceTask.Result.FirstOrDefault();
            }

            if (defaultDevice == null)
            {
                Assert.Inconclusive("Need an input device for this test.");
            }

            return defaultDevice;
        }
    }
}
