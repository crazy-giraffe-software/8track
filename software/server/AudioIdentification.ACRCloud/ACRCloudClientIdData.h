//-----------------------------------------------------------------------
// <copyright file="ACRCloudClientIdData.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification { namespace ACRCloud
{
    /// <summary>
    /// Class for reading clientId.json. See https://developer.ACRCloud.com/web-api, Getting Started.
    /// </summary>
    public ref class ACRCloudClientIdData sealed
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="ACRCloudClientIdData" /> class.
        /// </summary>
        ACRCloudClientIdData();

        /// <summary>
        /// The host. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        property Platform::String^ Host
        {
            Platform::String^ get();
            void set(Platform::String^ value);
        }

        /// <summary>
        /// The access key. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        property Platform::String^ AccessKey
        {
            Platform::String^ get();
            void set(Platform::String^ value);
        }

        /// <summary>
        /// The access secret. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        property Platform::String^ AccessSecret
        {
            Platform::String^ get();
            void set(Platform::String ^ value);
        }

    private:
        /// <summary>
        /// The host. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        Platform::String^ m_host;

        /// <summary>
        /// The access key. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        Platform::String^ m_accessKey;

        /// <summary>
        /// The access secret. See https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/, Getting Started.
        /// </summary>
        Platform::String^ m_accessSecret;
    };
} } }
