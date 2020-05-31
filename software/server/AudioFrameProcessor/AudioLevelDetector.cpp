//-----------------------------------------------------------------------
// <copyright file="AudioLevelDetector.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "AudioLevelDetector.h"
#include <Memorybuffer.h>

using namespace Platform;
using namespace CrazyGiraffe::AudioFrameProcessor;
using namespace Microsoft::WRL;
using namespace Windows::Media;
using namespace Windows::Media::MediaProperties;
using namespace Windows::Foundation;

AudioLevelDetector::AudioLevelDetector(
    AudioEncodingProperties^ encodingProperties,
    double thresholdValue,
    TimeSpan thresholdTimeSpan)
    : m_status(ThresholdStatus::Unknown)
    , m_encodingProperties(encodingProperties)
    , m_thresholdValue(thresholdValue)
    , m_thresholdDuration(thresholdTimeSpan.Duration)
    , m_thresholdBelowCount(0)
    , m_thresholdAboveCount(0)
{
    if (encodingProperties == nullptr)
    {
        throw ref new InvalidArgumentException("encodingProperties");
    }

    // The bit count per second is (sample rate (bits/sec) * channel count).
    long long bitCountPerSecond = this->m_encodingProperties->SampleRate * this->m_encodingProperties->ChannelCount;

    // m_thresholdDuration is a time period expressed in 100-nanosecond units
    // 10x9 nanoseconds in a second, 10x7 100-nanoseconds in a second.
    long long secondsPer100NanoSeconds = 10000000;

    // The max count is bit count per second / duration (seconds).
    m_thresholdMaxCount = (bitCountPerSecond * m_thresholdDuration) / secondsPer100NanoSeconds;
}

AudioEncodingProperties^ AudioLevelDetector::EncodingProperties::get()
{
    return m_encodingProperties;
}

double AudioLevelDetector::ThresholdValue::get()
{
    return m_thresholdValue;
}

TimeSpan AudioLevelDetector::ThresholdTimeSpan::get()
{
    TimeSpan timeSpan = { 0 };
    timeSpan.Duration = m_thresholdDuration;
    return timeSpan;
}

ThresholdStatus AudioLevelDetector::Status::get()
{
    return m_status;
}

void AudioLevelDetector::ProcessFrame(AudioFrame^ frame)
{
    if (frame != nullptr)
    {
        // Extract data for audio frame.
        AudioBuffer^ audioBuffer = frame->LockBuffer(AudioBufferAccessMode::Read);
        IMemoryBufferReference^ bufferReference = audioBuffer->CreateReference();

        ComPtr<IMemoryBufferByteAccess> bufferAccess;
        HRESULT hr = reinterpret_cast<IInspectable*>(bufferReference)->QueryInterface(IID_PPV_ARGS(&bufferAccess));
        if (FAILED(hr))
        {
            throw Exception::CreateException(hr);
        }

        // Get a pointer to the audio buffer
        byte* byteBuffer;
        uint32 byteBufferCapacity;
        hr = bufferAccess->GetBuffer(&byteBuffer, &byteBufferCapacity);
        if (FAILED(hr))
        {
            throw Exception::CreateException(hr);
        }

        // While the sample may be mono or stereo, we don't really care. We need each signal to compare to the
        // threshold the same way but it may impact the time of the frame: a store frame is twice the size for the
        // same time period.
        uint32 bytesPerFloat = sizeof(float);
        for (unsigned int i = 0; i < byteBufferCapacity; i += bytesPerFloat)
        {
            if (i + bytesPerFloat <= byteBufferCapacity)
            {
                float* floatValue = reinterpret_cast<float*>(byteBuffer + i);
                if (m_status != ThresholdStatus::BelowThrehold && std::abs(*floatValue) <= m_thresholdValue)
                {
                    ++m_thresholdBelowCount;
                    if (m_thresholdBelowCount >= m_thresholdMaxCount)
                    {
                        UpdateStatus(ThresholdStatus::BelowThrehold);
                        m_thresholdBelowCount = 0;
                    }
                }
                else if (m_status != ThresholdStatus::AboveThreshold && std::abs(*floatValue) > m_thresholdValue)
                {
                    ++m_thresholdAboveCount;
                    if (m_thresholdAboveCount >= m_thresholdMaxCount)
                    {
                        UpdateStatus(ThresholdStatus::AboveThreshold);
                        m_thresholdAboveCount = 0;
                    }
                }
            }
        }
    }
}

void AudioLevelDetector::UpdateStatus(ThresholdStatus newStatus)
{
    // Update.
    bool changed = (m_status != newStatus);
    m_status = newStatus;

    if (changed)
    {
        AudioThreholdDetectedEventArgs^ eventArgs = ref new AudioThreholdDetectedEventArgs(newStatus);
        ThreholdDetected(this, eventArgs);
    }
}
