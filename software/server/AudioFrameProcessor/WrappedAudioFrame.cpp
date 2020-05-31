//-----------------------------------------------------------------------
// <copyright file="WrappedAudioFrame.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "WrappedAudioFrame.h"
#include <Memorybuffer.h>
#include <random>

using namespace Microsoft::WRL;
using namespace Platform;
using namespace CrazyGiraffe::AudioFrameProcessor;
using namespace Windows::Media;
using namespace Windows::Media::MediaProperties;
using namespace Windows::Foundation;

WrappedAudioFrame::WrappedAudioFrame(uint32 capacity)
    : m_frameCapacity(capacity)
{
    m_currentFrame = ref new AudioFrame(capacity);

    // Estimate the duration assuming 44100 rate, 16 bits, 2 channels.
    // Each float requires 4 bytes of storage, 2 floats per channel.
    long numberOfSamples = (int)(capacity / 8);
    long numberOfMilliseconds = (int)(numberOfSamples / 44.1);

    TimeSpan timeSpan = { 0 };
    timeSpan.Duration = numberOfMilliseconds * 100;
    m_currentFrame->Duration = timeSpan;
}

AudioFrame^ WrappedAudioFrame::CurrentFrame::get()
{
    return m_currentFrame;
}

uint32 WrappedAudioFrame::Capacity::get()
{
    return m_frameCapacity;
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateEmpty()
{
    return ref new WrappedAudioFrame(0);
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateBlank()
{
    return CreateBlank(2048);
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateBlank(uint32 size)
{
    return ref new WrappedAudioFrame(size);
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateRandom()
{
    return CreateRandom(2048);
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateRandom(uint32 size)
{
    std::random_device rd;
    std::default_random_engine generator(rd());
    std::uniform_real_distribution<float> distribution(0.0, 1.0);

    WrappedAudioFrame^ mockFrame = ref new WrappedAudioFrame(size);
    mockFrame->PopulateFrame([&generator, &distribution]() -> float { return distribution(generator); });
    return mockFrame;
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateFixed(float value)
{
    return CreateFixed(value, 2048);
}

/*static*/
WrappedAudioFrame^ WrappedAudioFrame::CreateFixed(float value, uint32 size)
{
    if (value < -1 || value > 1)
    {
        throw ref new InvalidArgumentException("value");
    }

    WrappedAudioFrame^ mockFrame = ref new WrappedAudioFrame(size);
    mockFrame->PopulateFrame([value]() -> float { return value; });
    return mockFrame;
}

void WrappedAudioFrame::PopulateFrame(std::function<float()> valueFunction)
{
    // Extract data for audio frame.
    AudioBuffer^ audioBuffer = m_currentFrame->LockBuffer(AudioBufferAccessMode::ReadWrite);
    unsigned int cap = audioBuffer->Capacity;
    unsigned int len = audioBuffer->Length;

    IMemoryBufferReference^ bufferReference = audioBuffer->CreateReference();
    unsigned int cap2 = bufferReference->Capacity;

    ComPtr<IMemoryBufferByteAccess> bufferAccess;
    HRESULT hr = reinterpret_cast<IInspectable*>(bufferReference)->QueryInterface(IID_PPV_ARGS(&bufferAccess));
    if (FAILED(hr))
    {
        throw Platform::Exception::CreateException(hr);
    }

    // Get a pointer to the audio buffer
    byte* byteBuffer;
    uint32 byteBufferCapacity;
    hr = bufferAccess->GetBuffer(&byteBuffer, &byteBufferCapacity);
    if (FAILED(hr))
    {
        throw Platform::Exception::CreateException(hr);
    }

    // Populate the frame.
    union float_bytes
    {
        float value;
        unsigned char bytes[sizeof(float)];
    } floatData;

    uint32 bytesPerFloat = sizeof(float);
    for (unsigned int i = 0; i < byteBufferCapacity; i = i + bytesPerFloat)
    {
        floatData.value = valueFunction();
        for (unsigned int k = 0; k < bytesPerFloat && i + k < byteBufferCapacity; k++)
        {
            byteBuffer[i + k] = floatData.bytes[k];
        }
    }
}
