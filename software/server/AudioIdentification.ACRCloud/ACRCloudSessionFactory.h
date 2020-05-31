//-----------------------------------------------------------------------
// <copyright file="ACRCloudSessionFactory.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once
#include "ACRCloudClientIdData.h"

namespace CrazyGiraffe { namespace AudioIdentification { namespace ACRCloud
{
    /// <summary>
    ///  service.
    /// </summary>
    public ref class ACRCloudSessionFactory sealed : public  CrazyGiraffe::AudioIdentification::ISessionFactory
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="ACRCloudSessionFactory" /> class.
        /// </summary>
        /// <param name="clientdata">Client data for the factory.</param>
        ACRCloudSessionFactory(CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ clientdata);

        /// <summary>
        /// Create an instance of the <see cref="ACRCloudSessionFactory" /> class.
        /// </summary>
        /// <param name="clientdata">Client data for the factory.</param>
        ACRCloudSessionFactory(
            CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ clientdata,
            Windows::Web::Http::Filters::IHttpFilter^ httpFilter);

        /// <summary>
        /// Create a new session to identify a track.
        /// </summary>
        /// <param name="options">Options for the session.</param>
        /// <returns>A new session to identify a track.</returns>
        virtual Windows::Foundation::IAsyncOperation<CrazyGiraffe::AudioIdentification::ISession^>^
            CreateSessionAsync(CrazyGiraffe::AudioIdentification::SessionOptions^ options);

    private:
        /// <summary>
        /// Client data for the factory.
        /// </summary>
        CrazyGiraffe::AudioIdentification::ACRCloud::ACRCloudClientIdData^ m_clientdata;

        /// <summary>
        /// Flag to determine if we have initialized.
        /// </summary>
        bool m_initialized;

        ///
        /// An Http filter. Used for unit testing.
        ///
        Windows::Web::Http::Filters::IHttpFilter^ m_httpFilter;
    };
} } }
