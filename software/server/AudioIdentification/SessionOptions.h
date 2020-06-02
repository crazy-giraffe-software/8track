//-----------------------------------------------------------------------
// <copyright file="SessionOptions.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification
{
    /// <summary>
    ///  Option for a music id session.
    /// </summary>
    public interface class ISessionOptions
    {
    public:
        /// <summary>
        /// Gets or sets the audio sample rate in Hz, e.g. 44100.
        /// </summary>
        property uint16 SampleRate
        {
            uint16 get();
            void set(uint16 value);
        }

        /// <summary>
        /// Gets or sets the audio sample size in bits, e.g. 16.
        /// </summary>
        property uint16 SampleSize
        {
            uint16 get();
            void set(uint16 value);
        }

        /// <summary>
        /// Gets or sets the number of audio channels, e.g. 2.
        /// </summary>
        property uint16 ChannelCount
        {
            uint16 get();
            void set(uint16 value);
        }
    };

    /// <summary>
    ///  Option for a music id session.
    /// </summary>
    public ref class SessionOptions sealed : public ISessionOptions
    {
    public:
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionOptions" /> class.
        /// </summary>
        SessionOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionOptions" /> class.
        /// </summary>
        /// <param name="options">the options.</param>
        SessionOptions(SessionOptions^ options);

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionOptions" /> class.
        /// </summary>
        /// <param name="sampleRate">the audio sample rate in Hz, e.g. 44100.</param>
        /// <param name="sampleSize">the audio sample size in bits, e.g. 16.</param>
        /// <param name="channelCount">the number of audio channels, e.g. 2.</param>
        SessionOptions(
            uint16 sampleRate,
            uint16 sampleSize,
            uint16 channelCount);

        /// <summary>
        /// Gets or sets the audio sample rate in Hz, e.g. 44100.
        /// </summary>
        virtual property uint16 SampleRate
        {
            uint16 get();
            void set(uint16 value);
        }

        /// <summary>
        /// Gets or sets the audio sample size in bits, e.g. 16.
        /// </summary>
        virtual property uint16 SampleSize
        {
            uint16 get();
            void set(uint16 value);
        }

        /// <summary>
        /// Gets or sets the number of audio channels, e.g. 2.
        /// </summary>
        virtual property uint16 ChannelCount
        {
            uint16 get();
            void set(uint16 value);
        }

    private:
        /// <summary>
        /// The audio sample rate in Hz, e.g. 44100.
        /// </summary>
        uint16 m_sampleRate;

        /// <summary>
        /// The audio sample size in bits, e.g. 16.
        /// </summary>
        uint16 m_sampleSize;

        /// <summary>
        /// The number of audio channels, e.g. 2.
        /// </summary>
        uint16 m_channelCount;
    };
} }
