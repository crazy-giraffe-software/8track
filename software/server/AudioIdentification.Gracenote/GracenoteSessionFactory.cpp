//-----------------------------------------------------------------------
// <copyright file="GracenoteSessionFactory.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "GracenoteSessionFactory.h"
#include "GracenoteSession.h"
#include "ErrorMacros.h"
#include "SmartPointers.h"
#include <stdlib.h>

// For local queries, we will open the database at this location and refer to it with this ID
#define GRACENOTELOOKUPDATABASE_ID "8track_db_id"
#define GRACENOTELOOKUPDATABASE_PATH "gndb"

using namespace concurrency;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::Gracenote;

GracenoteSessionFactory::GracenoteSessionFactory(GracenoteClientIdData^ clientdata)
    : m_user_handle(make_user_handle_shared_ptr(GNSDK_NULL))
{
    m_clientdata = clientdata;
}

IAsyncOperation<ISession^>^ GracenoteSessionFactory::CreateSessionAsync(SessionOptions^ options)
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this, &options]() -> task<ISession^>
        {
            return create_task([this]
                {
                    return ApplicationData::Current->LocalFolder->TryGetItemAsync(GRACENOTELOOKUPDATABASE_PATH);
                }, task_continuation_context::use_arbitrary())
            .then([this](IStorageItem^ gndbPath)
                {
                    if (gndbPath != nullptr)
                    {
                        return ApplicationData::Current->LocalFolder->GetFolderAsync(GRACENOTELOOKUPDATABASE_PATH);
                    }

                    return ApplicationData::Current->LocalFolder->CreateFolderAsync(GRACENOTELOOKUPDATABASE_PATH);
                }, task_continuation_context::use_arbitrary())
            .then([this, options](IStorageFolder^ gndbPath)
                {
                    if (gndbPath == nullptr)
                    {
                        cancel_current_task();
                    }

                    // Initialize plugin.
                    if (m_user_handle.get() == GNSDK_NULL)
                    {
                        if (0 != Initialize(gndbPath->Path->Data()))
                        {
                            cancel_current_task();
                        }
                    }

                    // Create an initialize a new session.
                    GracenoteSession^ session = ref new GracenoteSession();
                    session->Initialize(options, m_user_handle.get());

                    return task_from_result<ISession^>(session);
                }, task_continuation_context::use_arbitrary())
            .then([this](task<ISession^> previousTask)
                {
                    try
                    {
                        ISession^ session = previousTask.get();
                        return task_from_result<ISession^>(session);
                    }
                    catch (const task_canceled&)
                    {
                        LogMessage("\nFailed to create session, task cancelled\n");
                    }
                    catch (Exception^ ex)
                    {
                        LogMessage(
                            "\nFailed to create session, %s\n",
                            ex->Message->Data());
                    }
                    return task_from_result<ISession^>(nullptr);
                }, task_continuation_context::use_arbitrary());
        });
}

int GracenoteSessionFactory::Initialize(std::wstring storagePath)
{
    size_t size = 0;
    char client_id[MAX_PATH] = { 0 };
    char client_id_tag[MAX_PATH] = { 0 };
    char client_app_version[MAX_PATH] = { 0 };
    char license_data[4096] = { 0 };
    char local_folder[MAX_PATH] = { 0 };
    int use_local = 0;
    gnsdk_user_handle_t user_handle = GNSDK_NULL;
    int rc = 0;

    rc = wcstombs_s(&size, client_id, MAX_PATH, m_clientdata->ClientId->Data(), MAX_PATH);
    ERRNO_CHECK(rc);

    rc = wcstombs_s(&size, client_id_tag, MAX_PATH, m_clientdata->ClientTag->Data(), MAX_PATH);
    ERRNO_CHECK(rc);

    rc = wcstombs_s(&size, client_app_version, MAX_PATH, m_clientdata->AppVersion->Data(), MAX_PATH);
    ERRNO_CHECK(rc);

    rc = wcstombs_s(&size, license_data, 4096, m_clientdata->License->Data(), 4096);
    ERRNO_CHECK(rc);

    rc = wcstombs_s(&size, local_folder, MAX_PATH, storagePath.c_str(), MAX_PATH);
    ERRNO_CHECK(rc);

    rc = InitGnSdk(
        client_id,
        client_id_tag,
        client_app_version,
        license_data,
        strlen(license_data),
        use_local,
        local_folder,
        &user_handle);
    GNSDK_CHECK(rc);

error:
    if (rc == 0)
    {
        m_user_handle = make_user_handle_shared_ptr(user_handle);
    }

    return rc;
}

gnsdk_error_t GracenoteSessionFactory::InitGnSdk(
    const gnsdk_char_t* client_id,
    const gnsdk_char_t* client_tag,
    const gnsdk_char_t* app_version,
    const gnsdk_char_t* license_data,
    gnsdk_size_t license_data_len,
    int use_local,
    const gnsdk_char_t* local_folder,
    gnsdk_user_handle_t* p_user_handle)
{
    gnsdk_manager_handle_t sdkmgr_handle = GNSDK_NULL;
    gnsdk_user_handle_t user_handle = GNSDK_NULL;
    gnsdk_error_t error = GNSDK_SUCCESS;
    int rc = 0;

    // Display GNSDK Version infomation
    LogMessage(
        "\nGNSDK Product Version    : %s \t(built %s)\n",
        gnsdk_manager_get_product_version(),
        gnsdk_manager_get_build_date());

    // Initialize the GNSDK Manager
    error = gnsdk_manager_initialize(
        &sdkmgr_handle,
        license_data,
        license_data_len);
    GNSDK_CHECK(error);

    // Enable logging
    error = EnableLogging(local_folder);
    GNSDK_CHECK(error);

    // Initialize the Storage SQLite Library
    error = gnsdk_storage_sqlite_initialize(sdkmgr_handle);
    GNSDK_CHECK(error);

    if (use_local)
    {
        // Initialize the Lookup Local Library
        error = gnsdk_lookup_local_initialize(sdkmgr_handle);
        GNSDK_CHECK(error);

        // Initialize the Lookup LocalStream Library
        error = gnsdk_lookup_localstream_initialize(sdkmgr_handle);
        GNSDK_CHECK(error);

        error = gnsdk_lookup_localstream_storage_location_set(local_folder);
        GNSDK_CHECK(error);

        // Open the local database for querying.
        error = OpenLocalDb(local_folder);
        GNSDK_CHECK(error);
    }

    // Initialize the DSP Library - used for generating fingerprints
    error = gnsdk_dsp_initialize(sdkmgr_handle);
    GNSDK_CHECK(error);

    // Initialize the -Stream Library
    error = gnsdk_musicidstream_initialize(sdkmgr_handle);
    GNSDK_CHECK(error);

    // Get a user handle for our client ID.  This will be passed in for all queries
    error = GetUserHandle(
        client_id,
        client_tag,
        app_version,
        use_local,
        &user_handle);
    GNSDK_CHECK(error);

    // Set the user option to use our local Gracenote DB unless overridden.
    if (use_local)
    {
        error = gnsdk_manager_user_option_set(
            user_handle,
            GNSDK_USER_OPTION_LOOKUP_MODE,
            GNSDK_LOOKUP_MODE_LOCAL);
        GNSDK_CHECK(error);
    }

    // Set the 'locale' to return locale-specifc results values. This examples loads an English locale.
    error = SetLocale(user_handle, local_folder);
    GNSDK_LOG(error);

    error = GNSDK_SUCCESS;
error:
    if (error != GNSDK_SUCCESS)
    {
        // Clean up on failure.
        GNSDK_LOG(gnsdk_manager_user_release(user_handle));
        GNSDK_LOG(gnsdk_manager_shutdown());
    }
    else
    {
        // return the User handle for use at query time
        *p_user_handle = user_handle;
    }

    return error;
}

void GracenoteSessionFactory::ShutdownGnSdk(gnsdk_user_handle_t user_handle)
{
    // Shutdown the Manager to shutdown all libraries
    GNSDK_LOG(gnsdk_manager_shutdown());
}

gnsdk_error_t GracenoteSessionFactory::OpenLocalDb(const gnsdk_char_t* local_folder)
{
    gnsdk_error_t error = GNSDK_SUCCESS;
    gnsdk_config_handle_t config_handle = GNSDK_NULL;
    gnsdk_gdo_handle_t db_info_gdo = GNSDK_NULL;
    gnsdk_str_t db_info_xml = GNSDK_NULL;
    int rc = 0;

    error = gnsdk_config_create(&config_handle);
    GNSDK_CHECK(error);

    error = gnsdk_config_value_set(config_handle, GNSDK_CONFIG_LOOKUPDATABASE_ALL_LOCATION, local_folder);
    GNSDK_CHECK(error);

    //error = gnsdk_config_value_set(config_handle, GNSDK_CONFIG_LOOKUPDATABASE_ENABLE, GNSDK_VALUE_TRUE);
    //GNSDK_CHECK(error);

    //error = gnsdk_config_value_set(config_handle, GNSDK_CONFIG_LOOKUPDATABASE_ENABLE__IMAGES, GNSDK_VALUE_TRUE);
    //GNSDK_CHECK(error);

    // Open the database and assign it an ID for use by your application
    error = gnsdk_lookupdatabase_open(GRACENOTELOOKUPDATABASE_ID, config_handle);
    GNSDK_CHECK(error);

    // Display information about our local EDB
    error = gnsdk_lookupdatabase_info_get(GRACENOTELOOKUPDATABASE_ID, &db_info_gdo);
    GNSDK_CHECK(error);

    error = gnsdk_manager_gdo_render(db_info_gdo, GNSDK_GDO_RENDER_XML, &db_info_xml);
    GNSDK_CHECK(error);

    LogMessage("Gracenote DB Info:\n%s\n", db_info_xml);

error:
    GNSDK_CLEANUP_RENDERED_STR(db_info_xml);
    GNSDK_CLEANUP_GDO(db_info_gdo);
    GNSDK_CLEANUP_CONFIG_HANDLE(config_handle);

    return error;
}

gnsdk_error_t GracenoteSessionFactory::GetUserHandle(
    const gnsdk_char_t* client_id,
    const gnsdk_char_t* client_tag,
    const gnsdk_char_t* app_version,
    int use_local,
    gnsdk_user_handle_t* p_user_handle)
{
    // Load existing user handle, or register new one.
    // GNSDK requires a user handle instance to perform queries.
    // User handles encapsulate your Gracenote provided Client ID
    // which is unique for your application.User handles are
    // registered once with Gracenote then must be saved by
    // your applicationand reused on future invocations.

    // Creating a GnUser is required before performing any queries to Gracenote services,
    // and such APIs in the SDK require a GnUser to be provided. GnUsers can be created
    // 'Online' which means they are created by the Gracenote backend and fully vetted.
    // Or they can be create 'Local Only' which means they are created locally by the
    // SDK but then can only be used locally by the SDK.
    //
    // If the application cannot go online at time of user-regstration it should
    // create a 'local only' user. If connectivity is available, an Online user should
    // be created. An Online user can do both Local and Online queries.
    gnsdk_user_handle_t user_handle = GNSDK_NULL;
    gnsdk_cstr_t userRegistrationMode = GNSDK_USER_REGISTER_MODE_ONLINE;
    gnsdk_char_t serialized_user_buf[1024] = { 0 };
    file_unique_ptr readFile = NULL;
    file_unique_ptr writeFile = NULL;
    gnsdk_str_t serialized_user = GNSDK_NULL;
    gnsdk_error_t error = GNSDK_SUCCESS;

    // Do we have a user saved locally?
    readFile = make_fopen("user.txt", "r");
    if (readFile)
    {
        fgets(serialized_user_buf, sizeof(serialized_user_buf), readFile.get());

        // Create the user handle from the saved user
        error = gnsdk_manager_user_create(serialized_user_buf, client_id, &user_handle);
        GNSDK_LOG(error);

        if (GNSDK_SUCCESS == error)
        {
            gnsdk_bool_t localonly = GNSDK_FALSE;
            error = gnsdk_manager_user_is_localonly(user_handle, &localonly);
            GNSDK_LOG(error);

            if (GNSDK_SUCCESS == error)
            {
                if (!localonly || (strcmp(userRegistrationMode, GNSDK_USER_REGISTER_MODE_LOCALONLY) == 0))
                {
                    // Return handle.
                    *p_user_handle = user_handle;
                    return error;
                }
            }

            // else desired regmode is online, but user is localonly - discard and register new online user
            gnsdk_manager_user_release(user_handle);
            user_handle = GNSDK_NULL;
        }
    }

    // Register new user
    error = gnsdk_manager_user_register(
        userRegistrationMode,
        client_id,
        client_tag,
        app_version,
        &serialized_user);
    GNSDK_CHECK(error);

    // Create the user handle from the newly registered user
    error = gnsdk_manager_user_create(serialized_user, client_id, &user_handle);
    GNSDK_CHECK(error);

    // Save newly registered user for use next time
    writeFile = make_fopen("user.txt", "w");
    if (writeFile.get())
    {
        fputs(serialized_user, writeFile.get());
    }

error:
    if (serialized_user != GNSDK_NULL)
    {
        // Cleanup
        GNSDK_LOG(gnsdk_manager_string_free(serialized_user));
    }

    if (error != GNSDK_SUCCESS)
    {
        // Clean up on failure.
        GNSDK_LOG(gnsdk_manager_user_release(user_handle));
    }
    else
    {
        // return the User handle for use at query time
        *p_user_handle = user_handle;
    }

    return error;
}

gnsdk_error_t GracenoteSessionFactory::SetLocale(gnsdk_user_handle_t user_handle, const gnsdk_char_t* local_folder)
{
    /// Set application locale. Note that this is only necessary if you are using
    /// locale - dependant fields such as genre, mood, origin, era, etc.Your app
    /// may or may not be accessing locale_dependent fields, but it does not hurt
    /// to do this initialization as a matter of course.

    gnsdk_locale_handle_t locale_handle = GNSDK_NULL;
    gnsdk_error_t error = GNSDK_SUCCESS;

    // Set the location of Gracenote Lists DB
    error = gnsdk_manager_storage_location_set(GNSDK_MANAGER_STORAGE_LISTS, local_folder);
    GNSDK_CHECK(error);

    error = gnsdk_manager_locale_load(
        GNSDK_LOCALE_GROUP_MUSIC,
        GNSDK_LANG_ENGLISH,
        GNSDK_REGION_GLOBAL,
        GNSDK_DESCRIPTOR_DEFAULT,
        user_handle,
        GNSDK_NULL,
        GNSDK_NULL,
        &locale_handle);
    GNSDK_CHECK(error);

    // Setting the 'locale' as default
    // If default not set, no locale-specific results would be available
    error = gnsdk_manager_locale_set_group_default(locale_handle);
    GNSDK_CHECK(error);

error:
    if (locale_handle != GNSDK_NULL)
    {
        // The manager will hold onto the locale when set as default
        // so it's ok to release our reference to it here
        GNSDK_LOG(gnsdk_manager_locale_release(locale_handle));
    }

    return error;
}

gnsdk_error_t GracenoteSessionFactory::EnableLogging(const gnsdk_char_t* local_folder)
{
    gnsdk_error_t error = GNSDK_SUCCESS;
    gnsdk_char_t logPath[MAX_PATH] = { 0 };

    strcat_s(logPath, MAX_PATH, local_folder);
    strcat_s(logPath, MAX_PATH, "\\gracenote.log");

    error = gnsdk_manager_logging_enable(
        logPath,
        GNSDK_LOG_PKG_ALL,
        //GNSDK_LOG_LEVEL_ERROR | GNSDK_LOG_LEVEL_WARNING,
        GNSDK_LOG_LEVEL_ALL,
        GNSDK_LOG_OPTION_ALL,
        0,
        GNSDK_FALSE);
    const gnsdk_error_info_t* error_info = gnsdk_manager_error_info();
    GNSDK_CHECK(error);

error:
    return error;
}
