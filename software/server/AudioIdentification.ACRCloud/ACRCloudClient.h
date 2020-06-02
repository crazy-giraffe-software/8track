//-----------------------------------------------------------------------
// <copyright file="ACRCloudClient.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once
#include "ACRCloudClientIdData.h"
#include "ACRCloudTrackResponse.h"

namespace CrazyGiraffe { namespace AudioIdentification { namespace ACRCloud
{
    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public ref class ACRCloudClient sealed
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="ACRCloudClient" /> class.
        /// </summary>
        /// <param name="clientdata">Client data for the factory.</param>
        ACRCloudClient(CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ clientdata);

        /// <summary>
        /// Create an instance of the <see cref="ACRCloudClient" /> class.
        /// </summary>
        /// <param name="clientdata">Client data for the factory.</param>
        ACRCloudClient(
            CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ clientdata,
            Windows::Web::Http::Filters::IHttpFilter^ httpFilter);

        ///
        /// Create the signature for the HTTP Request.
        ///
        Platform::String^ CreateSignature(Platform::String^ input, Platform::String^ key);

        ///
        /// Get the track info from ACRCloud.
        ///
        Windows::Foundation::IAsyncOperation<Windows::Web::Http::HttpRequestResult^>^
            QueryTrackInfoAsync(Windows::Storage::Streams::IBuffer^ fingerprintBuffer);

        ///
        /// Parse the track info response
        ///
        Windows::Foundation::IAsyncOperation<CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudTrackResponse^>^
            ParseTrackResponseAync(Platform::String^ responseBody);

    private:
        ///
        /// Get the Http client;
        ///
        Windows::Web::Http::HttpClient^ GetHttpClient();

    private:
        /// <summary>
        /// Client data for the session.
        /// </summary>
        CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ m_clientdata;

        ///
        /// An Http filter. Used for unit testing.
        ///
        Windows::Web::Http::Filters::IHttpFilter^ m_httpFilter;
    };
} } }
