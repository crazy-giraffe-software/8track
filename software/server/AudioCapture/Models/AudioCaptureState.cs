//-----------------------------------------------------------------------
// <copyright file="AudioCaptureState.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCapture
{
    using System;

    /// <summary>
    /// State for the capture.
    /// </summary>
    public enum AudioCaptureState
    {
        /// <summary>
        /// Capture is idle.
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Capture is starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        ///  Capture is running.
        /// </summary>
        Running = 2,

        /// <summary>
        ///  Capture is stopping.
        /// </summary>
        Stopping = 3,
    }
}
