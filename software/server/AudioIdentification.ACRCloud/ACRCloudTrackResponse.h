//-----------------------------------------------------------------------
// <copyright file="ACRCloudTrackResponse.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification { namespace ACRCloud
{
    /// <summary>
    ///  service.
    /// </summary>
    public ref class ACRCloudTrackResponse sealed
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="ACRCloudTrackResponse" /> class.
        /// </summary>
        ACRCloudTrackResponse(
            Platform::String^ message,
            Platform::String^ version,
            int16 code,
            Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ tracks);

        /// <summary>
        /// Gets the status message.
        /// </summary>
        property Platform::String^ Message
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the status version.
        /// </summary>
        property Platform::String^ Version
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        property int16 Code
        {
            int16 get();
        }

        /// <summary>
        /// Gets the tracks.
        /// </summary>
        property Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ Tracks
        {
            Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ get();
        }

    private:
        /// <summary>
        /// The status message.
        /// </summary>
        Platform::String^ m_message;

        /// <summary>
        /// The status version.
        /// </summary>
        Platform::String^ m_version;

        /// <summary>
        /// The status code.
        /// </summary>
        int16 m_code;

        ///
        /// The identified tracks.
        ///
        Windows::Foundation::Collections::IVectorView<CrazyGiraffe::AudioIdentification::IReadOnlyTrack^>^ m_tracks;
    };
} } }
