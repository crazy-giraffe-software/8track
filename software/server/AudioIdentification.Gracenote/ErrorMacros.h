//-----------------------------------------------------------------------
// <copyright file="ErrorMacros.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

// Log a message.
inline void LogMessage(char* format, ...)
{
    static char s_acBuf[2048];

    va_list args;
    va_start(args, format);
    vsprintf_s(s_acBuf, format, args);
    OutputDebugStringA(s_acBuf);
    va_end(args);
}

inline void TraceLastErrnoError(int line_num)
{
    // Get the last error information from the SDK
    int _errno;
    _get_errno(&_errno);

    // Error_info will never be GNSDK_NULL.
    // The SDK will always return a pointer to a populated error info structure.
    LogMessage(
        "\nerror [on line %d]\n\t0x%08x\n",
        line_num,
        _errno);
}

// errno Error tracing.
#define ERRNO_CHECK(x) do { \
  int retval = (x); \
  if (retval != 0) { \
    TraceLastErrnoError(__LINE__); \
    goto error; \
  } \
} while (0)

// errno Error logging.
#define ERRNO_LOG(x) do { \
  int retval = (x); \
  if (retval != 0) { \
    TraceLastErrnoError(__LINE__); \
  } \
} while (0)

//Gracenote error tracing
inline void TraceLastGracenoteError(int line_num)
{
    // Get the last error information from the SDK
    const gnsdk_error_info_t* error_info = gnsdk_manager_error_info();

    // Error_info will never be GNSDK_NULL.
    // The SDK will always return a pointer to a populated error info structure.
    LogMessage(
        "\nerror from: %s()  [on line %d]\n\t0x%08x %s\n",
        error_info->error_api,
        line_num,
        error_info->error_code,
        error_info->error_description);
}

// Error tracing.
#define GNSDK_CHECK(x) do { \
  gnsdk_error_t retval = (x); \
  if (retval != GNSDK_SUCCESS) { \
    TraceLastGracenoteError(__LINE__); \
    goto error; \
  } \
} while (0)

// Error logging.
#define GNSDK_LOG(x) do { \
  gnsdk_error_t retval = (x); \
  if (retval != GNSDK_SUCCESS) { \
    TraceLastGracenoteError(__LINE__); \
  } \
} while (0)

// GDO cleanup.
#define GNSDK_CLEANUP_CONFIG_HANDLE(x) do { \
  gnsdk_config_handle_t cleanup_handle = (x); \
  if (cleanup_handle != GNSDK_NULL) { \
    GNSDK_LOG(gnsdk_config_release(cleanup_handle)); \
  } \
} while (0)

// GDO cleanup.
#define GNSDK_CLEANUP_GDO(x) do { \
  gnsdk_gdo_handle_t cleanup_gdo = (x); \
  if (cleanup_gdo != GNSDK_NULL) { \
    GNSDK_LOG(gnsdk_manager_gdo_release(cleanup_gdo)); \
  } \
} while (0)

// Rendered string cleanup.
#define GNSDK_CLEANUP_RENDERED_STR(x) do { \
  gnsdk_str_t cleanup_str = (x); \
  if (cleanup_str != GNSDK_NULL) { \
    GNSDK_LOG(gnsdk_manager_string_free(cleanup_str)); \
  } \
} while (0)
