//-----------------------------------------------------------------------
// <copyright file="Session.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

#include "SessionOptions.h"
#include "Track.h"
#include "StatusChangedEventArgs.h"
#include <cstddef>

namespace CrazyGiraffe { namespace AudioIdentification
{
    /// <summary>
    /// Status changed event delegate.
    /// </summary>
    public delegate void StatusChangedEventHandler(Platform::Object^ sender, CrazyGiraffe::AudioIdentification::StatusChangedEventArgs^ eventArgs);

    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public interface class ISession
    {
    public:
        /// <summary>
        /// Event handler for status changed.
        /// </summary>
        virtual event StatusChangedEventHandler^ StatusChanged;

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
        virtual property IdentifyStatus IdentificationStatus
        {
            IdentifyStatus get();
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
    };

    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public ref class Session sealed : ISession
    {
    public:
        /// <summary>
        /// Event handler for status changed.
        /// </summary>
        virtual event StatusChangedEventHandler^ StatusChanged;

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
        virtual property IdentifyStatus IdentificationStatus
        {
            IdentifyStatus get();
        }

        /// <summary>
        /// Add an audio sample for fingerprint
        /// </summary>
        /// <param name="audioData">audio data as byte array</param>
        virtual void AddAudioSample(const Platform::Array<unsigned char>^ audioData);

        /// <summary>
        /// Gets the identified track(s).
        /// </summary>
        virtual Windows::Foundation::IAsyncOperation<Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^>^
            GetTracksAsync();

        /// <summary>
        /// Gets the identifier for the session.
        /// </summary>
        static Platform::String^ CreateSessionIdentifier();

    internal:
        /// <summary>
        /// Create an instance of the <see cref="Session" /> class.
        /// </summary>
        Session(CrazyGiraffe::AudioIdentification::SessionOptions^ options);

    private:
        /// <summary>
        /// Create an instance of the <see cref="Session" /> class.
        /// </summary>
        Session();

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
        CrazyGiraffe::AudioIdentification::IdentifyStatus m_status;

        ///
        /// The identified tracks.
        ///
        Platform::Collections::Vector<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ m_tracks;
    };
} }
