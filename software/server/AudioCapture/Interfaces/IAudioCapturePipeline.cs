//-----------------------------------------------------------------------
// <copyright file="IAudioCapturePipeline.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCapture
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Interface for capturing and broadcasting audio.
    /// </summary>
    public interface IAudioCapturePipeline : IDisposable
    {
        /// <summary>
        /// An event that client can be use to be notified whenever
        /// pipeline is started to stopped.
        /// </summary>
        event EventHandler CaptureStateChanged;

        /// <summary>
        /// Gets the state for the capture.
        /// </summary>
        AudioCaptureState CaptureState { get; }

        /// <summary>
        ///  Gets the encoding properties of the pipeline.
        /// </summary>
        AudioEncodingProperties EncodingProperties { get; }

        /// <summary>
        /// Gets or sets the capture device identifier.
        /// </summary>
        string CaptureDeviceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the render device.
        /// </summary>
        string RenderDeviceIdentifier { get; set; }

        /// <summary>
        /// Gets a list of capture plug-ins.
        /// </summary>
        IList<IAudioCapturePlugin> CapturePlugins { get; }

        /// <summary>
        /// Start the audio capture.
        /// </summary>
        /// <returns>A task on which to wait for completion.</returns>
        Task StartAsync();

        /// <summary>
        /// Stop the audio capture.
        /// </summary>
        /// <returns>A task on which to wait for completion.</returns>
        Task StopAsync();
    }
}
