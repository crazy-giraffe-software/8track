//-----------------------------------------------------------------------
// <copyright file="WrappedAudioFrame.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioFrameProcessor
{
    /// <summary>
    /// A wrapped audio frame class
    /// </summary>
    public ref class WrappedAudioFrame sealed
    {
    public:
        /// <summary>
        /// Gets the current AudioFrame.
        /// </summary>
        property Windows::Media::AudioFrame^ CurrentFrame
        {
            Windows::Media::AudioFrame^ get();
        }

        /// <summary>
        /// Gets the frame capacity.
        /// </summary>
        property uint32 Capacity
        {
            uint32 get();
        }

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateEmpty();

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateBlank();

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateBlank(uint32 size);

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateRandom();

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateRandom(uint32 size);

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateFixed(float value);

        /// <summary>
        /// Gets an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        static WrappedAudioFrame^ CreateFixed(float value, uint32 size);

    private:
        /// <summary>
        /// Create an instance of the <see cref="WrappedAudioFrame" /> class.
        /// </summary>
        WrappedAudioFrame(uint32 capacity);


        /// <summary>
        /// Populate the frame with audio data.
        /// </summary>
        void PopulateFrame(std::function<float()> valueFunction);

    private:
        /// <summary>
        /// Current frame.
        /// </summary>
        Windows::Media::AudioFrame^ m_currentFrame;

        /// <summary>
        /// Current frame capacity.
        /// </summary>
        uint32 m_frameCapacity;
    };
} }
