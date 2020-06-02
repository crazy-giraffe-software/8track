//-----------------------------------------------------------------------
// <copyright file="GracenoteSessionFactory.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once
#include "GracenoteClientIdData.h"
#include "SmartPointers.h"

namespace CrazyGiraffe { namespace AudioIdentification { namespace Gracenote
{
    /// <summary>
    ///  service.
    /// </summary>
    public ref class GracenoteSessionFactory sealed : public  CrazyGiraffe::AudioIdentification::ISessionFactory
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="GracenoteSessionFactory" /> class.
        /// </summary>
        /// <param name="clientdata">Client data for the factory.</param>
        GracenoteSessionFactory(GracenoteClientIdData^ clientdata);

        /// <summary>
        /// Create a new session to identify a track.
        /// </summary>
        /// <param name="options">Options for the session.</param>
        /// <returns>A new session to identify a track.</returns>
        virtual Windows::Foundation::IAsyncOperation<CrazyGiraffe::AudioIdentification::ISession^>^
            CreateSessionAsync(CrazyGiraffe::AudioIdentification::SessionOptions^ options);

    private:
        /// <summary>
        /// Initialize the plugin(s).
        /// </summary>
        int Initialize(std::wstring storagePath);

        /// Initializing the GNSDK is required before any other APIs can be called.
        /// First step is to always initialize the Manager module, then use the returned
        /// handle to initialize any modules to be used by the application.
        gnsdk_error_t InitGnSdk(
            const gnsdk_char_t* client_id,
            const gnsdk_char_t* client_tag,
            const gnsdk_char_t* app_version,
            const gnsdk_char_t* license_data,
            gnsdk_size_t license_data_len,
            int use_local,
            const gnsdk_char_t* local_folder,
            gnsdk_user_handle_t* p_user_handle);

        /// When your program is terminating, or you no longer need GNSDK, you should
        /// call gnsdk_manager_shutdown(). No other shutdown operations are required.
        /// gnsdk_manager_shutdown() also shuts down all other modules, regardless
        /// of the number of times they have been initialized.
        /// You can shut down individual modules while your program is running with
        /// their dedicated shutdown functions in order to free up resources.
        void ShutdownGnSdk(gnsdk_user_handle_t user_handle);

        ///
        /// Open the local DB for cache lookups
        ///
        gnsdk_error_t OpenLocalDb(const gnsdk_char_t* local_folder);

        ///
        /// Get the user handle.
        ///
        gnsdk_error_t GetUserHandle(
            const gnsdk_char_t* client_id,
            const gnsdk_char_t* client_tag,
            const gnsdk_char_t* app_version,
            int use_local,
            gnsdk_user_handle_t* p_user_handle);

        ///
        /// Set the locale for the user
        ///
        gnsdk_error_t SetLocale(gnsdk_user_handle_t user_handle, const gnsdk_char_t* local_folder);

        ///
        /// Enable logging fro the Gacenote SDK
        ///
        gnsdk_error_t EnableLogging(const gnsdk_char_t* local_folder);

    private:
        /// <summary>
        /// Client data for the factory.
        /// </summary>
        GracenoteClientIdData^ m_clientdata;

        /// <summary>
        /// The shared pointer to the user handle.
        /// </summary>
        user_handle_shared_ptr m_user_handle;
    };
} } }
