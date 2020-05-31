//-----------------------------------------------------------------------
// <copyright file="GracenoteSession.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "GracenoteSession.h"
#include "ErrorMacros.h"
#include "SmartPointers.h"

using namespace Concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::Gracenote;

// Track data.
typedef struct __track
{
    const char* p_album;
    const char* p_album_coverart_url;
    const char* p_album_releasedate;
    const char* p_album_label;
    const char* p_album_track_count;
    const char* p_title;
    const char* p_artist;
    const char* p_artist_image_url;
    const char* p_track_number;
    const char* p_track_duration;
    const char* p_track_duration_unit;
    const char* p_match_position;
    const char* p_match_duration;
    const char* p_current_position;
    const char* p_matchConfidence;
    const char* p_genre1;
    const char* p_genre2;
    const char* p_genre3;
    const char* p_mood1;
    const char* p_mood2;

    char* p_fingerprint;
} _track;

GracenoteSession::GracenoteSession()
    : m_options()
    , m_sessionId(Session::CreateSessionIdentifier())
    , m_status(IdentifyStatus::Invalid)
    , m_tracks(ref new Vector<IReadOnlyTrack^>())
    , m_user_handle(make_user_handle_shared_ptr(GNSDK_NULL))
    , m_channel_handle(make_channel_handle_unique_ptr(GNSDK_NULL))
    , m_weak_reference(new WeakReference(this))
{
    // m_weak_reference will leak but provides a good way to make sure
    // that any callbacks arriving after this object is destroyed
    // will be safely handled. The weak reference will fail to resolve
    // the destroyed class and skip the portions of the callback
    // that would cause an fault.
}

void GracenoteSession::Initialize(SessionOptions^ options, gnsdk_user_handle_t user_handle)
{
    // Cache the options.
    m_options = options;
    m_user_handle = make_user_handle_shared_ptr(user_handle);
}

String^ GracenoteSession::SessionIdentifier::get()
{
    return m_sessionId;
}

IdentifyStatus GracenoteSession::IdentificationStatus::get()
{
    return m_status;
}

void GracenoteSession::AddAudioSample(const Array<byte>^ audioData)
{
    // If not started, start.
    if (m_channel_handle.get() == GNSDK_NULL)
    {
        gnsdk_musicidstream_channel_handle_t channel_handle;
        if (0 == StreamBegin(
            m_user_handle.get(),
            m_options->SampleRate,
            m_options->SampleSize,
            m_options->ChannelCount,
            reinterpret_cast<gnsdk_void_t*>(m_weak_reference),
            &channel_handle))
        {
            m_channel_handle = make_channel_handle_unique_ptr(channel_handle);
            UpdateStatus(IdentifyStatus::Incomplete);
        }
    }

    // If not complete, add sample data.
    if (m_channel_handle.get() != GNSDK_NULL)
    {
        if (m_status == IdentifyStatus::Incomplete)
        {
            std::vector<unsigned char> audioVector(begin(audioData), end(audioData));
            StreamWrite(m_channel_handle.get(), audioVector.data(), audioVector.size());
        }

        if (m_status == IdentifyStatus::Complete)
        {
            // End the fingerprint session and get the track.
            StreamEnd(m_channel_handle.get());
        }
    }
}

IAsyncOperation<IVectorView<IReadOnlyTrack^>^>^ GracenoteSession::GetTracksAsync()
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this]() -> task<IVectorView<IReadOnlyTrack^>^>
        {
            return task_from_result(m_tracks->GetView());
        });
}

void GracenoteSession::UpdateStatus(IdentifyStatus newStatus)
{
    // Update.
    bool changed = (m_status != newStatus);
    m_status = newStatus;

    if (changed)
    {
        StatusChangedEventArgs^ eventArgs = ref new StatusChangedEventArgs(newStatus);
        StatusChanged(this, eventArgs);
    }
}

gnsdk_error_t GracenoteSession::StreamBegin(
    gnsdk_user_handle_t user_handle,
    unsigned int audio_sample_rate,
    unsigned int audio_sample_size,
    unsigned int audio_channels,
    gnsdk_void_t* _callback_data,
    gnsdk_musicidstream_channel_handle_t* p_channel_handle)
{
    gnsdk_musicidstream_channel_handle_t channel_handle = GNSDK_NULL;
    gnsdk_musicidstream_callbacks_t callbacks = { 0 };
    gnsdk_error_t error = GNSDK_SUCCESS;
    int rc = 0;

    // -Stream requires callbacks to receive identification results.
    // Here we set the various callbacks for results ands status.
    callbacks.callback_status = GNSDK_NULL;
    callbacks.callback_processing_status = GNSDK_NULL;
    callbacks.callback_identifying_status = StreamIdentifyingStatusCallback;
    callbacks.callback_result_available = StreamResultAvailableCallback;
    callbacks.callback_error = StreamCompletedWithErrorCallback;

    // Create the channel handle
    error = gnsdk_musicidstream_channel_create(
        user_handle,
        gnsdk_musicidstream_preset_radio,
        &callbacks,
        _callback_data,
        &channel_handle);
    GNSDK_CHECK(error);

    // initialize the fingerprinter
    // Note: The sample file shipped is a 44100Hz 16-bit stereo (2 channel) wav file
    error = gnsdk_musicidstream_channel_audio_begin(
        channel_handle,
        audio_sample_rate,
        audio_sample_size,
        audio_channels);
    GNSDK_CHECK(error);

    error = gnsdk_musicidstream_channel_identify(channel_handle);
    GNSDK_CHECK(error);

    //gnsdk_musicidstream_channel_set_locale

    // Set options.
    error = gnsdk_musicidstream_channel_option_set(
        channel_handle,
        GNSDK_MUSICIDSTREAM_OPTION_ENABLE_CONTENT_DATA,
        GNSDK_VALUE_TRUE);
    GNSDK_LOG(error);

    error = gnsdk_musicidstream_channel_option_set(
        channel_handle,
        GNSDK_MUSICIDSTREAM_OPTION_RESULT_PREFER_COVERART,
        GNSDK_VALUE_TRUE);
    GNSDK_LOG(error);

    error = GNSDK_SUCCESS;
error:
    if (error != GNSDK_SUCCESS)
    {
        GNSDK_LOG(gnsdk_musicidstream_channel_release(channel_handle));
    }
    else
    {
        // return the Channel handle for use at query time
        *p_channel_handle = channel_handle;
    }

    return error;
}

void GracenoteSession::StreamEnd(gnsdk_musicidstream_channel_handle_t channel_handle)
{
    if (channel_handle != GNSDK_NULL)
    {
        // signal that we are done
        GNSDK_LOG(gnsdk_musicidstream_channel_audio_end(channel_handle));
    }
}

gnsdk_error_t GracenoteSession::StreamWrite(
    gnsdk_musicidstream_channel_handle_t channel_handle,
    unsigned char* p_pcm_audio,
    size_t read_size)
{
    gnsdk_error_t error = GNSDK_SUCCESS;

    // write audio to the fingerprinter
    if (channel_handle != GNSDK_NULL)
    {
        error = gnsdk_musicidstream_channel_audio_write(
            channel_handle,
            p_pcm_audio,
            read_size);
        GNSDK_CHECK(error);

        LogMessage("\n%s: write %d bytes\n\n", __FUNCTION__, read_size);
    }

    error = GNSDK_SUCCESS;
error:
    return error;
}

gnsdk_error_t GracenoteSession::GetTrackData(
    gnsdk_gdo_handle_t response_gdo,
    gnsdk_uint32_t album_ordinal,
    gnsdk_void_t* callback_data,
    gnsdk_musicidstream_channel_handle_t channel_handle)
{
    gnsdk_error_t error = GNSDK_SUCCESS;
    gnsdk_gdo_handle_t album_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t album_title_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t album_coverart_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t album_coverart_asset_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t track_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t track_title_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t artist_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t artist_image_gdo = GNSDK_NULL;
    gnsdk_gdo_handle_t artist_image_asset_gdo = GNSDK_NULL;
    _track track_data = { 0 };

    // Get the album
    error = gnsdk_manager_gdo_child_get(response_gdo, GNSDK_GDO_CHILD_ALBUM, album_ordinal, &album_gdo);
    GNSDK_CHECK(error);

    // Get the matched track number: there is only 1. We need this, i.e. GNSDK_CHECK().
    error = gnsdk_manager_gdo_child_get(album_gdo, GNSDK_GDO_CHILD_TRACK_MATCHED, 1, &track_gdo);
    GNSDK_CHECK(error);

    // Get the artist: there is only 1. We need this, i.e. GNSDK_CHECK().
    error = gnsdk_manager_gdo_child_get(album_gdo, GNSDK_GDO_CHILD_ARTIST, 1, &artist_gdo);
    GNSDK_CHECK(error);

    // Get the album title: there is only 1; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_child_get(album_gdo, GNSDK_GDO_CHILD_TITLE_OFFICIAL, 1, &album_title_gdo);
    GNSDK_LOG(error);

    if (GNSDK_SUCCESS == error)
    {
        error = gnsdk_manager_gdo_value_get(album_title_gdo, GNSDK_GDO_VALUE_DISPLAY, 1, &track_data.p_album);
        GNSDK_LOG(error);
    }

    // Get the album covert art: there is only 1; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_child_get(album_gdo, GNSDK_GDO_CHILD_CONTENT_IMAGECOVER, 1, &album_coverart_gdo);
    GNSDK_LOG(error);

    if (GNSDK_SUCCESS == error)
    {
        error = gnsdk_manager_gdo_child_get(album_coverart_gdo, GNSDK_GDO_CHILD_ASSET_SIZE_MEDIUM, 1, &album_coverart_asset_gdo);
        GNSDK_LOG(error);

        if (GNSDK_SUCCESS == error)
        {
            error = gnsdk_manager_gdo_value_get(album_coverart_asset_gdo, GNSDK_GDO_VALUE_ASSET_URL_GNSDK, 1, &track_data.p_album_coverart_url);
            GNSDK_LOG(error);
        }
    }

    // Album release date; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_DATE_RELEASE, 1, &track_data.p_album_releasedate);
    GNSDK_LOG(error);

    // Album label; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_ALBUM_LABEL, 1, &track_data.p_album_label);
    GNSDK_LOG(error);

    // Album track count; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_ALBUM_TRACK_COUNT, 1, &track_data.p_album_track_count);
    GNSDK_LOG(error);

    // Track title; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_child_get(track_gdo, GNSDK_GDO_CHILD_TITLE_OFFICIAL, 1, &track_title_gdo);
    GNSDK_LOG(error);

    if (GNSDK_SUCCESS == error)
    {
        error = gnsdk_manager_gdo_value_get(track_title_gdo, GNSDK_GDO_VALUE_DISPLAY, 1, &track_data.p_title);
        GNSDK_LOG(error);
    }

    // Track number on album; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_TRACK_NUMBER, 1, &track_data.p_track_number);
    GNSDK_LOG(error);

    // Track duration; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_DURATION, 1, &track_data.p_track_duration);
    GNSDK_LOG(error);

    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_DURATION_UNITS, 1, &track_data.p_track_duration_unit);
    GNSDK_LOG(error);

    // Position in track where the fingerprint matched; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_MATCH_POSITION_MS, 1, &track_data.p_match_position);
    GNSDK_LOG(error);

    // Duration of the matched fingerprint.
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_MATCH_DURATION_MS, 1, &track_data.p_match_duration);
    GNSDK_LOG(error);

    // Position in track currently (adjusted for query time); optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_CURRENT_POSITION_MS, 1, &track_data.p_current_position);
    GNSDK_LOG(error);

    // Match confidence; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_TEXT_MATCH_SCORE, 1, &track_data.p_matchConfidence);
    GNSDK_LOG(error);

    // Generes; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_GENRE_LEVEL1, 1, &track_data.p_genre1);
    GNSDK_LOG(error);

    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_GENRE_LEVEL2, 1, &track_data.p_genre2);
    GNSDK_LOG(error);

    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_GENRE_LEVEL3, 1, &track_data.p_genre3);
    GNSDK_LOG(error);

    // Moods; optional, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_MOOD_LEVEL1, 1, &track_data.p_mood1);
    GNSDK_LOG(error);

    error = gnsdk_manager_gdo_value_get(track_gdo, GNSDK_GDO_VALUE_MOOD_LEVEL2, 1, &track_data.p_mood2);
    GNSDK_LOG(error);

    // Get artist, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_value_get(artist_gdo, GNSDK_GDO_VALUE_DISPLAY, 1, &track_data.p_artist);
    GNSDK_LOG(error);

    // Get the artist image: there is only 1, i.e. GNSDK_LOG().
    error = gnsdk_manager_gdo_child_get(artist_gdo, GNSDK_GDO_CHILD_CONTENT_IMAGEARTIST, 1, &artist_image_gdo);
    GNSDK_LOG(error);

    if (GNSDK_SUCCESS == error)
    {
        error = gnsdk_manager_gdo_child_get(artist_image_gdo, GNSDK_GDO_CHILD_ASSET_SIZE_MEDIUM, 1, &artist_image_asset_gdo);
        GNSDK_LOG(error);

        if (GNSDK_SUCCESS == error)
        {
            error = gnsdk_manager_gdo_value_get(artist_image_asset_gdo, GNSDK_GDO_VALUE_ASSET_URL_GNSDK, 1, &track_data.p_artist_image_url);
            GNSDK_LOG(error);
        }
    }

    error = GNSDK_SUCCESS;
error:
    GNSDK_CLEANUP_GDO(artist_image_asset_gdo);
    GNSDK_CLEANUP_GDO(artist_image_gdo);
    GNSDK_CLEANUP_GDO(artist_gdo);
    GNSDK_CLEANUP_GDO(track_title_gdo);
    GNSDK_CLEANUP_GDO(track_gdo);
    GNSDK_CLEANUP_GDO(album_coverart_asset_gdo);
    GNSDK_CLEANUP_GDO(album_coverart_gdo);
    GNSDK_CLEANUP_GDO(album_title_gdo);
    GNSDK_CLEANUP_GDO(album_gdo);

    return error;
}

/* static */
gnsdk_void_t GNSDK_CALLBACK_API GracenoteSession::StreamIdentifyingStatusCallback(
    gnsdk_void_t* callback_data,
    gnsdk_musicidstream_identifying_status_t status,
    gnsdk_bool_t* pb_abort)
{
    GNSDK_UNUSED(callback_data);

    gnsdk_cstr_t  tmp = GNSDK_NULL;

    switch (status)
    {
    case gnsdk_musicidstream_identifying_status_invalid:
        tmp = "status_invalid";
        break;

    case gnsdk_musicidstream_identifying_started:
        tmp = "started";
        break;

    case gnsdk_musicidstream_identifying_fp_generated:
        tmp = "fingerprint_generated";
        break;

    case gnsdk_musicidstream_identifying_local_query_started:
        tmp = "local_query_started";
        break;

    case gnsdk_musicidstream_identifying_local_query_ended:
        tmp = "local_query_ended";
        break;

    case gnsdk_musicidstream_identifying_online_query_started:
        tmp = "online_query_started";
        break;

    case gnsdk_musicidstream_identifying_online_query_ended:
        tmp = "online_query_ended";
        break;

    case gnsdk_musicidstream_identifying_ended:
        tmp = "ended";
        break;

    case gnsdk_musicidstream_identifying_no_new_result:
        tmp = "no_new_result";
        break;

    default:
        tmp = "unknown";
        break;
    }

    LogMessage("\n%s: status = %s\n\n", __FUNCTION__, tmp);

    // Do not cancel identification
    *pb_abort = GNSDK_FALSE;
}

/* static */
gnsdk_void_t GNSDK_CALLBACK_API GracenoteSession::StreamResultAvailableCallback(
    gnsdk_void_t* callback_data,
    gnsdk_musicidstream_channel_handle_t channel_handle,
    gnsdk_gdo_handle_t response_gdo,
    gnsdk_bool_t* pb_abort)
{
    gnsdk_uint32_t album_count = 0;
    gnsdk_uint32_t album_ordinal = 0;
    gnsdk_error_t error = GNSDK_SUCCESS;

    // Use the supplied weak reference to get the session.
    // The weak reference will fail to resolve a desttoyed class.
    Platform::WeakReference* reference = reinterpret_cast<Platform::WeakReference*>(callback_data);
    GracenoteSession^ session = reference->Resolve<GracenoteSession>();
    if (session != nullptr)
    {
        // how many albums were found.
        error = gnsdk_manager_gdo_child_count(response_gdo, GNSDK_GDO_CHILD_ALBUM, &album_count);
        GNSDK_CHECK(error);

        if (album_count == 0)
        {
            LogMessage("\nNo albums found for the input.\n");
        }
        else
        {
            LogMessage("\n%d albums found for the input.\n", album_count);

            for (album_ordinal = 1; album_ordinal <= album_count; album_ordinal++)
            {
                error = session->GetTrackData(response_gdo, album_ordinal, callback_data, channel_handle);
                GNSDK_CHECK(error);
            }
        }

        session->UpdateStatus(IdentifyStatus::Complete);
    }

error:
    // Do not cancel identification
    *pb_abort = GNSDK_FALSE;
}

/* static */
gnsdk_void_t GNSDK_CALLBACK_API GracenoteSession::StreamCompletedWithErrorCallback(
    gnsdk_void_t* callback_data,
    gnsdk_musicidstream_channel_handle_t channel_handle,
    const gnsdk_error_info_t* p_error_info)
{
    // Use the supplied weak reference to get the session.
    // The weak reference will fail to resolve a desttoyed class.
    Platform::WeakReference* reference = reinterpret_cast<Platform::WeakReference*>(callback_data);
    GracenoteSession^ session = reference->Resolve<GracenoteSession>();
    if (session != nullptr)
    {
        session->UpdateStatus(IdentifyStatus::Error);

        // an error occurred during identification
        LogMessage(
            "\nerror from: (%s:%s)  [error callback]\n\t0x%08x %s\n",
            p_error_info->error_api ? p_error_info->error_api : "API Unknown",
            p_error_info->error_module ? p_error_info->error_module : "Module Unknown",
            p_error_info->error_code,
            p_error_info->error_description);
    }
}
