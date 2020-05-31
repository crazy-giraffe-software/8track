//-----------------------------------------------------------------------
// <copyright file="ACRCloudSession.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once
#include "ACRCloudClient.h"
#include "ACRCloudClientIdData.h"
#include <SharedQueue.h>
#include <vector>

namespace CrazyGiraffe { namespace AudioIdentification { namespace ACRCloud
{
    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public ref class ACRCloudSession sealed : CrazyGiraffe::AudioIdentification::ISession
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
        /// Prevents a default instance of the <see cref="ACRCloudSession" /> class from being created.
        /// </summary>
        ACRCloudSession();

        /// <summary>
        /// Initializes an instance of the <see cref="ACRCloudSession" /> class.
        /// </summary>
        /// <param name="clientdata">the client data.</param>
        /// <param name="options">the options.</param>
        void Initialize(
            CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ clientdata,
            Windows::Web::Http::Filters::IHttpFilter^ httpFilter,
            CrazyGiraffe::AudioIdentification::SessionOptions^ options);

    protected:
        /// <summary>
        /// Update the status and send notifications.
        /// </summary>
        /// <param name="newStatus">the new status.</param>
        void UpdateStatus(CrazyGiraffe::AudioIdentification::IdentifyStatus newStatus);

    private:
        ///
        /// Process the audio sample upto audioDataSize bytes.
        ///
        void ProcessAudioSamples(unsigned long audioDataSize);

        ///
        /// Get the fingerprint
        ///
        Windows::Storage::Streams::IBuffer^ GetFingerprint(std::vector<byte>& audioContent, size_t audioContentSize);

        ///
        /// Append the file header to the audio content.
        ///
        int PrependFileHeader(const std::vector<byte>& audioContent, size_t audioContentSize, std::vector<byte>& fileContent);

    private:
        /// <summary>
        /// Client for the session.
        /// </summary>
        CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClient^ m_client;

        /// <summary>
        /// Client data for the session.
        /// </summary>
        CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ m_clientdata;

        /// <summary>
        /// Options for the session.
        /// </summary>
        CrazyGiraffe::AudioIdentification::SessionOptions^ m_options;

        /// <summary>
        /// Number of bytes per second of audio.
        /// </summary>
        unsigned long m_bytesPerSecond;

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
        Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ m_tracks;

        ///
        /// The buffered audio data.
        ///
        SharedQueue<std::vector<byte>> m_audioQueue;

        ///
        /// The buffered audio data size.
        ///
        std::atomic<unsigned long> m_audioQueueSize;

        ///
        /// The audio/file content.
        ///
        std::vector<byte> m_audioData;

        ///
        /// The target size of the audio/file content.
        ///
        std::atomic<unsigned long> m_audioDataTargetSize;

        ///
        /// The number of recognition attempts.
        ///
        Concurrency::task<void> m_recognitionTask;

        ///
        /// The number of recognition attempts.
        ///
        std::atomic<int> m_recognitionAttempts;
    };
} } }
