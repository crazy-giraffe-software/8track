//-----------------------------------------------------------------------
// <copyright file="GracenoteClientIdData.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification { namespace Gracenote
{
    /// <summary>
    /// Class for reading clientId.json. See https://developer.gracenote.com/web-api, Getting Started.
    /// </summary>
    public ref class GracenoteClientIdData sealed
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="GracenoteClientIdData" /> class.
        /// </summary>
        GracenoteClientIdData();

        /// <summary>
        /// The client id.
        /// </summary>
        property Platform::String^ ClientId
        {
            Platform::String^ get();
            void set(Platform::String^ value);
        }

        /// <summary>
        /// The client tag. See https://developer.gracenote.com/web-api, Getting Started.
        /// </summary>
        property Platform::String^ ClientTag
        {
            Platform::String^ get();
            void set(Platform::String^ value);
        }

        /// <summary>
        /// The app version. See https://developer.gracenote.com/web-api, Getting Started.
        /// </summary>
        property Platform::String^ AppVersion
        {
            Platform::String^ get();
            void set(Platform::String ^ value);
        }

        /// <summary>
        /// The client license.
        /// </summary>
        property Platform::String^ License
        {
            Platform::String^ get();
            void set(Platform::String^ value);
        }

    private:
        /// <summary>
        /// The client id.
        /// </summary>
        Platform::String^ m_clientId;

        /// <summary>
        /// The client tag. See https://developer.gracenote.com/web-api, Getting Started.
        /// </summary>
        Platform::String^ m_clientTag;

        /// <summary>
        /// The app version. See https://developer.gracenote.com/web-api, Getting Started.
        /// </summary>
        Platform::String^ m_appVersion;

        /// <summary>
        /// The client license.
        /// </summary>
        Platform::String^ m_license;
    };
} } }
