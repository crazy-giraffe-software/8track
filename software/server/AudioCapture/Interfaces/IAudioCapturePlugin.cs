//-----------------------------------------------------------------------
// <copyright file="IAudioCapturePlugin.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCapture
{
    using Windows.Media;

    /// <summary>
    /// Interface for processing an audio frame.
    /// </summary>
    public interface IAudioCapturePlugin
    {
        /// <summary>
        /// Process audio data from the graph.
        /// </summary>
        /// <param name="frame">The audio frame to process.</param>
        void ProcessAudioFrame(AudioFrame frame);
    }
}
