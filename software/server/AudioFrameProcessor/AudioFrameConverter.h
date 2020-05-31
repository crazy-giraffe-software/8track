//-----------------------------------------------------------------------
// <copyright file="AudioFrameConverter.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioFrameProcessor
{
    /// <summary>
    /// Interface to convert from AudioFrame to byte array.
    /// </summary>
    public interface class IAudioFrameConverter
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
        /// Convert an <see cref="Windows::Media::AudioFrame" /> to a byte array.
        /// </summary>
        Platform::Array<byte>^ ToByteArray(Windows::Media::AudioFrame^ frame);
    };

    /// <summary>
    /// Conversion from AudioFrame to byte array.
    /// </summary>
    public ref class AudioFrameConverter sealed : [Windows::Foundation::Metadata::Default] IAudioFrameConverter
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="AudioFrameConverter" /> class.
        /// </summary>
        AudioFrameConverter(Windows::Media::MediaProperties::AudioEncodingProperties^ encodingProperties);

        /// <summary>
        /// <summary>
        /// Gets the audio encoding properties.
        /// </summary>
        virtual property Windows::Media::MediaProperties::AudioEncodingProperties^ EncodingProperties
        {
            Windows::Media::MediaProperties::AudioEncodingProperties^ get();
        }

        /// <summary>
        /// Convert an <see cref="Windows::Media::AudioFrame" /> to a byte array.
        /// </summary>
        virtual Platform::Array<byte>^ ToByteArray(Windows::Media::AudioFrame^ frame);

    private:
        /// <summary>
        /// Convert a float array to an PCM byte array.
        /// </summary>
        Platform::Array<byte>^ ToPCMByteArray(
            float* floatBuffer,
            uint32 floatBufferCapacity,
            uint32 bytesPerFloat,
            uint32 max_Value);

        /// <summary>
        /// Convert a float array to an 32-bit float byte array.
        /// </summary>
        Platform::Array<byte>^ ToFloatByteArray(
            float* floatBuffer,
            uint32 floatBufferCapacity,
            uint32 dummy1,
            uint32 dummy2);

    private:
        /// <summary>
        /// The audio encoding properties.
        /// </summary>
        Windows::Media::MediaProperties::AudioEncodingProperties^ m_encodingProperties;

        ///
        /// Paramater to pass to conversionFunction's bytesPerFloat parameter;
        ///
        uint32 m_bytesPerFloat;

        ///
        /// Paramater to pass to conversionFunction's maxValue parameter;
        ///
        uint32 m_maxValue;

        ///
        /// Function pointer to conversion function
        ///
        Platform::Array<byte>^ (AudioFrameConverter::* m_conversionFunction)(float*, uint32, uint32, uint32);
    };
} }
