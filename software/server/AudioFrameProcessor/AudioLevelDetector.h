//-----------------------------------------------------------------------
// <copyright file="AudioLevelDetector.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

#include "AudioLevelDetector.h"
#include "AudioThreholdDetectedEventArgs.h"

namespace CrazyGiraffe { namespace AudioFrameProcessor
{
    /// Fwd Decl.
    ref class AudioLevelDetector;

    /// <summary>
    /// Interface to detect a specified level for a period of time.
    /// </summary>
    public interface class IAudioLevelDetector
    {
        /// <summary>
        /// <summary>
        /// Gets the audio encoding properties.
        /// </summary>
        property Windows::Media::MediaProperties::AudioEncodingProperties^ EncodingProperties
        {
            Windows::Media::MediaProperties::AudioEncodingProperties^ get();
        }

        /// <summary>
        /// <summary>
        /// Gets the audio threshold value.
        /// </summary>
        property double ThresholdValue
        {
            double get();
        }

        /// <summary>
        /// Gets the threshold time for the value to trigger an event.
        /// </summary>
        property Windows::Foundation::TimeSpan ThresholdTimeSpan
        {
            Windows::Foundation::TimeSpan get();
        }

        /// <summary>
        /// Gets the threshold status.
        /// </summary>
        property ThresholdStatus Status
        {
            ThresholdStatus get();
        }

        /// <summary>
        /// Event handler for threhold detected.
        /// </summary>
        event Windows::Foundation::TypedEventHandler<AudioLevelDetector^, AudioThreholdDetectedEventArgs^>^ ThreholdDetected;

        /// <summary>
        /// process an <see cref="Windows::Media::AudioFrame" />.
        /// </summary>
        void ProcessFrame(Windows::Media::AudioFrame^ frame);
    };

    /// <summary>
    /// Class to detect a specified level for a period of time.
    /// </summary>
    public ref class AudioLevelDetector sealed : [Windows::Foundation::Metadata::Default] IAudioLevelDetector
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="AudioLevelDetector" /> class.
        /// </summary>
        AudioLevelDetector(
            Windows::Media::MediaProperties::AudioEncodingProperties^ encodingProperties,
            double thresholdValue,
            Windows::Foundation::TimeSpan thresholdTimeSpan);

        /// <summary>
        /// <summary>
        /// Gets the audio encoding properties.
        /// </summary>
        virtual property Windows::Media::MediaProperties::AudioEncodingProperties^ EncodingProperties
        {
            Windows::Media::MediaProperties::AudioEncodingProperties^ get();
        }

        /// <summary>
        /// <summary>
        /// Gets the audio threshold value.
        /// </summary>
        virtual property double ThresholdValue
        {
            double get();
        }

        /// <summary>
        /// Gets the threshold time for the value to trigger an event.
        /// </summary>
        virtual property Windows::Foundation::TimeSpan ThresholdTimeSpan
        {
            Windows::Foundation::TimeSpan get();
        }

        /// <summary>
        /// Gets the threshold status.
        /// </summary>
        virtual property ThresholdStatus Status
        {
            ThresholdStatus get();
        }

        /// <summary>
        /// Event handler for threhold detected.
        /// </summary>
        virtual event Windows::Foundation::TypedEventHandler<AudioLevelDetector^, AudioThreholdDetectedEventArgs^>^ ThreholdDetected;

        /// <summary>
        /// process an <see cref="Windows::Media::AudioFrame" />.
        /// </summary>
        virtual void ProcessFrame(Windows::Media::AudioFrame^ frame);

    internal:
        /// <summary>
        /// Update the status and send notifications.
        /// </summary>
        /// <param name="newStatus">the new status.</param>
        void UpdateStatus(ThresholdStatus newStatus);

    private:
        /// <summary>
        /// The audio encoding properties.
        /// </summary>
        Windows::Media::MediaProperties::AudioEncodingProperties^ m_encodingProperties;

        /// <summary>
        /// The audio threshold value.
        /// </summary>
        double m_thresholdValue;

        /// <summary>
        /// The audio threshold duration.
        /// </summary>
        long long m_thresholdDuration;

        /// <summary>
        /// The status of the sample.
        /// </summary>
        ThresholdStatus m_status;

        /// <summary>
        /// The max count needed to meet the threshold.
        /// </summary>
        long long m_thresholdMaxCount;

        /// <summary>
        /// The count of samples below the threshold.
        /// </summary>
        long long m_thresholdBelowCount;

        /// <summary>
        /// The count of samples above the threshold.
        /// </summary>
        long long m_thresholdAboveCount;
    };
} }
