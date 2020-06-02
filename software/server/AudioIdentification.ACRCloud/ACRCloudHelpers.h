//-----------------------------------------------------------------------
// <copyright file="ACRCloudHelpers.h" company="CrazyGiraffeSoftware.net">
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

// Error tracing.
inline void TraceLastError(int line_num)
{
    // Get the last error information from the SDK
    //const gnsdk_error_info_t* error_info = gnsdk_manager_error_info();

    // Error_info will never be GNSDK_NULL.
    // The SDK will always return a pointer to a populated error info structure.
    //LogMessage(
    //    "\nerror from: %s()  [on line %d]\n\t0x%08x %s\n",
    //    error_info->error_api,
    //    line_num,
    //    error_info->error_code,
    //    error_info->error_description);
}

#define ACR_CHECK(x) do { \
  int retval = (x); \
  if (retval < 0) { \
    TraceLastError(__LINE__); \
     goto error; \
  } \
} while (0)

// Error logging.
#define ACR_LOG(x) do { \
  int retval = (x); \
  if (retval < 0) { \
    TraceLastError(__LINE__); \
  } \
} while (0)

// ACR cleanup.
#define ACR_CLEANUP_STRING(x) do { \
  char *cleanup_buffer = (x); \
  if (cleanup_buffer != NULL) { \
    acr_free(cleanup_buffer); \
  } \
} while (0)
//
//// GDO cleanup.
//#define GNSDK_CLEANUP_GDO(x) do { \
//  gnsdk_gdo_handle_t cleanup_gdo = (x); \
//  if (cleanup_gdo != GNSDK_NULL) { \
//    GNSDK_LOG(gnsdk_manager_gdo_release(cleanup_gdo)); \
//  } \
//} while (0)
//
//// Rendered string cleanup.
//#define GNSDK_CLEANUP_RENDERED_STR(x) do { \
//  gnsdk_str_t cleanup_str = (x); \
//  if (cleanup_str != GNSDK_NULL) { \
//    GNSDK_LOG(gnsdk_manager_string_free(cleanup_str)); \
//  } \
//} while (0)
