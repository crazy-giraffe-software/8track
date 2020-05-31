//-----------------------------------------------------------------------
// <copyright file="AudioThreholdDetectedEventArgs.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioFrameProcessor
{
    /// <summary>
    /// Status for the sample.
    /// </summary>
    public enum class ThresholdStatus
    {
        /// <summary>
        /// Level is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Sample is below the threshold
        /// </summary>
        BelowThrehold = 1,

        /// <summary>
        ///  Sample is above the threshold.
        /// </summary>
        AboveThreshold = 2
    };

    /// <summary>
    ///  A state changed event argument class.
    /// </summary>
    public ref class AudioThreholdDetectedEventArgs sealed
    {
    public:
        /// <summary>
        /// Gets the sample status.
        /// </summary>
        property ThresholdStatus Status
        {
            ThresholdStatus get();
        }

    internal:
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioThreholdDetectedEventArgs" /> class.
        /// </summary>
        /// <param name="status">The sample status.</param>
        AudioThreholdDetectedEventArgs(ThresholdStatus status);

    private:
        /// <summary>
        /// The sample status.
        /// </summary>
        ThresholdStatus m_status;
    };
} }
