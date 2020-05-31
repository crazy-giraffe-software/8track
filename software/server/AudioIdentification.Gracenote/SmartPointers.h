//-----------------------------------------------------------------------
// <copyright file="SmartPonters.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once
#include "ErrorMacros.h"
#include <memory>

// Smartpointer for FILE.
struct FileDeleter
{
    void operator()(FILE* file_handle)
    {
        if (file_handle)
        {
            fclose(file_handle);
        }
    }
};

using file_unique_ptr = std::unique_ptr<FILE, FileDeleter>;

inline file_unique_ptr make_fopen(const char* file_name, const char* file_mode)
{
    FILE* file_handle = nullptr;
    errno_t _errno = fopen_s(&file_handle, file_name, file_mode);
    if (_errno != 0)
    {
        LogMessage("\nerror [on line %d]\n\t0x%08x\n", __LINE__, _errno);
        return nullptr;
    }

    return file_unique_ptr(file_handle);
}

// Smartpointer for gnsdk_user_handle_t.
inline void UserHandleDeleter (gnsdk_user_handle_t user_handle)
{
    if (user_handle != INVALID_HANDLE_VALUE)
    {
        // The creation of a user implies a call to gnsdk_manager_initialize as well.
        // gnsdk_manager_initialize/gnsdk_manager_shutdown handle ref counting so we
        // just need to call gnsdk_manager_shutdown to match our gnsdk_manager_initialize,
        // which is synonymous with having a valid user handle.
        GNSDK_LOG(gnsdk_manager_user_release(user_handle));
        GNSDK_LOG(gnsdk_manager_shutdown());
    }
}

using user_handle_shared_ptr = std::shared_ptr<gnsdk_user_handle_t_s>;

inline user_handle_shared_ptr make_user_handle_shared_ptr(gnsdk_user_handle_t user_handle)
{
    if (user_handle == GNSDK_NULL)
    {
        LogMessage("\nerror [on line %d]\n\tuser_handle is null\n", __LINE__);
        return nullptr;
    }

    return user_handle_shared_ptr(user_handle, UserHandleDeleter);
}

// Smartpointer for FILE.
struct ChannelHandleDeleter
{
    void operator()(gnsdk_musicidstream_channel_handle_t channel_handle)
    {
        if (channel_handle == GNSDK_NULL)
        {
            GNSDK_LOG(gnsdk_musicidstream_channel_release(channel_handle));
        }
    }
};

using channel_handle_unique_ptr = std::unique_ptr<gnsdk_musicidstream_channel_handle_t_s, ChannelHandleDeleter>;

inline channel_handle_unique_ptr make_channel_handle_unique_ptr(gnsdk_musicidstream_channel_handle_t channel_handle)
{
    if (channel_handle == GNSDK_NULL)
    {
        LogMessage("\nerror [on line %d]\n\tchannel_handle is null\n", __LINE__);
        return nullptr;
    }

    return channel_handle_unique_ptr(channel_handle);
}
