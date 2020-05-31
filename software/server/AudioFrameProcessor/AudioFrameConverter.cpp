//-----------------------------------------------------------------------
// <copyright file="AudioFrameConverter.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "AudioFrameConverter.h"
#include <Memorybuffer.h>

using namespace std;
using namespace Platform;
using namespace CrazyGiraffe::AudioFrameProcessor;
using namespace Microsoft::WRL;
using namespace Windows::Media;
using namespace Windows::Media::MediaProperties;
using namespace Windows::Foundation;

AudioFrameConverter::AudioFrameConverter(AudioEncodingProperties^ encodingProperties)
    : m_encodingProperties(encodingProperties)
    , m_bytesPerFloat(0)
    , m_maxValue(0)
    , m_conversionFunction(nullptr)
{
    if (encodingProperties == nullptr)
    {
        throw ref new InvalidArgumentException("encodingProperties");
    }

    // Pick the conversion function and parameters.
    wstring subType = encodingProperties->Subtype->Data();
    if (0 == subType.compare(L"PCM"))
    {
        uint32 bytesPerFloat = 0;
        uint32 maxValue = 0;
        switch (encodingProperties->BitsPerSample)
        {
        case 8:
            bytesPerFloat = 1;
            maxValue = 0xff;
            break;

        case 16:
            bytesPerFloat = 2;
            maxValue = 0xffff;
            break;

        case 24:
            bytesPerFloat = 3;
            maxValue = 0xffffff;
            break;

        case 32:
            bytesPerFloat = 4;
            maxValue = 0xffffffff;
            break;

        default:
            throw ref new InvalidArgumentException("encodingProperties->BitsPerSample");
        }

        m_bytesPerFloat = bytesPerFloat;
        m_maxValue = maxValue;
        m_conversionFunction = &AudioFrameConverter::ToPCMByteArray;
    }
    else if (0 == subType.compare(L"Float"))
    {
        m_bytesPerFloat = 0;
        m_conversionFunction = &AudioFrameConverter::ToFloatByteArray;
    }
    else
    {
        throw ref new InvalidArgumentException("encodingProperties->Subtype");
    }
}

AudioEncodingProperties^ AudioFrameConverter::EncodingProperties::get()
{
    return m_encodingProperties;
}

Array<byte>^ AudioFrameConverter::ToByteArray(AudioFrame^ frame)
{
    Array<byte>^ audioData = ref new Array<byte>(0);
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

        float* floatBuffer = reinterpret_cast<float*>(byteBuffer);
        uint32 floatBufferCapacity = byteBufferCapacity / sizeof(float);

        // Now convert to desired size.
        audioData = (this->*(this->m_conversionFunction))(floatBuffer, floatBufferCapacity, m_bytesPerFloat, m_maxValue);
    }

    return audioData;
}

Array<byte>^ AudioFrameConverter::ToPCMByteArray(
    float* floatBuffer,
    uint32 floatBufferCapacity,
    uint32 bytesPerFloat,
    uint32 maxValue)
{
    Array<byte>^ audioData = ref new Array<byte>(floatBufferCapacity * bytesPerFloat);

    union convert_data
    {
        uint32 value;
        unsigned char bytes[sizeof(uint32)];
    } convertData;

    for (uint32 i = 0, j = 0; i < audioData->Length && j < floatBufferCapacity; i += bytesPerFloat, j++)
    {
        convertData.value = static_cast<uint32>(floatBuffer[j] * maxValue);

        // convertData is little-endian: copy from end to start to convertData.bytes.
        for (uint32 k = 0; k < bytesPerFloat; k++)
        {
            audioData[i + k] = convertData.bytes[k];
        }
    }

    return audioData;
}

Array<byte>^ AudioFrameConverter::ToFloatByteArray(
    float* floatBuffer,
    uint32 floatBufferCapacity,
    uint32 dummy1,
    uint32 dummy2)
{
    UNREFERENCED_PARAMETER(dummy1);
    UNREFERENCED_PARAMETER(dummy2);

    uint32 bytesPerValue = sizeof(float);
    Array<byte>^ audioData = ref new Array<byte>(floatBufferCapacity * bytesPerValue);

    union convert_data
    {
        float value;
        unsigned char bytes[4];
    } convertData;

    for (uint32 i = 0, j = 0; i < audioData->Length && j < floatBufferCapacity; i += bytesPerValue, j++)
    {
        convertData.value = floatBuffer[j];
        for (uint32 k = 0; k < bytesPerValue; k++)
        {
            audioData[i + k] = convertData.bytes[k];
        }
    }

    return audioData;
}
