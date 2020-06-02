//-----------------------------------------------------------------------
// <copyright file="GracenoteSession.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

#include "SmartPointers.h"

namespace CrazyGiraffe { namespace AudioIdentification { namespace Gracenote
{
    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public ref class GracenoteSession sealed : CrazyGiraffe::AudioIdentification::ISession
    {
    public:
        /// <summary>
        /// Event handler for status changed.
        /// </summary>
        virtual event CrazyGiraffe::AudioIdentification::StatusChangedEventHandler^ StatusChanged;

        /// <summary>
        /// Gets the identifier for the session.
        /// </summary>
        virtual property Platform::String^ SessionIdentifier
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the sample status.
        /// </summary>
        virtual property CrazyGiraffe::AudioIdentification::IdentifyStatus IdentificationStatus
        {
            CrazyGiraffe::AudioIdentification::IdentifyStatus get();
        }

        /// <summary>
        /// Add an audio sample for fingerprint
        /// </summary>
        /// <param name="audioData">audio data as byte array</param>
        virtual void AddAudioSample(const Platform::Array<byte>^ audioData);

        /// <summary>
        /// Gets the identified track(s).
        /// </summary>
        virtual Windows::Foundation::IAsyncOperation<Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^>^
            GetTracksAsync();

    internal:
        /// <summary>
        /// Prevents a default instance of the <see cref="GracenoteSession" /> class from being created.
        /// </summary>
        GracenoteSession();

        /// <summary>
        /// Initializes an instance of the <see cref="GracenoteSession" /> class.
        /// </summary>
        /// <param name="options">the options.</param>
        /// <param name="user_handle">the plugin user handle.</param>
        void Initialize(CrazyGiraffe::AudioIdentification::SessionOptions^ options, gnsdk_user_handle_t user_handle);

    protected:
        /// <summary>
        /// Update the status and send notifications.
        /// </summary>
        /// <param name="newStatus">the new status.</param>
        void UpdateStatus(CrazyGiraffe::AudioIdentification::IdentifyStatus newStatus);

    private:
        /// Begin the streaming identification.
        gnsdk_error_t StreamBegin(
            gnsdk_user_handle_t user_handle,
            unsigned int audio_sample_rate,
            unsigned int audio_sample_size,
            unsigned int audio_channels,
            gnsdk_void_t* _callback_data,
            gnsdk_musicidstream_channel_handle_t* p_channel_handle);

        /// End the streaming identification.
        void StreamEnd(gnsdk_musicidstream_channel_handle_t channel_handle);

        /// Write samples for streaming identification.
        gnsdk_error_t StreamWrite(
            gnsdk_musicidstream_channel_handle_t channel_handle,
            unsigned char* p_pcm_audio,
            size_t read_size);

        gnsdk_error_t GetTrackData(
            gnsdk_gdo_handle_t response_gdo,
            gnsdk_uint32_t album_ordinal,
            gnsdk_void_t* callback_data,
            gnsdk_musicidstream_channel_handle_t channel_handle);

    private:
        //
        // C-style callbacks (__decl) for GNSDK.
        //
        static gnsdk_void_t GNSDK_CALLBACK_API StreamIdentifyingStatusCallback(
            gnsdk_void_t* callback_data,
            gnsdk_musicidstream_identifying_status_t status,
            gnsdk_bool_t* pb_abort);

        static gnsdk_void_t GNSDK_CALLBACK_API StreamResultAvailableCallback(
            gnsdk_void_t* callback_data,
            gnsdk_musicidstream_channel_handle_t channel_handle,
            gnsdk_gdo_handle_t response_gdo,
            gnsdk_bool_t* pb_abort);

        static gnsdk_void_t GNSDK_CALLBACK_API StreamCompletedWithErrorCallback(
            gnsdk_void_t* callback_data,
            gnsdk_musicidstream_channel_handle_t channel_handle,
            const gnsdk_error_info_t* p_error_info);

    private:
        /// <summary>
        /// Options for the session.
        /// </summary>
        CrazyGiraffe::AudioIdentification::SessionOptions^ m_options;

        ///
        /// The session id.
        ///
        Platform::String^ m_sessionId;

        /// <summary>
        /// The status of the session.
        /// </summary>
        std::atomic<CrazyGiraffe::AudioIdentification::IdentifyStatus> m_status;

        ///
        /// The identified tracks.
        ///
        Platform::Collections::Vector<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ m_tracks;

        /// <summary>
        /// The shared pointer to the user handle.
        /// </summary>
        user_handle_shared_ptr m_user_handle;

        ///
        /// The identifying channel handle.
        ///
        channel_handle_unique_ptr m_channel_handle;

        ///
        /// A weak reference for C-style callbacks.
        ///
        Platform::WeakReference* m_weak_reference;
    };
} } }
